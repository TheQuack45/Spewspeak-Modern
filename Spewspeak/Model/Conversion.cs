using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FolderIntegrityVerifier;
using Spewspeak.Model.NLP;
using NHunspell;
using WordRatingLibrary;
using System.Text.RegularExpressions;
using SharpNL.Utility;
using WnLexicon;

namespace Spewspeak.Model
{
    /// <summary>
    /// A static class containing the main sentence conversion method and any relevant variables.
    /// </summary>
    static class Conversion
    {
        #region Static members definition
        private const string ALL_FILES_MATCH_PATTERN = "*";
        private const string NON_WORD_MATCH_PATTERN = @"[\d]{1,}.?[\d]{1,}";
        private const string WORD_MATCH_PATTERN = @"\b[\w']+\b";
        private const string SENTENCE_CASE_MATCH_PATTERN = "(^[\"]{0,1}[a-z]?|\\.\\s[\"]{0,1}[a-z])";
        private const string IS_BEFORE_PUNCTUATION_MATCH_PATTERN = @"[,;]{1}";
        private static readonly string WORDNET_ROOT_DIRECTORY = Properties.Settings.Default.WordNetRootDir;
        private static readonly string OPENNLP_RESOURCES_ROOT_DIRECTORY = Properties.Settings.Default.OpenNLPResourcesPath;
        private static readonly string OPENNLP_TOKENIZER_MODEL = Properties.Settings.Default.TokenizerModel;
        private static readonly string OPENNLP_SENT_DETECT_MODEL = Properties.Settings.Default.SentDetectModel;
        private static readonly string OPENNLP_POS_TAGGER_MODEL = Properties.Settings.Default.POSTaggerModel;
        private static readonly string OPENNLP_CHUNKER_MODEL = Properties.Settings.Default.ChunkerModel;
        private static readonly string OPENNLP_LEMMATIZER_MODEL = Properties.Settings.Default.LemmatizerModel;
        private static readonly string NHUNSPELL_THESAURUS_FILE = Properties.Settings.Default.NHunspellResourcesPath + Properties.Settings.Default.NHunspellEnglishThesaurus;

        private static SentenceDetector MEDetector;
        private static Tokenizer METokenizer;
        private static POSTagger METagger;
        private static Chunker MEChunker;
        private static DictLemmatizer SimpleLemmatizer;
        private static NameFinder MENameFinder;

        private static MyThes NHThesaurus;
        #endregion

