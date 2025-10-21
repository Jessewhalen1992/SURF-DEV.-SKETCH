// ResidenceSync UI Palette
// How to compile: Build the ResidenceSync solution in Visual Studio targeting .NET Framework 4.8.
// How to load: NETLOAD → pick ResidenceSync.dll.
// How to open the UI: run command RSUI.
// Usage notes: The palette only sends command macros; any required picks or prompts continue in AutoCAD.

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using System.Drawing;
using System.Windows.Forms;

namespace ResidenceSync.UI
{
    public class RSUiCommands : IExtensionApplication
    {
        private static PaletteSet _paletteSet;
        private static RSPanel _panel;
        private static readonly object _syncRoot = new object();
        private static readonly System.Guid PaletteGuid = new System.Guid("2F0D4F71-7A8C-4C44-9F2C-A0A5D5C9E51E");

        [CommandMethod("ResidenceSync", "RSUI", CommandFlags.Modal)]
        public void ShowResidenceSyncPalette()
        {
            EnsurePalette();

            if (_paletteSet != null)
            {
                if (!_paletteSet.Visible)
                {
                    _paletteSet.Visible = true;
                }

                _paletteSet.KeepFocus = false;
                _paletteSet.Activate(0);
            }
        }

        private static void EnsurePalette()
        {
            if (_paletteSet != null)
            {
                return;
            }

            lock (_syncRoot)
            {
                if (_paletteSet != null)
                {
                    return;
                }

                _panel = new RSPanel
                {
                    Dock = DockStyle.Fill
                };

                _paletteSet = new PaletteSet("ResidenceSync UI", PaletteGuid)
                {
                    Style = PaletteSetStyles.ShowCloseButton | PaletteSetStyles.ShowTabForSingle | PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.Snappable,
                    Size = new Size(360, 520),
                    KeepFocus = false
                };

                _paletteSet.MinimumSize = new Size(320, 360);
                _paletteSet.DockEnabled = DockSides.Left | DockSides.Right | DockSides.Bottom;
                _paletteSet.EnableTransparency(false);
                _paletteSet.Add("ResidenceSync", _panel);
                _paletteSet.Visible = true;
            }
        }

        public void Initialize()
        {
        }

        public void Terminate()
        {
            if (_paletteSet != null)
            {
                _paletteSet.Visible = false;
                _paletteSet.Dispose();
                _paletteSet = null;
                _panel = null;
            }
        }
    }
}
