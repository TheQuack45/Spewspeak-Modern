using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpNL.Tokenize;
using SharpNL.Utility;
using System.IO;

namespace Spewspeak.Model.NLP
{
    class Tokenizer
    {
        #region Static members definition
        private static readonly string TRAINING_MODEL_PATH = Properties.Settings.Default.OpenNLPResourcesPath + Properties.Settings.Default.SentDetectTrainingFile;
        private static readonly string TRAINING_LANGUAGE = Properties.Settings.Default.TrainingLanguage;
        #endregion

        #region Members definition
        private readonly TokenizerME tokenizer;
        #endregion

        #region Constructors definition
        public Tokenizer()
        {
            this.tokenizer = new TokenizerME(TrainModel(Environment.CurrentDirectory + TRAINING_MODEL_PATH));
        }

        public Tokenizer(FileStream modelStream)
        {
            TokenizerModel model = new TokenizerModel(modelStream);
            this.tokenizer = new TokenizerME(model);
        }

        public Tokenizer(TokenizerModel model)
        {
            this.tokenizer = new TokenizerME(model);
        }
        #endregion

        #region Methods definition
        public static TokenizerModel TrainModel(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            TokenSampleStream stream = new TokenSampleStream(new PlainTextByLineStream(fs));

            TrainingParameters trainParams = new TrainingParameters();
            trainParams.Set(Parameters.Iterations, "100");
            trainParams.Set(Parameters.Cutoff, "0");

            return TokenizerME.Train(stream, new TokenizerFactory(TRAINING_LANGUAGE, null, true), trainParams);
        }

        public string[] Tokenize(string sentence)
        {
            return this.tokenizer.Tokenize(sentence);
        }
        #endregion
    }
}
