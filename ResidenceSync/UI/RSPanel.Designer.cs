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
            this.labelSurveyed = new System.Windows.Forms.Label();
            this.comboSurveyed = new System.Windows.Forms.ComboBox();
            this.labelInsertResidences = new System.Windows.Forms.Label();
            this.comboInsertResidences = new System.Windows.Forms.ComboBox();
            this.flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnBuildSection = new System.Windows.Forms.Button();
            this.btnPushResidences = new System.Windows.Forms.Button();
            this.btnBuildSurface = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.tableLayoutMain.SuspendLayout();
            this.groupSurfDev.SuspendLayout();
            this.tableSurfDev.SuspendLayout();
            this.flowButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutMain
            // 
            this.tableLayoutMain.ColumnCount = 1;
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.Controls.Add(this.groupSurfDev, 0, 0);
            this.tableLayoutMain.Controls.Add(this.flowButtons, 0, 1);
            this.tableLayoutMain.Controls.Add(this.lblStatus, 0, 2);
            this.tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.RowCount = 3;
            this.tableLayoutMain.RowStyles.Clear();
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableLayoutMain.Size = new System.Drawing.Size(320, 200);
            this.tableLayoutMain.TabIndex = 0;
            //
            // groupSurfDev
            //
            this.groupSurfDev.AutoSize = false;
            this.groupSurfDev.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.groupSurfDev.Controls.Add(this.tableSurfDev);
            this.groupSurfDev.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupSurfDev.Location = new System.Drawing.Point(3, 3);
            this.groupSurfDev.Name = "groupSurfDev";
            this.groupSurfDev.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            this.groupSurfDev.Size = new System.Drawing.Size(314, 273);
            this.groupSurfDev.MinimumSize = new System.Drawing.Size(360, 220);
            this.groupSurfDev.TabIndex = 0;
            this.groupSurfDev.TabStop = false;
            this.groupSurfDev.Text = "SURFDEV Options";
            // 
            // tableSurfDev
            // 
            this.tableSurfDev.ColumnCount = 2;
            this.tableSurfDev.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tableSurfDev.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableSurfDev.Controls.Add(this.labelSection, 0, 0);
            this.tableSurfDev.Controls.Add(this.textSection, 1, 0);
            this.tableSurfDev.Controls.Add(this.labelTownship, 0, 1);
            this.tableSurfDev.Controls.Add(this.textTownship, 1, 1);
            this.tableSurfDev.Controls.Add(this.labelRange, 0, 2);
            this.tableSurfDev.Controls.Add(this.textRange, 1, 2);
            this.tableSurfDev.Controls.Add(this.labelMeridian, 0, 3);
            this.tableSurfDev.Controls.Add(this.textMeridian, 1, 3);
            this.tableSurfDev.Controls.Add(this.labelGridSize, 0, 4);
            this.tableSurfDev.Controls.Add(this.comboGridSize, 1, 4);
            this.tableSurfDev.Controls.Add(this.labelScale, 0, 5);
            this.tableSurfDev.Controls.Add(this.comboScale, 1, 5);
            this.tableSurfDev.Controls.Add(this.labelSurveyed, 0, 6);
            this.tableSurfDev.Controls.Add(this.comboSurveyed, 1, 6);
            this.tableSurfDev.Controls.Add(this.labelInsertResidences, 0, 7);
            this.tableSurfDev.Controls.Add(this.comboInsertResidences, 1, 7);
            this.tableSurfDev.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableSurfDev.Location = new System.Drawing.Point(8, 19);
            this.tableSurfDev.Name = "tableSurfDev";
            this.tableSurfDev.RowCount = 8;
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableSurfDev.Size = new System.Drawing.Size(298, 246);
            this.tableSurfDev.TabIndex = 0;
            //
            // labelSection
            //
            this.labelSection.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSection.AutoSize = true;
            this.labelSection.Location = new System.Drawing.Point(3, 7);
            this.labelSection.Name = "labelSection";
            this.labelSection.Size = new System.Drawing.Size(47, 15);
            this.labelSection.TabIndex = 0;
            this.labelSection.Text = "Section";
            //
            // textSection
            //
            this.textSection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textSection.Location = new System.Drawing.Point(113, 3);
            this.textSection.Name = "textSection";
            this.textSection.Size = new System.Drawing.Size(182, 23);
            this.textSection.TabIndex = 1;
            //
            // labelTownship
            //
            this.labelTownship.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelTownship.AutoSize = true;
            this.labelTownship.Location = new System.Drawing.Point(3, 37);
            this.labelTownship.Name = "labelTownship";
            this.labelTownship.Size = new System.Drawing.Size(62, 15);
            this.labelTownship.TabIndex = 2;
            this.labelTownship.Text = "Township";
            //
            // textTownship
            //
            this.textTownship.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textTownship.Location = new System.Drawing.Point(113, 33);
            this.textTownship.Name = "textTownship";
            this.textTownship.Size = new System.Drawing.Size(182, 23);
            this.textTownship.TabIndex = 3;
            //
            // labelRange
            //
            this.labelRange.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelRange.AutoSize = true;
            this.labelRange.Location = new System.Drawing.Point(3, 67);
            this.labelRange.Name = "labelRange";
            this.labelRange.Size = new System.Drawing.Size(40, 15);
            this.labelRange.TabIndex = 4;
            this.labelRange.Text = "Range";
            //
            // textRange
            //
            this.textRange.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textRange.Location = new System.Drawing.Point(113, 63);
            this.textRange.Name = "textRange";
            this.textRange.Size = new System.Drawing.Size(182, 23);
            this.textRange.TabIndex = 5;
            //
            // labelMeridian
            //
            this.labelMeridian.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelMeridian.AutoSize = true;
            this.labelMeridian.Location = new System.Drawing.Point(3, 97);
            this.labelMeridian.Name = "labelMeridian";
            this.labelMeridian.Size = new System.Drawing.Size(56, 15);
            this.labelMeridian.TabIndex = 6;
            this.labelMeridian.Text = "Meridian";
            //
            // textMeridian
            //
            this.textMeridian.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textMeridian.Location = new System.Drawing.Point(113, 93);
            this.textMeridian.Name = "textMeridian";
            this.textMeridian.Size = new System.Drawing.Size(182, 23);
            this.textMeridian.TabIndex = 7;
            //
            // labelGridSize
            //
            this.labelGridSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelGridSize.AutoSize = true;
            this.labelGridSize.Location = new System.Drawing.Point(3, 127);
            this.labelGridSize.Name = "labelGridSize";
            this.labelGridSize.Size = new System.Drawing.Size(58, 15);
            this.labelGridSize.TabIndex = 8;
            this.labelGridSize.Text = "Grid size";
            //
            // comboGridSize
            //
            this.comboGridSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboGridSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboGridSize.FormattingEnabled = true;
            this.comboGridSize.Location = new System.Drawing.Point(113, 123);
            this.comboGridSize.Name = "comboGridSize";
            this.comboGridSize.Size = new System.Drawing.Size(182, 23);
            this.comboGridSize.TabIndex = 9;
            //
            // labelScale
            //
            this.labelScale.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelScale.AutoSize = true;
            this.labelScale.Location = new System.Drawing.Point(3, 157);
            this.labelScale.Name = "labelScale";
            this.labelScale.Size = new System.Drawing.Size(35, 15);
            this.labelScale.TabIndex = 10;
            this.labelScale.Text = "Scale";
            //
            // comboScale
            //
            this.comboScale.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboScale.FormattingEnabled = true;
            this.comboScale.Location = new System.Drawing.Point(113, 153);
            this.comboScale.Name = "comboScale";
            this.comboScale.Size = new System.Drawing.Size(182, 23);
            this.comboScale.TabIndex = 11;
            //
            // labelSurveyed
            //
            this.labelSurveyed.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSurveyed.AutoSize = true;
            this.labelSurveyed.Location = new System.Drawing.Point(3, 187);
            this.labelSurveyed.Name = "labelSurveyed";
            this.labelSurveyed.Size = new System.Drawing.Size(56, 15);
            this.labelSurveyed.TabIndex = 12;
            this.labelSurveyed.Text = "Surveyed";
            //
            // comboSurveyed
            //
            this.comboSurveyed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboSurveyed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSurveyed.FormattingEnabled = true;
            this.comboSurveyed.Location = new System.Drawing.Point(113, 183);
            this.comboSurveyed.Name = "comboSurveyed";
            this.comboSurveyed.Size = new System.Drawing.Size(182, 23);
            this.comboSurveyed.TabIndex = 13;
            //
            // labelInsertResidences
            //
            this.labelInsertResidences.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelInsertResidences.AutoSize = true;
            this.labelInsertResidences.Location = new System.Drawing.Point(3, 219);
            this.labelInsertResidences.Name = "labelInsertResidences";
            this.labelInsertResidences.Size = new System.Drawing.Size(101, 15);
            this.labelInsertResidences.TabIndex = 14;
            this.labelInsertResidences.Text = "Insert residences";
            //
            // comboInsertResidences
            //
            this.comboInsertResidences.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboInsertResidences.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboInsertResidences.FormattingEnabled = true;
            this.comboInsertResidences.Location = new System.Drawing.Point(113, 213);
            this.comboInsertResidences.Name = "comboInsertResidences";
            this.comboInsertResidences.Size = new System.Drawing.Size(182, 23);
            this.comboInsertResidences.TabIndex = 15;
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
            this.flowButtons.Location = new System.Drawing.Point(3, 282);
            this.flowButtons.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.flowButtons.Name = "flowButtons";
            this.flowButtons.Size = new System.Drawing.Size(314, 33);
            this.flowButtons.TabIndex = 1;
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
            this.lblStatus.Location = new System.Drawing.Point(3, 318);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(314, 82);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Ready";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // RSPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutMain);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(320, 320);
            this.Name = "RSPanel";
            this.Size = new System.Drawing.Size(320, 400);
            this.tableLayoutMain.ResumeLayout(false);
            this.tableLayoutMain.PerformLayout();
            this.groupSurfDev.ResumeLayout(false);
            this.tableSurfDev.ResumeLayout(false);
            this.tableSurfDev.PerformLayout();
            this.flowButtons.ResumeLayout(false);
            this.flowButtons.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutMain;
        private System.Windows.Forms.GroupBox groupSurfDev;
        private System.Windows.Forms.TableLayoutPanel tableSurfDev;
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
        private System.Windows.Forms.Label labelSurveyed;
        private System.Windows.Forms.ComboBox comboSurveyed;
        private System.Windows.Forms.Label labelInsertResidences;
        private System.Windows.Forms.ComboBox comboInsertResidences;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button btnBuildSection;
        private System.Windows.Forms.Button btnPushResidences;
        private System.Windows.Forms.Button btnBuildSurface;
        private System.Windows.Forms.Label lblStatus;
    }
}
