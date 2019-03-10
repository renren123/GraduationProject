using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeNet_5.Con_Network
{
    /// <summary>
    /// 这个类的主要目的是为了记录listConInputMapIndex
    /// </summary>
    [Serializable]
    class ConLayerInputIndex
    {
        /// <summary>
        /// 每层一个ConLayerInputIndex，每一个卷积核一个int[]
        /// </summary>
        public List<int[]> listConInputMapIndex { set; get; }
    }
}
