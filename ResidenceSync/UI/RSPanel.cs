using Autodesk.AutoCAD.ApplicationServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using ResidenceSync.Properties;
using System;
using System.Windows.Forms;

namespace ResidenceSync.UI
{
    public partial class RSPanel : UserControl
    {
        public RSPanel()
        {
            InitializeComponent();
            InitializeGridSizeOptions();
            LoadUserSettings();
        }

        private void btnBuildSection_Click(object sender, EventArgs e)
        {
            SaveUserSettings();
            var macro = MacroBuilder.BuildBuildSec(
                null,
                null,
                null,
                null);
            SendMacro(macro);
        }

        private void btnPushResidences_Click(object sender, EventArgs e)
        {
            SaveUserSettings();
            var macro = MacroBuilder.BuildPushResS();
            SendMacro(macro);
        }

        private void btnBuildSurface_Click(object sender, EventArgs e)
        {
            SaveUserSettings();
            var macro = MacroBuilder.BuildSurfDev(
                null,
                null,
                null,
                null,
                GetSurfaceSizeSelection(),
                null,
                null,
                null);
            SendMacro(macro);
        }

        private void SendMacro(string macro)
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    SetStatus("No active document.");
                    return;
                }

                doc.SendStringToExecute(macro, true, false, false);
                SetStatus($"Sent: {macro.Replace("\n", "\\n")}");
            }
            catch (System.Exception ex)
            {
                SetStatus($"Error: {ex.Message}");
            }
        }

        private void SetStatus(string message)
        {
            lblStatus.Text = message;
        }

        private string GetSurfaceSizeSelection()
        {
            if (comboGridSize.SelectedItem is string selected && !string.IsNullOrWhiteSpace(selected))
            {
                return selected;
            }

            return "5x5";
        }

        private void LoadUserSettings()
        {
            var settings = Settings.Default;
            var savedSize = settings.SurfDevSize;
            if (!string.IsNullOrWhiteSpace(savedSize))
            {
                var index = comboGridSize.FindStringExact(savedSize);
                if (index >= 0)
                {
                    comboGridSize.SelectedIndex = index;
                    return;
                }
            }

            comboGridSize.SelectedIndex = comboGridSize.FindStringExact("5x5");
        }

        private void SaveUserSettings()
        {
            var settings = Settings.Default;
            settings.SurfDevSize = GetSurfaceSizeSelection();
            settings.Save();
        }

        private void InitializeGridSizeOptions()
        {
            comboGridSize.Items.Clear();
            comboGridSize.Items.AddRange(new object[]
            {
                "3x3",
                "5x5",
                "7x7",
                "9x9"
            });

            if (comboGridSize.SelectedIndex < 0)
            {
                comboGridSize.SelectedIndex = comboGridSize.FindStringExact("5x5");
            }
        }
    }
}
