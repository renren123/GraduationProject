using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMorphVOXPro
{
    class CaculateTimes
    {
        public int Hour { set; get; }
        public int Min { set; get; }
        public int Second { set; get; }
        public int Ms { set; get; }
        public CaculateTimes()
        {

        }
  
        public void StartTime()
        {
            Hour = DateTime.Now.Hour;
            Min = DateTime.Now.Minute;
            Second = DateTime.Now.Second;
            Ms = DateTime.Now.Millisecond;
        }
        public string EndTime()
        {
            int hour = DateTime.Now.Hour;
            int min = DateTime.Now.Minute;
            int second = DateTime.Now.Second;
            int ms = DateTime.Now.Millisecond;
            if (hour < Hour)
                hour += 24;
            int millisecond1 = ((hour * 60 + min) * 60 + second) * 1000 + ms;
            int millisecond2 = ((Hour * 60 + Min) * 60 + Second) * 1000 + Ms;
            int time = millisecond1 - millisecond2;
            string line = "";
            line = ":"+time % 1000;
            time /= 1000;
            line = time % 60+":" + line;
            time /= 60;
            line = time % 60 + ":" + line;
            return line;
        }
    }
}
