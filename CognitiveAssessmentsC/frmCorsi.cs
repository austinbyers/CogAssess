using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CognitiveAssessmentsC
{
    public partial class frmCorsi : Form
    {
        // global button array
        Button[] boxes = new Button[9];

        // global instance of the current test
        CorsiTest corsi;

        ArrayList answerSeq;     // correct sequence of button indexes 
        int curIndex;           // current index, either of highlight sequence or entered sequence
        bool correct;           // is their answer correct so far?

        int testStatus;         // TEST_DEMO, TEST_PRACTICE, or TEST_OFFICIAL

        #region Form Setup

        // constructor
        public frmCorsi(int testStatus)
        {
            InitializeComponent();

            // Add global event handlers
            this.SizeChanged += new EventHandler(CogTest.FormSizeChanged);
            this.KeyDown += new KeyEventHandler(CogTest.KeyDown);
            this.FormClosing += new FormClosingEventHandler(CogTest.FormClosing);

            /* each button event uses the same handler for all buttons */

            btn0.Click += new EventHandler(btn_Click);
            btn1.Click += new EventHandler(btn_Click);
            btn2.Click += new EventHandler(btn_Click);
            btn3.Click += new EventHandler(btn_Click);
            btn4.Click += new EventHandler(btn_Click);
            btn5.Click += new EventHandler(btn_Click);
            btn6.Click += new EventHandler(btn_Click);
            btn7.Click += new EventHandler(btn_Click);
            btn8.Click += new EventHandler(btn_Click);

            btn0.MouseDown += new MouseEventHandler(btn_MouseDown);
            btn1.MouseDown += new MouseEventHandler(btn_MouseDown);
            btn2.MouseDown += new MouseEventHandler(btn_MouseDown);
            btn3.MouseDown += new MouseEventHandler(btn_MouseDown);
            btn4.MouseDown += new MouseEventHandler(btn_MouseDown);
            btn5.MouseDown += new MouseEventHandler(btn_MouseDown);
            btn6.MouseDown += new MouseEventHandler(btn_MouseDown);
            btn7.MouseDown += new MouseEventHandler(btn_MouseDown);
            btn8.MouseDown += new MouseEventHandler(btn_MouseDown);

            btn0.MouseUp += new MouseEventHandler(btn_MouseUp);
            btn1.MouseUp += new MouseEventHandler(btn_MouseUp);
            btn2.MouseUp += new MouseEventHandler(btn_MouseUp);
            btn3.MouseUp += new MouseEventHandler(btn_MouseUp);
            btn4.MouseUp += new MouseEventHandler(btn_MouseUp);
            btn5.MouseUp += new MouseEventHandler(btn_MouseUp);
            btn6.MouseUp += new MouseEventHandler(btn_MouseUp);
            btn7.MouseUp += new MouseEventHandler(btn_MouseUp);
            btn8.MouseUp += new MouseEventHandler(btn_MouseUp);

            this.KeyPreview = true;

            this.testStatus = testStatus;
        }

        private void frmCorsi_Load(object sender, EventArgs e) {
            boxes[0] = btn0;
            boxes[1] = btn1;
            boxes[2] = btn2;
            boxes[3] = btn3;
            boxes[4] = btn4;
            boxes[5] = btn5;
            boxes[6] = btn6;
            boxes[7] = btn7;
            boxes[8] = btn8;

            CogTest.setControlVisibility(this, false);
            setupForm();
            if (testStatus == CogTest.TEST_DEMO) {
                // show demo screen
                this.BackColor = CogTest.DEMO_COLOR;
                CogTest.enableDoubleEnterAdvance(this);
                CogTest.setControlVisibility(this, true);
                btnSubmit.Visible = false;
                panel.Focus();
            } else {
                // practice test has 3 trials, official test has 15
                corsi = (this.testStatus == CogTest.TEST_PRACTICE) ? 
                    new CorsiTest(3, true, ref answerSeq) : new CorsiTest(15, false, ref answerSeq);

                CogTest.setControlVisibility(this, true);
                foreach (Button btn in boxes)
                    btn.Visible = true;
                setEnabledStatus(false);

                System.Threading.Thread.Sleep(1500);     // wait about 1.5 seconds before showing the first trial
                showNextTrial();
            }
        }

        private void setupForm() {
            /* what we want to preserve is the relative vertical and horizontal position of the box 
                leftRatio = pixels left of box / total panel width
                topRatio = pixels above box / total panel height */
            double[] leftRatio = new Double[9];
            double[] topRatio = new Double[9];

            /* These ratios were calculated based on the Corsi image in the program specification
               For more information, see the comments in the VB prototype */

            leftRatio[0] = 0.1111;
            leftRatio[1] = 0.5053;
            leftRatio[2] = 0.7857;
            leftRatio[3] = 0.1958;
            leftRatio[4] = 0.5661;
            leftRatio[5] = 0.8254;
            leftRatio[6] = 0.0317;
            leftRatio[7] = 0.4788;
            leftRatio[8] = 0.3148;

            topRatio[0] = 0.0754;
            topRatio[1] = 0.1279;
            topRatio[2] = 0.1869;
            topRatio[3] = 0.3377;
            topRatio[4] = 0.3902;
            topRatio[5] = 0.4787;
            topRatio[6] = 0.5869;
            topRatio[7] = 0.6689;
            topRatio[8] = 0.7967;

            // Set size of panel
            int PANEL_WIDTH = Math.Min((int) (ClientSize.Width * .75), 760);
            int PANEL_HEIGHT = (int) (PANEL_WIDTH * .6776); //515 max width;
            panel.Width = PANEL_WIDTH;
            panel.Height = PANEL_HEIGHT;

            // Set size of submit button
            const int SUBMIT_WIDTH = 145;
            const int SUBMIT_HEIGHT = 60;
            btnSubmit.Width = SUBMIT_WIDTH;
            btnSubmit.Height = SUBMIT_HEIGHT;
            btnSubmit.Font = new Font("Microsoft Sans Serif", 25);

            /* Center the panel and submit button */

            // vertical distance between the panel and the submit button beneath it
            const int SUBMIT_PANEL_DIST = 35;

            panel.Left = (ClientSize.Width - PANEL_WIDTH) / 2;
            panel.Top = Math.Max(10, (ClientSize.Height - (PANEL_HEIGHT + SUBMIT_PANEL_DIST + SUBMIT_HEIGHT)) / 2);

            btnSubmit.Left = (ClientSize.Width - SUBMIT_WIDTH) / 2;
            btnSubmit.Top = Math.Min(ClientSize.Height - btnSubmit.Height - 5, panel.Bottom + SUBMIT_PANEL_DIST);

            // Position each of the blocks
            const int BOX_SIZE = 70;
            for (int i = 0; i < 9; i++) {
                boxes[i].Height = BOX_SIZE;
                boxes[i].Width = BOX_SIZE;
                boxes[i].Left = (int)(panel.Width * leftRatio[i]);
                boxes[i].Top = (int)(panel.Height * topRatio[i]);
            }
        }

        #endregion

        #region Button Events

        private void btn_Click(object sender, EventArgs e) {
            if (this.testStatus != CogTest.TEST_DEMO) {
                Button btn = (Button)sender;

                // if too many buttons were clicked or the wrong button was clicked, set 'correct' to false
                if (curIndex >= answerSeq.Count || (int)answerSeq[curIndex++] != Convert.ToInt32(btn.Tag))
                    correct = false;
            }
        }

        private void btn_MouseDown(object sender, EventArgs e) {
            Button btn = (Button)sender;
            btn.BackColor = Color.Green;
        }

        private void btn_MouseUp(object sender, EventArgs e) {
            Button btn = (Button)sender;
            btn.BackColor = Color.White;
            btnSubmit.Focus();
        }

        private void btnSubmit_Click(object sender, EventArgs e) {
            // at this point, the only way they could be wrong that we didn't catch in btnClick
            // is if they didn't finish the sequence

            // don't do anything if we are in demo mode
            if (this.testStatus == CogTest.TEST_DEMO) return;

            if (curIndex < answerSeq.Count)
                correct = false;

            setEnabledStatus(false);

            if (corsi.nextTrial(correct, ref answerSeq) == true) {
                // there is another trial - run it
                System.Threading.Thread.Sleep(500);     // wait about 500 ms between trials
                showNextTrial();
            } else {
                // end of test
                if (this.testStatus == CogTest.TEST_OFFICIAL)
                    corsi.finishTest();
                CogTest.closeForm(this);
            }
        }

        #endregion

        #region Sequence Highlighting

        // enable or disable everything on the form
        private void setEnabledStatus(bool enabled) {
            Color backColor = (enabled == true) ? Color.White : Color.WhiteSmoke;

            btnSubmit.Visible = enabled;
            foreach (Button btn in boxes) {
                btn.BackColor = backColor;
                btn.Enabled = enabled;
            }
        }

        // show the next trial
        private void showNextTrial() {
            curIndex = 0;
            correct = true;
            highlightBoxes();
        }

        // highlight next button blue for 1 second
        private void highlightBoxes() {
            boxes[(int)answerSeq[curIndex]].BackColor = Color.Blue;
            highlightTimer.Start();
        }

        // after 1 second, highlight the next box
        // if there are no boxes left to highlight, re-enable the form
        private void highlightTimer_Tick(object sender, EventArgs e) {
            highlightTimer.Stop();
            boxes[(int)answerSeq[curIndex]].BackColor = Color.WhiteSmoke;
            curIndex++;

            if (curIndex != answerSeq.Count)
                highlightBoxes();
            else {
                curIndex = 0;
                setEnabledStatus(true);
                btnSubmit.Focus();
            }
        }

        #endregion
    }
}
