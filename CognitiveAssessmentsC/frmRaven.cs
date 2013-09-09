using System;
using System.Collections;
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
    public partial class frmRaven : Form
    {
        Random rand;
        ArrayList questions;
        Button[] btns;
        Label[] lbls;
        int answer;
        int numCorrect, numUnfinished;
        Stopwatch stopwatch = new Stopwatch();      // keeps track of time for official test

        bool doublePress;       // ignore double click (or double keypress)

        int testStatus;          // TEST_DEMO, TEST_PRACTICE, or TEST_OFFICIAL

        #region Form Setup

        public frmRaven(int testStatus)
        {
            InitializeComponent();

            // Add global event handlers
            this.SizeChanged += new EventHandler(CogTest.FormSizeChanged);
            this.KeyDown += new KeyEventHandler(CogTest.KeyDown);
            this.FormClosing += new FormClosingEventHandler(CogTest.FormClosing);

            this.btn0.Click += new EventHandler(btnClick);
            this.btn1.Click += new EventHandler(btnClick);
            this.btn2.Click += new EventHandler(btnClick);
            this.btn3.Click += new EventHandler(btnClick);
            this.btn4.Click += new EventHandler(btnClick);
            this.btn5.Click += new EventHandler(btnClick);
            this.btn6.Click += new EventHandler(btnClick);
            this.btn7.Click += new EventHandler(btnClick);
            this.KeyPress += new KeyPressEventHandler(frmRaven_KeyPress);
            this.KeyPreview = true;

            this.testStatus = testStatus;
        }

        private void frmRaven_Load(object sender, EventArgs e)
        {
            CogTest.setControlVisibility(this, false);
            rand = new Random();

            btns = new Button[8];
            btns[0] = btn0;
            btns[1] = btn1;
            btns[2] = btn2;
            btns[3] = btn3;
            btns[4] = btn4;
            btns[5] = btn5;
            btns[6] = btn6;
            btns[7] = btn7;

            lbls = new Label[8];
            lbls[0] = lbl0;
            lbls[1] = lbl1;
            lbls[2] = lbl2;
            lbls[3] = lbl3;
            lbls[4] = lbl4;
            lbls[5] = lbl5;
            lbls[6] = lbl6;
            lbls[7] = lbl7;

            // align main picture box
            picBox.Width = 550;
            picBox.Height = Math.Min((int) (this.ClientSize.Height / 2.5), 440);
            picBox.Left = (ClientSize.Width - picBox.Width) / 2;
            picBox.Top = 20;//-20;

            // test summary data
            numCorrect = 0;

            switch (this.testStatus) {
                case CogTest.TEST_DEMO:
                    // show what is currently on form for demo mode
                    this.BackColor = CogTest.DEMO_COLOR;
                    CogTest.enableDoubleEnterAdvance(this);
                    alignButtons(8);
                    CogTest.setControlVisibility(this, true);
                    break;
                case CogTest.TEST_PRACTICE:
                    questions = new ArrayList(2);
                    questions.Add(-1);
                    questions.Add(-2);
                    numUnfinished = 2;
                    CogTest.setControlVisibility(this, true);
                    showNextQuestion();
                    break;
                case CogTest.TEST_OFFICIAL:
                    questions = CogTest.createRandomList(1, 10, 10, ref rand);
                    numUnfinished = 10;

                    // set visibility before loading the question because the question may only need 6 butons
                    CogTest.setControlVisibility(this, true);
                    showNextQuestion();

                    testTimer.Start();
                    stopwatch.Start();
                    break;
            }

            picBox.Focus();
        }

        private void alignButtons(int numBtns) {

            int BTN_HEIGHT = Math.Min((int) (picBox.Height / 2.5), 130);
            int BTN_WIDTH = (int) (BTN_HEIGHT * 1.73); 
            int V_INTVL = 60;     // vertical space between buttons
            int H_INTVL = 15;     // horiztonal space between buttons

            int farTop = picBox.Top + picBox.Height + 15;

            if (numBtns == 8) {
                int farLeft = (int)(ClientSize.Width / 2 - 1.5 * H_INTVL - 2 * BTN_WIDTH);

                // align 8 buttons
                for (int i = 0; i <= 7; i++) {
                    lbls[i].Width = BTN_WIDTH;
                    lbls[i].Left = farLeft + (i % 4) * (BTN_WIDTH + H_INTVL);
                    lbls[i].Top = farTop + (i / 4) * (BTN_HEIGHT + V_INTVL);    // integer division

                    btns[i].Width = BTN_WIDTH;
                    btns[i].Height = BTN_HEIGHT;
                    btns[i].Left = lbls[i].Left;
                    btns[i].Top = lbls[i].Bottom;
                }

                lbl6.Visible = true;
                btn6.Visible = true;
                lbl7.Visible = true;
                btn7.Visible = true;
            } else if (numBtns == 6) {
                int farLeft = (int)(ClientSize.Width / 2 - H_INTVL - 1.5 * BTN_WIDTH);

                // align 6 buttons
                for (int i = 0; i <= 5; i++) {
                    lbls[i].Width = BTN_WIDTH;
                    lbls[i].Left = farLeft + (i % 3) * (BTN_WIDTH + H_INTVL);
                    lbls[i].Top = farTop + (i / 3) * (BTN_HEIGHT + V_INTVL);    // integer division

                    btns[i].Width = BTN_WIDTH;
                    btns[i].Height = BTN_HEIGHT;
                    btns[i].Left = lbls[i].Left;
                    btns[i].Top = lbls[i].Bottom;
                }

                lbl6.Visible = false;
                btn6.Visible = false;
                lbl7.Visible = false;
                btn7.Visible = false;
            }
        }

        #endregion

        #region User Input

        private void btnClick(object sender, EventArgs e) {
            Button btn = (Button)sender;
            if (this.testStatus != CogTest.TEST_DEMO)
                processInput(Convert.ToInt32(btn.Tag) == answer);
        }

        private void frmRaven_KeyPress(object sender, KeyPressEventArgs e) {
            // accept either 1-6 or 1-8
            if (this.testStatus != CogTest.TEST_DEMO && CogTest.ValidateKeyPress() == true) {
                if (e.KeyChar >= 49 && (e.KeyChar <= 54 || e.KeyChar <= 56 && lbl7.Visible == true))
                    processInput((e.KeyChar - 48) == answer);
            }
        }

        private void processInput(bool correct) {
            // double clicks (within 350 ms) are ignored
            if (doublePress == true) return;

            if (this.testStatus == CogTest.TEST_OFFICIAL) {
                if (correct == true) {
                    numCorrect++;
                }
            }

            numUnfinished--;
            questions.RemoveAt(0);

            if (numUnfinished == 0) {
                // finished with the test
                if (this.testStatus == CogTest.TEST_OFFICIAL)
                    saveResultsFile();
                CogTest.closeForm(this);
            } else {
                // load the next question
                showNextQuestion();
                doublePress = true;
                clickTimer.Start();
                picBox.Focus();     // focus away from the buttons
            }
        }

        // enable clicking again
        private void clickTimer_Tick(object sender, EventArgs e) {
            clickTimer.Stop();
            doublePress = false;
        }

        #endregion

        private void showNextQuestion() {
            switch ((int)questions[0]) {
                case -1:
                    practice1();
                    break;
                case -2:
                    practice2();
                    break;
                case 1:
                    test1();
                    break;
                case 2:
                    test2();
                    break;
                case 3:
                    test3();
                    break;
                case 4:
                    test4();
                    break;
                case 5:
                    test5();
                    break;
                case 6:
                    test6();
                    break;
                case 7:
                    test7();
                    break;
                case 8:
                    test8();
                    break;
                case 9:
                    test9();
                    break;
                case 10:
                    test10();
                    break;
            }
        }

        #region Raven Tests

        private void practice1() {
            picBox.Image = Properties.Resources.panelP1;
            btn0.BackgroundImage = Properties.Resources.btnP1_1;
            btn1.BackgroundImage = Properties.Resources.btnP1_2;
            btn2.BackgroundImage = Properties.Resources.btnP1_3;
            btn3.BackgroundImage = Properties.Resources.btnP1_4;
            btn4.BackgroundImage = Properties.Resources.btnP1_5;
            btn5.BackgroundImage = Properties.Resources.btnP1_6;
            btn6.BackgroundImage = Properties.Resources.btnP1_7;
            btn7.BackgroundImage = Properties.Resources.btnP1_8;
            alignButtons(8);
        }

        private void practice2() {
            picBox.Image = Properties.Resources.panelP2;
            btn0.BackgroundImage = Properties.Resources.btnP2_1;
            btn1.BackgroundImage = Properties.Resources.btnP2_2;
            btn2.BackgroundImage = Properties.Resources.btnP2_3;
            btn3.BackgroundImage = Properties.Resources.btnP2_4;
            btn4.BackgroundImage = Properties.Resources.btnP2_5;
            btn5.BackgroundImage = Properties.Resources.btnP2_6;
            btn6.BackgroundImage = Properties.Resources.btnP2_7;
            btn7.BackgroundImage = Properties.Resources.btnP2_8;
            alignButtons(8);
        }

        private void test1() {
            answer = 3;
            picBox.Image = Properties.Resources.panel1;
            btn0.BackgroundImage = Properties.Resources.btn1_1;
            btn1.BackgroundImage = Properties.Resources.btn1_2;
            btn2.BackgroundImage = Properties.Resources.btn1_3;
            btn3.BackgroundImage = Properties.Resources.btn1_4;
            btn4.BackgroundImage = Properties.Resources.btn1_5;
            btn5.BackgroundImage = Properties.Resources.btn1_6;
            btn6.BackgroundImage = Properties.Resources.btn1_7;
            btn7.BackgroundImage = Properties.Resources.btn1_8;
            alignButtons(8);
        }

        private void test2() {
            answer = 3;
            picBox.Image = Properties.Resources.panel2;
            btn0.BackgroundImage = Properties.Resources.btn2_1;
            btn1.BackgroundImage = Properties.Resources.btn2_2;
            btn2.BackgroundImage = Properties.Resources.btn2_3;
            btn3.BackgroundImage = Properties.Resources.btn2_4;
            btn4.BackgroundImage = Properties.Resources.btn2_5;
            btn5.BackgroundImage = Properties.Resources.btn2_6;
            alignButtons(6);
        }

        private void test3() {
            answer = 3;
            picBox.Image = Properties.Resources.panel3;
            btn0.BackgroundImage = Properties.Resources.btn3_1;
            btn1.BackgroundImage = Properties.Resources.btn3_2;
            btn2.BackgroundImage = Properties.Resources.btn3_3;
            btn3.BackgroundImage = Properties.Resources.btn3_4;
            btn4.BackgroundImage = Properties.Resources.btn3_5;
            btn5.BackgroundImage = Properties.Resources.btn3_6;
            alignButtons(6);
        }

        private void test4() {
            answer = 5;
            picBox.Image = Properties.Resources.panel4;
            btn0.BackgroundImage = Properties.Resources.btn4_1;
            btn1.BackgroundImage = Properties.Resources.btn4_2;
            btn2.BackgroundImage = Properties.Resources.btn4_3;
            btn3.BackgroundImage = Properties.Resources.btn4_4;
            btn4.BackgroundImage = Properties.Resources.btn4_5;
            btn5.BackgroundImage = Properties.Resources.btn4_6;
            alignButtons(6);
        }

        private void test5() {
            answer = 4;
            picBox.Image = Properties.Resources.panel5;
            btn0.BackgroundImage = Properties.Resources.btn5_1;
            btn1.BackgroundImage = Properties.Resources.btn5_2;
            btn2.BackgroundImage = Properties.Resources.btn5_3;
            btn3.BackgroundImage = Properties.Resources.btn5_4;
            btn4.BackgroundImage = Properties.Resources.btn5_5;
            btn5.BackgroundImage = Properties.Resources.btn5_6;
            alignButtons(6);
        }

        private void test6() {
            answer = 4;
            picBox.Image = Properties.Resources.panel6;
            btn0.BackgroundImage = Properties.Resources.btn6_1;
            btn1.BackgroundImage = Properties.Resources.btn6_2;
            btn2.BackgroundImage = Properties.Resources.btn6_3;
            btn3.BackgroundImage = Properties.Resources.btn6_4;
            btn4.BackgroundImage = Properties.Resources.btn6_5;
            btn5.BackgroundImage = Properties.Resources.btn6_6;
            btn6.BackgroundImage = Properties.Resources.btn6_7;
            btn7.BackgroundImage = Properties.Resources.btn6_8;
            alignButtons(8);
        }

        private void test7() {
            answer = 5;
            picBox.Image = Properties.Resources.panel7;
            btn0.BackgroundImage = Properties.Resources.btn7_1;
            btn1.BackgroundImage = Properties.Resources.btn7_2;
            btn2.BackgroundImage = Properties.Resources.btn7_3;
            btn3.BackgroundImage = Properties.Resources.btn7_4;
            btn4.BackgroundImage = Properties.Resources.btn7_5;
            btn5.BackgroundImage = Properties.Resources.btn7_6;
            btn6.BackgroundImage = Properties.Resources.btn7_7;
            btn7.BackgroundImage = Properties.Resources.btn7_8;
            alignButtons(8);
        }

        private void test8() {
            answer = 3;
            picBox.Image = Properties.Resources.panel8;
            btn0.BackgroundImage = Properties.Resources.btn8_1;
            btn1.BackgroundImage = Properties.Resources.btn8_2;
            btn2.BackgroundImage = Properties.Resources.btn8_3;
            btn3.BackgroundImage = Properties.Resources.btn8_4;
            btn4.BackgroundImage = Properties.Resources.btn8_5;
            btn5.BackgroundImage = Properties.Resources.btn8_6;
            btn6.BackgroundImage = Properties.Resources.btn8_7;
            btn7.BackgroundImage = Properties.Resources.btn8_8;
            alignButtons(8);
        }

        private void test9() {
            answer = 8;
            picBox.Image = Properties.Resources.panel9;
            btn0.BackgroundImage = Properties.Resources.btn9_1;
            btn1.BackgroundImage = Properties.Resources.btn9_2;
            btn2.BackgroundImage = Properties.Resources.btn9_3;
            btn3.BackgroundImage = Properties.Resources.btn9_4;
            btn4.BackgroundImage = Properties.Resources.btn9_5;
            btn5.BackgroundImage = Properties.Resources.btn9_6;
            btn6.BackgroundImage = Properties.Resources.btn9_7;
            btn7.BackgroundImage = Properties.Resources.btn9_8;
            alignButtons(8);
        }

        private void test10() {
            answer = 8;
            picBox.Image = Properties.Resources.panel10;
            btn0.BackgroundImage = Properties.Resources.btn10_1;
            btn1.BackgroundImage = Properties.Resources.btn10_2;
            btn2.BackgroundImage = Properties.Resources.btn10_3;
            btn3.BackgroundImage = Properties.Resources.btn10_4;
            btn4.BackgroundImage = Properties.Resources.btn10_5;
            btn5.BackgroundImage = Properties.Resources.btn10_6;
            btn6.BackgroundImage = Properties.Resources.btn10_7;
            btn7.BackgroundImage = Properties.Resources.btn10_8;
            alignButtons(8);
        }

        #endregion

        // check the official test stopwatch every second
        // quit early if the test takes longer than 10 minutes
        private void testTimer_Tick(object sender, EventArgs e) {
            if (stopwatch.ElapsedMilliseconds >= 600000) {
                testTimer.Stop();
                saveResultsFile();
                CogTest.closeForm(this);
            }
        }

        // write results to a text file
        private void saveResultsFile() {
            /* ----- write the trial data to a new file -------- */
            string rawFilename = "subj" + CogTest.uid + "raven.txt";
            System.IO.StreamWriter rawFile = new System.IO.StreamWriter(rawFilename);
            string line = CogTest.uid + ',' + numCorrect.ToString() + ',' + numUnfinished.ToString();
            rawFile.WriteLine(line);
            rawFile.Close();

            /* ----- append the summary data to ravenSummary.txt -------- */
            System.IO.StreamWriter summaryFile = new System.IO.StreamWriter("ravenSummary.txt", true);
            string summaryLine = CogTest.uid + ',' + numCorrect.ToString() + ',' +
                numUnfinished.ToString();
            summaryFile.WriteLine(summaryLine);
            summaryFile.Close();
        }
    }
}
