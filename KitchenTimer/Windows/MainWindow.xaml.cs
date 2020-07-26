using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;

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
        private object concurrencyLock = new object();
        private bool isTimerRunning = false;
        private double lastResetValue = 15.0;
        private delegate void UpdateTextBlockCallback(int hr, int min, int sec, int tenthsSec);

        /// <summary>
        /// get or set the current timer val, this is protected by a lock because a background timer thread may try to change the value.
        /// </summary>
        public double CurrentTimeVal
        {
            get
            {
                lock (concurrencyLock)
                {
                    return currentTimeVal;
                }
            }
            set
            {
                lock (concurrencyLock)
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
        }

        private void TimerCallback(Object o)
        {
            if (isTimerRunning)
            {
                CurrentTimeVal = Math.Max(0, CurrentTimeVal - DecreasePeriod);
                RefreshTimeDisplay();
            }
        }

        #region UI Event Handlers

        private void StartTimer(object sender, RoutedEventArgs e)
        {
            isTimerRunning = true;
        }

        private void PauseTimer(object sender, RoutedEventArgs e)
        {
            isTimerRunning = !isTimerRunning;
        }

        private void SetTimePeriod(object sender, RoutedEventArgs e)
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
  
        private void ResizeWindow(object sender, SizeChangedEventArgs e)
        {
            int newFontSize = (int)(Height / 3.2 + 20);
            newFontSize = ((int)(newFontSize / 6.0)) * 6;
            newFontSize = Math.Max(55, newFontSize);
            if (tbTime.FontSize != newFontSize)
            {
                tbTime.FontSize = newFontSize;
            }
        }
 
        private void StopTimer(object sender, RoutedEventArgs e)
        {
            isTimerRunning = false;
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

        private void Shutdown(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
