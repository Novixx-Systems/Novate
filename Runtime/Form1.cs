using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpeechLib;

namespace Novate
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> masculinePrefixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> femininePrefixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> masculineWords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> feminineWords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> defaultfirst = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> words = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> none = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        bool checkFem = false;
        bool checkMas = false;

        string src = "";
        string pron = "";
        string toktyp = "";
        public static int sed = 0;
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
            tokens.Clear();

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
        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
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
                pron = System.IO.File.ReadAllText(temp + "\\pronouncer.dat");
                toktyp = System.IO.File.ReadAllText(temp + "\\tokentype.dat");
                string str = CreateMD5(toktyp);
                sed = Convert.ToInt32(str.Substring(0, str.Length / 4), 16);
                if (toktyp == "0")
                {
                    sed = 0;
                }
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
            stroText = stroText.Replace("’", "'").Replace("…", "...").Replace("“", "\"").Replace("”", "\""); // Comment out if annoying
            foreach (string str in defaultfirst.Keys)
            {
                stroText = stroText.Replace(str, defaultfirst[str], StringComparison.OrdinalIgnoreCase);
            }
            stroText = " " + stroText;
            stroText = stroText.Replace("(", "( ").Replace(")", " )").Replace("-", " - ").Replace("\"", " \" ").Replace("?", " ?").Replace("!", " !").Replace(",", " ,").Replace(".", " .").Replace(":", " :").Replace("\n", " \n ");
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
                    if (a == " " || a == null || a == "")
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
            richTextBox2.Text = string.Join(' ', strs).Replace(" - ", "-").Replace("( ", "(").Replace(" )", ")").Replace(" \" ", "\"").Replace(" ?", "?").Replace(" !", "!").Replace(" ,", ",").Replace(" .", ".").Replace(" :", ":");
            string strsa = "";
            foreach (string str in richTextBox2.Text.Split("\n"))
            {
                List<string> list = str.Split(" ").ToList();
                list.Shuffle();
                strsa += string.Join(" ", list.ToArray()) + "\n";
            }
            richTextBox2.Text = strsa;
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
            string toSpeak = richTextBox2.Text;
            string[] a = pron.Split("=");
            try
            {
                for (int i = 0; i < a.Length; i++)
                {
                    toSpeak = toSpeak.Replace(a[i], a[i + 1], StringComparison.OrdinalIgnoreCase);
                    i++;
                }
            }
            catch
            {
            }
            toSpeak = toSpeak.Replace("sce", "ske");
            ISpeechVoice voice = new SpVoice();
            voice.Rate = 2;
            voice.Volume = 100;
            voice.Speak("<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>"
                        + toSpeak
                        + "</speak>",
                        SpeechVoiceSpeakFlags.SVSFlagsAsync | SpeechVoiceSpeakFlags.SVSFIsXML);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            Trans();

        }
        static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes); // .NET 5 +

                // Convert the byte array to hexadecimal string prior to .NET 5
                // StringBuilder sb = new System.Text.StringBuilder();
                // for (int i = 0; i < hashBytes.Length; i++)
                // {
                //     sb.Append(hashBytes[i].ToString("X2"));
                // }
                // return sb.ToString();
            }
        }
    }
    static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            if (Form1.sed == 0)
            {
                return;
            }
            int n = list.Count;
            Random rnd = new Random(Form1.sed);
            while (n > 1)
            {
                if (rnd.Next(1,16) != 4) { continue; }
                if (list[n-1].ToString() == "" || list[n-1].ToString() == " ") { n--;  continue;  }
                int k = (rnd.Next(0, n) % n);
                n--; if (char.IsUpper(list[n].ToString()[0]) || char.IsUpper(list[k].ToString()[0]) || list[n].ToString().Contains(".") || list[n].ToString().Contains("\"") || list[n].ToString().Contains("?") || list[n].ToString().Contains("!")) continue;
                (list[n], list[k]) = (list[k], list[n]);
            }
        }
    }
}
