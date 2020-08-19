using KitchenTimer.Entities;
using KitchenTimer.Resx;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using static KitchenTimer.Constants.FontSizing;

namespace KitchenTimer.Windows
{
    /// <summary>
    /// Code behind class for MainWindow.xaml window
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        // decrease minutes by this amount per tic
        private const double DecreasePeriod = 1.0 / (6000.0);
        // how many milliseconds to call timer function
        private const int MilliSecondTimerPeriod = 10;
        // timer object used by timer calls
        private Timer _timer = null;
        // current time for count down
        private double currentTimeVal = 15.0;

        // reference to local assembly
        System.Reflection.Assembly assembly;

        // utility locks to control access to core values, needed as timer runs in background thread
        private object currentTimeLock = new object();
        private object alarmPlayingLock = new object();
        private object isTimerRunningLock = new object();
        private object alarmStateChangeLock = new object();

        // flag, is the timer running now
        private bool isTimerRunning = false;
        // value used for resetting run times
        private double lastResetValue = 15.0;
        // delegate for update text block invoke calls
        private delegate void UpdateTextBlockCallback(int hr, int min, int sec, int tenthsSec);
        // sound player object used to play the alarms
        private SoundPlayer player;

        /// <summary>
        /// is alarm currently playing flag
        /// </summary>
        private bool alarmPlaying = false;

        #endregion

        #region Properties

        /// <summary>
        /// thread safe wrapper for isTimerRunning var 
        /// </summary>
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

        /// <summary>
        /// thread safe wrapper for alarmPlaying var
        /// </summary>
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
        /// thread safe wrapper for current timer value variable
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

        /// <summary>
        /// current alarm in use for timer
        /// </summary>
        public Alarm CurrentAlarm { get; private set; }

        #endregion

        #region Constructors 

