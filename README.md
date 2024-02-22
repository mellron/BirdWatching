using System;
using System.Text.Json;

public class JsonHelper
{
    public static string StringToJson(string key, string value)
    {
        // Creating an anonymous object with the provided key and value
        var obj = new { Key = key, Value = value };

        // Serializing the object to a JSON string
        string jsonString = JsonSerializer.Serialize(obj);

        return jsonString;
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Example usage
        string key = "name";
        string value = "Jane \"Doe\"";
        string json = JsonHelper.StringToJson(key, value);
        
        Console.WriteLine(json);
    }
}
