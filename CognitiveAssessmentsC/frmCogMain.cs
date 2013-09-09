using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CognitiveAssessmentsC
{
    public partial class frmCogMain : Form
    {
        bool userClosedForm = true;

        // the job of form main is only to get the user ID
        public frmCogMain() {
            InitializeComponent();
            this.Shown += new EventHandler(frmMain_Shown);
            this.FormClosing += new FormClosingEventHandler(frmMain_FormClosing);
            btnStart.Click += new EventHandler(btnStart_Click);
        }

        void frmMain_Shown(object sender, EventArgs e) {
            txtBoxID.Focus();
        }

        void btnStart_Click(object sender, EventArgs e) {
            StartTests();
        }

        private void StartTests() {
            string uid = txtBoxID.Text.Trim();
            string corsi = "subj" + uid + "corsi.txt";
            string raven = "subj" + uid + "raven.txt";
            string speed = "subj" + uid + "speed.txt";
            string stroop = "subj" + uid + "stroop.txt";

            if (uid == "") {
                MessageBox.Show("You cannot leave the userID field blank.", "Cognitive Assessments - Missing user ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBoxID.Focus();
            } else if (File.Exists(corsi) || File.Exists(raven) || File.Exists(speed) || File.Exists(stroop)) {
                MessageBox.Show("User '" + uid + "' already has test data. Please enter a unique user ID.", 
                    "Cognitive Assessments - Invalid user ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBoxID.Focus();
            } else {
                CogTest.uid = uid;
                userClosedForm = false;
                this.Close();
            }
        }

        private void btnHelp_Click(object sender, EventArgs e) {
            string nl = Environment.NewLine;
            string msg = "------------------ Individual Test Files --------------------" + nl +
                "'subj[ID]corsi.txt' - ID, span length, correct/incorrect [1/0]" + nl +
                "'subj[ID]raven.txt' - ID, # correct, # not completed" + nl +
                "'subj[ID]speed.txt' - ID, trial number, reaction time (ms)," + nl +
                "    correct/incorrect [1/0]" + nl +
                "'subj[ID]stroop.txt' - ID, block number, trial number," + nl +
                "    congruent/incongruent [1/0], correct/incorrect [1/0], " + nl +
                "    reaction time (in ms), display length, numbers displayed" + nl + nl +

                "---------- Test Summary Files (one line per user) ------------" + nl +
                "'corsiSummary.txt' - ID, # correct span-3, # correct span-4," + nl +
                "    # correct span-5, # correct span-6, # correct span-7," + nl +
                "'ravenSummary.txt' - ID, # correct, # not completed" + nl +
                "'speedSummary.txt' - ID, # correct, # completed, MRT correct," + nl +
                "    MRT incorrect, TRT correct, TRT incorrect" + nl +
                "'stroopSummary.txt' - ID, # correct congruent, # correct" + nl +
                "    incongruent, median reaction time (MRT) for correct congruent," + nl +
                "    MRT correct incongruent, MRT incorrect congruent, MRT incorrect" + nl +
                "    incongruent, total reaction time (TRT) correct congruent," + nl +
                "    TRT correct incongruent, TRT incorrect congruent," + nl +
                "    TRT incorrect incongruent";
            MessageBox.Show(msg, "Data File Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
            // if the user closed the form, quit the entire program
            if (userClosedForm == true)
                Environment.Exit(1);
        }
    }
}
