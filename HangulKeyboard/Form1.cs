using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HangulKeyboard
{
    public partial class Form1 : Form
    {
        private const int NoSound = 11;

        private static readonly string[] First = new string[]
        {
            "g", "kk", "n", "d", "tt", "r", "m", "b", "pp", "s", "ss", "", "j", "jj", "ch", "k", "t", "p", "h"
        };

        private static readonly string[] Mid = new string[]
        {
            "a", "ae", "ya", "yae", "eo", "e", "yeo", "ye", "o", "wa", "wae", "oe", "yo", "u", "wo", "we", "wi", "yu", "eu", "ui", "i"
        };

        private static readonly string[] Last = new string[]
        {
            "g", "kk", "ks", "n", "nj", "nh", "d", "r", "lg", "lm", "lb", "ls", "lt", "lp", "lh", "m", "b", "ps", "s", "ss", "ng", "j", "ch", "k", "t", "p", "h"
        };

        private static readonly string[] Jamo = new string[]
        {
            "g", "kk", "ks", "n", "nj", "nh", "d", "tt", "r", "lg", "lm", "lb", "ls", "lt", "lp", "lh", "m", "b", "pp", "ps", "s", "ss", "ng", "j", "jj", "ch", "k", "t", "p", "h"
        };

        public Form1()
        {
            InitializeComponent();
        }

        private static bool StartsWith(string text, string with, int index)
        {
            for(int i = 0; i < with.Length; i++)
            {
                if (index + i >= text.Length || text[index + i] != with[i]) return false;
            }
            return true;
        }

        private static string GetLatin(char c, out bool isHangul)
        {
            if('\u3131' <= c && c <= '\u314E')
            {
                isHangul = true;
                return Jamo[c - 0x3131];
            }
            else if('\uAC00' <= c && c <= '\uD7A3')
            {
                int i = (c - 0xAC00) / 588;
                int j = ((c - 0xAC00) % 588) / 28;
                int k = (c - 0xAC00) % 28;
                isHangul = true;
                if (k == 0) return First[i] + Mid[j];
                else return First[i] + Mid[j] + Last[k - 1];
            }
            else
            {
                isHangul = false;
                return "" + c;
            }
        }

        private static char GetHangul(string text, ref int index)
        {
            if (index < 0 || text.Length <= index) return '\u0000'; 
            if(text[index] < 'a' && 'z' < text[index])
            {
                index++;
                return text[index];
            }
            (string, char) max = ("", '0');
            bool couldBeFirst = false;
            for (int i = 0; i < First.Length; i++)
            {
                if (!StartsWith(text, First[i], index) || i == NoSound) continue;
                couldBeFirst = true;
                for (int j = 0; j < Mid.Length; j++)
                {
                    if (!StartsWith(text, Mid[j], index + First[i].Length)) continue;
                    if (max.Item1.Length < First[i].Length + Mid[j].Length) 
                        max = (First[i] + Mid[j], (char)('\uAC00' + 588 * i + 28 * j));
                    for(int k = 0; k < Last.Length; k++)
                    {
                        if (!StartsWith(text, Last[k], index + First[i].Length + Mid[j].Length)) continue;
                        if (max.Item1.Length < First[i].Length + Mid[j].Length + Last[k].Length) 
                            max = (First[i] + Mid[j] + Last[k], (char)('\uAC00' + 588 * i + 28 * j + k + 1));
                    }
                }
            }
            if (max.Item1.Length > 0)
            {
                index += max.Item1.Length;
                return max.Item2;
            }
            if (!couldBeFirst)
            {
                for (int j = 0; j < Mid.Length; j++)
                {
                    if (!StartsWith(text, Mid[j], index)) continue;
                    if (max.Item1.Length < Mid[j].Length)
                        max = (Mid[j], (char)('\uAC00' + 6468 + 28 * j));
                    for (int k = 0; k < Last.Length; k++)
                    {
                        if (!StartsWith(text, Last[k], index + Mid[j].Length)) continue;
                        if (max.Item1.Length < Mid[j].Length + Last[k].Length)
                            max = (Mid[j] + Last[k], (char)('\uAC00' + 6468 + 28 * j + k + 1));
                    }
                }
            }
            if (max.Item1.Length > 0)
            {
                index += max.Item1.Length;
                return max.Item2;
            }
            for(int l = 0; l < Jamo.Length; l++)
            {
                if (!StartsWith(text, Jamo[l], index)) continue;
                if (max.Item1.Length < Jamo[l].Length)
                    max = (Jamo[l], (char)('\u3131' + l));
            }
            if(max.Item1.Length > 0)
            {
                index += max.Item1.Length;
                return max.Item2;
            }
            else return text[index++];
        }

        private void FromTextBox1()
        {
            if (!radioButton1.Checked) return;
            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            bool wasHangul = false;
            bool isHangul = false;
            bool isSpace = false;
            bool wasSpace = false;
            foreach(char c in textBox1.Text)
            {
                string s = GetLatin(c, out isHangul);
                isSpace = s == " ";
                if (!isSpace && !wasSpace && (!isFirst && isHangul || wasHangul && !isHangul)) sb.Append('-');
                sb.Append(s);
                wasHangul = isHangul;
                wasSpace = isSpace;
                isFirst = false;
            }
            textBox2.Text = sb.ToString();
        }

        private void FromTextBox2()
        {
            if (!radioButton2.Checked) return;
            string[] split = textBox2.Text.Split('-');
            StringBuilder sb = new StringBuilder();
            foreach (string s in split)
            {
                int index = 0;
                while (index < s.Length) sb.Append(GetHangul(s, ref index));
            }
            textBox1.Text = sb.ToString();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.ReadOnly = false;
            textBox2.ReadOnly = true;
            clearButton1.Visible = true;
            clearButton1.Enabled = true;
            clearButton2.Visible = false;
            clearButton2.Enabled = false;
            FromTextBox1();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.ReadOnly = true;
            textBox2.ReadOnly = false;
            clearButton1.Visible = false;
            clearButton1.Enabled = false;
            clearButton2.Visible = true;
            clearButton2.Enabled = true;
            FromTextBox2();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            FromTextBox1();
            label1.Visible = false;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            FromTextBox2();
            label2.Visible = false;
        }

        private void copyButton1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text != null && textBox1.Text != "")
            {
                label1.Visible = true;
                Clipboard.SetText(textBox1.Text);
            }
            label2.Visible = false;
        }

        private void copyButton2_Click(object sender, EventArgs e)
        {
            if(textBox2.Text != null && textBox2.Text != "")
            {
                label2.Visible = true;
                Clipboard.SetText(textBox2.Text);
            }
            label1.Visible = false;
        }

        private void clearButton1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void clearButton2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.HangulChecked)
            {
                radioButton1.Checked = true;
                textBox1.Text = Properties.Settings.Default.SavedText;
            }
            else
            {
                radioButton2.Checked = true;
                textBox2.Text = Properties.Settings.Default.SavedText;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.HangulChecked = radioButton1.Checked;
            Properties.Settings.Default.SavedText = radioButton1.Checked ? textBox1.Text : textBox2.Text;
            Properties.Settings.Default.Save();
        }
    }
}
