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
        public double HueTolerance { get; set; } = 5;
        public double AreaThreshhold { get; set; } = 0.002f;
        public double DilationErotionSize { get; set; } = 0.01f;
        public int OutputWidth { get; set; } = 900;
        public int OutputHeight { get; set; } = 600;
        public double Brightness { get; set; } = 0.3;
    }

    public class RecognitionConfiguration
    {
        public string Letters { get; set; } = "abcdefghijklmnopqrstuvwxyz!1234567890^";
        public double rho { get; set; } = 1;
        public double theta { get; set; } = Math.PI / 180;
        public int threshold { get; set; } = 10;
        public double minLength { get; set; } = 200;
        public double maxLengthGap { get; set; } = 10;
        public int NumBoxes { get; set; } = 0;
        public int FoundParent { get; set; } = -1;
        public double TempMin { get; set; } = 0;
        public double Diff { get; set; } = 100;
        public string CorrectWord { get; set; } = "";
        public char CorrectLetter { get; set; } = '\0';
        public double Confidence { get; set; } = 0;
        public int CounterK { get; set; } = 0;
        public double epsilon { get; set; } = 0.000001;
        public double medianHeight { get; set; } = 0;
        public double MedianMul { get; set; } = 1.1;
        public int maskVar { get; set; } = 0;
        public double iRatioLBoundLCase = 0.18;
        public double iRatioUBoundLCase = 0.25;
        public double iRatioLBoundUCase = 0.11;
        public double iRatioUBoundUCase = 0.17;
        public double ExclamThres1 { get; set; } = 0.5;
        public double ExclamThres2 { get; set; } = 0.3;
        public double DiffThresh { get; set; } = 0.8;
        public double MedianHeightMul { get; set; } = 0.95;
    }
}
