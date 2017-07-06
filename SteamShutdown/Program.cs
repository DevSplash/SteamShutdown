﻿using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Windows.Forms;

namespace SteamShutdown
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string appProcessName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            Process[] RunningProcesses = Process.GetProcessesByName(appProcessName);

            // IPC: https://gorillacoding.wordpress.com/2013/02/03/using-wcf-for-inter-process-communication/
            const string address = "net.pipe://localhost/SteamShutdown/ShowBalloonTip";
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

            if (RunningProcesses.Length == 1)
            {
                // Already running. This process is the client of IPC
                ServiceHost serviceHost = new ServiceHost(typeof(IPCTestServer));
                serviceHost.AddServiceEndpoint(typeof(ITestContract), binding, address);
                serviceHost.Open();
            }
            else
            {
                // Only running process. This process is the server of IPC
                EndpointAddress ep = new EndpointAddress(address);
                ITestContract channel = ChannelFactory<ITestContract>.CreateChannel(binding, ep);
                channel.ShowAnInstanceIsRunning();
                return;
            }


            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var applicationContext = new CustomApplicationContext();
            Application.Run(applicationContext);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "SteamShutdown_Log.txt");
            File.WriteAllText(path, e.ExceptionObject.ToString());

            MessageBox.Show("Please send me the log file on GitHub or via E-Mail (Andreas.D.Korb@gmail.com)" + Environment.NewLine + path, "An Error occured");
        }
    }
}