        #region Methods definition
        /// <summary>
        /// Take in a paragraph and replace all non-ignored words with a 'smarter' synonym.
        /// </summary>
        /// <param name="data">Paragraph to convert.</param>
        /// <returns>The 'improved' paragraph.</returns>
        public static string ConvertParagraph(string data)
        {
            StringBuilder output = new StringBuilder();

            string[] sentences = MEDetector.Detect(data);
            
            foreach (string sentence in sentences)
            {
                string[] tokens = METokenizer.Tokenize(sentence);
                Span[] names = MENameFinder.Find(tokens);
                char[] sentenceArr = sentence.ToCharArray();
                for (int cCharIndex = 0; cCharIndex < sentence.Length; cCharIndex++)
                {
                    if (Char.IsUpper(sentenceArr[cCharIndex]))
                    {
                        bool isName = false;
                        for (int cSpanIndex = 0; cSpanIndex < names.Length; cSpanIndex++)
                        {
                            if (cCharIndex == names[cSpanIndex].Start)
                            {
                                isName = true;
                            }
                        }

                        if (!isName)
                        {
                            sentenceArr[cCharIndex] = Char.ToLower(sentenceArr[cCharIndex]);
                            // TODO: Have to keep track of where the capitals were in the original sentence to add them again later.
                        }
                    }
                }
                tokens = METokenizer.Tokenize(new string(sentenceArr));
                string[] tags = METagger.Tag(tokens);

                string[] chunks = MEChunker.Chunk(tokens, tags);

                Wnlib.PartsOfSpeech pos = Wnlib.PartsOfSpeech.Unknown;
                for (int i = 0; i < tokens.Length; i++)
                {
                    if (!ConversionConditions.ExcludedPOS.Contains(tags[i]))
                    {
                        // Current token POS is not excluded from conversion.
                        if (Regex.IsMatch(chunks[i], "-") && ConversionConditions.IncludedPhrases.Contains(Regex.Split(chunks[i], "-")[1]))
                        {
                            // The containing phrase of the current token is not excluded.
                            switch (tags[i])
                            {
                                case "NN":
                                case "NNS":
                                    pos = Wnlib.PartsOfSpeech.Noun;
                                    break;
                                case "JJ":
                                case "JJR":
                                case "JJS":
                                    pos = Wnlib.PartsOfSpeech.Adj;
                                    break;
                                case "RB":
                                case "RBR":
                                case "RBS":
                                    pos = Wnlib.PartsOfSpeech.Adv;
                                    break;
                                case "VB":
                                case "VBD":
                                case "VBG":
                                case "VBN":
                                case "VBP":
                                case "VBZ":
                                    pos = Wnlib.PartsOfSpeech.Verb;
                                    break;
                            }

                            string mostComplexSynonym = GetMostComplexSynyonymScoredWN(tokens[i], pos);
                            output.Append(mostComplexSynonym);
                        }
                        else
                        {
                            // The containing phrase of the current token is excluded.
                            output.Append(tokens[i]);
                        }
                    }
                    else
                    {
                        // Current token POS is excluded from conversion.
                        output.Append(tokens[i]);
                    }

                    // Checking if a space needs to be added after this token (eg, it is not at the end of the line).
                    // NOTE: Uses two inline if statements.
                    bool isBeforePunctuation;
                    try
                    {
                        isBeforePunctuation = Regex.IsMatch(tokens[i + 1], IS_BEFORE_PUNCTUATION_MATCH_PATTERN);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        isBeforePunctuation = false;
                    }

                    output.Append((i >= tokens.Length - (sentence.EndsWith(".") ? 2 : 1)) || isBeforePunctuation ? "" : " ");
                    if (tokens[i] == ".")
                        { output.Append(Array.IndexOf(sentences, sentence) == (sentences.Length - 1) ? "" : " "); }

                    try
                    {
                        if ((chunks[i + 1] == "O" && tokens[i + 1].Contains("'")) || tokens[i + 1] == "'s")
                        {
                            // This is a contraction. Remove the space between the two parts.
                            output.Length--;
                        }
                    }
                    catch (IndexOutOfRangeException)
                        { /* Don't need to do anything, just means we don't need to remove the last space. */ }
                }
            }
            
            return AddPeriod(StringToSentenceCase(output.ToString()));
        }

        public static string AddPeriod(string input)
        {
            if (!input.EndsWith("."))
            {
                input += ".";
            }

            return input;
        }

        public static string StringToSentenceCase(string input)
        {
            string lower = input.ToString();
            Regex capitalizeRegex = new Regex(SENTENCE_CASE_MATCH_PATTERN, RegexOptions.IgnoreCase);
            string result = capitalizeRegex.Replace(lower, s => s.Value.ToUpper());

            return result;
        }

