using KitchenTimer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitchenTimer
{
    public class Constants
    {
        public const string AppTitle = "Kitchen Timer";
        public static class FontSizing
        {
            public const double FontSizeHeightFactor = 3.2;
            public const int FontHeightMinStep = 20;
            public const double FontSizeStepIncrementer = 6.0;
            public const int MinimumFontSize = 55;
        }

        public static Alarm[] AlarmList = new Alarm[]
        {
          new Alarm()
          {
              WavName = "Alarm01",
              Title = "Alarm01"
          },
          new Alarm()
          {
              WavName = "Alarm02",
              Title = "Alarm02"
          },
          new Alarm()
          {
              WavName = "Alarm03",
              Title = "Alarm03"
          },
            new Alarm()
          {
              WavName = "Alarm04",
              Title = "Alarm04"
          },
          new Alarm()
          {
              WavName = "Alarm05",
              Title = "Alarm05"
          },
          new Alarm()
          {
              WavName = "Alarm06",
              Title = "Alarm06"
          },        
          new Alarm()
          {
              WavName = "Alarm07",
              Title = "Alarm07"
          },         
          new Alarm()
          {
              WavName = "Alarm08",
              Title = "Alarm08"
          },         
          new Alarm()
          {
              WavName = "Alarm09",
              Title = "Alarm09"
          },         
          new Alarm()
          {
              WavName = "Alarm10",
              Title = "Alarm10"
          },         
        };
    }
}
