// See https://aka.ms/new-console-template for more information
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

bool dotext = false;
if (args.Length == 0)
{
    dotext = true;
}
else
{
    if (args[0] == "--cryloc")
    {
        ConvertToCustom();
    }
}
string Src;
string Dest;
int Seed;
bool pkg = false;
bool rev = false;
string pron = "";
if (args.Length >= 4)
{
    if (args[3] == "rev")
    {
        rev = true;
    }
    Src = args[0];
    Dest = args[1];
    string str = CreateMD5(args[2]);
    Seed = Convert.ToInt32(str.Substring(0, str.Length / 4), 16);
    if (args.Length > 5)
    {
        if (args[4] == "--create-pkg")
        {
            pkg = true;
        }
        pron += args[5];
    }
    else
    {
        Console.WriteLine("warn: need another arg for pronounciation");
    }
}
else
{
    Console.WriteLine("SRC: ");
    Src = Console.ReadLine() + "";
    Console.WriteLine("DEST: ");
    Dest = Console.ReadLine() + "";
    Console.WriteLine("SEED: ");
    string str = CreateMD5(Console.ReadLine() + "");
    Seed = Convert.ToInt32(str.Substring(0, str.Length / 4), 16);
}

string srci = File.ReadAllText(Src);
string desti = File.ReadAllText(Dest);
if (dotext) Console.WriteLine("/* Pass 1: scan texts */");

if (srci == desti)
{
    Console.WriteLine("ERROR: Cannot 'GEN' a duplicate");
    Environment.Exit(1);
}

List<string> lstSRC = new List<string>();
List<string> lstDest = new List<string>();

List<string> lstSRCs = new List<string>();
List<string> lstDests = new List<string>();
srci = Regex.Replace(srci, "//.*\r\n", "");
desti = Regex.Replace(desti, "//.*\r\n", "");
foreach (string s in srci/* .Replace("?", " ?").Replace("!", " !").Replace(".", " .").Replace(",", " ,").Replace("\"", " \" ") */.Split(' ', '.', '?', '!', ',', '\r', '\n', '(', ')', '-', '"'))
{
    lstSRC.Add(s);
}
foreach (string s in desti/* .Replace("?", " ?").Replace("!", " !").Replace(".", " .").Replace(",", " ,").Replace("\"", " \" ") */.Split(' ', '.', '?', '!', ',', '\r', '\n', '(', ')', '-', '"'))
{
    lstDest.Add(s);
}
foreach (string s in srci/* .Replace("?", " ?").Replace("!", " !").Replace(".", " .").Replace(",", " ,").Replace("\"", " \" ") */.Split('.', '?', '!', ',', '\r', '\n', '(', ')', '-'))
{
    lstSRCs.Add(s);
}
foreach (string s in desti/* .Replace("?", " ?").Replace("!", " !").Replace(".", " .").Replace(",", " ,").Replace("\"", " \" ") */.Split('.', '?', '!', ',', '\r', '\n', '(', ')', '-'))
{
    lstDests.Add(s);
}

if (dotext) Console.WriteLine("/* Pass 2: detect grammar #1 */");
Dictionary<string, int> srcfreq = GetWordFrequency(srci);
Dictionary<string, int> destfreq = GetWordFrequency(desti);
Dictionary<string, (string, int)> guesses = new Dictionary<string, (string, int)>();
Random rand = new Random(Convert.ToInt32(Seed));
for (int i = 0; i < 2500; i++)
{
    rand.Next(i * 1000);
}
if (dotext) Console.WriteLine("/* Pass 3: detect grammar #2 */");

