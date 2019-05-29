using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace StatisticCS
{
    public class Test
    {
        public static Data data = new Data();
        public static List<Attribute> attrs = new List<Attribute>();
        public static List<string> attrNames = new List<string>();
        public static string target = null;
        public static Regex attrRegex = new Regex(@"^@ATTRIBUTE\s+(?<name>.*?)\s+{(?<values>(.+))}$", RegexOptions.Compiled);

        public static void Init(string path)
        {
            var lines = File.ReadLines(path);
            int index = 0;
            var examples = new List<Dictionary<string, string>>();
            foreach(var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                if(line.StartsWith("@ATTRIBUTE"))
                {
                    var match = attrRegex.Match(line);
                    var name = match.Groups["name"].Value;
                    var values = match.Groups["values"].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    attrNames.Add(name);
                    var attr = new Attribute(index++, name, values.ToList());
                    attrs.Add(attr);

                    target = name;
                }
                else if(line[0] != '@')
                {
                    var segs = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    var example = new Dictionary<string, string>();
                    for(int i = 0; i < segs.Length; i++)
                    {
                        var name = attrNames[i];
                        var value = segs[0];
                        example.Add(name, value);
                    }
                    examples.Add(example);
                }
            }
            data.Examples = examples; 
        }
    }
}
