using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        Dictionary<string, string> femininePrefixes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        Dictionary<string, string> masculinePrefixes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        Dictionary<string, string> feminineWords = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        Dictionary<string, string> masculineWords = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        Dictionary<string, string> words = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        bool checkFem = false;
        bool checkMas = false;

        string src = "";
        public Form1()
        {
            InitializeComponent();
        }
        void Init()
        {
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
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                src = System.IO.File.ReadAllText(openFileDialog.FileName);
                Init();
            }
        }
        void Trans()
        {

            List<string> strs = new List<string>();
            foreach (string b in richTextBox1.Text.Split(" ").Reverse())
            {
                string c = b.Replace("?", " ?").Replace("!", " !").Replace(",", " ,").Replace(".", " .");
                string a = c.Split(" ")[0];
                if (feminineWords.ContainsKey(a))
                {
                    checkFem = true;
                    strs.Add(ReplaceCaseInsensitive(b, a, feminineWords[a]));
                    continue;
                }
                else if (masculineWords.ContainsKey(a))
                {
                    checkMas = true;
                    strs.Add(ReplaceCaseInsensitive(b, a, masculineWords[a]));
                    continue;
                }
                if (checkMas)
                {
                    if (masculinePrefixes.ContainsKey(a))
                    {
                        strs.Add(ReplaceCaseInsensitive(b, a, masculinePrefixes[a]));
                        checkMas = false;
                    }
                }
                else if (checkFem)
                {
                    if (femininePrefixes.ContainsKey(a))
                    {
                        strs.Add(ReplaceCaseInsensitive(b, a, femininePrefixes[a]));
                        checkFem = false;
                    }
                }
                else if (words.ContainsKey(a))
                {
                    strs.Add(ReplaceCaseInsensitive(b, a, words[a]));
                }
                else if (b == " ")
                {
                    continue;
                }
                else
                {
                    strs.Add(b);
                }
            }
            strs = strs.ToArray().Reverse().ToList();
            richTextBox2.Text = string.Join(' ', strs);
        }
        static string ReplaceCaseInsensitive(string Text, string Find, string Replace)
        {
            char[] NewText = Text.ToCharArray();
            int ReplaceLength = Math.Min(Find.Length, Replace.Length);

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
                    for (int i = 0; i < ReplaceLength; i++)
                    {
                        if (NewText.Length > Replace.Length)
                        {
                            NewText = NewText.Take(NewText.Length - 1).ToArray();
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
            Trans();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            Trans();
        }
    }
}
