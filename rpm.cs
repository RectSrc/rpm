using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;
namespace rpm
{
    public static class rpm
    {

        static string branch = "beta";
        static string verison = "2";


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
                            client.DownloadFile("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/" + dep, Directory.GetCurrentDirectory() + "/packages/" + dep);
                        }
                        client.DownloadFile("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/package.json", Directory.GetCurrentDirectory() + "/packages/" + packageName + ".json");
                    }
                    else
                    {
                        Console.WriteLine("Outdated package or client");
                        Console.WriteLine("Package uses verison " + package.packageVerison + ", client uses verison " + verison);
                    }

                    
                }
                else
                {
                    if (!IsValid("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/package.json"))
                        Console.WriteLine("Package " + packageName + " not found!");
                    else
                        Console.WriteLine("Package " + packageName + " is already installed!\nUpdate with rpm update " + packageName);
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
                    Console.WriteLine("Package not installed!");
                }

            }
            else if (args.Length == 2 && args[0] == "update")
            {
                UpdatePackage(args[1]);
            }
            else
            {
                Console.WriteLine("ReCT Package Manager(rpm)");
                Console.WriteLine("Made by the RectSrc team");
                Console.WriteLine("Commands:");
                Console.WriteLine("get [packagename]    -Gets the package called [packagename]");
                Console.WriteLine("remove [packagename]    -Removes the package called [packagename]");
                Console.WriteLine("update [packagename]    -Updates the package called [packagename]");
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
                        Console.WriteLine("Outdated package or client!");
                        Console.WriteLine("Package uses verison " + packageUpdate.packageVerison + ", client uses verison " + verison);
                    }

                    
                }
                else
                {
                    if (!IsValid("https://raw.githubusercontent.com/RectSrc/rpm/" + branch + "/packages/" + packageName + "/package.json"))
                        Console.WriteLine("Package " + packageName + " not found!");
                }
            }
            else
            {
                Console.WriteLine("Package not installed!");
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
    }
}