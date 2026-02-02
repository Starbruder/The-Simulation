namespace TheSimulation;

/// <summary>
/// Hilfsklasse zur Berechnung von thermischen Einflüssen auf die Simulation.
/// Berechnet einen Multiplikator, der die Entzündungswahrscheinlichkeit basierend auf der Lufttemperatur steuert.
/// </summary>
public static class TemperatureHelper
{
    /// <summary>
    /// Berechnet den Temperatureffekt als Normalisierungswert für die Brandausbreitung.
    /// </summary>
    /// <remarks>
    /// Die Methode bildet die Temperatur auf eine Skala ab, wobei 0 °C den Wert 0.0 (kein Effekt/Hemmung) 
    /// und 30 °C den Wert 1.0 (voller Effekt) ergibt. Temperaturen über 30 °C führen zu einer 
    /// überproportionalen Erhöhung der Brandgefahr.
    /// </remarks>
    /// <param name="config">Die aktuelle Konfiguration der Atmosphäre, welche die Lufttemperatur enthält.</param>
    /// <returns>
    /// Ein Faktor (float), der typischerweise zwischen 0.0 und 1.1 liegt. 
    /// Dieser wird als Multiplikator für die Ausbreitungslogik verwendet.
    /// </returns>
    public static float CalculateTemperatureEffect(AtmosphereConfig config)
    {
        var tempature = config.AirTemperatureCelsius;

        // Referenzbereich für Waldbrandrelevanz: 0 Grad ist der Nullpunkt, 30 Grad ist das Maximum der Normal-Skala.
        const int minTemp = 0;
        const int maxTemp = 30;

        // Lineare Normalisierung: Berechnet, wo die aktuelle Temperatur im Bereich 0-30 liegt.
        var normalized = (tempature - minTemp) / (maxTemp - minTemp);

        // Begrenzen auf den Bereich [0, 1], um negative Werte bei Frost zu verhindern.
        normalized = Math.Clamp(normalized, 0, 1);

        // Leichte Überverstärkung bei extremer Hitze (über 30 °C):
        // Pro 10 Grad über dem Maximum erhöht sich der Faktor um weitere 0.1 (10%).
        const int extremeHeatScaleFaktor = 100;
        if (tempature > maxTemp)
        {
            normalized += (tempature - maxTemp) / extremeHeatScaleFaktor;
        }

        // Beispielrechnungen für die Logik:
        // –20 °C  ⇒  0.0 (Brandgefahr stark gehemmt)
        //  0  °C  ⇒  0.0
        // 15  °C  ⇒  0.5 (Mittlere Relevanz)
        // 30  °C  ⇒  1.0 (Standard-Maximum)
        // 40  °C  ⇒  1.10 (Erhöhtes Risiko durch extreme Hitze)
        return normalized;
    }
}