int poop = 0;
using (var progress = new ProgressBar())
{
    for (int i = 0; i < lstSRC.Count; i++)
    {
        progress.Report((double)i / lstSRC.Count);
        try
        {
            int theS = 0;
            if (srcfreq.ContainsKey(lstSRC[i - poop]))
            {
                theS = srcfreq[lstSRC[i - poop]];
            }
            if (lstSRC[i - poop] == "")
            {
                continue;
            }
            // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

            if (lstSRC[i - poop].StartsWith("{[3]"))
            {
                if (lstDest.Count > i && lstDest[i].StartsWith("{[3]"))
                {
                    lstDest[i] = lstDest[i].Substring(4);
                    lstSRC[i - poop] = lstSRC[i - poop].Substring(4);
                    if (guesses.ContainsKey(lstSRC[i - poop] + " " + lstSRC[i + 1 - poop] + " " + lstSRC[i + 2 - poop]))
                    {
                        guesses[lstSRC[i - poop] + " " + lstSRC[i + 1 - poop] + " " + lstSRC[i + 2 - poop]] = (guesses[lstSRC[i - poop] + " " + lstSRC[i + 1 - poop] + " " + lstSRC[i + 2 - poop]].Item1, guesses[lstSRC[i - poop] + " " + lstSRC[i + 1 - poop] + " " + lstSRC[i + 2 - poop]].Item2 + 1);
                        goto nope;
                    }
                    guesses.Add(lstSRC[i - poop] + " " + lstSRC[i + 1 - poop] + " " + lstSRC[i + 2 - poop], (lstDest[i] + " " + lstDest[i + 1] + " " + lstDest[i + 2], 1));
                nope:
                    lstDest.RemoveAt(i);
                    lstSRC.RemoveAt(i - poop);
                    i++;
                    continue;
                }
                lstSRC[i - poop] = lstSRC[i - poop].Substring(4);
                if (guesses.ContainsKey(lstSRC[i - poop] + " " + lstSRC[i + 1 - poop] + " " + lstSRC[i + 2 - poop]))
                {
                    guesses[lstSRC[i - poop] + " " + lstSRC[i + 1 - poop] + " " + lstSRC[i + 2 - poop]] = (guesses[lstSRC[i - poop] + " " + lstSRC[i + 1 - poop] + " " + lstSRC[i + 2 - poop]].Item1, guesses[lstSRC[i - poop] + " " + lstSRC[i + 1 - poop] + " " + lstSRC[i + 2 - poop]].Item2 + 1);
                    goto no;
                }
                guesses.Add(lstSRC[i - poop] + " " + lstSRC[i + 1 - poop] + " " + lstSRC[i + 2 - poop], (lstDest[i], 1));
            no:
                lstDest.RemoveAt(i);
                poop--;
                poop--;
                poop--;
                continue;
            }
            if (lstDest.Count > i && lstDest[i].StartsWith("{[3]"))
            {
                if (lstSRC[i - poop].StartsWith("{[3]"))
                {
                    lstDest[i] = lstDest[i].Substring(4);
                    lstSRC[i - poop] = lstSRC[i - poop].Substring(4);
                    if (guesses.ContainsKey(lstDest[i] + " " + lstDest[i + 1] + " " + lstDest[i + 2]))
                    {
                        guesses[lstDest[i] + " " + lstDest[i + 1] + " " + lstDest[i + 2]] = (guesses[lstDest[i] + " " + lstDest[i + 1] + " " + lstDest[i + 2]].Item1, guesses[lstDest[i] + " " + lstDest[i + 1] + " " + lstDest[i + 2]].Item2 + 1);
                        goto nope;
                    }
                    guesses.Add(lstDest[i] + " " + lstDest[i + 1] + " " + lstDest[i + 2], (lstSRC[i - poop] + " " + lstSRC[i + 1 - poop] + " " + lstSRC[i + 2 - poop], 1));
                nope:
                    lstDest.RemoveAt(i);
                    lstSRC.RemoveAt(i - poop);
                    i++;
                    continue;
                }
                lstDest[i] = lstDest[i].Substring(4);
                if (guesses.ContainsKey(lstSRC[i - poop]))
                {
                    guesses[lstSRC[i - poop]] = (guesses[lstSRC[i - poop]].Item1, guesses[lstSRC[i - poop]].Item2 + 1);
                    goto no;
                }
                guesses.Add(lstSRC[i - poop], (lstDest[i] + " " + lstDest[i + 1] + " " + lstDest[i + 2], 1));
            no:
                lstSRC.RemoveAt(i - poop);
                poop++;
                poop++;
                poop++;
                continue;
            }
            // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
            if (lstSRC[i - poop].StartsWith("{[2]"))
            {
                lstSRC[i - poop] = lstSRC[i - poop].Substring(4);
                if (guesses.ContainsKey(lstSRC[i - poop] + " " + lstSRC[i + 1 - poop]))
                {
                    guesses[lstSRC[i - poop] + " " + lstSRC[i + 1 - poop]] = (guesses[lstSRC[i - poop] + " " + lstSRC[i + 1 - poop]].Item1, guesses[lstSRC[i - poop] + " " + lstSRC[i + 1 - poop]].Item2 + 1);
                    goto no;
                }
                guesses.Add(lstSRC[i - poop] + " " + lstSRC[i + 1 - poop], (lstDest[i], 1));
            no:;
                lstDest.RemoveAt(i);
                poop--;
                poop--;
                continue;
            }
            if (lstDest.Count > i && lstDest[i].StartsWith("{[2]"))
            {
                lstDest[i] = lstDest[i].Substring(4);
                if (guesses.ContainsKey(lstSRC[i - poop]))
                {
                    guesses[lstSRC[i - poop]] = (guesses[lstSRC[i - poop]].Item1, guesses[lstSRC[i - poop]].Item2 + 1);
                    goto no;
                }
                guesses.Add(lstSRC[i - poop], (lstDest[i] + " " + lstDest[i + 1], 1));
            no:;
                lstSRC.RemoveAt(i - poop);
                poop++;
                poop++;
                continue;
            }
            if (lstDest.Count > i && lstSRC.Contains(lstDest[i]))
            {
                continue;
            }
            if (lstDest.Count > i && lstSRC[i - poop] == lstDest[i])
            {
                if (guesses.ContainsKey(lstSRC[i - poop]))
                {
                    guesses[lstSRC[i - poop]] = (guesses[lstSRC[i - poop]].Item1, guesses[lstSRC[i - poop]].Item2 + 1);
                    goto no;
                }
                guesses.Add(lstSRC[i - poop], (lstDest[i], 1));
            no:;
            }
            else if (destfreq.ContainsValue(theS))
            {
                if (!guesses.ContainsKey(lstSRC[i - poop]))
                {
                    guesses.Add(lstSRC[i - poop], (lstDest[i], i));
                    goto no;
                }
                if (guesses.ContainsKey(lstSRC[i - poop]))
                {
                    guesses[lstSRC[i - poop]] = (guesses[lstSRC[i - poop]].Item1, guesses[lstSRC[i - poop]].Item2 + 1);
                    goto no;
                }
                guesses.Add(lstSRC[i - poop], (destfreq.FirstOrDefault(x => x.Value == theS).Key, 1));
            no:;
            }
            if (lstDest.Count > i && lstSRC[i - poop].Length == lstDest[i].Length)
            {
                if (guesses.ContainsKey(lstSRC[i - poop]))
                {
                    guesses[lstSRC[i - poop]] = (guesses[lstSRC[i - poop]].Item1, guesses[lstSRC[i - poop]].Item2 + 1);
                    goto no;
                }
                guesses.Add(lstSRC[i - poop], (lstDest[i], 1));
            no:;
            }
            if (lstDest.Count > i && lstSRC[i - poop].StartsWith(lstDest[i]))
            {
                if (guesses.ContainsKey(lstSRC[i - poop]))
                {
                    guesses[lstSRC[i - poop]] = (guesses[lstSRC[i - poop]].Item1, guesses[lstSRC[i - poop]].Item2 + 1);
                    goto no;
                }
                guesses.Add(lstSRC[i - poop], (lstDest[i], 1));
            no:;
            }
            try
            {
                guesses.Add(lstSRC[i - poop], (lstDest[i], 1));
            }
            catch
            {

            }
        }
        catch
        {
            continue;
        }
    }
}
string outputJSON = "[\r\n";
List<string> duplicateAvoider = new List<string>();
foreach (string a in guesses.Keys)
{
    if (a.Contains(" "))
    {
        guesses = ReorderDictionary(guesses, a);
    }
}
if (rev)
{
    foreach ((string, int) a in guesses.Values)
    {
        if (a.Item1.Contains(" "))
        {
            guesses = ReorderDictionary2(guesses, a);
        }
    }
}
foreach (string guess in guesses.Keys)
{
    if (duplicateAvoider.Contains(guess.ToLower()))
    {
        continue;
    }
    if (guesses[guess].Item1.Contains(" "))
    {
        outputJSON += " {\r\n \"englishword\": \"" + (rev ? guesses[guess].Item1.ToLower() : guess.ToLower()) + "\",\r\n \"translatedword\": \"" + (!rev ? guesses[guess].Item1.ToLower() : guess.ToLower()) + "\",\r\n \"type\": \"default\"\r\n },";
    }
    else if (guess.Contains(" "))
    {
        outputJSON += " {\r\n \"englishword\": \"" + (rev ? guesses[guess].Item1.ToLower() : guess.ToLower()) + "\",\r\n \"translatedword\": \"" + (!rev ? guesses[guess].Item1.ToLower() : guess.ToLower()) + "\",\r\n \"type\": \"defaultfirst\"\r\n },";
    }
    else
    {
        outputJSON += " {\r\n \"englishword\": \"" + (rev ? guesses[guess].Item1.ToLower() : guess.ToLower()) + "\",\r\n \"translatedword\": \"" + (!rev ? guesses[guess].Item1.ToLower() : guess.ToLower()) + "\",\r\n \"type\": \"default\"\r\n },";
    }
    duplicateAvoider.Add(guess.ToLower());
}
outputJSON += "]\r\n";

