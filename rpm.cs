﻿using System;
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
using nocompress;
namespace rpm
{
    public static class rpm
    {

        static string branch = "master";
        static string verison = "2";
        static string currentLang = "";
        static Dictionary<string, Language> languages;
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
            if (!File.Exists("languages.json"))
            {
                WebClient client = new WebClient();
                client.DownloadFile("https://raw.githubusercontent.com/RectSrc/rpm/master/languages.json", Directory.GetCurrentDirectory() + "/languages.json");
            }
            languages = JsonConvert.DeserializeObject<Dictionary<string, Language>>(File.ReadAllText(Directory.GetCurrentDirectory() + "/languages.json"));
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
                            Console.WriteLine(Language.GetPhrase("download").phrase(new string[] { dep, Directory.GetCurrentDirectory() + dep }));
                            client.DownloadFile("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/" + dep, Directory.GetCurrentDirectory() + "/packages/" + dep);
                        }
                        client.DownloadFile("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/package.json", Directory.GetCurrentDirectory() + "/packages/" + packageName + ".json");
                    }
                    else if (package.packageVerison == "3")
                    {
                        Console.WriteLine(Language.GetPhrase("downloadv2"));
                        client.DownloadFile("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/" + packageName + ".pack", Directory.GetCurrentDirectory() + "/packages/tpmFile.pack");
                        Console.WriteLine(Language.GetPhrase("unpack"));
                        Converter.Decompress(Directory.GetCurrentDirectory() + "/packages/tpmFile.pack", Directory.GetCurrentDirectory() + "/packages/");
                        client.DownloadFile("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/package.json", Directory.GetCurrentDirectory() + "/packages/" + packageName + ".json");
                        Console.WriteLine(Language.GetPhrase("cleaning"));
                        File.Delete(Directory.GetCurrentDirectory() + "/packages/tpmFile.pack");
                    }
                    else
                    {
                        Console.WriteLine(Language.GetPhrase("outdated").phrase(new string[0]));
                        Console.WriteLine(Language.GetPhrase("outdatedinfo").phrase(new string[] { package.packageVerison, verison }));
                    }

                    
                }
                else
                {
                    if (!IsValid("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/package.json"))
                        Console.WriteLine(Language.GetPhrase("notfound").phrase(new string[] { packageName }));
                    else
                        Console.WriteLine(Language.GetPhrase("update").phrase(new string[] { packageName, packageName }));
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
                    Console.WriteLine(Language.GetPhrase("notinstalled").phrase(new string[0]));
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
                    Console.WriteLine(Language.GetPhrase("langchange").phrase(new string[] { args[1] }));
                }
                else
                {
                    Console.WriteLine(Language.GetPhrase("nolang").phrase(new string[] { args[1] }));
                }

            }
            else if (args.Length == 2 && args[0] == "update")
            {
                UpdatePackage(args[1]);
            }
            else if (args.Length == 1 && args[0] == "create")
            {
                Console.Write(Language.GetPhrase("packagestore").phrase(new string[0]));
                string packageLocation = Console.ReadLine();
                Console.Write(Language.GetPhrase("packagename").phrase(new string[0]));
                string packageName = Console.ReadLine();
                string[] contents = Directory.GetFiles(packageLocation, "*.*", SearchOption.AllDirectories);
                for (int i = 0; i < contents.Length; i++)
                {
                    contents[i] = contents[i].Replace(packageLocation, "");
                    contents[i] = contents[i].Replace("\\", "/").Remove(0, 1);
                }
                Package package = new Package(verison, contents);
                File.WriteAllText(packageLocation + "/package.json", JsonConvert.SerializeObject(package));
                Converter.Compress(packageLocation, contents, packageLocation + "/" + packageName + ".pack");
                /*File.Create(packageLocation + "/" + packageName + ".zip").Close();
                ZipArchive zip = ZipFile.Open(packageLocation + "/" + packageName + ".zip", ZipArchiveMode.Create);
                for (int i = 0; i < contents.Length; i++)
                {
                    zip.CreateEntryFromFile(packageLocation + contents[i], contents[i]);
                }
                */
                Console.WriteLine(Language.GetPhrase("packagedone").phrase(new string[0]));

            }
            else
            {
                Console.WriteLine(Language.GetPhrase("info").phrase(new string[0]));
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
                        Console.WriteLine(Language.GetPhrase("outdated").phrase(new string[0]));
                        Console.WriteLine(Language.GetPhrase("outdatedinfo").phrase(new string[] { packageUpdate.packageVerison, verison }));
                    }

                    
                }
                else
                {
                    if (!IsValid("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/package.json"))
                        Console.WriteLine(Language.GetPhrase("notfound").phrase(new string[] { packageName }));
                }
            }
            else
            {
                Console.WriteLine(Language.GetPhrase("notinstalled").phrase(new string[0]));
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

            public static Phrase GetPhrase(string name)
            {
                if (languages[currentLang].phrases.ContainsKey(name))
                    return languages[currentLang].phrases[name];
                else if (languages["eng"].phrases.ContainsKey(name))
                    return languages["eng"].phrases[name];
                return new Phrase("Phrase not found: " + name);
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