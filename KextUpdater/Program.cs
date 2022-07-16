using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.IO;
using System.IO.Compression;
using Microsoft.Win32;


HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://raw.githubusercontent.com/NotARobot6969/KextUpdater/master/KextUpdater/kexts.json");
HttpWebResponse response = (HttpWebResponse)request.GetResponse();
string kextJson;
var encoding = Encoding.ASCII;
using (var reader = new StreamReader(response.GetResponseStream(), encoding))
{
    kextJson = reader.ReadToEnd();
}
List<Kexts> kexts = JsonConvert.DeserializeObject<List<Kexts>>(kextJson);
foreach (Kexts currentKext in kexts)
{
    
    string downloadName = "";
    List<string> downloadList = currentKext.Format.Split(',').ToList();
    int versionIndex = 0;
    for (int i = 0; i < downloadList.Count; i++)
    {
        switch (downloadList[i])
        {
            case "Name":
                downloadList[i] = "-" + currentKext.Name;
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
                downloadList[i] = "-" + paths.Last();
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
        DirectoryInfo directory = new DirectoryInfo(downloadName);
        foreach (var dir in directory.EnumerateDirectories())
        {
            if (dir.Name.Contains("dSYM") || !dir.Name.Contains(".kext"))
            {
                Directory.Delete(dir.FullName, true);
            }
        }
    }
}
class Kexts
{
    public string Name { get; set; } = "undefined";
    public string URL { get; set; } = "https://example.com";
    public string Format { get; set; } = "Name,Version,(RELEASE)";
}