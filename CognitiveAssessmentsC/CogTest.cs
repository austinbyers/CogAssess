/* =========== CogAssess class - Austin Byers, 2013 ============
 * This is the main interface for the cognitive tests -
 * Data and methods that are common to all of the tests live in this class.
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CognitiveAssessmentsC
{
    static class CogTest
    {
        public static string uid = "";                       // userID used by all tests
        public static Color DEMO_COLOR = Color.FromArgb(20, 0, 0);    // background color for all demo screens

        // Each of the tests have 3 different types:
        public const int TEST_DEMO = 0;
        public const int TEST_PRACTICE = 1;
        public const int TEST_OFFICIAL = 2;

        private static bool wasMinimized = false;       // was the active form just minimized?
        private static bool ignoreKeypress = false;     // whether to ignore the current keypress or not

        private static Stopwatch enterStopwatch = new Stopwatch(); // stopwatch for the time between enter keys
        private const int DOUBLE_ENTER_INTVL = 500;     // amount of time between double-enter in order to advance
        private static Form currentSplashForm;            // the current form being displayed as a splash screen

        private static frmCogBase baseForm;                // pointer to the base form

        /// <summary>
        /// The main method to run through all of the cognitive tests
        /// </summary>
        public static void Run() {
            // run main form to get uid

            Application.Run(new frmCogMain());

            // start tests
            baseForm = new frmCogBase();
            Application.Run(baseForm);
        }

        /// <summary>
        /// Closes form after a double tap on the enter key.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                if (enterStopwatch.IsRunning && enterStopwatch.ElapsedMilliseconds <= DOUBLE_ENTER_INTVL) {
                    // successful double-tap on the enter key - close form
                    CogTest.closeForm(currentSplashForm);
                } else {
                    // first enter key pressed - start stopwatch to wait for second press
                    enterStopwatch.Reset();
                    enterStopwatch.Start();
                }
            }
        }

        /// <summary>
        /// KeyDown event to handle ctrl+m and ctrl+q. Also used to ignore numpad keys
        /// </summary>
        public static void KeyDown(object sender, KeyEventArgs e) {
            Form form = (Form)sender;
            if (Control.ModifierKeys == Keys.Control && e.KeyCode == Keys.M) {
                // ctrl+m - minimize current form and hide the other forms
                baseForm.Visible = false;
                form.WindowState = FormWindowState.Minimized;
                wasMinimized = true;
            } else if (Control.ModifierKeys == Keys.Control && e.KeyCode == Keys.Q) {
                // ctrl+q - quit
                form.DialogResult = DialogResult.Cancel;
                form.Close();
            } else if (Keys.NumPad0 <= e.KeyCode && e.KeyCode <= Keys.NumPad9) {
                // ignore numpad
                ignoreKeypress = true;
            }
        }

        /// <summary>
        /// Check if an illegal key press was detected by the CogTest.KeyDown event
        /// </summary>
        public static bool ValidateKeyPress() {
            bool isValid = true;
            if (ignoreKeypress == true) {
                isValid = false;
                ignoreKeypress = false;
            }
            return isValid;
        }

        /// <summary>
        /// FormSizeChanged event to re-maximize the form after minimization
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void FormSizeChanged(object sender, EventArgs e) {
            if (wasMinimized == true) {
                // re-maximize the form - show the forms we hid earlier
                Form form = (Form)sender;
                baseForm.Visible = true;
                form.Visible = true;
                wasMinimized = false;
            }
        }

        // set the visibility of every control on a form
        public static void setControlVisibility(Form owner, bool visible) {
            foreach (Control c in owner.Controls) {
                c.Visible = visible;
            }
        }

        // not actually used right now
        public static void handleIOError(string filename, string message) {
            MessageBox.Show("Error writing data file '" + filename + "' - " + message + ". Data for this test is probably corrupted.", 
                "File IO Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Allow a form to be closed with a double-tap on the enter key. Adds a KeyUp handler to the target form.
        /// </summary>
        /// <param name="target">form to be changed</param>
        public static void enableDoubleEnterAdvance(Form target) {
            currentSplashForm = target;
            target.KeyUp += new KeyEventHandler(CogTest.KeyUp);
        }

        /// <summary>
        /// Displays a warning dialog box if the user triggered the FormClosing event.
        /// To close the form normally, use CogTest.closeForm()
        /// </summary>
        public static void FormClosing(object sender, FormClosingEventArgs e) {
            Form form = (Form)sender;
            enterStopwatch.Reset();
            if (form.DialogResult == DialogResult.Cancel) {
                DialogResult result = MessageBox.Show("Are you sure you want to quit the program early?" +
                Environment.NewLine + "The data for each of the completed tests has already been saved," +
                Environment.NewLine + "but all of the data for the current test will be lost.",
                "Cognitive Assessments - Quit Program", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Yes){
                    // we have to sleep for a second before closing or Environment.Exit throws the following error:
                    // "Cannot execute operation because current thread is in a spin, wait, or sleep operation
                    CogTest.closeForm(form);
                    Environment.Exit(1);
                } else {
                    e.Cancel = true;
                }
            }
        }

        // close form without triggering the message box in the formClosing event
        public static void closeForm(Form form) {
            form.DialogResult = DialogResult.OK;
            form.Close();
            form.Dispose();
        }

        // return '1' for true, '0' for false
        // used for writing booleans to a CSV file
        public static char boolToChar(bool arg) {
            return (arg == true) ? '1' : '0';
        }

        // find the median element of an array
        public static double findMedian(ref int[] array) {
            Array.Sort(array);
            int mid = array.Length / 2;
            if (array.Length == 0)
                return 0;
            else if (array.Length % 2 == 0)  // even number of elements
                return (double)(array[mid - 1] + array[mid]) / 2;
            else
                return (double)array[mid];
        }

        // returns a list of 'length' unique random numbers between 0 and 'max' (inclusive)
        public static ArrayList createRandomList(int min, int max, int length, ref Random rand) {
            ArrayList result = new ArrayList(length);

            for (int i = 0; i < length; i++) {
            GenerateIndex:
                int num = rand.Next(min, max + 1);    // upper bound is exclusive for rand.Next()
                if (result.Contains(num)) goto GenerateIndex;
                result.Add(num);
            }

            return result;
        }

        /* note that the randomizeTrials() function appears in both Corsi and Stroop
         * However, the method is acting on an array of a different type
         * 
         * It is possible to create a generic method that randomizes an array of objects, 
         * but it would require explicitly creating an array of Objects to be shuffled and
         * re-creating a result array of the target type.
         * 
         * Since it is only used twice, it's easier to just copy the code */
    }
}
