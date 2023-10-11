using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration
{
    #region 峰信息
    /// <summary>
    /// 色谱峰的参数
    /// </summary>
    [Serializable]
    public struct PeakInformations
    {
        public double PeakStart                  //峰起点，单位min
        {
            get { return StartPoint.Time; }
            set { StartPoint.Time = value; }
        }
        public char PeakStartType                //峰起点类型，B(Base基线) V(Vale谷点) S(Single峰群中的谷点，不画标记)
        {
            get { return StartPoint.Type; }
            set { StartPoint.Type = value; }
        }
        public double RetentionTime               //峰顶点，保留时间，单位min
        {
            get { return PeakVertexPoint.Time; }
            set { PeakVertexPoint.Time = value; }
        }
        public double PeakEnd                    //峰终点，单位min
        {
            get { return EndPoint.Time; }
            set { EndPoint.Time = value; }
        }
        public char PeakEndType                  //峰终点类型，B(Base基线) V(Vale谷点) S(Single峰群中的谷点，不画标记)
        {
            get { return EndPoint.Type; }
            set { EndPoint.Type = value; }
        }
        public double PeakHigh;                   //峰高，单位mV
        public double PeakArea;                   //峰面积，单位mV*s
        public double AreaPercent;                //峰面积百分比
        public double Symmetry;                   //不对称性,B/A （10％峰高处）
        public double TrailFactor;                //拖尾因子,B/A （5％峰高处）
        public double TangentPeakWidth;           //切线峰宽,拐点处做切线，切线截取的基线长度
        public double HalfPeakWidth;              //半峰宽,1/2峰高处峰宽
        public double TenPeakWidth;               //10％峰宽
        public double TrailPeakWidth;             //拖尾峰宽,4.4％峰高处峰宽
        public double TheoreticalPlateNumber;    //理论塔板数（半峰宽）
        public double TPNTen;                    //10%理论塔板数
        public double TPNTrail;                    //拖尾理论塔板数
        public double TPNTangen;                    //切线理论塔板数
        public double EffectPlateNumber;         //有效塔板数
        public double EPNTen;                   //10%有效塔板数
        public double EPNTrail;                 //拖尾有效塔板数
        public double EPNTangen;                //切线有效塔板数
        public double CapacityFactor;             //容量因子
        public double SelectivityFactor;          //选择性因子 第一个峰没有选择性因子
        public double Resolution;               //（与前一个峰的）分离度 第一个峰没有分离度

        public double LeftInflexion;               //前拐点，单位min，用于计算切线峰宽
        public double RightInflexion;              //后拐点，单位min，用于计算切线峰宽
        public double PeakStartValue   //起点Y值,绘图基线时用
        {
            get { return StartPoint.Value; }
            set { StartPoint.Value = value; }
        }
        public double PeakValue        //顶点Y值,绘图基线时用
        {
            get { return PeakVertexPoint.Value; }
            set { PeakVertexPoint.Value = value; }
        }
        public double PeakEndValue     //终点Y值,绘图基线时用
        {
            get { return EndPoint.Value; }
            set { EndPoint.Value = value; }
        }
        public double BaseLineSValue    //起点基线Y值,绘图基线时用
        {
            get { return StartPoint.BaseLineValue; }
            set { StartPoint.BaseLineValue = value; }
        }
        public double BaseLineEValue    //终点基线Y值,绘图基线时用
        {
            get { return EndPoint.BaseLineValue; }
            set { EndPoint.BaseLineValue = value; }
        }

        public bool StartOnBaseline
        {
            get { return PeakStartType == 'B'; }
        }
        public bool EndOnBaseline
        {
            get { return PeakEndType == 'B'; }
        }
        public bool StartOnVale
        {
            get { return PeakStartType == 'V'; }
        }
        public bool EndOnVale
        {
            get { return PeakEndType == 'V'; }
        }

        /// <summary>
        /// 峰顶点
        /// </summary>
        public Point PeakVertexPoint;
        /// <summary>
        /// 峰起点
        /// </summary>
        public PeakInfo StartPoint;
        /// <summary>
        /// 峰终点
        /// </summary>
        public PeakInfo EndPoint;
        public bool IsNegative;

        public string PeakTypeString
        {
            get
            {
                return string.Format("{0}{1}", PeakStartType, PeakEndType);
            }
        }

        /// <summary>
        /// 峰起点线，竖直线
        /// </summary>
        public List<KeyValuePair<double, double>> BaseLineSS
        {
            get
            {
                List<KeyValuePair<double, double>> pairs = new List<KeyValuePair<double, double>>();
                pairs.Add(new KeyValuePair<double, double>(PeakStart, BaseLineSValue));
                pairs.Add(new KeyValuePair<double, double>(PeakStart, PeakStartValue));
                return pairs;
            }
        }

        /// <summary>
        /// 峰基线，起点到终点之间的水平线
        /// </summary>
        public List<KeyValuePair<double, double>> BaseLineSE
        {
            get
            {
                List<KeyValuePair<double, double>> pairs = new List<KeyValuePair<double, double>>();
                pairs.Add(new KeyValuePair<double, double>(PeakStart, BaseLineSValue));
                pairs.Add(new KeyValuePair<double, double>(PeakEnd, BaseLineEValue));
                return pairs;
            }
        }

        /// <summary>
        /// 峰终点线，竖直线
        /// </summary>
        public List<KeyValuePair<double, double>> BaseLineEE
        {
            get
            {
                List<KeyValuePair<double, double>> pairs = new List<KeyValuePair<double, double>>();
                pairs.Add(new KeyValuePair<double, double>(PeakEnd, BaseLineEValue));
                pairs.Add(new KeyValuePair<double, double>(PeakEnd, PeakEndValue));
                return pairs;
            }
        }
    }

    /// <summary>
    /// 峰顶点信息（时间+峰高）
    /// </summary>
    public struct Point
    {
        public double Time;
        public double Value;

        public void Set(double time, double value)
        {
            Time = time;
            Value = value;
        }
    }

    /// <summary>
    /// 峰起点和终点的信息
    /// </summary>
    public struct PeakInfo
    {
        public double Time;
        public double Value;
        public double BaseLineValue;
        public char Type;
        /// <summary>
        /// 起点和终点的一阶导斜率
        /// </summary>
        public double Slope1;

        public void Set(double time, double value, char type, double baseLineValue)
        {
            Time = time;
            Value = value;
            BaseLineValue = baseLineValue;
            Type = type;
        }

        public void Set(double time, double value, char type)
        {
            Time = time;
            Value = value;
            Type = type;
        }

        public void Set(double time, double value)
        {
            Time = time;
            Value = value;
        }
    }

    #endregion

    #region 积分参数
    /// <summary>
    /// 积分参数
    /// </summary>
    public class IntegrationParameters
    {
        /// <summary>
        /// 初始积分条件
        /// </summary>
        public InitialIntegrationParameters InitIntegration = new InitialIntegrationParameters();       
        /// <summary>
        /// 手动积分事件列表
        /// </summary>
        public List<ManualIntegrationEvent> ManualIntegrationEvents = new List<ManualIntegrationEvent>();
        /// <summary>
        /// 死时间，单位s
        /// </summary>
        public double DeadTime = 0;

        public IntegrationParameters()
        {
        }

        public IntegrationParameters(InitialIntegrationParameters initIntegration)
        {
            InitIntegration = initIntegration;
        }

        public IntegrationParameters(InitialIntegrationParameters initIntegration, List<ManualIntegrationEvent> manualEvents)
        {
            InitIntegration = initIntegration;
            ManualIntegrationEvents = manualEvents;
        }
    }

    /// <summary>
    /// 初始积分参数，自动积分时采用
    /// </summary>
    [Serializable]
    public struct InitialIntegrationParameters
    {
        /// <summary>
        /// 半峰宽
        /// </summary>
        public double HalfPeakWidth;    //半峰宽
        /// <summary>
        /// 阈值，用于判定峰终点（顶点峰高-基线>阈值）
        /// </summary>
        public double Noise;            //阈值(噪声水平)，单位mV
        /// <summary>
        /// 最小峰高
        /// </summary>
        public double MinPeakHigh;      //最小峰高，单位mV
        /// <summary>
        /// 最小峰面积
        /// </summary>
        public double MinPeakArea;      //最小峰面积，单位mV*s
        public bool bDetectShoulder;    //是否进行肩峰检测
        public char MultiPeakProcess;   //重叠峰处理，谷谷分割'V'、垂直分割'H'、强制单峰'S'

        public InitialIntegrationParameters(double width, double noise, double height, double area, bool detectShoulder, char multiProcess)
        {
            HalfPeakWidth = width;
            Noise = noise;
            MinPeakHigh = height;
            MinPeakArea = area;
            bDetectShoulder = detectShoulder;
            MultiPeakProcess = multiProcess;
        }

        public static char Vertical = 'H';
        public static char Vale = 'V';
        public static char Single = 'S';

        public void IndexToMultiProcess(int index)
        {
            switch (index)
            {
                case 0:
                    MultiPeakProcess = InitialIntegrationParameters.Vertical;
                    break;
                case 1:
                    MultiPeakProcess = InitialIntegrationParameters.Vale;
                    break;
                case 2:
                    MultiPeakProcess = InitialIntegrationParameters.Single;
                    break;
                default:
                    MultiPeakProcess = InitialIntegrationParameters.Vale;
                    break;
            }
        }

        public int MoutiProcessIndex()
        {
            if (MultiPeakProcess == InitialIntegrationParameters.Vertical)
                return 0;
            else if (MultiPeakProcess == InitialIntegrationParameters.Vale)
                return 1;
            else if (MultiPeakProcess == InitialIntegrationParameters.Single)
                return 2;

            return 0;
        }

        public void IndexToDetectShoulder(int index)
        {
            switch (index)
            {
                case 0:
                    bDetectShoulder = true;
                    break;
                case 1:
                    bDetectShoulder = false;
                    break;
                default:
                    bDetectShoulder = false;
                    break;
            }
        }

        public int DetectShouderIndex()
        {
            if (bDetectShoulder)
                return 0;
            else
                return 1;
        }

    }

    /// <summary>
    /// 手动积分事件
    /// </summary>
    [Serializable]
    public struct ManualIntegrationEvent
    {
        public double TimeStart;                //事件开始时间
        public double TimeEnd;                  //事件结束时间
        public IntegrationEventType EventType;　//事件类型
        public double Parameter;                //事件参数

    }

    /// <summary>
    /// 手动积分事件类型
    /// </summary>
    public enum IntegrationEventType
    {
        ManualBaseLine,     //手动基线 
        CancelIntegration,  //取消积分 
        HalfPeakWidth,      //调整半宽
        Noise,              //调整阈值
        MinPeakWidth,       //设定最小峰宽
        MinPeakArea,        //设定最小峰面积
        MinPeakHigh,        //设定最小峰高
        LevelBaseLine,       //水平基线
        DetectShourlder,    //肩峰处理
        ValeDevide,         //谷谷分割
        VerticalDevide,     //垂直分割
        SinglePeak,         //强制单峰 
        NegativePeak,       //处理负峰 
        CleavePeak,         //强制劈峰 
        Empty,               //空事件
    };

    #endregion
}
