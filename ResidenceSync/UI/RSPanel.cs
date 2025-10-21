using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
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
            LoadUserSettings();
        }

        private void btnBuildSection_Click(object sender, EventArgs e)
        {
            SaveUserSettings();
            var macro = MacroBuilder.BuildBuildSec(
                txtSec.Text,
                txtTwp.Text,
                txtRge.Text,
                txtMer.Text);
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
                txtSec.Text,
                txtTwp.Text,
                txtRge.Text,
                txtMer.Text,
                GetSurfaceSizeSelection(),
                GetScaleSelection(),
                GetSurveyedSelection(),
                GetInsertResidencesSelection());
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

        private string GetScaleSelection()
        {
            if (radioScale20.Checked)
            {
                return "20k";
            }

            if (radioScale25.Checked)
            {
                return "25k";
            }

            return "50k";
        }

        private string GetSurfaceSizeSelection()
        {
            if (radioSize3.Checked)
            {
                return "3x3";
            }

            if (radioSize7.Checked)
            {
                return "7x7";
            }

            if (radioSize9.Checked)
            {
                return "9x9";
            }

            return "5x5";
        }

        private bool? GetSurveyedSelection()
        {
            if (radioSurveyed.Checked)
            {
                return true;
            }

            if (radioUnsurveyed.Checked)
            {
                return false;
            }

            return null;
        }

        private bool? GetInsertResidencesSelection()
        {
            return checkInsertResidences.CheckState == CheckState.Indeterminate
                ? (bool?)null
                : checkInsertResidences.Checked;
        }

        private void LoadUserSettings()
        {
            var settings = Settings.Default;
            txtSec.Text = settings.SectionKeySec ?? string.Empty;
            txtTwp.Text = settings.SectionKeyTwp ?? string.Empty;
            txtRge.Text = settings.SectionKeyRge ?? string.Empty;
            txtMer.Text = settings.SectionKeyMer ?? string.Empty;

            switch (settings.SurfDevScale)
            {
                case "20k":
                    radioScale20.Checked = true;
                    break;
                case "25k":
                    radioScale25.Checked = true;
                    break;
                default:
                    radioScale50.Checked = true;
                    break;
            }

            switch (settings.SurfDevSize)
            {
                case "3x3":
                    radioSize3.Checked = true;
                    break;
                case "7x7":
                    radioSize7.Checked = true;
                    break;
                case "9x9":
                    radioSize9.Checked = true;
                    break;
                default:
                    radioSize5.Checked = true;
                    break;
            }

            if (settings.SurfDevSurveyed.HasValue)
            {
                if (settings.SurfDevSurveyed.Value)
                {
                    radioSurveyed.Checked = true;
                }
                else
                {
                    radioUnsurveyed.Checked = true;
                }
            }
            else
            {
                radioUnsurveyed.Checked = true;
            }

            switch (settings.SurfDevInsertResidences)
            {
                case true:
                    checkInsertResidences.Checked = true;
                    break;
                case false:
                    checkInsertResidences.Checked = false;
                    break;
                default:
                    checkInsertResidences.CheckState = CheckState.Indeterminate;
                    break;
            }
        }

        private void SaveUserSettings()
        {
            var settings = Settings.Default;
            settings.SectionKeySec = txtSec.Text?.Trim();
            settings.SectionKeyTwp = txtTwp.Text?.Trim();
            settings.SectionKeyRge = txtRge.Text?.Trim();
            settings.SectionKeyMer = txtMer.Text?.Trim();
            settings.SurfDevScale = GetScaleSelection();
            settings.SurfDevSize = GetSurfaceSizeSelection();
            var surveyed = GetSurveyedSelection();
            settings.SurfDevSurveyed = surveyed;

            var insert = GetInsertResidencesSelection();
            settings.SurfDevInsertResidences = insert;
            settings.Save();
        }
    }
}
