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
        private List<double> tmpX = new List<double>();  //�ݴ�X������
        private List<double> tmpY = new List<double>();  //�ݴ�Y������
        List<double> slope1 = new List<double>();
        private List<PeakInformations> peaks = new List<PeakInformations>();
        private string lblName = "";
        private IntegrationParameters integrationParameters = new IntegrationParameters();
        private const string NONEPARAMETER = "/";   //�����¼�����ָ�����޲�����

        private double integralEventStart = 0;      //�����¼����λ��
        private double integralEventEnd = 0;        //�����¼��յ�λ��

        private bool enableManualInsert = false;        //�Ƿ������ֶ������¼���ʼ
        private bool beginInsertEvent = true;             //�����¼���ǲ�����ʼ���
        private LineItem eventStartLine = null;     //�����¼������
        private LineItem eventEndLine = null;       //�����¼��յ���

        private IntegrationEventType currentEventType = IntegrationEventType.Empty;    //��ǰ�ֶ������¼�����
        /// <summary>
        /// ��ǰ�ֶ������¼�����
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
                //�����
            }
            else
            {
                if (flag == 3)
                {
                    //���յ�
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
            openFileDialogData.Title = "����ͼ";
            openFileDialogData.FileName = "";
            openFileDialogData.InitialDirectory = Application.StartupPath;

            if (openFileDialogData.ShowDialog() == DialogResult.OK)
            {
                string openFileName = openFileDialogData.FileName;
                lblName = Path.GetFileNameWithoutExtension(openFileName);

                #region ���ļ�
                tmpX.Clear();
                tmpY.Clear();

                FileStream fs = new FileStream(openFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                StreamReader sr = new StreamReader(fs, Encoding.Default);
                string tmpStr = "";

                //ѭ����ȡcsv��ʽ����
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

                //Y��������ƽ��
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

                //��ʼ��������
                textBoxHalfPeakWidth.Text = "1";
                textBoxMinHight.Text = "100";
                textBoxMinArea.Text = "1";

                enableManualInsert = false;

                GetIntrgrationParams();
                //����
                IntegrationAndDisplay();

                //������ͼ
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

            //����ͼ
            LineItem graphLine = zedGraphControl1.GraphPane.AddCurve(lblName, tmpX.ToArray(), tmpY.ToArray(), Color.Blue, SymbolType.None);

            //������
            List<LineItem> baseLines = DrawBaseLine(peaks, tmpX, tmpY);

            #region ��ʱ����б����
            ZedGraph.RollingPointPairList Slope1PointList = new ZedGraph.RollingPointPairList(tmpX.Count);

            for (int i = 0; i < slope1.Count; i++)
            {
                Slope1PointList.Add(tmpX[i + 1], slope1[i]);  //����̫�󣬻�ͼʱ��С10��
            }
            LineItem Slope1Line = zedGraphControl1.GraphPane.AddCurve("Slope1", Slope1PointList, Color.Black, SymbolType.None);
            Slope1Line.Link.Title = "Slope1";

            #endregion

            zedGraphControl1.RestoreScale(zedGraphControl1.GraphPane);
        }

        /// <summary>
        /// ������ʾ�������������ҷ����갴���¼���
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private bool Graph_MouseDown(object sender, MouseEventArgs e)
        {
            double[] xbe = new double[2];
            double ptx = zedGraphControl1.GraphPane.XAxis.Scale.ReverseTransform(e.X);
            xbe = PeakSelect(ptx);

            //��������¼�
            if (enableManualInsert && e.Button == MouseButtons.Left)
            {
                if (beginInsertEvent) //�������
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
        /// ѡ����������
        /// </summary>
        /// <param name="pt">���x���ϵ�λ��</param>
        /// <returns>ѡ���еķ�������յ���ֵ</returns>
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
        /// �����ֶ����ֱ����
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

            //������ͼ
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
            #region ���ݻ���

            peaks.Clear();
            slope1.Clear();
            IntegrationClass integrationClass = new IntegrationClass();
            if (tmpX.Count > 0)
            {
                peaks = integrationClass.DataIntegration(tmpX, tmpY, integrationParameters);
                slope1 = integrationClass.Slope1;
            }
            #endregion

            #region ��ʾ����Ϣ

            this.label1.Text = "��������" + peaks.Count;

            int pageRowCount = 200;
            dataGridViewIntegration.Rows.Clear();
            SetResultDataGrid(peaks, pageRowCount, 0, dataGridViewIntegration);
            #endregion
        }

        /// <summary>
        /// ���õ�ǰ��ʾ�Ļ��ֽ��
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

            //������ͼ
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

            //������ͼ
            DrawChromatogram();

            enableManualInsert = false;
        }

        /// <summary>
        /// ����ֶ����ֱ����
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

        //���ֽ������
        private enum ITColumn
        {
            Index,      //�����
            Time,       //����ʱ��
            Area,       //�����
            AreaP,      //������ٷֱ� 
            High,       //���
            Sym,        //���Գ���
            TraiF,      //��β����
            TanW,       //���߷��
            HW,         //����
            TenW,       //10%���
            TraiW,      //��β���
            Theor,      //��������
            TheorTen,   //10%��������
            TheorTrail, //��β��������
            TheorTan,   //������������
            Effe,       //��Ч����
            EffeTen,    //10%��Ч����
            EffeTrail,  //��β��Ч����
            EffeTan,    //������Ч����
            Capa,       //��������
            Sele,       //ѡ��������
            Reso,       //�����
            Type,       //����
            StartSlope1,//���б��
            EndSlope1,  //�յ�б��
        }
    }
}