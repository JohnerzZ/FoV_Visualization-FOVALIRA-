# Visualizing the Field of View of Optical or Lidar Sensors
# Using the Unity Development Platform
---
## Overview

The process of designing a perception system for a robot involves deciding what sensors are going to be used and what their position and rotation on the robot will be. The ability to visualise the combined field of view produced by camera and lidar sensors on the robot in simulation assists with such design decisions. There are several field of view visualisation tools freely available online but none of them is detailed, realistic or versatile enough to effectively compare multiple perception system designs in 3D space. Therefore, the goal of this project is to develop a three dimensional field of view visualisation tool and then use it to compare multiple perception system designs and reach a final decision on the design that will be used on our quadruped robot.

## Installation

1. Download and install the [Unity Development Platform]. The version of Unity used for this project was Unity 2020.3.27f1 (64-bit).
2. Download the FOV_Visualization Tool:
```sh
git clone https://bitbucket.org/csl_legged/mini-project-fov_visualisation/src/master.git
```
3. Open Unity Hub. On the projects tab, click Open, navigate to the project folder (probably named "master") and open it.
4. With the project open, go to the toolbar on top and click File -> Open Scene. Choose the Commit2.unity scene located in the Assets Folder.

## Quick Demo

1. To the left of the scene tab, on the Hierarchy tab some object names can be seen in fainted white color. These objects are also present in the scene but they are deactivated. The user can select any of these from the Hierarchy tab. Thus, the Inspector tab on the right side of the window will display information about the selected object. To activate the selected object, the user can check the checkbox near the name of the selected object in the Inspector tab.
2. Activate the objects Sensor, Sensor (1) and Sensor (2).
3. Start the simulation by pressing play (the play button is usually above the scene tab).
![live_scene](images/live_scene.png)

## Instantiate sensors

To spawn sensors in determinate positions on the scene, the instantiation.txt file should be used.

1. Open the Commit2.unity scene (If it is not already open).
2. Deactivate the objects Sensor, Sensor (1) and Sensor (2) if they are active.
3. Configure the instantiation.txt file according to the desired experiment. For more information, read the "Configure" section in the report. A typical instantiation configuration is written in typical_instantiation.txt. The user can copy its contents in instantiation.txt as a demo. If the instantiation.txt cannot be configured while unity is opened, then you might need to close it to configure the file and then reopen it.
4. Press play to start the simulation.

[Unity Development Platform]: <https://unity3d.com/get-unity/download>
