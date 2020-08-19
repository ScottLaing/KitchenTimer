using KitchenTimer.Entities;

namespace KitchenTimer
{
    public class Constants
    {
        public const string AppTitle = "Kitchen Timer";
        public const string WavExtension = ".wav";

        public static class FontSizing
        {
            public const double FontSizeHeightFactor = 3.2;
            public const int FontHeightMinStep = 20;
            public const double FontSizeStepIncrementer = 6.0;
            public const int MinimumFontSize = 55;
        }

        public static readonly Alarm[] AlarmList = new Alarm[]
        {
          new Alarm()
          {
              WavName = "Alarm01",
              Title = "Lively"
          },
          new Alarm()
          {
              WavName = "Alarm02",
              Title = "Raindrops"
          },
          new Alarm()
          {
              WavName = "Alarm03",
              Title = "Gentle River"
          },
            new Alarm()
          {
              WavName = "Alarm04",
              Title = "Tiptap Drops"
          },
          new Alarm()
          {
              WavName = "Alarm05",
              Title = "Bells"
          },
          new Alarm()
          {
              WavName = "Alarm06",
              Title = "Synth Pulse"
          },        
          new Alarm()
          {
              WavName = "Alarm07",
              Title = "Simple Tone"
          },         
          new Alarm()
          {
              WavName = "Alarm08",
              Title = "Tenor Taps"
          },         
          new Alarm()
          {
              WavName = "Alarm09",
              Title = "Sleepy Fog"
          },         
          new Alarm()
          {
              WavName = "Alarm10",
              Title = "Alien Fire"
          },         
        };
    }
}
