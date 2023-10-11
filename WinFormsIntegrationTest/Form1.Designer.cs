namespace WinFormsIntegrationTest
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            splitContainer1 = new SplitContainer();
            btnCancelPeak = new Button();
            btnAddPeak = new Button();
            btnDeleteAllEvents = new Button();
            label1 = new Label();
            groupBox1 = new GroupBox();
            textBoxMinArea = new TextBox();
            textBoxMinHight = new TextBox();
            textBoxNoise = new TextBox();
            textBoxHalfPeakWidth = new TextBox();
            label11 = new Label();
            label10 = new Label();
            label2 = new Label();
            label8 = new Label();
            btnReIntegration = new Button();
            btnOpenFile = new Button();
            splitContainer2 = new SplitContainer();
            zedGraphControl1 = new ZedGraph.ZedGraphControl();
            dataGridViewIntegration = new DataGridView();
            Column1 = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            Column3 = new DataGridViewTextBoxColumn();
            integralColumnAreaPercent = new DataGridViewTextBoxColumn();
            Column4 = new DataGridViewTextBoxColumn();
            Column5 = new DataGridViewTextBoxColumn();
            Column6 = new DataGridViewTextBoxColumn();
            Column7 = new DataGridViewTextBoxColumn();
            Column8 = new DataGridViewTextBoxColumn();
            Column9 = new DataGridViewTextBoxColumn();
            Column10 = new DataGridViewTextBoxColumn();
            Column11 = new DataGridViewTextBoxColumn();
            Column39 = new DataGridViewTextBoxColumn();
            Column40 = new DataGridViewTextBoxColumn();
            Column41 = new DataGridViewTextBoxColumn();
            Column37 = new DataGridViewTextBoxColumn();
            Column42 = new DataGridViewTextBoxColumn();
            Column43 = new DataGridViewTextBoxColumn();
            Column44 = new DataGridViewTextBoxColumn();
            Column12 = new DataGridViewTextBoxColumn();
            Column13 = new DataGridViewTextBoxColumn();
            Column14 = new DataGridViewTextBoxColumn();
            Column15 = new DataGridViewTextBoxColumn();
            Column16 = new DataGridViewTextBoxColumn();
            Column17 = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewIntegration).BeginInit();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(4);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(btnCancelPeak);
            splitContainer1.Panel1.Controls.Add(btnAddPeak);
            splitContainer1.Panel1.Controls.Add(btnDeleteAllEvents);
            splitContainer1.Panel1.Controls.Add(label1);
            splitContainer1.Panel1.Controls.Add(groupBox1);
            splitContainer1.Panel1.Controls.Add(btnReIntegration);
            splitContainer1.Panel1.Controls.Add(btnOpenFile);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(1507, 824);
            splitContainer1.SplitterDistance = 328;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 1;
            // 
            // btnCancelPeak
            // 
            btnCancelPeak.Location = new Point(18, 437);
            btnCancelPeak.Name = "btnCancelPeak";
            btnCancelPeak.Size = new Size(75, 23);
            btnCancelPeak.TabIndex = 5;
            btnCancelPeak.Text = "删除峰";
            btnCancelPeak.UseVisualStyleBackColor = true;
            btnCancelPeak.Click += btnCancelPeak_Click;
            // 
            // btnAddPeak
            // 
            btnAddPeak.Location = new Point(18, 395);
            btnAddPeak.Name = "btnAddPeak";
            btnAddPeak.Size = new Size(75, 23);
            btnAddPeak.TabIndex = 5;
            btnAddPeak.Text = "添加峰";
            btnAddPeak.UseVisualStyleBackColor = true;
            btnAddPeak.Click += btnAddPeak_Click;
            // 
            // btnDeleteAllEvents
            // 
            btnDeleteAllEvents.Location = new Point(18, 479);
            btnDeleteAllEvents.Name = "btnDeleteAllEvents";
            btnDeleteAllEvents.Size = new Size(123, 23);
            btnDeleteAllEvents.TabIndex = 4;
            btnDeleteAllEvents.Text = "取消所有手动事件";
            btnDeleteAllEvents.UseVisualStyleBackColor = true;
            btnDeleteAllEvents.Click += btnDeleteAllEvents_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 352);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(63, 17);
            label1.TabIndex = 2;
            label1.Text = "峰数量：0";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(textBoxMinArea);
            groupBox1.Controls.Add(textBoxMinHight);
            groupBox1.Controls.Add(textBoxNoise);
            groupBox1.Controls.Add(textBoxHalfPeakWidth);
            groupBox1.Controls.Add(label11);
            groupBox1.Controls.Add(label10);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label8);
            groupBox1.Font = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point);
            groupBox1.Location = new Point(1, 88);
            groupBox1.Margin = new Padding(4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4);
            groupBox1.Size = new Size(321, 178);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "积分参数";
            // 
            // textBoxMinArea
            // 
            textBoxMinArea.Location = new Point(166, 128);
            textBoxMinArea.Margin = new Padding(4);
            textBoxMinArea.Name = "textBoxMinArea";
            textBoxMinArea.Size = new Size(128, 21);
            textBoxMinArea.TabIndex = 3;
            textBoxMinArea.Text = "0.000";
            // 
            // textBoxMinHight
            // 
            textBoxMinHight.Location = new Point(166, 96);
            textBoxMinHight.Margin = new Padding(4);
            textBoxMinHight.Name = "textBoxMinHight";
            textBoxMinHight.Size = new Size(128, 21);
            textBoxMinHight.TabIndex = 2;
            textBoxMinHight.Text = "0.000";
            // 
            // textBoxNoise
            // 
            textBoxNoise.Location = new Point(166, 28);
            textBoxNoise.Margin = new Padding(4);
            textBoxNoise.Name = "textBoxNoise";
            textBoxNoise.Size = new Size(128, 21);
            textBoxNoise.TabIndex = 0;
            textBoxNoise.Text = "100";
            // 
            // textBoxHalfPeakWidth
            // 
            textBoxHalfPeakWidth.Location = new Point(166, 62);
            textBoxHalfPeakWidth.Margin = new Padding(4);
            textBoxHalfPeakWidth.Name = "textBoxHalfPeakWidth";
            textBoxHalfPeakWidth.Size = new Size(128, 21);
            textBoxHalfPeakWidth.TabIndex = 0;
            textBoxHalfPeakWidth.Text = "2.000";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(15, 128);
            label11.Margin = new Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new Size(107, 12);
            label11.TabIndex = 0;
            label11.Text = "最小峰面积(mV*nm)";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(15, 98);
            label10.Margin = new Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new Size(77, 12);
            label10.TabIndex = 0;
            label10.Text = "最小峰高(mV)";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(15, 30);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(53, 12);
            label2.TabIndex = 0;
            label2.Text = "阈值(mV)";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(15, 64);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(89, 12);
            label8.TabIndex = 0;
            label8.Text = "最小半峰宽(nm)";
            // 
            // btnReIntegration
            // 
            btnReIntegration.Location = new Point(14, 295);
            btnReIntegration.Margin = new Padding(4);
            btnReIntegration.Name = "btnReIntegration";
            btnReIntegration.Size = new Size(88, 33);
            btnReIntegration.TabIndex = 0;
            btnReIntegration.Text = "重新积分";
            btnReIntegration.UseVisualStyleBackColor = true;
            btnReIntegration.Click += btnReIntegration_Click;
            // 
            // btnOpenFile
            // 
            btnOpenFile.Location = new Point(14, 33);
            btnOpenFile.Margin = new Padding(4);
            btnOpenFile.Name = "btnOpenFile";
            btnOpenFile.Size = new Size(88, 33);
            btnOpenFile.TabIndex = 0;
            btnOpenFile.Text = "打开文件";
            btnOpenFile.UseVisualStyleBackColor = true;
            btnOpenFile.Click += btnOpenFile_Click;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Margin = new Padding(4);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(zedGraphControl1);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(dataGridViewIntegration);
            splitContainer2.Size = new Size(1174, 824);
            splitContainer2.SplitterDistance = 398;
            splitContainer2.SplitterWidth = 6;
            splitContainer2.TabIndex = 0;
            // 
            // zedGraphControl1
            // 
            zedGraphControl1.Dock = DockStyle.Fill;
            zedGraphControl1.Location = new Point(0, 0);
            zedGraphControl1.Margin = new Padding(5, 6, 5, 6);
            zedGraphControl1.Name = "zedGraphControl1";
            zedGraphControl1.ScrollGrace = 0D;
            zedGraphControl1.ScrollMaxX = 0D;
            zedGraphControl1.ScrollMaxY = 0D;
            zedGraphControl1.ScrollMaxY2 = 0D;
            zedGraphControl1.ScrollMinX = 0D;
            zedGraphControl1.ScrollMinY = 0D;
            zedGraphControl1.ScrollMinY2 = 0D;
            zedGraphControl1.Size = new Size(1174, 398);
            zedGraphControl1.TabIndex = 1;
            // 
            // dataGridViewIntegration
            // 
            dataGridViewIntegration.AllowUserToAddRows = false;
            dataGridViewIntegration.AllowUserToDeleteRows = false;
            dataGridViewIntegration.AllowUserToResizeRows = false;
            dataGridViewIntegration.BackgroundColor = Color.WhiteSmoke;
            dataGridViewIntegration.BorderStyle = BorderStyle.Fixed3D;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = SystemColors.ControlLight;
            dataGridViewCellStyle1.Font = new Font("宋体", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dataGridViewIntegration.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridViewIntegration.ColumnHeadersHeight = 21;
            dataGridViewIntegration.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2, Column3, integralColumnAreaPercent, Column4, Column5, Column6, Column7, Column8, Column9, Column10, Column11, Column39, Column40, Column41, Column37, Column42, Column43, Column44, Column12, Column13, Column14, Column15, Column16, Column17 });
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle4.BackColor = SystemColors.Window;
            dataGridViewCellStyle4.Font = new Font("宋体", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle4.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
            dataGridViewIntegration.DefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewIntegration.Dock = DockStyle.Fill;
            dataGridViewIntegration.EnableHeadersVisualStyles = false;
            dataGridViewIntegration.GridColor = SystemColors.ControlDarkDark;
            dataGridViewIntegration.Location = new Point(0, 0);
            dataGridViewIntegration.Margin = new Padding(4);
            dataGridViewIntegration.Name = "dataGridViewIntegration";
            dataGridViewIntegration.ReadOnly = true;
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = SystemColors.ControlLight;
            dataGridViewCellStyle5.Font = new Font("宋体", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle5.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = DataGridViewTriState.True;
            dataGridViewIntegration.RowHeadersDefaultCellStyle = dataGridViewCellStyle5;
            dataGridViewIntegration.RowTemplate.Height = 23;
            dataGridViewIntegration.Size = new Size(1174, 420);
            dataGridViewIntegration.TabIndex = 2;
            dataGridViewIntegration.TabStop = false;
            // 
            // Column1
            // 
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Column1.DefaultCellStyle = dataGridViewCellStyle2;
            Column1.Frozen = true;
            Column1.HeaderText = "编号";
            Column1.Name = "Column1";
            Column1.ReadOnly = true;
            Column1.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column1.Width = 49;
            // 
            // Column2
            // 
            Column2.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column2.HeaderText = "保留时间(nm)";
            Column2.Name = "Column2";
            Column2.ReadOnly = true;
            Column2.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column2.Width = 93;
            // 
            // Column3
            // 
            Column3.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column3.HeaderText = "峰面积(mV*nm)";
            Column3.Name = "Column3";
            Column3.ReadOnly = true;
            Column3.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column3.Width = 101;
            // 
            // integralColumnAreaPercent
            // 
            integralColumnAreaPercent.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            integralColumnAreaPercent.HeaderText = "峰面积百分比";
            integralColumnAreaPercent.Name = "integralColumnAreaPercent";
            integralColumnAreaPercent.ReadOnly = true;
            integralColumnAreaPercent.SortMode = DataGridViewColumnSortMode.NotSortable;
            integralColumnAreaPercent.Width = 91;
            // 
            // Column4
            // 
            Column4.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column4.HeaderText = "峰高(mV)";
            Column4.Name = "Column4";
            Column4.ReadOnly = true;
            Column4.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column4.Width = 67;
            // 
            // Column5
            // 
            Column5.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column5.HeaderText = "不对称性";
            Column5.Name = "Column5";
            Column5.ReadOnly = true;
            Column5.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column5.Width = 65;
            // 
            // Column6
            // 
            Column6.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column6.HeaderText = "拖尾因子";
            Column6.Name = "Column6";
            Column6.ReadOnly = true;
            Column6.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column6.Visible = false;
            // 
            // Column7
            // 
            Column7.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column7.HeaderText = "切线峰宽(nm)";
            Column7.Name = "Column7";
            Column7.ReadOnly = true;
            Column7.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column7.Visible = false;
            // 
            // Column8
            // 
            Column8.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column8.HeaderText = "半峰宽(nm)";
            Column8.Name = "Column8";
            Column8.ReadOnly = true;
            Column8.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column8.Width = 80;
            // 
            // Column9
            // 
            Column9.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column9.HeaderText = "10％峰宽(nm)";
            Column9.Name = "Column9";
            Column9.ReadOnly = true;
            Column9.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column9.Visible = false;
            // 
            // Column10
            // 
            Column10.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column10.HeaderText = "拖尾峰宽(nm)";
            Column10.Name = "Column10";
            Column10.ReadOnly = true;
            Column10.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column10.Visible = false;
            // 
            // Column11
            // 
            Column11.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column11.HeaderText = "理论塔板数";
            Column11.Name = "Column11";
            Column11.ReadOnly = true;
            Column11.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column11.Width = 78;
            // 
            // Column39
            // 
            Column39.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column39.HeaderText = "10%理论塔板数";
            Column39.Name = "Column39";
            Column39.ReadOnly = true;
            Column39.Visible = false;
            // 
            // Column40
            // 
            Column40.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column40.HeaderText = "拖尾理论塔板数";
            Column40.Name = "Column40";
            Column40.ReadOnly = true;
            Column40.Visible = false;
            // 
            // Column41
            // 
            Column41.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column41.HeaderText = "切线理论塔板数";
            Column41.Name = "Column41";
            Column41.ReadOnly = true;
            Column41.Visible = false;
            // 
            // Column37
            // 
            Column37.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column37.HeaderText = "有效塔板数";
            Column37.Name = "Column37";
            Column37.ReadOnly = true;
            Column37.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column37.Width = 78;
            // 
            // Column42
            // 
            Column42.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column42.HeaderText = "10%有效塔板数";
            Column42.Name = "Column42";
            Column42.ReadOnly = true;
            Column42.Visible = false;
            // 
            // Column43
            // 
            Column43.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column43.HeaderText = "拖尾有效塔板数";
            Column43.Name = "Column43";
            Column43.ReadOnly = true;
            Column43.Visible = false;
            // 
            // Column44
            // 
            Column44.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column44.HeaderText = "切线有效塔板数";
            Column44.Name = "Column44";
            Column44.ReadOnly = true;
            Column44.Visible = false;
            // 
            // Column12
            // 
            Column12.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column12.HeaderText = "容量因子";
            Column12.Name = "Column12";
            Column12.ReadOnly = true;
            Column12.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column12.Width = 65;
            // 
            // Column13
            // 
            Column13.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column13.HeaderText = "选择性因子";
            Column13.Name = "Column13";
            Column13.ReadOnly = true;
            Column13.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column13.Width = 78;
            // 
            // Column14
            // 
            Column14.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column14.HeaderText = "分离度";
            Column14.Name = "Column14";
            Column14.ReadOnly = true;
            Column14.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column14.Width = 52;
            // 
            // Column15
            // 
            Column15.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Column15.DefaultCellStyle = dataGridViewCellStyle3;
            Column15.HeaderText = "峰类型";
            Column15.Name = "Column15";
            Column15.ReadOnly = true;
            Column15.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column15.Width = 52;
            // 
            // Column16
            // 
            Column16.HeaderText = "起点斜率";
            Column16.Name = "Column16";
            Column16.ReadOnly = true;
            Column16.Width = 150;
            // 
            // Column17
            // 
            Column17.HeaderText = "终点斜率";
            Column17.Name = "Column17";
            Column17.ReadOnly = true;
            Column17.Width = 150;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1507, 824);
            Controls.Add(splitContainer1);
            Name = "Form1";
            Text = "Form1";
            WindowState = FormWindowState.Maximized;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewIntegration).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private Label label1;
        private GroupBox groupBox1;
        private TextBox textBoxMinArea;
        private TextBox textBoxMinHight;
        private TextBox textBoxHalfPeakWidth;
        private Label label11;
        private Label label10;
        private Label label8;
        private Button btnReIntegration;
        private Button btnOpenFile;
        private SplitContainer splitContainer2;
        private DataGridView dataGridViewIntegration;
        public ZedGraph.ZedGraphControl zedGraphControl1;
        private TextBox textBoxNoise;
        private Label label2;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Column3;
        private DataGridViewTextBoxColumn integralColumnAreaPercent;
        private DataGridViewTextBoxColumn Column4;
        private DataGridViewTextBoxColumn Column5;
        private DataGridViewTextBoxColumn Column6;
        private DataGridViewTextBoxColumn Column7;
        private DataGridViewTextBoxColumn Column8;
        private DataGridViewTextBoxColumn Column9;
        private DataGridViewTextBoxColumn Column10;
        private DataGridViewTextBoxColumn Column11;
        private DataGridViewTextBoxColumn Column39;
        private DataGridViewTextBoxColumn Column40;
        private DataGridViewTextBoxColumn Column41;
        private DataGridViewTextBoxColumn Column37;
        private DataGridViewTextBoxColumn Column42;
        private DataGridViewTextBoxColumn Column43;
        private DataGridViewTextBoxColumn Column44;
        private DataGridViewTextBoxColumn Column12;
        private DataGridViewTextBoxColumn Column13;
        private DataGridViewTextBoxColumn Column14;
        private DataGridViewTextBoxColumn Column15;
        private DataGridViewTextBoxColumn Column16;
        private DataGridViewTextBoxColumn Column17;
        private Button btnDeleteAllEvents;
        private Button btnCancelPeak;
        private Button btnAddPeak;
    }
}