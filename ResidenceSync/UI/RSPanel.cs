using Autodesk.AutoCAD.ApplicationServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using ResidenceSync.Properties;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ResidenceSync.UI
{
    public partial class RSPanel : UserControl
    {
        public RSPanel()
        {
            InitializeComponent();
            NormalizeTableChildren(); // ensure no child forces row growth
            InitializeZoneOptions();
            InitializeGridSizeOptions();
            InitializeScaleOptions();
            InitializeSurveyedOptions();
            InitializeInsertResidencesOptions();
            LoadUserSettings();
            InitializeQuickInsertButtons();
        }

        private void RSPanel_Load(object sender, EventArgs e)
        {
            // keep status row collapsed unless you set text/Visible=true later
            if (this.tableLayoutMain.RowStyles.Count >= 3)
            {
                this.tableLayoutMain.RowStyles[2].SizeType = System.Windows.Forms.SizeType.Absolute;
                this.tableLayoutMain.RowStyles[2].Height = 0F;
            }
        }

        private void NormalizeTableChildren()
        {
            foreach (Control c in tableSurfDev.Controls)
            {
                c.AutoSize = false;
                c.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top; // not Bottom
                c.Margin = new Padding(3);
                if (c is TextBox tb) tb.Multiline = false;
            }
        }


        // RSPanel.cs – prompt user before sending macro
        private void btnBuildSection_Click(object sender, EventArgs e)
        {
            SaveUserSettings();

            // Confirm whether the user is in UTM
            var result = MessageBox.Show(
                "Are you in UTM?",
                "Confirm UTM",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            // If the user clicks “No”, cancel the operation
            if (result != DialogResult.Yes)
            {
                SetStatus("Section build cancelled – not in UTM.");
                return;
            }

            // User confirmed they’re in UTM – proceed with macro
            var macro = MacroBuilder.BuildBuildSec(
                GetZoneSelectionValue(),
                GetTextValue(textSection),
                GetTextValue(textTownship),
                GetTextValue(textRange),
                GetTextValue(textMeridian));

            SendMacro(macro);
        }

        private void btnPushResidences_Click(object sender, EventArgs e)
        {
            SaveUserSettings();
            var macro = MacroBuilder.BuildPushResS(GetZoneSelectionValue());
            SendMacro(macro);
        }

        private void btnBuildSurface_Click(object sender, EventArgs e)
        {
            SaveUserSettings();
            var macro = MacroBuilder.BuildSurfDev(
                GetZoneSelectionValue(),
                GetTextValue(textSection),
                GetTextValue(textTownship),
                GetTextValue(textRange),
                GetTextValue(textMeridian),
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

        private string GetSurfaceSizeSelection()
        {
            if (comboGridSize.SelectedItem is string selected && !string.IsNullOrWhiteSpace(selected))
            {
                return selected;
            }

            return "5x5";
        }

        private string GetScaleSelection()
        {
            if (comboScale.SelectedItem is string selected && !string.IsNullOrWhiteSpace(selected))
            {
                return selected;
            }

            return null;
        }

        private bool? GetSurveyedSelection()
        {
            if (comboSurveyed.SelectedItem is string selected)
            {
                return string.Equals(selected, "Surveyed", StringComparison.OrdinalIgnoreCase);
            }

            return null;
        }

        private bool? GetInsertResidencesSelection()
        {
            if (comboInsertResidences.SelectedItem is string selected)
            {
                return string.Equals(selected, "Yes", StringComparison.OrdinalIgnoreCase);
            }

            return null;
        }

        private string GetTextValue(TextBox textBox)
        {
            var value = textBox.Text?.Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private void LoadUserSettings()
        {
            var settings = Settings.Default;
            SelectZoneValue(settings.SectionKeyZone ?? "11");
            textSection.Text = settings.SectionKeySec ?? string.Empty;
            textTownship.Text = settings.SectionKeyTwp ?? string.Empty;
            textRange.Text = settings.SectionKeyRge ?? string.Empty;
            textMeridian.Text = settings.SectionKeyMer ?? string.Empty;

            var savedSize = settings.SurfDevSize;
            var gridIndex = -1;
            if (!string.IsNullOrWhiteSpace(savedSize))
            {
                gridIndex = comboGridSize.FindStringExact(savedSize);
            }

            if (gridIndex >= 0)
            {
                comboGridSize.SelectedIndex = gridIndex;
            }
            else
            {
                comboGridSize.SelectedIndex = comboGridSize.FindStringExact("5x5");
            }

            var savedScale = settings.SurfDevScale;
            if (!string.IsNullOrWhiteSpace(savedScale))
            {
                SelectComboValue(comboScale, savedScale);
            }
            else
            {
                SelectComboValue(comboScale, null);
            }

            var savedSurveyed = settings.SurfDevSurveyed;
            if (savedSurveyed.HasValue)
            {
                SelectComboValue(comboSurveyed, savedSurveyed.Value ? "Surveyed" : "Unsurveyed");
            }
            else
            {
                SelectComboValue(comboSurveyed, null);
            }

            var savedInsert = settings.SurfDevInsertResidences;
            if (savedInsert.HasValue)
            {
                SelectComboValue(comboInsertResidences, savedInsert.Value ? "Yes" : "No");
            }
            else
            {
                SelectComboValue(comboInsertResidences, "No");
            }
        }

        private void SaveUserSettings()
        {
            var settings = Settings.Default;
            settings.SectionKeyZone = GetZoneSelectionValue() ?? "11";
            settings.SectionKeySec = textSection.Text?.Trim();
            settings.SectionKeyTwp = textTownship.Text?.Trim();
            settings.SectionKeyRge = textRange.Text?.Trim();
            settings.SectionKeyMer = textMeridian.Text?.Trim();
            settings.SurfDevSize = GetSurfaceSizeSelection();
            settings.SurfDevScale = GetScaleSelection() ?? string.Empty;
            settings.SurfDevSurveyed = GetSurveyedSelection();
            settings.SurfDevInsertResidences = GetInsertResidencesSelection();
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

        private void InitializeScaleOptions()
        {
            comboScale.Items.Clear();
            comboScale.Items.AddRange(new object[]
            {
                "50k",
                "25k",
                "20k"
            });

            SelectComboValue(comboScale, "50k");
        }

        private void InitializeSurveyedOptions()
        {
            comboSurveyed.Items.Clear();
            comboSurveyed.Items.AddRange(new object[]
            {
                "Surveyed",
                "Unsurveyed"
            });

            SelectComboValue(comboSurveyed, "Surveyed");
        }

        private void InitializeInsertResidencesOptions()
        {
            comboInsertResidences.Items.Clear();
            comboInsertResidences.Items.AddRange(new object[]
            {
                "Yes",
                "No"
            });

            SelectComboValue(comboInsertResidences, "No");
        }

        private void InitializeZoneOptions()
        {
            comboZone.Items.Clear();
            comboZone.Items.Add(new ZoneOption("Zone 11", "11"));
            comboZone.Items.Add(new ZoneOption("Zone 12", "12"));

            if (comboZone.Items.Count > 0)
            {
                comboZone.SelectedIndex = 0;
            }
        }

        private void SelectComboValue(ComboBox comboBox, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (comboBox.Items.Count > 0)
                {
                    comboBox.SelectedIndex = 0;
                }
                else
                {
                    comboBox.SelectedIndex = -1;
                }
                return;
            }

            var index = comboBox.FindStringExact(value);
            if (index >= 0)
            {
                comboBox.SelectedIndex = index;
            }
            else if (comboBox.Items.Count > 0 && comboBox.SelectedIndex < 0)
            {
                comboBox.SelectedIndex = 0;
            }
        }

        private void labelSurveyed_Click(object sender, EventArgs e)
        {

        }

        private void tableSurfDev_Paint(object sender, PaintEventArgs e)
        {

        }

        private string GetZoneSelectionValue()
        {
            if (comboZone.SelectedItem is ZoneOption option)
            {
                return option.Value;
            }

            if (comboZone.SelectedItem is string text)
            {
                return ExtractDigits(text);
            }

            return "11";
        }

        private void SelectZoneValue(string zoneValue)
        {
            if (string.IsNullOrWhiteSpace(zoneValue))
            {
                if (comboZone.Items.Count > 0)
                {
                    comboZone.SelectedIndex = 0;
                }
                return;
            }

            for (int i = 0; i < comboZone.Items.Count; i++)
            {
                if (comboZone.Items[i] is ZoneOption option &&
                    string.Equals(option.Value, zoneValue, StringComparison.OrdinalIgnoreCase))
                {
                    comboZone.SelectedIndex = i;
                    return;
                }
            }

            if (comboZone.Items.Count > 0)
            {
                comboZone.SelectedIndex = 0;
            }
        }

        private string ExtractDigits(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            var digits = new string(input.Where(char.IsDigit).ToArray());
            return string.IsNullOrEmpty(digits) ? null : digits;
        }

        private sealed class ZoneOption
        {
            public ZoneOption(string display, string value)
            {
                Display = display;
                Value = value;
            }

            public string Display { get; }
            public string Value { get; }

            public override string ToString() => Display;
        }

        private ContextMenuStrip BuildMacroMenu((string label, string macro)[] macros)
        {
            var menu = new ContextMenuStrip();
            foreach (var (label, macro) in macros)
            {
                var item = new ToolStripMenuItem(label);
                var macroToRun = macro;
                item.Click += (sender, e) => RunInsertMacro(macroToRun);
                menu.Items.Add(item);
            }

            return menu;
        }

        private void AttachMenu(Button button, ContextMenuStrip menu)
        {
            button.Click += (sender, e) => menu.Show(button, new Point(0, button.Height));
        }

        private void RunInsertMacro(string macro)
        {
            SendMacro(macro + "\n");
        }

        private void InitializeQuickInsertButtons()
        {
            const string arrow = " ▾";

            string BuildInsertMacro(string blockName) =>
                $"^C^C(progn (command \"-LAYER\" \"S\" \"0\" \"\") (InsertBlock1 \"{blockName}\"))";

            var labelQuickInserts = new Label
            {
                Text = "Quick inserts:",
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            var btnFreeholdRadius = new Button
            {
                Name = "btnFreeholdRadius",
                Text = "Freehold Radius" + arrow,
                AutoSize = true
            };

            var freeholdMenu = BuildMacroMenu(new[]
            {
                ("Freehold Radius", BuildInsertMacro("blk_surf_dev_freehold"))
            });
            AttachMenu(btnFreeholdRadius, freeholdMenu);

            var btnExtentFabric = new Button
            {
                Name = "btnExtentFabric",
                Text = "Extent Fabric" + arrow,
                AutoSize = true
            };

            var extentMenu = BuildMacroMenu(new[]
            {
                ("50k Surveyed Ext.", BuildInsertMacro("50000_surv_fabric")),
                ("50k Unsurveyed Ext.", BuildInsertMacro("50000_ut_fabric"))
            });
            AttachMenu(btnExtentFabric, extentMenu);

            var btnTownshipFabric = new Button
            {
                Name = "btnTownshipFabric",
                Text = "Township Fabric" + arrow,
                AutoSize = true
            };

            var townshipMenu = BuildMacroMenu(new[]
            {
                ("50k Surveyed", BuildInsertMacro("fabric_twp50000")),
                ("20k Surveyed", BuildInsertMacro("fabric_twp20000")),
                ("25k Surveyed", BuildInsertMacro("fabric_twp25000")),
                ("30k Surveyed", BuildInsertMacro("fabric_twp30000"))
            });
            AttachMenu(btnTownshipFabric, townshipMenu);

            var btnRadiusCircles = new Button
            {
                Name = "btnRadiusCircles",
                Text = "Radius Circles" + arrow,
                AutoSize = true
            };

            var radiusMenu = BuildMacroMenu(new[]
            {
                ("50k Radius Circle", BuildInsertMacro("blk_50000_rad_circles")),
                ("20k Radius Circle", BuildInsertMacro("blk_20000_rad_circles")),
                ("25k Radius Circle", BuildInsertMacro("blk_30000_rad_circles")),
                ("30k Radius Circle", BuildInsertMacro("blk_40000_rad_circles"))
            });
            AttachMenu(btnRadiusCircles, radiusMenu);

            flowButtons.Controls.Add(labelQuickInserts);
            flowButtons.Controls.Add(btnFreeholdRadius);
            flowButtons.Controls.Add(btnExtentFabric);
            flowButtons.Controls.Add(btnTownshipFabric);
            flowButtons.Controls.Add(btnRadiusCircles);
        }
    }
}
