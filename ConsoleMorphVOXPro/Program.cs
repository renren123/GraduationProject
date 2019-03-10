using ConsoleMorphVOXPro.SeriaLizer;
using ConsoleMorphVOXPro.WaveDeal;
using LeNet_5;
using LeNet_5.Con_Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMorphVOXPro
{
    class Program
    {
        static StartNet startNet;
        static int mini_batchsize = 20;
        static string saveFileName = @"1.name";
        static string  fileMaxAverageMinSaveName="1.cvs";
        static string trainPath1 = @"C:\Users\13643\Desktop\毕业设计\加工_1";
        static void Main(string[] args)
        {
            while (true)
            {
                switch (Console.ReadLine().Trim())
                {
                    case "train":
                        {
                            int.TryParse(Console.ReadLine().Trim(), out int trainNumber);
                            Train(trainNumber);
                        }
                        break;
                    case "test":
                        {
                            Test(Console.ReadLine());
                        }
                        break;
                    case "save":
                        {
                            save(saveFileName);
                        }
                        break;
                    case "load":
                        {
                            load(saveFileName);
                        }     
                        break;
                    default:
                        Console.WriteLine("Input Again!");
                        break;
                }
            }
        }
        static void save(string fileName)
        {
            if (startNet == null)
            {
                Console.WriteLine("Program->save()");
                return;
            }
            try
            {
                startNet.SetStaticValue();
                BinarySeriaLizer.SerializeMethod(startNet, saveFileName);
            }
            catch (Exception ew)
            {
                Console.WriteLine(ew.Message);
                return;
            }

            Console.WriteLine("保存成功！文件名：" + saveFileName);
        }
        static void load(string fileName)
        {
            try
            {
                startNet = (StartNet)BinarySeriaLizer.ReserializeMethod(saveFileName);
                Console.WriteLine("加载成功！文件名：" + saveFileName);
            }
            catch (Exception ew)
            {
                Console.WriteLine(ew.Message);
                return;
            }
            startNet.LoadStaticValue();
        }
        public static void Train(int trainNumber)
        {
            if (startNet == null)
            {
                startNet = new StartNet();
            }
            startNet.TrainPath = trainPath1;
            startNet.StandMiniBatchSize = mini_batchsize;
            startNet.FileMaxAverageMinSaveName = fileMaxAverageMinSaveName;
            startNet.Train(trainNumber);    
        }
        static void Test(string filePath)
        {
            startNet.Test(filePath);
        }
    }
}
