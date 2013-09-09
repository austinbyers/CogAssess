using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CognitiveAssessmentsC
{
    class CorsiTest
    {
        public struct Trial
        {
            public int spanLength;
            public bool correct;
        }

        /* Global array of trials */
        public Trial[] trials;
        public int numTrials;
        private int curTrialIndex;      // index of the current trial


        private Random rand;            // random number generator

        /* Test Summary Information */
        public int span3, span4, span5, span6, span7;   // number correct for a span of 3, 4, 5, etc

        #region Test Creation

        public CorsiTest(int numTrials, bool isPractice, ref ArrayList nextIndexes) {
            this.rand = new Random();

            this.numTrials = numTrials;
            this.trials = createTrials(numTrials, isPractice);
            this.span3 = 0; this.span4 = 0; this.span5 = 0;
            this.span6 = 0; this.span7 = 0;
            this.curTrialIndex = 0;

            nextIndexes = CogTest.createRandomList(0, 8, trials[0].spanLength, ref rand);
        }

        private Trial[] createTrials(int numTrials, bool practice) {
            Trial[] result = new Trial[numTrials];

            if (practice == true) {
                int index = 0;
                // create a set of practice trials - 1 trial each of span 3, 5, and 7
                for (int span = 3; span <= 7; span += 2) {
                    result[index].spanLength = span;
                    result[index++].correct = false;
                }
            } else {
                // create the trials for the official test
                int numRepeats = numTrials / 5;
                int index = 0;
                for (int i = 1; i <= numRepeats; i++) {
                    for (int span = 3; span <= 7; span++) {
                        result[index].spanLength = span;
                        result[index++].correct = false;
                    }
                }
            }

            randomizeTrials(ref result);
            return result;
        }

        // randomize an array of trials using the Knuth-Fisher-Yates shuffle algorithm
        private void randomizeTrials(ref Trial[] trials) {
            for (int i = trials.Length - 1; i > 0; i--) {
                int n = rand.Next(i + 1);

                // Swap trials[i] with trials[n]
                Trial temp = trials[i];
                trials[i] = trials[n];
                trials[n] = temp;
            }
        }

        #endregion

        // process input and return true if there are more trials
        public bool nextTrial(bool correct, ref ArrayList nextIndexes) {
            trials[curTrialIndex].correct = correct;

            // update global counter for appropriate span length
            if (correct == true) {
                switch (trials[curTrialIndex].spanLength) {
                    case 3:
                        span3++;
                        break;
                    case 4:
                        span4++;
                        break;
                    case 5:
                        span5++;
                        break;
                    case 6:
                        span6++;
                        break;
                    case 7:
                        span7++;
                        break;
                }
            }

            curTrialIndex++;

            if (curTrialIndex == numTrials) {
                // no trials left - end of the current test
                return false;
            } else {
                nextIndexes = CogTest.createRandomList(0, 8, trials[curTrialIndex].spanLength, ref rand);
                return true;
            }
        }

        #region Test Completion

        public void finishTest() {
            saveResultsFile();
        }

        // print test results in message box (for debugging purposes only)
        private void displayResults() {
            string result = "------- All Trials -------" + Environment.NewLine;
            for (int i = 0; i < numTrials; i++) {
                result += "span length = " + trials[i].spanLength.ToString() +
                    ", correct = " + trials[i].correct.ToString() + Environment.NewLine;
            }
            System.Windows.Forms.MessageBox.Show(result);

            result = "---------- Summary -----------" + Environment.NewLine;
            result += "correct for span 3 = " + span3.ToString() + Environment.NewLine;
            result += "correct for span 4 = " + span4.ToString() + Environment.NewLine;
            result += "correct for span 5 = " + span5.ToString() + Environment.NewLine;
            result += "correct for span 6 = " + span6.ToString() + Environment.NewLine;
            result += "correct for span 7 = " + span7.ToString() + Environment.NewLine;
            System.Windows.Forms.MessageBox.Show(result);
        }

        // write results to a text file
        private void saveResultsFile() {
            /* ----- write all of the raw trial data to a new file -------- */
            string rawFilename = "subj" + CogTest.uid + "corsi.txt";
            System.IO.StreamWriter rawFile = new System.IO.StreamWriter(rawFilename);

            for (int i = 0; i < numTrials; i++) {
                string line = CogTest.uid + ',' + trials[i].spanLength.ToString() + ',' +
                    CogTest.boolToChar(trials[i].correct);
                rawFile.WriteLine(line);
            }
            rawFile.Close();

            /* ----- append the summary data to corsiSummary.txt -------- */
            System.IO.StreamWriter summaryFile = new System.IO.StreamWriter("corsiSummary.txt", true);
            string summaryLine = CogTest.uid + ',' + span3.ToString() + ',' + span4.ToString() + ',' +
                span5.ToString() + ',' + span6.ToString() + ',' + span7.ToString();
            summaryFile.WriteLine(summaryLine);
            summaryFile.Close();
        }

        #endregion
    }
}
