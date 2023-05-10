# CSC3094: Procedurally Generated Planets with Quadtree LOD
## Overview
This repository contains a Unity project for generating procedurally generated planets using quadtree LOD. The project is developed using Unity version 2022.1.3f1.

## Usage
To use the project, simply download the Unity project files and open the project in Unity. The Assets folder contains the following:

- A demo scene with three planets, which can be explored in play mode.
- Two main MonoBehaviour components for generating planets:
  - PlanetPreview: allows users to manipulate the planet within the Unity editor.
  - PlanetRealtime: generates the planet in real time using multithreading and quadtree LOD. This component is for use in play mode only.
- A ColourSettings and a ShapeSettings file need to be assigned for each component. These are saveable planet assets.
The PlanetPresets folder contains three examples of saved planet assets.
- The main source code is located in the Scripts folder, which contains the PlanetPreview and PlanetRealtime components, as well as the ProcGen Planet folder that contains the most important code, such as the quadtree implementation, noise, and multithreading.
- The Planet shader can be found in the Shaders folder.

## Requirements
- Unity version 2022.1.3f1 or newer is required to run the project.
