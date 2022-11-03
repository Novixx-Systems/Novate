using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Novate
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> masculinePrefixes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        Dictionary<string, string> femininePrefixes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        Dictionary<string, string> masculineWords = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        Dictionary<string, string> feminineWords = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        Dictionary<string, string> defaultfirst = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        Dictionary<string, string> words = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        Dictionary<string, string> none = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        bool checkFem = false;
        bool checkMas = false;

        string src = "";
        public Form1()
        {
            InitializeComponent();
        }
        void Init()
        {
            masculinePrefixes.Clear();
            femininePrefixes.Clear();
            masculineWords.Clear();
            feminineWords.Clear();
            defaultfirst.Clear();
            words.Clear();
            none.Clear();

            JArray o = JArray.Parse(src);
            foreach (JObject item in o) // <-- Note that here we used JObject instead of usual JProperty
            {
                string name = item.GetValue("englishword").ToString();
                string name2 = item.GetValue("translatedword").ToString();
                string type = item.GetValue("type").ToString();
                if (type == "prefixFeminine")
                {
                    femininePrefixes.Add(name, name2);
                }
                else if (type == "prefixMasculine")
                {
                    masculinePrefixes.Add(name, name2);
                    words.Add(name, name2);
                }
                else if (type == "masculine")
                {
                    masculineWords.Add(name, name2);
                }
                else if (type == "feminine")
                {
                    feminineWords.Add(name, name2);
                }
                else if (type == "default")
                {
                    words.Add(name, name2);
                }
                else if (type == "none")
                {
                    none.Add(name, name2);
                }
                else if (type == "defaultfirst")
                {
                    defaultfirst.Add(name, name2);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Novate Packages (*.napkg)|*.napkg";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string temp = GetTemporaryDirectory();
                System.IO.Compression.ZipFile.ExtractToDirectory(openFileDialog.FileName, temp);
                src = System.IO.File.ReadAllText(temp + "\\words.json");
                label2.Text = File.ReadAllText(temp + "\\package.json");
                Init();
            }
        }
        public string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
        void Trans()
        {
            checkFem = false;
            checkMas = false;
            List<string> strs = new List<string>();
            int index = 0;
            string stroText = richTextBox1.Text;
            foreach (string str in defaultfirst.Keys)
            {
                stroText = stroText.Replace(str, defaultfirst[str], StringComparison.OrdinalIgnoreCase);
            }
            stroText = " " + stroText;
            stroText = stroText.Replace("\"", " \"").Replace("?", " ?").Replace("!", " !").Replace(",", " ,").Replace(".", " .").Replace(":", " :").Replace("\n", " \n ");
            string[] toTrans = stroText.Split(" ");
            foreach (string b in toTrans.Reverse())
            {
                index += 1;
                string a = b;
                if (a == "," || a == "." || a == "?" || a == "!" || a == ":")
                {
                    checkFem = false;
                    checkMas = false;
                }
                if (feminineWords.ContainsKey(a))
                {
                    checkFem = true;
                    strs.Add(ReplaceCaseInsensitive(a, a, feminineWords[a]));
                    continue;
                }
                else if (masculineWords.ContainsKey(a))
                {
                    checkMas = true;
                    strs.Add(ReplaceCaseInsensitive(a, a, masculineWords[a]));
                    continue;
                }
                if (checkMas && masculinePrefixes.ContainsKey(a))
                {
                    strs.Add(ReplaceCaseInsensitive(a, a, masculinePrefixes[a]));
                    checkMas = false;
                }
                else if (checkFem && femininePrefixes.ContainsKey(a))
                {
                    strs.Add(ReplaceCaseInsensitive(a, a, femininePrefixes[a]));
                    checkMas = false;
                }
                else if (words.ContainsKey(a))
                {
                    if (words[a] == " ")
                    {
                        continue;
                    }
                    strs.Add(ReplaceCaseInsensitive(a, a, words[a]));
                }
                else if (none.ContainsKey(a) && index != toTrans.Length)
                {
                    strs.Add(ReplaceCaseInsensitive(a, a, none[a]));
                }
                else
                {
                    if (none.ContainsKey(a))
                    {
                        continue;
                    }
                    strs.Add(a);
                }
            }
            strs = strs.ToArray().Reverse().ToList();
            richTextBox2.Text = string.Join(' ', strs).Replace(" \"", "\"").Replace(" ?", "?").Replace(" !", "!").Replace(" ,", ",").Replace(" .", ".").Replace(" :", ":");
        }
        static string ReplaceCaseInsensitive(string Text, string Find, string Replace)
        {
            char[] NewText = Text.Replace("?", "").Replace("!", "").Replace(",", "").Replace(".", "").Replace(":", "").ToCharArray();
            int ReplaceLength = Replace.Length;

            int LastIndex = -1;
            while (true)
            {
                LastIndex = Text.IndexOf(Find, LastIndex + 1, StringComparison.CurrentCultureIgnoreCase);

                if (LastIndex == -1)
                {
                    break;
                }
                else
                {
                    if (NewText.Length > Replace.Length)
                    {
                        NewText = NewText.Take(NewText.Length - 1).ToArray();
                        Text.Remove(Text.Length - 1);
                    }
                    else if (Replace.Length > NewText.Length)
                    {
                        NewText = new List<char>(NewText) { ' ' }.ToArray();
                        Text += " ";
                    }
                    for (int i = 0; i < ReplaceLength; i++)
                    {
                        if (NewText.Length > Replace.Length)
                        {
                            NewText = NewText.Take(NewText.Length - 1).ToArray();
                            Text.Remove(Text.Length - 1);
                        }
                        else if (Replace.Length > NewText.Length)
                        {
                            NewText = new List<char>(NewText) { ' ' }.ToArray();
                            Text += " ";
                        }

                        if (char.IsUpper(Text[i + LastIndex]))
                            NewText[i + LastIndex] = char.ToUpper(Replace[i]);
                        else
                            NewText[i + LastIndex] = char.ToLower(Replace[i]);
                    }
                }
            }

            return new string(NewText);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not implemented!!");
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            Trans();
        }
    }
}
