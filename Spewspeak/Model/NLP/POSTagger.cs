using SharpNL.ML.Model;
using SharpNL.POSTag;
using SharpNL.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spewspeak.Model.NLP
{
    class POSTagger
    {
        #region Static members definition
        private static readonly string TRAINING_MODEL_PATH = Properties.Settings.Default.OpenNLPResourcesPath + Properties.Settings.Default.SentDetectTrainingFile;
        private static readonly string TRAINING_LANGUAGE = Properties.Settings.Default.TrainingLanguage;
        #endregion

        #region Members definition
        private readonly POSTaggerME tagger;
        #endregion

        #region Constructors definition
        public POSTagger(ModelType type = ModelType.Maxent)
        {
            this.tagger = new POSTaggerME(TrainModel(Environment.CurrentDirectory + TRAINING_MODEL_PATH, type));
        }

        public POSTagger(FileStream modelStream)
        {
            POSModel model = new POSModel(modelStream);
            this.tagger = new POSTaggerME(model);
        }

        public POSTagger(POSModel model)
        {
            this.tagger = new POSTaggerME(model);
        }
        #endregion

        #region Methods definition
        public static POSModel TrainModel(string path, ModelType mt)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            WordTagSampleStream stream = new WordTagSampleStream(fs);

            TrainingParameters trainParams = new TrainingParameters();
            trainParams.Set(Parameters.Iterations, "100");
            trainParams.Set(Parameters.Cutoff, "0");
            switch (mt)
            {
                case ModelType.Maxent:
                    trainParams.Set(Parameters.Algorithm, "MAXENT");
                    break;
                case ModelType.Perceptron:
                    trainParams.Set(Parameters.Algorithm, "PERCEPTRON");
                    break;
                default:
                    throw new NotSupportedException();
            }

            return POSTaggerME.Train(TRAINING_LANGUAGE, stream, trainParams, new POSTaggerFactory());
        }

        public string[] Tag(string[] sentenceTokens)
        {
            return this.tagger.Tag(sentenceTokens);
        }
        #endregion
    }
}
