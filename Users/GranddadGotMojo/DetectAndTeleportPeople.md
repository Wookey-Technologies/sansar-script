## Overview

This is a script that is placed in an object that detects the people within a certain number of meters of the object and then teleports those people to a local destination in the Experience.

## Configuration

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/DetectAndTeleport.png)

The following are parameters you set in order to configure the Detect and Teleport Script.

* **Detect Event** - the name of the event that triggers this script to run.  It is listening for a Simple Script Event of this name.
* **Avatars Detected** - it sends this event if it detected any avatars in the detection range.
* **No One Detected** - it sends this event if no avatars were detected within the detection range.
* **Detection Range** - Meters from the object that this script is in that will cause the avatar to teleported.
* **Destination** - X,Y,Z coordinates in the experience to send send the detected avatars to.

