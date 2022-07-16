using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.IO;
using System.IO.Compression;
using Microsoft.Win32;
/*
 KextUpdater
 
 A (poorly written) tool written to update kexts (kernel extensions) on a hackintosh.
 Easily some of the worst code I have written in my life, but it seems to work.

 Authored by Not a Robot in 2022
*/
//Console.Write("Please drag and drop your kexts folder onto the console window: ");
// string kextdir = Console.ReadLine();
string kextdir = @"C:\Users\Shaun\Downloads\Lenovo-ThinkPad-X1C7-OC-Hackintosh-master(1)\Lenovo-ThinkPad-X1C7-OC-Hackintosh-master\EFI\OC\Kexts";
if (!Directory.Exists(kextdir))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Invalid directory: exiting...");
    Console.ForegroundColor = ConsoleColor.Gray;
    Environment.Exit(1);
}
HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://raw.githubusercontent.com/NotARobot6969/KextUpdater/master/KextUpdater/kexts.json"); // i get that this is a bad
HttpWebResponse response = (HttpWebResponse)request.GetResponse();                                                                                       // solution, github wont last
string kextJson;                                                                                                                                         // forever but idc lol
var encoding = Encoding.ASCII;
using (var reader = new StreamReader(response.GetResponseStream(), encoding))
{
    kextJson = reader.ReadToEnd();
}
List<Kexts> kexts = JsonConvert.DeserializeObject<List<Kexts>>(kextJson);
List<string> names = new List<string>();

foreach (Kexts kext in kexts)
{
    names.Add(kext.Name);
}

List<string> dirs = new List<string>(Directory.EnumerateDirectories(kextdir));
foreach (string curdir in dirs)
{
    string fileName = $"{curdir.Substring(curdir.LastIndexOf(Path.DirectorySeparatorChar) + 1)}";
    string kextName = fileName.Substring(0, fileName.Length - 5);
    if (names.Contains(kextName)) 
    {
        try
        {
            int index = -1;
            foreach (Kexts kext in kexts)
            {
                if (kext.Name == kextName)
                    index = kexts.IndexOf(kext);
            }
            Kexts currentKext = kexts[index];
            string downloadName = "";
            string seperator = "-";
            string versionprefix = "";
            List<string> downloadList = kexts[index].Format.Split(',').ToList();
            int versionIndex = 0;
            for (int i = 0; i < downloadList.Count; i++)
            {
                switch (downloadList[i])
                {
                    case "_":
                        seperator = "_";
                        break;
                    case "v":
                        versionprefix = "v";
                        break;
                    case "Name":
                        downloadList[i] = seperator + currentKext.Name;
                        break;
                    case "Version":
                        versionIndex = i;
                        string url = currentKext.URL + "releases/latest";
                        HttpWebRequest versionRequest = (HttpWebRequest)WebRequest.Create(url);
                        versionRequest.Method = "GET";
                        HttpWebResponse versionResponse = (HttpWebResponse)versionRequest.GetResponse();
                        Uri loc = versionResponse.ResponseUri;
                        string pathandquery = loc.PathAndQuery;
                        List<string> paths = pathandquery.Split("/").ToList();
                        downloadList[i] = seperator + versionprefix + paths.Last();
                        break;

                }
                if (downloadList[i].Substring(0, 1) == "(")
                {
                    string replaceString = downloadList[i].Substring(1, downloadList[i].Length - 2);
                    downloadList[i] = "-" + replaceString;
                }
                downloadName += downloadList[i];
            }
            downloadName = downloadName.Substring(1);
            using (var client = new WebClient())
            {
                string tmp = Path.GetTempPath();
                if (Directory.Exists(tmp + @"\" + downloadName))
                {
                    Directory.Delete(tmp + @"\" + downloadName, true);
                }
                string downloadUri = currentKext.URL + "releases/download/" + downloadList[versionIndex].Substring(1) + "/" + downloadName + ".zip";
                client.DownloadFile(downloadUri, tmp + @"\" + downloadName + ".zip");
                ZipFile.ExtractToDirectory(tmp + @"\" + downloadName + ".zip", tmp + @"\" + downloadName);
                File.Delete(tmp + @"\" + downloadName + ".zip");
                DirectoryInfo directory = new DirectoryInfo(tmp + @"\" + downloadName);
                foreach (var dir in directory.EnumerateDirectories())
                {
                    if (dir.Name.Contains("dSYM") || !dir.Name.Contains(".kext"))
                    {
                        Directory.Delete(dir.FullName, true);
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Kext " + kextName + " was updated successfully!");

        } catch
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Kext " + kextName + " was detected, but was unable to update. Please report which kext this is as a GitHub issue!");
        }
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(kextName + " was not found in the JSON list. Please create a pull request to add it or an issue with details!");
    }
}
Console.ForegroundColor = ConsoleColor.Gray;
class Kexts
{
    public string Name { get; set; } = "undefined";
    public string URL { get; set; } = "https://example.com";
    public string Format { get; set; } = "Name,Version,(RELEASE)";
}