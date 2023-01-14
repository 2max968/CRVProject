using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRVProject.Helper
{
    public class Configuration
    {
        public static Configuration Instance = new Configuration();
        public LocatorConfiguration Locator { get; set; } = new LocatorConfiguration();
        public RecognitionConfiguration Recognition { get; set; } = new RecognitionConfiguration();

        public static void LoadConfiguration(string path = "Config.json")
        {
            if(!File.Exists(path))
            {
                Console.WriteLine($"Warning: File '{path}' does not exist");
                Instance = new Configuration();
                var json_text = JsonConvert.SerializeObject(Instance, Formatting.Indented);
                File.WriteAllText(path, json_text);
                return;
            }
            string json = File.ReadAllText(path);
            var config = JsonConvert.DeserializeObject<Configuration>(json);
            if (config == null)
            {
                Console.WriteLine($"Warning: Cant deserialize file '{path}'");
                Instance = new Configuration();
            }
            Instance = config ?? new Configuration();
        }
    }

    public class LocatorConfiguration
    {
        public double HueValue { get; set; } = 20;
        public double HueTolerance { get; set; } = 10;
        public double AreaThreshhold { get; set; } = 0.0002f;
        public double DilationErotionSize { get; set; } = 0.01f;
        public int OutputWidth { get; set; } = 900;
        public int OutputHeight { get; set; } = 600;
        public double Brightness { get; set; } = 0.3;
    }

    public class RecognitionConfiguration
    {

    }
}