if (!pkg)
{
    File.WriteAllText("out.json", outputJSON);
    Console.WriteLine("Output written to OUT.JSON");
}
else
{
    Console.WriteLine("ENTER PACKAGE NAME: ");
    string pkgname = Console.ReadLine() + "";
    File.WriteAllText("package.json", pkgname);
    File.WriteAllText("words.json", outputJSON);
    File.WriteAllText("pronouncer.dat", pron);
    File.WriteAllText("tokentype.dat", "0");
    Process.Start("powershell", "Get-ChildItem -Path package.json, words.json, pronouncer.dat, tokentype.dat | Compress-Archive -Force -DestinationPath lang-" + pkgname.Replace(" ", "_") + (rev ? "-rev" : "") + ".zip");
    Thread.Sleep(5000);
    if (File.Exists("lang-" + pkgname.Replace(" ", "_") + (rev ? "-rev" : "") + ".napkg"))
    {
        File.Delete("lang-" + pkgname.Replace(" ", "_") + (rev ? "-rev" : "") + ".napkg");
    }
    File.Move("lang-" + pkgname.Replace(" ", "_") + (rev ? "-rev" : "") + ".zip", "lang-" + pkgname.Replace(" ", "_") + (rev ? "-rev" : "") + ".napkg");
    File.Delete("package.json");
    File.Delete("words.json");
    File.Delete("tokentype.dat");
    File.Delete("pronouncer.dat");
    Console.WriteLine("Output written to lang-" + pkgname.Replace(" ", "_") + (rev ? "-rev" : "") + ".napkg");
}

