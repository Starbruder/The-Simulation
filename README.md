# Forest Fire Simulation  
*Visualize how forest fires spread under different environmental conditions.*

This project simulates the spread of a forest fire under various environmental and weather conditions.  
It was developed as part of a school project in the third year of the IT specialist training (FIN31).

---

## Project Goal

The simulation explores the central question:

> **Visualize how forest fires spread under different environmental conditions.**

Users can adjust parameters such as wind direction, wind strength, fire spread chance, and forest density, temperature, humidity, and terrain generation to observe how these factors influence the behavior and speed of the fire.

The goal is to provide an intuitive and visual understanding of how environmental, climatic, and topographic factors affect wildfire dynamics.

---

## Features

### Fire Behavior
- Adjustable **additional fire spread chance**
- Dynamic fire propagation influenced by weather and environment
  - Wind direction and strength
  - Air temperature
  - Air humidity
  - Terrain elevation and slope
- Randomized ignition point (lightning strike)
  - Adjustable by using the lightning frequency slider in the configuration

### Weather System
- **Wind direction** N, NE, E, SE, S, SW, W, NW (static or randomized)
- **Wind strength** 0–100% and Beaford scale (static or randomized)
- Dynamic wind recalculation during the simulation
- Wind directly affects fire spread probability and direction
- Visual wind direction indicator
- **Beaufort Scale Explanation:**
The Beaufort scale is a standardized system to estimate wind force. Each level corresponds to a range of wind speeds and descriptive terms:
- 0 → Calm (<1 km/h)
- 1 → Light Air (1–5 km/h)
- 2 → Light Breeze (6–11 km/h)
- 3 → Gentle Breeze (12–19 km/h)
- 4 → Moderate Breeze (20–28 km/h)
- 5 → Fresh Breeze (29–38 km/h)
- 6 → Strong Breeze (39–49 km/h)
- 7 → Strong Wind (50–61 km/h)
- 8 → Severe Wind (62–74 km/h)
- 9 → Storm (75–88 km/h)
- 10 → Violent Storm (89–102 km/h)
- 11 → Hurricane (103–117 km/h)
- 12 → Hurricane Extreme (118+ km/h)

### Forest Parameters
- Optional **prefill** of the forest
- Adjustable **forest density**
- Randomized tree placement
- Optional forest regrowth
- Tree growth pauses while fire is active (configurable)

### Simulation
- Real-time grid-based visualization
- Adjustable simulation speed via slider
- Live statistics:
  - Total grown trees
  - Total burned trees
  - Forest density
  - Wind strengh (In % and Beaford scale 1 - 11)
  - Runtime

### Evaluation & Export
The evaluation interface provides real-time graphical and statistical insights into the simulation dynamics:

- **Grown / Burned Trees Graph**  
  Visualizes the cumulative count of trees grown and burned over time, allowing users to observe how the fire spreads and affects forest density.

- **Active Trees Graph**  
  Shows the number of currently active (healthy) trees over time and the average active trees, illustrating the balance between growth and destruction.

- **Wind Parameters**  
  Wind speed and direction strongly influence fire spread. Both the percent value and the Beaufort scale are tracked and shown, helping to correlate wind conditions with fire dynamics.

- **Air Humidity and Temperature**  
  These climatic parameters affect fire behavior and tree growth. Monitoring their values over time provides insight into how environmental conditions modulate fire propagation and recovery.

- **Runtime**  
  Shows the elapsed simulation time, facilitating time-based analysis and comparison between different simulation runs.

- **Export as CSV**  
  Simulation history can be exported for further analysis. Exported CSV includes the following columns:
  - **TimeSeconds** → simulation time in seconds
  - **TotalGrown** → total grown trees
  - **TotalBurned** → total burned trees
  - **ActiveTrees** → currently active trees (Grown – Burned)
  - **WindSpeed** → wind speed in percent
  - **WindBft** → wind strength according to the Beaufort scale

These insights help users understand the impact of environmental conditions on wildfire behavior, while exported data enables further statistical or visual analysis using external tools such as Excel, Python, or R.

---

### UI & Visualization
- Clear, minimalistic interface
- Color-coded grid:
  - Green → healthy tree (diffrent randomly choosen shades of green)
  - Red → burning  
  - Gray → burned (configurable)
- Optional visual effects
  - Lighning strikes
  - Bolt flashes
  - Flame animations
  - Fire particles
  - Smoke particles
  - Burned trees
  - Shape of the Trees
- Wind arrow visualization inside compass
  - Red arrow: Where the wind is coming from
  - Blue arrow: Where the wind is heading towards
  - Arrow length: Determines the windstrength
- Terrain-influenced tree coloring (brightness)
  - Bright: Far up (hill)
  - Dark: Down (sea level)

---

## Randomness & Distributions

The simulation uses multiple random processes to model natural behavior:

- **Uniform distribution** → random fire start position  
- **Normal distribution** → fire spread chance    
- **Exponential distribution** → potential secondary ignition events  

These random components ensure that each simulation run behaves differently while remaining reproducible in its overall patterns.

---

## Technologies Used

- **C#**
- **.NET**
- **WPF**
- Object‑oriented simulation architecture

---

## Educational Purpose

This project demonstrates:

- How simulations can model complex natural processes  
- How environmental parameters influence dynamic systems  
- How to visualize data and processes in real time  
- How randomness and probability affect outcomes  

It was created as part of a school assignment focusing on simulation, GUI development, and clean code principles.
