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
    class DataInput
    {
        Image image;
        Bitmap myBitmap;
        Bitmap BinaryNumber;
        public int[,] ImageArray;
        public int[,] color_r;
        public int[,] color_g;
        public int[,] color_b;
        public PictureBox pictureBox1;


        //------------------------------------------------------------------
        //ImageData_C 
        //sampleG 存取彩色影像之特徵值(R,G,B)
        //------------------------------------------------------------------
        public void ImageData_C(string imageName, int numClusters, int featureNumber, out double[,] sampleG)
        {
            pictureBox1 = new PictureBox();

            image = Image.FromFile(imageName);
            pictureBox1.Image = image;
            pictureBox1.Height = image.Height;
            pictureBox1.Width = image.Width;

            myBitmap = new Bitmap(imageName);
            BinaryNumber = new Bitmap(myBitmap.Width, myBitmap.Height);
            ImageArray = new int[myBitmap.Width, myBitmap.Height];
            color_r = new int[myBitmap.Width, myBitmap.Height];
            color_g = new int[myBitmap.Width, myBitmap.Height];
            color_b = new int[myBitmap.Width, myBitmap.Height];

            sampleG = new double[pictureBox1.Image.Height * pictureBox1.Image.Width, featureNumber + 2];
            int[,] feature = new int[sampleG.GetLength(0), featureNumber];

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color A1 = myBitmap.GetPixel(i, j);
                    int AR = A1.R;
                    int AG = A1.G;
                    int AB = A1.B;
                    //sampleG[i * pictureBox1.Height + j, 0] = (AR + AG + AB) / 3;
                    //sampleG[i * pictureBox1.Height + j, 1] = i;
                    //sampleG[i * pictureBox1.Height + j, 2] = j;
                    sampleG[i * pictureBox1.Height + j, 0] = AR;
                    sampleG[i * pictureBox1.Height + j, 1] = AG;
                    sampleG[i * pictureBox1.Height + j, 2] = AB;
                    //sampleG[i * pictureBox1.Height + j, 3] = i;
                    //sampleG[i * pictureBox1.Height + j, 4] = j;
                    sampleG[i * pictureBox1.Height + j, featureNumber] = i;
                    sampleG[i * pictureBox1.Height + j, featureNumber + 1] = j;
                }
            }
        }
        
        
        //------------------------------------------------------------------
        //ImageData_Cxys
        //sampleG 存取彩色影像之特徵值(R,G,B,x,y)
        //------------------------------------------------------------------
        public void ImageData_Cxys(string imageName, int numClusters, int featureNumber, out double[,] sampleG)
        {
            pictureBox1 = new PictureBox();

            image = Image.FromFile(imageName);
            pictureBox1.Image = image;
            pictureBox1.Height = image.Height;
            pictureBox1.Width = image.Width;

            myBitmap = new Bitmap(imageName);
            BinaryNumber = new Bitmap(myBitmap.Width, myBitmap.Height);
            ImageArray = new int[myBitmap.Width, myBitmap.Height];
            color_r = new int[myBitmap.Width, myBitmap.Height];
            color_g = new int[myBitmap.Width, myBitmap.Height];
            color_b = new int[myBitmap.Width, myBitmap.Height];

            sampleG = new double[pictureBox1.Image.Height * pictureBox1.Image.Width, featureNumber + 2];
            int[,] feature = new int[sampleG.GetLength(0), featureNumber];

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color A1 = myBitmap.GetPixel(i, j);
                    int AR = A1.R;
                    int AG = A1.G;
                    int AB = A1.B;
                    
                    sampleG[i * pictureBox1.Height + j, 0] = AR;
                    sampleG[i * pictureBox1.Height + j, 1] = AG;
                    sampleG[i * pictureBox1.Height + j, 2] = AB;
                    sampleG[i * pictureBox1.Height + j, 3] = i;
                    sampleG[i * pictureBox1.Height + j, 4] = j;
                    sampleG[i * pictureBox1.Height + j, featureNumber] = i;
                    sampleG[i * pictureBox1.Height + j, featureNumber + 1] = j;
                }
            }
        }
        
        
        //------------------------------------------------------------------
        //ImageData_CMS 
        //sampleG 存取彩色影像之特徵值(R,G,B,3*3平均值r,g,b,3*3標準差r,g,b)
        //------------------------------------------------------------------
        public void ImageData_CMS(string imageName, int windowsize, int numClusters, int featureNumber, out double[,] sampleG)
        {
            pictureBox1 = new PictureBox();

            image = Image.FromFile(imageName);
            pictureBox1.Image = image;
            pictureBox1.Height = image.Height;
            pictureBox1.Width = image.Width;

            myBitmap = new Bitmap(imageName);
            BinaryNumber = new Bitmap(myBitmap.Width, myBitmap.Height);
            ImageArray = new int[myBitmap.Width, myBitmap.Height];
            color_r = new int[myBitmap.Width, myBitmap.Height];
            color_g = new int[myBitmap.Width, myBitmap.Height];
            color_b = new int[myBitmap.Width, myBitmap.Height];

            sampleG = new double[pictureBox1.Image.Height * pictureBox1.Image.Width, featureNumber + 2];
            int[,] feature = new int[sampleG.GetLength(0), featureNumber];

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color A1 = myBitmap.GetPixel(i, j);
                    int AR = A1.R;
                    int AG = A1.G;
                    int AB = A1.B;
                    sampleG[i * pictureBox1.Height + j, 0] = AR;
                    sampleG[i * pictureBox1.Height + j, 1] = AG;
                    sampleG[i * pictureBox1.Height + j, 2] = AB;
                    //sampleG[i * pictureBox1.Height + j, 3] = i;
                    //sampleG[i * pictureBox1.Height + j, 4] = j;
                    sampleG[i * pictureBox1.Height + j, featureNumber] = i;
                    sampleG[i * pictureBox1.Height + j, featureNumber + 1] = j;
                }
            }


            for (int i = windowsize; i < pictureBox1.Width - windowsize; i++)
            {
                for (int j = windowsize; j < pictureBox1.Height - windowsize; j++)
                {
                    double mean_r = 0;
                    double std_r = 0;
                    double mean_g = 0;
                    double std_g = 0;
                    double mean_b = 0;
                    double std_b = 0;
                    double sum = 0;

                    for (int k = i - windowsize; k <= i + windowsize; k++)
                    {
                        for (int l = j - windowsize; l <= j + windowsize; l++)
                        {
                            mean_r += sampleG[k * pictureBox1.Height + l, 0];
                            mean_g += sampleG[k * pictureBox1.Height + l, 1];
                            mean_b += sampleG[k * pictureBox1.Height + l, 2];
                            std_r += sampleG[k * pictureBox1.Height + l, 0] * sampleG[k * pictureBox1.Height + l, 0];
                            std_g += sampleG[k * pictureBox1.Height + l, 1] * sampleG[k * pictureBox1.Height + l, 1];
                            std_b += sampleG[k * pictureBox1.Height + l, 2] * sampleG[k * pictureBox1.Height + l, 2];
                            sum++;
                        }
                    }
                    mean_r = mean_r / sum;
                    mean_g = mean_g / sum;
                    mean_b = mean_b / sum;
                    std_r = std_r / sum;
                    std_g = std_g / sum;
                    std_b = std_b / sum;

                    std_r = Math.Sqrt(std_r - (mean_r * mean_r));
                    std_g = Math.Sqrt(std_g - (mean_g * mean_g));
                    std_b = Math.Sqrt(std_b - (mean_b * mean_b));

                    sampleG[i * pictureBox1.Height + j, 3] = mean_r;
                    sampleG[i * pictureBox1.Height + j, 4] = mean_g;
                    sampleG[i * pictureBox1.Height + j, 5] = mean_b;
                    sampleG[i * pictureBox1.Height + j, 6] = std_r;
                    sampleG[i * pictureBox1.Height + j, 7] = std_g;
                    sampleG[i * pictureBox1.Height + j, 8] = std_b;

                }
            }



        }
        
        
        //------------------------------------------------------------------
        //ImageData_CMSs
        //sampleG 存取彩色影像之特徵值(R,G,B,3*3平均值r,g,b,3*3標準差r,g,b)皆正規化
        //------------------------------------------------------------------
        public void ImageData_CMSs(string imageName, int windowsize, int numClusters, int featureNumber, out double[,] sampleG) //標準化
        {
            pictureBox1 = new PictureBox();

            image = Image.FromFile(imageName);
            pictureBox1.Image = image;
            pictureBox1.Height = image.Height;
            pictureBox1.Width = image.Width;

            myBitmap = new Bitmap(imageName);
            BinaryNumber = new Bitmap(myBitmap.Width, myBitmap.Height);
            ImageArray = new int[myBitmap.Width, myBitmap.Height];
            color_r = new int[myBitmap.Width, myBitmap.Height];
            color_g = new int[myBitmap.Width, myBitmap.Height];
            color_b = new int[myBitmap.Width, myBitmap.Height];

            sampleG = new double[pictureBox1.Image.Height * pictureBox1.Image.Width, featureNumber + 2];
            int[,] feature = new int[sampleG.GetLength(0), featureNumber];





            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color A1 = myBitmap.GetPixel(i, j);
                    int AR = A1.R;
                    int AG = A1.G;
                    int AB = A1.B;
                    sampleG[i * pictureBox1.Height + j, 0] = Convert.ToDouble(AR / Convert.ToDouble(255));
                    sampleG[i * pictureBox1.Height + j, 1] = Convert.ToDouble(AG / Convert.ToDouble(255));
                    sampleG[i * pictureBox1.Height + j, 2] = Convert.ToDouble(AB / Convert.ToDouble(255));
                    sampleG[i * pictureBox1.Height + j, 9] = Convert.ToDouble(i / Convert.ToDouble(image.Width));
                    sampleG[i * pictureBox1.Height + j, 10] = Convert.ToDouble(j / Convert.ToDouble(image.Height));

                    sampleG[i * pictureBox1.Height + j, featureNumber] = i;
                    sampleG[i * pictureBox1.Height + j, featureNumber + 1] = j;
                }
            }

            double max_meanr = -10;
            double max_stdr = -100;
            double max_meang = -10;
            double max_stdg = -100;
            double max_meanb = -10;
            double max_stdb = -100;


            for (int i = windowsize; i < pictureBox1.Width - windowsize; i++)
            {
                for (int j = windowsize; j < pictureBox1.Height - windowsize; j++)
                {
                    double mean_r = 0;
                    double std_r = 0;
                    double mean_g = 0;
                    double std_g = 0;
                    double mean_b = 0;
                    double std_b = 0;
                    double sum = 0;

                    for (int k = i - windowsize; k <= i + windowsize; k++)
                    {
                        for (int l = j - windowsize; l <= j + windowsize; l++)
                        {
                            mean_r += sampleG[k * pictureBox1.Height + l, 0];
                            mean_g += sampleG[k * pictureBox1.Height + l, 1];
                            mean_b += sampleG[k * pictureBox1.Height + l, 2];
                            std_r += sampleG[k * pictureBox1.Height + l, 0] * sampleG[k * pictureBox1.Height + l, 0];
                            std_g += sampleG[k * pictureBox1.Height + l, 1] * sampleG[k * pictureBox1.Height + l, 1];
                            std_b += sampleG[k * pictureBox1.Height + l, 2] * sampleG[k * pictureBox1.Height + l, 2];
                            sum++;
                        }
                    }
                    mean_r = mean_r / sum;
                    mean_g = mean_g / sum;
                    mean_b = mean_b / sum;
                    std_r = std_r / sum;
                    std_g = std_g / sum;
                    std_b = std_b / sum;

                    std_r = Math.Sqrt(std_r - (mean_r * mean_r));
                    std_g = Math.Sqrt(std_g - (mean_g * mean_g));
                    std_b = Math.Sqrt(std_b - (mean_b * mean_b));

                    if (mean_r >= max_meanr)
                    {
                        max_meanr = mean_r;
                    }
                    if (std_r >= max_stdr)
                    {
                        max_stdr = std_r;
                    }

                    if (mean_g >= max_meang)
                    {
                        max_meang = mean_g;
                    }
                    if (std_g >= max_stdg)
                    {
                        max_stdg = std_g;
                    }

                    if (mean_b >= max_meanb)
                    {
                        max_meanb = mean_b;
                    }
                    if (std_b >= max_stdb)
                    {
                        max_stdb = std_b;
                    }

                    sampleG[i * pictureBox1.Height + j, 3] = mean_r;
                    sampleG[i * pictureBox1.Height + j, 4] = mean_g;
                    sampleG[i * pictureBox1.Height + j, 5] = mean_b;
                    sampleG[i * pictureBox1.Height + j, 6] = std_r;
                    sampleG[i * pictureBox1.Height + j, 7] = std_g;
                    sampleG[i * pictureBox1.Height + j, 8] = std_b;



                }
            }
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    sampleG[i * pictureBox1.Height + j, 3] = sampleG[i * pictureBox1.Height + j, 3] / max_meanr;
                    sampleG[i * pictureBox1.Height + j, 4] = sampleG[i * pictureBox1.Height + j, 4] / max_meang;
                    sampleG[i * pictureBox1.Height + j, 5] = sampleG[i * pictureBox1.Height + j, 5] / max_meanb;
                    sampleG[i * pictureBox1.Height + j, 6] = sampleG[i * pictureBox1.Height + j, 6] / max_stdr;
                    sampleG[i * pictureBox1.Height + j, 7] = sampleG[i * pictureBox1.Height + j, 7] / max_stdg;
                    sampleG[i * pictureBox1.Height + j, 8] = sampleG[i * pictureBox1.Height + j, 8] / max_stdb;
                }
            }

        }



        //------------------------------------------------------------------
        //ImageData_black 
        //sampleB 存取黑白影像之特徵值(灰階值,x,y)皆正規化
        //------------------------------------------------------------------
        public void ImageData_Bs(string imageName, int numClusters, int featureNumber, out double[,] sampleB)
        {
            pictureBox1 = new PictureBox();

            image = Image.FromFile(imageName);
            pictureBox1.Image = image;
            pictureBox1.Height = image.Height;
            pictureBox1.Width = image.Width;

            myBitmap = new Bitmap(imageName);
            BinaryNumber = new Bitmap(myBitmap.Width, myBitmap.Height);
            ImageArray = new int[myBitmap.Width, myBitmap.Height];
            color_r = new int[myBitmap.Width, myBitmap.Height];
            color_g = new int[myBitmap.Width, myBitmap.Height];
            color_b = new int[myBitmap.Width, myBitmap.Height];

            sampleB = new double[pictureBox1.Image.Height * pictureBox1.Image.Width, 2 + 3];

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color A1 = myBitmap.GetPixel(i, j);
                    int AR = A1.R;
                    int AG = A1.G;
                    int AB = A1.B;
                    sampleB[i * pictureBox1.Height + j, 0] = Convert.ToDouble((AR + AG + AB) / 3.0 / 255.0);
                    sampleB[i * pictureBox1.Height + j, 1] = Convert.ToDouble(i / Convert.ToDouble(image.Width));
                    sampleB[i * pictureBox1.Height + j, 2] = Convert.ToDouble(j / Convert.ToDouble(image.Height));
                    sampleB[i * pictureBox1.Height + j, featureNumber] = i;
                    sampleB[i * pictureBox1.Height + j, featureNumber + 1] = j;
                }
            }
        }


        //------------------------------------------------------------------
        //ImageData_BMS
        //sampleB 存取黑白影像之特徵值(灰階值,3*3平均值,3*3標準差)
        //------------------------------------------------------------------
        public void ImageData_BMS(string imageName, int windowsize, int numClusters, int featureNumber, out double[,] sampleB)
        {
            pictureBox1 = new PictureBox();

            image = Image.FromFile(imageName);
            pictureBox1.Image = image;
            pictureBox1.Height = image.Height;
            pictureBox1.Width = image.Width;

            myBitmap = new Bitmap(imageName);
            BinaryNumber = new Bitmap(myBitmap.Width, myBitmap.Height);
            ImageArray = new int[myBitmap.Width, myBitmap.Height];
            color_r = new int[myBitmap.Width, myBitmap.Height];
            color_g = new int[myBitmap.Width, myBitmap.Height];
            color_b = new int[myBitmap.Width, myBitmap.Height];

            sampleB = new double[pictureBox1.Image.Height * pictureBox1.Image.Width, featureNumber + 2];
            int[,] feature = new int[sampleB.GetLength(0), featureNumber];

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color A1 = myBitmap.GetPixel(i, j);
                    int AR = A1.R;
                    int AG = A1.G;
                    int AB = A1.B;
                    sampleB[i * pictureBox1.Height + j, 0] = (AR + AG + AB) / 3;
                    sampleB[i * pictureBox1.Height + j, featureNumber] = i;
                    sampleB[i * pictureBox1.Height + j, featureNumber + 1] = j;
                }
            }


            for (int i = windowsize; i < pictureBox1.Width - windowsize; i++)
            {
                for (int j = windowsize; j < pictureBox1.Height - windowsize; j++)
                {
                    double mean_ = 0;
                    double std_ = 0;
                    double sum = 0;

                    for (int k = i - windowsize; k <= i + windowsize; k++)
                    {
                        for (int l = j - windowsize; l <= j + windowsize; l++)
                        {
                            mean_ += sampleB[k * pictureBox1.Height + l, 0];
                            std_ += sampleB[k * pictureBox1.Height + l, 0] * sampleB[k * pictureBox1.Height + l, 0];
                           
                            sum++;
                        }
                    }
                    mean_ = mean_ / sum;
                    std_ = std_ / sum;
                    std_ = Math.Sqrt(std_ - (mean_ * mean_));

                    sampleB[i * pictureBox1.Height + j, 1] = mean_;
                    sampleB[i * pictureBox1.Height + j, 2] = std_;

                }
            }



        }

        //------------------------------------------------------------------
        //ImageData_BMS
        //sampleB 存取黑白影像之特徵值(灰階值,3*3平均值,3*3標準差)皆正規化
        //------------------------------------------------------------------
        public void ImageData_BMSs(string imageName, int windowsize, int numClusters, int featureNumber, out double[,] sampleB)
        {
            pictureBox1 = new PictureBox();

            image = Image.FromFile(imageName);
            pictureBox1.Image = image;
            pictureBox1.Height = image.Height;
            pictureBox1.Width = image.Width;

            myBitmap = new Bitmap(imageName);
            BinaryNumber = new Bitmap(myBitmap.Width, myBitmap.Height);
            ImageArray = new int[myBitmap.Width, myBitmap.Height];
            color_r = new int[myBitmap.Width, myBitmap.Height];
            color_g = new int[myBitmap.Width, myBitmap.Height];
            color_b = new int[myBitmap.Width, myBitmap.Height];

            sampleB = new double[pictureBox1.Image.Height * pictureBox1.Image.Width, featureNumber + 2];
            int[,] feature = new int[sampleB.GetLength(0), featureNumber];

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color A1 = myBitmap.GetPixel(i, j);
                    int AR = A1.R;
                    int AG = A1.G;
                    int AB = A1.B;
                    sampleB[i * pictureBox1.Height + j, 0] = Convert.ToDouble((AR + AG + AB) / 3.0 / 255.0);

                    sampleB[i * pictureBox1.Height + j, featureNumber] = i;
                    sampleB[i * pictureBox1.Height + j, featureNumber + 1] = j;

                }
            }
            double max_mean = -10;
            double max_std = -100;

            for (int i = windowsize; i < pictureBox1.Width - windowsize; i++)
            {
                for (int j = windowsize; j < pictureBox1.Height - windowsize; j++)
                {
                    double mean_ = 0;
                    double std_ = 0;
                    double sum = 0;

                    for (int k = i - windowsize; k <= i + windowsize; k++)
                    {
                        for (int l = j - windowsize; l <= j + windowsize; l++)
                        {
                            mean_ += sampleB[k * pictureBox1.Height + l, 0];
                            std_ += sampleB[k * pictureBox1.Height + l, 0] * sampleB[k * pictureBox1.Height + l, 0];

                            sum++;
                        }
                    }
                    mean_ = mean_ / sum;
                    std_ = std_ / sum;
                    std_ = Math.Sqrt(std_ - (mean_ * mean_));
                    if (mean_ >= max_mean)
                    {
                        max_mean = mean_;
                    }
                    if (std_ >= max_std)
                    {
                        max_std = std_;
                    }
                    sampleB[i * pictureBox1.Height + j, 1] = mean_;
                    sampleB[i * pictureBox1.Height + j, 2] = std_;

                }
            }
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    sampleB[i * pictureBox1.Height + j, 1] = sampleB[i * pictureBox1.Height + j, 1] / max_mean;
                    sampleB[i * pictureBox1.Height + j, 2] = sampleB[i * pictureBox1.Height + j, 2] / max_std;
                }
            }


        }

        //------------------------------------------------------------------
        //ImageData_BMS
        //sampleB 存取黑白影像之特徵值(灰階值,3*3平均值,3*3標準差,x,y)皆正規化
        //------------------------------------------------------------------
        public void ImageData_BxyMS(string imageName, int windowsize, int numClusters, int featureNumber, out double[,] sampleB)
        {
            pictureBox1 = new PictureBox();

            image = Image.FromFile(imageName);
            pictureBox1.Image = image;
            pictureBox1.Height = image.Height;
            pictureBox1.Width = image.Width;

            myBitmap = new Bitmap(imageName);
            BinaryNumber = new Bitmap(myBitmap.Width, myBitmap.Height);
            ImageArray = new int[myBitmap.Width, myBitmap.Height];
            color_r = new int[myBitmap.Width, myBitmap.Height];
            color_g = new int[myBitmap.Width, myBitmap.Height];
            color_b = new int[myBitmap.Width, myBitmap.Height];

            sampleB = new double[pictureBox1.Image.Height * pictureBox1.Image.Width, featureNumber + 2];
            int[,] feature = new int[sampleB.GetLength(0), featureNumber];

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color A1 = myBitmap.GetPixel(i, j);
                    int AR = A1.R;
                    int AG = A1.G;
                    int AB = A1.B;
                    sampleB[i * pictureBox1.Height + j, 0] = Convert.ToDouble((AR + AG + AB) / 3.0 / 255.0);

                    sampleB[i * pictureBox1.Height + j, 3] = Convert.ToDouble(i / Convert.ToDouble(image.Width));
                    sampleB[i * pictureBox1.Height + j, 4] = Convert.ToDouble(j / Convert.ToDouble(image.Height));
                    sampleB[i * pictureBox1.Height + j, featureNumber] = i;
                    sampleB[i * pictureBox1.Height + j, featureNumber + 1] = j;

                }
            }
            double max_mean = -10;
            double max_std = -100;

            for (int i = windowsize; i < pictureBox1.Width - windowsize; i++)
            {
                for (int j = windowsize; j < pictureBox1.Height - windowsize; j++)
                {
                    double mean_ = 0;
                    double std_ = 0;
                    double sum = 0;

                    for (int k = i - windowsize; k <= i + windowsize; k++)
                    {
                        for (int l = j - windowsize; l <= j + windowsize; l++)
                        {
                            mean_ += sampleB[k * pictureBox1.Height + l, 0];
                            std_ += sampleB[k * pictureBox1.Height + l, 0] * sampleB[k * pictureBox1.Height + l, 0];

                            sum++;
                        }
                    }
                    mean_ = mean_ / sum;
                    std_ = std_ / sum;
                    std_ = Math.Sqrt(std_ - (mean_ * mean_));
                    if (mean_ >= max_mean)
                    {
                        max_mean = mean_;
                    }
                    if (std_ >= max_std)
                    {
                        max_std = std_;
                    }
                    sampleB[i * pictureBox1.Height + j, 1] = mean_;
                    sampleB[i * pictureBox1.Height + j, 2] = std_;

                }
            }
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    sampleB[i * pictureBox1.Height + j, 1] = sampleB[i * pictureBox1.Height + j, 1] / max_mean;
                    sampleB[i * pictureBox1.Height + j, 2] = sampleB[i * pictureBox1.Height + j, 2] / max_std;
                }
            }


        }

    }
}
