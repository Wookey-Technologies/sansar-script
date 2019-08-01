# Action Two Step Mover

This script is embedded in an object and moves the object based on simple script messages that it receives.


![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/ActionTwoStepMover.png)

**Position Offset** - this is the XYZ and offset that it will move the object when getting the On Event. In this case, it would move the object 2 meters up (Z position is set to 2 meters) when it sees a MoveUp simple message sent from another script.

**On Event** - this is simple message that will move the object by the position offset.

**Off Event** - this is the simple message that will move the object back to the latest position before the offset was applied.

**Seconds** - how long it takes for the move to occur in seconds.  In this example, the object would move 2 meters over 2 seconds when it saw the On Event.

**Update From Move Position** - acts as a toggle.  If it is set, then the object will move from the new offset position on the next on Event.  If it is off, this means that will start the move from the initial position of the object.

**Start Moved** - this means that it will start with the object already moved.

I often use this to create buttons using two objects with one of the objects having an emissive material so it looks like when you select the button it turns on.  I also can be used to move an object from position to position using the Update from Move Position toggle.
