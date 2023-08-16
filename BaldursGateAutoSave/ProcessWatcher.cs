using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;

namespace BaldursGateAutoSave
{
    internal class ProcessWatcher
    {
        private Thread? processCheckThread;
        private MainWindow mainWindow;
        private string watchedProcessName;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private ManualResetEvent resetEvent = new ManualResetEvent(false);

        public ProcessWatcher(MainWindow mainWindow, string watchedProcess)
        {
            this.mainWindow = mainWindow;
            watchedProcessName = watchedProcess;
        }
        public void Start()
        {
            resetEvent.Reset();
            cancellationTokenSource = new CancellationTokenSource();
            processCheckThread = new Thread(ProcessCheckLoop);
            processCheckThread.Start();
        }

        public void Stop()
        {
            if (processCheckThread != null && processCheckThread.IsAlive)
            {
                cancellationTokenSource.Cancel();
                resetEvent.Set();
                processCheckThread.Join();
            }
        }

        private void ProcessCheckLoop()
        {
            //Set interval to check for game process 
            TimeSpan sleepInterval = TimeSpan.FromSeconds(15);
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            bool isSaving = false;
            while (!cancellationToken.IsCancellationRequested)
            {
                //Check if game process is currently running
                if (GetGameHWND(watchedProcessName) != IntPtr.Zero)
                {
                    if (!isSaving)
                    {
                        //Update main UI with running text and start the saving timer
                        mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            mainWindow.StartSaving();
                            mainWindow.lblRunning.Foreground = Brushes.Green;
                            mainWindow.lblRunning.Content = "Running";
                        }));

                        //While game process is running and actively saving, check status every five minutes
                        isSaving = true;
                        sleepInterval = TimeSpan.FromMinutes(5);
                    }
                }
                else
                {
                    //Reset interval to look for process
                    if (isSaving)
                    {
                        isSaving = false;
                        sleepInterval = TimeSpan.FromSeconds(15);
                    }
                }
                resetEvent.WaitOne(sleepInterval);
            }

            mainWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                mainWindow.StopSaving();
                mainWindow.lblRunning.Foreground = Brushes.Red;
                mainWindow.lblRunning.Content = "Not Running";
            }));
        }

        //Retrieve hWND for partial game title, used with AutoIt to set focus and send input
        public static IntPtr GetGameHWND(string gameName)
        {
            Process[] procs = Process.GetProcesses();
            foreach (Process proc in procs)
            {
                if (proc.ProcessName.Contains(gameName))
                {
                    return proc.MainWindowHandle;
                }
            }

            return IntPtr.Zero;
        }
    }
}
