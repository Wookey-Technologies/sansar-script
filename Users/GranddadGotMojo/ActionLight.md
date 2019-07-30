## Overview

The Light Controller is a script to control various effects you can use with a light.  You can program up to 18 light effects that can be called using Simple Script Events.  Each light effect is set using a control string.  You can use this to be able to change the properties of a light by sending simple script events using chat commands or any other trigger controller.  At the end of this wiki, you will see how it can be used to create disco lights that a DJ could control via chat commands.

## Configuration

The light controller script is placed on a 3d model/object like a can light.  The script is used to control the lighting properties.  Other scripts like the Animation Controller Script can be used to control moving the light like in the Disco Light example below.

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/light1.png)

The Light Controller can control up to two lights in the object.  In the disco example below, a spot light and a point light was used.  The spot light was the main light coming out of the can light and the point light was used to create the effect of the bulb lighting the interior or the can.  

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/light2.png)

The Light Controller Script uses control strings that act as small programs for the light controller.  These strings provide new properties for the light.  Only one control string at a time can be active.  You change the control string to use by sending simple events using chat commands or other triggers.  

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/light5.png)

The Light Controller Script Control Strings have the following parameters that can be set:

* **Event Name** - the light control string will be executed when it "hears" this simple event.
* **Light Intensity** - the intensity of the light.  From 0 to 100.
* **Light Color** - the color is set using the next four parameters.  
  * Light Mode1 above is an example of how to set a specific color for the light.  The fields are 1, 0, 0, 1.  It is using a structure called Sansar.Color that means the first value is the red value, the second value is the green value, the third value is the blue value and the fourth value is the alpha value.  These values must be between 0 and 1.  Since the Red Value is 1 and the Green and Blue values are 0 this means the light will be red.  Any color can be achieved using this Red/Green/Blue (RGB) value setting approach.  The Alpha value opacity of the color with 0 being transparent and 1 being opaque.  
  * Light Mode5 above is an example of using the random feature for setting the light color.  You call this functionality by putting the word random in the first (red) parameter.  It then ignores the next 3 light color parameters.  Using the word random means that light will be set to a random color at run time instead of you setting it to a specific color.
* **Light Angle** - is used for spot lights.  This is the cone angle of the spot light.  10 would be a tight spot light and 90 would be a wider spot light.  Valid values are 0.1 to 160.
* **Light Angular Falloff** - sets the edge of the spot light.  Valid values are 0 to 1.  A low value makes the edge of the light cone very sharp and defined.  A higher value will make the edge of the light cone fuzzier/softer and less defined.
* **Light Effect** - is made up of the next three parameters and it is used to control a function inside the Light Controller program.  The first of these parameters is the name of the function, the next two parameters pass values unique to the function.  These functions are primarily used to change the appearance of the light over time.  The current functions are:
  * **spot** - the spot function will just display spot light using the configured intensity and color.  It uses the first parameter after the keyword "spot" to set when to change the color if the random color approach was used.  For example, Light Mode5 will select a random color and change it every 1 seconds because the last parameters was set to 1.  Light Mode1 just sets a red light and does not change over time.
  * **blink** - the blink function will turn the light on and off based on the time that is identified in the first parameter after the keyword "blink".  You can blink it very fast to get a strobe like effect.  Light Mode2 is an example of a blinking light that changes every 0.5 seconds and each time it blinks it changes the light color because the random color approach was used.
  * **fade** - will just fade in the light.  It is controlled using the next two parameters after the keyword "fade".  The first value is how long to fade in the light and the second value is the time between steps in the fade in process.  Light Mode3 will fade in the green color light over 3 seconds changing the intensity every .1 seconds (for 30 steps).  
  * **pulse** - is much like fade, but, it fades in and out the light in a looping fashion creating a pulse light effect.  Light Mode4 will pulse a different color light every 0.5 seconds changing the intensity every .1 seconds.    

## Making it a Disco Light

A DJ type Disco Light can be made by configuring the Light Controller, adding an Animation Controller and then adding some Chat Commands to control which light and animation to use.  

The first thing to be added is an animated light.  It is added as a keyframed object.  Then the Animation Controller (NPCAnimations) is added.  This script is available on the store for free too.  The Animation Controller is documented on this wiki too if you are interested in the parameters and what you can do.  It is applied to the DJ Light that is an animated fbx which is also available on the store.

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/light3.png)

It this example, Animation Clip1 is called dj1spin and it spins the light frames 1 to 40 at a normal speed of 1 and loops it.  Animation Clip2 stops the light and only plays 1 frame which means it does not move.   Animation Clip3 is called sweeps the light from left to write using frames 10 through 30.  Animation Clips 4 and 5 work together to perform the same ping pong effect.  These two clips are shown just to show how you can string together animations by calling animations in a series.  See the Animation Controller documentation for more details.

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/light4.png)

The light is controlled via the chat window using the TriggerMultipleChatCommands script that is also available on the store for free.  In this example, all the animation commands and light commands are enabled to be listened for in chat.  So, if you type /dj1spin it will start Animation Clip1, if you type /pulse the light controller will start the light to pulsing, etc.

**Disco is Still King!**  
