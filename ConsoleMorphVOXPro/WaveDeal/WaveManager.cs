using LeNet_5;
using LeNet_5.Con_Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMorphVOXPro.WaveDeal
{
    [Serializable]
    class WaveManager
    {
        private List<WaveMap> waveMaps;//队列
        private int enlargementTimes = 2;//扩大倍数
        private int waveZipTimes = 10;//wave文件压缩的倍数
        private string fileName;
        //public List<WaveMap> waveMaps;
        public List<FileInfo> fileInfos;
        public WaveManager() { }
        public WaveManager(string fileName)
        {
            this.fileName = fileName;
        }
        public void GetWaveToList(string fileName)
        {
            if(fileName==null)
            {
                Console.WriteLine("ConsoleMorphVOXPro.WaveDeal->GetWaveToList->1");
                return;
            }
            this.fileName = fileName;
            //if (waveMaps == null)
            //    waveMaps = new List<WaveMap>();
            //else
            //    waveMaps.Clear();
            if (!Directory.Exists(fileName))
            {
                Console.WriteLine("ConsoleMorphVOXPro.WaveDeal->GetWaveToList->2");
                return;
            }
            DirectoryInfo folder = new DirectoryInfo(fileName);
            fileInfos = folder.GetFiles("*.wav").ToList();
            //FileInfo[] fileInfos = folder.GetFiles("*.wav");
            //Parallel.For(0, fileInfos.Length, (XP) =>
            //{
            //    int i = XP;
            //    FileInfo file = fileInfos[i];
            //    GetShiftNumber(file.Name, out float shiftNumber, out float deviation, out float threshold);
            //    List<WaveMap> waves = GetWaveMaps(file.FullName, shiftNumber, deviation, threshold);
            //    for (int j = 0; j < waves.Count; j++)
            //    {
            //        waveMaps.Add(waves[j]);
            //    }
            //});

            //foreach (FileInfo file in folder.GetFiles("*.wav"))
            //{
            //    GetShiftNumber(file.Name, out float shiftNumber, out float deviation, out float threshold);
            //    List<WaveMap> waves = GetWaveMaps(file.FullName, shiftNumber, deviation, threshold);
            //    for (int i = 0; i < waves.Count; i++)
            //    {
            //        waveMaps.Add(waves[i]);
            //    }
            //}

        }
        private WAVReader ZipWav(WAVReader wAVReader)
        {
            WAVReader newWav = new WAVReader();
            newWav.stereos = new List<Stereo>();

            for (int i = wAVReader.stereos.Count - 1; i >= 0; i -= waveZipTimes)
            {
                newWav.stereos.Add(wAVReader.stereos[i]);
                //wAVReader.stereos.RemoveAt(i);
            }
            return newWav;
        }
        private WaveMap GetWaveMaps(string fileName,float shiftNumber=0,float deviation=0,float threshold=0)
        {
            WAVReader wAVReader = new WAVReader();
            wAVReader.ReadWAVFile(fileName);
            WAVReader newWAVReader = ZipWav(wAVReader);
            EndpointDetection endpoint = new EndpointDetection();
            newWAVReader = endpoint.EndpointDetectionDeal(newWAVReader);
            //Stereo test169 = wAVReader.stereos[169];
            //Stereo test170 = wAVReader.stereos[170];
            //Stereo test171 = wAVReader.stereos[171];
            //List<WaveMap> waveMaps = new List<WaveMap>();
            Spectrogram spectrogram = new Spectrogram();
            return spectrogram.GetSpectrogram(newWAVReader.stereos, shiftNumber, deviation, threshold);
        }
        private void GetShiftNumber(string fileName,out float shiftNumber,out float deviation,out float threshold)
        {
            string[] splitArray = fileName.Split('_');
            if(float.TryParse(splitArray[1], out shiftNumber) ==false)
            {
                Console.WriteLine("ConsoleMorphVOXPro.WaveDeal->GetShiftNumber");
            }
            if (float.TryParse(splitArray[2], out deviation) == false)
            {
                Console.WriteLine("ConsoleMorphVOXPro.WaveDeal->GetShiftNumber");
            }
            string[] splitArray2 = splitArray[3].Split('.');

            if (float.TryParse(splitArray2[0], out threshold) == false)
            {
                Console.WriteLine("ConsoleMorphVOXPro.WaveDeal->GetShiftNumber");
            }
        }

        public List<ArrayMap> GetMini_batchList(int index, List<float[]> results, int length = 3)
        {
            List<ArrayMap> arrayMaps = new List<ArrayMap>(AgentClass.Mini_batchsize);
            if (waveMaps == null)
                waveMaps = new List<WaveMap>();
            while (waveMaps.Count < AgentClass.Mini_batchsize)
            {
                //for (int XP = index; XP < AgentClass.Mini_batchsize; XP++)
                //{
                //    int i = XP % fileInfos.Count;
                //    FileInfo file = fileInfos[i];
                //    GetShiftNumber(file.Name, out float shiftNumber, out float deviation, out float threshold);
                //    WaveMap wave = GetWaveMaps(file.FullName, shiftNumber, deviation, threshold);
                //    waveMaps.Add(wave);
                //}
                Parallel.For(index, index + AgentClass.Mini_batchsize, (XP) =>
                {
                    int i = XP % fileInfos.Count;
                    FileInfo file = fileInfos[i];
                    if (File.Exists(file.FullName))
                    {
                        GetShiftNumber(file.Name, out float shiftNumber, out float deviation, out float threshold);
                        WaveMap wave = GetWaveMaps(file.FullName, shiftNumber, deviation, threshold);
                        waveMaps.Add(wave);
                    }   
                });
            }
            //打乱队列顺序
            //waveMaps = RandomSortList(waveMaps);
            for (int i = 0; i < AgentClass.Mini_batchsize; i++)
            {
                if(i>=waveMaps.Count)
                {
                    Console.WriteLine("---WaveManager---" + waveMaps.Count+"------+ index:"+index+" fileInfos: "+ fileInfos.Count);
                }
                ArrayMap arrayMap = new ArrayMap
                {
                    map = new List<float[,]>(2)
                };
                arrayMap.map.Add(waveMaps[i].LeftSpectrogram);
                arrayMap.map.Add(waveMaps[i].RightSpectrogram);

                float[] result = new float[length];
                result[0] = enlargementTimes * (waveMaps[i].ShiftNumber + 2) / 4;
                result[1] = enlargementTimes * (waveMaps[i].Deviation + 1) / 2;
                result[2] = enlargementTimes * (waveMaps[i].Threshold) / 100;
                arrayMaps.Add(arrayMap);
                results.Add(result);
            }
            waveMaps.RemoveRange(0, AgentClass.Mini_batchsize);




            //int height = waveMaps[0].LeftSpectrogram.GetLength(0);
            //int weight = waveMaps[0].LeftSpectrogram.GetLength(1);

            //for (int i = 0; i < AgentClass.Mini_batchsize; i++)
            //{
            //    float[] result = new float[length];
            //    result[0] = (waveMaps[index].ShiftNumber + 2) / 4;
            //    result[1] = (waveMaps[index].Deviation + 1) / 2;
            //    result[2] = (waveMaps[index].Threshold) / 100;
            //    results.Add(result);
            //    ArrayMap arrayMap = new ArrayMap(2,weight, height);
            //    index = (index + i) % waveMaps.Count;
            //    for(int n=0;n<height;n++)
            //    {
            //        for(int m=0;m<weight;m++)
            //        {
            //            arrayMap.map[0][n, m] = waveMaps[index].LeftSpectrogram[n, m];
            //            arrayMap.map[1][n, m] = waveMaps[index].RightSpectrogram[n, m];
            //        }
            //    }
            //    arrayMaps.Add(arrayMap);
            //}
            return arrayMaps;
        }
        public void DoListRandom()
        {
            fileInfos = RandomSortList(fileInfos);
        }
        private List<T> RandomSortList<T>(List<T> ListT)
        {
            Random random = new Random();
            List<T> newList = new List<T>();
            foreach (T item in ListT)
            {
                newList.Insert(random.Next(newList.Count + 1), item);
            }
            return newList;
        }
        public List<ArrayMap> GetTest(string fileName, int length = 3)
        {
            List<ArrayMap> arrayMaps = new List<ArrayMap>(AgentClass.Mini_batchsize);
            if (waveMaps == null)
                waveMaps = new List<WaveMap>();
            WaveMap wave = GetWaveMaps(fileName);
            ArrayMap arrayMap = new ArrayMap
            {
                map = new List<float[,]>(2)
            };
            arrayMap.map.Add(wave.LeftSpectrogram);
            arrayMap.map.Add(wave.RightSpectrogram);
            arrayMaps.Add(arrayMap);
           
            return arrayMaps;
        }
        public string FileName { get => fileName; set => fileName = value; }
    }
}
