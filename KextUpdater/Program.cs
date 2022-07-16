using Newtonsoft.Json;
using System.Net;
using System.Text;

HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://raw.githubusercontent.com/NotARobot6969/KextUpdater/master/kexts.json");
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
    for (int i = 0; i < downloadList.Count; i++)
    {
        switch (downloadList[i])
        {
            case "Name":
                downloadList[i] = "-" + currentKext.Name;
                break;
            case "Version":
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
    Console.WriteLine(downloadName);
}
class Kexts
{
    public string Name { get; set; } = "undefined";
    public string URL { get; set; } = "https://example.com";
    public string Format { get; set; } = "Name,Version,(RELEASE)";
}