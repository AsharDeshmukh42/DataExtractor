using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataExtractor
{
    public class ParsedData
    {
        /// <summary>
        /// A collection of simple fields to be written to the output CSV.
        /// Example: ISIN, CFICode, Venue.
        /// </summary>
        public List<SimpleField> SimpleFields { get; set; }

        /// <summary>
        /// A collection of parsed complex fields (e.g., Contract Size from AlgoParams).
        /// </summary>
        public List<double> ComplexFields { get; set; }
    }

    public class SimpleField
    {
        /// <summary>
        /// Example: ISIN, CFICode, Venue fields as key-value pairs.
        /// </summary>
        public string ISIN { get; set; }
        public string CFICode { get; set; }
        public string Venue { get; set; }
    }
}
