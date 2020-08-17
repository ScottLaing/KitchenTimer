using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace KitchenTimer.Utils
{
    public class SoundUtils
    {
        public static void LoadWaveFile(string alarmName, SoundPlayer player)
        {
            //get the current assembly
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            //load the embedded resource as a stream
            var file = $"{alarmName}.wav";
            var stream = assembly.GetManifestResourceStream(string.Format("{0}.Resources.{1}", assembly.GetName().Name, file));

            //load the stream into the player
            player = new SoundPlayer(stream);

            // Load the .wav file.
            player.Load();
        }
    }
}
