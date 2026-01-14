# Forest Fire Simulation  
*How does a forest fire spread under different environmental conditions?*

This project simulates the spread of a forest fire under various environmental and weather conditions.  
It was developed as part of a school project in the third year of the IT specialist training (FIN31).

---

## Project Goal

The simulation explores the central question:

> **How does a forest fire spread under different environmental conditions?**

Users can adjust parameters such as wind direction, wind strength, fire spread chance, and forest density, temperature, humidity, and terrain generation to observe how these factors influence the behavior and speed of the fire.

The goal is to provide an intuitive and visual understanding of how environmental, climatic, and topographic factors affect wildfire dynamics.

---

## Features

### Fire Behavior
- Adjustable **fire spread chance**
- Dynamic fire propagation influenced by weather and environment
  - Wind direction and strength
  - Air temperature
  - Air humidity
  - Terrain elevation and slope
- Randomized ignition point (lightning strike)

### Weather System
- **Wind direction** N, NE, E, SE, S, SW, W, NW (static or randomized)
- **Wind strength** 0–100% (static or randomized)
- Dynamic wind recalculation during the simulation
- Wind directly affects fire spread probability and direction
- Visual wind direction indicator

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
  - Wind strengh
  - Runtime

### UI & Visualization
- Clear, minimalistic interface
- Color-coded grid:
  - Green → healthy tree  
  - Red → burning  
  - Black → burned  
- Optional visual effects
  - Lighning strikes
  - Fire particles
  - Smoke particles
  - Burned trees
- Wind arrow visualization
- Terrain-influenced tree coloring (brightness)

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
