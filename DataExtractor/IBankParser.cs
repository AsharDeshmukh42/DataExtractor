using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataExtractor
{
    public interface IBankParser
    {
        ParsedData Parse(string filePath);
        void WriteOutput(ParsedData data, string outputPath);
    }
}
