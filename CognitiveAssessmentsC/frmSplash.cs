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
    public partial class frmSplash : Form
    {
        const int BREAK_TIME = 45000;    // amount of time (in ms) to display the break splash screen

        // constructor
        public frmSplash()
        {
            InitializeComponent();

            // Add global event handlers
            this.KeyDown += new KeyEventHandler(CogTest.KeyDown);
            this.SizeChanged += new EventHandler(CogTest.FormSizeChanged);
            this.FormClosing += new FormClosingEventHandler(CogTest.FormClosing);

            this.Shown += new EventHandler(frmSplash_Shown);
        }

        // when form is shown, re-center labels
        void frmSplash_Shown(object sender, EventArgs e) {
            if (lblInstruct.Visible == false) {
                lblName.Left = (ClientSize.Width - lblName.Width) / 2;
                lblName.Top = (ClientSize.Height - lblName.Height) / 2;
            } else {
                // center horizontally
                // Note that the -5 compensates for the slant of the italic text in lblInstruct
                lblInstruct.Left = (ClientSize.Width - lblInstruct.Width) / 2 - 5;
                lblName.Left = (ClientSize.Width - lblName.Width) / 2;

                // center vertically
                const int LBL_INTVL = 20;     // distance between labels
                lblName.Top = (ClientSize.Height - lblName.Height - LBL_INTVL - lblInstruct.Height) / 2;
                lblInstruct.Top = lblName.Top + lblName.Height + LBL_INTVL;
            }
        }

        #region Splash Screens

        // main splash screen (blue)
        public static void mainSplash(string name, Form owner) {
            frmSplash frm = new frmSplash();
            frm.lblName.Text = name;
            frm.lblInstruct.Visible = false;
            frm.BackColor = Color.FromArgb(0, 0, 64);
            CogTest.enableDoubleEnterAdvance(frm);
            frm.ShowDialog(owner);
        }

        // practice splash screen (red)
        public static void practiceSplash(string name, Form owner) {
            frmSplash frm = new frmSplash();
            frm.lblName.Text = name;
            frm.lblInstruct.Visible = true;
            frm.lblInstruct.Text = "Practice Test";
            frm.BackColor = Color.FromArgb(64, 0, 0);
            CogTest.enableDoubleEnterAdvance(frm);
            frm.ShowDialog(owner);
        }

        // official test splash screen (green)
        public static void testSplash(string name, Form owner) {
            frmSplash frm = new frmSplash();
            frm.lblName.Text = name;
            frm.lblInstruct.Visible = true;
            frm.lblInstruct.Text = "Official Test";
            frm.BackColor = Color.FromArgb(0, 64, 0);
            CogTest.enableDoubleEnterAdvance(frm);
            frm.ShowDialog(owner);
        }

        // break screen (purple)
        public static void breakSplash(Form owner) {
            frmSplash frm = new frmSplash();
            frm.lblInstruct.Visible = false;
            frm.lblName.Text = "Break";
            frm.BackColor = Color.FromArgb(64, 0, 64);
            frm.breakTimer.Interval = BREAK_TIME;
            frm.breakTimer.Start();
            frm.ShowDialog(owner);
        }

        // final splash screen (yellow)
        public static void finalSplash(Form owner) {
            frmSplash frm = new frmSplash();
            frm.lblName.Text = "The End";
            frm.lblInstruct.Visible = true;
            frm.lblInstruct.Text = "Thank you!";
            frm.BackColor = Color.FromArgb(64, 64, 0);
            CogTest.enableDoubleEnterAdvance(frm);
            frm.ShowDialog(owner);
        }

        // close break screen after BREAK_TIME milliseconds
        private void breakTimer_Tick(object sender, EventArgs e) {
            CogTest.closeForm(this);
        }

        #endregion

    }
}
