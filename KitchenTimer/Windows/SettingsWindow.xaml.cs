using KitchenTimer.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KitchenTimer.Windows
{
    /// <summary>
    /// Interaction logic for SetTime.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        public double TimeValue { get; set; } = 0;
        public Alarm AlarmChosen { get; set; } = null;

        private bool alarmIsPlaying = false;

        private SoundPlayer player;

        public SettingsWindow()
        {
            InitializeComponent();
            cmbAlarmSound.ItemsSource = Constants.AlarmList;
            InitializeSoundPlayer();
        }

        private void InitializeSoundPlayer()
        {
            player = new SoundPlayer();
            LoadAlarm(1);
        }

        private void LoadAlarm(int alarmNumber)
        {
            try
            {
                // todo: location works for debugging but move it to better place soon 
                player.SoundLocation = $"../../Resources/sounds/Alarm{alarmNumber:00}.wav";

                // Load the .wav file.
                player.Load();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void LoadAlarm(string alarmName)
        {
            try
            {
                // todo: location works for debugging but move it to better place soon 
                player.SoundLocation = $"../../Resources/sounds/{alarmName}.wav";

                // Load the .wav file.
                player.Load();
            }
            catch (Exception ex)
            {
                player.SoundLocation = "";
                Debug.WriteLine(ex.Message);
            }
        }

        private void PlayAlarm()
        {
            if (string.IsNullOrWhiteSpace(player.SoundLocation))
            {
                MessageBox.Show("Sound not loaded yet, cannot play.");
                return;
            }
            if (alarmIsPlaying)
            {
                StopAlarm();
            }
            player.PlayLooping();
            alarmIsPlaying = true;
        }

        private void StopAlarm()
        {
            if (alarmIsPlaying)
            {
                player.Stop();
                alarmIsPlaying = false;
            }
        }


        private void SetTime_Click(object sender, RoutedEventArgs e)
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
                MessageBox.Show($"Some error with saving: {ex.Message}.");
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            var alarm = this.cmbAlarmSound.SelectedItem as Alarm;
            if (alarm == null)
            {
                MessageBox.Show("Trouble getting alarm location from drop down choice.");
                return;
            }
            LoadAlarm(alarm.WavName);
            PlayAlarm();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            StopAlarm();
        }

        private void cmbAlarmSound_Selected(object sender, RoutedEventArgs e)
        {
            var alarm = this.cmbAlarmSound.SelectedItem as Alarm;
            if (alarm == null)
            {
                MessageBox.Show("Trouble getting alarm location from drop down choice.");
                return;
            }
            AlarmChosen = alarm;
        }
    }
}