static Dictionary<string, int> GetWordFrequency(string file)
{
    return file.Split('\n')
               .SelectMany(x => x.Split())
               .Where(x => x != string.Empty)
               .GroupBy(x => x)
               .ToDictionary(x => x.Key, x => x.Count());
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
static Dictionary<string, (string, int)> ReorderDictionary(Dictionary<string, (string, int)> originalDictionary, string newTopItem)
{
    // Initialize ordered dictionary with new top item.
    var reorderedDictionary = new Dictionary<string, (string, int)> { { newTopItem, originalDictionary[newTopItem] } };
    foreach (var item in originalDictionary)
    {
        if (item.Key == newTopItem)
        {
            // Skip the new top item.
            continue;
        }

        reorderedDictionary.Add(item.Key, item.Value);
    }

    return reorderedDictionary;
}
static Dictionary<string, (string, int)> ReorderDictionary2(Dictionary<string, (string, int)> originalDictionary, (string, int) newTopItem)
{
    // Initialize ordered dictionary with new top item.
    var reorderedDictionary = new Dictionary<string, (string, int)> { { originalDictionary.FirstOrDefault(x => x.Value == newTopItem).Key, newTopItem } };
    foreach (var item in originalDictionary)
    {
        if (item.Value == newTopItem)
        {
            // Skip the new top item.
            continue;
        }

        reorderedDictionary.Add(item.Key, item.Value);
    }

    return reorderedDictionary;
}
static void ConvertToCustom()
{
    Random RANDOM = new Random();
    string cyrillicCharacters = "авгдезиклмнопрстухцчшщъяә";
    string eng = File.ReadAllText("in.txt");
    Dictionary<string, string> log = new Dictionary<string, string>();
    int index = -1;
    string newf = "";
    int cnt = eng.Split("\r\n").Length - 1;
    using (var progress = new ProgressBar())
    {
        foreach (string sentence in eng.Split("\r\n"))
        {
            index++;
            progress.Report((double)index / cnt);
            foreach (string word in sentence.Split(' ', '.', '?', '!', ',', '(', ')', '[', ']', ';', '"'))
            {
                if (word == "")
                {
                    continue;
                }
            probeer:
                int worldLength = RANDOM.Next((int)Math.Round((decimal)word.Length / 2), word.Length + 2);
                string chars = cyrillicCharacters;
                string world = new string(Enumerable.Repeat(chars, worldLength)
                    .Select(s => s[RANDOM.Next(s.Length)]).ToArray());
                if (world == "")
                {
                    newf += word + " ";
                    continue;
                }
                if (log.ContainsValue(world.ToLower()))
                {
                    goto probeer;
                }
                if (log.ContainsKey(word.ToLower()))
                {
                    newf += log[word.ToLower()] + " ";
                    continue;
                }
                if (log.ContainsKey(word.ToLower().Substring(0, word.Length - 1)))
                {
                    newf += log[word.ToLower().Substring(0, word.Length - 1)] + "ә ";
                    continue;
                }
                if (word.Length > 1 && log.ContainsKey(word.ToLower().Substring(0, word.Length - 2)))
                {
                    newf += log[word.ToLower().Substring(0, word.Length - 2)] + "ух ";
                    continue;
                }
                log.Add(word.ToLower(), world.ToLower());
                newf += log[word.ToLower()] + " ";
            }
            newf += "\r\n";
        }
    }
    File.WriteAllText("in.txt.new", newf);
    Environment.Exit(0);
}