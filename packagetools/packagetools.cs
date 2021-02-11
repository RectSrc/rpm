using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO.Compression;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.CodeDom;
using rpm;


namespace packagetools
{
    class Setup
    {
        public string projectpath
        {
            get { return System.IO.Directory.GetCurrentDirectory(); }
        }

        public string projectName;
        public readonly string packageVerison = "2";
        private bool customDotNet = false;
        private string dnverison;
        private string dotNet
        {
            get { return customDotNet ? dnverison : "netcoreapp3.1"; }
            set { customDotNet = true; dnverison = value; }
        }
        public Setup(string name, string dotnetverison)
        {
            projectName = name;
            dotNet = dotnetverison;
        }

        public Setup(string name)
        {
            projectName = name;
        }

        public void Run()
        {
            RunCommand("dotnet build " + projectName);
            Directory.SetCurrentDirectory(this.projectpath + "/bin/Debug/" + dotNet);
            rpm.rpm.Package package = new rpm.rpm.Package(projectName, Directory.GetFiles(Directory.GetCurrentDirectory()));
            string json = JsonConvert.SerializeObject(package);
            File.WriteAllText(Directory.GetCurrentDirectory() + "/package.json", json);
            try
            {
                ZipFile.CreateFromDirectory(Directory.GetCurrentDirectory(), Directory.GetCurrentDirectory() + "/" + projectName + ".zip");
            }
            catch
            {

            }
            Console.WriteLine("Package created!");
            
        }

        private void RunCommand(string command)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = command;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = false;
            startInfo.CreateNoWindow = false;
            process.StartInfo = startInfo;
            process.Start();
        }

    }
}
