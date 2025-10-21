namespace ResidenceSync.UI
{
    partial class RSPanel
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.tableLayoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.groupSectionKey = new System.Windows.Forms.GroupBox();
            this.tableSection = new System.Windows.Forms.TableLayoutPanel();
            this.labelSec = new System.Windows.Forms.Label();
            this.txtSec = new System.Windows.Forms.TextBox();
            this.labelTwp = new System.Windows.Forms.Label();
            this.txtTwp = new System.Windows.Forms.TextBox();
            this.labelRge = new System.Windows.Forms.Label();
            this.txtRge = new System.Windows.Forms.TextBox();
            this.labelMer = new System.Windows.Forms.Label();
            this.txtMer = new System.Windows.Forms.TextBox();
            this.groupSurfDev = new System.Windows.Forms.GroupBox();
            this.tableSurfDev = new System.Windows.Forms.TableLayoutPanel();
            this.groupScale = new System.Windows.Forms.GroupBox();
            this.tableScale = new System.Windows.Forms.TableLayoutPanel();
            this.radioScale50 = new System.Windows.Forms.RadioButton();
            this.radioScale25 = new System.Windows.Forms.RadioButton();
            this.radioScale20 = new System.Windows.Forms.RadioButton();
            this.groupSurveyed = new System.Windows.Forms.GroupBox();
            this.tableSurveyed = new System.Windows.Forms.TableLayoutPanel();
            this.radioSurveyed = new System.Windows.Forms.RadioButton();
            this.radioUnsurveyed = new System.Windows.Forms.RadioButton();
            this.checkInsertResidences = new System.Windows.Forms.CheckBox();
            this.groupSurfaceSize = new System.Windows.Forms.GroupBox();
            this.tableSurfaceSize = new System.Windows.Forms.TableLayoutPanel();
            this.radioSize3 = new System.Windows.Forms.RadioButton();
            this.radioSize5 = new System.Windows.Forms.RadioButton();
            this.radioSize7 = new System.Windows.Forms.RadioButton();
            this.radioSize9 = new System.Windows.Forms.RadioButton();
            this.flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnBuildSection = new System.Windows.Forms.Button();
            this.btnPushResidences = new System.Windows.Forms.Button();
            this.btnBuildSurface = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.tableLayoutMain.SuspendLayout();
            this.groupSectionKey.SuspendLayout();
            this.tableSection.SuspendLayout();
            this.groupSurfDev.SuspendLayout();
            this.tableSurfDev.SuspendLayout();
            this.groupScale.SuspendLayout();
            this.tableScale.SuspendLayout();
            this.groupSurveyed.SuspendLayout();
            this.tableSurveyed.SuspendLayout();
            this.groupSurfaceSize.SuspendLayout();
            this.tableSurfaceSize.SuspendLayout();
            this.flowButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutMain
            // 
            this.tableLayoutMain.ColumnCount = 1;
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.Controls.Add(this.groupSectionKey, 0, 0);
            this.tableLayoutMain.Controls.Add(this.groupSurfDev, 0, 1);
            this.tableLayoutMain.Controls.Add(this.flowButtons, 0, 2);
            this.tableLayoutMain.Controls.Add(this.lblStatus, 0, 3);
            this.tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.RowCount = 4;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutMain.Size = new System.Drawing.Size(340, 460);
            this.tableLayoutMain.TabIndex = 0;
            // 
            // groupSectionKey
            // 
            this.groupSectionKey.AutoSize = true;
            this.groupSectionKey.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupSectionKey.Controls.Add(this.tableSection);
            this.groupSectionKey.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupSectionKey.Location = new System.Drawing.Point(3, 3);
            this.groupSectionKey.Name = "groupSectionKey";
            this.groupSectionKey.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            this.groupSectionKey.Size = new System.Drawing.Size(334, 146);
            this.groupSectionKey.TabIndex = 0;
            this.groupSectionKey.TabStop = false;
            this.groupSectionKey.Text = "Section Key";
            // 
            // tableSection
            // 
            this.tableSection.ColumnCount = 2;
            this.tableSection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableSection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableSection.Controls.Add(this.labelSec, 0, 0);
            this.tableSection.Controls.Add(this.txtSec, 1, 0);
            this.tableSection.Controls.Add(this.labelTwp, 0, 1);
            this.tableSection.Controls.Add(this.txtTwp, 1, 1);
            this.tableSection.Controls.Add(this.labelRge, 0, 2);
            this.tableSection.Controls.Add(this.txtRge, 1, 2);
            this.tableSection.Controls.Add(this.labelMer, 0, 3);
            this.tableSection.Controls.Add(this.txtMer, 1, 3);
            this.tableSection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableSection.Location = new System.Drawing.Point(8, 19);
            this.tableSection.Name = "tableSection";
            this.tableSection.RowCount = 4;
            this.tableSection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableSection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableSection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableSection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableSection.Size = new System.Drawing.Size(318, 119);
            this.tableSection.TabIndex = 0;
            // 
            // labelSec
            // 
            this.labelSec.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSec.AutoSize = true;
            this.labelSec.Location = new System.Drawing.Point(3, 6);
            this.labelSec.Name = "labelSec";
            this.labelSec.Size = new System.Drawing.Size(26, 17);
            this.labelSec.TabIndex = 0;
            this.labelSec.Text = "SEC";
            // 
            // txtSec
            // 
            this.txtSec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSec.Location = new System.Drawing.Point(63, 3);
            this.txtSec.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.txtSec.Name = "txtSec";
            this.txtSec.Size = new System.Drawing.Size(252, 23);
            this.txtSec.TabIndex = 1;
            // 
            // labelTwp
            // 
            this.labelTwp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelTwp.AutoSize = true;
            this.labelTwp.Location = new System.Drawing.Point(3, 34);
            this.labelTwp.Name = "labelTwp";
            this.labelTwp.Size = new System.Drawing.Size(34, 17);
            this.labelTwp.TabIndex = 2;
            this.labelTwp.Text = "TWP";
            // 
            // txtTwp
            // 
            this.txtTwp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTwp.Location = new System.Drawing.Point(63, 31);
            this.txtTwp.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.txtTwp.Name = "txtTwp";
            this.txtTwp.Size = new System.Drawing.Size(252, 23);
            this.txtTwp.TabIndex = 3;
            // 
            // labelRge
            // 
            this.labelRge.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelRge.AutoSize = true;
            this.labelRge.Location = new System.Drawing.Point(3, 62);
            this.labelRge.Name = "labelRge";
            this.labelRge.Size = new System.Drawing.Size(34, 17);
            this.labelRge.TabIndex = 4;
            this.labelRge.Text = "RGE";
            // 
            // txtRge
            // 
            this.txtRge.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRge.Location = new System.Drawing.Point(63, 59);
            this.txtRge.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.txtRge.Name = "txtRge";
            this.txtRge.Size = new System.Drawing.Size(252, 23);
            this.txtRge.TabIndex = 5;
            // 
            // labelMer
            // 
            this.labelMer.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelMer.AutoSize = true;
            this.labelMer.Location = new System.Drawing.Point(3, 90);
            this.labelMer.Name = "labelMer";
            this.labelMer.Size = new System.Drawing.Size(34, 17);
            this.labelMer.TabIndex = 6;
            this.labelMer.Text = "MER";
            // 
            // txtMer
            // 
            this.txtMer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMer.Location = new System.Drawing.Point(63, 87);
            this.txtMer.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.txtMer.Name = "txtMer";
            this.txtMer.Size = new System.Drawing.Size(252, 23);
            this.txtMer.TabIndex = 7;
            // 
            // groupSurfDev
            // 
            this.groupSurfDev.AutoSize = true;
            this.groupSurfDev.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupSurfDev.Controls.Add(this.tableSurfDev);
            this.groupSurfDev.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupSurfDev.Location = new System.Drawing.Point(3, 155);
            this.groupSurfDev.Name = "groupSurfDev";
            this.groupSurfDev.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            this.groupSurfDev.Size = new System.Drawing.Size(334, 213);
            this.groupSurfDev.TabIndex = 1;
            this.groupSurfDev.TabStop = false;
            this.groupSurfDev.Text = "SURFDEV Options";
            // 
            // tableSurfDev
            // 
            this.tableSurfDev.ColumnCount = 1;
            this.tableSurfDev.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableSurfDev.Controls.Add(this.groupSurfaceSize, 0, 0);
            this.tableSurfDev.Controls.Add(this.groupScale, 0, 1);
            this.tableSurfDev.Controls.Add(this.groupSurveyed, 0, 2);
            this.tableSurfDev.Controls.Add(this.checkInsertResidences, 0, 3);
            this.tableSurfDev.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableSurfDev.Location = new System.Drawing.Point(8, 19);
            this.tableSurfDev.Name = "tableSurfDev";
            this.tableSurfDev.RowCount = 4;
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableSurfDev.Size = new System.Drawing.Size(318, 186);
            this.tableSurfDev.TabIndex = 0;
            // 
            // groupScale
            // 
            this.groupScale.AutoSize = true;
            this.groupScale.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupScale.Controls.Add(this.tableScale);
            this.groupScale.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupScale.Location = new System.Drawing.Point(3, 75);
            this.groupScale.Name = "groupScale";
            this.groupScale.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            this.groupScale.Size = new System.Drawing.Size(312, 83);
            this.groupScale.TabIndex = 1;
            this.groupScale.TabStop = false;
            this.groupScale.Text = "Scale";
            // 
            // tableScale
            // 
            this.tableScale.ColumnCount = 1;
            this.tableScale.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableScale.Controls.Add(this.radioScale50, 0, 0);
            this.tableScale.Controls.Add(this.radioScale25, 0, 1);
            this.tableScale.Controls.Add(this.radioScale20, 0, 2);
            this.tableScale.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableScale.Location = new System.Drawing.Point(8, 19);
            this.tableScale.Name = "tableScale";
            this.tableScale.RowCount = 3;
            this.tableScale.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableScale.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableScale.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableScale.Size = new System.Drawing.Size(296, 56);
            this.tableScale.TabIndex = 0;
            // 
            // radioScale50
            // 
            this.radioScale50.AutoSize = true;
            this.radioScale50.Location = new System.Drawing.Point(3, 3);
            this.radioScale50.Name = "radioScale50";
            this.radioScale50.Size = new System.Drawing.Size(53, 19);
            this.radioScale50.TabIndex = 0;
            this.radioScale50.TabStop = true;
            this.radioScale50.Text = "50k";
            this.radioScale50.UseVisualStyleBackColor = true;
            // 
            // radioScale25
            // 
            this.radioScale25.AutoSize = true;
            this.radioScale25.Location = new System.Drawing.Point(3, 28);
            this.radioScale25.Name = "radioScale25";
            this.radioScale25.Size = new System.Drawing.Size(53, 19);
            this.radioScale25.TabIndex = 1;
            this.radioScale25.Text = "25k";
            this.radioScale25.UseVisualStyleBackColor = true;
            // 
            // radioScale20
            // 
            this.radioScale20.AutoSize = true;
            this.radioScale20.Location = new System.Drawing.Point(3, 53);
            this.radioScale20.Name = "radioScale20";
            this.radioScale20.Size = new System.Drawing.Size(53, 19);
            this.radioScale20.TabIndex = 2;
            this.radioScale20.Text = "20k";
            this.radioScale20.UseVisualStyleBackColor = true;
            // 
            // groupSurveyed
            // 
            this.groupSurveyed.AutoSize = true;
            this.groupSurveyed.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupSurveyed.Controls.Add(this.tableSurveyed);
            this.groupSurveyed.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupSurveyed.Location = new System.Drawing.Point(3, 164);
            this.groupSurveyed.Name = "groupSurveyed";
            this.groupSurveyed.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            this.groupSurveyed.Size = new System.Drawing.Size(312, 70);
            this.groupSurveyed.TabIndex = 2;
            this.groupSurveyed.TabStop = false;
            this.groupSurveyed.Text = "Surveyed?";
            // 
            // tableSurveyed
            // 
            this.tableSurveyed.ColumnCount = 1;
            this.tableSurveyed.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableSurveyed.Controls.Add(this.radioSurveyed, 0, 0);
            this.tableSurveyed.Controls.Add(this.radioUnsurveyed, 0, 1);
            this.tableSurveyed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableSurveyed.Location = new System.Drawing.Point(8, 19);
            this.tableSurveyed.Name = "tableSurveyed";
            this.tableSurveyed.RowCount = 2;
            this.tableSurveyed.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableSurveyed.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableSurveyed.Size = new System.Drawing.Size(296, 43);
            this.tableSurveyed.TabIndex = 0;
            // 
            // radioSurveyed
            // 
            this.radioSurveyed.AutoSize = true;
            this.radioSurveyed.Location = new System.Drawing.Point(3, 3);
            this.radioSurveyed.Name = "radioSurveyed";
            this.radioSurveyed.Size = new System.Drawing.Size(77, 19);
            this.radioSurveyed.TabIndex = 0;
            this.radioSurveyed.Text = "Surveyed";
            this.radioSurveyed.UseVisualStyleBackColor = true;
            // 
            // radioUnsurveyed
            // 
            this.radioUnsurveyed.AutoSize = true;
            this.radioUnsurveyed.Location = new System.Drawing.Point(3, 28);
            this.radioUnsurveyed.Name = "radioUnsurveyed";
            this.radioUnsurveyed.Size = new System.Drawing.Size(96, 19);
            this.radioUnsurveyed.TabIndex = 1;
            this.radioUnsurveyed.Text = "Unsurveyed";
            this.radioUnsurveyed.UseVisualStyleBackColor = true;
            // 
            // checkInsertResidences
            // 
            this.checkInsertResidences.AutoSize = true;
            this.checkInsertResidences.Dock = System.Windows.Forms.DockStyle.Top;
            this.checkInsertResidences.Location = new System.Drawing.Point(3, 240);
            this.checkInsertResidences.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.checkInsertResidences.Name = "checkInsertResidences";
            this.checkInsertResidences.Size = new System.Drawing.Size(312, 19);
            this.checkInsertResidences.TabIndex = 3;
            this.checkInsertResidences.Text = "Insert residences?";
            this.checkInsertResidences.ThreeState = true;
            this.checkInsertResidences.UseVisualStyleBackColor = true;
            // 
            // groupSurfaceSize
            // 
            this.groupSurfaceSize.AutoSize = true;
            this.groupSurfaceSize.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupSurfaceSize.Controls.Add(this.tableSurfaceSize);
            this.groupSurfaceSize.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupSurfaceSize.Location = new System.Drawing.Point(3, 3);
            this.groupSurfaceSize.Name = "groupSurfaceSize";
            this.groupSurfaceSize.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            this.groupSurfaceSize.Size = new System.Drawing.Size(312, 66);
            this.groupSurfaceSize.TabIndex = 0;
            this.groupSurfaceSize.TabStop = false;
            this.groupSurfaceSize.Text = "Surface size";
            // 
            // tableSurfaceSize
            // 
            this.tableSurfaceSize.ColumnCount = 4;
            this.tableSurfaceSize.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableSurfaceSize.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableSurfaceSize.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableSurfaceSize.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableSurfaceSize.Controls.Add(this.radioSize3, 0, 0);
            this.tableSurfaceSize.Controls.Add(this.radioSize5, 1, 0);
            this.tableSurfaceSize.Controls.Add(this.radioSize7, 2, 0);
            this.tableSurfaceSize.Controls.Add(this.radioSize9, 3, 0);
            this.tableSurfaceSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableSurfaceSize.Location = new System.Drawing.Point(8, 19);
            this.tableSurfaceSize.Name = "tableSurfaceSize";
            this.tableSurfaceSize.RowCount = 1;
            this.tableSurfaceSize.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableSurfaceSize.Size = new System.Drawing.Size(296, 39);
            this.tableSurfaceSize.TabIndex = 0;
            // 
            // radioSize3
            // 
            this.radioSize3.AutoSize = true;
            this.radioSize3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioSize3.Location = new System.Drawing.Point(3, 3);
            this.radioSize3.Name = "radioSize3";
            this.radioSize3.Size = new System.Drawing.Size(68, 33);
            this.radioSize3.TabIndex = 0;
            this.radioSize3.Text = "3x3";
            this.radioSize3.UseVisualStyleBackColor = true;
            // 
            // radioSize5
            // 
            this.radioSize5.AutoSize = true;
            this.radioSize5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioSize5.Location = new System.Drawing.Point(77, 3);
            this.radioSize5.Name = "radioSize5";
            this.radioSize5.Size = new System.Drawing.Size(68, 33);
            this.radioSize5.TabIndex = 1;
            this.radioSize5.TabStop = true;
            this.radioSize5.Text = "5x5";
            this.radioSize5.UseVisualStyleBackColor = true;
            // 
            // radioSize7
            // 
            this.radioSize7.AutoSize = true;
            this.radioSize7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioSize7.Location = new System.Drawing.Point(151, 3);
            this.radioSize7.Name = "radioSize7";
            this.radioSize7.Size = new System.Drawing.Size(68, 33);
            this.radioSize7.TabIndex = 2;
            this.radioSize7.Text = "7x7";
            this.radioSize7.UseVisualStyleBackColor = true;
            // 
            // radioSize9
            // 
            this.radioSize9.AutoSize = true;
            this.radioSize9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioSize9.Location = new System.Drawing.Point(225, 3);
            this.radioSize9.Name = "radioSize9";
            this.radioSize9.Size = new System.Drawing.Size(68, 33);
            this.radioSize9.TabIndex = 3;
            this.radioSize9.Text = "9x9";
            this.radioSize9.UseVisualStyleBackColor = true;
            // 
            // flowButtons
            // 
            this.flowButtons.AutoSize = true;
            this.flowButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowButtons.Controls.Add(this.btnBuildSection);
            this.flowButtons.Controls.Add(this.btnPushResidences);
            this.flowButtons.Controls.Add(this.btnBuildSurface);
            this.flowButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flowButtons.Location = new System.Drawing.Point(3, 374);
            this.flowButtons.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.flowButtons.Name = "flowButtons";
            this.flowButtons.Size = new System.Drawing.Size(334, 33);
            this.flowButtons.TabIndex = 2;
            this.flowButtons.WrapContents = false;
            // 
            // btnBuildSection
            // 
            this.btnBuildSection.AutoSize = true;
            this.btnBuildSection.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnBuildSection.Location = new System.Drawing.Point(3, 3);
            this.btnBuildSection.Name = "btnBuildSection";
            this.btnBuildSection.Size = new System.Drawing.Size(99, 27);
            this.btnBuildSection.TabIndex = 0;
            this.btnBuildSection.Text = "Build Section";
            this.btnBuildSection.UseVisualStyleBackColor = true;
            this.btnBuildSection.Click += new System.EventHandler(this.btnBuildSection_Click);
            // 
            // btnPushResidences
            // 
            this.btnPushResidences.AutoSize = true;
            this.btnPushResidences.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnPushResidences.Location = new System.Drawing.Point(108, 3);
            this.btnPushResidences.Name = "btnPushResidences";
            this.btnPushResidences.Size = new System.Drawing.Size(120, 27);
            this.btnPushResidences.TabIndex = 1;
            this.btnPushResidences.Text = "Push Residences";
            this.btnPushResidences.UseVisualStyleBackColor = true;
            this.btnPushResidences.Click += new System.EventHandler(this.btnPushResidences_Click);
            // 
            // btnBuildSurface
            // 
            this.btnBuildSurface.AutoSize = true;
            this.btnBuildSurface.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnBuildSurface.Location = new System.Drawing.Point(234, 3);
            this.btnBuildSurface.Name = "btnBuildSurface";
            this.btnBuildSurface.Size = new System.Drawing.Size(97, 27);
            this.btnBuildSurface.TabIndex = 2;
            this.btnBuildSurface.Text = "Build Surface";
            this.btnBuildSurface.UseVisualStyleBackColor = true;
            this.btnBuildSurface.Click += new System.EventHandler(this.btnBuildSurface_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Location = new System.Drawing.Point(3, 407);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(334, 50);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Ready";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // RSPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutMain);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(320, 360);
            this.Name = "RSPanel";
            this.Size = new System.Drawing.Size(340, 460);
            this.tableLayoutMain.ResumeLayout(false);
            this.tableLayoutMain.PerformLayout();
            this.groupSectionKey.ResumeLayout(false);
            this.tableSection.ResumeLayout(false);
            this.tableSection.PerformLayout();
            this.groupSurfDev.ResumeLayout(false);
            this.tableSurfDev.ResumeLayout(false);
            this.tableSurfDev.PerformLayout();
            this.groupScale.ResumeLayout(false);
            this.tableScale.ResumeLayout(false);
            this.tableScale.PerformLayout();
            this.groupSurveyed.ResumeLayout(false);
            this.tableSurveyed.ResumeLayout(false);
            this.tableSurveyed.PerformLayout();
            this.groupSurfaceSize.ResumeLayout(false);
            this.tableSurfaceSize.ResumeLayout(false);
            this.tableSurfaceSize.PerformLayout();
            this.flowButtons.ResumeLayout(false);
            this.flowButtons.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutMain;
        private System.Windows.Forms.GroupBox groupSectionKey;
        private System.Windows.Forms.TableLayoutPanel tableSection;
        private System.Windows.Forms.Label labelSec;
        private System.Windows.Forms.TextBox txtSec;
        private System.Windows.Forms.Label labelTwp;
        private System.Windows.Forms.TextBox txtTwp;
        private System.Windows.Forms.Label labelRge;
        private System.Windows.Forms.TextBox txtRge;
        private System.Windows.Forms.Label labelMer;
        private System.Windows.Forms.TextBox txtMer;
        private System.Windows.Forms.GroupBox groupSurfDev;
        private System.Windows.Forms.TableLayoutPanel tableSurfDev;
        private System.Windows.Forms.GroupBox groupScale;
        private System.Windows.Forms.TableLayoutPanel tableScale;
        private System.Windows.Forms.RadioButton radioScale50;
        private System.Windows.Forms.RadioButton radioScale25;
        private System.Windows.Forms.RadioButton radioScale20;
        private System.Windows.Forms.GroupBox groupSurveyed;
        private System.Windows.Forms.TableLayoutPanel tableSurveyed;
        private System.Windows.Forms.RadioButton radioSurveyed;
        private System.Windows.Forms.RadioButton radioUnsurveyed;
        private System.Windows.Forms.CheckBox checkInsertResidences;
        private System.Windows.Forms.GroupBox groupSurfaceSize;
        private System.Windows.Forms.TableLayoutPanel tableSurfaceSize;
        private System.Windows.Forms.RadioButton radioSize3;
        private System.Windows.Forms.RadioButton radioSize5;
        private System.Windows.Forms.RadioButton radioSize7;
        private System.Windows.Forms.RadioButton radioSize9;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button btnBuildSection;
        private System.Windows.Forms.Button btnPushResidences;
        private System.Windows.Forms.Button btnBuildSurface;
        private System.Windows.Forms.Label lblStatus;
    }
}
