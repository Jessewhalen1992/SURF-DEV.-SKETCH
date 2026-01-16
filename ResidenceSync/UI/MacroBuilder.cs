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

        public static string BuildBuildSec(string zone, string sec, string twp, string rge, string mer)
        {
            var macro = "BUILDSEC\n";
            // BUILDSEC first prompts for UTM confirmation; default to "Yes" so UI macros align with prompt order.
            macro = AppendIfPresent(macro, "Yes");
            macro = AppendIfPresent(macro, zone);
            macro = AppendIfPresent(macro, sec);
            macro = AppendIfPresent(macro, twp);
            macro = AppendIfPresent(macro, rge);
            macro = AppendIfPresent(macro, mer);
            return macro.EndsWith("\n", StringComparison.Ordinal) ? macro : macro + "\n";
        }

        public static string BuildPushResS(string zone)
        {
            var macro = "PUSHRESS\n";
            macro = AppendIfPresent(macro, zone);
            return macro.EndsWith("\n", StringComparison.Ordinal) ? macro : macro + "\n";
        }

        public static string BuildSurfDev(
            string zone,
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
            macro = AppendIfPresent(macro, zone);
            macro = AppendIfPresent(macro, sec);
            macro = AppendIfPresent(macro, twp);
            macro = AppendIfPresent(macro, rge);
            macro = AppendIfPresent(macro, mer);
            // SURFDEV now prompts for grid size before scale.
            macro = AppendIfPresent(macro, size);
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
