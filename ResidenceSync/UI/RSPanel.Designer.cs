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
            this.labelGridSize = new System.Windows.Forms.Label();
            this.comboGridSize = new System.Windows.Forms.ComboBox();
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
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutMain.Size = new System.Drawing.Size(320, 200);
            this.tableLayoutMain.TabIndex = 0;
            // 
            // groupSurfDev
            // 
            this.groupSurfDev.AutoSize = true;
            this.groupSurfDev.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupSurfDev.Controls.Add(this.tableSurfDev);
            this.groupSurfDev.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupSurfDev.Location = new System.Drawing.Point(3, 3);
            this.groupSurfDev.Name = "groupSurfDev";
            this.groupSurfDev.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            this.groupSurfDev.Size = new System.Drawing.Size(314, 81);
            this.groupSurfDev.TabIndex = 0;
            this.groupSurfDev.TabStop = false;
            this.groupSurfDev.Text = "SURFDEV Options";
            // 
            // tableSurfDev
            // 
            this.tableSurfDev.ColumnCount = 2;
            this.tableSurfDev.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableSurfDev.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableSurfDev.Controls.Add(this.labelGridSize, 0, 0);
            this.tableSurfDev.Controls.Add(this.comboGridSize, 1, 0);
            this.tableSurfDev.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableSurfDev.Location = new System.Drawing.Point(8, 19);
            this.tableSurfDev.Name = "tableSurfDev";
            this.tableSurfDev.RowCount = 1;
            this.tableSurfDev.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableSurfDev.Size = new System.Drawing.Size(298, 54);
            this.tableSurfDev.TabIndex = 0;
            // 
            // labelGridSize
            // 
            this.labelGridSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelGridSize.AutoSize = true;
            this.labelGridSize.Location = new System.Drawing.Point(3, 19);
            this.labelGridSize.Name = "labelGridSize";
            this.labelGridSize.Size = new System.Drawing.Size(58, 15);
            this.labelGridSize.TabIndex = 0;
            this.labelGridSize.Text = "Grid size";
            // 
            // comboGridSize
            // 
            this.comboGridSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboGridSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboGridSize.FormattingEnabled = true;
            this.comboGridSize.Location = new System.Drawing.Point(93, 3);
            this.comboGridSize.Name = "comboGridSize";
            this.comboGridSize.Size = new System.Drawing.Size(202, 23);
            this.comboGridSize.TabIndex = 1;
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
            this.flowButtons.Location = new System.Drawing.Point(3, 90);
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
            this.lblStatus.Location = new System.Drawing.Point(3, 123);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(314, 74);
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
            this.MinimumSize = new System.Drawing.Size(320, 160);
            this.Name = "RSPanel";
            this.Size = new System.Drawing.Size(320, 200);
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
        private System.Windows.Forms.Label labelGridSize;
        private System.Windows.Forms.ComboBox comboGridSize;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button btnBuildSection;
        private System.Windows.Forms.Button btnPushResidences;
        private System.Windows.Forms.Button btnBuildSurface;
        private System.Windows.Forms.Label lblStatus;
    }
}
