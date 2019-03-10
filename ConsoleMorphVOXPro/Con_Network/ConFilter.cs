using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LeNet_5.Con_Network
{
    [Serializable]
    class ConFilter
    {
        /// <summary>
        /// 对应的卷积的那几个输入
        /// </summary>
        public int[] ConInputMapIndex { set; get; }
        /// <summary>
        /// 这个数值是为了调整全连接层与卷积层学习速率的问题
        /// </summary>
        private float adjustBNNumber = 1;
        private Random r;

        /// <summary>
        /// 主要用于记录Wi对应的Inputmap的序号，它的长度就是Wi的深度
        /// </summary>
        private List<float[,]> BNOut { set; get; }
        /// <summary>
        /// 正方形,是指3*3的大小不是指深度
        /// </summary>
        public int Filter_size { set; get; }
        public List<float[,]> Wi { set; get; }
        private List<float[,]> V { set; get; }
        private List<float[,]> S { set; get; }
        private List<int[,,]> PositionOfPoolingMap { set; get; }
        /// <summary>
        /// 每个Filter对应一个BN
        /// </summary>
        public ConBN conBN { set; get; }
        public ConFilter()
        {
            var seed = Guid.NewGuid().GetHashCode();
            r = new Random(seed);
            //conBN = new ConBN();
        }
        public void InitWi(int sumInput)
        {
            if (Wi == null || Wi.Count == 0)
            {
                Console.WriteLine("Error!InitWi");
                return;
            }
            for (int i = 0; i < Wi.Count; i++)
            {
                for (int x = 0; x < Wi[i].GetLength(0); x++)
                {
                    for (int y = 0; y < Wi[i].GetLength(1); y++)
                    {
                        Wi[i][x, y] = W_value_method(sumInput);
                    }
                }
            }
        }
        private float W_value_method(int sumInput)
        {
            float y = (float)r.NextDouble();
            float x = (float)r.NextDouble();
            float number = (float)(Math.Cos(2 * Math.PI * x) * Math.Sqrt(-2 * Math.Log(1 - y)));
            number *= (float)Math.Sqrt(2.0 / sumInput);
            return number;
        }
        /// <summary>
        /// 返回值是Minibatchsize个样本的feather
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns></returns>
        public List<float[,]> forward(List<ArrayMap> inputArray)
        {
            if (inputArray == null)
            {
                Console.WriteLine("Error!位置：\r\n->ConFilter->forward");
                return null;
            }
            if (conBN == null)
            {
                conBN = new ConBN();
            }
            conBN.xin = ConAction(inputArray, conBN.xin);
            //conBN.xin已经有了
            BNOut = conBN.GetX_out(BNOut);
            List<float[,]> featherMap = F(BNOut);
            List<float[,]> poolingMap = Pooling(featherMap);
            return poolingMap;
        }
        private List<float[,]> ConAction(List<ArrayMap> inputArray, List<float[,]> xout)
        {
            if (xout==null)
            {
                xout = new List<float[,]>(inputArray.Count);
            }
            else if (xout.Count>0)
            {
                xout.Clear();
            }
            //每个filter对应一个输入xin，这个是对应样本的数量
            int featherW = inputArray[0].map[0].GetLength(0) - Filter_size + 1;
            int featherH = inputArray[0].map[0].GetLength(1) - Filter_size + 1;
            for (int i = 0; i < inputArray.Count; i++)
            {
                xout.Add(new float[featherW, featherH]);
            }
            //inputArray.Count是样本的数量，一个inputArray[i]是一个样本
            Parallel.For(0, inputArray.Count,(xp=>{
                int i = xp;
                xout[i] = ArrayConvoLute(inputArray[i], xout[i]);
            }));
            //不用并行
            //for (int i = 0; i < inputArray.Count; i++)
            //{
            //    xout[i] = ArrayConvoLute(inputArray[i], xout[i]);
            //}
            return xout;
        }
        /// <summary>
        /// 返回一个新的featherSensitiveMap
        /// </summary>
        /// <param name="poolingSensitive"></param>
        /// <param name="positionOfPoolingMap"></param>
        /// <param name="featherSensitiveW"></param>
        /// <param name="featherSensitiveH"></param>
        /// <returns>返回一个新的featherSensitiveMap</returns>
        private List<float[,]> GetFeatherSensitiveMap(List<float[,]> poolingSensitive, List<int[,,]> positionOfPoolingMap, int featherSensitiveW, int featherSensitiveH)
        {
            List<float[,]> featherSensitiveMap = new List<float[,]>(poolingSensitive.Count);
            for (int i = 0; i < poolingSensitive.Count; i++)
            {
                featherSensitiveMap.Add(new float[featherSensitiveW, featherSensitiveH]);
            }
            for (int i = 0; i < featherSensitiveMap.Count; i++)
            {
                for (int j = 0; j < poolingSensitive[i].GetLength(0); j++)
                {
                    for (int k = 0; k < poolingSensitive[i].GetLength(1); k++)
                    {
                        featherSensitiveMap[i][positionOfPoolingMap[i][j, k, 0], positionOfPoolingMap[i][j, k, 1]] = poolingSensitive[i][j, k];
                    }
                }
            }
            return featherSensitiveMap;
        }

        private List<ArrayMap> ConActionBack(List<float[,]> bnoutSensitive, List<ArrayMap> inputArraySensitive)
        {
            int W = inputArraySensitive[0].map[0].GetLength(0) + Filter_size - 1;
            int H = inputArraySensitive[0].map[0].GetLength(1) + Filter_size - 1;
            int xadd = (W - bnoutSensitive[0].GetLength(0)) / 2;
            int yadd = (H - bnoutSensitive[0].GetLength(1)) / 2;
            List<float[,]> inputSensitiveAdd0 = new List<float[,]>(bnoutSensitive.Count);
            for (int i = 0; i < bnoutSensitive.Count; i++)
            {
                inputSensitiveAdd0.Add(new float[W, H]);
            }
            for (int i = 0; i < inputSensitiveAdd0.Count; i++)
            {
                for (int j = 0; j < bnoutSensitive[i].GetLength(0); j++)
                {
                    for (int k = 0; k < bnoutSensitive[i].GetLength(1); k++)
                    {
                        inputSensitiveAdd0[i][j + xadd, k + yadd] = bnoutSensitive[i][j, k];
                    }
                }
            }
            List<float[,]> correlation_wi = new List<float[,]>(Wi.Count);
            for (int i = 0; i < Wi.Count; i++)
            {
                correlation_wi.Add(new float[Wi[i].GetLength(0), Wi[i].GetLength(1)]);
            }
            int Wi_W = Filter_size - 1;
            int Wi_H = Filter_size - 1;
            for (int i = 0; i < Wi.Count; i++)
            {
                for (int x = 0; x < Wi[i].GetLength(0); x++)
                {
                    for (int y = 0; y < Wi[i].GetLength(1); y++)
                    {
                        correlation_wi[i][x, y] = Wi[i][Wi_W - x, Wi_H - y];
                    }
                }
            }
            //inputSensitiveAdd0  minibatch个
            Parallel.For(0, inputArraySensitive.Count, (xp) => {
                int i = xp;
                for (int j = 0; j < ConInputMapIndex.Length; j++)
                {
                    int whichMap = ConInputMapIndex[j];
                    inputArraySensitive[i].map[whichMap] = ArraySensitiveConvoLute(inputSensitiveAdd0[i], inputArraySensitive[i].map[whichMap], correlation_wi[j]);
                }
            });
            //不用并行
            //for(int i=0;i< inputArraySensitive.Count;i++)
            //{
            //    for (int j = 0; j < ConInputMapIndex.Length; j++)
            //    {
            //        int whichMap = ConInputMapIndex[j];
            //        inputArraySensitive[i].map[whichMap] = ArraySensitiveConvoLute(inputSensitiveAdd0[i], inputArraySensitive[i].map[whichMap], correlation_wi[j]);
            //    }
            //}
            //for (int i = 0; i < inputArraySensitive.Count; i++)
            //{
            //    for (int j = 0; j < ConInputMapIndex.Length; j++)
            //    {
            //        int whichMap = ConInputMapIndex[j];
            //        inputArraySensitive[i].map[whichMap] = ArraySensitiveConvoLute(inputSensitiveAdd0[i], inputArraySensitive[i].map[whichMap], correlation_wi[j]);
            //    }
            //    //for (int j = 0; j < inputArraySensitive[i].map.Count; j++)
            //    //{
            //    //    inputArraySensitive[i].map[j] = ArraySensitiveConvoLute(inputSensitiveAdd0[i], inputArraySensitive[i].map[j], correlation_wi[j]);
            //    //}
            //}
            
            return inputArraySensitive;
        }
        
        /// <summary>
        /// 在反向传播到这一层时记得要对inputArraySensitive初始化为0
        /// </summary>
        /// <param name="poolingSensitive">是一个minibatch样本的值</param>
        /// <param name="inputArray">用于更新权值</param>
        /// <returns></returns>
        public List<ArrayMap> backward(List<float[,]> poolingSensitive, List<ArrayMap> inputArray, List<ArrayMap> inputArraySensitive)
        {
            List<float[,]> feaherSensitiveMap = GetFeatherSensitiveMap(poolingSensitive, PositionOfPoolingMap, conBN.xin[0].GetLength(0), conBN.xin[0].GetLength(1));
            List<float[,]> foutSensitive = F_back(feaherSensitiveMap);
            List<float[,]> bnoutSensitive = conBN.GetDout(foutSensitive);
            inputArraySensitive = ConActionBack(bnoutSensitive, inputArraySensitive);

            ////不加BN
            //List<float[,]> bnout = F_back(feaherSensitiveMap, conBN.xin);

            UpdateWi(inputArray, bnoutSensitive);
            return inputArraySensitive;
        }
        private void UpdateWi(List<ArrayMap> inputArray, List<float[,]> bnout)
        {
            //这是minibatch个样本的总的dwis
            List<List<float[,]>> dwis = new List<List<float[,]>>(inputArray.Count);
            for (int i = 0; i < inputArray.Count; i++)
            {
                dwis.Add(new List<float[,]>(Wi.Count));
                for (int j = 0; j < Wi.Count; j++)
                {
                    dwis[i].Add(new float[Filter_size, Filter_size]);
                }
            }
            Parallel.For(0, inputArray.Count, (xp) => {
                int i = xp;
                for (int j = 0; j < inputArray[i].map.Count; j++)
                {
                    dwis[i][j] = ArrayWiConvoLute(inputArray[i].map[j], dwis[i][j], bnout[i]);
                }
            });
            //不用并行
            //for (int i = 0; i < inputArray.Count; i++)
            //{
            //    //dwis[i] = ArrayWiConvoLute(inputArray[i].map, dwis[i], bnout[i]);
            //    for (int j = 0; j < inputArray[i].map.Count; j++)
            //    {
            //        dwis[i][j] = ArrayWiConvoLute(inputArray[i].map[j], dwis[i][j], bnout[i]);
            //    }
            //}
            List<float[,]> dwAverage = ArrayAction.ListAverage(dwis);
            if (V == null)
            {
                V = new List<float[,]>(Wi.Count);
                for (int i = 0; i < Wi.Count; i++)
                {
                    V.Add(new float[Filter_size, Filter_size]);
                }
            }
            if (S == null)
            {
                S = new List<float[,]>(Wi.Count);
                for (int i = 0; i < Wi.Count; i++)
                {
                    S.Add(new float[Filter_size, Filter_size]);
                }
            }
            float agentClass_B1_temp = (float)(1 - AgentClass.B1);
            for (int i = 0; i < V.Count; i++)
            {
                for (int x = 0; x < V[i].GetLength(0); x++)
                {
                    for (int y = 0; y < V[i].GetLength(1); y++)
                    {
                        V[i][x, y] = (float)(AgentClass.B1 * V[i][x, y] + agentClass_B1_temp * dwAverage[i][x, y]);
                    }
                }
            }
            float agentClass_B2_temp = (float)(1 - AgentClass.B2);
            for (int i = 0; i < S.Count; i++)
            {
                for (int x = 0; x < S[i].GetLength(0); x++)
                {
                    for (int y = 0; y < S[i].GetLength(1); y++)
                    {
                        S[i][x, y] = (float)(AgentClass.B2 * S[i][x, y] + agentClass_B2_temp * dwAverage[i][x, y] * dwAverage[i][x, y]);
                    }
                }
            }
            List<float[,]> V_cor = new List<float[,]>(Wi.Count);
            List<float[,]> S_cor = new List<float[,]>(Wi.Count);
            for (int i = 0; i < Wi.Count; i++)
            {
                V_cor.Add(new float[Filter_size, Filter_size]);
                S_cor.Add(new float[Filter_size, Filter_size]);
            }

            float b1_t = (float)(1 - Adam.B1_pow);
            float b2_t = (float)(1 - Adam.B2_pow);
            for (int i = 0; i < V_cor.Count; i++)
            {
                for (int x = 0; x < V_cor[i].GetLength(0); x++)
                {
                    for (int y = 0; y < V_cor[i].GetLength(1); y++)
                    {
                        V_cor[i][x, y] = V[i][x, y] / b1_t;
                        S_cor[i][x, y] = S[i][x, y] / b2_t;
                    }
                }
            }


            //更新
            for (int i = 0; i < S_cor.Count; i++)
            {
                int Wtemp = S_cor[i].GetLength(0);
                int Htemp = S_cor[i].GetLength(1);
                for (int x = 0; x < Wtemp; x++)
                {
                    for (int y = 0; y < Htemp; y++)
                    {
                        Wi[i][x, y] -= (float)(adjustBNNumber * AgentClass.Study_rate * V_cor[i][x, y] / (Math.Sqrt(S_cor[i][x, y]) + Adam.E));
                    }
                }
            }
        }
        private List<float[,]> Pooling(List<float[,]> feahterMap)
        {
            List<float[,]> poolingMap = new List<float[,]>(feahterMap.Count);
            int x_pooling = (feahterMap[0].GetLength(0) + 1) / 2;
            int y_pooling = (feahterMap[0].GetLength(1) + 1) / 2;
            for (int i = 0; i < feahterMap.Count; i++)
            {
                poolingMap.Add(new float[x_pooling, y_pooling]);
            }
            if (PositionOfPoolingMap == null)
            {
                PositionOfPoolingMap = new List<int[,,]>(feahterMap.Count);
                for (int i = 0; i < feahterMap.Count; i++)
                {
                    PositionOfPoolingMap.Add(new int[x_pooling, y_pooling, 2]);
                }
            }
          
            //申请一个临时的
            List<float[,]> featherMapTemp = new List<float[,]>(feahterMap.Count);
            for (int i = 0; i < feahterMap.Count; i++)
            {
                featherMapTemp.Add(new float[2 * x_pooling, 2 * y_pooling]);
            }
            for (int i = 0; i < feahterMap.Count; i++)
            {
                for (int n = 0; n < feahterMap[i].GetLength(0); n++)
                {
                    for (int m = 0; m < feahterMap[i].GetLength(1); m++)
                    {
                        featherMapTemp[i][n, m] = feahterMap[i][n, m];
                    }
                }
            }
            ///////////////
            for (int pooling_index = 0; pooling_index < poolingMap.Count; pooling_index++)
            {
                float[,] poolingArray = poolingMap[pooling_index];
                float[,] featherArrayTemp = featherMapTemp[pooling_index];
                for (int i = 0; i < poolingArray.GetLength(0); i++)
                {
                    for (int j = 0; j < poolingArray.GetLength(1); j++)
                    {
                        float temp = featherArrayTemp[2 * i, 2 * j];
                        int xx = 2 * i, yy = 2 * j;//最大值的坐标
                        if (temp < featherArrayTemp[2 * i, 2 * j + 1])
                        {
                            temp = featherArrayTemp[2 * i, 2 * j + 1];
                            yy = 2 * j + 1;
                        }
                        if (temp < featherArrayTemp[2 * i + 1, 2 * j])
                        {
                            xx = 2 * i + 1;
                            temp = featherArrayTemp[2 * i + 1, 2 * j];
                        }
                        if (temp < featherArrayTemp[2 * i + 1, 2 * j + 1])
                        {
                            temp = featherArrayTemp[2 * i + 1, 2 * j + 1];
                            xx = 2 * i + 1;
                            yy = 2 * j + 1;
                        }
                        poolingArray[i, j] = temp;
                        if (ClassPublicValue.TrainOrTest.Equals("train"))
                        {
                            PositionOfPoolingMap[pooling_index][i, j, 0] = xx;
                            PositionOfPoolingMap[pooling_index][i, j, 1] = yy;
                        }
                    }
                }
            }
            return poolingMap;
        }
        private List<float[,]> F_back(List<float[,]> xout)
        {
            for (int i = 0; i < xout.Count; i++)
            {
                for (int j = 0; j < xout[i].GetLength(0); j++)
                {
                    for (int k = 0; k < xout[i].GetLength(1); k++)
                    {
                        if (BNOut[i][j, k] < 0)
                        {
                            xout[i][j, k] = 0;
                        }
                    }
                }
            }
            return xout;
        }
        private List<float[,]> F(List<float[,]> xin)
        {
            List<float[,]> xout = new List<float[,]>(xin.Count);
            for (int i = 0; i < xin.Count; i++)
            {
                xout.Add(new float[xin[i].GetLength(0), xin[i].GetLength(1)]);
            }
            for (int i = 0; i < xout.Count; i++)
            {
                for (int x = 0; x < xout[i].GetLength(0); x++)
                {
                    for (int y = 0; y < xout[i].GetLength(1); y++)
                    {
                        xout[i][x, y] = xin[i][x, y];
                        if (xout[i][x, y]<0)
                        {
                            xout[i][x, y] = 0;
                        }
                    }
                }
            }
            return xout;
        }
        private float GetArrayConvoLuteNumber(int xx, int yy, float[,] inputArray, float[,] ConvolutionKernel)
        {
            float sum = 0;
            int W = ConvolutionKernel.GetLength(0);
            int H = ConvolutionKernel.GetLength(1);
            for (int i = 0; i <W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    sum += inputArray[xx + i, yy + j] * ConvolutionKernel[i, j];
                }
            }
            return sum;
        }
        private float GetAllArrayConvoLuteResult(ArrayMap inputArray,int xx,int yy)
        {
            float sum = 0;
            for (int i = 0; i < ConInputMapIndex.Length; i++)
            {
                sum += GetArrayConvoLuteNumber(xx, yy, inputArray.map[ConInputMapIndex[i]], Wi[i]);
            }
            return sum;
        }
        /// <summary>
        /// 这个是正常的
        /// </summary>
        /// <param name="inputArray"></param>
        /// <param name="aimArray"></param>
        /// <param name="ConvolutionKernel"></param>
        /// <returns></returns>
        private float[,] ArrayWiConvoLute(float[,] inputArray, float[,] aimArray, float[,] ConvolutionKernel)
        {
            //GPU并行计算的代码，但没有成功
            //aimArray = OpenCLArrayCon.convolute(inputArray,aimArray, ConvolutionKernel);
            int Wtemp = aimArray.GetLength(0);
            int Htemp = aimArray.GetLength(1);
            for (int i = 0; i < Wtemp; i++)
            {
                for (int j = 0; j < Htemp; j++)
                {
                    aimArray[i, j] = GetArrayConvoLuteNumber(i, j, inputArray, ConvolutionKernel);
                }
            }
            return aimArray;
        }
        /// <summary>
        /// 注意这个和其他的不一样,这个是 aimArray[i, j] +=
        /// </summary>
        /// <param name="inputArray"></param>
        /// <param name="aimArray"></param>
        /// <param name="ConvolutionKernel"></param>
        /// <returns></returns>
        private float[,] ArraySensitiveConvoLute(float[,] inputArray,float [,] aimArray, float[,] ConvolutionKernel)
        {
            if (aimArray==null)
            {
                Console.WriteLine("Error!->ArraySensitiveConvoLute");
                return null;
            }
            int Wtemp = aimArray.GetLength(0);
            int Htemp = aimArray.GetLength(1);
            for (int i = 0; i < Wtemp; i++)
            {
                for (int j = 0; j < Htemp; j++)
                {
                    aimArray[i, j] += GetArrayConvoLuteNumber(i, j, inputArray, ConvolutionKernel);
                }
            }
            return aimArray;
        }
        private float[,] ArrayConvoLute(ArrayMap inputArray,float[,] aimArray)
        {
            int Wtemp = aimArray.GetLength(0);
            int Htemp = aimArray.GetLength(1);
            for (int i = 0; i < Wtemp; i++)
            {
                for (int j = 0; j < Htemp; j++)
                {
                    aimArray[i, j] = GetAllArrayConvoLuteResult(inputArray,i,j);
                }
            }
            return aimArray;
        }
    }
}
