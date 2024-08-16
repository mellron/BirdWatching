using System;
using System.IO;

namespace W2PrintProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!CheckArguments(args))
            {
                Environment.Exit(1);
            }

            Console.WriteLine("Starting....");

            DateTime startTime = DateTime.Now;

            // Explicitly declare the type of the processor
            W2Print processor = new W2Print(int.Parse(args[0]), args[1], args[2]);

            // Call the method to process the PDF files
            processor.ProcessPDF();

            DateTime endTime = DateTime.Now;

            Console.WriteLine($"\tProcess completed successfully. It took {(endTime - startTime).TotalSeconds} seconds");
        }

        // Function to validate the command-line arguments
        static bool CheckArguments(string[] args)
        {
            // Check if the length of the array is 3
            if (args.Length != 3)
            {
                Console.WriteLine("************ Print Processing W2 version 1.0.00 **************************");
                Console.WriteLine(); // Write a blank line
                Console.WriteLine("Usage: W2PrintProcessor <batchSize> <inputPDFDir> <OutputFolder>");
                Console.WriteLine();
                Console.WriteLine("**************************************************************************");

                return false;
            }

            // Validate if the input PDF directory exists
            if (!Directory.Exists(args[1]))
            {
                Console.WriteLine($"Input PDF Directory {args[1]} does not exist");
                return false;
            }

            // Validate if the output folder exists
            if (!Directory.Exists(args[2]))
            {
                Console.WriteLine($"Output Folder {args[2]} does not exist");
                return false;
            }

            // Validate the batch size is greater than 0 and less than or equal to 200,000
            int batchSize;
            if (!int.TryParse(args[0], out batchSize) || batchSize <= 0 || batchSize > 200000)
            {
                Console.WriteLine("Batch size must be a valid integer greater than 0 and less than or equal to 200,000");
                return false;
            }

            return true;
        }
    }
}
