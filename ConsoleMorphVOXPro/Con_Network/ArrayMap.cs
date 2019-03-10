using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeNet_5.Con_Network
{
    [Serializable]
    class ArrayMap
    {
        public List<float[,]> map;
        public ArrayMap()
        {

        }
        public ArrayMap(int mapCount,int ArrayW,int ArrayH)
        {
            map = new List<float[,]>(mapCount);
            for (int i = 0; i < mapCount; i++)
            {
                map.Add(new float[ArrayW, ArrayH]);
            }
        }
    }
}
