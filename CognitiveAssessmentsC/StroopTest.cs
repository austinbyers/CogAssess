using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CognitiveAssessmentsC
{
    class StroopTest
    {
        public struct Block
        {
            public int block_number;
            public Trial[] trials;
        }

        public struct Trial
        {
            public int trial_number;
            public bool congruent;
            public bool correct;
            public int reaction_time;      // measured in ms
            public int length;             // character length (2-5)
            public string display;         // text displayed on screen
        }

        public Block[] blocks; 
        public int num_blocks;
        public int num_trials;         // number of trials per block
        public string display;        // current display     

        /* private variables */
        private int cbi;            // current block index
        private int cti;            // current trial index
        private Random rand;        // random number generator

        /****** Test Summary Data *******
         *      Abbreviations used:
         * cor = correct, incor = incorrect
         * cong = congruent, incong = incongruent
         * mrt = median reaction time
         * trt = total reaction time */
        public int corCong;
        public int corIncong;
        public double mrtCorCong;
        public double mrtCorIncong;
        public double mrtIncorCong;
        public double mrtIncorIncong;
        public int trtCorCong;
        public int trtCorIncong;
        public int trtIncorCong;
        public int trtIncorIncong;

        # region Test Creation

        // constructor
        public StroopTest(int num_blocks, int num_trials)
        {
            this.rand = new Random();
            this.num_blocks = num_blocks;
            this.num_trials = num_trials;
            this.blocks = createBlocks(num_blocks, num_trials);
            this.cbi = 0;
            this.cti = 0;
            this.display = blocks[0].trials[0].display;
        }

        private Block[] createBlocks(int num_blocks, int num_trials)
        {
            Block[] result = new Block[num_blocks];
            for (int i = 0; i < num_blocks; i++)
            {
                result[i].block_number = i + 1;
                result[i].trials = createTrials(num_trials);
            }
            return result;
        }

        private Trial[] createTrials(int num_trials)
        {
            Trial[] result = new Trial[num_trials];
            int num_repeats = num_trials / 8;

            // set value of display first
            int index = 0;
            for (int i = 1; i <= num_repeats; i++)
            {
                // congruent trials
                result[index].display = "22"; result[index++].congruent = true;
                result[index].display = "333"; result[index++].congruent = true;
                result[index].display = "4444"; result[index++].congruent = true;
                result[index].display = "55555"; result[index++].congruent = true;

                // incongruent trials
                result[index].display = incongruent(2); result[index++].congruent = false;
                result[index].display = incongruent(3); result[index++].congruent = false;
                result[index].display = incongruent(4); result[index++].congruent = false;
                result[index].display = incongruent(5); result[index++].congruent = false;
            }

            randomizeTrials(ref result);

            // set other fields of the Trial struct
            for (int i = 0; i < num_trials; i++)
            {
                result[i].trial_number = i + 1;
                result[i].correct = false;
                result[i].reaction_time = 0;
                result[i].length = result[i].display.Length;
            }

            return result;
        }

        // generate an incongruent string of the given length
        private string incongruent(int length)
        {
            string result = "";

            GetDigit:
                int digit = rand.Next(2, 6);
                if (digit == length) goto GetDigit;

            for (int i = 1; i <= length; i++)
                result += digit.ToString();

            return result;
        }

        // randomize an array of trials using the Knuth-Fisher-Yates shuffle algorithm
        private void randomizeTrials(ref Trial[] trials)
        {
            for (int i = trials.Length - 1; i > 0; i--)
            {
                int n = rand.Next(i + 1);

                // Swap trials[i] with trials[n]
                Trial temp = trials[i];
                trials[i] = trials[n];
                trials[n] = temp;
            }
        }

        #endregion

        #region Input Processing

        // process the input for the current trial
        // if there is another trial remaining in the current block,
        // set the display and return true. Otherwise, return false
        public bool processTrial(int input, int reaction_time)
        {
            // blocks[cbi].trials[cti] is the current trial

            blocks[cbi].trials[cti].reaction_time = reaction_time;

            if (input == blocks[cbi].trials[cti].display.Length) {
                // correct answer
                blocks[cbi].trials[cti].correct = true;

                if (blocks[cbi].trials[cti].congruent == true) {
                    this.corCong++;
                    this.trtCorCong += reaction_time;
                } else {
                    this.corIncong++;
                    this.trtCorIncong += reaction_time;
                }
            } else {
                // incorrect answer

                if (blocks[cbi].trials[cti].congruent == true)
                    this.trtIncorCong += reaction_time;
                else
                    this.trtIncorIncong += reaction_time;
            }

            // if this is the last trial in the block, return false
            if (blocks[cbi].trials[cti].trial_number == num_trials) 
                return false;

            // otherwise, advance to the next trial
            cti++;
            this.display = blocks[cbi].trials[cti].display;

            return true;
        }

        // if there is another block in the test, 
        // update the private indexes and set the next string to display
        public bool processBlock()
        {
            cbi++;
            cti = 0;

            if (cbi == num_blocks)
                return false;

            this.display = blocks[cbi].trials[0].display;

            return true;
        }

        #endregion

        #region Test Completion

        // finishes the current test by calling appropriate methods
        // to calculate and save the data
        public void finish() {
            calculateMedians();
            saveResultsFile();
        }

        private void calculateMedians() {
            int total_trials = num_blocks * num_trials;

            // create new arrays for each of the statistics
            int[] corCongTimes = new int[corCong];
            int[] corIncongTimes = new int[corIncong];
            int[] incorCongTimes = new int[total_trials / 2 - corCong];
            int[] incorIncongTimes = new int[total_trials / 2 - corIncong];

            int corCongIndex = 0, corIncongIndex = 0, incorCongIndex = 0, incorIncongIndex = 0;

            // fill each array with data from the appropriate trial
            for (int b = 0; b < num_blocks; b++) {
                for (int t = 0; t < num_trials; t++) {
                    Trial trl = blocks[b].trials[t];
                    if (trl.correct)
                        if (trl.congruent)
                            corCongTimes[corCongIndex++] = trl.reaction_time;
                        else
                            corIncongTimes[corIncongIndex++] = trl.reaction_time;
                    else
                        if (trl.congruent)
                            incorCongTimes[incorCongIndex++] = trl.reaction_time;
                        else
                            incorIncongTimes[incorIncongIndex++] = trl.reaction_time;
                }
            }

            // calculate the median
            this.mrtCorCong = CogTest.findMedian(ref corCongTimes);
            this.mrtCorIncong = CogTest.findMedian(ref corIncongTimes);
            this.mrtIncorCong = CogTest.findMedian(ref incorCongTimes);
            this.mrtIncorIncong = CogTest.findMedian(ref incorIncongTimes);
        }

        // NOTE: use this only for debugging purposes
        // prints the results in a message box on screen
        private void printResults() {
            // all information
            for (int b = 0; b < num_blocks; b++) {
                string results = "------ Block " + blocks[b].block_number.ToString() + " ------" + Environment.NewLine;
                for (int t = 0; t < num_trials; t++) {
                    Trial trl = blocks[b].trials[t];    // note that this is a read-only declaration
                    results += "Trial " + trl.trial_number + ": congruent = " + trl.congruent.ToString() +
                        ", correct = " + trl.correct.ToString() + ", time = " + trl.reaction_time.ToString() +
                        " ms, char-len = " + trl.length.ToString() + ", display = " + trl.display + Environment.NewLine;
                }
                MessageBox.Show(results);
            }

            // summary information
            string summary = "------ SUMMARY ------" + Environment.NewLine;
            summary += "Correct Congruent = " + corCong + Environment.NewLine;
            summary += "Correct Incongruent = " + corIncong + Environment.NewLine + Environment.NewLine;

            summary += "Median RT Correct Congruent = " + mrtCorCong + " ms " + Environment.NewLine;
            summary += "Median RT Correct Incongruent = " + mrtCorIncong + " ms " + Environment.NewLine;
            summary += "Median RT Incorrect Congruent = " + mrtIncorCong + " ms " + Environment.NewLine;
            summary += "Median RT Incorrect Incongruent = " + mrtIncorIncong + " ms " + Environment.NewLine + Environment.NewLine;

            summary += "Total RT Correct Congruent = " + trtCorCong + " ms " + Environment.NewLine;
            summary += "Total RT Correct Incongruent = " + trtCorIncong + " ms " + Environment.NewLine;
            summary += "Total RT Incorrect Congruent = " + trtIncorCong + " ms " + Environment.NewLine;
            summary += "Total RT Incorrect Incongruent = " + trtIncorIncong + " ms " + Environment.NewLine;

            MessageBox.Show(summary);
        }

        // write results to a text file
        private void saveResultsFile() {
            /* ----- write all of the raw trial data to a new file -------- */
            string rawFilename = "subj" + CogTest.uid + "stroop.txt";
            System.IO.StreamWriter rawFile = new System.IO.StreamWriter(rawFilename);

            for (int b = 0; b < num_blocks; b++) {
                for (int t = 0; t < num_trials; t++) {
                    Trial trl = blocks[b].trials[t];    // note that this is a read-only declaration
                    string line = CogTest.uid + ',' +
                        blocks[b].block_number.ToString() + ',' + trl.trial_number.ToString() + ',' +
                        CogTest.boolToChar(trl.congruent) + ',' + CogTest.boolToChar(trl.correct) + ',' +
                        trl.reaction_time.ToString() + ',' + trl.length.ToString() + ',' + trl.display;
                    rawFile.WriteLine(line);
                }
            }
            rawFile.Close();

            /* ----- append the summary data to stroopSummary.txt -------- */
            System.IO.StreamWriter summaryFile = new System.IO.StreamWriter("stroopSummary.txt", true);
            string summaryLine = CogTest.uid + ',' + corCong.ToString() + ',' +
                corIncong.ToString() + ',' + mrtCorCong.ToString() + ',' + mrtCorIncong.ToString() + ',' +
                mrtIncorCong.ToString() + ',' + mrtIncorIncong.ToString() + ',' + trtCorCong.ToString() + ',' +
                trtCorIncong.ToString() + ',' + trtIncorCong.ToString() + ',' + trtIncorIncong.ToString();
            summaryFile.WriteLine(summaryLine);
            summaryFile.Close();
        }

        #endregion
    }
}
