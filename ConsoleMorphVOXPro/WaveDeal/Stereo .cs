using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMorphVOXPro
{
    [Serializable]
    class Stereo
    {
        public Stereo()
        {

        }
        public Stereo(short left , short right)
        {
            this.Left = left;
            this.Right = right;
        }
        private short left = 0;
        private short right = 0;
        override
        public string ToString()
        {
            return Left + " " + Right;
        }
        /// <summary>
        /// 0声道
        /// </summary>
        public short Left { get => left; set => left = value; }
        /// <summary>
        /// 1声道
        /// </summary>
        public short Right { get => right; set => right = value; }
    }
}
