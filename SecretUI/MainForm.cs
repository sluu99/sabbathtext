using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SabbathText;

namespace SecretUI
{
    public partial class MainForm : Form
    {
        private SecretProvider secretProvider;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*
             To make a cert use the following commands:
             C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Bin>makecert.exe -sv devcert.pvk -n "CN=Development Certificate" devcert.cer -b 01/01/2015 -e 01/01/2115 -r -sky Exchange -len 2048 -a sha256
              
             
             */
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            if (this.secretProvider == null)
            {
                MessageBox.Show("Please select a certificate!");
            }

            this.txtEncryptedText.Text = this.secretProvider.Encrypt(
                this.txtPlainText.Text);
            this.txtPlainText.Text = string.Empty;
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            if (this.secretProvider == null)
            {
                MessageBox.Show("Please select a certificate!");
            }

            this.txtPlainText.Text = this.secretProvider.Decrypt(
                this.txtEncryptedText.Text);
            this.txtEncryptedText.Text = string.Empty;
        }

        private void linkSelectCert_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.openFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (PasswordForm pwForm = new PasswordForm())
                {
                    pwForm.ShowDialog();

                    try
                    {
                        this.secretProvider = new SecretProvider(this.openFileDialog.FileName, pwForm.Password);
                        this.linkSelectCert.Text = this.openFileDialog.FileName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
    }
}
