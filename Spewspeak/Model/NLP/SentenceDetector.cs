using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpNL.SentenceDetector;
using SharpNL.Utility;
using System.IO;

namespace Spewspeak.Model.NLP
{
    public class SentenceDetector
    {
        #region Static members definition
        private static readonly string TRAINING_MODEL_PATH = Properties.Settings.Default.OpenNLPResourcesPath + Properties.Settings.Default.SentDetectTrainingFile;
        private static readonly string TRAINING_LANGUAGE = Properties.Settings.Default.TrainingLanguage;
        #endregion

        #region Members definition
        private readonly SentenceDetectorME detector;
        #endregion

        #region Constructors definition
        public SentenceDetector()
        {
            this.detector = new SentenceDetectorME(TrainModel(Environment.CurrentDirectory + TRAINING_MODEL_PATH));
        }

        public SentenceDetector(FileStream modelStream)
        {
            SentenceModel model = new SentenceModel(modelStream);
            this.detector = new SentenceDetectorME(model);
        }

        public SentenceDetector(SentenceModel model)
        {
            this.detector = new SentenceDetectorME(model);
        }
        #endregion

        #region Methods definition
        public static SentenceModel TrainModel(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

            TrainingParameters trainParams = new TrainingParameters();
            trainParams.Set(Parameters.Iterations, "100");
            trainParams.Set(Parameters.Cutoff, "0");

            SentenceDetectorFactory detectorFactory = new SentenceDetectorFactory(TRAINING_LANGUAGE, true, null, null);
            SentenceSampleStream sampleStream = new SentenceSampleStream(new PlainTextByLineStream(fs));

            return SentenceDetectorME.Train(TRAINING_LANGUAGE, sampleStream, detectorFactory, trainParams);
        }

        public string[] Detect(string paragraph)
        {
            return this.detector.SentDetect(paragraph);
        }
        #endregion
    }
}