        /// <summary>
        /// Performs startup checks and then initializes NLP classes if the program install is valid.
        /// </summary>
        /// <returns>0 if NLP classes were initialized properly. Otherwise, returns as RunStartupChecks.</returns>
        public static int RunStartupActions()
        {
            int startupChecksOutput = RunStartupChecks();

            if (startupChecksOutput != 0)
                { return startupChecksOutput; }

            MEDetector = new SentenceDetector(new FileStream(Environment.CurrentDirectory + OPENNLP_RESOURCES_ROOT_DIRECTORY + OPENNLP_SENT_DETECT_MODEL, FileMode.Open, FileAccess.Read));
            METokenizer = new Tokenizer(new FileStream(Environment.CurrentDirectory + OPENNLP_RESOURCES_ROOT_DIRECTORY + OPENNLP_TOKENIZER_MODEL, FileMode.Open, FileAccess.Read));
            METagger = new POSTagger(new FileStream(Environment.CurrentDirectory + OPENNLP_RESOURCES_ROOT_DIRECTORY + OPENNLP_POS_TAGGER_MODEL, FileMode.Open, FileAccess.Read));
            MEChunker = new Chunker(new FileStream(Environment.CurrentDirectory + OPENNLP_RESOURCES_ROOT_DIRECTORY + OPENNLP_CHUNKER_MODEL, FileMode.Open, FileAccess.Read));
            SimpleLemmatizer = new DictLemmatizer();
            MENameFinder = new NameFinder(NameFinder.DETECTOR_TYPE.Person);
            NHThesaurus = new MyThes(Environment.CurrentDirectory + NHUNSPELL_THESAURUS_FILE);
            Wnlib.WNCommon.path = Environment.CurrentDirectory + WORDNET_ROOT_DIRECTORY;

            return 0;
        }

        /// <summary>
        /// Check if all required files are in the expected locations. Returns 0 if all checks pass.
        /// </summary>
        /// <returns>Integer 0 if there are no problems, integer 1 if the WordNet directory is not found, integer 2 if the hash of the WordNet directory does not match, integer 3 if the OpenNLP resources directory is not found, integer 4 if the hash of the OpenNLP resources directory does not match.</returns>
        public static int RunStartupChecks()
        {
            string WNPath, opNLPPath;

            try
            {
                WNPath = Environment.CurrentDirectory + WORDNET_ROOT_DIRECTORY;
                opNLPPath = Environment.CurrentDirectory + OPENNLP_RESOURCES_ROOT_DIRECTORY;
            }
            catch (Exception e)
            {
                throw e;
            }

            if (!Directory.Exists(WNPath))
            {
                // The WordNet root directory does not exist.
                return 1;
            }

            if (!Verifier.Verify(WNPath, Properties.Settings.Default.WordNetDirHash))
            {
                // The WordNet directory hash does not match the expected hash.
                return 2;
            }

            if (!Directory.Exists(opNLPPath))
            {
                // The OpenNLP resources root directory does not exist.
                return 3;
            }

            if (!Verifier.Verify(opNLPPath, Properties.Settings.Default.OpenNLPResourcesDirHash))
            {
                // The OpenNLP resources directory hash does not match the expected hash.
                return 4;
            }

            return 0;
        }

        /// <summary>
        /// Gets all synonyms of the given word in the given part of speech and finds the most complex word in the set.
        /// Uses WordNet as thesaurus and weighted scores to determine which is most complex.
        /// </summary>
        /// <param name="word">Word to get synonyms of.</param>
        /// <param name="pos">Part of speech of the given word. Used to find more accurate synonyms.</param>
        /// <returns>The most complex synonym of the given word in the given part of speech.</returns>
        public static string GetMostComplexSynyonymScoredWN(string word, Wnlib.PartsOfSpeech pos)
        {
            // TODO: Lemmatization?

            if (pos == Wnlib.PartsOfSpeech.Unknown)
            {
                // We're gonna have some serious problems (namely, a NullReferenceException) if we don't back out of this right now.
                return word;
            }

            string[] synonymsArr = Lexicon.FindSynonyms(word, pos, false);

            if (synonymsArr == null || synonymsArr.Length == 0)
            {
                return word;
            }

            List<string> synonyms = new List<string>(synonymsArr);
            string mostComplexSynyonym = "";
            double mostComplexScore = 0.0;

            #region Synonym collection setup
            if (!synonyms.Contains<string>(word))
            {
                synonyms.Add(word);
            }
            #endregion

            #region Most complex synonym find
            foreach (string cs in synonyms)
            {
                double csScore = WordRater.GetTotalScore(cs);
                if (mostComplexSynyonym == "" || csScore > mostComplexScore)
                {
                    mostComplexSynyonym = cs;
                    mostComplexScore = csScore;
                }
            }
            #endregion

            return mostComplexSynyonym;
        }

