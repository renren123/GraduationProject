using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeNet_5.Con_Network
{
    [Serializable]
    class ConBN
    {
        public Adam AdamGrama { set; get; }
        public Adam AdamBata { set; get; }
        private float momentum = 0.9F;
        /// <summary>
        /// 这个数值是为了调整全连接层与卷积层学习速率的问题
        /// </summary>
        private float adjustBNNumber = 1;
        public int conBnCount { set; get; }
        /// <summary>
        /// 作为BN的输入，其输入在F之前
        /// </summary>
        public List<float[,]> xin;
        public List<float[,]> x_hat { set; get; }
        public float[,] Ub { set; get; }

        private float[,] running_mean { set; get; }
        private float[,] running_var { set; get; }
        private float[,] UbAverage { set; get; }
        private float[,] SigmabAverage { set; get; }
        /// <summary>
        /// 这个是直接取的平方
        /// </summary>
        public float[,] Sigmab { set; get; }
        public float Gamma { set; get; }
        public float Bata { set; get; }
        //public float Vgamma { set; get; }
        //public float Sgamma { set; get; }
        //public float Vbata { set; get; }
        //public float Sbata { set; get; }
        public ConBN()
        {
            conBnCount = 0;
            Bata = 2;
            Gamma = 5;
            //Vgamma = Sgamma = Vbata = Sbata = 0;
        }
        public List<float[,]> GetDout(List<float[,]> dyi)
        {
            List<float[,]> dx_hat = new List<float[,]>(dyi.Count);
            for (int i = 0; i < dyi.Count; i++)
            {
                dx_hat.Add(new float[dyi[i].GetLength(0), dyi[i].GetLength(1)]);
            }
            float[,] dsigmab = new float[dyi[0].GetLength(0), dyi[0].GetLength(1)];
            float[,] dub= new float[dyi[0].GetLength(0), dyi[0].GetLength(1)];
            float dgamma = 0;
            float dbata = 0;
            List<float[,]> dx = new List<float[,]>(dyi.Count);
            for (int i = 0; i < dyi.Count; i++)
            {
                dx.Add(new float[dyi[i].GetLength(0), dyi[i].GetLength(1)]);
            }
            for (int i = 0; i < dx_hat.Count; i++)
            {
                for (int j = 0; j < dx_hat[i].GetLength(0); j++)
                {
                    for (int k = 0; k < dx_hat[i].GetLength(1); k++)
                    {
                        dx_hat[i][j, k] = dyi[i][j, k] * Gamma;
                    }
                }
            }
            float[,] dx_hat_sum = new float[dyi[0].GetLength(0), dyi[0].GetLength(1)];
            for (int i = 0; i < dx_hat_sum.GetLength(0); i++)
            {
                for (int j = 0; j < dx_hat_sum.GetLength(1); j++)
                {
                    for (int k = 0; k < dx_hat.Count; k++)
                    {
                        dx_hat_sum[i, j] += dx_hat[k][i, j] * (xin[k][i, j] - Ub[i, j]);
                    }
                }
            }
            for (int i = 0; i < dsigmab.GetLength(0); i++)
            {
                for (int j = 0; j < dsigmab.GetLength(1); j++)
                {
                    dsigmab[i, j] =(float) (dx_hat_sum[i, j] * (-0.5) * Math.Pow((Sigmab[i, j] + Adam.E), -1.5));
                }
            }
            float[,] dub_temp_1 = new float[dyi[0].GetLength(0), dyi[0].GetLength(1)];
            for (int i = 0; i < dub_temp_1.GetLength(0); i++)
            {
                for (int j = 0; j < dub_temp_1.GetLength(1); j++)
                {
                    for (int k = 0; k < dx_hat.Count; k++)
                    {
                        dub_temp_1[i, j] += (float)(dx_hat[k][i, j] * ((-1) / Math.Sqrt(Sigmab[i, j] + Adam.E)));
                    }
                }
            }
            float[,] dub_temp_2 = new float[dyi[0].GetLength(0), dyi[0].GetLength(1)];
            for (int i = 0; i < dub_temp_2.GetLength(0); i++)
            {
                for (int j = 0; j < dub_temp_2.GetLength(1); j++)
                {
                    for (int k = 0; k < xin.Count; k++)
                    {
                        dub_temp_2[i, j] += (-2) * (xin[k][i, j] - Ub[i, j]);
                    }
                }
            }
            for (int i = 0; i < dub.GetLength(0); i++)
            {
                for (int j = 0; j < dub.GetLength(1); j++)
                {
                    dub[i, j] = dub_temp_1[i, j] + dsigmab[i, j] * dub_temp_2[i, j] / xin.Count;
                }
            }
            for (int i = 0; i < dx.Count; i++)
            {
                for (int j = 0; j < dx[i].GetLength(0); j++)
                {
                    for (int k = 0; k < dx[i].GetLength(1); k++)
                    {
                        dx[i][j, k] =(float) (dx_hat[i][j, k] / Math.Sqrt(Sigmab[j, k] + Adam.E) + dsigmab[j, k] * 2 * (xin[i][j, k] - Ub[j, k]) / xin.Count + dub[j, k] / xin.Count);
                    }
                }
            }
            for (int i = 0; i < dx_hat.Count; i++)
            {
                for (int j = 0; j < x_hat[i].GetLength(0); j++)
                {
                    for (int k = 0; k < x_hat[i].GetLength(1); k++)
                    {
                        dgamma += dyi[i][j, k] * x_hat[i][j, k];
                        dbata += dyi[i][j, k];
                    }
                }
            }
            //更新
            //Gamma -= AgentClass.Study_rate * dgamma;
            //Bata -= AgentClass.Study_rate * dbata;

            if (AdamBata == null)
                AdamBata = new Adam();
            if (AdamGrama == null)
                AdamGrama = new Adam();
            Gamma -= AdamGrama.GetAdam(dgamma);
            Bata -= AdamBata.GetAdam(dbata);
            //BNAdam(dgamma, dbata);
            return dx;
        }
        //private void BNAdam(float dgamma, float dbata)
        //{
        //    Vgamma = (float)(AgentClass.B1 * Vgamma + (1 - AgentClass.B1) * dgamma);
        //    Vbata = (float)(AgentClass.B1 * Vbata + (1 - AgentClass.B1) * dbata);
        //    Sgamma = (float)(AgentClass.B2 * Sgamma + (1 - AgentClass.B2) * dgamma * dgamma);
        //    Sbata = (float)(AgentClass.B2 * Sbata + (1 - AgentClass.B2) * dbata * dbata);
        //    float Vgamma_correction = (float)(Vgamma / (1 - Adam.B1_pow));
        //    float Vbata_correction = (float)(Vbata / (1 - Adam.B1_pow));
        //    float Sgamma_correction = (float)(Sgamma / (1 - Adam.B2_pow));
        //    float Sbata_correction = (float)(Sbata / (1 - Adam.B2_pow));

        //    Gamma -= (float)(adjustBNNumber * AgentClass.Study_rate * Vgamma_correction / (Math.Sqrt(Sgamma_correction) + Adam.E));
        //    Bata -= (float)(adjustBNNumber * AgentClass.Study_rate * Vbata_correction / (Math.Sqrt(Sbata_correction) + Adam.E));
        //}
        /// <summary>
        /// 返回一个新的
        /// </summary>
        /// <returns></returns>
        public List<float[,]> GetX_out(List<float[,]> xout)
        {
            //输入什么，也输出什么，只是改变了其中的值
            if (xout==null)
            {
                xout = new List<float[,]>(xin.Count);
            }
            else
            {
                xout.Clear();
            }
            for (int i = 0; i < xin.Count; i++)
            {
                xout.Add(new float[xin[i].GetLength(0), xin[i].GetLength(1)]);
            }
            if (ClassPublicValue.TrainOrTest.Equals("train"))
            {
                int xinW = xin[0].GetLength(0);
                int xinH = xin[0].GetLength(1);
                if (x_hat == null)
                {
                    x_hat = new List<float[,]>(xin.Count);
                    for (int i = 0; i < xin.Count; i++)
                    {
                        x_hat.Add(new float[xinW, xinH]);
                    }
                }
                if (Sigmab == null)
                {
                    Sigmab = new float[xinW, xinH];
                }
                if (Ub == null)
                {
                    Ub = new float[xinW, xinH];
                }
                if(running_mean==null)
                    running_mean= new float[xinW, xinH];
                if(running_var==null)
                    running_var= new float[xinW, xinH];
                //用于暂存Math.Sqrt(Sigmab[j, k] + Adam.E)
                float[,] sqrtSigmab = new float[xinW, xinH];
                for (int i = 0; i < xinW; i++)
                {
                    for (int j = 0; j < xinH; j++)
                    {
                        Ub[i, j] = GetAvarage(xin, i, j);
                        running_mean[i, j] = running_mean[i, j] * momentum + Ub[i, j] * (1 - momentum);
                        Sigmab[i, j] = GetVariance(xin, i, j);
                        running_var[i, j] = running_var[i, j] * momentum + (1 - momentum) * Sigmab[i, j];
                        sqrtSigmab[i, j] = (float)Math.Sqrt(Sigmab[i, j] + Adam.E);
                    }
                }
                
                
                //for (int i = 0; i < xinW; i++)
                //{
                //    for (int j = 0; j < xinH; j++)
                //    {
                //        Sigmab[i, j] = GetVariance(xin, i, j);
                //        running_var[i, j] = running_var[i, j] * momentum + (1 - momentum) * Sigmab[i, j];
                //        sqrtSigmab[i,j]= (float)Math.Sqrt(Sigmab[i, j] + Adam.E);
                //    }
                //}

                for (int i = 0; i < x_hat.Count; i++)
                {
                    for (int j = 0; j < xinW; j++)
                    {
                        for (int k = 0; k < xinH; k++)
                        {
                            x_hat[i][j, k] = (xin[i][j, k] - Ub[j, k]) / sqrtSigmab[j, k];
                            xout[i][j, k] = Gamma * x_hat[i][j, k] + Bata;
                        }
                    }
                }
                //if (ClassPublicValue.isReadyToUbAndSigmma==true)
                //{
                //    conBnCount++;
                //    AddToE("sigmma", Sigmab);
                //    AddToE("ub", Ub);
                //}
                return xout;
            }
            else if (ClassPublicValue.TrainOrTest.Equals("test"))
            {
                int xinW = xin[0].GetLength(0);
                int xinH = xin[0].GetLength(1);
                double temp = 0;
                for(int i=0;i<xinW;i++)
                {
                    for(int j=0;j<xinH;j++)
                    {
                        temp = Gamma / Math.Sqrt(running_var[i, j] + Adam.E);
                        xout[0][i, j] = (float)(xin[0][i, j] * temp + (Bata - running_mean[i, j] * temp));
                    }
                }
                return xout;
                //float[,] SigmmaTemp = new float[SigmabAverage.GetLength(0), SigmabAverage.GetLength(1)];
                //int Mcount = AgentClass.Mini_batchSizeTrain/();
                ////float xishu = 1;
                //for (int i = 0; i < SigmabAverage.GetLength(0); i++)
                //{
                //    for (int j = 0; j < SigmabAverage.GetLength(1); j++)
                //    {
                //        SigmmaTemp[i, j] = SigmabAverage[i, j] * Mcount / (Mcount - 1);
                //    }
                //}
                //float x_hatTest = 0;
                //for (int n = 0; n < xin[0].GetLength(0); n++)
                //{
                //    for (int m = 0; m < xin[0].GetLength(1); m++)
                //    {
                //        x_hatTest = (float)((xin[0][n, m] - UbAverage[n, m]) / Math.Sqrt(SigmmaTemp[n, m] + Adam.E));
                //        xout[0][n, m] = Gamma * x_hatTest + Bata;
                //        //xout[0][n, m] = (Gamma / (Math.Sqrt(sigmabTrain[n, m] + Adam.E))) * xin[0][n, m] + (Bata - Gamma * Ub[n, m] / Math.Sqrt(sigmabTrain[n, m] + Adam.E));
                //    }
                //} 
                //return xout;
            }
            return null;
        }
        private float[,] GetArrayAvrage(float[,] array)
        {
            float[,] arrayTemp = new float[array.GetLength(0), array.GetLength(1)];
            for (int n = 0; n < array.GetLength(0); n++)
            {
                for (int m = 0; m < array.GetLength(1); m++)
                {
                    arrayTemp[n, m] = array[n, m];
                }
            }
            int Mcount = Adam.T_step - 1;//样本的数量
            for (int i = 0; i < arrayTemp.GetLength(0); i++)
            {
                for (int j = 0; j < arrayTemp.GetLength(1); j++)
                {
                    arrayTemp[i, j] /= Mcount;
                }
            }
            return arrayTemp;
        }
        private void AddToE(string which,float[,] array)
        {
            switch (which)
            {
                case "sigmma":
                    {
                        if (SigmabAverage == null)
                        {
                            SigmabAverage = new float[array.GetLength(0), array.GetLength(1)];
                        }
                        for (int i = 0; i < array.GetLength(0); i++)
                        {
                            for (int j = 0; j < array.GetLength(1); j++)
                            {
                                SigmabAverage[i, j] = (SigmabAverage[i, j] * (conBnCount - 1) + array[i, j]) / conBnCount;
                            }
                        }
                    }
                    break;
                case "ub":
                    {
                        if (UbAverage == null)
                        {
                            UbAverage = new float[array.GetLength(0), array.GetLength(1)];
                        }
                        for (int i = 0; i < array.GetLength(0); i++)
                        {
                            for (int j = 0; j < array.GetLength(1); j++)
                            {
                                UbAverage[i, j] = (UbAverage[i, j] * (conBnCount - 1) + array[i, j]) / conBnCount;
                            }
                        }
                    }
                    break;
            }
        }
        private float GetVariance(List<float[,]> xin, int x, int y)
        {
            float sum = 0;
            for (int i = 0; i < xin.Count; i++)
            {
                sum += (xin[i][x, y] - Ub[x, y]) * (xin[i][x, y] - Ub[x, y]);
            }
            return sum / xin.Count;
        }
        /// <summary>
        /// 往深处的平均值
        /// </summary>
        /// <returns></returns>
        private float GetAvarage(List<float[,]> xin,int x,int y)
        {
            float sum = 0;
            for (int i = 0; i < xin.Count; i++)
            {
                sum += xin[i][x, y];
            }
            return sum / xin.Count;
        }

    }
}
