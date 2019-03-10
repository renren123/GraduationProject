using LeNet_5.Con_Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeNet_5
{
    /// <summary>
    /// 这个类用作中介类
    /// </summary>
    [Serializable]
    class AgentClass
    {
        public static int Mini_batchSizeTrain { set; get; }
        private static int arrayW { set; get; }
        private static int arrayH { set; get; }
        private static int mapCount { set; get; }
        public static int Mini_batchsize { set; get; }
        public static double B1 = 0.9;
        public static double B2 = 0.999;
        public static float f_derivative { set; get; }
        public static double Study_rate { set; get; }
        private static List<List<float[,]>> lists;
        public static float[] ThreeListToOneDArray(List<float[,]> listR, List<float[,]> listG, List<float[,]>listB)
        {
            int sum = 0;//里面元素的个数
            float[] array = null;//输出的数组
            int arrayIndex = 0;
            for (int i = 0; i < listR.Count; i++)
            {
                sum += listR[i].Length;
            }
            for (int i = 0; i < listG.Count; i++)
            {
                sum += listG[i].Length;
            }
            for (int i = 0; i < listB.Count; i++)
            {
                sum += listB[i].Length;
            }
            array = new float[sum];
            for (int i = 0; i < listR.Count; i++)
            {
                for (int j = 0; j < listR[i].GetLength(0); j++)
                {
                    for (int k = 0; k < listR[i].GetLength(1); k++)
                    {
                        array[arrayIndex] = listR[i][j, k];
                        arrayIndex++;
                    }
                }
            }
            for (int i = 0; i < listG.Count; i++)
            {
                for (int j = 0; j < listG[i].GetLength(0); j++)
                {
                    for (int k = 0; k < listG[i].GetLength(1); k++)
                    {
                        array[arrayIndex] = listG[i][j, k];
                        arrayIndex++;
                    }
                }
            }
            for (int i = 0; i < listB.Count; i++)
            {
                for (int j = 0; j < listB[i].GetLength(0); j++)
                {
                    for (int k = 0; k < listB[i].GetLength(1); k++)
                    {
                        array[arrayIndex] = listB[i][j, k];
                        arrayIndex++;
                    }
                }
            }
            return array;
        }
        public static void OneDArrayToThreeList(int cengshu, int filteSize, int imgW,int imgH, float[] fullyArray,out List<float[,]> listR,out List<float[,]> listG,out List<float[,]> listB)
        {
            int numberOfRGB = 3;
            if (lists==null)
            {
                lists = new List<List<float[,]>>(numberOfRGB);
            }
           
            int W = imgW;
            int H = imgH;
            for (int i = 0; i < cengshu; i++)
            {
                W = (W - filteSize + 1+1) / 2;
                H = (H - filteSize + 1+1) / 2;
            }
            int listSize = fullyArray.Length / numberOfRGB / (W * H);
            if (lists.Count==0)
            {
                for (int i = 0; i < numberOfRGB; i++)
                {
                    lists.Add(new List<float[,]>(listSize));
                }
                for (int i = 0; i < numberOfRGB; i++)
                {
                    for (int j = 0; j < listSize; j++)
                    {
                        lists[i].Add(new float[W, H]);
                    }
                }
            }
            
            
            for (int i = 0; i < numberOfRGB; i++)
            {
                for (int j = 0; j < listSize; j++)
                {
                    for (int k = 0; k < W; k++)
                    {
                        for (int m = 0; m < H; m++)
                        {
                            int index = ((i * listSize + j) * W + k) * H + m;
                            lists[i][j][k, m] = fullyArray[index];
                        }
                    }
                }
            }
            listR = lists[0];
            listG = lists[1];
            listB = lists[2];

        }

        private static List<float[,]> OneDToThreeTwoD(float[] array,int W,int H)
        {
            List<float[,]> lists = new List<float[,]>(3);
            for (int i = 0; i < 3; i++)
            {
                lists.Add(new float[W, H]);
            }
            for (int i = 0; i < lists.Count; i++)
            {
                for (int j = 0; j < lists[i].GetLength(0); j++)
                {
                    for (int k = 0; k < lists[i].GetLength(1); k++)
                    {
                        int index = ((i * lists[i].GetLength(0)) + j) + k;
                        lists[i][j, k] = array[index];
                    }
                }
            }
            return lists;
        }
        public static List<ArrayMap> ListToMap(int cengshu, int filteSize, int imgW, int imgH, List<float[]> lists)
        {
            int arrayW = imgW, arrayH = imgH;
            for (int i = 0; i < cengshu; i++)
            {
                arrayW = (arrayW - filteSize + 2) / 2;
                arrayH = (arrayH - filteSize + 2) / 2;
            }
            List<ArrayMap> maps = new List<ArrayMap>(3);
            for (int i = 0; i < 3; i++)
            {
                maps.Add(new ArrayMap());
                maps[i].map = new List<float[,]>(AgentClass.Mini_batchsize);
            }
            for (int i = 0; i < lists.Count; i++)
            {
                List<float[,]> temp = OneDToThreeTwoD(lists[i], arrayW, arrayH);
                for (int j = 0; j < maps.Count; j++)
                {
                    maps[j].map.Add(temp[j]);
                }
            }
            return maps;
        }
        private static float[] ThreeTwoDToOneD(float[,] array1,float[,] array2,float[,] array3)
        {
            int sum = array1.Length + array2.Length + array3.Length;
            float[] newArray = new float[sum];
            for (int i = 0; i < array1.GetLength(0); i++)
            {
                for (int j = 0; j < array1.GetLength(1); j++)
                {
                    newArray[i * array1.GetLength(1) + j] = array1[i, j];
                }
            }
            for (int i = 0; i < array2.GetLength(0); i++)
            {
                for (int j = 0; j < array2.GetLength(1); j++)
                {
                    newArray[array1.Length+ i * array2.GetLength(1) + j] = array2[i, j];
                }
            }
            for (int i = 0; i < array3.GetLength(0); i++)
            {
                for (int j = 0; j < array3.GetLength(1); j++)
                {
                    newArray[array1.Length+array2.Length+ i * array3.GetLength(1) + j] = array3[i, j];
                }
            }
            return newArray;
        }
        private static float[] TwoArrayToOneArray(float[] array1,float[] array2)
        {
            float[] newArray = new float[array1.Length + array2.Length];
            for (int i = 0; i < array1.Length; i++)
            {
                newArray[i] = array1[i];
            }
            for (int i = 0; i < array2.Length; i++)
            {
                newArray[i + array1.Length] = array2[i];
            }
            return newArray;
        }
        public static List<ArrayMap> ListToMaps(List<float[]> lists)
        {
            List<ArrayMap> maps = new List<ArrayMap>(Mini_batchsize);
            for (int i = 0; i < Mini_batchsize; i++)
            {
                maps.Add(new ArrayMap());
                maps[i].map = new List<float[,]>(mapCount);
                for (int j = 0; j < mapCount; j++)
                {
                    maps[i].map.Add(new float[arrayW, arrayH]);
                }
            }
            for (int i = 0; i < maps.Count; i++)
            {
                for (int j = 0; j < maps[i].map.Count; j++)
                {
                    for (int x = 0; x < maps[i].map[j].GetLength(0); x++)
                    {
                        for (int y = 0; y < maps[i].map[j].GetLength(1); y++)
                        {
                            int index = (j * arrayW + x) * arrayH + y;
                            maps[i].map[j][x, y] = lists[i][index];
                           // maps[i].map[j][x, y] = 1;
                        }
                    }
                }
            }
            return maps;
        }
        /// <summary>
        /// list代表样本的个数，一个ArrayMap代表一个样本
        /// </summary>
        /// <param name="maps"></param>
        /// <returns></returns>
        public static List<float[]> MapsToList(List<ArrayMap> maps)
        {
            List<float[]> lists = new List<float[]>(AgentClass.Mini_batchsize);
            int arrayLength = maps[0].map.Count * maps[0].map[0].GetLength(0) * maps[0].map[0].GetLength(1);
            for (int i = 0; i < Mini_batchsize; i++)
            {
                lists.Add(new float[arrayLength]);
            }
            mapCount = maps[0].map.Count;
            arrayW = maps[0].map[0].GetLength(0);
            arrayH= maps[0].map[0].GetLength(1);
            for (int i = 0; i < maps.Count; i++)
            {
                for (int j = 0; j < maps[i].map.Count; j++)
                {
                    for (int x = 0; x < maps[i].map[j].GetLength(0); x++)
                    {
                        for (int y = 0; y < maps[i].map[j].GetLength(1); y++)
                        {
                            int index = (j * arrayW + x) * arrayH + y;
                            lists[i][index] = maps[i].map[j][x, y];
                        }
                    }
                }
            }
            
            return lists;
        }
        public static List<float[]> MapToList(ArrayMap map1, ArrayMap map2, ArrayMap map3)
        {
            List<float[]> lists = new List<float[]>(map1.map.Count);
            for (int i = 0; i < map1.map.Count; i++)
            {
                float[] temp = ThreeTwoDToOneD(map1.map[i], map2.map[i], map3.map[i]);
                lists.Add(temp);
            }
            return lists;
        }
        public static float[] TwoDArrayToOneDArray(float[,] inputArray)
        {
            float[] xout = new float[inputArray.Length];
            int W = inputArray.GetLength(0);
            for (int i = 0; i < W; i++)
            {
                for (int j = 0; j < inputArray.GetLength(1); j++)
                {
                    xout[i * W + j] = inputArray[i, j];
                }
            }
            return xout;
        }
        public static void ArrayCopy(float[,] array,ref float[,] aimarray)
        {
            for (int i = 0; i < aimarray.GetLength(0); i++)
            {
                for (int j = 0; j < aimarray.GetLength(1); j++)
                {
                    aimarray[i, j] = array[i, j];
                }
            }
        }
        public static bool isTwoArrayEqual(float[,] array1,float [,] array2)
        {
            for (int i = 0; i < array1.GetLength(0); i++)
            {
                for (int j = 0; j < array1.GetLength(1); j++)
                {
                    if (array1[i,j]!=array2[i,j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public static float[,] NewArray(float [,] array)
        {
            float[,] newArray = new float[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    newArray[i, j] = array[i, j];
                }
            }
            return newArray;
        }

        /// <summary>
        /// 将静态变量进栈
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static Stack<object> SetStaticValue(Stack<object> stack)
        {
            if (stack == null)
                stack = new Stack<object>();
            stack.Push(Mini_batchSizeTrain);
            stack.Push(arrayW);
            stack.Push(arrayH);
            stack.Push(mapCount);
            stack.Push(Mini_batchsize);
            stack.Push(f_derivative);
            stack.Push(Study_rate);
            return stack;
        }
        /// <summary>
        /// 将静态变量出栈
        /// </summary>
        /// <param name="stack"></param>
        public static void GetStaticValue(Stack<object> stack)
        {
            if (stack == null || stack.Count == 0)
            {
                Console.WriteLine("AgentClass->GetStaticValue()");
                return;
            }
            Study_rate = (double)stack.Pop();
            f_derivative = (float)stack.Pop();
            Mini_batchsize = (int)stack.Pop();
            mapCount = (int)stack.Pop();
            arrayH = (int)stack.Pop();
            arrayW = (int)stack.Pop();
            Mini_batchSizeTrain = (int)stack.Pop();

        }
    }
}
