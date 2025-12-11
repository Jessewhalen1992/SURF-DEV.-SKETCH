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
            this.groupSurfDev = new System.Windows.Forms.GroupBox();
            this.tableSurfDev = new System.Windows.Forms.TableLayoutPanel();
            this.labelZone = new System.Windows.Forms.Label();
            this.comboZone = new System.Windows.Forms.ComboBox();
            this.labelSection = new System.Windows.Forms.Label();
            this.textSection = new System.Windows.Forms.TextBox();
            this.labelTownship = new System.Windows.Forms.Label();
            this.textTownship = new System.Windows.Forms.TextBox();
            this.labelRange = new System.Windows.Forms.Label();
            this.textRange = new System.Windows.Forms.TextBox();
            this.labelMeridian = new System.Windows.Forms.Label();
            this.textMeridian = new System.Windows.Forms.TextBox();
            this.labelGridSize = new System.Windows.Forms.Label();
            this.comboGridSize = new System.Windows.Forms.ComboBox();
            this.labelScale = new System.Windows.Forms.Label();
            this.comboScale = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboSurveyed = new System.Windows.Forms.ComboBox();
            this.labelInsertRes = new System.Windows.Forms.Label();
            this.comboInsertResidences = new System.Windows.Forms.ComboBox();
            this.tableButtons = new System.Windows.Forms.TableLayoutPanel();
            this.tablePrimaryButtons = new System.Windows.Forms.TableLayoutPanel();
            this.btnBuildSection = new System.Windows.Forms.Button();
            this.btnPushResidences = new System.Windows.Forms.Button();
            this.btnBuildSurface = new System.Windows.Forms.Button();
            this.tableQuickButtons = new System.Windows.Forms.TableLayoutPanel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.tableLayoutMain.SuspendLayout();
            this.groupSurfDev.SuspendLayout();
            this.tableSurfDev.SuspendLayout();
            this.tableButtons.SuspendLayout();
            this.tablePrimaryButtons.SuspendLayout();
            this.tableQuickButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // tableLayoutMain
            //
            this.tableLayoutMain.ColumnCount = 1;
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.Controls.Add(this.groupSurfDev, 0, 0);
            this.tableLayoutMain.Controls.Add(this.tableButtons, 0, 1);
            this.tableLayoutMain.Controls.Add(this.lblStatus, 0, 2);
            this.tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.RowCount = 3;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.Size = new System.Drawing.Size(320, 400);
            this.tableLayoutMain.TabIndex = 0;
            // 
            // groupSurfDev
            // 
            this.groupSurfDev.Controls.Add(this.tableSurfDev);
            this.groupSurfDev.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupSurfDev.Location = new System.Drawing.Point(3, 3);
            this.groupSurfDev.MinimumSize = new System.Drawing.Size(320, 220);
            this.groupSurfDev.Name = "groupSurfDev";
            this.groupSurfDev.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            this.groupSurfDev.Size = new System.Drawing.Size(320, 319);
            this.groupSurfDev.TabIndex = 0;
            this.groupSurfDev.TabStop = false;
            this.groupSurfDev.Text = "SURFDEV Options";
            // 
            // tableSurfDev
            // 
            this.tableSurfDev.ColumnCount = 2;
            this.tableSurfDev.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tableSurfDev.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableSurfDev.Controls.Add(this.labelZone, 0, 0);
            this.tableSurfDev.Controls.Add(this.comboZone, 1, 0);
            this.tableSurfDev.Controls.Add(this.labelSection, 0, 1);
            this.tableSurfDev.Controls.Add(this.textSection, 1, 1);
            this.tableSurfDev.Controls.Add(this.labelTownship, 0, 2);
            this.tableSurfDev.Controls.Add(this.textTownship, 1, 2);
            this.tableSurfDev.Controls.Add(this.labelRange, 0, 3);
            this.tableSurfDev.Controls.Add(this.textRange, 1, 3);
            this.tableSurfDev.Controls.Add(this.labelMeridian, 0, 4);
            this.tableSurfDev.Controls.Add(this.textMeridian, 1, 4);
            this.tableSurfDev.Controls.Add(this.labelGridSize, 0, 5);
            this.tableSurfDev.Controls.Add(this.comboGridSize, 1, 5);
            this.tableSurfDev.Controls.Add(this.labelScale, 0, 6);
            this.tableSurfDev.Controls.Add(this.comboScale, 1, 6);
            this.tableSurfDev.Controls.Add(this.label1, 0, 7);
            this.tableSurfDev.Controls.Add(this.comboSurveyed, 1, 7);
            this.tableSurfDev.Controls.Add(this.labelInsertRes, 0, 8);
            this.tableSurfDev.Controls.Add(this.comboInsertResidences, 1, 8);
            this.tableSurfDev.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableSurfDev.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableSurfDev.Location = new System.Drawing.Point(8, 30);
            this.tableSurfDev.Name = "tableSurfDev";
            this.tableSurfDev.RowCount = 9;
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableSurfDev.Size = new System.Drawing.Size(304, 281);
            this.tableSurfDev.TabIndex = 0;
            this.tableSurfDev.Paint += new System.Windows.Forms.PaintEventHandler(this.tableSurfDev_Paint);
            // 
            // labelZone
            //
            this.labelZone.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelZone.AutoSize = true;
            this.labelZone.Location = new System.Drawing.Point(3, 5);
            this.labelZone.Name = "labelZone";
            this.labelZone.Size = new System.Drawing.Size(54, 25);
            this.labelZone.TabIndex = 0;
            this.labelZone.Text = "Zone";
            //
            // comboZone
            //
            this.comboZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboZone.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboZone.FormattingEnabled = true;
            this.comboZone.Location = new System.Drawing.Point(113, 3);
            this.comboZone.Name = "comboZone";
            this.comboZone.Size = new System.Drawing.Size(188, 33);
            this.comboZone.TabIndex = 1;
            //
            // labelSection
            //
            this.labelSection.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSection.AutoSize = true;
            this.labelSection.Location = new System.Drawing.Point(3, 36);
            this.labelSection.Name = "labelSection";
            this.labelSection.Size = new System.Drawing.Size(70, 25);
            this.labelSection.TabIndex = 2;
            this.labelSection.Text = "Section";
            //
            // textSection
            //
            this.textSection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textSection.Location = new System.Drawing.Point(113, 34);
            this.textSection.Name = "textSection";
            this.textSection.Size = new System.Drawing.Size(188, 31);
            this.textSection.TabIndex = 3;
            //
            // labelTownship
            //
            this.labelTownship.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelTownship.AutoSize = true;
            this.labelTownship.Location = new System.Drawing.Point(3, 67);
            this.labelTownship.Name = "labelTownship";
            this.labelTownship.Size = new System.Drawing.Size(86, 25);
            this.labelTownship.TabIndex = 4;
            this.labelTownship.Text = "Township";
            //
            // textTownship
            //
            this.textTownship.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textTownship.Location = new System.Drawing.Point(113, 65);
            this.textTownship.Name = "textTownship";
            this.textTownship.Size = new System.Drawing.Size(188, 31);
            this.textTownship.TabIndex = 5;
            //
            // labelRange
            //
            this.labelRange.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelRange.AutoSize = true;
            this.labelRange.Location = new System.Drawing.Point(3, 98);
            this.labelRange.Name = "labelRange";
            this.labelRange.Size = new System.Drawing.Size(62, 25);
            this.labelRange.TabIndex = 6;
            this.labelRange.Text = "Range";
            //
            // textRange
            //
            this.textRange.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textRange.Location = new System.Drawing.Point(113, 96);
            this.textRange.Name = "textRange";
            this.textRange.Size = new System.Drawing.Size(188, 31);
            this.textRange.TabIndex = 7;
            //
            // labelMeridian
            //
            this.labelMeridian.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelMeridian.AutoSize = true;
            this.labelMeridian.Location = new System.Drawing.Point(3, 129);
            this.labelMeridian.Name = "labelMeridian";
            this.labelMeridian.Size = new System.Drawing.Size(81, 25);
            this.labelMeridian.TabIndex = 8;
            this.labelMeridian.Text = "Meridian";
            //
            // textMeridian
            //
            this.textMeridian.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textMeridian.Location = new System.Drawing.Point(113, 127);
            this.textMeridian.Name = "textMeridian";
            this.textMeridian.Size = new System.Drawing.Size(188, 31);
            this.textMeridian.TabIndex = 9;
            //
            // labelGridSize
            //
            this.labelGridSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelGridSize.AutoSize = true;
            this.labelGridSize.Location = new System.Drawing.Point(3, 160);
            this.labelGridSize.Name = "labelGridSize";
            this.labelGridSize.Size = new System.Drawing.Size(79, 25);
            this.labelGridSize.TabIndex = 10;
            this.labelGridSize.Text = "Grid size";
            //
            // comboGridSize
            //
            this.comboGridSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboGridSize.Location = new System.Drawing.Point(113, 158);
            this.comboGridSize.Name = "comboGridSize";
            this.comboGridSize.Size = new System.Drawing.Size(188, 33);
            this.comboGridSize.TabIndex = 11;
            //
            // labelScale
            //
            this.labelScale.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelScale.AutoSize = true;
            this.labelScale.Location = new System.Drawing.Point(3, 191);
            this.labelScale.Name = "labelScale";
            this.labelScale.Size = new System.Drawing.Size(55, 25);
            this.labelScale.TabIndex = 12;
            this.labelScale.Text = "Scale";
            //
            // comboScale
            //
            this.comboScale.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboScale.Location = new System.Drawing.Point(113, 189);
            this.comboScale.Name = "comboScale";
            this.comboScale.Size = new System.Drawing.Size(188, 33);
            this.comboScale.TabIndex = 13;
            //
            // label1
            //
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 222);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 25);
            this.label1.TabIndex = 14;
            this.label1.Text = "Surveyed";
            this.label1.Click += new System.EventHandler(this.labelSurveyed_Click);
            //
            // comboSurveyed
            //
            this.comboSurveyed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboSurveyed.Location = new System.Drawing.Point(113, 220);
            this.comboSurveyed.Name = "comboSurveyed";
            this.comboSurveyed.Size = new System.Drawing.Size(188, 33);
            this.comboSurveyed.TabIndex = 15;
            //
            // labelInsertRes
            //
            this.labelInsertRes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelInsertRes.AutoSize = true;
            this.labelInsertRes.Location = new System.Drawing.Point(3, 253);
            this.labelInsertRes.Name = "labelInsertRes";
            this.labelInsertRes.Size = new System.Drawing.Size(147, 25);
            this.labelInsertRes.TabIndex = 16;
            this.labelInsertRes.Text = "Insert Residences";
            //
            // comboInsertResidences
            //
            this.comboInsertResidences.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboInsertResidences.Location = new System.Drawing.Point(113, 251);
            this.comboInsertResidences.Name = "comboInsertResidences";
            this.comboInsertResidences.Size = new System.Drawing.Size(188, 33);
            this.comboInsertResidences.TabIndex = 17;
            // tableButtons
            //
            this.tableButtons.AutoSize = true;
            this.tableButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableButtons.ColumnCount = 1;
            this.tableButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableButtons.Controls.Add(this.tablePrimaryButtons, 0, 0);
            this.tableButtons.Controls.Add(this.tableQuickButtons, 0, 1);
            this.tableButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableButtons.Location = new System.Drawing.Point(3, 328);
            this.tableButtons.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.tableButtons.Name = "tableButtons";
            this.tableButtons.RowCount = 2;
            this.tableButtons.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableButtons.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableButtons.Size = new System.Drawing.Size(314, 88);
            this.tableButtons.TabIndex = 1;
            //
            // tablePrimaryButtons
            //
            this.tablePrimaryButtons.AutoSize = true;
            this.tablePrimaryButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tablePrimaryButtons.ColumnCount = 3;
            this.tablePrimaryButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tablePrimaryButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tablePrimaryButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tablePrimaryButtons.Controls.Add(this.btnBuildSection, 0, 0);
            this.tablePrimaryButtons.Controls.Add(this.btnPushResidences, 1, 0);
            this.tablePrimaryButtons.Controls.Add(this.btnBuildSurface, 2, 0);
            this.tablePrimaryButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.tablePrimaryButtons.Location = new System.Drawing.Point(0, 0);
            this.tablePrimaryButtons.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.tablePrimaryButtons.Name = "tablePrimaryButtons";
            this.tablePrimaryButtons.RowCount = 1;
            this.tablePrimaryButtons.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tablePrimaryButtons.Size = new System.Drawing.Size(314, 41);
            this.tablePrimaryButtons.TabIndex = 0;
            //
            // btnBuildSection
            //
            this.btnBuildSection.AutoSize = true;
            this.btnBuildSection.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnBuildSection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBuildSection.Location = new System.Drawing.Point(3, 3);
            this.btnBuildSection.Name = "btnBuildSection";
            this.btnBuildSection.Size = new System.Drawing.Size(124, 35);
            this.btnBuildSection.TabIndex = 0;
            this.btnBuildSection.Text = "Build Section";
            this.btnBuildSection.UseVisualStyleBackColor = true;
            this.btnBuildSection.Click += new System.EventHandler(this.btnBuildSection_Click);
            //
            // btnPushResidences
            //
            this.btnPushResidences.AutoSize = true;
            this.btnPushResidences.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnPushResidences.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPushResidences.Location = new System.Drawing.Point(107, 3);
            this.btnPushResidences.Name = "btnPushResidences";
            this.btnPushResidences.Size = new System.Drawing.Size(98, 35);
            this.btnPushResidences.TabIndex = 1;
            this.btnPushResidences.Text = "Push Residences";
            this.btnPushResidences.UseVisualStyleBackColor = true;
            this.btnPushResidences.Click += new System.EventHandler(this.btnPushResidences_Click);
            //
            // btnBuildSurface
            //
            this.btnBuildSurface.AutoSize = true;
            this.btnBuildSurface.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnBuildSurface.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBuildSurface.Location = new System.Drawing.Point(211, 3);
            this.btnBuildSurface.Name = "btnBuildSurface";
            this.btnBuildSurface.Size = new System.Drawing.Size(124, 35);
            this.btnBuildSurface.TabIndex = 2;
            this.btnBuildSurface.Text = "Build Surface";
            this.btnBuildSurface.UseVisualStyleBackColor = true;
            this.btnBuildSurface.Click += new System.EventHandler(this.btnBuildSurface_Click);
            //
            // tableQuickButtons
            //
            this.tableQuickButtons.AutoSize = true;
            this.tableQuickButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableQuickButtons.ColumnCount = 4;
            this.tableQuickButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableQuickButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableQuickButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableQuickButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableQuickButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableQuickButtons.Location = new System.Drawing.Point(0, 47);
            this.tableQuickButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tableQuickButtons.Name = "tableQuickButtons";
            this.tableQuickButtons.RowCount = 1;
            this.tableQuickButtons.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableQuickButtons.Size = new System.Drawing.Size(314, 41);
            this.tableQuickButtons.TabIndex = 1;
            //
            // lblStatus
            //
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.AutoSize = true;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatus.Location = new System.Drawing.Point(3, 372);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(3);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(314, 25);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatus.Visible = false;
            // 
            // RSPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutMain);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(320, 320);
            this.Name = "RSPanel";
            this.Size = new System.Drawing.Size(320, 400);
            this.Load += new System.EventHandler(this.RSPanel_Load);
            this.tableLayoutMain.ResumeLayout(false);
            this.tableLayoutMain.PerformLayout();
            this.groupSurfDev.ResumeLayout(false);
            this.tableSurfDev.ResumeLayout(false);
            this.tableSurfDev.PerformLayout();
            this.tableButtons.ResumeLayout(false);
            this.tableButtons.PerformLayout();
            this.tablePrimaryButtons.ResumeLayout(false);
            this.tablePrimaryButtons.PerformLayout();
            this.tableQuickButtons.ResumeLayout(false);
            this.tableQuickButtons.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutMain;
        private System.Windows.Forms.GroupBox groupSurfDev;
        private System.Windows.Forms.TableLayoutPanel tableSurfDev;
        private System.Windows.Forms.Label labelZone;
        private System.Windows.Forms.ComboBox comboZone;
        private System.Windows.Forms.Label labelSection;
        private System.Windows.Forms.TextBox textSection;
        private System.Windows.Forms.Label labelTownship;
        private System.Windows.Forms.TextBox textTownship;
        private System.Windows.Forms.Label labelRange;
        private System.Windows.Forms.TextBox textRange;
        private System.Windows.Forms.Label labelMeridian;
        private System.Windows.Forms.TextBox textMeridian;
        private System.Windows.Forms.Label labelGridSize;
        private System.Windows.Forms.ComboBox comboGridSize;
        private System.Windows.Forms.Label labelScale;
        private System.Windows.Forms.ComboBox comboScale;
        private System.Windows.Forms.ComboBox comboSurveyed;
        private System.Windows.Forms.ComboBox comboInsertResidences;
        private System.Windows.Forms.TableLayoutPanel tableButtons;
        private System.Windows.Forms.TableLayoutPanel tablePrimaryButtons;
        private System.Windows.Forms.TableLayoutPanel tableQuickButtons;
        private System.Windows.Forms.Button btnBuildSection;
        private System.Windows.Forms.Button btnPushResidences;
        private System.Windows.Forms.Button btnBuildSurface;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelInsertRes;
    }
}
