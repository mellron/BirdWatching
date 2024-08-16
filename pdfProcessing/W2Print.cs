using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace W2PrintProcessor
{
    class W2Print
    {
        private static readonly Regex namePattern = new Regex("[^a-zA-Z0-9]");

        private int batchSize;
        private string masterPath;
        private string outputDir;

        public int BatchSize
        {
            get { return batchSize; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Batch size must be greater than 0.");
                batchSize = value;
            }
        }

        public string MasterPath
        {
            get { return masterPath; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Master path cannot be null or empty.");
                masterPath = value;
            }
        }

        public string OutputDir
        {
            get { return outputDir; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Output directory cannot be null or empty.");
                outputDir = value;
            }
        }

        public int BatchNum { get; private set; } = 0;
        public int PdfCount { get; private set; } = 0;
        public int ErrorCount { get; private set; } = 0;
        public string BatchDir { get; set; } = string.Empty;
        public string ErrBatchDir { get; set; } = string.Empty;
        public string ManifestFileName { get; set; } = string.Empty;
        public StreamWriter ManifestWriter { get; set; }
        public string DirectorySeparator { get; } = Path.DirectorySeparatorChar.ToString();
        public string ManifestDelimiter { get; set; } = "|";
        public string ManifestW2Constant { get; set; } = "W2";



        //***
        // Constructor to initialize the object
        // Parameters:
        // batchSize: The number of PDFs to be processed in a batch
        // masterPath: The path to the directory containing the PDF files
        // outputDir: The path to the output directory
        // ***

        public W2Print(int batchSize, string masterPath, string outputDir)
        {
            BatchSize = batchSize; // Validation will be done in the setter
            MasterPath = masterPath; // Validation will be done in the setter
            OutputDir = outputDir; // Validation will be done in the setter

            string timestamp = DateTime.Now.ToString("MMddyyyy_HHmmss");
            OutputDir = Path.Combine(outputDir, timestamp);
        }

        /// <summary>
        /// { AddressExtract }
        /// </summary>
        ///
        /// <param name="filePath">The file path</param>
        ///
        /// <returns>< description_of_the_return_value ></returns>
        public string AddressExtract(string filePath)
        {
            using (UglyToad.PdfPig.PdfDocument pdfDocument = UglyToad.PdfPig.PdfDocument.Open(filePath))
            {
                Page page = pdfDocument.GetPage(1);
                string text = page.Text;
                return text.Substring(0, text.IndexOf("\n")).Trim();
            }
        }

        /// <summary>
        /// { SetupBatch }
        /// </summary>
        /// <returns>< description_of_the_return_value ></returns>
        /// <exception cref="Exception">Thrown when an error occurs</exception>
        public void SetupBatch()
        {
            BatchDir = Path.Combine(OutputDir, $"W2sPDF{BatchNum:D2}");
            Directory.CreateDirectory(BatchDir);
            ManifestWriter?.Close();

            ManifestFileName = Path.Combine(BatchDir, $"W2sPDF{BatchNum:D2}_Manifest.txt");
            ManifestWriter = new StreamWriter(ManifestFileName);
        }

        /// <summary>
        /// { SetupErrorBatch }
        /// </summary>
        /// <returns>< description_of_the_return_value ></returns>
        /// <exception cref="Exception">Thrown when an error occurs</exception>
        public void SetupErrorBatch()
        {
            ErrBatchDir = Path.Combine(OutputDir, "W2sPDFError");
            Directory.CreateDirectory(ErrBatchDir);
        }

        /// <summary>
        /// { GetAddressElements }
        /// </summary>
        /// <param name="address">The address</param>
        /// <returns>< description_of_the_return_value ></returns>
        /// <exception cref="Exception">Thrown when an error occurs</exception>
        public Dictionary<string, string> GetAddressElements(string address)
        {
            Dictionary<string, string> addressElements = new Dictionary<string, string>();
            string[] lines = address.Split('\n');

            if (lines.Length < 3)
            {
                throw new Exception("Address Extract Error. Total Address lines less than expected");
            }

            string empFileName = namePattern.Replace(lines[0], "");
            addressElements["Name"] = lines[0];
            addressElements["FileName"] = empFileName;
            addressElements["Address1"] = lines[1];

            string[] cityStateZip = lines[^1].Split(',');
            addressElements["City"] = cityStateZip[0];
            string[] stateZip = cityStateZip[1].Trim().Split(' ');
            addressElements["State"] = stateZip[0];
            addressElements["Zip"] = stateZip[1];

            return addressElements;
        }

        /// <summary>
        /// { ProcessPDF }
        /// </summary>
        /// <exception cref="Exception">Thrown when an error occurs</exception>

        public void ProcessPDF()
        {
            string[] files = Directory.GetFiles(this.MasterPath, "*.pdf");

            foreach (string file in files)
            {
                Console.WriteLine($"Processing {file}");

                using (PdfSharpCore.Pdf.PdfDocument document = PdfReader.Open(file, PdfDocumentOpenMode.Import))
                {
                    SetupErrorBatch();

                    for (int i = 0; i < document.PageCount; i++)
                    {
                        if (PdfCount % BatchSize == 0)
                        {
                            SetupBatch();
                        }

                        PdfSharpCore.Pdf.PdfPage page = document.Pages[i];

                        try
                        {
                            string addressesInfo = AddressExtract(file);
                            Dictionary<string, string> addressElements = GetAddressElements(addressesInfo);

                            string formattedCount = $"{++PdfCount:D5}";
                            string splitPdfName = $"{addressElements["FileName"]}_{formattedCount}.pdf";

                            using (PdfSharpCore.Pdf.PdfDocument newDocument = new PdfSharpCore.Pdf.PdfDocument())
                            {
                                newDocument.AddPage(page);

                                string outputPath = Path.Combine(this.BatchDir, splitPdfName);
                                newDocument.Save(outputPath);
                            }

                            string manifestFileStr = $"{splitPdfName}{this.ManifestDelimiter}{addressElements["Name"]}" +
                                                     $"{this.ManifestDelimiter}{addressElements["Address1"]}" +
                                                     $"{this.ManifestDelimiter}{addressElements["City"]}" +
                                                     $"{this.ManifestDelimiter}{addressElements["State"]}" +
                                                     $"{this.ManifestDelimiter}{addressElements["Zip"]}" +
                                                     $"{this.ManifestDelimiter}{this.ManifestW2Constant}";

                            this.ManifestWriter.WriteLine(manifestFileStr);
                            Console.WriteLine($"\t{splitPdfName}");
                        }
                        catch (Exception e)
                        {
                            ErrorCount++;
                            Console.WriteLine("\tERROR - Processing PDF. Moving the following PDF to Error Batch: " + e.ToString());

                            string errorPdfName = $"ErrorPDF_{ErrorCount:D5}.pdf";
                            string errorPath = Path.Combine(this.ErrBatchDir, errorPdfName);

                            using (PdfSharpCore.Pdf.PdfDocument newDocument = new PdfSharpCore.Pdf.PdfDocument())
                            {
                                newDocument.AddPage(page);
                                newDocument.Save(errorPath);
                            }
                        }
                    }

                    Console.WriteLine("\tSplit Successful.");
                    Console.WriteLine("------------------------------------------");
                    Console.WriteLine($"\tPDFs Processed in this file - {document.PageCount}");
                    Console.WriteLine($"\tError Records in this file  - {ErrorCount}");
                    Console.WriteLine("------------------------------------------\n");
                }
            }

            if (this.ManifestWriter != null)
            {
                this.ManifestWriter.Close();
            }

            Console.WriteLine("****************************************************");
            Console.WriteLine($"\tTotal PDF Records Split - {PdfCount}");
            Console.WriteLine($"\tTotal Error Records - {ErrorCount}");
            Console.WriteLine("****************************************************");
        }
    }
}
