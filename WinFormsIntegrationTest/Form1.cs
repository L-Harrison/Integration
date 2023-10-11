using Integration;
using System.Data.Common;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using ZedGraph;
using static System.Formats.Asn1.AsnWriter;

namespace WinFormsIntegrationTest
{
    public partial class Form1 : Form
    {
        private List<double> tmpX = new List<double>();  //暂存X轴数据
        private List<double> tmpY = new List<double>();  //暂存Y轴数据
        List<double> slope1 = new List<double>();
        private List<PeakInformations> peaks = new List<PeakInformations>();
        private string lblName = "";
        private IntegrationParameters integrationParameters = new IntegrationParameters();
        private const string NONEPARAMETER = "/";   //积分事件表中指代“无参数”

        private double integralEventStart = 0;      //积分事件起点位置
        private double integralEventEnd = 0;        //积分事件终点位置

        private bool enableManualInsert = false;        //是否允许手动设置事件起始
        private bool beginInsertEvent = true;             //积分事件标记插入起始与否
        private LineItem eventStartLine = null;     //积分事件起点线
        private LineItem eventEndLine = null;       //积分事件终点线

        private IntegrationEventType currentEventType = IntegrationEventType.Empty;    //当前手动积分事件类型
        /// <summary>
        /// 当前手动积分事件类型
        /// </summary>
        private IntegrationEventType IntegralEventType
        {
            get
            {
                return currentEventType;
            }
            set
            {
                currentEventType = value;
                beginInsertEvent = true;
            }
        }

        public Form1()
        {
            InitializeComponent();
            InitGrap();

            DynamicPeakSeekingClass dynamicPeakSeekingClass = new DynamicPeakSeekingClass();
            dynamicPeakSeekingClass.ClearData();
            int flag = dynamicPeakSeekingClass.AddNewPoint(0, 0);
            if (flag == 1)
            {
                //峰起点
            }
            else
            {
                if (flag == 3)
                {
                    //峰终点
                }
            }
        }

