# Overview

The Movement Controller Script is written to be able to control the physical movements of an object in terms of position and rotation over time.  You identify a series of Movement Instructions which identifies the relative position to move the object to and the rotation of the object over a period of time based on receiving an event to start the Movement playing. Also, you can define a "Done Event" that will be sent when the Movement has completed.  

# Configuration

The Movement Controller Script uses comma delimited strings called Movement Instructions that define how the object is to move over time.  There are up to 18 Movement Instructions that can be set for the script.  The Movement Instruction has the following format:

* **Movement Event Name** - the defined movement will happen when it "hears" this event.
* **Movement Done Event** - when the movement finishes this event is sent.
* **X Position to Move To** - the X Position to Move to relative to the current position of the object.
* **Y Position to Move To** - the Y Position to Move to relative to the current position of the object.
* **Z Position to Move To** - the Z Position to Move to relative to the current position of the object.
* **X Rotation** - the number of degrees on the X axis to rotate the object relative to the current position of the object.
* **Y Rotation** - the number of degrees on the Y axis to rotate the object relative to the current position of the object.
* **Z Rotation** - the number of degrees on the Z axis to rotate the object relative to the current position of the object.
* **Movement Duration** - the number of seconds in which to accomplish the movement.  This can be whole seconds and parts of a second (i.e. 1.45 seconds).

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/Movement2.png)

In the above example:    

* When the Script "hears" the Move1 event it moves the object to a position 10 feet forward on the X axis and turns it 45 degrees.  This is done over a 2 second time duration.  When complete it sends a Move2 event.  
* The Script "hears" the Move2 event (it can receive events from other scripts or from itself) and moves the object 10 feet to the right.  There is no change in rotation, so, the object is still rotated 45 degrees.  If you wanted it to rotate it back to 0 you would have put -45.  The movement is done over a 2 second time duration and when it completes it sends a Move3 Event.
* The Script "hears" the Move3 event and moves the object 10 feet backwards.  There is no change in rotation.  The movement is done over a 2 second time duration and when it completes it sends a Move4 Event.
* The Script "hears" the Move4 event and moves the object 10 feet to the left.  There is no change in rotation.  The movement is done over a 2 second time duration and when it completes it sends a none event that nothing is listening for.

This click is kicked off by a Simple Collision which send the initial Move1 Event.  Another way to control this movement would be through the a script that sends event messages on based on time delays.  The Logic Sequence Player is such a script.  In this case you would have a Movement Controller Script that didn't chain together the movement via Done Events, but, rather the moves were "orchestrated" via the Logic Sequence Player.  Here is the set up for this type of movement:

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/Movement.png)

The Logic Sequence Player setup:

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/LogicSeqCheckForMovement.png)

[Short Video Showing Movement Controller In Sansar](https://www.youtube.com/watch?v=77oUKXH9nNI)
