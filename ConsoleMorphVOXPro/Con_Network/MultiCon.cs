using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LeNet_5.Con_Network
{
    [Serializable]
    class MultiCon
    {
        public int [] FilterNumberArray { set; get; }
        public int [] FilterSizeArray { set; get; }
        public List<ConLayerInputIndex> conLayerInputIndexs { set; get; }
        public List<ArrayMap> inputMaps { set; get; }
        public List<ArrayMap> poolingMaps { set; get; }
        public List<ArrayMap> poolingSensitives;
        //public int filter_size { set; get; }//正方形
        public int numberOfLayor { set; get; }//层数
        public List<ConLayer> conLayers { set; get; }
        public void update()
        {
            if (poolingSensitives == null)
            {
                Console.WriteLine("Error!->MultiCon->update");
                return;
            }
            conLayers[conLayers.Count - 1].poolingSensitives = poolingSensitives;
            for (int i = conLayers.Count-1; i >=0; i--)
            {
                conLayers[i].update();
                if (i>0)
                {
                    conLayers[i - 1].poolingSensitives = conLayers[i].inputSensitives;
                }
            }
        }
        public void caculate()
        {
            if (inputMaps==null)
            {
                Console.WriteLine("Error->MultiCon->caculate");
                return;
            }
            if (conLayers==null)
            {
                conLayers = new List<ConLayer>(numberOfLayor);
                for (int i = 0; i < numberOfLayor; i++)
                {
                    conLayers.Add(new ConLayer());
                }
                for (int i = 0; i < numberOfLayor; i++)
                {
                    conLayers[i].FilterNumber = FilterNumberArray[i];
                    conLayers[i].FilterSize= FilterSizeArray[i];
                    conLayers[i].listConInputMapIndex = conLayerInputIndexs[i].listConInputMapIndex;
                    conLayers[i].conFilters = new List<ConFilter>(FilterNumberArray[i]);
                    //每一层conLayers的FilterSize都是一样的
                    for (int j = 0; j < FilterNumberArray[i]; j++)
                    {
                        conLayers[i].conFilters.Add(new ConFilter());
                        conLayers[i].conFilters[j].Filter_size = FilterSizeArray[i];
                    }
                }
            }
            conLayers[0].inputMaps = inputMaps;
            for (int i = 0; i < conLayers.Count; i++)
            {
                conLayers[i].execute();
                if (i< conLayers.Count-1)
                {
                    conLayers[i + 1].inputMaps = conLayers[i].poolingMaps;
                } 
            }
            poolingMaps = conLayers[conLayers.Count - 1].poolingMaps;
        }
    }
}
