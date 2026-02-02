namespace TheSimulation;

/// <summary>
/// Enthält die globalen Standardwerte für die Initialisierung der Simulationsparameter.
/// Diese Konstanten definieren das Standardverhalten der Simulation, sofern keine Benutzereinstellungen vorliegen.
/// </summary>
public static class SimulationDefaultsData
{
	/// <summary>
	/// Die standardmäßige Simulationsgeschwindigkeit. 
	/// Voreingestellt auf <see cref="SimulationSpeed.Ultra"/> für maximale Berechnungsfrequenz.
	/// </summary>
	public const SimulationSpeed DefaultSimulationSpeed = SimulationSpeed.Ultra;

	/// <summary>
	/// Die standardmäßige visuelle Form der Bäume im Renderer.
	/// Voreingestellt auf <see cref="TreeShapeType.Ellipse"/>.
	/// </summary>
	public const TreeShapeType DefaultTreeShapeType = TreeShapeType.Ellipse;

	/// <summary>
	/// Die standardmäßige Windrichtung bei Simulationsstart.
	/// Voreingestellt auf <see cref="WindDirection.North"/>.
	/// </summary>
	public const WindDirection DefaultWindDirection = WindDirection.North;
}
