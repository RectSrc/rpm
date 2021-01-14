﻿using System;
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
                if (IsValid("https://raw.githubusercontent.com/RectSrc/rpm/deps-beta/packages/" + packageName + "/" + packageName + ".dll"))
                {
                    Console.WriteLine("Getting package " + packageName + "...");
                    Console.Write("Getting package from ");
                    Console.Write("https://raw.githubusercontent.com/RectSrc/rpm/deps-beta/packages/" + packageName + "/" + packageName + ".dll\n");
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/packages/");
                    wc.DownloadFile("https://raw.githubusercontent.com/RectSrc/rpm/deps-beta/packages/" + packageName + "/" + packageName + ".dll", Directory.GetCurrentDirectory() + "/packages/" + packageName + ".dll");
                    if (IsValid("https://raw.githubusercontent.com/RectSrc/rpm/deps-beta/packages/" + packageName + "/" + packageName + "-DEPS/deps.dps"))
                    {
                        Console.WriteLine("Deps exists, downloading... ");
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/packages/" + packageName + "-DEPS/");
                        string[] deps = wc.DownloadString("https://raw.githubusercontent.com/RectSrc/rpm/deps-beta/packages/" + packageName + "/" + packageName + "-DEPS/deps.dps").Split("\n");
                        foreach(string dep in deps)
                        {
                            try
                            {
                                wc.DownloadFile("https://raw.githubusercontent.com/RectSrc/rpm/deps-beta/packages/" + packageName + "/" + packageName + "-DEPS/" + dep, Directory.GetCurrentDirectory() + "/packages/" + packageName + "-DEPS/" + dep);
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
            }
            else
            {
                Console.WriteLine("Invalid arguments");
            }
        }
    }
}
