/* ======== frmCogBase =========
 * The actual test forms are called by this base form.
 * The form is just a black screen behind all of the tests, so that the screen
 * (hopefully) doesn't flash when switching between tests
 *  */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CognitiveAssessmentsC
{
    public partial class frmCogBase : Form
    {
        Random rand;    // random number generator

        public frmCogBase() {
            InitializeComponent();
            this.Shown += new EventHandler(frmBase_Shown);
        }

        private void frmBase_Load(object sender, EventArgs e) {
            rand = new Random();
        }

        /* ========== This is the top level execution sequence =========*/
        void frmBase_Shown(object sender, EventArgs e) {
            // randomize the test ordering
            System.Collections.ArrayList order = CogTest.createRandomList(1, 4, 4, ref rand);

            frmSplash.mainSplash("Keyboard Training", this);
            frmKeyboardTutorial keyTut = new frmKeyboardTutorial();
            keyTut.ShowDialog(this);

            dispatch((int)order[0]);
            frmSplash.breakSplash(this);
            dispatch((int)order[1]);
            frmSplash.breakSplash(this);
            dispatch((int)order[2]);
            frmSplash.breakSplash(this);
            dispatch((int)order[3]);
            frmSplash.finalSplash(this);

            Environment.Exit(0);
        }

        private void dispatch(int tag) {
            switch (tag) {
                case 1:
                    stroop();
                    break;
                case 2:
                    speed();
                    break;
                case 3:
                    corsi();
                    break;
                case 4:
                    raven();
                    break;
            }
        }

        private void stroop() {
            frmSplash.mainSplash("Stroop", this);
            frmStroop demo = new frmStroop(CogTest.TEST_DEMO);
            demo.ShowDialog(this);

            frmSplash.practiceSplash("Stroop", this);
            frmStroop practice = new frmStroop(CogTest.TEST_PRACTICE);
            practice.ShowDialog(this);

            frmSplash.testSplash("Stroop", this);
            frmStroop official = new frmStroop(CogTest.TEST_OFFICIAL);
            official.ShowDialog(this);
        }

        private void speed() {
            frmSplash.mainSplash("Speed", this);
            frmSpeed demo = new frmSpeed(CogTest.TEST_DEMO);
            demo.ShowDialog(this);

            frmSplash.practiceSplash("Speed", this);
            frmSpeed practice = new frmSpeed(CogTest.TEST_PRACTICE);
            practice.ShowDialog(this);

            frmSplash.testSplash("Speed", this);
            frmSpeed official = new frmSpeed(CogTest.TEST_OFFICIAL);
            official.ShowDialog(this);
        }

        private void corsi() {
            frmSplash.mainSplash("Corsi", this);
            frmCorsi demo = new frmCorsi(CogTest.TEST_DEMO);
            demo.ShowDialog(this);

            frmSplash.practiceSplash("Corsi", this);
            frmCorsi practice = new frmCorsi(CogTest.TEST_PRACTICE);
            practice.ShowDialog(this);

            frmSplash.testSplash("Corsi", this);
            frmCorsi official = new frmCorsi(CogTest.TEST_OFFICIAL);
            official.ShowDialog(this);
        }

        private void raven() {
            frmSplash.mainSplash("Raven", this);
            frmRaven demo = new frmRaven(CogTest.TEST_DEMO);
            demo.ShowDialog(this);

            frmSplash.practiceSplash("Raven", this);
            frmRaven practice = new frmRaven(CogTest.TEST_PRACTICE);
            practice.ShowDialog(this);

            frmSplash.testSplash("Raven", this);
            frmRaven official = new frmRaven(CogTest.TEST_OFFICIAL);
            official.ShowDialog(this);
        }

    }
}
