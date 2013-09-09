using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CognitiveAssessmentsC
{
    public partial class frmStroop : Form
    {
        StroopTest curTest;         // global test object
        Font fntFixation;           // font to be displayed for the fixation cross (cross must be bigger than normal)
        Font fntNormal;              // font to be displayed otherwise
        Stopwatch stopwatch;        // stopwatch to time reactions (much more accurate than form timer)
        int break_time;             // amount of time left for the current break
        int testStatus;             // TEST_DEMO, TEST_PRACTICE, or TEST_OFFICIAL

        #region Form Setup

        /// <summary>
        /// frmStroop constructor
        /// </summary>
        /// <param name="testStatus">CogTest.TEST_DEMO, CogTest.TEST_PRACTICE, or CogTest.TEST_OFFICIAL</param>
        public frmStroop(int testStatus)
        {
            InitializeComponent();

            // add global event handlers
            this.SizeChanged += new EventHandler(CogTest.FormSizeChanged);
            this.FormClosing += new FormClosingEventHandler(CogTest.FormClosing);
            this.KeyDown += new KeyEventHandler(CogTest.KeyDown);

            this.KeyPress += new KeyPressEventHandler(frmStroop_KeyPress);

            this.fntFixation = new Font("Microsoft Sans Serif", 100);
            this.fntNormal = new Font("Microsoft Sans Serif", 75);
            this.stopwatch = new Stopwatch();
            this.testStatus = testStatus;
        }

        // load form - align elements and set up test
        private void frmStroop_Load(object sender, EventArgs e) {
            CogTest.setControlVisibility(this, false);
            if (this.testStatus == CogTest.TEST_DEMO) {
                // demo screen - show three different labels
                Point center = new Point(this.ClientSize.Width / 2, this.ClientSize.Height / 2);
                lblDisplay.Text = "55555";
                lblDisplay.Left = (ClientSize.Width - lblDisplay.Width) / 2;
                lblDisplay.Top = (ClientSize.Height - lblDisplay.Height) / 2 + 100;
                Label lblDemo1 = new Label();
                Label lblDemo2 = new Label();
                this.addLabel(lblDemo1, "3333", new Point(
                    (center.X - lblDisplay.Width) / 2, center.Y - lblDisplay.Height - 100));
                this.addLabel(lblDemo2, "444", new Point(
                    (center.X + lblDemo1.Left), center.Y - lblDisplay.Height - 100));
                this.BackColor = CogTest.DEMO_COLOR;
                CogTest.enableDoubleEnterAdvance(this);
                CogTest.setControlVisibility(this, true);
            } else {
                lblDisplay.Left = (ClientSize.Width - lblDisplay.Width) / 2;
                lblDisplay.Top = (ClientSize.Height - lblDisplay.Height) / 2;

                // practice test    - 1 block with 16 trials (8 congruent, 8 incongruent)
                // or official test - 4 blocks with 32 trials (16 congruent, 16 incongruent)
                curTest = (this.testStatus == CogTest.TEST_PRACTICE) ? new StroopTest(1, 16) : new StroopTest(4, 32);
                showFixation();
                CogTest.setControlVisibility(this, true);
            }
        }

        // add labels to the form - used for training screen
        private void addLabel(Label lbl, string text, Point location) {
            lbl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lbl.Font = fntNormal;
            lbl.ForeColor = System.Drawing.Color.White;
            lbl.Location = location;
            lbl.Size = new System.Drawing.Size(385, 163);
            lbl.Text = text;
            lbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Controls.Add(lbl);
        }

        #endregion

        // process a keypress during the test
        private void frmStroop_KeyPress(object sender, KeyPressEventArgs e) {
            // only allow non-numpad number keys to be pressed, and only while a test is running
            if (e.KeyChar >= 49 && e.KeyChar <= 57 && stopwatch.IsRunning && CogTest.ValidateKeyPress() == true) {
                stopwatch.Stop();
                int elapsedTime = (int)stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
                if (curTest.processTrial(e.KeyChar - 48, elapsedTime) == true) {
                    // there is another trial to run
                    showFixation();
                } else if (curTest.processBlock() == true) {
                    // advance to the next block after a 30 second break

                    lblDisplay.BorderStyle = BorderStyle.None;
                    lblDisplay.Text = "00:30";
                    lblDisplay.Font = fntNormal;

                    break_time = 30;
                    breakTimer.Interval = 1000;
                    breakTimer.Start();
                } else {
                    // we have reached the end of the current test

                    if (testStatus == CogTest.TEST_OFFICIAL) {
                        // save the data only if it is an official test
                        curTest.finish();
                    }

                    CogTest.closeForm(this);
                }
            }
        }

        #region Trial Display

        // show fixation cross before each trial
        private void showFixation() {
            lblDisplay.BorderStyle = BorderStyle.None;
            lblDisplay.Text = "+";
            lblDisplay.Font = fntFixation;

            fixationTimer.Start();
        }

        // fixation cross is done - resume test
        private void fixationTimer_Tick(object sender, EventArgs e) {
            fixationTimer.Stop();
            showTrial();
        }

        // update the display with the current test
        private void showTrial() {
            lblDisplay.BorderStyle = BorderStyle.FixedSingle;
            lblDisplay.Font = fntNormal;
            lblDisplay.Text = curTest.display;
            stopwatch.Start();
        }

        #endregion

        // update the break countdown every second
        private void breakTimer_Tick(object sender, EventArgs e) {
            // showing countdown timer
            break_time--;

            if (break_time == 0) {
                // break is over
                breakTimer.Stop();
                showFixation();
            } else
                lblDisplay.Text = "00:" + break_time.ToString().PadLeft(2, '0');
        }
    }
}
