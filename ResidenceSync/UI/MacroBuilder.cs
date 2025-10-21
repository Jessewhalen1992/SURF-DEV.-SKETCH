using System;

namespace ResidenceSync.UI
{
    internal static class MacroBuilder
    {
        private static string Sanitize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var cleaned = value.Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');
            return cleaned.Trim();
        }

        private static string AppendIfPresent(string macro, string value)
        {
            var sanitized = Sanitize(value);
            if (!string.IsNullOrEmpty(sanitized))
            {
                macro += sanitized + "\n";
            }

            return macro;
        }

        public static string BuildBuildSec(string sec, string twp, string rge, string mer)
        {
            var macro = "BUILDSEC\n";
            macro = AppendIfPresent(macro, sec);
            macro = AppendIfPresent(macro, twp);
            macro = AppendIfPresent(macro, rge);
            macro = AppendIfPresent(macro, mer);
            return macro.EndsWith("\n", StringComparison.Ordinal) ? macro : macro + "\n";
        }

        public static string BuildPushResS()
        {
            return "PUSHRESS\n";
        }

        public static string BuildSurfDev(
            string sec,
            string twp,
            string rge,
            string mer,
            string size,
            string scale,
            bool? isSurveyed,
            bool? insertResidences)
        {
            var macro = "SURFDEV\n";
            macro = AppendIfPresent(macro, sec);
            macro = AppendIfPresent(macro, twp);
            macro = AppendIfPresent(macro, rge);
            macro = AppendIfPresent(macro, mer);
            _ = size; // size is prompted later; keep parameter for compatibility
            macro = AppendIfPresent(macro, scale);

            if (isSurveyed.HasValue)
            {
                macro += (isSurveyed.Value ? "Surveyed" : "Unsurveyed") + "\n";
            }

            if (insertResidences.HasValue)
            {
                macro += (insertResidences.Value ? "Yes" : "No") + "\n";
            }

            return macro.EndsWith("\n", StringComparison.Ordinal) ? macro : macro + "\n";
        }
    }
}
