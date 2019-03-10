using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMorphVOXPro.WaveDeal
{
    [Serializable]
    class WaveMap
    {
        public float[,] LeftSpectrogram { set; get; }
        public float[,] RightSpectrogram { set; get; }
        /// <summary>
        /// 移位
        /// </summary>
        public float ShiftNumber { set; get; }
        /// <summary>
        /// 偏移
        /// </summary>
        public float Deviation { set; get; }
        /// <summary>
        /// 阈值
        /// </summary>
        public float Threshold { set; get; }
    }
}
