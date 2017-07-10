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
using System.Diagnostics;
using System.Data.OleDb;


namespace RFC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public Stopwatch stopWatch;
       
        double[,] sampleG; //用於存取欲分群之樣本
        string result;
        int[] cluster_;
        double[,] s;
        double[,] s1;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();


        //------------------------------------------------------------------------------------------------------
        // numClusters 欲分群之群數
        // maxIterations 疊代次數的最大值
        // featureNumber 樣本之特徵值數
        // fernNumber 欲使用分群器個數
        // sizeFeatures 各分群器中欲使用之特徵值數


        //------------------------------------------------------------------------------------------------------

        private void data_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox_1.Image = null;
            pictureBox_2.Image = null;
            pictureBox_3.Image = null;
            pictureBox_original.Image = null;
            pictureBox_Historgram.Image = null;


            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    int numClusters = (int)nUD_Clusternumber.Value;
                    int featureNumber = (int)nUD_Featurenumber.Value;

                    pictureBox1.Image = new Bitmap(openFileDialog1.FileName);
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;

                    button_Cluster.Enabled = true;
                    button_1_Cluster.Enabled = true;

                    NumbersCommonMembers.Enabled = true;
                    SimilarNumber.Enabled = true;
                    QualityCenter.Enabled = true;

                    //------------------------------------------------------------------
                    //直方圖
                    //------------------------------------------------------------------

                    int[] bw = new int[256];
                    Bitmap b = (Bitmap)Bitmap.FromFile(openFileDialog1.FileName);
                    for (int y = 0; y < b.Height; y++)
                    {
                        for (int x = 0; x < b.Width; x++)
                        {
                            Color color = b.GetPixel(x, y);
                            int avg = (color.R + color.G + color.B) / 3; //RGB同除3就會變成灰階
                            b.SetPixel(x, y, Color.FromArgb(avg, avg, avg));
                            bw[avg] += 1;
                        }
                    }

                    pictureBox2.Image = b;
                    Draw_Histogram(pictureBox2, ref pictureBox_Historgram);


                    DataInput input1 = new DataInput();
                    input1.ImageData_C(openFileDialog1.FileName, numClusters, featureNumber, out sampleG);

                    //------------------------------------------------------------------
                    //直方圖
                    //------------------------------------------------------------------
                }
                catch (NotSupportedException ex)
                {
                    MessageBox.Show("Image format is not supported: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show("Invalid image: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch
                {
                    MessageBox.Show("Failed loading the image", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }
        private void Draw_Histogram(PictureBox pb1, ref PictureBox pb2)
        {
            //using System.Drawing.Imaging; // for ImageFormat       
            Bitmap MyBmp = new Bitmap(pb1.Width, pb1.Height);
            Graphics g = Graphics.FromImage((Image)MyBmp);
            //g.InterpolationMode = InterpolationMode.HighQualityBicubic;  
            g.DrawImage(pb1.Image, 0, 0, pb1.Width, pb1.Height);
            g.Dispose();

            long[] myHistogram = new long[256];

            // Step 1: 先鎖住存放圖片的記憶體  
            BitmapData bmData = MyBmp.LockBits(new Rectangle(0, 0, MyBmp.Width, MyBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;


            // Step 2: 取得像點資料的起始位址  
            System.IntPtr Scan0 = bmData.Scan0;
            // 計算每行的像點所佔據的byte 總數  
            int ByteNumber_Width = MyBmp.Width * 3;
            // 計算每一行後面幾個 Padding bytes  
            int ByteOfSkip = stride - ByteNumber_Width;
            int Height = MyBmp.Height;
            int Width = MyBmp.Width;
            int[, ,] rgbData = new int[Width, Height, 3];
            long max_value = 0;


            // Step 3: 直接利用指標, 把影像資料取出來  
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        long Temp = 0;
                        Temp += p[0];    // B  
                        ++p;
                        Temp += p[0];    // G  
                        ++p;
                        Temp += p[0];    // R  
                        ++p;
                        Temp = (int)Temp / 3;
                        myHistogram[Temp]++;
                        if (myHistogram[Temp] > max_value) max_value = myHistogram[Temp];
                    }
                    p += ByteOfSkip; // 跳過剩下的 Padding bytes  
                }
            }

            // Step 4: 釋放存放圖片的記憶體  
            MyBmp.UnlockBits(bmData);

            //--------------------------------------------------------------  
            Bitmap MyBmp2 = new Bitmap(pb2.Width, pb2.Height);
            Graphics g2 = Graphics.FromImage((Image)MyBmp2);

            pb2.BackColor = Color.Black;
            Pen myPen = new Pen(new SolidBrush(Color.White), 1);
            int x1, y1, x2, y2;


            double ratey = (pb2.Height * 1.0) / (max_value * 1.0);
            double ratex = (pb2.Width * 1.0) / (255 * 1.0);
            int y_height;
            for (int i = 0; i <= pb2.Width; i++)
            {
                x1 = i;
                x2 = x1;
                y1 = pb2.Height;
                y_height = Convert.ToInt32(myHistogram[Convert.ToInt32(i / ratex)] * ratey);
                y2 = pb2.Height - y_height;
                g2.DrawLine(myPen, x1, y1, x2, y2);

            }
            pb2.Image = (Image)MyBmp2;
            pb2.Refresh();
        }

        private void button_1_Cluster_Click(object sender, EventArgs e)
        {
            Bitmap BinaryNumber2;

            sw.Reset();
            sw.Start();
            int iteration = 0;
            int numClusters = (int)nUD_Clusternumber.Value;
            int maxIterations = (int)nUD_Maxiternumber.Value;
            int featureNumber = (int)nUD_Featurenumber.Value;
            int fernsNumber = (int)nUD_FernsSize.Value;
            int percision = (int)nUD_Percision.Value;

            SingleFern_Algorithm fomula_image = new SingleFern_Algorithm();
            fomula_image.Calculation_Image(openFileDialog1.FileName, sampleG, featureNumber, numClusters, (int)nUD_Maxiternumber.Value, (int)nUD_Percision.Value, out BinaryNumber2, out iteration, out result, out  cluster_, out s);

            pictureBox2.Height = pictureBox1.Height;
            pictureBox2.Width = pictureBox1.Width;
            pictureBox2.Image = BinaryNumber2;
            

            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            toolStripStatusLabel3.Text = sw.Elapsed.TotalSeconds.ToString() + "秒";
            toolStripStatusLabel4.Text = Convert.ToString(iteration);
            DialogResult result1 = MessageBox.Show("是否滿意", "Question", MessageBoxButtons.YesNo);
            if (result1 == DialogResult.Yes)
            {
                showResult(result, cluster_, s);
            }
        }
        private void showResult(string result, int[] cluster_, double[,] s)
        {
            textBox1.Text = result;

            string pathFile = @"C:\Users\Ada\Desktop\DATA";
            Excel.Application excelApp;
            Excel._Workbook wBook;
            Excel._Worksheet wSheet;

            // 開啟一個新的應用程式
            excelApp = new Excel.Application();

            // 讓Excel文件可見
            excelApp.Visible = true;

            // 停用警告訊息
            excelApp.DisplayAlerts = false;

            // 加入新的活頁簿
            excelApp.Workbooks.Add(Type.Missing);

            // 引用第一個活頁簿
            wBook = excelApp.Workbooks[1];

            // 設定活頁簿焦點
            wBook.Activate();
            try
            {
                // 引用第一個工作表
                wSheet = (Excel._Worksheet)wBook.Worksheets[1];
                // 命名工作表的名稱
                wSheet.Name = "圖檔資料";
                // 設定第1列資料
                excelApp.Cells[1, 1] = "sample";
                excelApp.Cells[1, 2] = "x座標";
                excelApp.Cells[1, 3] = "y座標";
                excelApp.Cells[1, 4] = "灰階值";
                excelApp.Cells[1, 5] = "群別";

                int a = 2;

                //int number = Convert.ToInt32(textBox2.Text);
                for (int i = 0; i < s.GetLength(0); i++)
                {
                    excelApp.Cells[a, 1] = i;
                    excelApp.Cells[a, 2] = s[i, 1];
                    excelApp.Cells[a, 3] = s[i, 2];
                    excelApp.Cells[a, 4] = s[i, 0];
                    excelApp.Cells[a, 5] = cluster_[i];
                    a++;
                }

                try
                {
                    //另存活頁簿
                    wBook.SaveAs(pathFile, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                    Console.WriteLine("儲存文件於 " + Environment.NewLine + pathFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("儲存檔案出錯，檔案可能正在使用" + Environment.NewLine + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("產生報表時出錯！" + Environment.NewLine + ex.Message);
            }


        }

        
        private void button_Cluster_Click(object sender, EventArgs e)
        {

            Bitmap BinaryNumber2;
            textBox2.Text = "";
            textBox3.Text = "";
            textBox_w.Text = "";

            sw.Reset();
            sw.Start();
            int iteration = 0;
            int numClusters = (int)nUD_Clusternumber.Value;
            int maxIterations = (int)nUD_Maxiternumber.Value;
            int featureNumber = (int)nUD_Featurenumber.Value;
            int fernsNumber = (int)nUD_FernsSize.Value;
            int percision = (int)nUD_Percision.Value;
            int sizeFeatures = (int)nUD_SubsetSize.Value;
            int[,] fea;
            int[,] r;
            int[][] allGCluster; //記錄每一個樣本在每一個ferns被分群的群別
            double[][,] allMean; //記錄每一個ferns的各特徵值的平均值
            double[][,] allinitialMu;
            double[] mmm;

            MultipleFerns_Algorithm fomula_image = new MultipleFerns_Algorithm();
            fomula_image.Calculation_FernsImage(openFileDialog1.FileName, sampleG, featureNumber, fernsNumber, numClusters, sizeFeatures, maxIterations, percision, out BinaryNumber2, out iteration, out allMean,out allinitialMu,out allGCluster, out fea, out r,  out result, out cluster_, out s,out mmm);
            toolStripStatusLabel3.Text = sw.Elapsed.TotalSeconds.ToString() + "秒";
            toolStripStatusLabel4.Text = Convert.ToString(iteration);

            for (int i = 0; i < fea.GetLength(0); i++)
            {
                textBox2.Text += "(";
                for (int j = 0; j < fea.GetLength(1); j++)
                {
                    textBox2.Text += fea[i, j] + ",";
                }
                textBox2.Text += ")";
            }
            for (int i = 0; i < allinitialMu.GetLength(0); i++)
            {
                textBox3.Text += i + "-" + "(";
                for (int j = 0; j < allinitialMu[i].GetLength(0); j++)
                {
                    for (int k = 0; k < allinitialMu[i].GetLength(1); k++)
                    {
                        textBox3.Text += allinitialMu[i][j, k] + ",";
                    }
                }
                textBox3.Text += ")";
            }
            for (int i = 0; i < mmm.GetLength(0); i++)
            {
                double sum = mmm.Sum();
                textBox_w.Text += "(";
                textBox_w.Text += mmm[i] / sum + ",";
                textBox_w.Text += ")";
                textBox_w.Text += "　　　";
            }
            pictureBox2.Height = pictureBox1.Height;
            pictureBox2.Width = pictureBox1.Width;
            pictureBox2.Image = BinaryNumber2;


            pictureBox_original.Image = pictureBox2.Image;
            pictureBox_original.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            

            Bitmap BinaryNumber3;
            Bitmap BinaryNumber4;
            Bitmap BinaryNumber5;

            ImageFern(sampleG, openFileDialog1.FileName , numClusters, fernsNumber, allGCluster, r, out BinaryNumber3, out BinaryNumber4, out BinaryNumber5);
            pictureBox_1.Image = BinaryNumber3;
            pictureBox_2.Image = BinaryNumber4;
            pictureBox_3.Image = BinaryNumber5;

            pictureBox_1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox_2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox_3.SizeMode = PictureBoxSizeMode.StretchImage;

            
            
            DialogResult result1 = MessageBox.Show("是否滿意", "Question", MessageBoxButtons.YesNo);
            if (result1 == DialogResult.Yes)
            {
                showResult(result, cluster_, s);
            }
            
        }

        private void NumbersCommonMembers_Click(object sender, EventArgs e)
        {
            Bitmap BinaryNumber2;
            textBox2.Text = "";
            textBox3.Text = "";
            textBox_w.Text = "";

            sw.Reset();
            sw.Start();
            int iteration = 0;
            int numClusters = (int)nUD_Clusternumber.Value;
            int maxIterations = (int)nUD_Maxiternumber.Value;
            int featureNumber = (int)nUD_Featurenumber.Value;
            int fernsNumber = (int)nUD_FernsSize.Value;
            int percision = (int)nUD_Percision.Value;
            int sizeFeatures = (int)nUD_SubsetSize.Value;
            int[,] fea;
            int[,] r;
            int[][] allGCluster; //記錄每一個樣本在每一個ferns被分群的群別
            double[][,] allMean; //記錄每一個ferns的各特徵值的平均值
            double[][,] allinitialMu;

            double[] mmm;

            MultipleFerns_Algorithm_CommonM fomula_image = new MultipleFerns_Algorithm_CommonM();
            fomula_image.Calculation_FernsImage_Common(openFileDialog1.FileName, sampleG, featureNumber, fernsNumber, numClusters, sizeFeatures, maxIterations, percision, out BinaryNumber2, out iteration, out allMean, out allinitialMu, out allGCluster, out fea, out r, out result, out cluster_, out s, out mmm);
            toolStripStatusLabel3.Text = sw.Elapsed.TotalSeconds.ToString() + "秒";
            toolStripStatusLabel4.Text = Convert.ToString(iteration);

            for (int i = 0; i < fea.GetLength(0); i++)
            {
                textBox2.Text += "(";
                for (int j = 0; j < fea.GetLength(1); j++)
                {
                    textBox2.Text += fea[i, j] + ",";
                }
                textBox2.Text += ")";
            }
            for (int i = 0; i < allinitialMu.GetLength(0); i++)
            {
                textBox3.Text += i + "-" + "(";
                for (int j = 0; j < allinitialMu[i].GetLength(0); j++)
                {
                    for (int k = 0; k < allinitialMu[i].GetLength(1); k++)
                    {
                        textBox3.Text += allinitialMu[i][j, k] + ",";
                    }
                }
                textBox3.Text += ")";
            }
            for (int i = 0; i < mmm.GetLength(0); i++)
            {
                double sum = mmm.Sum();
                textBox_w.Text += "(";
                textBox_w.Text += mmm[i] / sum + ",";
                textBox_w.Text += ")";
                textBox_w.Text += "　　　";
            }

            pictureBox2.Height = pictureBox1.Height;
            pictureBox2.Width = pictureBox1.Width;
            pictureBox2.Image = BinaryNumber2;


            pictureBox_original.Image = pictureBox2.Image;
            pictureBox_original.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;


            Bitmap BinaryNumber3;
            Bitmap BinaryNumber4;
            Bitmap BinaryNumber5;

            ImageFern(sampleG, openFileDialog1.FileName, numClusters, fernsNumber, allGCluster, r, out BinaryNumber3, out BinaryNumber4, out BinaryNumber5);
            pictureBox_1.Image = BinaryNumber3;
            pictureBox_2.Image = BinaryNumber4;
            pictureBox_3.Image = BinaryNumber5;

            pictureBox_1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox_2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox_3.SizeMode = PictureBoxSizeMode.StretchImage;



            DialogResult result1 = MessageBox.Show("是否滿意", "Question", MessageBoxButtons.YesNo);
            if (result1 == DialogResult.Yes)
            {
                showResult(result, cluster_, s);
            }
        }

        private void SimilarNumber_Click(object sender, EventArgs e)
        {
            Bitmap BinaryNumber2;
            textBox2.Text = "";
            textBox3.Text = "";
            textBox_w.Text = "";

            sw.Reset();
            sw.Start();
            int iteration = 0;
            int numClusters = (int)nUD_Clusternumber.Value;
            int maxIterations = (int)nUD_Maxiternumber.Value;
            int featureNumber = (int)nUD_Featurenumber.Value;
            int fernsNumber = (int)nUD_FernsSize.Value;
            int percision = (int)nUD_Percision.Value;
            int sizeFeatures = (int)nUD_SubsetSize.Value;
            int[,] fea;
            int[,] r;
            int[][] allGCluster; //記錄每一個樣本在每一個ferns被分群的群別
            double[][,] allMean; //記錄每一個ferns的各特徵值的平均值
            double[] mmm;
            double[][,] allinitialMu;

            MultipleFerns_Algorithm_NumberM fomula_image = new MultipleFerns_Algorithm_NumberM();
            fomula_image.Calculation_FernsImage_Number(openFileDialog1.FileName, sampleG, featureNumber, fernsNumber, numClusters, sizeFeatures, maxIterations, percision, out BinaryNumber2, out iteration, out allMean,out allinitialMu, out allGCluster, out fea, out r, out result, out cluster_, out s, out mmm);


            toolStripStatusLabel3.Text = sw.Elapsed.TotalSeconds.ToString() + "秒";
            toolStripStatusLabel4.Text = Convert.ToString(iteration);

            for (int i = 0; i < fea.GetLength(0); i++)
            {
                textBox2.Text += "(";
                for (int j = 0; j < fea.GetLength(1); j++)
                {
                    textBox2.Text += fea[i, j] + ",";
                }
                textBox2.Text += ")";
            }
            for (int i = 0; i < allinitialMu.GetLength(0); i++)
            {
                textBox3.Text += i + "-" + "(";
                for (int j = 0; j < allinitialMu[i].GetLength(0); j++)
                {
                    for (int k = 0; k < allinitialMu[i].GetLength(1); k++)
                    {
                        textBox3.Text += allinitialMu[i][j, k] + ",";
                    }
                }
                textBox3.Text += ")";
            }
            for (int i = 0; i < mmm.GetLength(0); i++)
            {
                double sum = mmm.Sum();
                textBox_w.Text += "(";
                textBox_w.Text += mmm[i]/sum + ",";
                textBox_w.Text += ")";
                textBox_w.Text += "　　　";
            }

            pictureBox2.Height = pictureBox1.Height;
            pictureBox2.Width = pictureBox1.Width;
            pictureBox2.Image = BinaryNumber2;


            pictureBox_original.Image = pictureBox2.Image;
            pictureBox_original.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;


            Bitmap BinaryNumber3;
            Bitmap BinaryNumber4;
            Bitmap BinaryNumber5;

            ImageFern(sampleG, openFileDialog1.FileName, numClusters, fernsNumber, allGCluster, r, out BinaryNumber3, out BinaryNumber4, out BinaryNumber5);
            pictureBox_1.Image = BinaryNumber3;
            pictureBox_2.Image = BinaryNumber4;
            pictureBox_3.Image = BinaryNumber5;

            pictureBox_1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox_2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox_3.SizeMode = PictureBoxSizeMode.StretchImage;



            DialogResult result1 = MessageBox.Show("是否滿意", "Question", MessageBoxButtons.YesNo);
            if (result1 == DialogResult.Yes)
            {
                showResult(result, cluster_, s);
            }
        }

        private void QualityCenter_Click(object sender, EventArgs e)
        {
            Bitmap BinaryNumber2;
            textBox2.Text = "";
            textBox3.Text = "";
            textBox_w.Text = "";

            sw.Reset();
            sw.Start();
            int iteration = 0;
            int numClusters = (int)nUD_Clusternumber.Value;
            int maxIterations = (int)nUD_Maxiternumber.Value;
            int featureNumber = (int)nUD_Featurenumber.Value;
            int fernsNumber = (int)nUD_FernsSize.Value;
            int percision = (int)nUD_Percision.Value;
            int sizeFeatures = (int)nUD_SubsetSize.Value;
            int[,] fea;
            int[,] r;
            int[][] allGCluster; //記錄每一個樣本在每一個ferns被分群的群別
            double[][,] allMean; //記錄每一個ferns的各特徵值的平均值
            double[][,] allinitialMu;
            double[] mmm;

            MultipleFerns_Algorithm fomula_image = new MultipleFerns_Algorithm();
            fomula_image.Calculation_FernsImage(openFileDialog1.FileName, sampleG, featureNumber, fernsNumber, numClusters, sizeFeatures, maxIterations, percision, out BinaryNumber2, out iteration, out allMean, out allinitialMu, out allGCluster, out fea, out r, out result, out cluster_, out s, out mmm);
            toolStripStatusLabel3.Text = sw.Elapsed.TotalSeconds.ToString() + "秒";
            toolStripStatusLabel4.Text = Convert.ToString(iteration);

            for (int i = 0; i < fea.GetLength(0); i++)
            {
                textBox2.Text += "(";
                for (int j = 0; j < fea.GetLength(1); j++)
                {
                    textBox2.Text += fea[i, j] + ",";
                }
                textBox2.Text += ")";
            }
            for (int i = 0; i < allinitialMu.GetLength(0); i++)
            {
                textBox3.Text += i + "-" + "(";
                for (int j = 0; j < allinitialMu[i].GetLength(0); j++)
                {
                    for (int k = 0; k < allinitialMu[i].GetLength(1); k++)
                    {
                        textBox3.Text += allinitialMu[i][j, k] + ",";
                    }
                }
                textBox3.Text += ")";
            }
            for (int i = 0; i < mmm.GetLength(0); i++)
            {
                double sum = mmm.Sum();
                textBox_w.Text += "(";
                textBox_w.Text += mmm[i] / sum + ",";
                textBox_w.Text += ")";
                textBox_w.Text += "　　　";
            }
            pictureBox2.Height = pictureBox1.Height;
            pictureBox2.Width = pictureBox1.Width;
            pictureBox2.Image = BinaryNumber2;


            pictureBox_original.Image = pictureBox2.Image;
            pictureBox_original.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;


            Bitmap BinaryNumber3;
            Bitmap BinaryNumber4;
            Bitmap BinaryNumber5;

            ImageFern(sampleG, openFileDialog1.FileName, numClusters, fernsNumber, allGCluster, r, out BinaryNumber3, out BinaryNumber4, out BinaryNumber5);
            pictureBox_1.Image = BinaryNumber3;
            pictureBox_2.Image = BinaryNumber4;
            pictureBox_3.Image = BinaryNumber5;

            pictureBox_1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox_2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox_3.SizeMode = PictureBoxSizeMode.StretchImage;



            DialogResult result1 = MessageBox.Show("是否滿意", "Question", MessageBoxButtons.YesNo);
            if (result1 == DialogResult.Yes)
            {
                showResult(result, cluster_, s);
            }
        }

        
        //----------不同fern之顯示----------//
        Bitmap myBitmap1;
        public void ImageFern(double[,] sampleG, string imageName, int numClusters, int fernsNumber, int[][] allGCluster , int[,] r, out Bitmap BinaryNumber3, out Bitmap BinaryNumber4, out Bitmap BinaryNumber5)
        {
            myBitmap1 = new Bitmap(imageName);
            
            BinaryNumber5 = new Bitmap(myBitmap1.Width, myBitmap1.Height);
            BinaryNumber3 = new Bitmap(myBitmap1.Width, myBitmap1.Height);
            BinaryNumber4 = new Bitmap(myBitmap1.Width, myBitmap1.Height);
            int[][] colorarray = new int[numClusters][];
            for (int i = 0; i < numClusters; i++)
            {
                colorarray[i] = GetRandomColor();
            }
            
            for (int i = 0; i < allGCluster.GetLength(0); i++) 
            {
                for (int j = 0; j < allGCluster[i].Length; j++)
                {
                    for (int k = 0; k < numClusters; k++)
                    {
                        if (i==0 && allGCluster[i][j] == r[i, k])
                        {
                            BinaryNumber3.SetPixel(Convert.ToInt32(sampleG[j, sampleG.GetLength(1) - 2]), Convert.ToInt32(sampleG[j, sampleG.GetLength(1) - 1]), Color.FromArgb(colorarray[k][0], colorarray[k][1], colorarray[k][2]));
                        }
                        if (i == 1 && allGCluster[i][j] == r[i, k])
                        {
                            BinaryNumber4.SetPixel(Convert.ToInt32(sampleG[j, sampleG.GetLength(1) - 2]), Convert.ToInt32(sampleG[j, sampleG.GetLength(1) - 1]), Color.FromArgb(colorarray[k][0], colorarray[k][1], colorarray[k][2]));
                        }
                        if (i == 2 && allGCluster[i][j] == r[i, k])
                        {
                            BinaryNumber5.SetPixel(Convert.ToInt32(sampleG[j, sampleG.GetLength(1) - 2]), Convert.ToInt32(sampleG[j, sampleG.GetLength(1) - 1]), Color.FromArgb(colorarray[k][0], colorarray[k][1], colorarray[k][2]));
                        }
                    }
                }
            }
           

           

            //for (int i = 0; i < sampleG.GetLength(0); i++) //比較Pck_xg，判斷which cluster
            //{
            //    for (int j = 0; j < numClusters; j++)
            //    {
            //        if (G_cluster_1[i, 0] == r[0, 0])
            //        {
            //            BinaryNumber3.SetPixel(Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 2]), Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 1]), Color.FromArgb(255, 0, 0));
            //        }
            //        if (G_cluster_1[i, 0] == r[0, 1])
            //        {
            //            BinaryNumber3.SetPixel(Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 2]), Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 1]), Color.FromArgb(0, 0, 255));
            //        }

            //        if (G_cluster_1[i, 1] == r[1, 0])
            //        {
            //            BinaryNumber4.SetPixel(Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 2]), Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 1]), Color.FromArgb(255, 0, 0));
            //            //BinaryNumber4.SetPixel(Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 2]), Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 1]), Color.FromArgb(0, 0, 0));
            //        }
            //        if (G_cluster_1[i, 1] == r[1, 1])
            //        {
            //            BinaryNumber4.SetPixel(Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 2]), Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 1]), Color.FromArgb(0, 0, 255));

            //        }
            //        if (G_cluster_1[i, 0] == r[0, 2])
            //        {
            //            BinaryNumber3.SetPixel(Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 2]), Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 1]), Color.FromArgb(0, 255, 0));
            //        }
            //        if (G_cluster_1[i, 1] == r[1, 2])
            //        {
            //            BinaryNumber4.SetPixel(Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 2]), Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 1]), Color.FromArgb(0, 255, 0));
            //        }
            //        if (G_cluster_1[i, 2] == r[2, 2])
            //        {
            //            BinaryNumber5.SetPixel(Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 2]), Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 1]), Color.FromArgb(0, 255, 0));
            //        }
            //        if (G_cluster_1[i, 2] == r[2, 0])
            //        {
            //            BinaryNumber5.SetPixel(Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 2]), Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 1]), Color.FromArgb(255, 0, 0));
            //            //BinaryNumber4.SetPixel(Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 2]), Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 1]), Color.FromArgb(0, 0, 0));
            //        }
            //        if (G_cluster_1[i, 2] == r[2, 1])
            //        {
            //            BinaryNumber5.SetPixel(Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 2]), Convert.ToInt32(sampleG[i, sampleG.GetLength(1) - 1]), Color.FromArgb(0, 0, 255));

            //        }

            //    }
            //}
        }
        public int[] GetRandomColor()
        {
            Random RandomNum_First = new Random((int)DateTime.Now.Ticks);
            //對於C#的隨機數，沒什麼好說的
            System.Threading.Thread.Sleep(RandomNum_First.Next(50));
            Random RandomNum_Sencond = new Random((int)DateTime.Now.Ticks);         //為了在白色背景上顯示，盡量生成深色         
            int int_Red = RandomNum_First.Next(256);
            int int_Green = RandomNum_Sencond.Next(256);
            int int_Blue = (int_Red + int_Green > 400) ? 0 : 400 - int_Red - int_Green;
            int_Blue = (int_Blue > 255) ? 255 : int_Blue;

            int[] arrayColor = { int_Red, int_Green, int_Blue };

            return arrayColor;
        }
        //----------不同fern之顯示----------//

        //-------顯示之按鈕---------//
        private void pictureBox_1_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = pictureBox_1.Image;

        }
        private void pictureBox_2_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = pictureBox_2.Image;
        }
        private void pictureBox_3_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = pictureBox_3.Image;
        }
        private void pictureBox_original_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = pictureBox_original.Image;
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            textBox_X.Text = Convert.ToString(e.Y);
            textBox_Y.Text = Convert.ToString(e.X);
        }
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image == null)
            {
            }
            else
            {
                Bitmap b = new Bitmap(pictureBox1.Image);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                Color color = b.GetPixel(e.X * pictureBox1.Image.Width / pictureBox1.Width, e.Y * pictureBox1.Image.Height / pictureBox1.Height);
                textBox_R.Text = Convert.ToString(color.R);
                textBox_G.Text = Convert.ToString(color.G);
                textBox_B.Text = Convert.ToString(color.B);
            }
        }
        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            textBox_X1.Text = Convert.ToString(e.Y);
            textBox_Y1.Text = Convert.ToString(e.X);
        }
        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image == null)
            {
            }
            else
            {
                Bitmap b = new Bitmap(pictureBox2.Image);
                pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                Color color = b.GetPixel(e.X * pictureBox2.Image.Width / pictureBox2.Width, e.Y * pictureBox2.Image.Height / pictureBox2.Height);
                textBox_R1.Text = Convert.ToString(color.R);
                textBox_G1.Text = Convert.ToString(color.G);
                textBox_B1.Text = Convert.ToString(color.B);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

		private void toolStripDropDownButton1_Click(object sender, EventArgs e)
		{

		}


		//--------------------------//



	}
}
