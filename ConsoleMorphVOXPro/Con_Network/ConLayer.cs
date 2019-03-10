using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LeNet_5.Con_Network
{
    [Serializable]
    class ConLayer
    {
        public bool initConfilter { set; get; }
        /// <summary>
        /// 这两个数是卷积核一次卷积图层的个数
        /// </summary>
        public int FilterNumber { set; get; }
        public int FilterSize { set; get; }
        public List<int[]> listConInputMapIndex { set; get; }
        public List<ConFilter> conFilters;
        /// <summary>
        /// 数量为mini-batch的输入数组
        /// </summary>
        public List<ArrayMap> inputMaps;
        public List<ArrayMap> poolingMaps;
        public List<ArrayMap> inputSensitives;
        public List<ArrayMap> poolingSensitives;

        public ConLayer()
        {
            initConfilter = false;
        }
        public void execute()
        {
            if (inputMaps == null || conFilters == null)
            {
                Console.WriteLine("Error!->ConLayer->execute");
                return;
            }
            //InitConFilter
            if (initConfilter == false)
            {
                if (listConInputMapIndex==null)
                {
                    Console.WriteLine("Error!->initConfilter");
                    return;
                }
                for (int i = 0; i < conFilters.Count; i++)
                {
                    conFilters[i].Filter_size = FilterSize;
                    conFilters[i].ConInputMapIndex = listConInputMapIndex[i];
                }
                initConfilter = true;
            }

            if (poolingMaps == null)
            {
                poolingMaps = new List<ArrayMap>(conFilters.Count);
            }
            if (poolingMaps.Count > 0)
            {
                poolingMaps.Clear();
            }
            for (int i = 0; i < inputMaps.Count; i++)
            {
                poolingMaps.Add(new ArrayMap());
                poolingMaps[i].map = new List<float[,]>(conFilters.Count);
            }

            for (int j = 0; j < conFilters.Count; j++)
            {
                if (conFilters[j].Wi==null)
                {
                    conFilters[j].Wi = new List<float[,]>(inputMaps[0].map.Count);
                    for (int i = 0; i < inputMaps[0].map.Count; i++)
                    {
                        conFilters[j].Wi.Add(new float[FilterSize, FilterSize]);
                    }
                    int inputSumTemp = inputMaps.Count * FilterSize * FilterSize;
                    //int inputSum = inputMaps[0].map[0].GetLength(0) * inputMaps[0].map[0].GetLength(1);
                    conFilters[j].InitWi(inputSumTemp);
                }
                //出来的是一些列，各个minibatch针对filter的单个样本
                List<float[,]> poolingMapTemp = conFilters[j].forward(inputMaps);
                //inputMaps.Count是minibatch的大小
                for (int k = 0; k < inputMaps.Count; k++)
                {
                    poolingMaps[k].map.Add(poolingMapTemp[k]);
                }
            }
        }
        public void update()
        {
            if (poolingSensitives == null)
            {
                Console.WriteLine("Error!->ConLayer->update");
                return;
            }
            if (inputSensitives == null)
            {
                inputSensitives = new List<ArrayMap>(inputMaps.Count);
                for (int i = 0; i < inputMaps.Count; i++)
                {
                    inputSensitives.Add(new ArrayMap());
                    inputSensitives[i].map = new List<float[,]>(inputMaps[i].map.Count);
                    int mapW = inputMaps[i].map[0].GetLength(0);
                    int mapH = inputMaps[i].map[0].GetLength(1);
                    for (int j = 0; j < inputMaps[i].map.Count; j++)
                    {
                        inputSensitives[i].map.Add(new float[mapW, mapH]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < inputSensitives.Count; i++)
                {
                    for (int j = 0; j < inputSensitives[i].map.Count; j++)
                    {
                        for (int x = 0; x < inputSensitives[i].map[j].GetLength(0); x++)
                        {
                            for (int y = 0; y < inputSensitives[i].map[j].GetLength(1); y++)
                            {
                                inputSensitives[i].map[j][x, y] = 0;
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < conFilters.Count; i++)
            {
                List<float[,]> poolingSensitiveTemp = new List<float[,]>(poolingSensitives.Count);
                //inputMaps.Count是minibatch的大小
                for (int j = 0; j < inputMaps.Count; j++)
                {
                    poolingSensitiveTemp.Add(poolingSensitives[j].map[i]);
                }
                inputSensitives = conFilters[i].backward(poolingSensitiveTemp, inputMaps, inputSensitives);
            }
        }
        private void ArrayPlus(ref List<float[,]> array1,ref List<float[,]> array2)
        {
            if (array2==null)
            {
                Console.WriteLine("Error!->ConLayer->ArrayPlus");
                return;
            }
            if (array1==null)
            {
                array1 = new List<float[,]>(array2.Count);
                for (int i = 0; i < array2.Count; i++)
                {
                    array1.Add(new float[array2[i].GetLength(0), array2[i].GetLength(1)]);
                }
            }
            for (int i = 0; i < array2.Count; i++)
            {
                for (int j = 0; j < array2[i].GetLength(0); j++)
                {
                    for (int k = 0; k < array2[i].GetLength(1); k++)
                    {
                        array1[i][j, k] += array2[i][j, k];
                    }
                }
            }
        }
    }
}
