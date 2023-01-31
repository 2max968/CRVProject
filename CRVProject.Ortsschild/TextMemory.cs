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
        double AreaThreshhold = 0.03;

        List<TextEntry> entries = new List<TextEntry>();
        
        public void PushEntry(double timestamp, double confidence, string text, int framePos, double size, bool ausfahrt)
        {
            if(entries.Count > 0 && timestamp - entries.Last().Timestamp >= maxTimeout)
            {
                entries.Clear();
            }
            entries.Add(new TextEntry(timestamp, confidence, text, framePos, size, ausfahrt));
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
                    TextEntry? bestResult = null;
                    foreach (var result in entries)
                        if ((bestResult == null || result.Confidence > bestResult.Confidence) && result.Size > AreaThreshhold)
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
        public double Size;
        public bool Ausfahrt;

        public TextEntry(double timestamp, double confidence, string text, int framePos, double size, bool ausfahrt)
        {
            Timestamp = timestamp;
            Confidence = confidence;
            Text = text;
            FramePos = framePos;
            Size = size;
            Ausfahrt = ausfahrt;
        }
    }
}
