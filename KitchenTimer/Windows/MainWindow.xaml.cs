using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Media;
using System.Threading;
using System.Windows;
using static KitchenTimer.Constants.FontSizing;

namespace KitchenTimer.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double DecreasePeriod = 1.0 / (6000.0);
        private const int MilliSecondTimerPeriod = 10;
        private Timer _timer = null;
        private double currentTimeVal = 15.0;

        private object currentTimeLock = new object();
        private object alarmPlayingLock = new object();
        private object isTimerRunningLock = new object();
        private object alarmStateChangeLock = new object();

        private bool isTimerRunning = false;
        private double lastResetValue = 15.0;
        private delegate void UpdateTextBlockCallback(int hr, int min, int sec, int tenthsSec);

        private SoundPlayer player;
        private bool alarmPlaying = false;

        public bool IsTimerRunning
        {
            get
            {
                lock (isTimerRunningLock)
                {
                    return isTimerRunning;
                }
            }
            set
            {
                lock (isTimerRunningLock)
                {
                    isTimerRunning = value;
                }
            }
        }
        public bool AlarmPlaying
        {
            get
            {
                lock (alarmPlayingLock)
                {
                    return alarmPlaying;
                }
            }
            set
            {
                lock (alarmPlayingLock)
                {
                    alarmPlaying = value;
                }
            }
        }

        /// <summary>
        /// get or set the current timer val, this is protected by a lock because a background timer thread may try to change the value.
        /// </summary>
        public double CurrentTimeVal
        {
            get
            {
                lock (currentTimeLock)
                {
                    return currentTimeVal;
                }
            }
            set
            {
                lock (currentTimeLock)
                {
                    currentTimeVal = value;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            RefreshTimeDisplay();
            _timer = new Timer(TimerCallback, null, 0, MilliSecondTimerPeriod);
            InitializeSoundPlayer();
        }

        #region Alarm and Sound Player related

        private void InitializeSoundPlayer()
        {
            player = new SoundPlayer();

            player.LoadCompleted += new AsyncCompletedEventHandler(player_LoadCompleted);
            player.SoundLocationChanged += new EventHandler(player_LocationChanged);

            LoadAlarm(1);
        }

        private void LoadAlarm(int alarmNumber)
        {
            try
            {
                // todo: location works for debugging but move it to better place soon 
                player.SoundLocation = $"../../Resources/sounds/Alarm{alarmNumber:00}.wav";

                // Load the .wav file.
                player.LoadAsync();
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message);
            }
        }

        #region Event Handlers

        private void player_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string message = "sound file load completed"; 
            ReportStatus(message);
        }

        // Handler for the SoundLocationChanged event.
        private void player_LocationChanged(object sender, EventArgs e)
        {
            string message = String.Format("SoundLocationChanged: {0}",
                player.SoundLocation);
            ReportStatus(message);
        }

        #endregion

        #region Change Alarm ON/OFF

        private void PlayAlarm()
        {
            ReportStatus("Looping .wav file asynchronously.");
            lock (alarmStateChangeLock)
            {
                if (!AlarmPlaying)
                {
                    player.PlayLooping();
                    AlarmPlaying = true;
                }
            }
        }

        // Stops the currently playing .wav file, if any.
        private void StopAlarm()
        {
            lock (alarmStateChangeLock)
            {
                if (AlarmPlaying)
                {
                    player.Stop();
                    AlarmPlaying = false;
                }
            }
            ReportStatus("Stopped by user.");
        }

        #endregion

        #endregion

        private void TimerCallback(Object o)
        {
            if (IsTimerRunning)
            {
                CurrentTimeVal = Math.Max(0, CurrentTimeVal - DecreasePeriod);
                if (currentTimeVal < .01)
                {
                    if (! AlarmPlaying)
                    {
                        PlayAlarm();
                    }
                }
                RefreshTimeDisplay();
            }
        }

        #region Menu Event Handlers

        private void Shutdown(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ShowAboutWindow(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        #endregion

        #region UI Event Handlers

        private void StartTimer(object sender, RoutedEventArgs e)
        {
            IsTimerRunning = true;
        }

        private void PauseTimer(object sender, RoutedEventArgs e)
        {
            IsTimerRunning = !IsTimerRunning;
        }

        private void ChangeSettings(object sender, RoutedEventArgs e)
        {
            var setTime = new SettingsWindow();
            var dlgResult = setTime.ShowDialog();
            if (dlgResult.HasValue && dlgResult.Value && setTime.TimeValue > 0)
            {
                var newTime = setTime.TimeValue;
                CurrentTimeVal = newTime;
                lastResetValue = newTime;
                RefreshTimeDisplay();
            }
        }
 
        private void StopTimer(object sender, RoutedEventArgs e)
        {
            IsTimerRunning = false;
            // if an alarm is going off, stop it
            StopAlarm();
        }

        private void ResetTime(object sender, RoutedEventArgs e)
        {
            CurrentTimeVal = lastResetValue;
            RefreshTimeDisplay();
        }

        #endregion

        /// <summary>
        /// Call UpdateTextBlock via dispatcher, helper method.
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="min"></param>
        /// <param name="sec"></param>
        /// <param name="tenthsOfSec"></param>
        private void DispInvokeUpdate(int hour, int min, int sec, int tenthsOfSec)
        {
            var methodParams = GetParamsForInvoke(hour, min, sec, tenthsOfSec);
            tbTime.Dispatcher.Invoke(new UpdateTextBlockCallback(UpdateTextBlock), methodParams);
        }

        /// <summary>
        /// Refresh the timer, called by the windows timer in background thread.
        /// </summary>
        private void RefreshTimeDisplay()
        {
            var timeSpan = TimeSpan.FromMinutes(CurrentTimeVal);
            int tenthsSecond = (int)(timeSpan.Milliseconds / 100.0);
            UpdateTextBlock(timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, tenthsSecond);
        }

        /// <summary>
        /// Update the timer text block.
        /// </summary>
        /// <param name="hrs"></param>
        /// <param name="mins"></param>
        /// <param name="secs"></param>
        /// <param name="tenthsOfSec"></param>
        private void UpdateTextBlock(int hrs, int mins, int secs, int tenthsOfSec)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                try
                {
                    tbTime.Text = $"{hrs:00}:{mins:00}:{secs:00}.{tenthsOfSec:0}";
                }
                catch (InvalidOperationException ex)
                {
                    DispInvokeUpdate(hrs, mins, secs, tenthsOfSec);
                }
            }
            else
            {
                DispInvokeUpdate(hrs, mins, secs, tenthsOfSec);
            }
        }

        #region Window Event Handlers

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        private void ResizeWindow(object sender, SizeChangedEventArgs e)
        {
            int newFontSize = (int)(Height / FontSizeHeightFactor + FontHeightMinStep);
            newFontSize = Convert.ToInt32(((int)(newFontSize / FontSizeStepIncrementer)) * FontSizeStepIncrementer);
            newFontSize = Math.Max(MinimumFontSize, newFontSize);
            if (tbTime.FontSize != newFontSize)
            {
                tbTime.FontSize = newFontSize;
            }
        }

        #endregion

        /// <summary>
        /// Helper method to get params object for invoke call.
        /// </summary>
        /// <param name="hrs"></param>
        /// <param name="mins"></param>
        /// <param name="secs"></param>
        /// <param name="tenthsSec"></param>
        /// <returns></returns>
        private static object[] GetParamsForInvoke(int hrs, int mins, int secs, int tenthsSec)
        {
            object[] result = new object[] { hrs, mins, secs, tenthsSec };
            return result;
        }

        private static void ReportStatus(string statusMessage)
        {
            if (!string.IsNullOrEmpty(statusMessage))
            {
                Debug.WriteLine(statusMessage);
            }
        }

 
    }
}
