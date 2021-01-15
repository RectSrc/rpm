using System;
using System.Net;
using System.IO;
namespace rpm
{
    class rpm
    {
        static public bool IsValid(string url)
        {

            bool Test = true;
            Uri urlCheck = new Uri(url);
            WebRequest request = WebRequest.Create(urlCheck);
            request.Timeout = 15000;

            WebResponse response;
            try
            {
                response = request.GetResponse();
            }
            catch (Exception)
            {
                Test = false; //url does not exist
            }

            return Test;

        }
        static void Main(string[] args)
        {
            if (args.Length == 2 && args[0] == "get")
            {
                string packageName = args[1];
                WebClient wc = new WebClient();
                Console.WriteLine("Checking if package " + packageName + " exists...");
                if (IsValid("https://raw.githubusercontent.com/RectSrc/rpm/master/packages/" + packageName + "/" + packageName + ".dll"))
                {
                    Console.WriteLine("Getting package " + packageName + "...");
                    Console.Write("Getting package from ");
                    Console.Write("https://raw.githubusercontent.com/RectSrc/rpm/master/packages/" + packageName + "/" + packageName + ".dll\n");
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/packages/");
                    wc.DownloadFile("https://raw.githubusercontent.com/RectSrc/rpm/master/packages/" + packageName + "/" + packageName + ".dll", Directory.GetCurrentDirectory() + "/packages/" + packageName + ".dll");
                    if (IsValid("https://raw.githubusercontent.com/RectSrc/rpm/master/packages/" + packageName + "/" + packageName + "-DEPS/deps.dps"))
                    {
                        Console.WriteLine("Deps exists, downloading... ");
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/packages/" + packageName + "-DEPS/");
                        string[] deps = wc.DownloadString("https://raw.githubusercontent.com/RectSrc/rpm/master/packages/" + packageName + "/" + packageName + "-DEPS/deps.dps").Split(Environment.NewLine.ToCharArray());
                        foreach (string dep in deps)
                        {
                            try
                            {
                                Console.WriteLine("Downloading " + dep + "...");
                                wc.DownloadFile("https://raw.githubusercontent.com/RectSrc/rpm/master/packages/" + packageName + "/" + packageName + "-DEPS/" + dep, Directory.GetCurrentDirectory() + "/packages/" + packageName + "-DEPS/" + dep);
                            } catch { }
                        }
                    }
                    Console.WriteLine("package " + packageName + " installed!");
                }
                else
                {
                    Console.WriteLine("Installation of package" + packageName + " failed, no package called " + packageName);
                }
            } else if(args.Length == 0)
            {
                Console.WriteLine("ReCT Package Manager(rpm)");
                Console.WriteLine("Made by the RectSrc team");
                Console.WriteLine("Commands:");
                Console.WriteLine("get [packagename]    -Gets the package called [packagename]");
            } else if(args.Length == 2 && args[0] == "remove")
            {
                string packageName = args[1];
                Console.WriteLine("Removing package " + packageName + "...");
                if (File.Exists(Directory.GetCurrentDirectory() + "/packages/" + packageName + ".dll"))
                {
                    File.Delete(Directory.GetCurrentDirectory() + "/packages/" + packageName + ".dll");
                    Console.WriteLine("Checking for deps...");
                    if (Directory.Exists(Directory.GetCurrentDirectory() + "/packages/" + packageName + "-DEPS/")){
                        foreach(string filePath in Directory.GetFiles(Directory.GetCurrentDirectory() + "/packages/" + packageName + "-DEPS/"))
                        {
                            Console.WriteLine("Removing " + filePath + "...");
                            File.Delete(filePath);
                        }
                        Directory.Delete(Directory.GetCurrentDirectory() + "/packages/" + packageName + "-DEPS/");
                    }
                    else
                    {
                        Console.WriteLine("No deps, skipping...");
                    }
                }
                else
                {
                    Console.WriteLine("No package called " + packageName + " installed, unable to remove");
                }

            }
            else
            {
                Console.WriteLine("Invalid arguments");
            }
        }
    }
}