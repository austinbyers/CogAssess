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
    public partial class frmKeyboardTutorial : Form
    {
        const int MAX_NUMBERS = 81;     // maximum number of numbers to display
        const int WAIT_INTERVAL = 700;  // how many milliseconds to wait between attempts

        int numFinished;                // number of finished attempts
        Random randGen;

        public frmKeyboardTutorial() {
            InitializeComponent();

            // Add global event handlers
            this.SizeChanged += new EventHandler(CogTest.FormSizeChanged);
            this.KeyDown += new KeyEventHandler(CogTest.KeyDown);
            this.FormClosing += new FormClosingEventHandler(CogTest.FormClosing);

            this.KeyPreview = true;
            txtInput.TextChanged += new EventHandler(txtInput_TextChanged);
            txtInput.KeyPress += new KeyPressEventHandler(txtInput_KeyPress);
            colorTimer.Interval = WAIT_INTERVAL;

            numFinished = 0;
            randGen = new Random();
        }

        private void frmKeyboardTutorial_Load(object sender, EventArgs e) {
            CogTest.setControlVisibility(this, false);
            Point center = new Point(ClientSize.Width / 2, ClientSize.Height / 2);

            btnContinue.Top = 20;
            btnContinue.Left = ClientSize.Width - btnContinue.Width - 20;

            lblDisplay.Left = center.X - lblDisplay.Width / 2;
            lblDisplay.Top = center.Y - lblDisplay.Height - 50;

            txtInput.Left = center.X - txtInput.Width / 2;
            txtInput.Top = center.Y + 50;
            txtInput.Text = "";

            nextNumber();

            CogTest.setControlVisibility(this, true);
            txtInput.Focus();
        }

        void txtInput_TextChanged(object sender, EventArgs e) {
            colorTimer.Stop();
            if (txtInput.Text == "") {
                // reset textbox
                txtInput.BackColor = Color.White;
            } else if (txtInput.Text == lblDisplay.Text) {
                // correct answer
                numFinished++;
                txtInput.Enabled = false;
                txtInput.BackColor = Color.Green;
                lblDisplay.Focus();
                colorTimer.Start();
            } else {
                // incorrect answer
                txtInput.BackColor = Color.Red;
                colorTimer.Start();
            }
        }

        // check keypress and ignore anything that isn't a number
        void txtInput_KeyPress(object sender, KeyPressEventArgs e) {
            if ( CogTest.ValidateKeyPress() == false || e.KeyChar < 49 || e.KeyChar > 57) {
                e.Handled = true;
            }
        }

        // pick a new number to display
        private void nextNumber() {
            if (numFinished == MAX_NUMBERS) {
                // finished all of the trials - close form
                CogTest.closeForm(this);
            } else {
                string newDigit;
                do {
                    newDigit = randGen.Next(1, 10).ToString();
                } while (newDigit == txtInput.Text);

                lblDisplay.Text = newDigit;
                txtInput.Enabled = true;
                txtInput.Text = "";
                txtInput.Focus();
            }
        }

        private void colorTimer_Tick(object sender, EventArgs e) {
            colorTimer.Stop();
            if (txtInput.BackColor == Color.Green) {
                nextNumber();
            } else if (txtInput.BackColor == Color.Red) {
                txtInput.Text = "";
            }
        }

        private void btnContinue_Click(object sender, EventArgs e) {
            CogTest.closeForm(this);
        }
    }
}
