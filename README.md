using System.Data;
using Newtonsoft.Json;

public class DataSetConverter
{
    public static string ConvertFirstTableToJson(DataSet oDS)
    {
        if (oDS != null && oDS.Tables.Count > 0)
        {
            // Convert the first DataTable to JSON
            string json = JsonConvert.SerializeObject(oDS.Tables[0], Formatting.Indented, new JsonSerializerSettings {
                // Settings if needed, for example, to handle loop references or to include type names
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        return null; // or "{}" if you prefer to return an empty JSON object when the dataset is null or empty
    }
}
