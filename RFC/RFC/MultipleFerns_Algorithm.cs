using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Drawing.Imaging;

namespace RFC
{
    class MultipleFerns_Algorithm
    {
        public double[,] sampleG { get; set; }
        public double[,] sampleG1 { get; set; }

        public double[,] initialMu; //初始平均值
        public double[,] initialVar; //初始標準差
        
        public int[] G_cluster; //群別歸屬
        public int[][] allGCluster; //記錄每一個樣本在每一個ferns被分群的群別
       
        public double[] pck; //屬於各群別初始機率

        public double[] pckNumber; //屬於各群別數目
        public double[] pxg; //P(Xg)
        public double[, ,] pxgi_ck; //P(Xgi|Ck)
        public double[,] pxg_ck;  //P(Xg|Ck)
        public double[,] pck_xg; //P(Ck|Xg)

        public double[] pck_Mstep;
        public double[,] mu_Mstep;
        public double[,] var_Mstep;

        public double[] mu_Estep;
        public double[] var_Estep;
        public double[] pck_Estep;
        public double[,] mu_percision;

        //-------------------------------------------與fern有關--------------------------//
        public int fernsNumber;

        public double[,] pck_xgtotal;
        public double[,] fernij;
        public double[,] fernpcknumber;
        public double[] newFern;
        public double[] newNumber;
        public double[,] ij;
        public double[,] a_b;

        public double[, ,] feature_Fmeans; //09/30
        public double[,] feature_Fvar; //09/30

        public double[, ,] totalpck_xg;
        public double[,] totalmu_;
        public double[,] totalstd_;

        public double[] mmm; //紀錄組間組內權重
        int num; //ferns次數遞減用

        int[] arrNumber;
        int[] cluster_;
        double[,] s;
        TextBox textBox1 = new TextBox();

        int[,] fea;
        double[,] aa;
        double[,] bb;
        
        int[,] r;
         
        double[][,] allMean; //記錄每一個ferns的各特徵值的平均值


        public void Calculation_FernsImage(string imageName, double[,] sampleG, int featureNumber, int fernsNumber, int numClusters, int sizeFeatures, int maxIterations, double percision, out Bitmap BinaryNumber2, out int Iterations, out double[][,] allMean, out double[][,] allinitialMu, out int[][] allGCluster, out int[,] fea, out int[,] r, out string result, out int[] cluster_, out double[,] s, out double[] mmm)
        {
            Create_Size(sampleG, featureNumber, numClusters, maxIterations, fernsNumber);
            ferns_EMstep(sampleG, featureNumber, fernsNumber, sizeFeatures, numClusters, maxIterations, percision,out Iterations,out mmm, out allMean,out allinitialMu , out  fea, out r, out allGCluster, out result, out cluster_, out s);
            Image(sampleG, imageName, numClusters, aa, out BinaryNumber2);
        }

          public void Create_Size(double[,] sampleG, int featureNumber, int numClusters, int maxIterations,int fernsNumber)
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

            //---------------與ferns有關
            totalpck_xg = new double[fernsNumber, sampleG.GetLength(0), numClusters]; //ferns,sample數,群數
            feature_Fmeans = new double[fernsNumber, numClusters, featureNumber];  // 存取ferns之間的feature比較
            feature_Fvar = new double[fernsNumber, featureNumber];  // 存取ferns之間的feature比較
            fernij = new double[fernsNumber, numClusters]; // 用於比對fern與fern之間
            

