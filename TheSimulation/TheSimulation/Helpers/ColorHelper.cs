namespace TheSimulation;

public static class ColorHelper
{
    //public static Brush AdjustColorByElevation(Brush baseColor, float elevation)
    //{
    //    // elevation: 0.0 (tief) bis 1.0 (hoch)
    //    var sc = ((SolidColorBrush)baseColor).Color;

    //    // Helligkeit linear anpassen: tief = dunkel, hoch = hell
    //    byte Adjust(byte channel) =>
    //        (byte)Math.Clamp(channel * 0.5 + 0.5 * channel * elevation, 0, 255);

    //    var newColor = Color.FromRgb(
    //        Adjust(sc.R),
    //        Adjust(sc.G),
    //        Adjust(sc.B)
    //    );

    //    return new SolidColorBrush(newColor);
    //}

    /// <summary>
    /// Passt die Helligkeit einer uint-Farbe (Bgra32) basierend auf der Geländehöhe an.
    /// </summary>
    /// <param name="baseColorUint">Die Ausgangsfarbe als uint (0xBBGGRRAA oder 0xAARRGGBB).</param>
    /// <param name="elevation">Höhenwert von 0.0 (tief) bis 1.0 (hoch).</param>
    /// <returns>Die angepasste Farbe als uint.</returns>
    public static uint AdjustColorByElevation(uint baseColorUint, float elevation)
    {
        // 1. Kanäle extrahieren (Bgra32)
        byte b = (byte)(baseColorUint & 0xFF);
        byte g = (byte)((baseColorUint >> 8) & 0xFF);
        byte r = (byte)((baseColorUint >> 16) & 0xFF);
        byte a = (byte)((baseColorUint >> 24) & 0xFF);

        // 2. Deine Formel anwenden: tief = dunkel (50%), hoch = original (100%)
        // Hinweis: Deine alte Formel war: channel * 0.5 + 0.5 * channel * elevation
        // Das entspricht: channel * (0.5 + 0.5 * elevation)
        float factor = 0.5f + 0.5f * elevation;

        byte Adjust(byte channel) => (byte)Math.Clamp(channel * factor, 0, 255);

        byte newR = Adjust(r);
        byte newG = Adjust(g);
        byte newB = Adjust(b);

        // 3. Wieder zu uint zusammensetzen
        return (uint)(newB | (newG << 8) | (newR << 16) | (a << 24));
    }
}
