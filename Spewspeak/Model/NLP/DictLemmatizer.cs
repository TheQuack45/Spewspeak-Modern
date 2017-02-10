using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpNL.Lemmatizer;
using SharpNL.Utility;

namespace Spewspeak.Model.NLP
{
    class DictLemmatizer
    {
        #region Static members definition
        private static readonly string DICTIONARY_FILE_PATH = Properties.Settings.Default.OpenNLPResourcesPath + Properties.Settings.Default.LemmatizerTrainingFile;
        #endregion

        #region Members definition
        private readonly DictionaryLemmatizer lemmatizer;
        #endregion

        #region Constructors definition
        public DictLemmatizer()
        {
            this.lemmatizer = new DictionaryLemmatizer(Environment.CurrentDirectory + DICTIONARY_FILE_PATH);
        }

        public DictLemmatizer(string path)
        {
            this.lemmatizer = new DictionaryLemmatizer(path);
        }
        #endregion

        #region Methods definition
        public string[] Lemmatize(string[] tokens, string[] tags)
        {
            return this.lemmatizer.Lemmatize(tokens, tags);
        }

        public string Lemmatize(string token, string tag)
        {
            return this.lemmatizer.Lemmatize(new string[1] { token }, new string[1] { tag })[0];
        }
        #endregion
    }
}
