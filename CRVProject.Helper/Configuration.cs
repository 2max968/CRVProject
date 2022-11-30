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
        public LocatorConfiguration Locator = new LocatorConfiguration();
        public RecognitionConfiguration Recognition = new RecognitionConfiguration();

        public static void LoadConfiguration(string path = "Config.json")
        {
            if(!File.Exists(path))
            {
                Console.WriteLine($"Warning: File '{path}' does not exist");
                Instance = new Configuration();
            }
            string json = File.ReadAllText(path);
            var config = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(json);
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
        public double HueValue = 90;
        public double HueTolerance = 10;
        public double AreaThreshhold = 0.0002f;
        public double DilationErotionSize = 0.01f;
        public int OutputWidth = 900;
        public int OutputHeight = 600;
        public double Brightness = 0.3;
    }

    public class RecognitionConfiguration
    {

    }
}