        private void InitGrap()
        {
            zedGraphControl1.GraphPane.Fill = new ZedGraph.Fill(Color.White, Color.White, 45.0f);
            zedGraphControl1.GraphPane.Chart.Fill = new ZedGraph.Fill(Color.White, Color.White, 45.0f);
            zedGraphControl1.GraphPane.YAxis.MajorGrid.IsZeroLine = false;

            string fontfamily = "Times New Roman";

            float fontSize = 10;
            zedGraphControl1.GraphPane.Title.FontSpec.Size = fontSize;
            zedGraphControl1.GraphPane.Title.FontSpec.Family = fontfamily;
            zedGraphControl1.GraphPane.Title.FontSpec.IsBold = false;
            zedGraphControl1.GraphPane.Title.IsVisible = false;

            zedGraphControl1.GraphPane.XAxis.Title.FontSpec.Size = fontSize;
            zedGraphControl1.GraphPane.XAxis.Title.FontSpec.Family = fontfamily;
            zedGraphControl1.GraphPane.XAxis.Title.FontSpec.IsBold = false;
            zedGraphControl1.GraphPane.XAxis.Title.Text = "nm";

            zedGraphControl1.GraphPane.YAxis.Title.FontSpec.Size = fontSize;
            zedGraphControl1.GraphPane.YAxis.Title.FontSpec.Family = fontfamily;
            zedGraphControl1.GraphPane.YAxis.Title.FontSpec.IsBold = false;
            zedGraphControl1.GraphPane.YAxis.Title.Text = "mV";

            zedGraphControl1.GraphPane.XAxis.MinorGrid.IsVisible = false;
            zedGraphControl1.GraphPane.XAxis.MajorGrid.IsVisible = true;
            zedGraphControl1.GraphPane.XAxis.MajorGrid.Color = Color.Gray;
            zedGraphControl1.GraphPane.XAxis.MinorGrid.Color = Color.Gray;

            zedGraphControl1.GraphPane.YAxis.MinorGrid.IsVisible = false;
            zedGraphControl1.GraphPane.YAxis.MajorGrid.IsVisible = true;
            zedGraphControl1.GraphPane.YAxis.MajorGrid.Color = Color.Gray;
            zedGraphControl1.GraphPane.YAxis.MinorGrid.Color = Color.Gray;

            zedGraphControl1.GraphPane.XAxis.Scale.Format = "0.000";
            zedGraphControl1.GraphPane.XAxis.Scale.FontSpec.Size = fontSize;

            zedGraphControl1.GraphPane.YAxis.Scale.Format = "0.000";
            zedGraphControl1.GraphPane.YAxis.Scale.FontSpec.Size = fontSize;


            zedGraphControl1.MouseDownEvent += new ZedGraphControl.ZedMouseEventHandler(Graph_MouseDown);
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialogData = new OpenFileDialog();

            openFileDialogData.Multiselect = false;
            openFileDialogData.Title = "打开谱图";
            openFileDialogData.FileName = "";
            openFileDialogData.InitialDirectory = Application.StartupPath;

            if (openFileDialogData.ShowDialog() == DialogResult.OK)
            {
                string openFileName = openFileDialogData.FileName;
                lblName = Path.GetFileNameWithoutExtension(openFileName);

                #region 打开文件
                tmpX.Clear();
                tmpY.Clear();

                FileStream fs = new FileStream(openFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                StreamReader sr = new StreamReader(fs, Encoding.Default);
                string tmpStr = "";

                //循环读取csv格式数据
                int iCount = 0;
                while ((tmpStr = sr.ReadLine()) != null)
                {
                    double tempWave = 0;
                    double tempSignal = 0;
                    string[] temps = tmpStr.Split(' ');
                    if (temps.Length > 1)
                    {
                        if (double.TryParse(temps[0], out tempWave) && double.TryParse(temps[1], out tempSignal))
                        {
                            if (tempSignal > 0)
                            {
                                //tmpX.Add(tempWave);
                                tmpX.Add(iCount);
                                tmpY.Add(tempSignal);
                                iCount++;
                            }
                        }
                    }
                }

                //Y轴数据做平滑
                int smoothCount = 3;
                for (int i = 0; i < tmpY.Count; i++)
                {
                    double tempSum = tmpY[i];
                    int count = 1;
                    if (i >= smoothCount - 1)
                    {
                        while (count < smoothCount)
                        {
                            tempSum += tmpY[i - count];
                            count++;
                        }
                    }

                    tmpY[i] = tempSum / count;
                }

                #endregion

                //初始积分条件
                textBoxHalfPeakWidth.Text = "1";
                textBoxMinHight.Text = "100";
                textBoxMinArea.Text = "1";

                enableManualInsert = false;

                GetIntrgrationParams();
                //积分
                IntegrationAndDisplay();

                //绘制谱图
                DrawChromatogram();
            }

        }

        private void DrawChromatogram()
        {
            zedGraphControl1.GraphPane.CurveList.Clear();
            zedGraphControl1.GraphPane.GraphObjList.Clear();

            zedGraphControl1.GraphPane.XAxis.Scale.Min = 0;
            zedGraphControl1.GraphPane.XAxis.Scale.Max = tmpX.Max();
            zedGraphControl1.GraphPane.YAxis.Scale.Min = -10;
            zedGraphControl1.GraphPane.YAxis.Scale.Max = tmpY.Max() * 1.1;

            //画谱图
            LineItem graphLine = zedGraphControl1.GraphPane.AddCurve(lblName, tmpX.ToArray(), tmpY.ToArray(), Color.Blue, SymbolType.None);

            //画基线
            List<LineItem> baseLines = DrawBaseLine(peaks, tmpX, tmpY);

            #region 临时绘制斜率线
            ZedGraph.RollingPointPairList Slope1PointList = new ZedGraph.RollingPointPairList(tmpX.Count);

            for (int i = 0; i < slope1.Count; i++)
            {
                Slope1PointList.Add(tmpX[i + 1], slope1[i]);  //坐标太大，绘图时缩小10倍
            }
            LineItem Slope1Line = zedGraphControl1.GraphPane.AddCurve("Slope1", Slope1PointList, Color.Black, SymbolType.None);
            Slope1Line.Link.Title = "Slope1";

            #endregion

            zedGraphControl1.RestoreScale(zedGraphControl1.GraphPane);
        }

        /// <summary>
        /// 加亮显示峰或启动基线拖曳（鼠标按下事件）
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private bool Graph_MouseDown(object sender, MouseEventArgs e)
        {
            double[] xbe = new double[2];
            double ptx = zedGraphControl1.GraphPane.XAxis.Scale.ReverseTransform(e.X);
            xbe = PeakSelect(ptx);

            //插入积分事件
            if (enableManualInsert && e.Button == MouseButtons.Left)
            {
                if (beginInsertEvent) //插入起点
                {
                    ClearEventLine();
                    beginInsertEvent = false;
                    integralEventStart = ptx;
                    DrawEventLine(integralEventStart, ref eventStartLine, true);
                }
                else
                {
                    beginInsertEvent = true;
                    integralEventEnd = ptx;
                    DrawEventLine(integralEventEnd, ref eventEndLine, false);
                    if (integralEventStart <= integralEventEnd)
                    {
                        InsertIntigralEvent(currentEventType, integralEventStart, integralEventEnd);
                    }
                }
            }
            return default(bool);
        }

        /// <summary>
        /// 选择鼠标所点峰
        /// </summary>
        /// <param name="pt">鼠标x轴上的位置</param>
        /// <returns>选择中的峰起点与终点数值</returns>
        private double[] PeakSelect(double pt)
        {
            double[] x = new double[2];
            foreach (PeakInformations peak in peaks)
            {
                if (pt > peak.PeakStart && pt < peak.PeakEnd)
                {
                    x[0] = peak.PeakStart;
                    x[1] = peak.PeakEnd;
                    return x;
                }
            }
            return x;
        }

        /// <summary>
        /// 绘制手动积分标尺线
        /// </summary>
        /// <param name="xAis"></param>
        /// <param name="line"></param>
        /// <param name="g"></param>
        private void DrawEventLine(double xAis, ref LineItem line, bool isStart)
        {
            if (enableManualInsert)
            {
                PointPairList Peakbe = new PointPairList();
                Peakbe.Add(xAis, zedGraphControl1.GraphPane.YAxis.Scale.Max);
                Peakbe.Add(xAis, zedGraphControl1.GraphPane.YAxis.Scale.Min);
                if (isStart)
                {
                    line = zedGraphControl1.GraphPane.AddCurve("", Peakbe, Color.PaleGreen, ZedGraph.SymbolType.None);
                }
                else
                {
                    line = zedGraphControl1.GraphPane.AddCurve("", Peakbe, Color.PaleGreen, ZedGraph.SymbolType.None);
                }
                zedGraphControl1.Invalidate();
            }
        }

        private void InsertIntigralEvent(IntegrationEventType type, double timeStart, double timeEnd)
        {
            ManualIntegrationEvent currentEvent = new ManualIntegrationEvent();
            currentEvent.EventType = type;
            currentEvent.TimeStart = timeStart;
            currentEvent.TimeEnd = timeEnd;

            integrationParameters.ManualIntegrationEvents.Add(currentEvent);

            IntegrationAndDisplay();

            //绘制谱图
            DrawChromatogram();
        }

        private List<LineItem> DrawBaseLine(List<PeakInformations> peakList, List<double> listX, List<double> listY)
        {
            List<PeakInformations> peakInfoList = peakList;
            List<LineItem> lineItems = new List<LineItem>();
            ZedGraph.PointPairList tick = new ZedGraph.PointPairList();
            List<ZedGraph.PointPairList> listBaseLine = new List<ZedGraph.PointPairList>();
            ZedGraph.PointPairList listStart = new ZedGraph.PointPairList();
            ZedGraph.PointPairList listEnd = new ZedGraph.PointPairList();

            foreach (PeakInformations tempInfo in peakInfoList)
            {
                listStart.Add(tempInfo.PeakStart, tempInfo.PeakStartValue);
                listEnd.Add(tempInfo.PeakEnd, tempInfo.PeakEndValue);

                AddBaseLine(listBaseLine, tempInfo.BaseLineSS);
                AddBaseLine(listBaseLine, tempInfo.BaseLineSE);
                AddBaseLine(listBaseLine, tempInfo.BaseLineEE);
            }

            foreach (PointPairList baseline in listBaseLine)
            {
                LineItem item = zedGraphControl1.GraphPane.AddCurve("", baseline, Color.Red, SymbolType.None);
                lineItems.Add(item);
            }


            return lineItems;
        }

        private void AddBaseLine(List<ZedGraph.PointPairList> listBaseLine, List<KeyValuePair<double, double>> keyValuePairs)
        {
            ZedGraph.PointPairList tick = new ZedGraph.PointPairList();
            foreach (var pair in keyValuePairs)
            {
                tick.Add(pair.Key, pair.Value);
            }
            listBaseLine.Add(tick);
        }

        private void IntegrationAndDisplay()
        {
            #region 数据积分

            peaks.Clear();
            slope1.Clear();
            IntegrationClass integrationClass = new IntegrationClass();
            if (tmpX.Count > 0)
            {
                peaks = integrationClass.DataIntegration(tmpX, tmpY, integrationParameters);
                slope1 = integrationClass.Slope1;
            }
            #endregion

            #region 显示峰信息

            this.label1.Text = "峰数量：" + peaks.Count;

            int pageRowCount = 200;
            dataGridViewIntegration.Rows.Clear();
            SetResultDataGrid(peaks, pageRowCount, 0, dataGridViewIntegration);
            #endregion
        }

        /// <summary>
        /// 设置当前显示的积分结果
        /// </summary>
        /// <param name="peaks"></param>
        /// <param name="onePageRows"></param>
        /// <param name="pageIndex"></param>
        private void SetResultDataGrid(List<PeakInformations> peaks, int onePageRows, int pageIndex, DataGridView gridView)
        {
            int showRows = onePageRows;
            if (pageIndex * onePageRows + onePageRows > peaks.Count)
            {
                showRows = peaks.Count - pageIndex * onePageRows;
            }

            while (showRows > gridView.Rows.Count)
            {
                gridView.Rows.Add();
            }
            while (showRows < gridView.Rows.Count)
            {
                gridView.Rows.RemoveAt(gridView.Rows.Count - 1);
            }
            DataGridViewRowCollection allRows = gridView.Rows;
            PeakInformations peak = new PeakInformations();
            for (int i = 0; i < showRows; i++)
            {
                peak = peaks[pageIndex * onePageRows + i];
                allRows[i].Cells[(int)ITColumn.Index].Value = pageIndex * onePageRows + i + 1;
                allRows[i].Cells[(int)ITColumn.Time].Value = peak.RetentionTime.ToString("f3");
                allRows[i].Cells[(int)ITColumn.Area].Value = peak.PeakArea.ToString("f3");
                allRows[i].Cells[(int)ITColumn.AreaP].Value = peak.AreaPercent.ToString("f2");
                allRows[i].Cells[(int)ITColumn.High].Value = peak.PeakHigh.ToString("f3");
                allRows[i].Cells[(int)ITColumn.Sym].Value = peak.Symmetry.ToString("f3");
                allRows[i].Cells[(int)ITColumn.TraiF].Value = peak.TrailFactor.ToString("f3");
                allRows[i].Cells[(int)ITColumn.TanW].Value = peak.TangentPeakWidth.ToString("f3");
                allRows[i].Cells[(int)ITColumn.HW].Value = peak.HalfPeakWidth.ToString("f3");
                allRows[i].Cells[(int)ITColumn.TenW].Value = peak.TenPeakWidth.ToString("f3");
                allRows[i].Cells[(int)ITColumn.TraiW].Value = peak.TrailPeakWidth.ToString("f3");
                allRows[i].Cells[(int)ITColumn.Theor].Value = peak.TheoreticalPlateNumber.ToString("0");
                allRows[i].Cells[(int)ITColumn.TheorTen].Value = peak.TPNTen.ToString("0");
                allRows[i].Cells[(int)ITColumn.TheorTrail].Value = peak.TPNTrail.ToString("0");
                allRows[i].Cells[(int)ITColumn.TheorTan].Value = peak.TPNTangen.ToString("0");
                allRows[i].Cells[(int)ITColumn.Effe].Value = peak.EffectPlateNumber.ToString("0");
                allRows[i].Cells[(int)ITColumn.EffeTen].Value = peak.EPNTen.ToString("0");
                allRows[i].Cells[(int)ITColumn.EffeTrail].Value = peak.EPNTrail.ToString("0");
                allRows[i].Cells[(int)ITColumn.EffeTan].Value = peak.EPNTangen.ToString("0");
                allRows[i].Cells[(int)ITColumn.Capa].Value = peak.CapacityFactor.ToString("f3");
                if (peak.SelectivityFactor != 0)
                {
                    allRows[i].Cells[(int)ITColumn.Sele].Value = peak.SelectivityFactor.ToString("f3");
                }
                else
                {
                    allRows[i].Cells[(int)ITColumn.Sele].Value = NONEPARAMETER;
                }
                if (peak.Resolution != 0)
                {
                    allRows[i].Cells[(int)ITColumn.Reso].Value = peak.Resolution.ToString("f3");
                }
                else
                {
                    allRows[i].Cells[(int)ITColumn.Reso].Value = NONEPARAMETER;
                }
                allRows[i].Cells[(int)ITColumn.Type].Value = peak.PeakTypeString;
                allRows[i].Cells[(int)ITColumn.StartSlope1].Value = peak.StartPoint.Slope1.ToString("f3");
                allRows[i].Cells[(int)ITColumn.EndSlope1].Value = peak.EndPoint.Slope1.ToString("f3");
                //allRows[i].Cells[(int)ITColumn.StartSlope1].Value = peak.StartPoint.Time + "-" + peak.StartPoint.Value.ToString("f3") + "-" + peak.StartPoint.Slope1.ToString("f3");
                //allRows[i].Cells[(int)ITColumn.EndSlope1].Value = peak.EndPoint.Time + "-" + peak.EndPoint.Value.ToString("f3") + "-" + peak.EndPoint.Slope1.ToString("f3");
            }
            if (showRows + pageIndex * onePageRows == peaks.Count)
            {
                allRows.Add(1);
                double sumA;
                double sumH;
                double sumAp;
                GetIntegrationSum(peaks, out sumA, out sumH, out sumAp);
                allRows[allRows.Count - 1].Cells[(int)ITColumn.Index].Value = "Total";
                allRows[allRows.Count - 1].Cells[(int)ITColumn.Area].Value = sumA.ToString("f3");
                allRows[allRows.Count - 1].Cells[(int)ITColumn.AreaP].Value = sumAp.ToString("f2");
                allRows[allRows.Count - 1].Cells[(int)ITColumn.High].Value = sumH.ToString("f3");
            }
        }

        public void GetIntegrationSum(List<PeakInformations> peaks, out double sumArea, out double sumHeight, out double sumAreaPercent)
        {
            sumArea = 0;
            sumHeight = 0;
            sumAreaPercent = 0;

            foreach (PeakInformations p in peaks)
            {
                sumArea += p.PeakArea;
                sumHeight += p.PeakHigh;
                sumAreaPercent += p.AreaPercent;
            }
        }

        private void btnReIntegration_Click(object sender, EventArgs e)
        {
            enableManualInsert = false;

            GetIntrgrationParams();

            IntegrationAndDisplay();

            //绘制谱图
            DrawChromatogram();
        }

        private void GetIntrgrationParams()
        {
            integrationParameters.InitIntegration.Noise = double.Parse(textBoxNoise.Text.Trim());
            integrationParameters.InitIntegration.HalfPeakWidth = double.Parse(textBoxHalfPeakWidth.Text.Trim());
            integrationParameters.InitIntegration.MinPeakHigh = double.Parse(textBoxMinHight.Text.Trim());
            integrationParameters.InitIntegration.MinPeakArea = double.Parse(textBoxMinArea.Text.Trim());
        }

        private void btnAddPeak_Click(object sender, EventArgs e)
        {
            enableManualInsert = true;
            IntegralEventType = IntegrationEventType.ManualBaseLine;
        }

        private void btnCancelPeak_Click(object sender, EventArgs e)
        {
            enableManualInsert = true;
            IntegralEventType = IntegrationEventType.CancelIntegration;
        }

        private void btnDeleteAllEvents_Click(object sender, EventArgs e)
        {
            integrationParameters.ManualIntegrationEvents.Clear();
            ClearEventLine();

            IntegrationAndDisplay();

            //绘制谱图
            DrawChromatogram();

            enableManualInsert = false;
        }

        /// <summary>
        /// 清除手动积分标尺线
        /// </summary>
        private void ClearEventLine()
        {
            if (eventStartLine != null)
            {
                eventStartLine.Clear();
            }
            if (eventEndLine != null)
            {
                eventEndLine.Clear();
            }

            this.zedGraphControl1.Invalidate();
        }

        //积分结果表列
        private enum ITColumn
        {
            Index,      //峰序号
            Time,       //保留时间
            Area,       //峰面积
            AreaP,      //峰面积百分比 
            High,       //峰高
            Sym,        //不对称性
            TraiF,      //拖尾因子
            TanW,       //切线峰宽
            HW,         //半峰宽
            TenW,       //10%峰宽
            TraiW,      //拖尾峰宽
            Theor,      //理论塔板
            TheorTen,   //10%理论塔板
            TheorTrail, //拖尾理论塔板
            TheorTan,   //切线理论塔板
            Effe,       //有效塔板
            EffeTen,    //10%有效塔板
            EffeTrail,  //拖尾有效塔板
            EffeTan,    //切线有效塔板
            Capa,       //容量因子
            Sele,       //选择性因子
            Reso,       //分离度
            Type,       //峰型
            StartSlope1,//起点斜率
            EndSlope1,  //终点斜率
        }
    }
}