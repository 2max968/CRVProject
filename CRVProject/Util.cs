using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRVProject
{
    public class Util
    {
        public static int Select(string comment, params string[] options)
        {
            char[] chars = new char[options.Length];
            Console.WriteLine(comment);
            for(int i = 0; i < options.Length; i++)
            {
                char chr = (char)('1' + i);
                if (i >= 9)
                    chr = (char)('a' + i - 9);
                chars[i] = chr;
                Console.WriteLine($" {chr}) {options[i]}");
            }
            Console.Write("> ");

            while(true)
            {
                var inp = Console.ReadKey(true);
                if (chars.Contains(inp.KeyChar))
                {
                    int selectedIndex = chars.Select((chr, index) => (chr, index)).Where(i => i.chr == inp.KeyChar).First().index;
                    Console.WriteLine($"Selected '{options[selectedIndex]}'");
                    return selectedIndex;
                }
            }
        }
    }
}
