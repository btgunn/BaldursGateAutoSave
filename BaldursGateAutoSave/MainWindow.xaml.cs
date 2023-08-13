using AutoIt;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
namespace BaldursGateAutoSave
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int saveInterval = 15;
        private DispatcherTimer? timer;
        private bool IsRunning = false;
        private IntPtr gameProcess = IntPtr.Zero;
        private DateTime lastSaveTime = DateTime.MinValue;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonIncrement_Click(object sender, RoutedEventArgs e)
        {
            saveInterval++;
            UpdateIntervalTextBox();
        }

        private void ButtonDecrement_Click(object sender, RoutedEventArgs e)
        {
            saveInterval--;
            UpdateIntervalTextBox();
        }
        private void UpdateIntervalTextBox()
        {
            textBoxInterval.Text = saveInterval.ToString();
        }

        private void TextBoxInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(textBoxInterval.Text, out int enteredValue))
            {
                if (enteredValue >= 0)
                {
                    saveInterval = enteredValue;
                    return;
                }
            }
            
            UpdateIntervalTextBox();
        }

        private void ButtonStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRunning)
            {
                //start auto save (press F5) thread/timer
                StartSaving();
                textBoxInterval.IsEnabled = false;
                buttonStartStop.Content = "Stop";
                IsRunning = true;
            }
            else
            {
                //Stop auto saving
                StopSaving();
                textBoxInterval.IsEnabled = true;
                buttonStartStop.Content = "Start";
                IsRunning = false;

            }
        }

        private void StartSaving()
        {
            timer = new DispatcherTimer();
            
            gameProcess = GetGameHWND("bg3"); //Currently hardcoded to baldur's gate 3

            if (gameProcess != nint.Zero)
            {
                timer.Interval = TimeSpan.FromMinutes(saveInterval);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
        }
        private void StopSaving()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {

            AutoItX.WinActivate(gameProcess);
            AutoItX.Send("{F5}");

            // Update the last save time
            lastSaveTime = DateTime.Now;

            // Update the label content with the new last save time
            tbLastSave.Text = $"{lastSaveTime:HH:mm:ss}";
        }

        //Retrieve hWND for partial game title, used with AutoIt to set focus and send input
        private IntPtr GetGameHWND(string gameName)
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
