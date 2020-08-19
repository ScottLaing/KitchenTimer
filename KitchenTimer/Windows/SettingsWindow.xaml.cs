using KitchenTimer.Entities;
using KitchenTimer.Resx;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Media;
using System.Windows;
using System.Windows.Controls;

namespace KitchenTimer.Windows
{
    /// <summary>
    /// Interaction logic for SetTime.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        #region Fields

        // current assembly
        private System.Reflection.Assembly assembly;

        // whether an alarm is playing now
        private bool alarmIsPlaying = false;

        // the sound player used by dialog for testing alarm sounds
        private SoundPlayer player;

        // the current alarm selected
        private Alarm currentAlarm;

        #endregion

        #region Properties

        /// <summary>
        /// time value chosen by dialog call
        /// </summary>
        public double TimeValue { get; set; } = 0;

        /// <summary>
        /// alarm chosen by dialog call
        /// </summary>
        public Alarm AlarmChosen { get; set; } = null;

        #endregion

        #region Constructors

        /// <summary>
        /// main constructor
        /// </summary>
        public SettingsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// overloaded constructor passing in current alarm and current counter time value
        /// </summary>
        /// <param name="currentAlarm"></param>
        /// <param name="countDown"></param>
        public SettingsWindow(Alarm currentAlarm, double countDown) : this()
        {
            this.currentAlarm = currentAlarm;
            cmbAlarmSound.ItemsSource = Constants.AlarmList;
            txtSetTime2.Value = countDown;
            int index = FindAlarmIndex(currentAlarm);
            cmbAlarmSound.SelectedIndex = index;
            InitializeSoundPlayer();
        }

        #endregion

        #region Methods

        /// <summary>
        /// setup sound player, load in current alarm
        /// </summary>
        private void InitializeSoundPlayer()
        {
            player = new SoundPlayer();
            LoadAlarm(this.currentAlarm.WavName);
        }

        /// <summary>
        /// load alarm of given name into sound player
        /// </summary>
        /// <param name="alarmName"></param>
        private void LoadAlarm(string alarmName)
        {
            try
            {
                // note about wav file processing in the project:
                // a normal inclusion of a data Resource in WPF as a "Resource" file type fails and the wav won't play.
                // using "Embedded Resource" file type however for the wav files, works ok.  
                if (assembly == null)
                {
                    assembly = System.Reflection.Assembly.GetExecutingAssembly();
                }

                //load the embedded resource as a stream
                var wavFile = $"{alarmName}{Constants.WavExtension}";
                wavFile = CleanFormat(wavFile);

                var name = assembly.GetName().Name;
                var embeddedPath = string.Format(Strings.EmbeddedResourcePath, name, wavFile);
                var stream = assembly.GetManifestResourceStream(embeddedPath);

                //load the stream into the player
                player = new SoundPlayer(stream);

                // Load the .wav file.
                player.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// play the currently selected alarm
        /// </summary>
        private void PlayAlarm()
        {
            if (alarmIsPlaying)
            {
                StopAlarm();
            }
            try
            {
                player.PlayLooping();
                alarmIsPlaying = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// stop the alarm sound (if playing)
        /// </summary>
        private void StopAlarm()
        {
            if (alarmIsPlaying)
            {
                player.Stop();
                alarmIsPlaying = false;
            }
        }

        /// <summary>
        /// save the settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TimeValue = Convert.ToDouble(this.txtSetTime2.Text);
                this.DialogResult = true;
                StopAlarm();
                this.Close();
            }
            catch (Exception ex)
            {
                var error = string.Format(Strings.SavingError, ex.Message);
                MessageBox.Show(error);
            }
        }

        /// <summary>
        /// handle clicking play button for drop down alarms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            var alarm = this.cmbAlarmSound.SelectedItem as Alarm;
            if (alarm == null)
            {
                MessageBox.Show(Strings.AlarmDropDownError);
                return;
            }
            LoadAlarm(alarm.WavName);
            PlayAlarm();
        }

        /// <summary>
        /// handle window closing event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            StopAlarm();
        }

        /// <summary>
        /// handle alarm drop down change selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbAlarmSound_Selected(object sender, SelectionChangedEventArgs e)
        {
            var alarm = this.cmbAlarmSound.SelectedItem as Alarm;
            if (alarm == null)
            {
                MessageBox.Show(Strings.AlarmDropDownError);
                return;
            }

            AlarmChosen = alarm;
            if (alarmIsPlaying)
            {
                StopAlarm();
                alarmIsPlaying = false;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// get index for alarm situation
        /// </summary>
        /// <param name="currentAlarm"></param>
        /// <returns></returns>
        private static int FindAlarmIndex(Alarm currentAlarm)
        {
            int index = -1;
            int k = 0;
            foreach (var item in Constants.AlarmList)
            {
                if (item.WavName + Constants.WavExtension == currentAlarm.WavName ||
                    item.WavName == currentAlarm.WavName)
                {
                    index = k;
                    break;
                }
                k++;
            }

            return index;
        }

        /// <summary>
        /// clean up anomalous file formats
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static string CleanFormat(string file)
        {
            file = file.Replace($"{Constants.WavExtension}{Constants.WavExtension}", Constants.WavExtension);
            if (!file.EndsWith(Constants.WavExtension))
            {
                file += Constants.WavExtension;
            }
            return file;
        }

        #endregion
    }
}
