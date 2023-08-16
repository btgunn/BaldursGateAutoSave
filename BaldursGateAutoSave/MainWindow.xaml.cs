using AutoIt;
using System;
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
        private int saveInterval = 15; //Default save interval 15 minutes
        private DispatcherTimer? timer;
        private bool IsRunning = false; //Flag for determining if actively auto-saving
        private IntPtr gameProcess = IntPtr.Zero;
        private DateTime lastSaveTime = DateTime.MinValue;
        private ProcessWatcher? procWatch;
        private string gameName = "bg3";
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

        //On time interval change, verify positive integer and update control
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
                //start process detection and saving
                StartProcessCheck();

                //Disable changing of interval while saving thread is running
                DisableUI(true);
            }
            else
            {
                //Signal process thread to end
                StopProcessCheck();

                //Re-enable interaction with UI
                DisableUI(false);
            }
        }
        private void DisableUI(bool disable)
        {
            if (disable)
            {
                buttonStartStop.Content = "Stop";
            }
            else
            {
                buttonStartStop.Content = "Start";
            }

            textBoxInterval.IsEnabled = !disable;
            buttonIncrement.IsEnabled = !disable;
            buttonDecrement.IsEnabled = !disable;
            IsRunning = disable;

        }

        private void StartProcessCheck()
        {
            procWatch = new ProcessWatcher(this, gameName);
            procWatch.Start();
        }
        private void StopProcessCheck()
        {
            if (procWatch != null)
            {
                procWatch.Stop();
            }
        }

        public void StartSaving()
        {
            timer = new DispatcherTimer();

            //Retrieve window handle for game process
            gameProcess = ProcessWatcher.GetGameHWND(gameName); //Currently hardcoded to baldur's gate 3

            if (gameProcess != nint.Zero)
            {
                timer.Interval = TimeSpan.FromMinutes(saveInterval);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
        }
        public void StopSaving()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            //AutoIt and autohotkey seem to be the only methods of sending keystrokes to Baldur's gate 3
            //Should work in most games without cheat detection.
            AutoItX.WinActivate(gameProcess);
            AutoItX.Send("{F5}");

            // Update the last save time
            lastSaveTime = DateTime.Now;

            // Update the label content with the new last save time
            tbLastSave.Text = $"{lastSaveTime:HH:mm:ss}";
        }

    }
}