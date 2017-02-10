using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spewspeak.Model.NLP
{
    public static class ConversionConditions
    {
        #region Static members definition
        public static readonly List<string> IncludedPhrases = new List<string>()
        {
            "NP",
            "VP",
            "ADJP",
            "ADVP",
        };

        public static readonly List<string> ExcludedPOS = new List<string>()
        {
            "EX",
            "PRP",
            "PRP$",
            "TO",
            "IN",
            "DT",
            "CD",
            "CC",
            "LS",
            "MD",
            "PDT",
            "POS",
            "RP",
            "SYM",
            "UH",
        };
        #endregion
    }
}
