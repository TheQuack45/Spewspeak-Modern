using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Spewspeak.Model;
using System.Windows.Media;

namespace Spewspeak.ViewModel
{
    /// <summary>
    /// Represents the ViewModel for the MainWindow View.
    /// </summary>
    class MainWindowViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Static members definition
        private static readonly List<string> WNErrorDisplayTexts = new List<string>()
        {
            @"The WordNet root directory was not found at the expected path. Please verify that the WordNet data directory is present within the 'inc' directory located within the same directory as the Spewspeak executable, then restart Spewspeak.",
            @"The integrity of the WordNet data directory has been compromised. Please either redownload and replace the directory then restart Spewspeak, or reinstall Spewspeak.",
            @"The OpenNLP resources root directory was not found at the expected path. Please verify that the OpenNLP resources directory is present within the 'inc' directory located within the same directory as the Spewspeak executable, then restart Spewspeak.",
            @"The integrity of the OpenNLP resources directory has been compromised. Please either redownload and replace the directory then restart Spewspeak, or reinstall Spewspeak.",
        };

        private const string PLACEHOLDER_TEXT = "Please enter your text to convert here.";
        private const string LOADING_TEXT = "Loading. Please wait.";
        private const string READING_EASE_TEXT_FORMAT = "Readability score: {0}";
        private const string READING_EASE_TEXT_CONVERTED_FORMAT = "Readability score: {0} (from {1})";

        private static readonly Brush STANDARD_LABEL_COLOR = Brushes.Black;
        private static readonly Brush SUCCESS_LABEL_COLOR = Brushes.Green;
        private static readonly Brush FAILURE_LABEL_COLOR = Brushes.Red;
        #endregion

        #region Members definition
        private ICommand startConversionCommand;
        public ICommand StartConversionCommand
        {
            get
            {
                if (startConversionCommand == null)
                {
                    startConversionCommand = new AsyncDelegateCommand(data => { this.CallConvertSentence(data.ToString()); });
                }
                return startConversionCommand;
            }
        }

        private ICommand startupCommand;
        public ICommand StartupCommand
        {
            get
            {
                if (startupCommand == null)
                {
                    startupCommand = new NoParamAsyncDelegateCommand(() => { this.RunStartupActions(); });
                }
                return startupCommand;
            }
        }

        private ICommand removePlaceholderTextCommand;
        public ICommand RemovePlaceholderTextCommand
        {
            get
            {
                if (removePlaceholderTextCommand == null)
                {
                    removePlaceholderTextCommand = new AsyncDelegateCommand(text => { this.CheckPlaceholderText(text.ToString(), true); });
                }
                return removePlaceholderTextCommand;
            }
        }

        private ICommand addPlaceholderTextCommand;
        public ICommand AddPlaceholderTextCommand
        {
            get
            {
                if (addPlaceholderTextCommand == null)
                {
                    addPlaceholderTextCommand = new AsyncDelegateCommand(text => { this.CheckPlaceholderText(text.ToString(), false); });
                }
                return addPlaceholderTextCommand;
            }
        }

        private ICommand calculateReadabilityScoreCommand;
        public ICommand CalculateReadabilityScoreCommand
        {
            get
            {
                if (calculateReadabilityScoreCommand == null)
                {
                    calculateReadabilityScoreCommand = new AsyncDelegateCommand(text => { this.TextChangedCheck(text.ToString()); });
                }
                return calculateReadabilityScoreCommand;
            }
        }

        private string boxText;
        public string BoxText
        {
            get { return boxText; }
            set { boxText = value; }
        }

        private bool isConverting;
        public bool IsConverting
        {
            get { return isConverting; }
            set { isConverting = value; }
        }

        private bool hasConverted;
        public bool HasConverted
        {
            get { return hasConverted; }
            set { hasConverted = value; }
        }

        private bool isConvertReady;
        public bool IsConvertReady
        {
            get { return isConvertReady; }
            set { isConvertReady = value; }
        }

        private string readingEaseText;
        public string ReadingEaseText
        {
            get { return readingEaseText; }
            set { readingEaseText = value; }
        }

        public double OriginalParagraphScore;

        private Brush readingEaseLabelColor;
        public Brush ReadingEaseLabelColor
        {
            get { return readingEaseLabelColor; }
            set { readingEaseLabelColor = value; }
        }
        #endregion

        #region Constructors definition
        /// <summary>
        /// Represents the ViewModel for the MainWindow.xaml View.
        /// </summary>
        public MainWindowViewModel()
        {

        }
        #endregion

        #region Methods
        private void CallConvertSentence(string data)
        {
            this.IsConverting = true;
            RaisePropertyChanged("IsConverting");

            this.OriginalParagraphScore = Conversion.CalculateReadabilityScore(data);
            this.BoxText = Conversion.ConvertParagraph(data);
            RaisePropertyChanged("BoxText");
            CalculateAndDisplayReadabilityScore(this.BoxText);

            this.IsConverting = false;
            RaisePropertyChanged("IsConverting");
        }

