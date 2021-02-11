using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Microsoft.CSharp;
using System.Collections;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;
namespace rpm
{
    public static class rpm
    {

        static string branch = "master";
        static string verison = "2";
        static string currentLang = "";
        static Dictionary<string, Language> languages = JsonConvert.DeserializeObject<Dictionary<string, Language>>(File.ReadAllText(Directory.GetCurrentDirectory() + "/languages.json"));

        public static bool IsValid(string url)
        {
            WebRequest webRequest = WebRequest.Create(url);
            WebResponse webResponse;
            try
            {
                webResponse = webRequest.GetResponse();
            }
            catch //If exception thrown then couldn't get response from address
            {
                return false;
            }
            return true;
        }

        public static void Main(string[] args)
        {
            currentLang = ConfigurationManager.AppSettings.Get("language");
            //Packagetools are experimental, so hands off.
            //packagetools.Setup setup = new packagetools.Setup("Byte");
            //setup.Run();

            //Package pack = new Package("2", new string[] { "package.json" });
            //Console.WriteLine(JsonConvert.SerializeObject(pack));
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/packages/"))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/packages/");

            if (args.Length == 2 && args[0] == "get")
            {
                string packageName = args[1];
                WebClient client = new WebClient();
                if (IsValid("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/package.json") && !File.Exists(Directory.GetCurrentDirectory() + "/packages/" + packageName + ".json"))
                {
                    string packageInfo = client.DownloadString("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/package.json");
                    Package package = JsonConvert.DeserializeObject<Package>(packageInfo);
                    if (package.packageVerison == verison)
                    {
                        foreach (string dep in package.dependencies)
                        {
                            Console.WriteLine(languages[currentLang].phrases["download"].phrase(new string[] { dep, Directory.GetCurrentDirectory() + dep }));
                            client.DownloadFile("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/" + dep, Directory.GetCurrentDirectory() + "/packages/" + dep);
                        }
                        client.DownloadFile("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/package.json", Directory.GetCurrentDirectory() + "/packages/" + packageName + ".json");
                    }
                    else
                    {
                        Console.WriteLine(languages[currentLang].phrases["outdated"].phrase(new string[0]));
                        Console.WriteLine(languages[currentLang].phrases["outdatedinfo"].phrase(new string[] { package.packageVerison, verison }));
                    }

                    
                }
                else
                {
                    if (!IsValid("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/package.json"))
                        Console.WriteLine(languages[currentLang].phrases["notfound"].phrase(new string[] { packageName }));
                    else
                        Console.WriteLine(languages[currentLang].phrases["update"].phrase(new string[] { packageName, packageName }));
                }
            }
            else if (args.Length == 2 && args[0] == "remove")
            {
                string packageName = args[1];
                if (File.Exists(Directory.GetCurrentDirectory() + "/packages/" + packageName + ".json"))
                {
                    string packageInfo = File.ReadAllText(Directory.GetCurrentDirectory() + "/packages/" + packageName + ".json");
                    Package package = JsonConvert.DeserializeObject<Package>(packageInfo);
                    foreach (string dep in package.dependencies)
                    {
                        File.Delete(Directory.GetCurrentDirectory() + "/packages/" + dep);
                    }
                    File.Delete(Directory.GetCurrentDirectory() + "/packages/" + packageName + ".json");
                }
                else
                {
                    Console.WriteLine(languages[currentLang].phrases["notinstalled"].phrase(new string[0]));
                }

            }
            else if (args.Length == 2 && args[0] == "language")
            {
                if (args[1] == "-l")
                {
                    foreach(string lang in languages.Keys)
                    {
                        Console.WriteLine(lang);
                    }
                    return;
                }

                if (languages.ContainsKey(args[1]))
                {
                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["language"].Value = args[1];
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
                    Console.WriteLine(languages[currentLang].phrases["langchange"].phrase(new string[] { args[1] }));
                }
                else
                {
                    Console.WriteLine(languages[currentLang].phrases["nolang"].phrase(new string[] { args[1] }));
                }

            }
            else if (args.Length == 2 && args[0] == "update")
            {
                UpdatePackage(args[1]);
            }
            else
            {
                Console.WriteLine(languages[currentLang].phrases["info"].phrase(new string[0]));
                /*Console.WriteLine("ReCT Package Manager(rpm)");
                Console.WriteLine("Made by the RectSrc team");
                Console.WriteLine("Commands:");
                Console.WriteLine("get [packagename]    -Gets the package called [packagename]");
                Console.WriteLine("remove [packagename]    -Removes the package called [packagename]");
                Console.WriteLine("update [packagename]    -Updates the package called [packagename]");*/
            }
        }

        static void UpdatePackage(string packageName)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "/packages/" + packageName + ".json"))
            {
                string packageInfo = File.ReadAllText(Directory.GetCurrentDirectory() + "/packages/" + packageName + ".json");
                Package package = JsonConvert.DeserializeObject<Package>(packageInfo);
                foreach (string dep in package.dependencies)
                {
                    File.Delete(dep);
                }
                WebClient client = new WebClient();
                if (IsValid("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/package.json") && !File.Exists(Directory.GetCurrentDirectory() + "/packages/" + packageName + ".json"))
                {
                    string packageInfoForDownload = client.DownloadString("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/package.json");
                    Package packageUpdate = JsonConvert.DeserializeObject<Package>(packageInfoForDownload);
                    if (packageUpdate.packageVerison == verison)
                    {
                        foreach (string dep in packageUpdate.dependencies)
                        {
                            client.DownloadFile("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/" + dep, Directory.GetCurrentDirectory() + "/packages/" + dep);
                        }
                        client.DownloadFile("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/package.json", Directory.GetCurrentDirectory() + "/packages/" + packageName + ".json");
                    }
                    else
                    {
                        Console.WriteLine(languages[currentLang].phrases["outdated"].phrase(new string[0]));
                        Console.WriteLine(languages[currentLang].phrases["outdatedinfo"].phrase(new string[] { packageUpdate.packageVerison, verison }));
                    }

                    
                }
                else
                {
                    if (!IsValid("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/package.json"))
                        Console.WriteLine(languages[currentLang].phrases["notfound"].phrase(new string[] { packageName }));
                }
            }
            else
            {
                Console.WriteLine(languages[currentLang].phrases["notinstalled"].phrase(new string[0]));
            }
        }


        [Serializable]
        public class Package
        {

            public string packageVerison;
            public string[] dependencies;
            public Package(string pv, string[] deps)
            {
                packageVerison = pv;
                dependencies = deps;
            }

        }

        public class Language
        {
            public Dictionary<string, Phrase> phrases;

            public Language()
            {
                phrases = new Dictionary<string, Phrase>();
            }
        }

        public class Phrase
        {
            //Example phrase
            public string rootphrase = "downloading {val0} to {val1}";

            public string phrase(string[] values)
            {
                string outPhrase = rootphrase;

                for (int i = 0; i < values.Length; i++)
                    outPhrase = tools.ReplaceFirst(outPhrase, "{val" + i.ToString() + "}", values[i]);
                return outPhrase;
            }

            public Phrase(string text)
            {
                rootphrase = text;
            }
        }
    }

    public static class tools {
        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static void SetSetting(string key, string value)
        {
            Configuration configuration =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}