using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration
{
    public class DynamicPeakSeekingClass
    {
        private static List<double> originDataX = new List<double>();   //所有谱图原始数据X值
        private static List<double> originDataY = new List<double>();   //所有谱图原始数据Y值

        /// <summary>
        /// 一阶导斜率
        /// </summary>
        private static List<double> slope1 = new List<double>();
        /// <summary>
        /// 二阶导
        /// </summary>
        private static List<double> slope2 = new List<double>();

        private static bool bFindStart;   //寻起点标志
        private static bool bFindPeak;    //寻顶点标志
        private static bool bFindEnd;     //寻终点标志 

        private static int dPeakStartIndex;   //起点
        private static int dPeakIndex;        //顶点
        private static int dPeakEndIndex;     //终点 

        private int smoothWidth = 2;     //数据平滑窗口宽度

        /// <summary>
        /// 清除数据，下一个峰开始之前，必须先清除前一个峰
        /// </summary>
        public void ClearData()
        {
            originDataX.Clear();
            originDataY.Clear();
            slope1.Clear();
            slope2.Clear();


            bFindStart = true;
            bFindPeak = false;
            bFindEnd = false;

            dPeakStartIndex = 0;
            dPeakIndex = 0;
            dPeakEndIndex = 0;
        }

        /// <summary>
        /// 接收一个新的点，计算斜率，判断并返回是否是峰起点还是终点
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>0为数据点数不够，1为峰起点，2为顶点，3为终点</returns>
        public int AddNewPoint(double x, double y)
        {
            int result = 0;

            originDataX.Add(x);
            originDataY.Add(y);

            int lastIndex = originDataX.Count - 1;
            if (lastIndex > 0)
            {
                //求一阶导
                double tmpSlope1 = (originDataY[lastIndex] - originDataY[lastIndex - 1]) / (originDataX[lastIndex] - originDataX[lastIndex - 1]);
                slope1.Add(tmpSlope1);

                //求二阶导
                if (lastIndex > 1)
                {
                    int lastSlope1 = slope1.Count - 1;
                    double tmpSlope2 = (slope1[lastSlope1] - slope1[lastSlope1 - 1]) / (originDataX[lastSlope1] - originDataX[lastSlope1 - 1]);
                    slope2.Add(tmpSlope2);
                }

                int lastI = slope2.Count - 1;
                if (lastI > 0)
                {
                    #region 起点

                    if (bFindStart)
                    {
                        bool state1 = lastI > 0 && slope1[lastI] >= 0 && slope1[lastI - 1] < 0 && slope2[lastI] > 0;

                        bool state3 = lastI > 0 && slope1[lastI] > 0 && slope1[lastI] > slope1[lastI - 1] && slope2[lastI] > 0;

                        int startLoca = 0;

                        if (state1)
                        {
                            startLoca = lastI;
                        }
                        else if (state3)
                        {
                            startLoca = lastI;
                        }

                        if (startLoca != 0)
                        {
                            result = 1; 

                            dPeakStartIndex = startLoca;
                            bFindStart = false;
                            bFindPeak = true;
                        }
                    }

                    #endregion

                    #region 峰顶点

                    if (bFindPeak)
                    {
                        if ((slope1[lastI - 1] > 0 && slope1[lastI] < 0) && lastI - dPeakStartIndex > smoothWidth)
                        {
                            result = 2;

                            dPeakIndex = lastI;
                            bFindPeak = false;
                            bFindEnd = true;
                        }
                    }

                    #endregion

                    #region 终点

                    if (bFindEnd)
                    {
                        bool state1 = slope1[lastI - 1] < 0 && slope1[lastI] >= 0 && slope2[lastI - 1] > 0 && lastI - dPeakIndex >= smoothWidth;

                        if (state1)
                        {
                            result = 3;

                            dPeakEndIndex = lastI;
                            bFindEnd = false;
                            bFindStart = true;

                            //峰终点找到后，可清除当前峰数据
                            ClearData();
                        }

                    }

                    #endregion
                }
            }

            return result;
        } 


    }
}
