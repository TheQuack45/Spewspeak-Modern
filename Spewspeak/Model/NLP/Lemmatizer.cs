using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpNL.Lemmatizer;
using SharpNL.Utility;
using System.IO;

namespace Spewspeak.Model.NLP
{
    class Lemmatizer
    {
        #region Static members definition
        private static readonly string TRAINING_MODEL_PATH = Properties.Settings.Default.OpenNLPResourcesPath + Properties.Settings.Default.LemmatizerTrainingFile;
        private static readonly string TRAINING_LANGUAGE = Properties.Settings.Default.TrainingLanguage;
        #endregion

        #region Members definition
        private readonly LemmatizerME lemmatizer;
        #endregion

        #region Constructors definition
        public Lemmatizer()
        {
            this.lemmatizer = new LemmatizerME(TrainModel(Environment.CurrentDirectory + TRAINING_MODEL_PATH));
        }

        public Lemmatizer(string path)
        {
            this.lemmatizer = new LemmatizerME(TrainModel(path));
        }

        public Lemmatizer(FileStream modelStream)
        {
            
            this.lemmatizer = new LemmatizerME(new LemmatizerModel(modelStream));
            
        }

        public Lemmatizer(LemmatizerModel model)
        {
            this.lemmatizer = new LemmatizerME(model);
        }
        #endregion

        #region Methods definition
        public static LemmatizerModel TrainModel(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

            TrainingParameters trainParams = new TrainingParameters();
            trainParams.Set(Parameters.Iterations, "1");
            trainParams.Set(Parameters.Cutoff, "0");

            LemmatizerFactory lemmatizerFactory = new LemmatizerFactory();
            LemmaSampleStream sampleStream = new LemmaSampleStream(new PlainTextByLineStream(fs));

            return LemmatizerME.Train(TRAINING_LANGUAGE, sampleStream, trainParams, lemmatizerFactory);
        }

        public string[] Lemmatize(string[] tokens, string[] tags)
        {
            return this.lemmatizer.Lemmatize(tokens, tags);
        }
        #endregion
    }
}
