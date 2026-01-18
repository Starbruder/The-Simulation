using System.Globalization;
using System.IO;
using System.Text;

namespace TheSimulation;

public static class EvaluationExporter
{
    /// <summary>
    /// Erstellt den CSV-Inhalt für eine Simulation.
    /// </summary>
    /// <param name="data">Die Evaluation mit History.</param>
    /// <param name="normalizeWind">WindSpeed wird auf 0..1 skaliert, wenn true.</param>
    /// <returns>CSV als String</returns>
    public static string ToCsv(Evaluation data)
    {
        if (data.History.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.AppendLine("TimeSeconds,TotalGrown,TotalBurned,ActiveTrees,WindSpeed,WindBft");

        foreach (var snapShot in data.History)
        {
            var active = snapShot.Grown - snapShot.Burned;

            var windPercent = snapShot.WindSpeed * 100;

            var windBft =
                (int)WindMapper.ConvertWindPercentStrenghToBeaufort(snapShot.WindSpeed);

            sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "{0},{1},{2},{3},{4:F0},{5}",
                (int)snapShot.Time.TotalSeconds,
                snapShot.Grown,
                snapShot.Burned,
                active,
                windPercent,
                windBft));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Schreibt die CSV-Datei direkt auf die Festplatte.
    /// </summary>
    /// <param name="evaluationData">Evaluation</param>
    /// <param name="filePath">Pfad zur Datei</param>
    /// <param name="normalizeWind">WindSpeed normalisieren auf 0..1</param>
    public static void ExportCsv(Evaluation evaluationData, string filePath)
    {
        var csvContent = ToCsv(evaluationData);
        File.WriteAllText(filePath, csvContent, Encoding.UTF8);
    }
}
