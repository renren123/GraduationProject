using ConsoleMorphVOXPro.SeriaLizer;
using ConsoleMorphVOXPro.WaveDeal;
using LeNet_5;
using LeNet_5.Fully_Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMorphVOXPro
{
    [Serializable]
    class StartNet
    {
        /// <summary>
        /// 这个是存储静态公共变量
        /// </summary>
        private Stack<object> StaticValueStack { set; get; }
        /// <summary>
        /// 这个值用于设置miniBatch的大小,标准MiniBatchSize
        /// </summary>
        public int StandMiniBatchSize { set; get; }
        public string TrainPath { set; get; }
        public string FileMaxAverageMinSaveName { set; get; }
        private MultiConvolutionManager multi;
        private WaveManager waveManager;
        public void Train(int trainNumber)
        {
            if (Directory.Exists(TrainPath) == false)
            {
                Console.WriteLine("ConsoleMorphVOXPro->StartNet->Train");
                return;
            }
            if (multi == null)
            {
                Adam.T_step = 1;
                multi = new MultiConvolutionManager();
                multi.FileMaxAverageMinSaveName = FileMaxAverageMinSaveName;           
            }
            AgentClass.Mini_batchsize = StandMiniBatchSize;
            AgentClass.Mini_batchSizeTrain = StandMiniBatchSize;
            ClassPublicValue.isReadyToUbAndSigmma = false;
            ClassPublicValue.TrainOrTest = "train";
            if (waveManager == null)
            {
                waveManager = new WaveManager();
                waveManager.GetWaveToList(TrainPath);
            }
            CaculateTimes caculateTimes = new CaculateTimes();
            for (int i = 0; i < trainNumber; i++)
            {
                if (i == trainNumber - 1)
                    ClassPublicValue.isReadyToUbAndSigmma = true;
                waveManager.DoListRandom();
                int jCount = waveManager.fileInfos.Count % AgentClass.Mini_batchsize == 0 ? (waveManager.fileInfos.Count / AgentClass.Mini_batchsize) : (waveManager.fileInfos.Count / AgentClass.Mini_batchsize + 1);

                for (int j = 0; j < waveManager.fileInfos.Count; j += AgentClass.Mini_batchsize)
                {
                    caculateTimes.StartTime();
                    List<float[]> results = new List<float[]>(AgentClass.Mini_batchsize);
                    multi.inputMap = waveManager.GetMini_batchList(j, results);
                    multi.execute();
                    //System.Threading.Thread.Sleep(100);
                    Console.WriteLine("计算值：");
                    Console.WriteLine(multi.GetResultOut(true));
                    Console.WriteLine("目标值：");
                    Console.WriteLine(GetString(results));
                    multi.standardOutData = results;
                    Console.WriteLine("逼近值：" + multi.ApproachPercent());
                    multi.update();
                    Adam.T_step++;
                    Console.WriteLine("episode: " + i + "\t" + "miniCount: " + j);
                    Console.WriteLine(caculateTimes.EndTime((trainNumber - i) * jCount - j/ StandMiniBatchSize - 1));
                }
            }
            Console.WriteLine("训练结束！");
        }
        public void Test(string filePath)
        {
            if (File.Exists(filePath) == false)
            {
                Console.WriteLine(filePath + " 文件不存在！ConsoleMorphVOXPro->StartNet->test()");
                return;
            }
            ClassPublicValue.TrainOrTest = "test";
            AgentClass.Mini_batchsize = 1;
            WaveManager waveManagerTest = new WaveManager();
            multi.inputMap=waveManager.GetTest(filePath);
            multi.execute();
            Console.WriteLine("计算值：");
            Console.WriteLine(multi.GetResultOut(false));
        }

        private static string GetString(List<float[]> results)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < results.Count; i++)
            {
                string line = "";
                for (int j = 0; j < results[i].Length; j++)
                {
                    line += Math.Round(results[i][j], 2) + "\t";
                }
                stringBuilder.AppendLine(line);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 将静态变量进栈
        /// </summary>
        public void SetStaticValue()
        {
            if (StaticValueStack == null)
            {
                StaticValueStack = new Stack<object>();
            }
            else
            {
                StaticValueStack.Clear();
            }
            StaticValueStack = Adam.SetStaticValue(StaticValueStack);
            StaticValueStack = ClassPublicValue.SetStaticValue(StaticValueStack);
            StaticValueStack = AgentClass.SetStaticValue(StaticValueStack);
        }
        /// <summary>
        /// 将静态变量出栈
        /// </summary>
        public void LoadStaticValue()
        {
            if (StaticValueStack == null || StaticValueStack.Count == 0)
            {
                Console.WriteLine("StartNet->LoadStaticValue()");
                return;
            }
            AgentClass.GetStaticValue(StaticValueStack);
            ClassPublicValue.GetStaticValue(StaticValueStack);
            Adam.GetStackValue(StaticValueStack);
        }
        
    }
}
