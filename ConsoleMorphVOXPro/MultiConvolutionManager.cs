using ConsoleMorphVOXPro.FileSave;
using LeNet_5.Con_Network;
using LeNet_5.Fully_Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using LeNet_5.ImgAction;


namespace LeNet_5
{
    [Serializable]
    class MultiConvolutionManager
    {
        private FileSave fileSave;
        private MultiCon multiCon;
        private FullyNet fullyNet;

        public int [] outPut { set; get; }
        public List<ConLayerInputIndex> conLayerInputIndexs { set; get; }
        public List<ArrayMap> inputMap { set; get; }
        public List<float[]> standardOutData { set; get; }// 标准的输出值，用于计算误差项
        List<float[]> OutDataArray { set; get; }
        /// <summary>
        /// 这个是每层卷积核的个数
        /// </summary>
        int[] filterNumberArray = { 32,32, 64, 64, 128,128 ,256 };
        int[] filterSize = { 3, 3, 3, 3, 3, 3,3};//卷积层共有8层
        double study_rate = 0.1;
        int NumberOfLayor { set; get; }//层数
        public string FileMaxAverageMinSaveName { set; get; }

        /// <summary>
        /// 全连接的第一层，输入层的神经元个数是根据卷积层的最后一层卷积核的个数确定的
        /// </summary>
        public int[] numberOfFullyNetworkArray = { 0,100,100,100,100,3};
        public MultiConvolutionManager()
        {
            NumberOfLayor = filterNumberArray.Length;
            AgentClass.Study_rate = study_rate;
            //OpenCLArrayCon.Init(); //调用GPU加速
            InitConLayerInputIndexs();
            fileSave = new FileSave();
            multiCon = new MultiCon
            {
                conLayerInputIndexs = conLayerInputIndexs,
                FilterSizeArray = filterSize,
                numberOfLayor = NumberOfLayor,
                FilterNumberArray = filterNumberArray
            };

            fullyNet = new FullyNet
            {
                NumberOfLayorNeuron = numberOfFullyNetworkArray
            };
        }
        public void update()
        {
            //计算ADAM
            Adam.B1_pow *= AgentClass.B1;
            Adam.B2_pow *= AgentClass.B2;
            //
            List<float[]> lossValue = loss(standardOutData, OutDataArray);
            List<float[]> outDataArrayTemp = new List<float[]>(numberOfFullyNetworkArray[numberOfFullyNetworkArray.Length - 1]);
            for (int i = 0; i < numberOfFullyNetworkArray[numberOfFullyNetworkArray.Length - 1]; i++)
            {
                float[] arrayTemp = new float[AgentClass.Mini_batchsize];
                for (int j = 0; j < AgentClass.Mini_batchsize; j++)
                {
                    arrayTemp[j] = lossValue[j][i];
                }
                outDataArrayTemp.Add(arrayTemp);
            }
            //ReduceMemory.SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle.ToInt32(), -1, -1);
            fullyNet.lossValue = outDataArrayTemp;
            fullyNet.updateWi();
            multiCon.poolingSensitives = AgentClass.ListToMaps(fullyNet.OutSensitive);
            multiCon.update();
        }
        public void execute()
        {
            multiCon.inputMaps = inputMap;
            multiCon.caculate();
            List<float[]> fulInput = AgentClass.MapsToList(multiCon.poolingMaps);
            fullyNet.InputArray = fulInput;
            fullyNet.caculate();
            OutDataArray = fullyNet.outData;
        }
        public string ApproachPercent()
        {
            double max = -1000,min=1000;
            double result = 0;
            for(int i=0;i<OutDataArray.Count;i++)
            {
                for(int j=0;j<OutDataArray[0].Length;j++)
                {
                    double number= Math.Abs(OutDataArray[i][j] - standardOutData[i][j]);
                    if (number > max)
                        max = number;
                    if (number < min)
                        min = number;
                    result += number;
                }
            }
            double average = result / (OutDataArray.Count * OutDataArray[0].Length);
            fileSave.SaveMaxAveRageMin(FileMaxAverageMinSaveName, max, average, min);
            return "Max:"+max+"\t"+"Average:"+average +"\t"+"Min:"+min;
        }
        public string GetResultOut(bool isTrain)
        {
            if(isTrain)
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < OutDataArray.Count; i++)
                {
                    string line = "";
                    for (int j = 0; j < OutDataArray[i].Length; j++)
                    {
                        line += Math.Round(OutDataArray[i][j], 2) + "\t";
                    }
                    stringBuilder.AppendLine(line);
                }
                return stringBuilder.ToString();
            }
            else
            {
                string line = "";
                for (int j = 0; j < OutDataArray[0].Length; j++)
                {
                    line += Math.Round(OutDataArray[0][j], 2) + "\t";
                }
                return line;
            }
            
        }
        public void getOutPut()
        {
            if (outPut==null)
            {
                outPut = new int[AgentClass.Mini_batchsize];
            }
            for (int i = 0; i < OutDataArray.Count; i++)
            {
                float temp = 0;
                int index = -1;
                for (int j = 0; j < OutDataArray[i].Length; j++)
                {
                    if (OutDataArray[i][j] > temp)
                    {
                        temp = OutDataArray[i][j];
                        index = j;
                    }
                }
                outPut[i] = index;
            }
        }
        public bool isNaN(List<float[]> array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                for (int j = 0; j < array[i].Length; j++)
                {
                    if (float.IsNaN( array[i][j]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public List<float[]> loss(List<float[]> t , List<float[]> yi)
        {
            List<float[]> stan = new List<float[]>(t.Count);
            for (int i = 0; i < t.Count; i++)
            {
                stan.Add(new float[t[0].Length]);
            }
            for (int i = 0; i < t.Count; i++)
            {
                for (int j = 0; j < t[i].Length; j++)
                {
                    stan[i][j] = yi[i][j] - t[i][j];
                }
            }
            return stan;
        }
        private void InitConLayerInputIndexs()
        {
            conLayerInputIndexs = new List<ConLayerInputIndex>(NumberOfLayor);
            for (int i = 0; i < NumberOfLayor; i++)
            {
                conLayerInputIndexs.Add(new ConLayerInputIndex());
            }
            //第一层是全卷积
            conLayerInputIndexs[0].listConInputMapIndex = new List<int[]>(filterNumberArray[0]);
            for (int i = 0; i < filterNumberArray[0]; i++)
            {
                conLayerInputIndexs[0].listConInputMapIndex.Add(new[] { 0, 1 });
            }
            ////第二层是错开卷积1、2、3//
            //conLayerInputIndexs[1].listConInputMapIndex = new List<int[]>(filterNumberArray[1]);
            //for (int i=0;i< filterNumberArray[1];i++)
            //{
            //    conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { (i)% filterNumberArray[0], (i + 1) % filterNumberArray[0], (i + 2) % filterNumberArray[0] });
            //}

            ////第三层是错开卷积1、3、5//2、4、6//
            //conLayerInputIndexs[2].listConInputMapIndex = new List<int[]>(filterNumberArray[2]);
            //for (int i = 0; i < filterNumberArray[2]; i++)
            //{
            //    conLayerInputIndexs[2].listConInputMapIndex.Add(new[] { (i) % filterNumberArray[1], (i + 2) % filterNumberArray[1], (i + 4) % filterNumberArray[1] });
            //}

            ////第四层是卷积1、2、3//1、2、3、4//
            //conLayerInputIndexs[3].listConInputMapIndex = new List<int[]>(filterNumberArray[3]);
            //for (int i = 0; i < filterNumberArray[3]/2; i++)
            //{
            //    conLayerInputIndexs[3].listConInputMapIndex.Add(new[] { (i) % filterNumberArray[2], (i + 1) % filterNumberArray[2], (i + 2) % filterNumberArray[2] });
            //}
            //for (int i = 0; i < filterNumberArray[3] / 2; i++)
            //{
            //    conLayerInputIndexs[3].listConInputMapIndex.Add(new[] { (i) % filterNumberArray[2], (i + 1) % filterNumberArray[2], (i + 2) % filterNumberArray[2], (i + 3) % filterNumberArray[2] });
            //}

            ////第五层是错开卷积1、2、3//
            //conLayerInputIndexs[4].listConInputMapIndex = new List<int[]>(filterNumberArray[4]);
            //for (int i = 0; i < filterNumberArray[4]; i++)
            //{
            //    conLayerInputIndexs[4].listConInputMapIndex.Add(new[] { (i) % filterNumberArray[3], (i + 1) % filterNumberArray[3],(i + 2) % filterNumberArray[3] });
            //}

            ////第六层是卷积1、2、3//1、2、3、4//
            //conLayerInputIndexs[5].listConInputMapIndex = new List<int[]>(filterNumberArray[5]);
            //for (int i = 0; i < filterNumberArray[5] / 2; i++)
            //{
            //    conLayerInputIndexs[5].listConInputMapIndex.Add(new[] { (i) % filterNumberArray[4], (i + 1) % filterNumberArray[4], (i + 2) % filterNumberArray[4] });
            //}
            //for (int i = 0; i < filterNumberArray[5] / 2; i++)
            //{
            //    conLayerInputIndexs[5].listConInputMapIndex.Add(new[] { (i) % filterNumberArray[4], (i + 1) % filterNumberArray[4], (i + 2) % filterNumberArray[4], (i + 3) % filterNumberArray[4] });
            //}

            ////第七层是卷积1、2、3//1、2、3、4//
            //conLayerInputIndexs[6].listConInputMapIndex = new List<int[]>(filterNumberArray[6]);
            //for (int i = 0; i < filterNumberArray[6] / 2; i++)
            //{
            //    conLayerInputIndexs[6].listConInputMapIndex.Add(new[] { (i) % filterNumberArray[5], (i + 1) % filterNumberArray[5], (i + 2) % filterNumberArray[5] });
            //}
            //for (int i = 0; i < filterNumberArray[6] / 2; i++)
            //{
            //    conLayerInputIndexs[6].listConInputMapIndex.Add(new[] { (i) % filterNumberArray[5], (i + 1) % filterNumberArray[5], (i + 2) % filterNumberArray[5], (i + 3) % filterNumberArray[5] });
            //}

            for (int i = 1; i < filterNumberArray.Length; i++)
            {
                conLayerInputIndexs[i].listConInputMapIndex = new List<int[]>(filterNumberArray[i]);
                for (int j = 0; j < filterNumberArray[i]; j++)
                {
                    conLayerInputIndexs[i].listConInputMapIndex.Add(GetJuanJi(0, filterNumberArray[i - 1]));
                }
            }
            ////以下每一层都是全卷积,第二层卷积
            //conLayerInputIndexs[1].listConInputMapIndex = new List<int[]>(filterNumberArray[1]);
            //for(int i=0;i< filterNumberArray[1];i++)
            //{
            //    conLayerInputIndexs[1].listConInputMapIndex.Add(GetJuanJi(0, filterNumberArray[0] - 1));
            //}
            //conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { 0, 1, 2 });
            //conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { 1, 2, 3 });
            //conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { 2, 3, 4 });
            //conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { 3, 4, 5 });
            //conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { 4, 5, 0 });
            //conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { 5, 0, 1 });

            //conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { 0, 1, 2, 3 });
            //conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { 1, 2, 3, 4 });
            //conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { 2, 3, 4, 5 });
            //conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { 3, 4, 5, 0 });
            //conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { 4, 5, 0, 1 });
            //conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { 5, 0, 1, 2 });

            //conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { 0, 1, 3, 4 });
            //conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { 1, 2, 4, 5 });
            //conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { 0, 2, 3, 5 });

            //conLayerInputIndexs[1].listConInputMapIndex.Add(new[] { 0, 1, 2, 3, 4, 5 });
            ////最后一层是全卷积
            //conLayerInputIndexs[2].listConInputMapIndex = new List<int[]>(filterNumberArray[2]);
            //for (int i = 0; i < filterNumberArray[2]; i++)
            //{
            //    conLayerInputIndexs[2].listConInputMapIndex.Add(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            //}
        }
        /// <summary>
        /// 不包括end
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private int [] GetJuanJi(int start ,int end)
        {
            int[] array = new int[end - start];
            for(int i=start;i<end;i++)
            {
                array[i] = i;
            }
            return array;
        }
    }
}
