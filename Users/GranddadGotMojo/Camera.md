## Overview

The cameraman script works by taking control of an avatar and a disc that the avatar is standing on.  It teleports the avatar and disc to a position and then rotates the disc to rotate the view point of the camera.  Currently you cannot directly rotate the avatar so this disc is rotated to adjust the point of view of the avatar camera after it has been teleported.  An avatar and disc is moved to the predefined position when it receives a simple event that has been assigned to it.  This simple event can be sent by using simple chat commands, simple collisions or PC Keys.

There are two basic roles when using the Cameraman script.  The first is the Cameraman.  This is the avatar that is standing on the disk and acts as the Camera.  The second is the Director.  This is the person that is issuing the Cameraman positions to move to.  These can be two different avatars or the Cameraman can also be the Director (they control where they are moved).  In any case, even if you are wanting to be both the Director and Cameraman you must Trigger both Roles via stepping into the separate Director and Cameraman Trigger volumes.  

There are three things you must get from the store to get the entire Camera System.  The Camera - Set Director item, the Camera - Set Cameraman item and the Camera - Platform.

## Configure Camera Positions

You can predefine up to 18 camera positions to move the cameraman and disc to.  The format of the comma delimited control string is Event Name, the X position, Y position, Z position and Z rotation.  The X, Y and Z Positions are in world coordinates and not relative coordinates.  The word dummy was put in for a future feature not yet implemented but still needs to be in the configuration.  

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/camerconfig.png)

Here is an example of how to send commands using Simple Chat Commands:

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/chatcommands.png)

You can also set PC Keys to send the commmands.  Use this script and set up if you want to control the camera via PC keyboard commands:

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/key1.png)

## Assigning a Director

The Cameraman is assigned in an experience by being the avatar that issues an event named SetDirector.  This is most easily done by setting up a Trigger Volume and configuring a Simple Collision to send the event (which by default sends the Cameraman's Avatar ID to the Camera Script.  The Avatar that wants to become the Cameraman then walks into the Trigger Volume and they are assigned as the cameraman. 



## Assigning a Cameraman 

The Cameraman is assigned in an experience by being the avatar that issues an event named SetCameraMan.  This is most easily done by setting up a Trigger Volume and configuring a Simple Collision to send the event (which by default sends the Cameraman's Avatar ID to the Camera Script.  The Avatar that wants to become the Cameraman then walks into the Trigger Volume and they are assigned as the cameraman.  

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/SetCameraManScript.png)

For convenience, it is best to also include a local teleport script with the XYZ coordinates of the initial position of the disc to place the cameraman on the disc to start with.  

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/initialteleport.png)

## Using the Camera

Once the Cameraman has gone into the Trigger Volume they are teleported to the disc.  They should then go in first person and for the most part not move around beyond adjusting the position they are looking at using the right mouse.  They should be in desktop mode and not VR mode.  A typical scenario is the Director (one controlling the Cameraman) should issue the Camera Position Events.  Here is a short video showing what is going on when commands are issued:

[Short Video on Using Cameraman Script](https://www.youtube.com/watch?v=ZIOoojrF_Ek)

