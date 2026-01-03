# Forest Fire Simulation  
*How does a forest fire spread under different weather conditions?*

This project simulates the spread of a forest fire under various environmental and weather conditions.  
It was developed as part of a school project in the third year of the IT specialist training (FIN31).

---

## Project Goal

The simulation explores the central question:

> **How does a forest fire spread under different weather conditions?**

Users can adjust parameters such as wind direction, wind strength, fire spread chance, and forest density to observe how these factors influence the behavior and speed of the fire.

The goal is to provide an intuitive and visual understanding of how environmental factors affect wildfire dynamics.

---

## Features

### Fire Behavior
- Adjustable **fire spread chance**
- Dynamic fire propagation influenced by weather and environment
- Randomized ignition point (lightning strike)

### Weather System
- **Wind direction** (N, E, S, W)
- **Wind strength** (0–100%)
- Wind affects fire spread probability and direction

### Forest Parameters
- Optional **prefill** of the forest
- Adjustable **forest density**
- Randomized tree placement

### Simulation
- Real-time grid-based visualization
- Adjustable simulation speed via slider
- Live statistics:
  - Total grown trees
  - Total burned trees
  - Forest density
  - Runtime

### UI & Visualization
- Clear, minimalistic interface
- Color-coded grid:
  - Green → healthy tree  
  - Red → burning  
  - Black → burned  
- Designed for intuitive use

---

## Randomness & Distributions

The simulation uses multiple random processes to model natural behavior:

- **Uniform distribution** → random fire start position  
- **Normal distribution** → wind fluctuations  
- **Exponential distribution** → potential secondary ignition events  

These distributions help create more realistic and varied simulation outcomes.

---

## Technologies Used

- **C#**
- **.NET**
- **WPF** (or WinForms, depending on implementation)
- Object‑oriented simulation architecture

---

## Educational Purpose

This project demonstrates:

- How simulations can model complex natural processes  
- How environmental parameters influence dynamic systems  
- How to visualize data and processes in real time  
- How randomness and probability affect outcomes  

It was created as part of a school assignment focusing on simulation, GUI development, and clean code principles.

```md
