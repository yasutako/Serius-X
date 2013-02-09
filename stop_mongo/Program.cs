using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace stop_mongo
{
    class Program
    {
        static void Main(string[] args)
        { 
            String s = @"C:\Users\yasu\Desktop\mongodb\bin";
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = System.Environment.GetEnvironmentVariable("ComSpec"),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                Arguments = "/c " + args[0] + "/mongo.exe",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            p = new Process() { StartInfo = psi };
            p.OutputDataReceived += p_OutputDataReceived;
            p.ErrorDataReceived += p_OutputDataReceived;
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            count = 0;
            timer = new Timer(new TimerCallback(call_back), null, 300, 300);
            p.WaitForExit(5000);
            p.Kill();
        }
        static Process p;
        static Timer timer;
        static int count;
        static void call_back(object state)
        {
            switch(count) {
                case 0:
                    p.StandardInput.WriteLine("use admin");
                    break;
                case 1:
                    p.StandardInput.WriteLine("db.shutdownServer()");
                    break;
            }
            count++;
        }
        static void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null) Console.WriteLine(e.Data);
        }
    }
}