            fernpcknumber = new double[fernsNumber, numClusters];
            newFern = new double[numClusters];
            newNumber = new double[numClusters];
            ij = new double[fernsNumber, numClusters];
            a_b = new double[numClusters, numClusters];
            totalpck_xg = new double[fernsNumber, sampleG.GetLength(0), numClusters]; //ferns,sample數,群數
            //---------------與ferns有關
          }
          public void ferns_EMstep(double[,] sampleG, int featureNumber, int fernsNumber, int sizeFeatures, int numClusters, int maxIterations, double percision, out int Iterations, out double[] mmm,out double[][,] allMean , out double[][,] allinitialMu , out int[,] fea, out int[,] r1, out int[][] allGCluster, out string result, out int[] cluster_, out double[,] s)
          {
              result = "";
              cluster_ = new int[sampleG.GetLength(0)];
              s = new double[sampleG.GetLength(0), sampleG.GetLength(1)];
              Iterations = 0;
              double limit = 0;

              sampleG1 = new double[sampleG.GetLength(0), sizeFeatures];
              num = fernsNumber;

              int[] randomArray = new int[sizeFeatures];
              r1 = new int[fernsNumber, numClusters];
              allGCluster = new int[fernsNumber][];
              fea = new int[fernsNumber, sizeFeatures];
              allMean = new double[fernsNumber][,];
              allinitialMu = new double[fernsNumber][,];
              mmm = new double[fernsNumber];

              int[,] asd = { { 0, 1 }, { 0, 2 }, { 1, 2 } };

              do
              {
                  //Random rnd = new Random();
                  //for (int i = 0; i < sizeFeatures; i++)
                  //{
                  //    randomArray[i] = rnd.Next(0, featureNumber - 1);

                  //    for (int j = 0; j < i; j++)
                  //    {
                  //        //檢查是否發生重複，如果有就重新產生
                  //        while (randomArray[j] == randomArray[i])
                  //        {
                  //            j = 0;  //如有重複，將變數j設為0，再次檢查(因為還是有重複的可能)
                  //            randomArray[i] = rnd.Next(0, featureNumber -1);   //重新產生，存回陣列
                  //        }
                  //    }
                  //}

                  //for (int i = 0; i < sampleG.GetLength(0); i++)
                  //{
                  //    for (int N1 = 0; N1 < sizeFeatures; N1++)
                  //    {
                  //        sampleG1[i, N1] = sampleG[i, randomArray[N1]];
                  //        fea[fernsNumber - num, N1] = randomArray[N1];
                  //    }
                  //}

                  for (int i = 0; i < sampleG.GetLength(0); i++)
                  {
                      for (int N1 = 0; N1 < sizeFeatures; N1++)
                      {
                          sampleG1[i, N1] = sampleG[i, asd[fernsNumber - num, N1]];
                      }
                  }
                  fea = asd;

                  Newmean(sampleG1, numClusters, sizeFeatures, out initialMu);
                  //initialMean(sampleG1, numClusters, sizeFeatures, out initialMu); //初始值mean
                  initialStd(sampleG1, initialMu, numClusters, sizeFeatures, out initialVar); //初始值std

                  allinitialMu[fernsNumber - num] = initialMu;

                  Pxgi_ck(sampleG1, initialMu, initialVar, numClusters, sizeFeatures, out pxgi_ck); //初始值Pxgi_ck
                  InitialPck(numClusters, out pck); //初始值Pck
                  PxgCk(sampleG1.GetLength(0), numClusters, sizeFeatures, pxgi_ck, out pxg_ck);//Pxg_ck
                  P_Xg(sampleG1.GetLength(0), numClusters, pck, pxg_ck, out pxg);//計算Pxg
                  P_CkXg(pxg_ck, pck, pxg, numClusters, out pck_xg); //計算Pck_xg
                  WhichCluster(pck_xg, numClusters, out G_cluster, out pckNumber);  //哪個cluster
                  M_step(pck_xg, sampleG1, numClusters, sizeFeatures, G_cluster, out pck_Mstep, out mu_Mstep, out var_Mstep); //m_step

                  mu_percision = mu_Mstep;

                  do
                  {
                      if (maxIterations > 0)
                      {
                          PxgCk(sampleG1.GetLength(0), numClusters, sizeFeatures, pxgi_ck, out pxg_ck);//Pxg_ck
                          P_Xg(sampleG1.GetLength(0), numClusters, pck, pxg_ck, out pxg);//計算Pxg
                          P_CkXg(pxg_ck, pck_Mstep, pxg, numClusters, out pck_xg); //計算Pck_xg
                          WhichCluster(pck_xg, numClusters, out G_cluster, out pckNumber);  //哪個cluster

                          M_step(pck_xg, sampleG1, numClusters, sizeFeatures, G_cluster, out pck_Mstep, out mu_Mstep, out var_Mstep);
                          Percision(mu_Mstep, mu_percision, out limit);

                          Iterations++;
                          maxIterations--;
                      }
                      else
                      {
                          break;
                      }
                  } while (limit > percision);

                  double[,] a = new double[numClusters, featureNumber];
                  double[,] b = new double[numClusters, featureNumber];
                  FernsMean(sampleG, numClusters, featureNumber, pckNumber, G_cluster, out a, out b); //計算fern中各個cluster的mean


                  pckxgTotal(sampleG1, numClusters, sizeFeatures, fernsNumber - num, pck_xg, pckNumber, G_cluster); //將fern的資料存起來
                  mmm[fernsNumber - num] = mi(sampleG1, numClusters, sizeFeatures, G_cluster, pckNumber);
                  allGCluster[fernsNumber - num] = G_cluster;
                  allMean[fernsNumber - num] = a;

                  if (mmm[fernsNumber - num] != 0)
                  {
                      num--;
                  }

              } while (num > 0);
              //Pxgi_ck(sampleG, mu_Mstep, var_Mstep, numClusters, featureNumber, out pxgi_ck);
              //PxgCk(sampleG.GetLength(0), numClusters, featureNumber, pxgi_ck, out pxg_ck);//Pxg_ck
              //P_Xg(sampleG.GetLength(0), numClusters, pck, pxg_ck, out pxg);//計算Pxg
              //P_CkXg(pxg_ck, pck_Mstep, pxg, numClusters, out pck_xg); //計算Pck_xg
              //WhichCluster(pck_xg, numClusters, out G_cluster, out pckNumber);  //哪個cluster

              FernIJ(feature_Fmeans);
              Compared_Ferns(fernij, fernsNumber, numClusters, fernpcknumber, out _r1); //0924新增!!!!!!!!!! 比對ferns!!!
              Totalpckxg(numClusters, fernsNumber, fernpcknumber, _r1, totalpck_xg, mmm, out pck_xgtotal);
              WhichCluster(pck_xgtotal, numClusters, out G_cluster, out pckNumber);  //哪個cluster

              Finial_Mean(sampleG, numClusters, featureNumber, pckNumber, G_cluster, out aa, out bb);

             
              s = sampleG;
              List<int> lsy = G_cluster.OfType<int>().ToList();
              r1 = _r1;
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
                          if (a[i, featureNumber-1] - mean_a[j, k] == 0)
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
          //--------------------------------------------
          // 使用mu_Mstep(1)-mu_Mstep(0)判斷是否收斂
          //--------------------------------------------
          public void Percision(double[,] mu_Mstep, double[,] mu_percision, out double percision)
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

                      if (pck_Mstep[j] == 0)
                      {
                          m_stepmu[k, j] = 0;
                      }
                      else
                      {
                          m_stepmu[k, j] = m_stepmu[k, j] / pck_Mstep[j];
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
                  if (pckNumber[j] == 0)
                  {
                      pck_Mstep[j] = 0;
                  }
                  else
                  {
                      pck_Mstep[j] = pck_Mstep[j] / pckNumber[j];
                  }
              }
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
          // 計算最後個特徵值各群之平均值
          //--------------------------------------------



          //----------有ferns有關
          private void pckxgTotal(double[,] sampleG, int numClusters, int featureNumber, int i, double[,] pck_xg, double[] pckNumber, int[] G_cluster)
          {

              for (int j = 0; j < sampleG.GetLongLength(0); j++)  //sample size
              {
                  for (int k = 0; k < numClusters; k++)  //cluster size
                  {
                      totalpck_xg[i, j, k] = pck_xg[j, k];
                  }
              }

              for (int k = 0; k < numClusters; k++)  //cluster size
              {
                  for (int j = 0; j < sampleG.GetLength(0); j++)
                  {
                      if (G_cluster[j] == k)
                      {
                          for (int n = 0; n < featureNumber; n++)  //feature size
                          {
                              feature_Fmeans[i, k, n] += sampleG[j, n];
                          }
                      }
                      //break;
                  }
                  for (int n = 0; n < featureNumber; n++)  //feature size
                  {
                      if (pckNumber[k] == 0)
                      {
                          feature_Fmeans[i, k, n] = 0;
                      }
                      else
                      {
                          feature_Fmeans[i, k, n] = feature_Fmeans[i, k, n] / pckNumber[k];
                      }
                  }
                  fernpcknumber[i, k] = pckNumber[k];
              }


          }

          private void total_pck_xg(int ferns, double[,] pck_xg, double[] mu, double[] std) //儲存每一個fern的pck_xg/mu/std
          {
              for (int j = 0; j < pck_xg.GetLongLength(0); j++)  //sample size
              {
                  for (int k = 0; k < pck_xg.GetLongLength(1); k++)  //cluster size
                  {
                      totalpck_xg[ferns, j, k] = pck_xg[j, k];
                      totalmu_[ferns, k] = mu[k];
                      totalstd_[ferns, k] = std[k];
                  }
              }

          }
          public double mi(double[,] sample_G1, int numClusters, int featureNumber, int[] G_cluster, double[] pckNumber)
          {
              double[,] sb = new double[featureNumber, featureNumber];
              double[,] sw = new double[featureNumber, featureNumber];

              double[,] mi = new double[numClusters, featureNumber];
              double[] mm = new double[featureNumber];
              double[, ,] ii = new double[numClusters, featureNumber, featureNumber];
              double[, ,] jj = new double[numClusters, featureNumber, featureNumber];
              double[] a = new double[featureNumber];
              double[] b = new double[featureNumber];

              for (int i = 0; i < G_cluster.Length; i++)
              {
                  for (int j = 0; j < numClusters; j++)
                  {
                      if (G_cluster[i] == j)
                      {
                          for (int k = 0; k < featureNumber; k++)
                          {
                              mi[j, k] += sample_G1[i, k];
                          }
                          break;
                      }
                  }
              }
              for (int j = 0; j < numClusters; j++)
              {
                  for (int k = 0; k < featureNumber; k++)
                  {
                      if (pckNumber[j] == 0)
                      {
                          mi[j, k] = 0;
                      }
                      else
                      {
                          mi[j, k] = mi[j, k] / pckNumber[j];
                      }
                  }
              }
              // 將mi算好了


              for (int j = 0; j < numClusters; j++)
              {
                  for (int k = 0; k < featureNumber; k++)
                  {
                      if (pckNumber[j] == 0)
                      {
                          mi[j, k] = 0;
                      }
                      else
                      {
                          mm[k] += pckNumber[j] * mi[j, k];
                      }
                  }
              }
              for (int k = 0; k < featureNumber; k++)
              {
                  mm[k] = mm[k] / sample_G1.GetLength(0);
              }
              // 將mm算好了


              for (int i = 0; i < G_cluster.Length; i++)
              {
                  for (int j = 0; j < numClusters; j++)
                  {
                      if (G_cluster[i] == j)
                      {
                          for (int k = 0; k < sample_G1.GetLength(1); k++)
                          {
                              a[k] = Math.Pow(sample_G1[i, k] - mi[j, k], 2);
                          }
                          for (int aa = 0; aa < a.Length; aa++)
                          {
                              for (int bb = 0; bb < a.Length; bb++)
                              {
                                  ii[j, aa, bb] += (1.0 / pckNumber[j]) * a[aa] * a[bb];
                              }
                          }
                          break;
                      }

                  }
              }
              for (int j = 0; j < numClusters; j++)
              {
                  for (int aa = 0; aa < a.Length; aa++)
                  {
                      for (int bb = 0; bb < a.Length; bb++)
                      {
                          if (pckNumber[j] == 0)
                          {
                              sw[aa, bb] += 0;
                          }
                          else
                          {
                              sw[aa, bb] += (pckNumber[j] / sample_G1.GetLength(0)) * ii[j, aa, bb];
                          }
                      }
                  }
              }
              //將sw算好

              for (int j = 0; j < numClusters; j++)
              {
                  for (int k = 0; k < featureNumber; k++)
                  {
                      b[k] = Math.Pow(mi[j, k] - mm[k], 2);
                  }
                  for (int aa = 0; aa < b.Length; aa++)
                  {
                      for (int bb = 0; bb < b.Length; bb++)
                      {
                          if (pckNumber[j] == 0)
                          {
                              jj[j, aa, bb] += 0;
                          }
                          else
                          {
                              jj[j, aa, bb] += (1.0 / pckNumber[j]) * b[aa] * b[bb];
                          }
                      }
                  }
              }

              for (int j = 0; j < numClusters; j++)
              {
                  for (int aa = 0; aa < a.Length; aa++)
                  {
                      for (int bb = 0; bb < a.Length; bb++)
                      {
                          if (pckNumber[j] == 0)
                          {
                              sb[aa, bb] += 0;
                          }
                          else
                          {
                              sb[aa, bb] += (pckNumber[j] / sample_G1.GetLength(0)) * jj[j, aa, bb];
                          }
                      }
                  }
              }
              //將sb算好

              double x = matrix_surplus(sw);
              double y = matrix_surplus(sb);

              if (y == 0 || x == 0)
              {
                  return 0;
              }
              else
              {
                  return Math.Abs(y / x);
              }
          }
          public double matrix_surplus(double[,] a)
          {
              int i, j, k, p, r, m, n;
              m = a.GetLength(0);
              n = a.GetLength(1);
              double X, temp = 1, temp1 = 1, s = 0, s1 = 0;

              if (n == 2)
              {
                  for (i = 0; i < m; i++)
                  {
                      for (j = 0; j < n; j++)
                      {
                          if ((i + j) % 2 > 0)
                          {
                              temp1 *= a[i, j];
                          }
                          else
                              temp *= a[i, j];
                      }
                  }
                  X = temp - temp1;

              }

              else
              {

                  for (k = 0; k < n; k++)
                  {
                      for (i = 0, j = k; i < m && j < n; i++, j++)
                          temp *= a[i, j];

                      if (m - i > 0)
                      {
                          for (p = m - 1, r = m - 1; p > 0; p--, r--)
                              temp *= a[r, p - 1];
                      }

                      s += temp;
                      temp = 1;
                  }

                  for (k = n - 1; k >= 0; k--)
                  {
                      for (i = 0, j = k; i < m && j >= 0; i++, j--)
                          temp1 *= a[i, j];
                      if (m - i > 0)
                      {
                          for (p = m - 1, r = i; r < m; p--, r++)
                              temp1 *= a[r, p];
                      }
                      s1 += temp1;
                      temp1 = 1;

                  }
                  X = s - s1;
              }

              return X;
          }
        
        
        public void FernsMean(double[,] sampleG, int numClusters, int featureNumber, double[] pckNumber, int[] G_cluster, out double[,] a, out double[,] b)
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

          public int[,] _r1;
          public List<Point> _r = new List<Point>(); //用於存取 指派問題 - 匈牙利法的結果
          private void Compared_Ferns(double[,] fernij, int fernsNumber, int numClusters, double[,] fernpcknumber, out int[,] _r1) //ferns size, sample size, cluster size
          {
              newFern = new double[numClusters]; //存取newFerns(用來持續修正)
              //第一次比對(尚未開始加入修正的值)
              int a = 0;
              _r1 = new int[fernsNumber, numClusters]; //用來存取ferns之間cluster的對應
              //將fern1的cluster先放好 此動作只需要一次
              for (int i = 0; i < numClusters; i++)
              {
                  _r1[a, i] = i;
              }
              if (fernsNumber == 1)
              { }
              else
              {
                  int b = 1;
                  while (b < fernsNumber)
                  {
                      if (b == 1)
                      {

                          //--------------//這個動作只要一次--------
                          for (int i = 0; i < numClusters; i++)
                          {
                              for (int j = 0; j < numClusters; j++)
                              {
                                  a_b[i, j] = Math.Abs(fernij[a, i] - fernij[b, j]);
                              }
                          }
                          //--------------//
                          //利用指派問題 - 匈牙利法 得到最佳的指派
                          ZMatrix m = new ZMatrix(numClusters, numClusters, a_b);
                          m.Calculation();
                          _r = m._result;
                          for (int i = 0; i < numClusters; i++)    //與fern1比較 將與fern1對應的cluster存入!!!!!!! 
                          {
                              for (int j = 0; j < numClusters; j++)
                              {
                                  if (_r[i].X == j)
                                  {
                                      _r1[b, j] = _r[i].Y;
                                      break;
                                  }
                              }
                          }
                          //-------------------------------------------------------------
                          //開始修正fern1值
                          for (int i = 0; i < numClusters; i++)
                          {
                              newNumber[i] = fernpcknumber[a, _r1[a, i]] + fernpcknumber[b, _r1[b, i]];
                              if (newNumber[i] == 0)
                              {
                                  newFern[i] = 0;
                              }
                              else
                              {
                                  newFern[i] = (fernij[a, _r1[a, i]] * fernpcknumber[a, _r1[a, i]] + fernij[b, _r1[b, i]] * fernpcknumber[b, _r1[b, i]]) / newNumber[i];
                              }
                          }

                      }
                      else
                      {
                          for (int i = 0; i < numClusters; i++) //計算newFern與其他fern之值
                          {
                              for (int j = 0; j < numClusters; j++)
                              {
                                  a_b[i, j] = Math.Abs(newFern[i] - fernij[b, j]);
                              }
                          }
                          ZMatrix m = new ZMatrix(numClusters, numClusters, a_b);
                          m.Calculation();
                          _r = m._result;

                          for (int i = 0; i < numClusters; i++)    //與fern1比較 將與fern1對應的cluster存入!!!!!!! 
                          {
                              for (int j = 0; j < numClusters; j++)
                              {
                                  if (_r[i].X == j)
                                  {
                                      _r1[b, j] = _r[i].Y;
                                      break;
                                  }
                              }
                          }
                          for (int i = 0; i < numClusters; i++)
                          {
                              newNumber[i] = newNumber[i] + fernpcknumber[b, _r1[b, i]];
                              newFern[i] = ((newFern[_r1[a, i]] * newNumber[i]) + (fernij[b, _r1[b, i]]) * fernpcknumber[b, _r1[b, i]]) / newNumber[i];
                          }

                      }
                      b++;
                  }
                  //-----------------------------------------------------------------------------------------
              }

          }
        
          private void FernIJ(double[, ,] feature_Fmeans)
          {
              for (int i = 0; i < feature_Fmeans.GetLength(0); i++)
              {

                  for (int j = 0; j < feature_Fmeans.GetLength(1); j++)
                  {
                      for (int k = 0; k < feature_Fmeans.GetLength(2); k++)
                      {
                          fernij[i, j] += feature_Fmeans[i, j, k];
                      }
                      fernij[i, j] = fernij[i, j] / feature_Fmeans.GetLength(2);
                  }
              }
          }
          private void Totalpckxg(int numClusters, int fernsNumber, double[,] fernpcknumber, int[,] _r1, double[, ,] totalpck_xg, double[] mmm, out double[,] pck_xgtotal)
          {
              pck_xgtotal = new double[pck_xg.GetLength(0), pck_xg.GetLength(1)];
              double[] nu = new double[numClusters];
              double sum = mmm.Sum();

              for (int j = 0; j < totalpck_xg.GetLength(1); j++)
              {
                  for (int k = 0; k < totalpck_xg.GetLength(2); k++)
                  {
                      for (int i = 0; i < fernsNumber; i++)
                      {
                         
                          pck_xgtotal[j, k] += (mmm[i]/sum) * totalpck_xg[i, j, _r1[i, k]];
                          nu[k] += fernpcknumber[i, _r1[i, k]];
                      }

                      if (nu[k] == 0)
                      {
                          pck_xgtotal[j, k] = 0;
                      }
                      else
                      {
                          pck_xgtotal[j, k] = pck_xgtotal[j, k] / nu[k];
                      }
                  }
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
