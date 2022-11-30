using CRVProject.Helper;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRVProject.Ortsschild
{
    public class TemplateDatabase : IDisposable
    {
        public Dictionary<char, List<Mat>> Templates { get; private set; } = new Dictionary<char, List<Mat>>();

        public static TemplateDatabase LoadDatabase(string dirName)
        {
            var database = new TemplateDatabase();
            DirectoryInfo dir = new DirectoryInfo(dirName);
            var templateFiles = dir.GetFiles()
                .Where(fi => Util.SupportedImageTypes
                    .Select(type => fi.Name.ToLower().EndsWith($".{type}"))
                    .Contains(true))
                .ToArray();

            foreach(var file in templateFiles)
            {
                char character = file.Name[0];
                if (!database.Templates.ContainsKey(character))
                    database.Templates.Add(character, new List<Mat>());

                Mat image = Cv2.ImRead(file.FullName, ImreadModes.Grayscale);
                Cv2.Threshold(image, image, 127, 255, ThresholdTypes.Binary);
                database.Templates[character].Add(image);
            }

            return database;
        }

        public List<Mat> GetTemplates(char character)
        {
            if (!Templates.ContainsKey(character))
                return new List<Mat>();
            return Templates[character];
        }

        public void Dispose()
        {
            foreach(var k in Templates.Keys)
            {
                foreach(var mat in Templates[k])
                {
                    if (!mat.IsDisposed)
                        mat.Dispose();
                }
                Templates[k] = new List<Mat>();
            }
        }
    }
}
