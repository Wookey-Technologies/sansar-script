# VR Controllers

There are four VR Controller Scripts.  Both of them work inside a Trigger Volume.  The first one is SensorVRController reports your hands positions at a time interval you choose.  The second one TriggerVRController reports your hands positions based on when you press and release the VR Control Triggers.  
The third one 
## Sensor Controller called TriggerVRControllerSendVectorMsg is a version of TriggerVRController that send VR Controller Information in a custom Reflective Message.  The fourth one ActionVRControllerReceiveVectorMsg shows how you would have a script that would listen for the message and then do something like move the object, change the light, play a sound, etc.

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/SensorVRController.png)

**Time Delay** - reports the position of the hand controllers every Time Delay milliseconds.  For example, if this is set to 1000 it reports the hand position every second (1000 milliseconds in a second).

This script is meant to be modified by the creator for some interaction.  It has stubbed in the code to generate a simple script event.  Now it just writes log messages.  It is assumed that you would either modify the script and put the action you want to happen at certain hand positions directly in the code or have it send a simple message event to have a nother script do something based on the hand positions.

## Trigger Controller

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/TriggerVRController.png)  

Works much like the Sensor Controller, but, rather than continuously report like the Sensor Controller it only reports based on the if the trigger is pressed.  

**Time Delay** - reports the position of the hand controllers every Time Delay milliseconds if the VR Controller Trigger is pressed.  For example, if this is set to 1000 it reports the hand position every second (1000 milliseconds in a second).

**MultiClick** - if this to On this means that it will report the hand position as long as you have the Trigger pressed every Time Delay milliseconds.  If this is set to Off it will only report the position once per Trigger press.
