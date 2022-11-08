// See https://aka.ms/new-console-template for more information
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
bool dotext = false;
if (args.Length == 0)
{
    dotext = true;
}
string Src;
string Dest;
int Seed;
if (args.Length == 3)
{
    Src = args[0];
    Dest = args[1];
    string str = CreateMD5(args[2]);
    Seed = Convert.ToInt32(str.Substring(0, str.Length / 4), 16);
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

foreach (string s in srci/* .Replace("?", " ?").Replace("!", " !").Replace(".", " .").Replace(",", " ,").Replace("\"", " \" ") */.Split(' ', '.', '?', '!', ',', '\r', '\n', '(', ')', '-', '\'', '"'))
{
    lstSRC.Add(s);
}
foreach (string s in desti/* .Replace("?", " ?").Replace("!", " !").Replace(".", " .").Replace(",", " ,").Replace("\"", " \" ") */.Split(' ', '.', '?', '!', ',', '\r', '\n', '(', ')', '-', '\'', '"'))
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
for (int i = 0; i < lstSRC.Count; i++)
{
    int theS = 0;
    if (srcfreq.ContainsKey(lstSRC[i - poop]))
    {
        theS = srcfreq[lstSRC[i - poop]];
    }
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
string outputJSON = "[\r\n";
List<string> duplicateAvoider = new List<string>();
foreach (string guess in guesses.Keys)
{
    if (duplicateAvoider.Contains(guess.ToLower()))
    {
        continue;
    }
    outputJSON += " {\r\n \"englishword\": \"" + guess.ToLower() + "\",\r\n \"translatedword\": \"" + guesses[guess].Item1.ToLower() + "\",\r\n \"type\": \"default\"\r\n },";
    duplicateAvoider.Add(guess.ToLower());
}
outputJSON += "]\r\n";

Console.WriteLine(outputJSON);

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