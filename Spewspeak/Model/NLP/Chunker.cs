using System;
using SharpNL.Chunker;
using System.IO;
using SharpNL.Utility;

namespace Spewspeak.Model.NLP
{
    class Chunker
    {
        #region Static members definition
        private static readonly string TRAINING_FILE_PATH = Properties.Settings.Default.OpenNLPResourcesPath + Properties.Settings.Default.SentDetectTrainingFile;
        private static readonly string TRAINING_LANGUAGE = Properties.Settings.Default.TrainingLanguage;
        #endregion

        #region Members definition
        private readonly ChunkerME chunker;
        #endregion

        #region Constructors definition
        public Chunker()
        {
            this.chunker = new ChunkerME(TrainModel(Environment.CurrentDirectory + TRAINING_FILE_PATH));
        }

        public Chunker(FileStream modelStream)
        {
            ChunkerModel model = new ChunkerModel(modelStream);
            this.chunker = new ChunkerME(model);
        }

        public Chunker(ChunkerModel model)
        {
            this.chunker = new ChunkerME(model);
        }
        #endregion

        #region Methods definition
        public static ChunkerModel TrainModel(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            ChunkSampleStream stream = new ChunkSampleStream(new PlainTextByLineStream(fs));

            TrainingParameters trainParams = new TrainingParameters();
            trainParams.Set(Parameters.Iterations, "70");
            trainParams.Set(Parameters.Cutoff, "1");

            return ChunkerME.Train(TRAINING_LANGUAGE, stream, trainParams, new ChunkerFactory());
        }

        public string[] Chunk(string[] tokens, string[] tags)
        {
            return this.chunker.Chunk(tokens, tags);
        }
        #endregion
    }
}
