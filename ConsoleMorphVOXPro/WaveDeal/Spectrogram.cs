using AForge.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMorphVOXPro.WaveDeal
{
    [Serializable]
    class Spectrogram
    {
        int mapSize = 256;
        int frameSize = 128;
        int frameZip = 64;
        /// <summary>
        /// 返回语谱图，语谱图是一个二维数组，大小为frameSize*frameSize
        /// </summary>
        /// <param name="stereos"></param>
        /// <returns></returns>
        public WaveMap GetSpectrogram(List<Stereo> stereos, float shiftNumber=0, float deviation=0, float threshold=0)
        {
            if (stereos == null)
            {
                Console.WriteLine("ConsoleMorphVOXPro.WaveDeal.Spectrogram->GetSpectrogram");
                return null;
            }
            
            int startNumber = 0;
            
            int wavMapLength = (frameSize / 2) * frameSize - frameZip * (frameSize / 2 - 1);
           
            float[,] spectrogramLMap = new float[mapSize, mapSize];
            float[,] spectrogramRMap = new float[mapSize, mapSize];
            int test = 0;
            //最后一帧舍弃
            for (int i=0;i<mapSize&& frameSize + startNumber<= stereos.Count;i++)
            {
                Complex[] complicesL = new Complex[frameSize];
                Complex[] complicesR = new Complex[frameSize];
                for (int j = 0; j < frameSize; j++)
                {
                    double hamm = Hamming(j, frameSize);
                    complicesL[j] = new Complex(stereos[j + startNumber].Left * hamm, 0);
                    complicesR[j] = new Complex(stereos[j + startNumber].Right * hamm, 0);
                }
                double[] spectrumMapL = GetSpectrum(complicesL);
                double[] spectrumMapR = GetSpectrum(complicesR);
                int setPosition = (mapSize - frameSize / 2) / 2;
                for (int j = 0; j < frameSize / 2; j++)
                {
                    spectrogramLMap[i, j+ setPosition] = (float)spectrumMapL[j];
                    spectrogramRMap[i, j+ setPosition] = (float)spectrumMapR[j];
                }
                startNumber = startNumber + frameSize - frameZip;
                test++;
            }
            WaveMap waveMap = new WaveMap
            {
                LeftSpectrogram = spectrogramLMap,
                RightSpectrogram = spectrogramRMap,
                ShiftNumber = shiftNumber,
                Threshold = threshold,
                Deviation = deviation
            };

            return waveMap;

            //---------------//----------------//Test:
            //startNumber+ wavMapLength < stereos.Count//原先的表达式
            //大于一半就能用
            //List<WaveMap> waveMaps = new List<WaveMap>();
            //while (startNumber+ wavMapLength <= stereos.Count)
            //{
            //    float[,] spectrogramLMap = new float[frameSize / 2, frameSize / 2];
            //    float[,] spectrogramRMap = new float[frameSize / 2, frameSize / 2];
            //    for (int i = 0; i < frameSize / 2; i++)
            //    {
            //        Complex[] complicesL = new Complex[frameSize];
            //        Complex[] complicesR = new Complex[frameSize];
            //        double []test1=new double[frameSize];
            //        for (int j = 0; j < frameSize; j++)
            //        {
            //            test1[j] = stereos[j  + startNumber].Left;
            //            complicesL[j] = new Complex(stereos[j  + startNumber].Left * Hamming(j, frameSize), 0);
            //            complicesR[j] = new Complex(stereos[j  + startNumber].Right * Hamming(j, frameSize), 0);
            //        }
            //        double[] spectrumMapL = GetSpectrum(complicesL);
            //        double[] spectrumMapR = GetSpectrum(complicesR);
            //        for (int j = 0; j < frameSize / 2; j++)
            //        {
            //            spectrogramLMap[i, j] = (float)spectrumMapL[j];
            //            spectrogramRMap[i, j] = (float)spectrumMapR[j];
            //        }
            //        startNumber = startNumber + frameSize - frameZip;
            //    }
            //    WaveMap waveMap = new WaveMap
            //    {
            //        LeftSpectrogram = spectrogramLMap,
            //        RightSpectrogram = spectrogramRMap,
            //        ShiftNumber = shiftNumber,
            //        Threshold = threshold,
            //        Deviation = deviation
            //    };
            //    waveMaps.Add(waveMap);
            //}


            //return waveMaps;
        }
        /// <summary>
        /// 得到频谱数值
        /// </summary>
        /// <param name="complices"></param>
        /// <returns></returns>
        private double[] GetSpectrum(Complex [] complices)
        {
            if(complices==null)
            {
                Console.WriteLine("ConsoleMorphVOXPro.WaveDeal.Spectrogram->GetSpectrum");
                return null;
            }
            
            double[] spectrumMap = new double[complices.Length];
            FourierTransform.FFT(complices, FourierTransform.Direction.Backward);
            for(int i=0;i<spectrumMap.Length;i++)
            {
                spectrumMap[i] = complices[i].Magnitude;
            }
            return spectrumMap;
        }
        /// <summary>
        /// 因为index在代码中不会出现(index > N || index < 0)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        private double Hamming(int index, int N)
        {
            return 0.54 - 0.46 * Math.Cos(2 * Math.PI * index / (N - 1));
        }

        /// <summary>
        /// 帧的长度，如果不赋值就是默认值
        /// </summary>
        public int FrameSize { get => frameSize; set => frameSize = value; }
        /// <summary>
        /// 两个帧重叠的部分，如果不赋值就是默认值
        /// </summary>
        public int FrameZip { get => frameZip; set => frameZip = value; }
    }
}
