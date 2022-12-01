using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

class Program
{
    static string ExecuteCommand(string command)
    {
        ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + command)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = $"{Directory.GetCurrentDirectory()}/platform-tools"
        };
        using (Process proc = new Process())
        {
            proc.StartInfo = procStartInfo;
            proc.Start();
            string output = proc.StandardOutput.ReadToEnd();
            if (string.IsNullOrEmpty(output)) output = proc.StandardError.ReadToEnd();
            return output;
        }
    }
    static List<Item> LoadJson()
    {
        using (StreamReader r = new StreamReader("mobile_config.json"))
        {
            string json = r.ReadToEnd();
            List<Item> items = JsonConvert.DeserializeObject<Devices>(json).devices;
            return items;
        }
    }
    static void Main()
    {
        List<Item> items = LoadJson();
        List<string> adb_ids = new();
        foreach(var item in items) adb_ids.Add(item.adb_id.ToLower());
        List<string> str = ExecuteCommand("adb devices").Split("\r\n").ToList();
        str.RemoveAt(0);
        str.RemoveAll(i => i == "");
        int size = str.Count;
        for (int i = 0; i < size; i++)
        {
            str[i] = str[i].Split("\t")[0].ToLower();
            if (adb_ids.Contains(str[i]))
            {
                var item = items.Find(j => j.adb_id == str[i]);
                if (item.id < 10) Console.WriteLine(ExecuteCommand($"adb -s {item.adb_id} forward tcp:770{item.id} localabstract:chrome_devtools_remote"));
                else Console.WriteLine(ExecuteCommand($"adb -s {item.adb_id} forward tcp:77{item.id} localabstract:chrome_devtools_remote"));
            }
        }  
        Console.ReadKey();
    }
}
public class Item
{
    public int id;
    public string adb_id;
}
public class Devices
{
    public List<Item> devices;
}

