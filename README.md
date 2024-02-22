using Newtonsoft.Json.Linq;

public class JsonFixer
{
    public static void FixJsonFileEncoding(string inputFilePath, string outputFilePath)
    {
        try
        {
            // Read the original JSON content from the file
            string originalJson = File.ReadAllText(inputFilePath);

            // Parse the JSON content to a JObject
            var parsedJson = JObject.Parse(originalJson);

            // Convert the JObject back to a string, which automatically handles encoding issues
            string fixedJson = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);

            // Save the fixed JSON to a new file
            File.WriteAllText(outputFilePath, fixedJson);

            Console.WriteLine("JSON file has been fixed and saved.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
