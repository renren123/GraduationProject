using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMorphVOXPro.SeriaLizer
{
    [Serializable]
    class BinarySeriaLizer
    {
        public static void SerializeMethod(Object list, string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, list);
                Console.WriteLine("序列化成功!");
            }
        }
        public static Object ReserializeMethod(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {

                BinaryFormatter bf = new BinaryFormatter();
                Object list = bf.Deserialize(fs);
                return list;
            }
        }
    }
}
