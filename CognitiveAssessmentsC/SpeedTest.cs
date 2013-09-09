using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CognitiveAssessmentsC
{
    class SpeedTest
    {
        public struct Trial
        {
            // test data
            public int trialNum;
            public int reactTime;
            public bool correct;

            public Trial(int trialNum, int reactTime, bool correct) {
                this.trialNum = trialNum;
                this.reactTime = reactTime;
                this.correct = correct;
            }
        }

        // use generic list since we don't know how many trials there will be
        public List<Trial> trials;

        // Test summary data
        public int totalCorrect;
        public int totalCompleted;
        public double mrtCorrect;       // mrt = median reaction time
        public double mrtIncorrect;
        public int trtCorrect;          // trt = total reaction time
        public int trtIncorrect;

        private Random rand;    // global random number generator
        private char[] randomChars = { 'ᴎ', 'ᴒ', 'ᴥ', 'Ψ', 'ξ', 'Φ', 'η', '§', 'ƍ', 'λ', 'Ϫ', 'Ϡ', 'Ѧ', 'ϑ', 'ϟ' };
        private int numTrials;

        // constructor
        public SpeedTest() {
            trials = new List<Trial>(150);  // set initial capacity to 150
            rand = new Random();
            numTrials = 0;
        }

        #region Test Generation

        /* generate 10 unique <key, value> string pairs, 
         * as well as 5 questions: each question has 4 options, 
         * only one of which is correct */
        public Dictionary<string, string> generateGrid(out string[] answers, out string[,] options) {

            // create the main grid
            Dictionary<string, string> dict = new Dictionary<string, string>(10);
            for (int i = 1; i <= 10; i++) {
            FindRandoms:
                string num = rand.Next(1000, 10000).ToString();
                string str = randString();
                if (dict.Keys.Contains(num) || dict.Values.Contains(str))
                    goto FindRandoms;
                dict.Add(num, str);
            }

            // pick 5 unique answers
            ArrayList answerIndexes = CogTest.createRandomList(0, 9, 5, ref rand);

            options = new string[5, 4];
            answers = new string[5];

            // pick 4 options for each question (one of which is correct)
            for (int i = 0; i < 5; i++) {
                answers[i] = dict.ElementAt((int)answerIndexes[i]).Value;

                // choose which option will be the correct answer
                options[i, rand.Next(0, 4)] = dict.ElementAt((int)answerIndexes[i]).Key;

                // set other possible options (must be different from correct answer)
            GenerateOtherOptions:
                ArrayList dictIndexes = CogTest.createRandomList(0, 9, 3, ref rand);
                if (dictIndexes.Contains((int)answerIndexes[i]))
                    goto GenerateOtherOptions;

                int arrayListIndex = 0;
                for (int j = 0; j < 4; j++) {
                    if (options[i, j] == null)
                        options[i, j] = dict.ElementAt((int)dictIndexes[arrayListIndex++]).Key;
                }
            }

            return dict;
        }

        // generates a 4-character random string
        private string randString() {
            string result = "";
            for (int i = 1; i <= 4; i++)
                result += randomChars[rand.Next(0, randomChars.Length)];
            return result;
        }

        #endregion

        // process input for the current trial
        public void input(bool correct, int reactTime) {
            totalCompleted++;
            if (correct == true) {
                totalCorrect++;
                trtCorrect += reactTime;
            } else {
                trtIncorrect += reactTime;
            }
            trials.Add(new Trial(++numTrials, reactTime, correct));
        }

        #region Test Completion

        // calculate medians and save results
        public void finish() {
            /* calculate medians */
            int[] reactTimesCorrect = new int[totalCorrect];
            int[] reactTimesIncorrect = new int[totalCompleted - totalCorrect];

            int rtcIndex = 0, rtiIndex = 0;
            for (int i = 0; i < totalCompleted; i++) {
                if (trials[i].correct)
                    reactTimesCorrect[rtcIndex++] = trials[i].reactTime;
                else
                    reactTimesIncorrect[rtiIndex++] = trials[i].reactTime;
            }

            this.mrtCorrect = CogTest.findMedian(ref reactTimesCorrect);
            this.mrtIncorrect = CogTest.findMedian(ref reactTimesIncorrect);

            saveResultsFile();
        }

        // for debugging purposes only - prints information in a message box on screen
        private void displayResults() {
            // print all information
            string display = "--------- ALL TRIALS ----------" + Environment.NewLine;
            for (int i = 0; i < totalCompleted; i++) {
                display += "trial " + trials[i].trialNum.ToString() + ": time = " + trials[i].reactTime.ToString() +
                    " (ms), correct = " + trials[i].correct.ToString() + Environment.NewLine;
            }
            MessageBox.Show(display);

            // print summary
            display = "----------- SUMMARY ------------" + Environment.NewLine;
            display += "total correct = " + totalCorrect.ToString() + Environment.NewLine;
            display += "total completed = " + totalCompleted.ToString() + Environment.NewLine;
            display += "median RT correct = " + mrtCorrect.ToString() + Environment.NewLine;
            display += "median RT incorrect = " + mrtIncorrect.ToString() + Environment.NewLine;
            display += "total RT correct = " + trtCorrect.ToString() + Environment.NewLine;
            display += "total RT incorrect = " + trtIncorrect.ToString() + Environment.NewLine;

            MessageBox.Show(display);
        }

        // write results to a text file
        private void saveResultsFile() {
            /* ----- write all of the raw trial data to a new file -------- */
            string rawFilename = "subj" + CogTest.uid + "speed.txt";
            System.IO.StreamWriter rawFile = new System.IO.StreamWriter(rawFilename);
            for (int i = 0; i < totalCompleted; i++) {
                string line = CogTest.uid + ',' + trials[i].trialNum.ToString() + ',' +
                    trials[i].reactTime.ToString() + ',' + CogTest.boolToChar(trials[i].correct);
                rawFile.WriteLine(line);
            }
            rawFile.Close();

            /* ----- append the summary data to speedSummary.txt -------- */
            System.IO.StreamWriter summaryFile = new System.IO.StreamWriter("speedSummary.txt", true);
            string summaryLine = CogTest.uid + ',' + totalCorrect.ToString() + ',' +
                totalCompleted.ToString() + ',' + mrtCorrect.ToString() + ',' + mrtIncorrect.ToString() + ',' +
                trtCorrect.ToString() + ',' + trtIncorrect.ToString();
            summaryFile.WriteLine(summaryLine);
            summaryFile.Close();
        }

        #endregion
    }
}
