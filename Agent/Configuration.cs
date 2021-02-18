using System.IO;
using System.Text.Json;

namespace Agent 
{
    public class Configuration
    {
        public string BaseURL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IgnoreCerts { get; set; }
        public int SyncInterval { get; set; }
        public string DatabaseFile { get; set; }
        public string LogFile { get; set; }

        public static Configuration LoadFromFile(string path)
        {
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<Configuration>(File.ReadAllText(path), jsonOptions);
        }
    }
}