using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace RFC
{
    class SingleFern_Algorithm
    {
        public double[,] sampleG { get; set; }
        public double[,] sampleG1 { get; set; }

        public double[,] initialMu;
        public double[,] initialVar;
        public double[] pck;

        public double[] pckNumber;
        public double[] pxg;
        public double[, ,] pxgi_ck;
        public double[,] pxg_ck;
        public double[,] pck_xg;

        public double[] pck_Mstep;
        public double[,] mu_Mstep;
        public double[,] var_Mstep;
        public double[] mu_Estep;
        public double[] var_Estep;
        public double[] pck_Estep;
        public double[,] mu_percision;

        double[,] aa;
        double[,] bb;

        

        int[,] fea;
        //-------------------------------------------與fern有關--------------------------//

        public int[] G_cluster;

        public void Calculation_Image(string imageName, double[,] sampleG, int featureNumber, int numClusters, int maxIterations,double percision ,out Bitmap BinaryNumber2,out int Iterations,out string result, out int[] cluster_, out double[,] s)
        {
            Create_Size(sampleG, featureNumber, numClusters);
            EMstep(sampleG, featureNumber, numClusters, maxIterations, percision, out Iterations, out aa ,out result, out cluster_, out s);

            Image(sampleG, imageName, numClusters, aa, out BinaryNumber2);
        }
        public void Create_Size(double[,] sampleG, int featureNumber, int numClusters)
        {

            initialMu = new double[featureNumber, numClusters];
            initialVar = new double[featureNumber, numClusters];
            pxgi_ck = new double[sampleG.GetLength(0), featureNumber, numClusters]; //儲存P(Xgi|Ck)
            pxg_ck = new double[sampleG.GetLength(0), numClusters]; //儲存P(Xg|Ck)
            pck = new double[numClusters]; //儲存P(Ck)
            pckNumber = new double[numClusters];
           
            pck_Mstep = new double[numClusters];
            mu_Mstep = new double[featureNumber, numClusters];
            var_Mstep = new double[featureNumber, numClusters];

            mu_Estep = new double[numClusters];
            var_Estep = new double[numClusters];
            pck_Estep = new double[numClusters];

            mu_percision = new double[featureNumber, numClusters];

            pxg = new double[sampleG.GetLength(0)];
            pck_xg = new double[sampleG.GetLength(0), numClusters];
            G_cluster = new int[sampleG.GetLength(0)];

            aa = new double[numClusters, featureNumber];
            bb = new double[numClusters, featureNumber];

            


        }
        public void EMstep(double[,] sampleG, int featureNumber, int numClusters, int maxIterations,double percision,out int Iterations ,out double[,] aa ,out string result, out int[] cluster_, out double[,] s)
        {
            result = "";
            cluster_ = new int[sampleG.GetLength(0)];
            s = new double[sampleG.GetLength(0), sampleG.GetLength(1)];
            Iterations = 0;

            double limit = 0;
            Newmean(sampleG, numClusters, featureNumber, out initialMu);
            //initialMean(sampleG, numClusters, featureNumber, out initialMu); //初始值mean
            initialStd(sampleG, initialMu, numClusters, featureNumber, out initialVar); //初始值std
            
            Pxgi_ck(sampleG, initialMu, initialVar, numClusters, featureNumber, out pxgi_ck); //初始值Pxgi_ck
            InitialPck(numClusters, out pck); //初始值Pck
            PxgCk(sampleG.GetLength(0), numClusters, featureNumber, pxgi_ck, out pxg_ck);//Pxg_ck
            P_Xg(sampleG.GetLength(0), numClusters, pck, pxg_ck, out pxg);//計算Pxg
            P_CkXg(pxg_ck, pck, pxg, numClusters, out pck_xg); //計算Pck_xg
            WhichCluster(pck_xg, numClusters, out G_cluster, out pckNumber);  //哪個cluster
            M_step(pck_xg, sampleG, numClusters, featureNumber, G_cluster, out pck_Mstep, out mu_Mstep, out var_Mstep); //m_step

            mu_percision = mu_Mstep;

            do
            {
                if (maxIterations > 0)
                {
                    Pxgi_ck(sampleG, mu_Mstep, var_Mstep, numClusters, featureNumber, out pxgi_ck);
                    PxgCk(sampleG.GetLength(0), numClusters, featureNumber, pxgi_ck, out pxg_ck);//Pxg_ck
                    P_Xg(sampleG.GetLength(0), numClusters, pck, pxg_ck, out pxg);//計算Pxg
                    P_CkXg(pxg_ck, pck_Mstep, pxg, numClusters, out pck_xg); //計算Pck_xg
                    WhichCluster(pck_xg, numClusters, out G_cluster, out pckNumber);  //哪個cluster

                    M_step(pck_xg, sampleG, numClusters, featureNumber, G_cluster, out pck_Mstep, out mu_Mstep, out var_Mstep);
                    Percision(mu_Mstep, mu_percision, out limit);
                   
                    Iterations++;
                    maxIterations--;
                }
                else
                {
                    break;
                }
            } while (limit>percision);

            //Pxgi_ck(sampleG, mu_Mstep, var_Mstep, numClusters, featureNumber, out pxgi_ck);
            //PxgCk(sampleG.GetLength(0), numClusters, featureNumber, pxgi_ck, out pxg_ck);//Pxg_ck
            //P_Xg(sampleG.GetLength(0), numClusters, pck, pxg_ck, out pxg);//計算Pxg
            //P_CkXg(pxg_ck, pck_Mstep, pxg, numClusters, out pck_xg); //計算Pck_xg
            //WhichCluster(pck_xg, numClusters, out G_cluster, out pckNumber);  //哪個cluster


            Finial_Mean(sampleG, numClusters, featureNumber, pckNumber, G_cluster, out aa, out bb);

            cluster_ = G_cluster;
            s = sampleG;
            List<int> lsy = G_cluster.OfType<int>().ToList();
        }

        //--------------------------------------------
        // 計算最後個特徵值各群之平均值
        //--------------------------------------------
        public void Finial_Mean(double[,] sampleG, int numClusters, int featureNumber, double[] pckNumber, int[] G_cluster, out double[,] a, out double[,] b)
        {
            a = new double[numClusters, featureNumber];
            b = new double[numClusters, featureNumber];

            for (int j = 0; j < G_cluster.Length; j++)
            {
                for (int i = 0; i < numClusters; i++)
                {
                    for (int k = 0; k < featureNumber; k++)
                    {
                        if (G_cluster[j] == i)
                        {
                            a[i, k] += sampleG[j, k];
                            b[i, k] += sampleG[j, k] * sampleG[j, k];
                        }
                    }
                }
            }
            for (int i = 0; i < numClusters; i++)
            {
                for (int k = 0; k < featureNumber; k++)
                {
                    if (pckNumber[i] == 0)
                    {
                        a[i, k] = 0;
                        b[i, k] = 0;
                    }
                    else
                    {
                        a[i, k] = a[i, k] / pckNumber[i];
                        b[i, k] = b[i, k] / pckNumber[i];
                        b[i, k] = Math.Sqrt(b[i, k] - (a[i, k] * a[i, k]));
                    }
                }
            }
        }
       
        //--------------------------------------------
        // 使用mu_Mstep(1)-mu_Mstep(0)判斷是否收斂
        //--------------------------------------------
        public void Percision(double[,] mu_Mstep, double[,] mu_percision,out double percision)
        {
            percision = 0;
            for (int i = 0; i < mu_Mstep.GetLength(0); i++)
            {
                for (int j = 0; j < mu_Mstep.GetLength(1); j++)
                {
                    percision += mu_Mstep[i, j] - mu_percision[i, j];
                }
                      
            }
        
        
        }

       

        //--------------------------------------------
        // 初始值使用隨機挑選
        //--------------------------------------------
        static void initialMean(double[,] list, int clusterNumber, int featureNumber, out double[,] mu)
        {
            
            mu = new double[featureNumber, clusterNumber];

            Random random = new Random();
            for (int k = 0; k < featureNumber; k++)
            {
                for (int i = 0; i < clusterNumber; i++)
                {
                    int randomNumber = random.Next(256);
                    mu[k, i] = randomNumber;
                }
            }
        }

        //--------------------------------------------
        // 初始值使用各樣本點-總平均 排序後 再取值
        //--------------------------------------------
        static void Newmean(double[,] list, int clusterNumber, int featureNumber, out double[,] mu)
        {
            double[,] a = new double[list.GetLength(0), list.GetLength(1)];
            a = list;
            double[] featuerMean = new double[featureNumber];
            double[,] b = new double[list.GetLength(0), list.GetLength(1)];
            double[] c = new double[list.GetLength(0)];
            mu = new double[featureNumber, clusterNumber];

            for (int i = 0; i < list.GetLength(0); i++)
            {
                for (int j = 0; j < featureNumber; j++)
                {
                    featuerMean[j] += a[i, j];
                }
            }
            for (int j = 0; j < featureNumber; j++)
            {
                featuerMean[j] = featuerMean[j] / list.GetLength(0);
            }
            for (int i = 0; i < list.GetLength(0); i++)
            {
                for (int j = 0; j < featureNumber; j++)
                {
                    b[i, j] = Math.Abs(a[i, j] - featuerMean[j]);
                    c[i] += b[i, j];
                }
            }
            int[] DIndex = new int[list.GetLength(0)];
            for (int i = 0; i < list.GetLength(0); i++)
            {
                DIndex[i] = i;
            }
            Array.Sort(c, DIndex);

            for (int i = 0; i < featureNumber; i++)
            {
                for (int j = 0; j < clusterNumber; j++)
                {
                    mu[i, j] = a[DIndex[list.GetLength(0) * (j + 1) / (clusterNumber * 2)], i];
                }
            }
        }

        static void Pxgi_ck(double[,] a, double[,] mean_a, double[,] std_a, int clusterNumber, int featureNumber, out double[, ,] p)
        {
            p = new double[a.GetLength(0), featureNumber, clusterNumber];

            for (int i = 0; i < p.GetLength(0); i++)
            {
                for (int j = 0; j < featureNumber; j++)
                {
                    for (int k = 0; k < clusterNumber; k++)
                    {
                        if (a[i, featureNumber] - mean_a[j, k] == 0)
                        {
                            p[i, j, k] = 1;
                        }
                        if (std_a[j, k] == 0)
                        {
                            std_a[j, k] = 1;
                        }
                        p[i, j, k] = (1 / (Math.Sqrt(2 * 3.14159) * std_a[j, k])) * Math.Exp(-0.5 * Math.Pow(a[i, j] - mean_a[j, k], 2) / Math.Pow(std_a[j, k], 2));
                    }
                }
            }
        } //初始值Pxgi_ck
        static void initialStd(double[,] a, double[,] mean_a, int clusterNumber, int featureNumber, out double[,] var)
        {
            var = new double[featureNumber, clusterNumber];

            int[] number_stop = new int[clusterNumber];
            int[] number_star = new int[clusterNumber];
            for (int k = 0; k < featureNumber; k++)
            {
                int decile_var = 0;
                for (int j = 0; j < clusterNumber; j++)
                {
                    number_star[j] = a.GetLength(0) * decile_var / clusterNumber;
                    number_stop[j] = a.GetLength(0) * (1 + decile_var) / clusterNumber;
                    decile_var++;
                }
                for (int j = 0; j < clusterNumber; j++)
                {
                    for (int i = number_star[j]; i < number_stop[j]; i++)
                    {
                        var[k, j] += (Math.Pow(Math.Abs(a[i, k] - mean_a[k, j]), 2));
                    }
                    var[k, j] = Math.Sqrt(var[k, j] / (number_stop[j] - number_star[j] + 1));
                    if (var[k, j] == 0)
                    {
                        var[k, j] = 1;
                    }
                }
            }
        }
        static void InitialPck(int clusterNuber, out double[] pck)
        {
            pck = new double[clusterNuber];
            for (int i = 0; i < pck.GetLength(0); i++) //計算P(Ck)
            {
                pck[i] = 1.0 / clusterNuber;
            }
        } //初始值Pck
        static void PxgCk(int Samplelength, int clusterNumber, int featureNumber, double[, ,] p, out double[,] pXgck) //計算Pxg_ck
        {   //i=sample數，j=cluster數，k=特徵數
            //將所有smaple_g屬於cluster=j的值相乘，得到P(Xg|Ck)

            pXgck = new double[Samplelength, clusterNumber];
            for (int i = 0; i < Samplelength; i++) //計算P(Xg|Ck)
            {
                for (int j = 0; j < clusterNumber; j++)
                {
                    pXgck[i, j] = 1;
                    for (int k = 0; k < featureNumber; k++)
                    {
                        pXgck[i, j] = (pXgck[i, j] * p[i, k, j]);
                    }
                }
            }
        }
        static void P_Xg(int Samplelength, int clusterNumber, double[] pck, double[,] pxg_ck, out double[] pxg)//計算Pxg
        {
            pxg = new double[Samplelength];
            for (int i = 0; i < Samplelength; i++) //計算P(Xg)
            {
                for (int j = 0; j < clusterNumber; j++)
                {
                    pxg[i] += pxg_ck[i, j] * pck[j];
                }
            }
        }
        static void P_CkXg(double[,] pxg_ck, double[] pck, double[] pxg, int clusterNumber, out double[,] pck_xg) //計算Pck_xg
        {
            pck_xg = new double[pxg_ck.GetLength(0), clusterNumber];

            for (int i = 0; i < pxg_ck.GetLength(0); i++)
            {
                for (int j = 0; j < clusterNumber; j++)
                {
                    if (pxg_ck[i, j] == 0 || pxg[i] == 0 || pck[j] == 0)
                    {
                        pck_xg[i, j] = 0;
                    }
                    else
                    {
                        pck_xg[i, j] = pxg_ck[i, j] * pck[j] / pxg[i];
                    }
                }
            }
        }
        static void WhichCluster(double[,] pck_xg, int clusterNumber, out int[] xg_cluster, out double[] pckNumber)
        {
            xg_cluster = new int[pck_xg.GetLength(0)];
            pckNumber = new double[clusterNumber];
            for (int i = 0; i < pck_xg.GetLength(0); i++) //比較Pck_xg，判斷which cluster
            {
                double max = -1.0;
                for (int k = 0; k < clusterNumber; k++)
                {
                    if (pck_xg[i, k] > max)
                    {
                        max = pck_xg[i, k];
                        xg_cluster[i] = k;
                    }
                }
                for (int k = 0; k < clusterNumber; k++)
                {
                    if (xg_cluster[i] == k)
                    {
                        pckNumber[k] += 1;
                    }
                }
            }
        }

        static void M_step(double[,] pck_xg, double[,] sampleG, int clusterNumber, int featureNumber, int[] g_cluster, out double[] pck_Mstep, out double[,] m_stepmu, out double[,] m_stepvar)
        {
            pck_Mstep = new double[clusterNumber];
            m_stepmu = new double[featureNumber, clusterNumber];
            m_stepvar = new double[featureNumber, clusterNumber];
            int[] pckNumber = new int[clusterNumber];

            for (int i = 0; i < sampleG.GetLength(0); i++) //M-step P(Ck),尚未除上g
            {
                for (int j = 0; j < clusterNumber; j++)
                {
                    if (g_cluster[i] == j)
                    {
                        pck_Mstep[j] += pck_xg[i, j];
                        pckNumber[j] += 1;
                    }
                }
            }
            for (int i = 0; i < sampleG.GetLength(0); i++) //M-step mu_ik,尚未除上P(Ck|Xg)
            {
                for (int k = 0; k < featureNumber; k++)
                {
                    for (int j = 0; j < clusterNumber; j++)
                    {
                        if (g_cluster[i] == j)
                        {
                            m_stepmu[k, j] += (sampleG[i, k] * pck_xg[i, j]);
                        }
                    }
                }
            }
            for (int k = 0; k < featureNumber; k++) //mu 除上P(Ck|Xg)
            {
                for (int j = 0; j < clusterNumber; j++)
                {
                    m_stepmu[k, j] = m_stepmu[k, j] / pck_Mstep[j];
                    if (pck_Mstep[j] == 0)
                    {
                        m_stepmu[k, j] = 0;
                    }
                }
            }
            for (int i = 0; i < sampleG.GetLength(0); i++) //M-step var_ik,尚未除上P(Ck|Xg)
            {
                for (int k = 0; k < featureNumber; k++)
                {
                    for (int j = 0; j < clusterNumber; j++)
                    {
                        if (g_cluster[i] == j)
                        {
                            m_stepvar[k, j] += (Math.Pow((sampleG[i, k] - m_stepmu[k, j]), 2) * pck_xg[i, j]);
                        }
                    }
                }
            }

            for (int k = 0; k < featureNumber; k++) //var 除上P(Ck|Xg)
            {
                for (int j = 0; j < clusterNumber; j++)
                {
                    m_stepvar[k, j] = m_stepvar[k, j] / pck_Mstep[j];
                    m_stepvar[k, j] = Math.Sqrt(m_stepvar[k, j]);
                    if (pck_Mstep[j] == 0)
                    {
                        m_stepvar[k, j] = 0;
                    }
                    if (k == (featureNumber - 1))
                    {
                        pck_Mstep[j] = pck_Mstep[j] / (pckNumber[j] + 1); //避免除以0
                    }
                }
            }
            for (int j = 0; j < clusterNumber; j++)
            {
                pck_Mstep[j] = pck_Mstep[j] / pckNumber[j];
            }
        }
        
        //--------------------------------------------
        // 顯示圖片
        //--------------------------------------------
        Bitmap myBitmap;
        public void Image(double[,] sampleG, string imageName, int numClusters, double[,] aa, out Bitmap BinaryNumber2)
        {
            myBitmap = new Bitmap(imageName);
            BinaryNumber2 = new Bitmap(myBitmap.Width, myBitmap.Height);
            for (int i = 0; i < pxg_ck.GetLength(0); i++) //比較Pck_xg，判斷which cluster
            {
                for (int j = 0; j < numClusters; j++)
                {
                    if (G_cluster[i] == j)
                    {
                        BinaryNumber2.SetPixel(Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 2]), Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 1]), Color.FromArgb(Convert.ToInt32(aa[j, 0]), Convert.ToInt32(aa[j, 1]), Convert.ToInt32(aa[j, 2])));

                    }
                }
            }
        }


    }
}
