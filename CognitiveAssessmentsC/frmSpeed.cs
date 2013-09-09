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
    public partial class frmSpeed : Form
    {
        SpeedTest test;                         // global instance of the current test
        Dictionary<string, string> dict;        // grid displayed at top
        string[] answers; string[,] options;    // arrays of 5 answers and 5 option lists to go along with each grid

        Stopwatch reactStopwatch = new Stopwatch();  // stopwatch to time reactions
        Stopwatch testStopwatch = new Stopwatch();   // stopwatch to accurately time 5 mins for the official test
        int trials;                             // number of trials so far on the current grid

        const int STOPWATCH_CHECK_INTVL = 1000; // how often (in ms) to check the test stopwatch
        const int PRACTICE_DUARTION = 30000;    // duration (in ms) for the practice test
        const int OFFICIAL_DURATION = 180000;   // duration (in ms) for the official test

        Label[] grid = new Label[10];
        Button[] btns = new Button[4];
        Label[] lblBtns = new Label[4];

        bool doublePress;       // ignore double click (or double keypress)

        int testStatus;         // TEST_DEMO, TEST_PRACTICE, or TEST_OFFICIAL

        #region Form Setup

        // constructor
        public frmSpeed(int testStatus)
        {
            InitializeComponent();

            // Add global event handlers
            this.SizeChanged += new EventHandler(CogTest.FormSizeChanged);
            this.KeyDown += new KeyEventHandler(CogTest.KeyDown);
            this.FormClosing += new FormClosingEventHandler(CogTest.FormClosing);

            this.btnA.Click += new EventHandler(button_Click);
            this.btnB.Click += new EventHandler(button_Click);
            this.btnC.Click += new EventHandler(button_Click);
            this.btnD.Click += new EventHandler(button_Click);
            this.KeyPress += new KeyPressEventHandler(frmSpeed_KeyPress);

            // This is very important - it allows the form to see all of the key events
            // before they reach the active control
            this.KeyPreview = true;
            
            test = new SpeedTest();
            this.testStatus = testStatus;
        }

        private void frmSpeed_Load(object sender, EventArgs e) {
            grid[0] = lbl0;
            grid[1] = lbl1;
            grid[2] = lbl2;
            grid[3] = lbl3;
            grid[4] = lbl4;
            grid[5] = lbl5;
            grid[6] = lbl6;
            grid[7] = lbl7;
            grid[8] = lbl8;
            grid[9] = lbl9;

            btns[0] = btnA;
            btns[1] = btnB;
            btns[2] = btnC;
            btns[3] = btnD;

            lblBtns[0] = lblBtnA;
            lblBtns[1] = lblBtnB;
            lblBtns[2] = lblBtnC;
            lblBtns[3] = lblBtnD;

            CogTest.setControlVisibility(this, false);

            alignElements();

            if (testStatus == CogTest.TEST_DEMO) {
                // demo screen - use what's in the designer
                this.BackColor = CogTest.DEMO_COLOR;
                CogTest.enableDoubleEnterAdvance(this);
                CogTest.setControlVisibility(this, true);
                lblQuestion.Focus();
            } else {
                // display the actual test
                newGrid();
                nextTrial();
                formTimer.Interval = STOPWATCH_CHECK_INTVL;
                CogTest.setControlVisibility(this, true);
                lblQuestion.Focus();
                formTimer.Start();
                testStopwatch.Start();
            }
        }

        // align all of the elements of the form
        private void alignElements() {

            /* ======== Align the top grid ========= */
            int LBL_WIDTH = Math.Min(272, ClientSize.Width / 5);
            const int LBL_HEIGHT = 60;
            int LBL_OFFSET = 5 + 10 * (int) (ClientSize.Height / 600.0); // offset from top of form
            int farLeft = (ClientSize.Width - LBL_WIDTH) / 2 - 2 * LBL_WIDTH;

            // Find the largest font size that fits the screen
            float fontSize = 30F;
            System.Drawing.Font activeFont;
            Graphics g = this.CreateGraphics();
            string strWide = "5555 - ΨΨΨΨ";

            VerifyFontFit:
                activeFont = new Font("Microsoft Sans Serif", fontSize, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                SizeF stringSize = g.MeasureString(strWide, activeFont);
                if ((stringSize.Width + 10) >= LBL_WIDTH) {
                    fontSize -= 0.5F;
                    goto VerifyFontFit;
                }

            g.Dispose();

            for (int i = 0; i <= 9; i++) {
                grid[i].Font = activeFont;
                grid[i].Height = LBL_HEIGHT;
                grid[i].Width = LBL_WIDTH;
                grid[i].Top = LBL_OFFSET + LBL_OFFSET + (i / 5) * LBL_HEIGHT;
                grid[i].Left = farLeft + (i % 5) * LBL_WIDTH;
            }

            /* ========== Align lblQuestion  ========= */
            int QUESTION_OFFSET = (int) (50 + (ClientSize.Height - 600) / 2.5);   // vertical distance between top grid and question label
            lblQuestion.Font = activeFont;
            lblQuestion.Width = LBL_WIDTH;
            lblQuestion.Height = LBL_HEIGHT;
            lblQuestion.Left = (ClientSize.Width - lblQuestion.Width) / 2;
            lblQuestion.Top = LBL_OFFSET + 2 * LBL_HEIGHT + QUESTION_OFFSET;

            /* ========== Align answer buttons ============ */
            int BTN_WIDTH = (int) (lbl0.Width * 0.7);
            int BTN_HEIGHT = LBL_HEIGHT + 5;
            int BTN_INTERVAL = 10;     // vertical distance between buttons
            
            int centerLeft = (ClientSize.Width - BTN_WIDTH) / 2;
            for (int i = 0; i <= 3; i++) {
                btns[i].Font = activeFont;
                btns[i].Height = BTN_HEIGHT;
                btns[i].Width = BTN_WIDTH;
                btns[i].Left = centerLeft;
                btns[i].Top = lblQuestion.Bottom + (i * BTN_INTERVAL) + (i * BTN_HEIGHT);
            }

            /* ========== Align labels for buttons ============ */
            const int LBL_BTN_WIDTH = 77;

            for (int i = 0; i < 4; i++) {
                lblBtns[i].Font = activeFont;
                lblBtns[i].Height = BTN_HEIGHT;
                lblBtns[i].Width = LBL_BTN_WIDTH;
                lblBtns[i].Left = btns[i].Left - LBL_BTN_WIDTH;
                lblBtns[i].Top = btns[i].Top - 3;
            }
        }

        #endregion

        #region User Input

        private void button_Click(object sender, EventArgs e) {
            Button btn = (Button)sender;
            if (testStatus != CogTest.TEST_DEMO)
                processInput(btn.Text);
        }

        private void frmSpeed_KeyPress(object sender, KeyPressEventArgs e) {
            if (reactStopwatch.IsRunning && testStatus != CogTest.TEST_DEMO && CogTest.ValidateKeyPress() == true) {
                switch (e.KeyChar) {
                    case (char)49:  // the '1' key
                        processInput(btnA.Text);
                        break;
                    case (char)50:  // the '2' key
                        processInput(btnB.Text);
                        break;
                    case (char)51:  // the '3' key
                        processInput(btnC.Text);
                        break;
                    case (char)52:  // the '4' key
                        processInput(btnD.Text);
                        break;
                }
            }
        }

        // process input after keypress or button click
        private void processInput(string input) {
            // double clicks (within 350 ms) are ignored
            if (doublePress == true) return;

            int reactTime = (int)reactStopwatch.ElapsedMilliseconds;
            reactStopwatch.Reset();

            bool correct = (dict[input] == lblQuestion.Text) ? true : false;
            test.input(correct, reactTime);

            doublePress = true;
            clickTimer.Start();

            nextTrial();
        }

        // enable clicking again
        private void clickTimer_Tick(object sender, EventArgs e) {
            clickTimer.Stop();
            doublePress = false;
        }

        #endregion

        /* Timer tick - either the practice test ended, or it's time to check the official test stopwatch.
         * A form timer is not used for the official test because form timers aren't very accurate
         * for long intervals (more than a minute). */ 
        private void formTimer_Tick(object sender, EventArgs e) {

            if (this.testStatus == CogTest.TEST_PRACTICE) {
                if (testStopwatch.ElapsedMilliseconds >= PRACTICE_DUARTION) {
                    // practice test ended
                    CogTest.closeForm(this);
                }

            } else if (this.testStatus == CogTest.TEST_OFFICIAL) {
                if (testStopwatch.ElapsedMilliseconds >= OFFICIAL_DURATION) {
                    // regular test ended
                    test.finish();
                    CogTest.closeForm(this);
                }
            }
        }

        // generate a new grid
        private void newGrid() {
            trials = 0;
            dict = test.generateGrid(out answers, out options);

            // update grid on form
            for (int i = 0; i < 10; i++)
                grid[i].Text = dict.ElementAt(i).Key + " - " + dict.ElementAt(i).Value;
        }

        // display the next trial for the current grid
        private void nextTrial() {
            if (trials == 5) newGrid();
            trials++;

            int trialIndex = trials - 1;

            // update question
            lblQuestion.Text = answers[trialIndex];

            // update buttons
            btns[0].Text = options[trialIndex, 0];
            btns[1].Text = options[trialIndex, 1];
            btns[2].Text = options[trialIndex, 2];
            btns[3].Text = options[trialIndex, 3];

            // bring focus away from the button (removes blue highlight after click)
            lblQuestion.Focus(); 

            reactStopwatch.Start();
        }

    }
}
