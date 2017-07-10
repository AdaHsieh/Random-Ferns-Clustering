using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;


namespace RFC
{
    class ZMatrix
    {
        private double[,] _data;
        public List<Point> _result = new List<Point>();
        public List<Point> _result1 = new List<Point>();
        private int _x;
        private int _y;

        public static int[][] a4;
        public static int bb = 0;

        public ZMatrix(int botNum, int PointNum, double[,] _d) //存取data
        {
            _x = botNum;
            _y = PointNum;
            _data = new double[botNum, PointNum];

            for (int i = 0; i < botNum; i++)
            {
                for (int j = 0; j < PointNum; j++)
                {
                    _data[i, j] = _d[i, j];  //data存取
                }

            }
        }

        public void Calculation()
        {
            //step0();
            int a = 0;
            step1();
            while (!step2())
            {
                step3();
                a++;
                if (a >= 2)
                {
                    permutations(_x);
                }
            }
            
            
        }

     
        /// <summary>
        /// 畫出最少數目的垂直與水平的刪除線來包含所有的零至少一次。
        /// </summary>
        private void step3()
        {
            bool[,] isDelete = new bool[_x, _y];
            for (int x = 0; x < _x; x++)
            {
                for (int y = 0; y < _y; y++)
                {
                    if (_data[x, y] == 0 && !isDelete[x, y])
                    {
                        int xc = 0;
                        int yc = 0;

                        //lie
                        for (int nx = 0; nx < _x; nx++)
                        {
                            if (nx != x && _data[nx, y] == 0)
                            {
                                xc++;
                            }
                        }

                        //hang
                        for (int ny = 0; ny < _y; ny++)
                        {
                            if (ny != y && _data[x, ny] == 0)
                            {
                                yc++;
                            }
                        }

                        if (xc > yc)
                        {
                            for (int xx = 0; xx < _x; xx++)
                            {
                                isDelete[xx, y] = true;
                            }
                        }
                        else
                        {
                            for (int yy = 0; yy < _y; yy++)
                            {
                                isDelete[x, yy] = true;
                            }
                        }
                    }
                }
            }


            //找出未被畫線的元素中之最小值 K
            double k = 99999;
            for (int x = 0; x < _x; x++)
            {
                for (int y = 0; y < _y; y++)
                {
                    if (!isDelete[x, y])
                    {
                        if (_data[x, y] < k)
                        {
                            k = _data[x, y];
                        }
                    }
                }
            }

            //將含有此些未被畫線的元素的各列所有元素減去K 
            for (int x = 0; x < _x; x++)
            {
                for (int y = 0; y < _y; y++)
                {
                    if (!isDelete[x, y])
                    {
                        for (int y1 = 0; y1 < _y; y1++)
                        {
                            _data[x, y1] -= k;
                        }
                        break;
                    }
                }
            }

            //若造成負值，則將該欄加上K (Step 4.2)。形成新矩陣後回到Step2
            for (int x = 0; x < _x; x++)
            {
                for (int y = 0; y < _y; y++)
                {
                    if (_data[x, y] < 0)
                    {
                        for (int x1 = 0; x1 < _x; x1++)
                        {
                            _data[x1, y] += k;
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 檢驗各列，對碰上之第一個零，做記號，同列或同欄的其他零則畫X (由零較少的列先做，可不依順序)
        /// 
        /// 檢驗可否完成僅含零的完全指派，若不能，則false
        /// </summary>
        private bool step2()
        {
            _result.Clear();
            bool[,] isDelete = new bool[_x, _y];

            //零的数量由少到多
            List<ZZeroNode> zeroNodes = new List<ZZeroNode>();
            for (int x = 0; x < _x; x++)
            {
                int zeroNum = 0;
                for (int y = 0; y < _y; y++)
                {
                    if (_data[x, y] == 0)
                    {
                        zeroNum++;
                    }
                }
                if (zeroNum > 0)
                {
                    zeroNodes.Add(new ZZeroNode(x, zeroNum));
                }
            }
            zeroNodes.Sort(ZZeroNode.Cmp);

            //从零较少的行开始
            while (zeroNodes.Count > 0)
            {
                ZZeroNode node = zeroNodes[0];

                if (node.ZeroNum <= 0)
                {
                    zeroNodes.RemoveAt(0);
                }
                else
                {
                    for (int y = 0; y < _y; y++)
                    {
                        if (_data[node.X, y] == 0 && !isDelete[node.X, y])
                        {
                            _result.Add(new Point(node.X, y));
                            zeroNodes.RemoveAt(0);

                            //删除与该零在同一列的其他零
                            for (int xxx = 0; xxx < _x; xxx++)
                            {
                                if (_data[xxx, y] == 0)
                                {
                                    isDelete[xxx, y] = true;
                                    for (int i = 0; i < zeroNodes.Count; i++)
                                    {
                                        if (zeroNodes[i].X == xxx)
                                        {
                                            zeroNodes[i].ZeroNum--;

                                        }
                                    }
                                }
                            }

                            break;
                        }
                    }
                }

                zeroNodes.Sort(ZZeroNode.Cmp);
            }
            return _result.Count == _x;
        }

        /// <summary>
        /// 在各列中找最小值，將該列中各元素檢去此值，對各行重複一次。
        /// </summary>
        private void step1()
        {
            //列
            for (int x = 0; x < _x; x++)
            {
                double minY = 99999;
                //找到每列最小的值
                for (int y = 0; y < _y; y++)
                {
                    if (_data[x, y] < minY)
                    {
                        minY = _data[x, y];
                    }
                }
                //讓該列减去最小的值
                for (int y = 0; y < _y; y++)
                {
                    _data[x, y] -= minY;
                }
            }
            //行
            for (int y = 0; y < _y; y++)
            {
                double minX = 99999;
                //找到每列最小的值
                for (int x = 0; x < _x; x++)
                {
                    if (_data[x, y] < minX)
                    {
                        minX = _data[x, y];
                    }
                }
                //讓該列减去最小的值
                for (int x = 0; x < _x; x++)
                {
                    _data[x, y] -= minX;
                }
            }
        }
        //private void step0()
        //{
        //    double max = 0;

        //    for (int x = 0; x < _x; x++)
        //    {
        //        //找到全部裡面最大的值
        //        for (int y = 0; y < _y; y++)
        //        {
        //            if (_data[x, y] > max)
        //            {
        //                max = _data[x, y];
        //            }
        //        }

        //    }
        //    for (int x = 0; x < _x; x++)
        //    {
        //        for (int y = 0; y < _y; y++)
        //        {
        //            _data[x, y] -= max;
        //        }
        //    }
        //    //行

        //}


        private void permutations(int Size)
        {
            string str = "";
            string result = "";
            int aa = 1;
                  

            for (int i = 0; i < Size; i++)
            {
                aa = aa * (i + 1);
                str += i;
            }
            int[,] a = new int[aa, Size];
            a4 = new int[aa][];
            cc = 0;
            permute(result, str);
            permuteCal(a4, _data, out _result);
        
        }
        public static int cc;
        static void permute(string result, string now)
        {
            
            if (now == "")
            {
               
                int[] a = new int[result.Length];

                for (int i = 0; i < a.Length; i++)
                {
                    a[i] = Convert.ToInt32(result.Substring(i, 1));
                }
                a4[cc] = a;
                cc++;
            }
            else
            {
                for (int i = 0; i < now.Length; i++)
                {
                    permute(result + now[i], now.Substring(0, i) + now.Substring(i + 1));

                }
            }
        }
        private void permuteCal(int[][] a4, double[,] _data, out List<Point> _result1)
        { 
           _result1 = new List<Point>();
           double [] sum = new double[a4.GetLength(0)];
            int a = 0;
            int max = -100;
            int index = 0;

           for (int i = 0; i < a4.GetLength(0); i++)
           {
               a = 0;
               for (int j = 0; j < a4[i].Length; j++)
               {
                   sum[i] += _data[a, j];
                   a= a + 1;
               } 
           }
           for (int i = 0; i < a4.GetLength(0); i++)
           {
               if (sum[i] > max)
               {
                   index = i;
               }
           }
           for (int i = 0; i < a4[i].Length; i++)
           {
               _result1.Add(new Point(i, a4[index][i]));
           }        
        }
    }

    class ZZeroNode
    {
        public int X;
        public int ZeroNum;

        public ZZeroNode(int x, int zeroNum)
        {
            X = x;
            ZeroNum = zeroNum;
        }

        public static int Cmp(ZZeroNode a, ZZeroNode b)
        {
            return a.ZeroNum.CompareTo(b.ZeroNum);
        }
    }

   
}