        private void TextChangedCheck(string text)
        {
            if (text == "")
            {
                DisplayReadabilityScore(0);
                IsConvertReady = false;
                RaisePropertyChanged(nameof(IsConvertReady));
            }
            else
            {
                IsConvertReady = true;
                RaisePropertyChanged(nameof(IsConvertReady));
            }
        }

        private void CalculateAndDisplayReadabilityScore(string paragraph)
        {
            if (BoxText == LOADING_TEXT || BoxText == PLACEHOLDER_TEXT || BoxText == "")
            {
                ReadingEaseLabelColor = STANDARD_LABEL_COLOR;
                RaisePropertyChanged(nameof(ReadingEaseLabelColor));
                OriginalParagraphScore = -1.0;
                DisplayReadabilityScore(0);
            }
            else
            {
                double score = Conversion.CalculateReadabilityScore(paragraph);
                if (OriginalParagraphScore != -1.0)
                {
                    if (score > OriginalParagraphScore)
                    {
                        // Successfully improved the "complexity" of the paragraph
                        ReadingEaseLabelColor = SUCCESS_LABEL_COLOR;
                        RaisePropertyChanged(nameof(ReadingEaseLabelColor));
                    }
                    else
                    {
                        // Did not successfully improve the "complexity" of the paragraph.
                        // We must need a bigger thesaurus.
                        ReadingEaseLabelColor = FAILURE_LABEL_COLOR;
                        RaisePropertyChanged(nameof(ReadingEaseLabelColor));
                    }
                    DisplayReadabilityScore(score, OriginalParagraphScore);
                }
                else
                {
                    OriginalParagraphScore = score;
                    DisplayReadabilityScore(score);
                }
            }
        }

        private void DisplayReadabilityScore(double score = 0)
        {
            ReadingEaseText = String.Format(READING_EASE_TEXT_FORMAT, Math.Round(score, 6));
            RaisePropertyChanged(nameof(ReadingEaseText));

            if (score == 0)
            {
                ReadingEaseLabelColor = STANDARD_LABEL_COLOR;
                RaisePropertyChanged(nameof(ReadingEaseLabelColor));
            }
        }

        private void DisplayReadabilityScore(double newScore, double oldScore)
        {
            ReadingEaseText = String.Format(READING_EASE_TEXT_CONVERTED_FORMAT, Math.Round(newScore, 6), Math.Round(oldScore, 6));
            RaisePropertyChanged(nameof(ReadingEaseText));
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            
            // Uses null-conditional operator to invoke the event handler if it is not null.
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Checks that all program dependencies are in the expected places at the time of startup.
        /// </summary>
        private void RunStartupActions()
        {
            IsConvertReady = false;
            RaisePropertyChanged("IsConvertReady");
            IsConverting = true;
            RaisePropertyChanged("IsConverting");
            BoxText = LOADING_TEXT;
            RaisePropertyChanged("BoxText");
            DisplayReadabilityScore();
            ReadingEaseLabelColor = STANDARD_LABEL_COLOR;
            RaisePropertyChanged(nameof(ReadingEaseLabelColor));
            OriginalParagraphScore = -1.0;
            RaisePropertyChanged(nameof(OriginalParagraphScore));

            int result = Conversion.RunStartupActions();

            if (result == 0)
            {
                // All WordNet files are present and the program can run.
                IsConvertReady = true;
                RaisePropertyChanged("IsConvertReady");
                IsConverting = false;
                RaisePropertyChanged("IsConverting");
                BoxText = PLACEHOLDER_TEXT;
                RaisePropertyChanged("BoxText");
            }
            else
            {
                BoxText = WNErrorDisplayTexts[result - 1];
                RaisePropertyChanged("BoxText");
            }
        }

        /// <summary>
        /// Add or remove the placeholder text depending on whether the user has entered their own text.
        /// </summary>
        /// <param name="text">Current text of the textbox, used to check if the user has entered text themselves.</param>
        /// <param name="isRemoval">Whether the textbox gained focus (true) or lost focus (false).</param>
        private void CheckPlaceholderText(string text, bool isRemoval)
        {
            if (isRemoval && (text == PLACEHOLDER_TEXT))
            {
                // Focus was gained and the current text is the placeholder text.
                BoxText = "";
                RaisePropertyChanged("BoxText");
            }
            else if (!isRemoval && (text == ""))
            {
                // Focus was lost and the current text is blank.
                BoxText = PLACEHOLDER_TEXT;
                RaisePropertyChanged("BoxText");
            }
        }
        #endregion
    }
}
