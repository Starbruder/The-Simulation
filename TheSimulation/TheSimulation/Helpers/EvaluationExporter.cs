using System.Globalization;
using System.IO;
using System.Text;

namespace TheSimulation;

/// <summary>
/// Stellt Funktionen bereit, um Simulationsergebnisse und statistische Daten in externe Dateiformate zu exportieren.
/// Der Fokus liegt auf der Erzeugung von zeitreihenbasierten CSV-Daten für die spätere Analyse (z. B. in Excel oder Python).
/// </summary>
public static class EvaluationExporter
{
    /// <summary>
    /// Konvertiert die gesammelten Historie-Daten einer Simulation in einen CSV-formatierten String.
    /// </summary>
    /// <remarks>
    /// Die Methode verwendet <see cref="CultureInfo.InvariantCulture"/>, um sicherzustellen, dass Dezimaltrennzeichen 
    /// unabhängig von den lokalen Systemeinstellungen konsistent als Punkte (.) exportiert werden.
    /// </remarks>
    /// <param name="data">Das <see cref="Evaluation"/>-Objekt, welches die Liste der Zeitstempel-Schnappschüsse enthält.</param>
    /// <returns>
    /// Ein String im CSV-Format mit Header. Gibt <see cref="string.Empty"/> zurück, wenn keine Historie-Daten vorhanden sind.
    /// </returns>
    public static string ToCsv(Evaluation data)
    {
        if (data.History.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        // Header-Zeile: Definiert die Spaltennamen für die Auswertung
        sb.AppendLine("TimeSeconds,TotalGrown,TotalBurned,ActiveTrees,WindSpeed,WindBft");

        foreach (var snapShot in data.History)
        {
            // Berechnung der aktuell lebenden Bäume zum Zeitpunkt des Schnappschusses
            var active = snapShot.Grown - snapShot.Burned;

            // Skalierung der Windstärke auf eine Prozentanzeige (0-100)
            var windPercent = snapShot.WindSpeed * 100;

            // Umrechnung der Windstärke in die ganzzahlige Beaufort-Skala (0-12)
            var windBft = (int)WindMapper.ConvertWindPercentStrenghToBeaufort(snapShot.WindSpeed);

            // Formatierung der Zeile: Invariante Kultur verhindert Probleme beim Einlesen in Analyse-Tools
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
    /// Erzeugt den CSV-Inhalt und schreibt diesen asynchron oder blockierend direkt in eine Datei auf der Festplatte.
    /// </summary>
    /// <remarks>
    /// Die Datei wird mit UTF-8 Kodierung gespeichert, um Kompatibilität mit modernen Texteditoren zu gewährleisten.
    /// </remarks>
    /// <param name="evaluationData">Die auszuwertenden Simulationsdaten.</param>
    /// <param name="filePath">Der vollständige Zielpfad inklusive Dateiname (z.B. "C:/Exports/SimResult_01.csv").</param>
    /// <exception cref="IOException">Wird geworfen, wenn die Datei nicht geschrieben werden kann (z.B. fehlende Berechtigungen oder Datei geöffnet).</exception>
    public static void ExportCsv(Evaluation evaluationData, string filePath)
    {
        var csvContent = ToCsv(evaluationData);
        File.WriteAllText(filePath, csvContent, Encoding.UTF8);
    }
}
