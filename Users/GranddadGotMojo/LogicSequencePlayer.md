# Logic Sequence Player

The Logic Sequence Player will send a group of messages at intervals determined by a set of of timing delays.  This can be used to play songs, control lights, move objects or any other actions that need to be controlled via timed messages.

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/LogicSequencePlayer.png)

**Trigger Event** - this is the event that the Logic Sequence Player is listening for to start the sending of messages.  In this case, when it receives the PlaySequence message it will being sending the messages from the Sequence String.

**Sequence Name** - this is the base event name that will be added to the Sequence Events to make up the name of the message.  In the example, it is "Beethoven".  That means that the first message sent went would be Beethoven7 followed by Beethoven7 all the way to the last event Beethoven3.

**Sequence** - the suffixes to be used when sending the messages.

**Timing** - determines when to send the message.  It is a delay.  It is in seconds.  In this case, the script will send the "Beethoven7" message, wait .33 seconds, send the second message "Beethoven7", wait another .33 seconds and so on for the number of events.
