# Trigger Complex Interaction Rectangle

This script is meant to be placed in a 3d model object.  You define logical control surfaces for that 3d model in terms of rectangles that have x,y positions that are relative to the center of the model.  It maps this logical control surface, so, that if the user interacts with that portion of the model it will send a message that you define.

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/RectangleControlSurfaces.png)

In this example, a 3d model was created that will be used as a keypad.  A logical control surface was determined 12 rectangles representing a typical keypad layout.  The four X,Y coordinates for each corner of the rectangular control surface is entered as a string in the Trigger Complex Interaction configuration in the following format (MessageToSend, Relative X1, Relative Y1, Relative X2, Relative Y2, Relative X3, Relative Y3, Relative X4, Relative Y4, Z-Minimum and Z-Maximum).  The Z-Minimum and Z-Maximum allow you to stack control surfaces at different heights so that you could overlap control surfaces as long as they were at different heights.  You can also use this to be able to rotate the model to work in both the laying down or standing up positions with this script.

**Complex Interaction** - message you want to appear when a person hovers over the model before interacting with it.

**Cur Pos** - the current position of the model.  This must match the position of the model in your experience.  This has to be entered because the current API incorrectly reports the X,Y,Z coordinates of the model to scripts.

**Z Rotation** - the current Z rotation of the model.  This must match the  rotation of the model in your experience.  Like the Cur Pos setting this is needed because of the API not reporting rotation correctly to scripts.

**Off Timer** - this allows for an additional "off" message to be sent if this is set to a value greater than zero.  For example, if the message to send is Surface3 based on the user hitting the 3rd control surface, then 1.2 seconds later (the value in this example) a Surface3Off message is sent.  If the Off Timer had been set to 0 then no SurfaceOff message would have been sent.

**Debug** - when this is set to On and you hover over the 3d model with the script in it, the current position and rotation of the cursor will be shown on the screen.  This allows you to fine tune the control surface settings by seeing what is actually happening when using this script in Sansar.

**ControlSurface1 thru ControlSurface12** - these define the logical control surfaces for the 3d model that the script is attached to.  
