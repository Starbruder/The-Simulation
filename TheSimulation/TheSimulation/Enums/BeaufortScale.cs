namespace TheSimulation;

public enum BeaufortScale
{
    Calm = 0,               // Windstille, Flaute < 1 kn < 1 km/h 
    LightAir = 1,           // Leichter/Leiser Zug 1–3 kn 1–5 km/h 
    LightBreeze = 2,        // Leichte Brise 4–6 kn 1–5 km/h 
    GentleBreeze = 3,       // Schwache/Mäßige Brise 7–10 kn 12–19 km/h
    ModerateBreeze = 4,     // Mäßige/Frische Brise 11–15 kn 20–28 km/h 
    FreshBreeze = 5,        // Frische Brise, Frischer Wind 16–21 kn 29–38 km/h 
    StrongBreeze = 6,       // Starker Wind 22–27 kn 39–49 km/h 
    StrongWind = 7,               // Steifer Wind 28–33 kn 50–61 km/h
    SevereWind = 8,         // Stürmischer Wind 34–40 kn 62–74 km/h 
    Storm = 9,              // Sturm 41–47 kn 75–88 km/h 
    ViolentStorm = 10,      // Schwerer Sturm 48–55 kn 89–102 km/h 
    Hurricane = 11,         // Orkanartiger Sturm 56–63 kn 103–117 km/h
    HurricaneExtreme = 12,  // Orkan 64+ kn 118+ km/h
}
