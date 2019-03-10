using ConsoleMorphVOXPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMorphVOXPro.WaveDeal
{
    /// <summary>
    /// 端点检测
    /// </summary>
    [Serializable]
    class EndpointDetection
    {
        private int N = 128;//帧的长度
        private int number = 10;//默认取前100帧作为E0
        /// <summary>
        /// 自动去掉音频中。。。。，这里的窗函数w(n)自动设为1，能量En
        /// 参考资料：https://wenku.baidu.com/view/5c76a2d9ce2f0066f5332270.html
        /// 这里为了计算省时间，把平方运算换成了取绝对值
        /// </summary>
        /// <param name="wAVReader"></param>
        /// <returns></returns>
        public WAVReader EndpointDetectionDeal(WAVReader wAVReader)
        {
            double E0 = 0;
            
            for(int i=0;i<number&&(i+1)*N< wAVReader.stereos.Count; i++)
            {
                E0 += GetEnForOneFrame(wAVReader, i * N, (i + 1) * N);
            }
            E0 /= number;
            for(int i=0;i+ N < wAVReader.stereos.Count;i+=N)
            {
                if(GetEnForOneFrame(wAVReader,i,i+N)<=E0)
                {
                    wAVReader.stereos.RemoveRange(i, N);
                    i -= N;
                }
            }
            return wAVReader;
        }
        /// <summary>
        /// start开始，不到end
        /// </summary>
        /// <param name="wAVReader"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private double GetEnForOneFrame(WAVReader wAVReader,int start,int end)
        {
            double En = 0;
            for(int i=start;i<end;i++)
            {
                En += Math.Abs(wAVReader.stereos[i].Left) + Math.Abs(wAVReader.stereos[i].Right);
            }
            return En;
        }
    }
}
