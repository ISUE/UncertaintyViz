# Uncertainty Visualization Tool

A Unity application for testing visualizations for a party island resource maximization scenario.

## Table of Contents

- [Usage](#usage)
- [Modification](#modification)
- [Support](#support)

## Usage
Supports any version of Unity3D >2017.2.0.

To get started, clone the repository and open with Unity3D game engine. There are three scenes which form the basis of the application:
- BuilderScene2: Scene for building up new scenarios by adding new assets, crates, changing visualization types. Supports local file IO.
- ExperimentScene2: Scene for loading previously constructed scenes (press LShift+O to open file dialog).
- StudyFlow: An example of a full fledged study with input logging and data forwarding. Built to be usable in a WebGL context on Qualtrics. Most logic is in two scripts (StudyFlowControl, LogHelper).

## Extension
One highly desired area of extension is changing the visualizations to something more complex. To do this, first build a prefab of the new visualization, and then change the prefabs pointed to by the ConeHelper script. Then, add in the necessary code to help it cycle through that visualization appropriately.

## Support

Please [open an issue](https://github.com/ISUE/UncertaintyViz/issues/new) for support, or contact Corey <cpittman@knights.ucf.edu>.
