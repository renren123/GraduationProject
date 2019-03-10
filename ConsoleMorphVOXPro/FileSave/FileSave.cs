using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMorphVOXPro.FileSave
{
    [Serializable]
    class FileSave
    {
        string FileName { set; get; }
        public void SaveMaxAveRageMin(string fileName,double max,double average,double min)
        {
            FileName = fileName;
            string line = max + "," + average + "," + min;
            using (StreamWriter streamWriter = new StreamWriter(fileName, true))
            {
                streamWriter.WriteLine(line);
            }
        }
    }
}
