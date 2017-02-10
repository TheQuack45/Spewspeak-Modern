using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpNL.NameFind;
using SharpNL.Utility;
using System.IO;

namespace Spewspeak.Model.NLP
{
    class NameFinder
    {
        #region Static members definition
        private static readonly string DATE_MODEL_PATH = Properties.Settings.Default.OpenNLPResourcesPath + Properties.Settings.Default.NameFindDateModelPath;
        private static readonly string LOCATION_MODEL_PATH = Properties.Settings.Default.OpenNLPResourcesPath + Properties.Settings.Default.NameFindLocationModelPath;
        private static readonly string MONEY_MODEL_PATH = Properties.Settings.Default.OpenNLPResourcesPath + Properties.Settings.Default.NameFindMoneyModelPath;
        private static readonly string ORGANIZATION_MODEL_PATH = Properties.Settings.Default.OpenNLPResourcesPath + Properties.Settings.Default.NameFindOrganizationModelPath;
        private static readonly string PERCENTAGE_MODEL_PATH = Properties.Settings.Default.OpenNLPResourcesPath + Properties.Settings.Default.NameFindPercentageModelPath;
        private static readonly string PERSON_MODEL_PATH = Properties.Settings.Default.OpenNLPResourcesPath + Properties.Settings.Default.NameFindPersonModelPath;
        private static readonly string TIME_MODEL_PATH = Properties.Settings.Default.OpenNLPResourcesPath + Properties.Settings.Default.NameFindTimeModelPath;

        public enum DETECTOR_TYPE
        {
            Date,
            Location,
            Money,
            Organization,
            Percentage,
            Person,
            Time,
        };
        #endregion

        #region Members definition
        private readonly NameFinderME nameFinder;
        #endregion

        #region Constructors definition
        public NameFinder(DETECTOR_TYPE finderType = DETECTOR_TYPE.Person)
        {
            string modelPath = "";

            switch (finderType)
            {
                //case DETECTOR_TYPE.Date:
                //    this.nameFinder = new NameFinderME(new TokenNameFinderModel(new FileStream(Environment.CurrentDirectory + DATE_MODEL_PATH, FileMode.Open, FileAccess.Read)));
                //    break;
                //case DETECTOR_TYPE.Location:
                //    this.nameFinder = new NameFinderME(new TokenNameFinderModel(new FileStream(Environment.CurrentDirectory + LOCATION_MODEL_PATH, FileMode.Open, FileAccess.Read)));
                //    break;
                //case DETECTOR_TYPE.Money:
                //    this.nameFinder = new NameFinderME(new TokenNameFinderModel(new FileStream(Environment.CurrentDirectory + MONEY_MODEL_PATH, FileMode.Open, FileAccess.Read)));
                //    break;
                //case DETECTOR_TYPE.Organization:
                //    this.nameFinder = new NameFinderME(new TokenNameFinderModel(new FileStream(Environment.CurrentDirectory + ORGANIZATION_MODEL_PATH, FileMode.Open, FileAccess.Read)));
                //    break;
                //case DETECTOR_TYPE.Percentage:
                //    this.nameFinder = new NameFinderME(new TokenNameFinderModel(new FileStream(Environment.CurrentDirectory + PERCENTAGE_MODEL_PATH, FileMode.Open, FileAccess.Read)));
                //    break;
                //case DETECTOR_TYPE.Person:
                //    this.nameFinder = new NameFinderME(new TokenNameFinderModel(new FileStream(Environment.CurrentDirectory + PERSON_MODEL_PATH, FileMode.Open, FileAccess.Read)));
                //    break;
                //case DETECTOR_TYPE.Time:
                //    this.nameFinder = new NameFinderME(new TokenNameFinderModel(new FileStream(Environment.CurrentDirectory + TIME_MODEL_PATH, FileMode.Open, FileAccess.Read)));
                //    break;
                case DETECTOR_TYPE.Date:
                    modelPath = DATE_MODEL_PATH;
                    break;
                case DETECTOR_TYPE.Location:
                    modelPath = LOCATION_MODEL_PATH;
                    break;
                case DETECTOR_TYPE.Money:
                    modelPath = MONEY_MODEL_PATH;
                    break;
                case DETECTOR_TYPE.Organization:
                    modelPath = ORGANIZATION_MODEL_PATH;
                    break;
                case DETECTOR_TYPE.Percentage:
                    modelPath = PERCENTAGE_MODEL_PATH;
                    break;
                case DETECTOR_TYPE.Person:
                    modelPath = PERSON_MODEL_PATH;
                    break;
                case DETECTOR_TYPE.Time:
                    modelPath = TIME_MODEL_PATH;
                    break;
            }

            this.nameFinder = new NameFinderME(new TokenNameFinderModel(new FileStream(Environment.CurrentDirectory + modelPath, FileMode.Open, FileAccess.Read)));
        }

        public NameFinder(FileStream modelStream)
        {
            TokenNameFinderModel model = new TokenNameFinderModel(modelStream);
            this.nameFinder = new NameFinderME(model);
        }

        public NameFinder(TokenNameFinderModel model)
        {
            this.nameFinder = new NameFinderME(model);
        }
        #endregion

        #region Methods definition
        public Span[] Find(string[] tokens)
        {
            return this.nameFinder.Find(tokens);
        }
        #endregion
    }
}
