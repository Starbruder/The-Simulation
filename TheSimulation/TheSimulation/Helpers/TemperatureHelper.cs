namespace TheSimulation;

public static class TemperatureHelper
{
    public static float CalculateTemperatureEffect(AtmosphereConfig config)
    {
        var tempature = config.AirTemperatureCelsius;

        // Referenzbereich für Waldbrandrelevanz
        const int minTemp = 0;
        const int maxTemp = 30;

        var normalized =
            (tempature - minTemp) / (maxTemp - minTemp);

        // Begrenzen
        normalized = Math.Clamp(normalized, 0, 1);

        // leichte Überverstärkung bei extremer Hitze
        const int extremeHeatScaleFaktor = 100;
        if (tempature > maxTemp)
        {
            normalized += (tempature - maxTemp) / extremeHeatScaleFaktor;
        }

        // –20 °C  ⇒  0.0
        // –10 °C  ⇒  0.0
        //  0  °C  ⇒  0.0
        // 15  °C  ⇒  0.5
        // 30  °C  ⇒  1.0
        // 35  °C  ⇒  1.05
        // 40  °C  ⇒  1.10
        return normalized;
    }
}
