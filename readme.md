# Uncertainty Visualization Tool

A Unity application for testing visualizations for a party island resource maximization scenario.

## Table of Contents

- [Usage](#usage)
- [Extension](#extension)
- [Support](#support)

## Usage
Supports any version of Unity3D >2017.2.0.

To get started, clone the repository and open with Unity3D game engine. There are three scenes which form the basis of the application:
- BuilderScene2: Scene for building up new scenarios by adding new assets, crates, changing visualization types. Supports local file IO.
- ExperimentScene2: Scene for loading previously constructed scenes (press LShift+O to open file dialog).
- StudyFlow: An example of a full fledged study with input logging and data forwarding. Built to be usable in a WebGL context on Qualtrics. Most logic is in two scripts (StudyFlowControl, LogHelper). The scenario files are stored as strings within StudyFlowControl.cs, instead of as local files, and use a different function (OpenStream vs OpenFile).

### Building a Scene
The buttons in the scene are self explanatory. To change all visualizations simultaneously, use the number keys. Alternatively, a single visualization can be toggled through by holding left shift and clicking on it. By clicking and dragging on the visualization, it can be rotated around the origin. Objects can also be dragged around the map. 

## Extension
One highly desired area of extension is changing the visualizations to something more complex. To do this, first build a prefab of the new visualization. Examples of the proper orientation, scale, and attached scripts for the prefab can be found under Prefabs/VisualizationTypes. Next, edit the ConeHelper script to properly point towards this new prefab by adding it to the prefab list in the ChangePrefabToType function. Don't forget to add appropriate code for changing the transparency, if applicable.

Note that there are a few places within UserInputManager.cs which make changes to the visualizations being presented based on the name of the scene and visualization being loaded from the scenario strings. See functions Update() (line 99)  and TimeStepChangedLerp() (lines 916-927) for examples.

## Support

Please [open an issue](https://github.com/ISUE/UncertaintyViz/issues/new) for support, or contact Corey <cpittman@knights.ucf.edu>.