        public static string GetMostComplexSynonymScored(string word)
        {
            // TODO: Lemmatization

            ThesResult result = NHThesaurus.Lookup(word);
            if (result == null)
            {
                // No results in thesaurus.
                return word;
            }

            Dictionary<string, List<ThesMeaning>> synonymsDict = result.GetSynonyms();
            string mostComplexSynonym = "";
            double mostComplexScore = 0;

            // TODO: Check if each 'synonym' can be the same POS tag as the original word before putting it into the synonym collection.

            #region Synonym collection setup
            List<string> synonyms = new List<string>();
            foreach (string synonym in synonymsDict.Keys)
            {
                // Get each key (synonym) from the result dictionary and add it to the list of synonyms.
                synonyms.Add(synonym);
            }

            if (synonyms.Contains(word)) { /* Original word is already in the list of synonyms. Do nothing. */ }
            else { /* Original word is not in the list of synonyms. Add it. */ synonyms.Add(word); }
            #endregion

            #region Find the most complex synonym
            foreach (string cs in synonyms)
            {
                double csScore = WordRater.GetTotalScore(cs);
                if (mostComplexSynonym == "" || csScore > mostComplexScore)
                {
                    mostComplexSynonym = cs;
                    mostComplexScore = csScore;
                }
            }
            #endregion

            return mostComplexSynonym;
        }

        /// <summary>
        /// Get all synonyms of the given word and find the most complex word of the set.
        /// </summary>
        /// <param name="word">The word to find synonyms for.</param>
        /// <returns>The most complex of all the synonyms and the original word.</returns>
        public static string GetMostComplexSynonym(string word)
        {
            // TODO: Lemmatization

            ThesResult result = NHThesaurus.Lookup(word);
            if (result == null)
            {
                // No results in thesaurus.
                return word;
            }

            Dictionary<string, List<ThesMeaning>> synonymsDict = result.GetSynonyms();
            string mostComplexSynonym = "";

            // TODO: Check if each 'synonym' can be the same POS tag as the original word before putting it into the synonym collection.

            #region Synonym collection setup
            List<string> synonyms = new List<string>();
            foreach (string synonym in synonymsDict.Keys)
            {
                // Get each key (synonym) from the result Dictionary and add it to the list of synonyms.
                synonyms.Add(synonym);
            }

            if (synonyms.Contains(word)) { /* Original word is already in the list of synonyms. Do nothing. */ }
            else { /* Original word is not in the list of synonyms. Add it. */ synonyms.Add(word); }
            #endregion

            #region Find the most complex synoynm
            foreach (string cs in synonyms)
            {
                // Check if the current word is more complex than the previous most complex word.
                // If so, replace the most complex word with the current word.
                if (mostComplexSynonym == "" || WordRater.CompareWords(mostComplexSynonym, cs) == cs) { mostComplexSynonym = cs; }
            }
            #endregion

            return mostComplexSynonym;
        }

        public static double CalculateReadabilityScore(string paragraph)
        {
            if (paragraph == "")
            {
                return 0;
            }

            int sentenceCount = MEDetector.Detect(paragraph.ToLower()).Length;
            if (sentenceCount == 0)
            {
                throw new InvalidOperationException("The given paragraph could not be split into at least one sentence.");
            }

            List<string> words = METokenizer.Tokenize(paragraph.ToLower()).ToList<string>();
            if (words.Count == 0)
            {
                throw new InvalidOperationException("The given paragraph could not be split into at least one word.");
            }

            for (int i = 0; i < words.Count; i++)
            {
                if (words[i] == "%" || words[i].Count(c => char.IsLetter(c)) != words[i].Length) 
                {
                    words.RemoveAt(i);
                    i--;
                }
            }

            return ReadabilityScorer.GetScore(words, sentenceCount);
        }
        #endregion
    }
}
