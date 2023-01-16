using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRVProject.Ortsschild
{
    public class TextMemory
    {
        double maxTimeout = 0.5;
        double minInScreen = 0.5;

        List<TextEntry> entries = new List<TextEntry>();
        
        public void PushEntry(double timestamp, double confidence, string text, int framePos)
        {
            if(entries.Count > 0 && timestamp - entries.Last().Timestamp >= maxTimeout)
            {
                entries.Clear();
            }
            entries.Add(new TextEntry(timestamp, confidence, text, framePos));
        }

        public TextEntry? GetResult(double currentTime)
        {
            if(entries.Count == 0)
            {
                return null;
            }

            if(currentTime - entries.Last().Timestamp >= maxTimeout)
            {
                double duration = entries.Last().Timestamp - entries.First().Timestamp;
                if(duration > minInScreen)
                {
                    var bestResult = entries.First();
                    foreach (var result in entries)
                        if (result.Confidence > bestResult.Confidence)
                            bestResult = result;
                    entries.Clear();
                    return bestResult;
                }
            }

            return null;
        }
    }

    public class TextEntry
    {
        public double Timestamp;
        public double Confidence;
        public string Text;
        public int FramePos;

        public TextEntry(double timestamp, double confidence, string text, int framePos)
        {
            Timestamp = timestamp;
            Confidence = confidence;
            Text = text;
            FramePos = framePos;
        }
    }
}
