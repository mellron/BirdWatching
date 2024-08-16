using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace W2PrintProcessor
{
    class W2Print
    {
        private static readonly Regex namePattern = new Regex("[^a-zA-Z0-9]");
        private int batchSize;
        private int batchNum;
        private int pdfCount;
        private int errorCount;
        private string masterPath;
        private string outputDir;
        private string batchDir;
        private string errBatchDir;
        private string manifestFileName;
        private StreamWriter manifestWriter;
        private string directorySep;
        private string manifestDelimiter;
        private string manifestW2Constant;

        // Properties with validation in the setters
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

        // Constructor
        public W2Print(int batchSize, string masterPath, string outputDir)
        {
            BatchSize = batchSize;
            MasterPath = masterPath;
            OutputDir = outputDir;

            string timestamp = DateTime.Now.ToString("MMddyyyy_HHmmss");
            OutputDir = Path.Combine(outputDir, timestamp);
        }

        public void ProcessPDF()
        {
            string[] files = Directory.GetFiles(this.MasterPath.Trim(), "*.pdf");

            foreach (string file in files)
            {
                Console.WriteLine($"Processing {file}");

                try
                {
                    using (UglyToad.PdfPig.PdfDocument pdfDocument = UglyToad.PdfPig.PdfDocument.Open(file))
                    {
                        SetupErrorBatch();

                        for (int i = 0; i < pdfDocument.NumberOfPages; i++)
                        {
                            if (PdfCount % BatchSize == 0)
                            {
                                SetupBatch();
                            }

                            Page page = pdfDocument.GetPage(i + 1);  // Pages are 1-indexed in PdfPig

                            try
                            {
                                string addressesInfo = AddressExtract(page);
                                Dictionary<string, string> addressElements = GetAddressElements(addressesInfo);

                                string formattedCount = $"{++PdfCount:D5}";
                                string splitPdfName = $"{addressElements["FileName"]}_{formattedCount}.pdf";

                                // Only logging and extracting text, no saving/modifying PDF

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
                                Console.WriteLine($"\tERROR - Processing PDF: {e.ToString()}");
                            }
                        }

                        Console.WriteLine("\tProcessing Successful.");
                        Console.WriteLine("------------------------------------------");
                        Console.WriteLine($"\tPDFs Processed in this file - {pdfDocument.NumberOfPages}");
                        Console.WriteLine($"\tError Records in this file  - {ErrorCount}");
                        Console.WriteLine("------------------------------------------\n");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\tERROR - An unexpected error occurred while processing {file}: {ex.Message}");
                    // Handle other exceptions as needed.
                }
            }

            if (this.ManifestWriter != null)
            {
                this.ManifestWriter.Close();
            }

            Console.WriteLine("****************************************************");
            Console.WriteLine($"\tTotal PDF Records Processed - {PdfCount}");
            Console.WriteLine($"\tTotal Error Records - {ErrorCount}");
            Console.WriteLine("****************************************************");
        }

        
        public string AddressExtract(Page page)
        {
            // Define the rectangle area from which to extract text
            PdfRectangle rect = new PdfRectangle(14, page.Height - 162, 14 + 280, page.Height - (162 + 54));

            // Extract text from the specified region
            string addressLines = string.Empty;
            foreach (var word in page.GetWords())
            {
                if (rect.Contains(word.BoundingBox))
                {
                    addressLines += word.Text + " ";
                }
            }

            return addressLines.Trim();
        }

        public void SetupBatch()
        {
            BatchDir = Path.Combine(OutputDir, $"W2sPDF{BatchNum:D2}");
            Directory.CreateDirectory(BatchDir);
            ManifestWriter?.Close();

            ManifestFileName = Path.Combine(BatchDir, $"W2sPDF{BatchNum:D2}_Manifest.txt");
            ManifestWriter = new StreamWriter(ManifestFileName);
        }

        public void SetupErrorBatch()
        {
            ErrBatchDir = Path.Combine(OutputDir, "W2sPDFError");
            Directory.CreateDirectory(ErrBatchDir);
        }

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
    }
}
