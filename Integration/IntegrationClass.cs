namespace Integration
{
    public class IntegrationClass
    {
        class ExList<T> : List<T>
        {
            public T Last
            {
                get
                {
                    return this[this.Count - 1];
                }
                set
                {
                    this[this.Count - 1] = value;
                }
            }
        }

        #region 变量

        private InitialIntegrationParameters initParam = new InitialIntegrationParameters();   //初始积分事件，与InitIntegrationEvent属性对应
        private List<ManualIntegrationEvent> manualEvents = new List<ManualIntegrationEvent>();   //积分事件列表，与IntegrationEvents属性对应
        private double m_DeadTime;   //死时间 
        private List<double> originDataX = new List<double>();   //所有谱图原始数据X值
        private List<double> originDataY = new List<double>();   //所有谱图原始数据Y值
        private ExList<PeakInformations> integralResultPeaks = new ExList<PeakInformations>();   //峰信息列表
        private PeakInformations currentFindPeak = new PeakInformations();   //峰信息

        private ExList<int> listPeakStart = new ExList<int>();   //峰起点标记
        private ExList<int> listPeak = new ExList<int>();   //峰顶点标记
        private ExList<int> listPeakEnd = new ExList<int>();   //峰终点标记

        private int smoothWidth;   //数据平滑窗口宽度，单位秒
        private double minSlope;   //由噪声决定的最小斜率
        private bool bFindStart;   //寻起点标志
        private bool bFindPeak;    //寻顶点标志
        private bool bFindEnd;     //寻终点标志 
        private int lastDataLength;   //上一次的数据长度
        private bool isAutoIntegral = true; //是否为自动积分

        #endregion

        #region 属性

        /// <summary>
        /// 全谱图的一阶导结果
        /// </summary>
        public List<double> Slope1 { get; set; } = new List<double>();

        /// <summary>
        /// 全谱图的二阶导集合
        /// </summary>
        public List<double> Slope2 { get; set; } = new List<double>();

        #endregion

        public IntegrationClass()
        {
            initParam.HalfPeakWidth = 0.05;
            initParam.Noise = 100;
            initParam.MinPeakArea = 0;
            initParam.MinPeakHigh = 0;
            initParam.bDetectShoulder = false;
            initParam.MultiPeakProcess = 'H';
        }

        /// <summary>
        /// 谱图积分
        /// </summary>
        /// <param name="listX"></param>
        /// <param name="listY"></param>
        /// <param name="halfPeakWidth">最小半峰宽</param>
        /// <param name="minPeakHigh">最小峰高</param>
        /// <param name="minPeakArea">最小峰面积</param>
        /// <returns></returns>
        public List<PeakInformations> DataIntegration(List<double> listX, List<double> listY, double halfPeakWidth, double minPeakHigh, double minPeakArea)
        {
            IntegrationParameters parameters = new IntegrationParameters();
            parameters.InitIntegration.HalfPeakWidth = halfPeakWidth;
            parameters.InitIntegration.MinPeakHigh = minPeakHigh;
            parameters.InitIntegration.MinPeakArea = minPeakArea;
            return DataIntegration(listX, listY, parameters);
        }

        /// <summary>
        /// 谱图积分
        /// </summary>
        /// <param name="listX"></param>
        /// <param name="listY"></param>
        /// <param name="parameters">积分参数</param>
        /// <returns></returns>
        public List<PeakInformations> DataIntegration(List<double> listX, List<double> listY, IntegrationParameters parameters)
        {
            List<PeakInformations> listPeaks = new List<PeakInformations>();

            if (listX.Count > 0 && listY.Count > 0)
            {
                ClearObjectData();

                //积分条件赋值
                initParam = parameters.InitIntegration;
                manualEvents = parameters.ManualIntegrationEvents;
                m_DeadTime = 0; //死时间

                //准备积分数据
                PrepareData(listX, listY);

                //自动积分
                listPeaks = AutoIntegration();

                //手动积分事件积分
                listPeaks = ManualIntegration(listPeaks);

                //计算斜率
                CalculatePeakSlope(listPeaks);
            }
            return listPeaks;
        }

        /// <summary>
        /// 准备待积分谱图数据
        /// </summary>
        public void PrepareData(List<double> listX, List<double> listY)
        {
            if (listX.Count > 0)
            {
                lastDataLength = originDataX.Count;

                for (int i = lastDataLength; i < listX.Count; i++)
                {
                    originDataX.Add(listX[i]);
                    //originDataX.Add(listX[i].X / 60);//将x转换为min单位
                    originDataY.Add(listY[i]);
                }
            }
        }

        /// <summary>
        /// 自动积分
        /// </summary>
        /// <returns></returns>
        public List<PeakInformations> AutoIntegration()
        {
            minSlope = initParam.Noise;
            smoothWidth = (int)(initParam.HalfPeakWidth);  //* 60

            if (smoothWidth < 1)
                smoothWidth = 1;

            if (originDataX.Count < smoothWidth)
                return new List<PeakInformations>();

            ExList<double> slope1 = new ExList<double>();
            ExList<double> slope2 = new ExList<double>();
            int mostNearZero = 0;
            double nearZeroValue = double.MaxValue;
            int tickPeakS = 0;
            int tickPeakE = 0;
            int beginDown = 0;

            double tmpSlope = 0;
            for (int j = 1; j < originDataY.Count; j++)
            {
                tmpSlope = (originDataY[j] - originDataY[j - 1]) / (originDataX[j] - originDataX[j - 1]);
                slope1.Add(tmpSlope);
            }

            for (int j = 1; j < slope1.Count; j++)
            {
                tmpSlope = (slope1[j] - slope1[j - 1]) / (originDataX[j] - originDataX[j - 1]);
                //slope2.Add(-1 * tmpSlope);
                slope2.Add(tmpSlope);
            }

            Slope1 = slope1; Slope2 = slope2;

            PeakInformations tempinfo = new PeakInformations();
            isAutoIntegral = true;
            bFindStart = true;
            bFindPeak = false;
            bFindEnd = false;
            int lastI = slope2.Count - 1;

            for (int i = 0; i <= lastI; i++)
            {
                #region 起点

                if (bFindStart)
                {
                    bool state1 = i > 0 && slope1[i] >= 0 && slope1[i - 1] < 0 && slope2[i] > 0;
                    bool state2 = listPeakEnd.Count > 0 && slope1[i] < slope1[listPeakEnd.Last] && slope2[i] < 0;
                    bool state3 = i > 0 && slope1[i] > 0 && slope1[i] > slope1[i - 1] && slope2[i] > 0;
                    int startLoca = 0;

                    if (state1)
                    {
                        startLoca = i;
                    }
                    else if (state2)
                    {
                        startLoca = listPeakEnd.Last;
                    }
                    else if (state3)
                    {
                        startLoca = i;
                    }

                    if (startLoca != 0)
                    {
                        currentFindPeak.PeakStartType = 'B';
                        listPeakStart.Add(startLoca);
                        currentFindPeak.StartPoint.Set(originDataX[startLoca], originDataY[startLoca]);
                        currentFindPeak.StartPoint.Slope1 = slope1[startLoca];
                        bFindStart = false;
                        bFindPeak = true;
                        tickPeakS = 0;
                        tickPeakE = 0;
                    }
                }

                #endregion

                #region 峰

                if (bFindPeak)
                {
                    if (slope1[i] > 0 && slope1[i + 1] < 0)

                        if (slope2[i] < 0 && tickPeakS == 0)
                            tickPeakS = i;

                    if (slope2[i] > 0 && slope1[i] < 0 && tickPeakS != 0)
                        tickPeakE = i;

                    if ((slope1[i] > 0 && slope1[i + 1] < 0) && i - listPeakStart.Last > smoothWidth)
                    {
                        int peakloca = MaxIndex(listPeakStart.Last, i, originDataY);
                        listPeak.Add(peakloca);
                        currentFindPeak.PeakVertexPoint.Set(originDataX[peakloca], originDataY[peakloca]);

                        integralResultPeaks.Add(currentFindPeak);

                        bFindPeak = false;
                        bFindEnd = true;
                        beginDown = peakloca;
                    }

                    //调整起点
                    if (listPeakEnd.Count > 0 && listPeakStart.Last - listPeakEnd.Last > smoothWidth && slope1[i] < 0)
                    {
                        int startLoca = i;
                        currentFindPeak.PeakStartType = 'B';
                        listPeakStart.Last = i;
                        currentFindPeak.StartPoint.Set(originDataX[startLoca], originDataY[startLoca]);
                        currentFindPeak.StartPoint.Slope1 = slope1[startLoca];
                    }
                }
                #endregion

                #region 终点

                if (bFindEnd)
                {
                    //调整顶点
                    if (originDataY[i] >= currentFindPeak.PeakValue)
                    {
                        int peakloca = i;
                        listPeak.Last = i;
                        currentFindPeak.PeakVertexPoint.Set(originDataX[peakloca], originDataY[peakloca]);

                        tempinfo = integralResultPeaks.Last;
                        tempinfo.PeakVertexPoint.Set(originDataX[peakloca], originDataY[peakloca]);
                        integralResultPeaks.Last = tempinfo;
                    }

                    bool state1 = slope1[i] < 0 && slope1[i + 1] >= 0 && slope2[i] > 0 && i - listPeak.Last >= smoothWidth;
                    bool state2 = currentFindPeak.PeakValue - originDataY[i] > minSlope;
                    int endloca = 0;

                    if (state1 && state2)
                    {
                        endloca = i + 1;
                        listPeakEnd.Add(endloca);
                        currentFindPeak.EndPoint.Set(originDataX[endloca], originDataY[endloca], 'B');
                        currentFindPeak.EndPoint.Slope1 = slope1[endloca];

                        bFindEnd = false;
                        bFindStart = true;
                    }

                    if (state1)
                    {
                        tempinfo = integralResultPeaks.Last;
                        tempinfo.EndPoint.Set(currentFindPeak.PeakEnd, currentFindPeak.PeakEndValue, currentFindPeak.PeakEndType);
                        if (endloca > 0)
                        {
                            currentFindPeak.EndPoint.Slope1 = slope1[endloca];
                            tempinfo.EndPoint.Slope1 = slope1[endloca];
                        }
                        integralResultPeaks.Last = tempinfo;
                    }

                    //避免漏掉最后无法满足终点条件的峰
                    if (integralResultPeaks.Count > 0 && i == lastI)
                    {
                        nearZeroValue = double.MaxValue;
                        for (int j = listPeak.Last; j < i; j++)
                        {
                            if (slope1[j] < 0 && 0 - slope1[j] < nearZeroValue)
                            {
                                mostNearZero = j;
                                nearZeroValue = 0 - slope1[j];
                            }
                        }

                        tempinfo = integralResultPeaks[integralResultPeaks.Count - 1];
                        tempinfo.EndPoint.Set(originDataX[mostNearZero], originDataY[mostNearZero]);
                        tempinfo.EndPoint.Slope1 = slope1[mostNearZero];
                        currentFindPeak.EndPoint.Slope1 = slope1[mostNearZero];
                        integralResultPeaks.Last = tempinfo;
                    }


                }

                #endregion
            }
            SelectFromWidth(integralResultPeaks);
            CalculatePeaksInf(integralResultPeaks, originDataX, originDataY);
            SelectFromWidth(integralResultPeaks);
            SelectPeak(integralResultPeaks, initParam);
            CalculatePeaksInf(integralResultPeaks, originDataX, originDataY); //因有峰被排除，与保留时间计算相关的参数需重新计算
            SelectFromHalfPeakWidth(integralResultPeaks);
            CalculateAreaPercent(integralResultPeaks);
            return integralResultPeaks;
        }

        /// <summary>
        /// 手动积分，基于自动积分结果根据积分事件表作重新积分
        /// </summary>
        /// <param name="peakInfoAuto"></param>
        /// <returns></returns>
        public List<PeakInformations> ManualIntegration(List<PeakInformations> peakListAuto)
        {
            PeakInformations tempInfo = new PeakInformations();
            PeakInformations tempInfoCopy = new PeakInformations();
            List<PeakInformations> peakList = new List<PeakInformations>(peakListAuto);
            InitialIntegrationParameters tempInitEvent = initParam;

            int j, k;

            isAutoIntegral = false;

            foreach (ManualIntegrationEvent tempEvent in manualEvents)
            {
                List<PeakInformations> tempList = new List<PeakInformations>();

                if (tempEvent.TimeStart > tempEvent.TimeEnd
                 || tempEvent.TimeStart > originDataX[originDataX.Count - 1]
                 || tempEvent.TimeEnd > originDataX[originDataX.Count - 1])
                    continue;

                double start = tempEvent.TimeStart;
                double end = tempEvent.TimeEnd;

                for (j = 0; j < originDataX.Count - 1; j++)
                {
                    if (start >= originDataX[j] && start < originDataX[j + 1])
                    {
                        start = originDataX[j];
                    }
                    if (end >= originDataX[j] && end < originDataX[j + 1])
                    {
                        end = originDataX[j];
                        break;
                    }
                }

                List<double> tempListX = new List<double>();
                List<double> tempListY = new List<double>();
                List<KeyValuePair<double, double>> pairs = GetPartData(peakListAuto, tempEvent.TimeStart, tempEvent.TimeEnd, ref tempListX, ref tempListY);

                switch (tempEvent.EventType)
                {
                    case IntegrationEventType.HalfPeakWidth://半峰宽

                        tempInitEvent.HalfPeakWidth = tempEvent.Parameter;

                        tempList = PartIntegral(tempListX, tempListY, tempInitEvent);
                        peakList = ReplacePeakList(peakList, tempList, tempEvent);
                        PeakTypeProcess(peakList, originDataX, originDataY, initParam); //因替换后需重新判断峰谷
                        CalculatePeaksInf(peakList, originDataX, originDataY);

                        break;
                    case IntegrationEventType.Noise://阈值

                        tempInitEvent.Noise = tempEvent.Parameter;
                        tempList = PartIntegral(tempListX, tempListY, tempInitEvent);
                        peakList = ReplacePeakList(peakList, tempList, tempEvent);
                        PeakTypeProcess(peakList, originDataX, originDataY, initParam); //因替换后需重新判断峰谷
                        CalculatePeaksInf(peakList, originDataX, originDataY);

                        break;
                    case IntegrationEventType.MinPeakWidth://最小峰宽

                        foreach (PeakInformations peak in peakList)
                        {
                            if (IsValid(peak, tempEvent))
                                tempList.Add(peak);
                        }
                        peakList = tempList;
                        CalculatePeaksInf(peakList, originDataX, originDataY);

                        break;
                    case IntegrationEventType.MinPeakArea://最小峰面积

                        foreach (PeakInformations peak in peakList)
                        {
                            if (IsValid(peak, tempEvent))
                                tempList.Add(peak);
                        }
                        peakList = tempList;
                        CalculatePeaksInf(peakList, originDataX, originDataY);

                        break;
                    case IntegrationEventType.MinPeakHigh: //最小峰高

                        foreach (PeakInformations peak in peakList)
                        {
                            if (IsValid(peak, tempEvent))
                                tempList.Add(peak);
                        }
                        peakList = tempList;
                        CalculatePeaksInf(peakList, originDataX, originDataY);

                        break;
                    case IntegrationEventType.DetectShourlder://肩峰检测

                        tempInitEvent.bDetectShoulder = true;
                        tempList = PartIntegral(tempListX, tempListY, tempInitEvent);
                        peakList = ReplacePeakList(peakList, tempList, tempEvent);

                        break;
                    case IntegrationEventType.ValeDevide://谷谷分割

                        for (j = 0; j < peakList.Count; j++)
                        {
                            tempInfo = peakList[j];

                            if (tempInfo.StartOnVale && tempInfo.PeakStart >= tempEvent.TimeStart && tempInfo.PeakStart <= tempEvent.TimeEnd)
                                tempInfo.PeakStartType = 'B';

                            if (tempInfo.EndOnVale && tempInfo.PeakEnd >= tempEvent.TimeStart && tempInfo.PeakEnd <= tempEvent.TimeEnd)
                                tempInfo.PeakEndType = 'B';

                            peakList[j] = tempInfo;
                        }
                        CalculatePeaksInf(peakList, originDataX, originDataY);

                        break;
                    case IntegrationEventType.VerticalDevide://垂直分割

                        for (j = 0; j < peakList.Count - 1; j++)
                        {
                            tempInfo = peakList[j];
                            tempInfoCopy = peakList[j + 1];
                            if (tempInfo.PeakStart >= tempEvent.TimeStart && tempInfoCopy.PeakEnd <= tempEvent.TimeEnd)
                            {
                                if (tempInfo.PeakEnd == tempInfoCopy.PeakStart)
                                {
                                    tempInfo.PeakEndType = 'V';
                                    tempInfoCopy.PeakStartType = 'V';
                                    peakList[j] = tempInfo;
                                    peakList[j + 1] = tempInfoCopy;
                                }
                            }
                        }
                        PeakTypeProcess(peakList, originDataX, originDataY, initParam); //因强制转换会出现基线贯穿的连续峰
                        CalculatePeaksInf(peakList, originDataX, originDataY);

                        break;
                    case IntegrationEventType.SinglePeak: //强制单峰，合并事件时间起止涉及到的峰

                        for (j = 0; j < peakList.Count; j++)
                        {
                            tempInfo = peakList[j];
                            if (tempInfo.StartOnVale && tempInfo.PeakStart >= tempEvent.TimeStart && tempInfo.PeakStart <= tempEvent.TimeEnd)
                                tempInfo.PeakStartType = 'S';
                            if (tempInfo.EndOnVale && tempInfo.PeakEnd >= tempEvent.TimeStart && tempInfo.PeakEnd <= tempEvent.TimeEnd)
                                tempInfo.PeakEndType = 'S';
                            peakList[j] = tempInfo;
                        }

                        List<PeakInformations> multiPeaks = new List<PeakInformations>();
                        for (j = 0; j < peakList.Count; j++)
                        {
                            if (peakList[j].PeakEndType == 'S' || peakList[j].PeakStartType == 'S')
                                multiPeaks.Add(peakList[j]);
                        }

                        if (multiPeaks.Count > 0)
                        {
                            int index = peakList.IndexOf(multiPeaks[0]);
                            double time = multiPeaks[0].RetentionTime;
                            double peakvalue = multiPeaks[0].PeakValue;
                            foreach (PeakInformations peak in multiPeaks)
                            {
                                peakList.RemoveAt(peakList.IndexOf(peak));
                                if (peakvalue < peak.PeakValue)
                                {
                                    time = peak.RetentionTime;
                                    peakvalue = peak.PeakValue;
                                }
                            }

                            tempInfo = multiPeaks[0];
                            tempInfo.PeakVertexPoint.Set(time, peakvalue);
                            tempInfo.EndPoint.Set(multiPeaks[multiPeaks.Count - 1].PeakEnd, multiPeaks[multiPeaks.Count - 1].PeakEndValue, multiPeaks[multiPeaks.Count - 1].PeakEndType);
                            peakList.Insert(index, tempInfo);
                            CalculatePeaksInf(peakList, originDataX, originDataY);
                        }

                        break;
                    case IntegrationEventType.NegativePeak://负峰

                        List<double> tempListX2 = new List<double>();
                        List<double> tempListY2 = new List<double>();
                        List<KeyValuePair<double, double>> pairs2 = GetNegativeData(tempEvent.TimeStart, tempEvent.TimeEnd, ref tempListX2, ref tempListY2);
                        tempList = PartIntegral(tempListX2, tempListY2, tempInitEvent);

                        for (j = 0; j < tempList.Count; j++)
                        {
                            tempInfo = tempList[j];
                            int st = originDataX.IndexOf(tempInfo.PeakStart);
                            int en = originDataX.IndexOf(tempInfo.PeakEnd);
                            int pe = MinIndex(st, en, originDataY);
                            tempInfo.PeakVertexPoint.Set(originDataX[pe], originDataY[pe]);
                            tempInfo.IsNegative = true;
                            tempList[j] = tempInfo;
                        }

                        peakList = ReplacePeakList(peakList, tempList, tempEvent);
                        CalculatePeaksInf(peakList, originDataX, originDataY);

                        break;
                    case IntegrationEventType.CleavePeak: //辟峰

                        for (j = 0; j < peakList.Count; j++)
                        {
                            tempInfo = peakList[j];
                            tempInfoCopy = tempInfo;

                            if (tempEvent.TimeStart > tempInfo.PeakStart && tempEvent.TimeStart < tempInfo.PeakEnd)
                            {
                                double YPeak;
                                int PeakStart, PeakEnd, Peak, PeakCleave;
                                PeakStart = originDataX.IndexOf(tempInfo.PeakStart);
                                PeakEnd = originDataX.IndexOf(tempInfo.PeakEnd);
                                Peak = originDataX.IndexOf(tempInfo.RetentionTime);
                                PeakCleave = Peak;

                                //找事件时间对应的X值
                                for (k = PeakStart; k < PeakEnd; k++)
                                {
                                    if (tempEvent.TimeStart >= originDataX[k] && tempEvent.TimeStart < originDataX[k + 1])
                                    {
                                        PeakCleave = k;
                                        break;
                                    }
                                }

                                //计算事件点所在的基线高度
                                if (tempInfo.BaseLineEValue > tempInfo.BaseLineSValue)
                                    YPeak = (tempInfo.BaseLineEValue - tempInfo.BaseLineSValue) * (PeakCleave - PeakStart) / (PeakEnd - PeakStart) + tempInfo.BaseLineSValue;
                                else
                                    YPeak = (tempInfo.BaseLineSValue - tempInfo.BaseLineEValue) * (PeakEnd - PeakCleave) / (PeakEnd - PeakStart) + tempInfo.BaseLineEValue;

                                int maxLoca = 0;

                                if (PeakCleave < Peak)//辟峰点在左半峰
                                {
                                    maxLoca = MaxIndex(PeakStart, PeakCleave, originDataY);
                                    tempInfo.PeakVertexPoint.Set(originDataX[maxLoca], originDataY[maxLoca]);
                                }
                                else
                                {
                                    maxLoca = MaxIndex(PeakCleave, PeakEnd, originDataY);
                                    tempInfoCopy.PeakVertexPoint.Set(originDataX[maxLoca], originDataY[maxLoca]);
                                }

                                tempInfo.EndPoint.Set(originDataX[PeakCleave], originDataY[PeakCleave], 'V', YPeak);
                                tempInfoCopy.StartPoint.Set(originDataX[PeakCleave], originDataY[PeakCleave], 'V', YPeak);

                                peakList.Insert(j, tempInfo);
                                peakList.Insert(j + 1, tempInfoCopy);
                                peakList.RemoveAt(j + 2);

                                CalculateSinglePeakInf(peakList, j, originDataX, originDataY);
                                CalculateSinglePeakInf(peakList, j + 1, originDataX, originDataY);
                                break;
                            }
                        }
                        CalculatePeaksInf(peakList, originDataX, originDataY);

                        break;
                    case IntegrationEventType.CancelIntegration: //取消积分

                        foreach (PeakInformations peak in peakList)
                        {
                            if (peak.PeakEnd < tempEvent.TimeStart || peak.PeakStart > tempEvent.TimeEnd)
                                tempList.Add(peak);
                        }

                        peakList = tempList;
                        CalculatePeaksInf(peakList, originDataX, originDataY);

                        break;
                    case IntegrationEventType.ManualBaseLine: //手动基线

                        int pLoca = -1;
                        for (j = 0; j < peakList.Count; j++)
                        {
                            if (peakList[j].PeakEnd <= tempEvent.TimeStart || peakList[j].PeakStart >= tempEvent.TimeEnd)
                            {
                                tempList.Add(peakList[j]);
                                if (pLoca == -1 && peakList[j].PeakStart >= tempEvent.TimeEnd)
                                    pLoca = tempList.Count - 1;
                            }
                        }

                        peakList = tempList;

                        tempInfo = new PeakInformations();
                        tempInfo.StartPoint.Set(start, tempEvent.Parameter, 'B');
                        int pIndex = MaxIndex(originDataX.IndexOf(start), originDataX.IndexOf(end), originDataY);
                        tempInfo.PeakVertexPoint.Set(originDataX[pIndex], originDataY[pIndex]);
                        tempInfo.EndPoint.Set(end, originDataY[originDataX.IndexOf(end)], 'B');

                        if (pLoca == -1)
                            peakList.Add(tempInfo);
                        else
                            peakList.Insert(pLoca, tempInfo);

                        CalculatePeaksInf(peakList, originDataX, originDataY);

                        break;
                    case IntegrationEventType.LevelBaseLine: //水平基线，将事件起止点内的峰认为是一个峰

                        List<PeakInformations> peaks = new List<PeakInformations>();

                        foreach (PeakInformations peak in peakList)
                        {
                            if ((tempEvent.TimeStart >= peak.PeakStart && tempEvent.TimeStart <= peak.PeakEnd)
                                || (tempEvent.TimeEnd >= peak.PeakStart && tempEvent.TimeEnd <= peak.PeakEnd)
                                || (peak.RetentionTime > tempEvent.TimeStart
                                && peak.RetentionTime < tempEvent.TimeEnd))
                                peaks.Add(peak);
                        }

                        if (peaks.Count > 0)
                        {
                            int loca = peakList.IndexOf(peaks[0]);

                            foreach (PeakInformations peak in peaks)
                            {
                                peakList.RemoveAt(peakList.IndexOf(peak));
                            }

                            tempInfo.StartPoint.Set(start, tempEvent.Parameter, 'B', tempEvent.Parameter);
                            tempInfo.PeakVertexPoint.Set(originDataX[MaxIndex(originDataX.IndexOf(start), originDataX.IndexOf(end), originDataY)], Max(originDataX.IndexOf(start), originDataX.IndexOf(end), originDataY));
                            tempInfo.EndPoint.Set(end, originDataY[originDataX.IndexOf(end)], 'B', tempEvent.Parameter);
                            peakList.Insert(loca, tempInfo);
                            CalculatePeaksInf(peakList, originDataX, originDataY);
                        }
                        break;
                }
            }

            SelectPeak(peakList, initParam);

            CalculateAreaPercent(peakList);
            return peakList;
        }

        #region 私有函数

        #region 寻峰处理

        /// 截取数据，用于手动积分
        /// </summary>
        /// <param name="peakInfoAuto"></param>
        /// <param name="tStart"></param>
        /// <param name="tEnd"></param>
        /// <returns></returns>
        private List<KeyValuePair<double, double>> GetPartData(List<PeakInformations> peakInfoAuto, double tStart, double tEnd, ref List<double> listX, ref List<double> listY)
        {
            listX.Clear();
            listY.Clear();
            List<KeyValuePair<double, double>> dataList = new List<KeyValuePair<double, double>>();
            int i;

            if (tStart > tEnd)
                return dataList;

            //调整起点位置为前一个峰的终点或后一个峰的起点
            for (i = 0; i < peakInfoAuto.Count - 1; i++)
            {
                if (tStart > peakInfoAuto[i].RetentionTime && tStart < peakInfoAuto[i + 1].RetentionTime)
                {
                    if (tStart < peakInfoAuto[i].PeakEnd)
                        tStart = peakInfoAuto[i].PeakEnd;
                    if (tStart > peakInfoAuto[i + 1].PeakStart)
                        tStart = peakInfoAuto[i + 1].PeakStart;
                    break;
                }
            }

            //调整终点位置为前一个峰的终点或后一个峰的起点
            for (; i < peakInfoAuto.Count - 1; i++)
            {
                if (tEnd > peakInfoAuto[i].RetentionTime && tEnd < peakInfoAuto[i + 1].RetentionTime)
                {
                    if (tEnd < peakInfoAuto[i].PeakEnd)
                        tEnd = peakInfoAuto[i].PeakEnd;
                    if (tEnd > peakInfoAuto[i + 1].PeakStart)
                        tEnd = peakInfoAuto[i + 1].PeakStart;
                    break;
                }
            }

            for (int j = 0; j < originDataX.Count; j++)
            {
                if (originDataX[j] >= tStart && originDataX[j] <= tEnd + initParam.HalfPeakWidth)
                {
                    listX.Add(originDataX[j]);
                    listY.Add(originDataY[j]);
                    dataList.Add(new KeyValuePair<double, double>(originDataX[j], originDataY[j]));
                }
            }

            return dataList;
        }

        /// <summary>
        /// 返回一段谱图的数据
        /// </summary>
        /// <param name="tStart"></param>
        /// <param name="tEnd"></param>
        /// <returns></returns>
        private List<KeyValuePair<double, double>> GetNegativeData(double tStart, double tEnd, ref List<double> listX, ref List<double> listY)
        {
            listX.Clear();
            listY.Clear();
            List<KeyValuePair<double, double>> dataList = new List<KeyValuePair<double, double>>();
            for (int i = 0; i < originDataX.Count; i++)
            {
                if (originDataX[i] >= tStart && originDataX[i] <= tEnd)
                {
                    listX.Add(originDataX[i]);
                    listY.Add(-originDataY[i]);
                    dataList.Add(new KeyValuePair<double, double>(originDataX[i], -originDataY[i]));
                }
            }

            return dataList;
        }

        /// <summary>
        /// 分段积分，用于手动积分
        /// </summary>
        /// <param name="pointList"></param>
        /// <param name="IntegralInitEvent"></param>
        /// <returns></returns>
        private List<PeakInformations> PartIntegral(List<double> listX, List<double> listY, InitialIntegrationParameters IntegralInitEvent)
        {
            initParam = IntegralInitEvent;
            PrepareData(listX, listY);
            return AutoIntegration();
        }

        /// <summary>
        /// 替换峰信息列表，用于手动积分
        /// </summary>
        /// <param name="peakInfoAuto"></param>
        /// <param name="subList"></param>
        /// <param name="integralEvent"></param>
        /// <returns></returns>
        private List<PeakInformations> ReplacePeakList(List<PeakInformations> peakInfoAuto, List<PeakInformations> subList, ManualIntegrationEvent integralEvent)
        {
            List<PeakInformations> templist = new List<PeakInformations>();

            if (subList.Count > 0)
            {
                if (peakInfoAuto.Count == 0)
                    templist = subList;
                else
                {
                    PeakInformations[] subListArray = subList.ToArray();

                    //subList[0]之前的峰
                    foreach (PeakInformations peak in peakInfoAuto)
                    {
                        if (peak.PeakEnd <= subList[0].PeakStart)
                        {
                            if (peak.PeakEnd == subList[0].PeakStart)
                            {
                                subListArray[0].PeakStartType = peak.PeakEndType;
                                subList[0] = subListArray[0];
                            }

                            templist.Add(peak);
                        }
                        else
                            break;
                    }

                    int index = 0;
                    foreach (PeakInformations peak in peakInfoAuto)
                    {
                        if (peak.PeakStart >= subList[subList.Count - 1].PeakEnd)
                            templist.Add(peak);
                        else if (index < subList.Count)
                        {
                            templist.Add(subList[index]);
                            index++;
                        }
                    }
                }
            }
            else
            {
                foreach (PeakInformations peak in peakInfoAuto)
                {
                    if (peak.PeakStart >= integralEvent.TimeStart && peak.PeakEnd <= integralEvent.TimeEnd)
                        continue;
                    else
                        templist.Add(peak);
                }
            }

            CheckPeakType(templist);

            return templist;
        }

        /// <summary>
        /// 融合峰判定及处理，峰型调整
        /// </summary>
        /// <param name="peakInfo"></param>
        /// <param name="listX"></param>
        /// <param name="listY"></param>
        /// <param name="inteInitEvent"></param>
        private void PeakTypeProcess(List<PeakInformations> peakInfo, List<double> listX, List<double> listY, InitialIntegrationParameters inteInitEvent)
        {
            int peakstart;
            int peakend;
            PeakInformations tempInfo1 = new PeakInformations();
            PeakInformations tempInfo2 = new PeakInformations();
            PeakInformations[] peakInfoArray;

            for (int i = 1; i < peakInfo.Count; i++)
            {
                tempInfo1 = peakInfo[i - 1];
                tempInfo2 = peakInfo[i];

                //如果两峰距离很近，则认为两峰相连
                peakstart = listX.IndexOf(tempInfo2.PeakStart);
                peakend = listX.IndexOf(tempInfo1.PeakEnd);

                if (peakstart < peakend) //排除肩峰情况
                {
                    continue;
                }

                if ((peakstart - peakend) < smoothWidth / 2)
                {
                    peakstart = peakend;
                    tempInfo2.PeakStart = listX[peakstart];
                    tempInfo2.PeakStartValue = listY[peakstart];
                }

                //本起点和前一个终点连续，改变本起点和前一个峰终点类型
                if (tempInfo2.PeakStart == tempInfo1.PeakEnd)
                {
                    if (inteInitEvent.MultiPeakProcess == 'H')
                    {
                        tempInfo2.PeakStartType = 'V';
                        tempInfo1.PeakEndType = 'V';
                    }
                    else if (inteInitEvent.MultiPeakProcess == 'S')
                    {
                        tempInfo2.PeakStartType = 'S';
                        tempInfo1.PeakEndType = 'S';
                    }

                    peakInfo[i - 1] = tempInfo1;
                    peakInfo[i] = tempInfo2;
                }
            }

            //处理出现负峰时不合理垂直分割的情况
            for (int i = 1; i < peakInfo.Count; i++)
            {
                tempInfo1 = peakInfo[i - 1];
                tempInfo2 = peakInfo[i];
                if (tempInfo1.PeakEndValue < -initParam.Noise
                    && tempInfo1.PeakEndValue < tempInfo1.PeakStartValue
                    && tempInfo2.PeakStartValue < tempInfo2.PeakEndValue
                    && tempInfo1.PeakValue - tempInfo1.PeakStartValue < tempInfo1.PeakStartValue - tempInfo1.PeakEndValue
                    && tempInfo2.PeakValue - tempInfo2.PeakEndValue < tempInfo2.PeakEndValue - tempInfo2.PeakStartValue)
                {
                    tempInfo1.PeakStartType = 'B';
                    tempInfo1.PeakEndType = 'B';
                    tempInfo2.PeakStartType = 'B';
                    tempInfo2.PeakEndType = 'B';
                    peakInfo[i - 1] = tempInfo1;
                    peakInfo[i] = tempInfo2;
                }
            }

            CheckPeakType(peakInfo);

            for (int i = 0; i < peakInfo.Count; i++)
                CalculateBaseLineValue(peakInfo, i, listX, listY);

            //调整基线贯穿


            //处理不合理垂直分割
            peakInfoArray = peakInfo.ToArray();
            for (int i = 1; i < peakInfo.Count; i++)
            {
                if (peakInfo[i].StartOnVale
                    && peakInfo[i].EndOnVale
                    && Math.Abs(peakInfo[i].PeakStartValue - peakInfo[i].PeakEndValue) < 3 * Math.Abs(peakInfo[i].BaseLineSValue - peakInfo[i].BaseLineEValue))
                {
                    peakInfoArray[i].PeakStartType = 'B';
                    peakInfoArray[i].PeakEndType = 'B';
                    peakInfo[i] = peakInfoArray[i];
                }
            }

            CheckPeakType(peakInfo);

            for (int i = 0; i < peakInfo.Count; i++)
                CalculateBaseLineValue(peakInfo, i, listX, listY);

            peakInfoArray = peakInfo.ToArray();

            for (int i = 0; i < peakInfo.Count; i++)
            {
                //处理基线贯穿峰终点的情况
                if (peakInfo[i].PeakEndValue < peakInfo[i].BaseLineEValue)
                {
                    peakInfoArray[i].PeakEndType = 'B';
                    peakInfoArray[i].BaseLineEValue = tempInfo1.PeakEndValue;
                    peakInfo[i] = peakInfoArray[i];

                    if (i + 1 < peakInfo.Count)
                    {
                        peakInfoArray[i + 1].PeakStartType = 'B';
                        peakInfoArray[i + 1].BaseLineSValue = tempInfo2.PeakStartValue;
                        peakInfo[i + 1] = peakInfoArray[i + 1];
                    }
                }
            }

            SinglePeakProcess(peakInfo);
        }

        /// <summary>
        /// 重叠峰处理参数为单峰处理时合并峰
        /// </summary>
        /// <param name="peakInfo"></param>
        private void SinglePeakProcess(List<PeakInformations> peakInfo)
        {
            if (initParam.MultiPeakProcess != 'S')
                return;

            int p1 = 0;
            int p2 = 0;
            List<PeakInformations> tempList = new List<PeakInformations>();
            PeakInformations temp = new PeakInformations();

            for (int i = 0; i < peakInfo.Count; i++)
            {
                if (peakInfo[i].StartOnBaseline)
                    p1 = i;

                if (peakInfo[i].EndOnBaseline)
                {
                    p2 = i;

                    double maxPeakValue = peakInfo[p1].PeakValue;
                    int maxPeakIndex = p1;
                    for (int j = p1 + 1; j <= p2; j++)
                    {
                        if (peakInfo[j].PeakValue > maxPeakValue)
                        {
                            maxPeakValue = peakInfo[j].PeakValue;
                            maxPeakIndex = j;
                        }
                    }

                    temp.StartPoint.Set(peakInfo[p1].PeakStart, peakInfo[p1].PeakStartValue, peakInfo[p1].PeakStartType, temp.BaseLineSValue);
                    temp.PeakVertexPoint.Set(peakInfo[maxPeakIndex].RetentionTime, maxPeakValue);
                    temp.EndPoint.Set(peakInfo[p2].PeakEnd, peakInfo[p2].PeakEndValue, peakInfo[p2].PeakEndType, temp.BaseLineEValue);
                    tempList.Add(temp);
                }
            }


            if (tempList.Count > 0)
            {
                peakInfo.Clear();
                foreach (PeakInformations peak in tempList)
                    peakInfo.Add(peak);
            }
        }

        /// <summary>
        /// 调整峰型，排除峰型不衔接的情况
        /// </summary>
        /// <param name="peakInfo"></param>
        private void CheckPeakType(List<PeakInformations> peakInfo)
        {
            PeakInformations temppeak1 = new PeakInformations();
            PeakInformations temppeak2 = new PeakInformations();
            for (int i = 0; i < peakInfo.Count - 1; i++)
            {
                temppeak1 = peakInfo[i];
                temppeak2 = peakInfo[i + 1];

                if ((temppeak1.EndOnBaseline && temppeak2.StartOnVale)
                    || (temppeak1.EndOnVale && temppeak2.StartOnBaseline))
                {
                    temppeak1.PeakEndType = 'B';
                    temppeak2.PeakStartType = 'B';
                    peakInfo[i] = temppeak1;
                    peakInfo[i + 1] = temppeak2;
                }
            }
        }

        /// <summary>
        /// 寻找满足初始积分条件（最小峰高，最小峰面积）的峰，用于自动积分
        /// </summary>
        /// <param name="peakInfo"></param>
        /// <param name="IntegralInitEvent"></param>
        /// <returns></returns>
        private void SelectPeak(List<PeakInformations> peakList, InitialIntegrationParameters IntegralInitEvent)
        {
            List<PeakInformations> tempList = new List<PeakInformations>();
            PeakInformations temppeak = new PeakInformations();

            List<ManualIntegrationEvent> menualEvents = new List<ManualIntegrationEvent>();
            foreach (ManualIntegrationEvent ev in manualEvents)
            {
                if (ev.EventType == IntegrationEventType.ManualBaseLine)
                    menualEvents.Add(ev);
            }

            for (int i = 0; i < peakList.Count; i++)
            {
                temppeak = peakList[i];

                if (temppeak.PeakStart < temppeak.RetentionTime && temppeak.RetentionTime < temppeak.PeakEnd)
                {
                    bool isInMenual = false;
                    foreach (ManualIntegrationEvent ev in menualEvents)
                    {
                        isInMenual = isInMenual
                             || (temppeak.PeakStart < ev.TimeStart && temppeak.PeakEnd > ev.TimeStart)
                             || (temppeak.PeakStart < ev.TimeEnd && temppeak.PeakEnd > ev.TimeEnd)
                             || (temppeak.PeakStart > ev.TimeStart && temppeak.PeakEnd < ev.TimeEnd);
                    }

                    if (isInMenual)
                        tempList.Add(temppeak); //手动基线内的峰不考虑最小峰高峰面积因素
                    else if (temppeak.PeakHigh > IntegralInitEvent.MinPeakHigh && temppeak.PeakArea > IntegralInitEvent.MinPeakArea)
                    {
                        if (i == 0 || (i > 0))
                        {
                            tempList.Add(temppeak);
                        }
                    }
                }

            }

            peakList.Clear();

            if (tempList.Count > 0)
            {
                PeakInformations[] tmpListArray = tempList.ToArray();

                tmpListArray[0].PeakStartType = 'B';
                tempList[0] = tmpListArray[0];

                tmpListArray[tempList.Count - 1].PeakEndType = 'B';
                tempList[tempList.Count - 1] = tmpListArray[tempList.Count - 1];

                foreach (PeakInformations p in tempList)
                    peakList.Add(p);
            }
        }

        private void SelectFromWidth(List<PeakInformations> peakList)
        {
            List<PeakInformations> tmpList = new List<PeakInformations>();
            int index = 0;
            foreach (PeakInformations peak in peakList)
            {
                if (peak.PeakEnd - peak.PeakStart > initParam.HalfPeakWidth * 1.5
                    && peak.PeakValue - peak.PeakStartValue > minSlope
                    && peak.PeakValue - peak.PeakEndValue > minSlope)
                {
                    tmpList.Add(peak);
                }
                index++;
            }

            peakList.Clear();
            foreach (PeakInformations peak in tmpList)
            {
                peakList.Add(peak);
            }
        }
        private void SelectFromHalfPeakWidth(List<PeakInformations> peakList)
        {
            List<PeakInformations> tmpList = new List<PeakInformations>();
            int index = 0;
            foreach (PeakInformations peak in peakList)
            {
                if (peak.HalfPeakWidth >= initParam.HalfPeakWidth)
                {
                    tmpList.Add(peak);
                }
                index++;
            }

            peakList.Clear();
            foreach (PeakInformations peak in tmpList)
            {
                peakList.Add(peak);
            }
        }

        /// <summary>
        /// 判断单个峰是否符合最小峰面积、峰宽等单个条件，用于手动积分
        /// </summary>
        /// <param name="peakInfo"></param>
        /// <param name="IntegrationEvent"></param>
        /// <returns></returns>
        private bool IsValid(PeakInformations peakInfo, ManualIntegrationEvent IntegrationEvent)
        {
            bool flag = true;

            if (peakInfo.RetentionTime > IntegrationEvent.TimeStart && peakInfo.RetentionTime < IntegrationEvent.TimeEnd)
            {
                switch (IntegrationEvent.EventType)
                {
                    case IntegrationEventType.MinPeakArea:
                        if (peakInfo.PeakArea < IntegrationEvent.Parameter)
                            flag = false;
                        break;
                    case IntegrationEventType.MinPeakHigh:
                        if (peakInfo.PeakHigh < IntegrationEvent.Parameter)
                            flag = false;
                        break;
                    case IntegrationEventType.MinPeakWidth:
                        if (peakInfo.HalfPeakWidth < IntegrationEvent.Parameter)
                            flag = false;
                        break;
                    default:
                        break;
                }
            }

            return flag;
        }
        #endregion

        #region 参数计算

        /// <summary>
        /// 计算起点终点在基线上的值
        /// </summary>
        /// <param name="peakInfo"></param>
        /// <param name="PeakIndex"></param>
        /// <param name="listX"></param>
        /// <param name="listY"></param>
        private void CalculateBaseLineValue(List<PeakInformations> peakList, int peakIndex, List<double> listX, List<double> listY)
        {
            PeakInformations[] peakArray = peakList.ToArray();

            int iBaseStart, iBaseEnd, iStart, iEnd;

            if (isAutoIntegral == false)
            {
                foreach (ManualIntegrationEvent tempevent in manualEvents)
                {
                    if (tempevent.EventType == IntegrationEventType.LevelBaseLine)
                    {
                        //涉及到水平基线事件的峰不重新计算基线上的值
                        if ((tempevent.TimeStart >= peakArray[peakIndex].PeakStart && tempevent.TimeStart <= peakArray[peakIndex].PeakEnd)
                            || (tempevent.TimeEnd >= peakArray[peakIndex].PeakStart && tempevent.TimeEnd <= peakArray[peakIndex].PeakEnd)
                            || (peakArray[peakIndex].RetentionTime > tempevent.TimeStart && peakArray[peakIndex].RetentionTime < tempevent.TimeEnd))
                            return;
                    }
                }
            }

            iStart = listX.IndexOf(peakArray[peakIndex].PeakStart);
            iEnd = listX.IndexOf(peakArray[peakIndex].PeakEnd);

            if (iStart < 0 || iEnd < 0)
                return;

            peakArray[peakIndex].PeakStartValue = listY[iStart];
            peakArray[peakIndex].PeakEndValue = listY[iEnd];
            peakList[peakIndex] = peakArray[peakIndex];

            if (iEnd <= iStart) //寻峰过程中还没有找到peakend
            {
                peakArray[peakIndex].EndPoint.Set(listX[iStart], listY[iStart], 'B', peakList[peakIndex].PeakStartValue);
                peakArray[peakIndex].BaseLineSValue = peakList[peakIndex].PeakStartValue;
                peakList[peakIndex] = peakArray[peakIndex];
                return;
            }

            int i = peakIndex;

            while (i > 0 && !peakList[i].StartOnBaseline)
                i--;

            iBaseStart = listX.IndexOf(peakList[i].PeakStart);

            i = peakIndex;

            while (i < peakList.Count - 1 && !peakList[i].EndOnBaseline)
                i++;

            iBaseEnd = listX.IndexOf(peakList[i].PeakEnd);

            if (peakArray[peakIndex].StartOnBaseline)
                peakArray[peakIndex].BaseLineSValue = peakArray[peakIndex].PeakStartValue;
            else
                peakArray[peakIndex].BaseLineSValue = (listY[iBaseEnd] - listY[iBaseStart]) / (iBaseEnd - iBaseStart) * (iStart - iBaseStart) + listY[iBaseStart];

            if (peakArray[peakIndex].EndOnBaseline)
                peakArray[peakIndex].BaseLineEValue = peakArray[peakIndex].PeakEndValue;
            else
                peakArray[peakIndex].BaseLineEValue = (listY[iBaseEnd] - listY[iBaseStart]) / (iBaseEnd - iBaseStart) * (iEnd - iBaseStart) + listY[iBaseStart];

            peakList[peakIndex] = peakArray[peakIndex];
        }

        /// <summary>
        /// 计算峰列表所有峰信息
        /// </summary>
        /// <param name="peakInfo"></param>
        /// <param name="listX"></param>
        /// <param name="listY"></param>
        private void CalculatePeaksInf(List<PeakInformations> peakList, List<double> listX, List<double> listY)
        {
            CheckPeakType(peakList);

            if (isAutoIntegral)
                PeakTypeProcess(peakList, listX, listY, initParam);


            for (int i = 0; i < peakList.Count; i++)
            {
                CalculateBaseLineValue(peakList, i, listX, listY);
                CalculateSinglePeakInf(peakList, i, listX, listY);
            }
        }

        /// <summary>
        /// 计算某一峰信息
        /// </summary>
        /// <param name="peakInfo"></param>
        /// <param name="index"></param>
        /// <param name="listX"></param>
        /// <param name="listY"></param>
        private void CalculateSinglePeakInf(List<PeakInformations> peakList, int index, List<double> listX, List<double> listY)
        {
            CalculatePeakArea(peakList, index, listX, listY);
            CalculatePeakHigh(peakList, index, listX, listY);
            CalculateHighRelate(peakList, index, listX, listY);
            CalculateTangentPeakWidth(peakList, index, listX, listY);
            CalculateTimeRelate(peakList, index);
        }

        private void CalculatePeakSlope(List<PeakInformations> peakList)
        {
            PeakInformations temp;

            for (int i = 0; i < peakList.Count; i++)
            {
                temp = peakList[i];

                int startIndex = originDataX.IndexOf(temp.PeakStart);
                if(startIndex > 0)
                {
                    temp.StartPoint.Slope1 = Slope1[startIndex - 1];
                }

                int endIndex = originDataX.IndexOf(temp.PeakEnd);
                if(endIndex > 0)
                {
                    temp.EndPoint.Slope1 = Slope1[endIndex - 1];
                }

                peakList[i] = temp;
            }
        }

        private void CalculateAreaPercent(List<PeakInformations> peakList)
        {
            double sum = 0;
            foreach (PeakInformations p in peakList)
            {
                sum += p.PeakArea;
            }

            PeakInformations temp;

            for (int i = 0; i < peakList.Count; i++)
            {
                temp = peakList[i];
                temp.AreaPercent = peakList[i].PeakArea / sum * 100;
                peakList[i] = temp;
            }
        }

        /// <summary>
        /// 计算峰面积
        /// </summary>
        /// <param name="peakInfo"></param>
        /// <param name="index"></param>
        /// <param name="listX"></param>
        /// <param name="listY"></param>
        private void CalculatePeakArea(List<PeakInformations> peakList, int index, List<double> listX, List<double> listY)
        {
            PeakInformations currentPeak = peakList[index];

            int iStart, iEnd, iPeak;
            double bigValue = 0;
            double smallValue = 0;

            iStart = listX.IndexOf(currentPeak.PeakStart);
            iPeak = listX.IndexOf(currentPeak.RetentionTime);
            iEnd = listX.IndexOf(currentPeak.PeakEnd);

            if (iStart < 0 || iEnd < 0)
                return;

            bool startSmall = true;
            double nSum;
            bool endChange = false;
            bool startChange = false;

            do
            {
                startSmall = true;
                if (currentPeak.BaseLineSValue > currentPeak.BaseLineEValue)
                {
                    bigValue = currentPeak.BaseLineSValue;
                    smallValue = currentPeak.BaseLineEValue;
                    startSmall = false;
                }
                else
                {
                    bigValue = currentPeak.BaseLineEValue;
                    smallValue = currentPeak.BaseLineSValue;
                }

                if (endChange)
                {
                    iEnd = iEnd - 1;
                    endChange = false;
                }
                else if (startChange)
                {
                    iStart = iStart + 1;
                    startChange = false;
                }

                double x = iEnd - iStart;
                double tg = (bigValue - smallValue) / x;
                nSum = 0;
                double xi = 0;
                double yi = 0;
                double temp = 0;
                double tempCycle;

                for (int i = iStart; i <= iEnd; i++)
                {
                    xi = 0;
                    yi = 0;

                    if (startSmall)
                        xi = i - iStart;
                    else
                        xi = iEnd - i;

                    if (currentPeak.IsNegative)
                    {
                        if (!startSmall)
                            xi = i - iStart;
                        else
                            xi = iEnd - i;
                        yi = xi * tg;
                        temp = bigValue - listY[i] - yi;
                    }
                    else
                    {
                        if (startSmall)
                            xi = i - iStart;
                        else
                            xi = iEnd - i;
                        yi = xi * tg;
                        temp = listY[i] - smallValue - yi;
                    }

                    if (iStart > 0)
                        tempCycle = listX[i] - listX[i - 1];
                    else
                        tempCycle = listX[i + 1] - listX[i];

                    if (Math.Round(temp, 4) >= 0)
                    {
                        nSum += Math.Round(temp, 4) * tempCycle;
                    }
                    else
                    {
                        if (i < iPeak)
                        {
                            currentPeak.StartPoint.Set(listX[iStart + 1], listY[iStart + 1]);
                            currentPeak.BaseLineSValue = listY[iStart + 1];
                            startChange = true;
                            break;
                        }
                        else if (i > iPeak)
                        {
                            currentPeak.EndPoint.Set(listX[iEnd - 1], listY[iEnd - 1]);
                            currentPeak.BaseLineEValue = listY[iEnd - 1];
                            endChange = true;
                            break;
                        }
                    }
                }

                if (endChange || startChange)
                {
                    CalculateBaseLineValue(peakList, index, listX, listY);
                }

            } while (endChange || startChange);

            currentPeak.PeakArea = nSum;// nSum * 60 * 1000; //峰面积           

            peakList[index] = currentPeak;
        }

        /// <summary>
        /// 计算峰高
        /// </summary>
        /// <param name="peakInfo"></param>
        /// <param name="index"></param>
        /// <param name="listX"></param>
        /// <param name="listY"></param>
        private void CalculatePeakHigh(List<PeakInformations> peakList, int index, List<double> listX, List<double> listY)
        {
            PeakInformations currentPeak = peakList[index];
            int iStart, iEnd, iPeak;
            double yBelowBaseLine;

            iStart = listX.IndexOf(currentPeak.PeakStart);
            iEnd = listX.IndexOf(currentPeak.PeakEnd);
            iPeak = listX.IndexOf(currentPeak.RetentionTime);

            if (iStart < 0 || iEnd < 0 || iPeak < 0)
                return;

            if (currentPeak.BaseLineEValue > currentPeak.BaseLineSValue)
                yBelowBaseLine = (currentPeak.BaseLineEValue - currentPeak.BaseLineSValue) * (iPeak - iStart) / (iEnd - iStart) + currentPeak.BaseLineSValue;
            else
                yBelowBaseLine = (currentPeak.BaseLineSValue - currentPeak.BaseLineEValue) * (iEnd - iPeak) / (iEnd - iStart) + currentPeak.BaseLineEValue;

            currentPeak.PeakHigh = listY[iPeak] - yBelowBaseLine;

            if (currentPeak.IsNegative)
                currentPeak.PeakHigh *= -1;

            peakList[index] = currentPeak;
        }

        /// <summary>
        /// 计算不对称性、拖尾因子、半峰宽、10％峰宽、拖尾峰宽、理论塔板数（与峰高有关）
        /// </summary>
        /// <param name="peakInfo"></param>
        /// <param name="index"></param>
        /// <param name="listX"></param>
        /// <param name="listY"></param>
        private void CalculateHighRelate(List<PeakInformations> peakList, int index, List<double> listX, List<double> listY)
        {
            PeakInformations currentPeak = peakList[index];
            int iStart, iEnd, iPeak;
            double[] LeftWidth = new double[4];
            double[] RightWidth = new double[4];
            double High44, High5, High10, High50;
            int i, j;

            iStart = listX.IndexOf(currentPeak.PeakStart);
            iEnd = listX.IndexOf(currentPeak.PeakEnd);
            iPeak = listX.IndexOf(currentPeak.RetentionTime);

            if (iStart < 0 || iEnd < 0 || iPeak < 0 || currentPeak.PeakHigh < 0)
                return;

            High44 = listY[iPeak] - 0.956 * currentPeak.PeakHigh;
            High5 = listY[iPeak] - 0.95 * currentPeak.PeakHigh;
            High10 = listY[iPeak] - 0.9 * currentPeak.PeakHigh;
            High50 = listY[iPeak] - 0.5 * currentPeak.PeakHigh;

            i = 0;
            for (j = iStart; j < iPeak; j++)
            {
                if (i == 0 && listY[j] >= High44)
                {
                    LeftWidth[0] = currentPeak.RetentionTime - listX[j];
                    i = 1;
                }
                if (i == 1 && listY[j] >= High5)
                {
                    LeftWidth[1] = currentPeak.RetentionTime - listX[j];
                    i = 2;
                }
                if (i == 2 && listY[j] >= High10)
                {
                    LeftWidth[2] = currentPeak.RetentionTime - listX[j];
                    i = 3;
                }
                if (i == 3 && listY[j] >= High50)
                {
                    LeftWidth[3] = currentPeak.RetentionTime - listX[j];
                    break;
                }
            }

            i = 3;
            for (j = iPeak; j < listY.Count;)
            {
                if (i == 0)
                {
                    if (listY[j] <= High44 || j == iEnd)
                    {
                        RightWidth[0] = listX[j] - currentPeak.RetentionTime;
                        currentPeak.TrailPeakWidth = LeftWidth[0] + RightWidth[0];

                        //拖尾理论塔板数
                        double temp1 = currentPeak.RetentionTime / currentPeak.TrailPeakWidth;
                        currentPeak.TPNTrail = 25 * temp1 * temp1;

                        //拖尾有效塔板数
                        double temp2 = 0;
                        if (m_DeadTime == 0)
                            temp2 = (currentPeak.RetentionTime - peakList[0].RetentionTime) / currentPeak.TrailPeakWidth;
                        else
                            temp2 = (currentPeak.RetentionTime - m_DeadTime) / currentPeak.TrailPeakWidth;
                        currentPeak.EPNTrail = 25 * temp2 * temp2;

                        break;
                    }
                }
                if (i == 1)
                {
                    if (listY[j] <= High5 || j == iEnd)
                    {
                        RightWidth[1] = listX[j] - currentPeak.RetentionTime;

                        //拖尾因子
                        if (LeftWidth[1] != 0)
                            currentPeak.TrailFactor = (LeftWidth[1] + RightWidth[1]) / (2 * LeftWidth[1]);
                        else
                            currentPeak.TrailFactor = 0;
                        i = 0;
                    }
                }
                if (i == 2)
                {
                    if (listY[j] <= High10 || j == iEnd)
                    {
                        RightWidth[2] = listX[j] - currentPeak.RetentionTime;
                        currentPeak.TenPeakWidth = LeftWidth[2] + RightWidth[2];

                        //10%理论塔板数
                        double temp1 = currentPeak.RetentionTime / currentPeak.TenPeakWidth;
                        currentPeak.TPNTen = 18.4 * temp1 * temp1;

                        //10%有效塔板数
                        double temp2 = 0;
                        if (m_DeadTime == 0)
                            temp2 = (currentPeak.RetentionTime - peakList[0].RetentionTime) / currentPeak.TenPeakWidth;
                        else
                            temp2 = (currentPeak.RetentionTime - m_DeadTime) / currentPeak.TenPeakWidth;
                        currentPeak.EPNTen = 18.4 * temp2 * temp2;

                        if (LeftWidth[2] != 0)
                            currentPeak.Symmetry = RightWidth[2] / LeftWidth[2];
                        else
                            currentPeak.Symmetry = 0;
                        i = 1;
                    }
                }
                if (i == 3)
                {
                    if (listY[j] <= High50 || j == iEnd)
                    {
                        RightWidth[3] = listX[j] - currentPeak.RetentionTime;
                        //半宽
                        currentPeak.HalfPeakWidth = LeftWidth[3] + RightWidth[3];

                        #region 原来计算理论塔板数的方法
                        //理论塔板数
                        double temp1 = currentPeak.RetentionTime / currentPeak.HalfPeakWidth;
                        currentPeak.TheoreticalPlateNumber = 5.54 * temp1 * temp1;

                        //有效塔板数
                        double temp2 = 0;
                        if (m_DeadTime == 0)
                            temp2 = (currentPeak.RetentionTime - peakList[0].RetentionTime) / currentPeak.HalfPeakWidth;
                        else
                            temp2 = (currentPeak.RetentionTime - m_DeadTime) / currentPeak.HalfPeakWidth;
                        currentPeak.EffectPlateNumber = 5.54 * temp2 * temp2;
                        #endregion

                        i = 2;
                    }
                }

                if (j != iEnd)
                    j++;
            }

            peakList[index] = currentPeak;
        }

        /// <summary>
        /// 计算容量因子、选择性因子、分离度（与保留时间有关）
        /// </summary>
        /// <param name="peakInfo"></param>
        /// <param name="index"></param>
        private void CalculateTimeRelate(List<PeakInformations> peakList, int index)
        {
            PeakInformations currentPeak = peakList[index];

            //容量因子，死时间为零时用第一个峰的时间作为死时间
            if (m_DeadTime == 0)
                currentPeak.CapacityFactor = currentPeak.RetentionTime / peakList[0].RetentionTime - 1;
            else
                currentPeak.CapacityFactor = currentPeak.RetentionTime / m_DeadTime - 1;

            if (index != 0)
            {
                //选择性因子

                currentPeak.SelectivityFactor = currentPeak.CapacityFactor / peakList[index - 1].CapacityFactor;

                if (double.IsInfinity(currentPeak.SelectivityFactor))
                    currentPeak.SelectivityFactor = 0; //置0 结果显示时代表不计算 “/”

                //分离度
                currentPeak.Resolution = 2 * (currentPeak.RetentionTime - peakList[index - 1].RetentionTime) / (currentPeak.TangentPeakWidth + peakList[index - 1].TangentPeakWidth);
            }
            else
            {
                //第一个峰不计算选择性因子
                currentPeak.SelectivityFactor = 0;
                //第一个峰的分离度认为前一个峰保留时间和半宽为0
                currentPeak.Resolution = (currentPeak.RetentionTime) / (currentPeak.TangentPeakWidth / 2);
            }

            peakList[index] = currentPeak;
        }

        /// <summary>
        /// 计算切线峰宽
        /// </summary>
        /// <param name="peakInfo"></param>
        /// <param name="index"></param>
        /// <param name="listX"></param>
        /// <param name="listY"></param>
        private void CalculateTangentPeakWidth(List<PeakInformations> peakList, int index, List<double> listX, List<double> listY)
        {
            PeakInformations currentPeak = peakList[index];

            int iStart = listX.IndexOf(currentPeak.PeakStart);
            int iEnd = listX.IndexOf(currentPeak.PeakEnd);
            int iPeak = listX.IndexOf(currentPeak.RetentionTime);

            if (iStart < 0 || iEnd < 0 || iStart >= iEnd)
                return;

            List<double> slope = new List<double>();

            for (int i = iStart; i <= iPeak; i++)
                slope.Add(listY[i + 1] - listY[i]);

            int iLeftInflex = iStart + MaxIndex(0, slope.Count - 1, slope);   //左拐点索引值
            double leftSlope = Max(0, slope.Count - 1, slope);
            slope.Clear();

            for (int i = iPeak; i <= iEnd; i++)
                slope.Add(listY[i + 1] - listY[i]);

            int iRightInflex = iPeak + MinIndex(0, slope.Count - 1, slope);  //右拐点索引值  
            double rightSlope = Min(0, slope.Count - 1, slope);

            double aLeft = leftSlope / (listX[iLeftInflex + 1] - listX[iLeftInflex]);
            double bLeft = listY[iLeftInflex] - aLeft * listX[iLeftInflex];
            double aRight = rightSlope / (listX[iRightInflex + 1] - listX[iRightInflex]);
            double bRight = listY[iRightInflex] - aRight * listX[iRightInflex];
            double aBase = (currentPeak.BaseLineEValue - currentPeak.BaseLineSValue) / (currentPeak.PeakEnd - currentPeak.PeakStart);
            double bBase = currentPeak.BaseLineSValue - aBase * currentPeak.PeakStart;
            double crossLeft = (bBase - bLeft) / (aLeft - aBase);
            double crossRight = (bBase - bRight) / (aRight - aBase);

            //切线峰宽
            currentPeak.TangentPeakWidth = Math.Abs(crossRight - crossLeft);

            //切线理论塔板数
            double temp1 = currentPeak.RetentionTime / currentPeak.TangentPeakWidth;
            currentPeak.TPNTangen = 16 * temp1 * temp1;

            //切线有效塔板数
            double temp2 = 0;
            if (m_DeadTime == 0)
                temp2 = (currentPeak.RetentionTime - peakList[0].RetentionTime) / currentPeak.TangentPeakWidth;
            else
                temp2 = (currentPeak.RetentionTime - m_DeadTime) / currentPeak.TangentPeakWidth;
            currentPeak.EPNTangen = 16 * temp2 * temp2;

            peakList[index] = currentPeak;
        }

        #endregion

        #region 公共

        /// <summary>
        /// 计算List<double>累加和
        /// </summary>
        /// <param name="LeftIndex"></param>
        /// <param name="RightIndex"></param>
        /// <param name="dataList"></param>
        /// <returns></returns>
        private double GetSum(int LeftIndex, int RightIndex, List<double> dataList)
        {
            if (LeftIndex < 0 || RightIndex - LeftIndex <= 0)
                return 0;
            else
            {
                double Sum = 0;
                List<double> temp = dataList.GetRange(LeftIndex, RightIndex - LeftIndex + 1);
                foreach (double d in temp)
                    Sum += d;

                return Sum;
            }
        }

        /// <summary>
        /// 求最大最小值 
        /// </summary>
        /// <param name="LeftIndex"></param>
        /// <param name="RightIndex"></param>
        /// <param name="array"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private double Max(int LeftIndex, int RightIndex, List<double> array)
        {
            if (LeftIndex < 0 || RightIndex - LeftIndex <= 0)
                return 0;
            else
            {
                double value = array[LeftIndex];
                List<double> temp = array.GetRange(LeftIndex, RightIndex - LeftIndex);
                temp.Sort();
                value = temp[temp.Count - 1];

                return value;
            }
        }

        private double Min(int LeftIndex, int RightIndex, List<double> array)
        {
            if (LeftIndex < 0 || RightIndex - LeftIndex <= 0)
                return 0;
            else
            {
                double value = array[LeftIndex];
                List<double> temp = array.GetRange(LeftIndex, RightIndex - LeftIndex);
                temp.Sort();
                value = temp[0];

                return value;
            }
        }

        /// <summary>
        /// 求最大最小值索引
        /// </summary>
        /// <param name="LeftIndex"></param>
        /// <param name="RightIndex"></param>
        /// <param name="array"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private int MaxIndex(int LeftIndex, int RightIndex, List<double> array)
        {
            if (LeftIndex < 0 || RightIndex - LeftIndex <= 0)
                return 0;
            else
            {
                int value = LeftIndex;
                for (int i = LeftIndex; i <= RightIndex && i < array.Count; i++)
                {
                    if (array[value] <= array[i])
                    {
                        value = i;
                    }
                }

                return value;
            }
        }

        private int MinIndex(int LeftIndex, int RightIndex, List<double> array)
        {
            if (LeftIndex < 0 || RightIndex - LeftIndex <= 0)
                return 0;
            else
            {
                int value = LeftIndex;

                for (int i = LeftIndex; i <= RightIndex && i < array.Count; i++)
                {
                    if (array[value] > array[i])
                        value = i;
                }

                return value;
            }
        }

        #endregion


        /// <summary>
        /// 清除以前谱图相关信息,开始新的积分,使用同一对象对不同数据进行积分时使用
        /// </summary>
        private void ClearObjectData()
        {
            originDataX.Clear();
            originDataY.Clear();
            integralResultPeaks.Clear();
            listPeakStart.Clear();
            listPeak.Clear();
            listPeakEnd.Clear();
        }

        #endregion

    }
}