        /// <summary>
        /// constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            RefreshTimeDisplay();
            _timer = new Timer(TimerCallback, null, 0, MilliSecondTimerPeriod);
            InitializeSoundPlayer();
        }

        /// <summary>
        /// secondary window constructor
        /// </summary>
        /// <param name="windowCount"></param>
        public MainWindow(int windowCount) : this()
        {
            this.Title = $"Kitchen Timer ({windowCount})";
        }

        #endregion

        #region Alarm and Sound Player related

        #region Sound Init

        /// <summary>
        /// Startup sound player.
        /// </summary>
        private void InitializeSoundPlayer()
        {
            player = new SoundPlayer();

            player.LoadCompleted += new AsyncCompletedEventHandler(PlayerLoadCompleted);
            player.SoundLocationChanged += new EventHandler(PlayerLocationChanged);

            LoadAlarm(1);
        }

        /// <summary>
        /// Loads an alarm based on index value (1-10 currently)
        /// </summary>
        /// <param name="alarmNumber"></param>
        private void LoadAlarm(int alarmNumber)
        {
            try
            {
                // todo: location works for debugging but move it to better place soon 
                string path;
                if (assembly == null)
                {
                    assembly = System.Reflection.Assembly.GetExecutingAssembly();
                }

                var file = $"Alarm{alarmNumber:00}.wav";
                var stream = assembly.GetManifestResourceStream(string.Format("{0}.Resources.{1}", assembly.GetName().Name, file));

                //load the stream into the player
                player = new System.Media.SoundPlayer(stream);

                string waveFile = file;
                CurrentAlarm = new Alarm()
                {
                    WavName = waveFile,
                    Title = waveFile
                };

                // Load the .wav file.
                try
                {
                    player.Load();
                }
                catch
                {
                    path = $"Resources/sounds/Alarm{alarmNumber:00}.wav";
                    player.SoundLocation = path;
                    player.LoadAsync();
                }
            }
            catch (Exception ex)
            {
                LogStatus(ex.Message);
            }
        }

        /// <summary>
        /// load an alarm
        /// </summary>
        /// <param name="alarmName"></param>
        private void LoadAlarm(string alarmName)
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var file = alarmName;
                if (!alarmName.EndsWith(".wav"))
                {
                    file += ".wav";
                }
                var stream = assembly.GetManifestResourceStream(string.Format("{0}.Resources.{1}", assembly.GetName().Name, file));

                //load the stream into the player
                player = new System.Media.SoundPlayer(stream);

                player.Load();

            }
            catch (Exception ex)
            {
                player.SoundLocation = "";
                Debug.WriteLine(ex.Message);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle sound player load completed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerLoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string message = "sound file load completed"; 
            LogStatus(message);
        }

        /// <summary>
        /// Handle sound player location changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerLocationChanged(object sender, EventArgs e)
        {
            string message = String.Format("SoundLocationChanged: {0}", player.SoundLocation);
            LogStatus(message);
        }

        #endregion

        #region Change Alarm ON/OFF

        /// <summary>
        /// Cause sound player to start playing currently loaded alarm.
        /// </summary>
        private void PlayAlarm()
        {
            LogStatus("Looping .wav file asynchronously.");
            lock (alarmStateChangeLock)
            {
                if (!AlarmPlaying)
                {
                    player.PlayLooping();
                    AlarmPlaying = true;
                }
            }
        }

        /// <summary>
        /// Have sound player stop playing alarm, if playing.
        /// </summary>
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
            LogStatus("Stopped by user.");
        }

        #endregion

        #endregion

        #region Timer Related Methods

        /// <summary>
        /// Main timer call back method.
        /// </summary>
        /// <param name="o"></param>
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

        #endregion

        #region Menu Event Handlers

        /// <summary>
        /// Handle exit menu call.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShutdownApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Show the settings window, change settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowSettingsWindow(object sender, RoutedEventArgs e)
        {
            var setTime = new SettingsWindow(CurrentAlarm, lastResetValue);
            var dlgResult = setTime.ShowDialog();
            if (dlgResult.HasValue && dlgResult.Value && setTime.TimeValue > 0)
            {
                ChangeSettings(setTime, CurrentAlarm);
            }
            e.Handled = true;
        }

        /// <summary>
        /// apply new settings
        /// </summary>
        /// <param name="setTime"></param>
        /// <param name="currentAlarm"></param>
        private void ChangeSettings(SettingsWindow setTime, Alarm currentAlarm)
        {
            if (setTime != null)
            {
                var newTime = setTime.TimeValue;
                CurrentTimeVal = newTime;
                lastResetValue = newTime;
                if (setTime.AlarmChosen != null)
                {
                    CurrentAlarm = setTime.AlarmChosen;
                    LoadAlarm(CurrentAlarm.WavName);
                }
                RefreshTimeDisplay();
            }
        }

        /// <summary>
        /// Handle show about window menu call.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowAboutWindow(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        #endregion

        #region UI Event Handlers

        /// <summary>
        /// Start countdown timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartTimer(object sender, RoutedEventArgs e)
        {
            IsTimerRunning = true;
        }

        /// <summary>
        /// Pause the countdown timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PauseTimer(object sender, RoutedEventArgs e)
        {
            IsTimerRunning = !IsTimerRunning;
        }

        /// <summary>
        /// Stop the countdown timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopTimer(object sender, RoutedEventArgs e)
        {
            IsTimerRunning = false;
            // if an alarm is going off, stop it
            StopAlarm();
        }

        /// <summary>
        /// Reset countdown timer back to reset value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetTimer(object sender, RoutedEventArgs e)
        {
            CurrentTimeVal = lastResetValue;
            RefreshTimeDisplay();
        }

        #endregion

        #region Refreshing Time UI Related

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
                    Debug.WriteLine($"Invoke required for UpdateTextBlock call, {ex.Message}");
                    DispInvokeUpdate(hrs, mins, secs, tenthsOfSec);
                }
            }
            else
            {
                DispInvokeUpdate(hrs, mins, secs, tenthsOfSec);
            }
        }

        #endregion

        #region Window Event Handlers

        /// <summary>
        /// Window is closing, do some clean up, dispose timer.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        /// <summary>
        /// Handle resize main window event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// handle the new window menu option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewTimerWindow_Click(object sender, RoutedEventArgs e)
        {
            var app = Application.Current as App;
            if (app == null)
            {
                MessageBox.Show(Strings.TroubleGettingApplicationObject);
                return;
            }
            var mainWin = new MainWindow(++app.NewWindowCounter);
            mainWin.Show();
        }

        #endregion

        #region Utility Methods

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

        /// <summary>
        /// Log some sort of status change.
        /// </summary>
        /// <param name="logMessage"></param>
        private static void LogStatus(string logMessage)
        {
            if (!string.IsNullOrEmpty(logMessage))
            {
                // todo: add real logging to here soon
                Debug.WriteLine(logMessage);
            }
        }

        #endregion

        private void RedColorOption_Click(object sender, RoutedEventArgs e)
        {
            tbTime.Foreground = Brushes.Red;
            e.Handled = true;
        }

        private void GreenColorOption_Click1(object sender, RoutedEventArgs e)
        {
            tbTime.Foreground = Brushes.LightGreen;
            e.Handled = true;
        }

        private void BlueColorOption_Click(object sender, RoutedEventArgs e)
        {
            tbTime.Foreground = Brushes.LightBlue;
            e.Handled = true;
        }

        private void PurpleColorOption_Click(object sender, RoutedEventArgs e)
        {
            tbTime.Foreground = Brushes.Purple;
            e.Handled = true;
        }

        private void WhiteColorOption_Click(object sender, RoutedEventArgs e)
        {
            tbTime.Foreground = Brushes.White; 
            e.Handled = true;
        }

        // textBlock.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDFD991")); 
        //#FF18B265
    }
}
