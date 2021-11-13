using NXSBinEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace NBEGUI {
    public partial class NBEGUI : Form {

        private List<string> Temp = new List<string>();
        private string fileName = "";

        private bool Filter(string str)
        {
            foreach (var fil in filter_l)
            {
                if (str.StartsWith(fil.ToString())) return false;
                if (str.EndsWith(fil.ToString())) return false;
            }
            return true;
        }
        private char[] filter_l = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'\".()!@#$%^&*▽[]→".ToCharArray();

        public NBEGUI() {
            InitializeComponent();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\n' || e.KeyChar == '\r') {
                try {
                    listBox1.Items[listBox1.SelectedIndex] = textBox1.Text;
                } catch { }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                textBox1.Text = listBox1.Items[listBox1.SelectedIndex].ToString();
            } catch { }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            openFileDialog1.DefaultExt = "bin";
            openFileDialog1.Filter = "NeXAS Script File | *.bin";
            openFileDialog1.ShowDialog();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            if (fileName == "")
            {
                MessageBox.Show("No script file opened.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                saveFileDialog1.DefaultExt = "bin";
                saveFileDialog1.Filter = "NeXAS Script File | *.bin";
                saveFileDialog1.ShowDialog();
            }
        }

        BinHelper Editor;
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e) {
            fileName = openFileDialog1.FileName;
            this.Text = Path.GetFileName(openFileDialog1.FileName);
            Editor = new BinHelper(File.ReadAllBytes(openFileDialog1.FileName));

            listBox1.Items.Clear();
            foreach (string str in Editor.Import()) {
                listBox1.Items.Add(str);
            }

            var lines = listBox1.Items;

            Temp.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                Temp.Add(lines[i].ToString());
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e) { 
            List<string> Rst = new List<string>();
            foreach (string str in listBox1.Items)
                Rst.Add(str);

            File.WriteAllBytes(saveFileDialog1.FileName, Editor.Export(Rst.ToArray()));

            MessageBox.Show("Saved", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (fileName == "")
            {
                this.saveToolStripMenuItem.Enabled = false;
                this.extractTextToolStripMenuItem.Enabled = false;
                this.applyTextToolStripMenuItem.Enabled = false;
            }
            else
            {
                this.saveToolStripMenuItem.Enabled = true;
                this.extractTextToolStripMenuItem.Enabled = true;
                this.applyTextToolStripMenuItem.Enabled = true;
            }
        }

        /// <summary>
        /// Extract to an easy-to-edit text file in a text editor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void extractTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var lines = listBox1.Items;
            int lineIndex = 1;
            List<string> arr = new List<string>();

            for (int i = 0; i < lines.Count; i++)
            {
                if (Filter(listBox1.Items[i].ToString()))
                {
                    arr.Add("@" + lineIndex);
                    arr.Add("//Text:" + lines[i].ToString());
                    arr.Add(lines[i].ToString());
                    arr.Add("");
                    lineIndex++;
                    //arr[i] = listBox1.Items[i].ToString();
                }
                else
                {
                    continue;
                    //arr[i] = "";
                }
            }

            string txtFile;
            txtFile = Path.ChangeExtension(fileName, ".txt");
            string txtFileName = $@"{txtFile}";

            try
            {
                if (fileName == "") MessageBox.Show("No text to extract.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                if (File.Exists(txtFileName) && MessageBox.Show("Extraction text has already created. \nDo you want to create a new extraction text?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    File.Delete(txtFileName);

                    using (StreamWriter sw = File.CreateText(txtFileName))
                    {
                        foreach (string line in arr)
                        {
                            sw.WriteLine(line);
                        }
                    }

                    MessageBox.Show("Text extracted", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
            }
        }

        private bool Filter2(string str)
        {
            if (str == "") return false;
            foreach (var fil in filter_l2)
            {
                if (str.StartsWith(fil.ToString())) return false;
                if (str.EndsWith(fil.ToString())) return false;
            }
            return true;
        }
        private char[] filter_l2 = "@/".ToCharArray();

        private void applyTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileName == "")
            {
                MessageBox.Show("No script file opened.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                string str = string.Empty;
                var lines = listBox1.Items;
                string[] arr = new string[listBox1.Items.Count];
                List<string> Replacement = new List<string>();
                int replacementIndex = 0;
                int replaceableLine = 0;

                var openFile = new OpenFileDialog();
                openFile.DefaultExt = "txt";
                openFile.Filter = "Text File | *.txt";
                var dr = openFile.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        lines[i] = Temp[i];
                    }

                    str = File.ReadAllText(openFile.FileName, System.Text.Encoding.UTF8);
                    arr = Regex.Split(str, "\r\n");

                    foreach (string line in arr)
                    {
                        if (Filter2(line))
                        {
                            Replacement.Add(line);
                        }
                        else
                        {
                            continue;
                        }
                    }
       
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (Filter(lines[i].ToString()))
                        {
                            replaceableLine++;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (replaceableLine == Replacement.Count)
                    {
                        for (int i = 0; i < lines.Count; i++)
                        {
                            if (Filter(lines[i].ToString()))
                            {
                                lines[i] = Replacement[replacementIndex];
                                replacementIndex++;
                                //arr[i] = listBox1.Items[i].ToString();
                            }
                            else
                            {
                                continue;
                                //arr[i] = "";
                            }
                        }

                        replacementIndex = 0;
                    }
                    else
                    {
                        MessageBox.Show("Txt file does not match.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
        }
    }
}
