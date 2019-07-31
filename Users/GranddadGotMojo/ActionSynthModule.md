# Action Synth Module

This module is the heart of my Synth Instrument that plays notes using an in world keyboard with your mouse or VR controllers, as well as, the PC Keyboard for input.  This documentation is not meant to show you how to wire up this module with your own instruments, but, only a gentle guide to the code that you can look at for ideas on how musical instruments work in Sansar.

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/ActionSynthModule.png)

**Input Channel** - this is the channel the synthesizer is listening to.  This is paired with the keyboard keys that also have a setting for input channel.  This allows you to have more than one synth in an experience with each using a different input channel.  It is set to **K1** in this example.

**S#SoundResources** - with the # being a number 0 through 7.  The number represents the octave on the keyboard.  This is where you identify the sample to use for appropriate note in the octave.  For example, S3SoundResources represents the samples for the 3rd octave on the keyboard.  The Sound Resources use a List for input and the Action Synth Module script expects 12 entries for each octave list.  These represent the 12 notes in an octave.  The 0 entry of this list is a C note, the 1 entry in the list is C#, the 2 entry is D, etc.  The Action Synth Module uses both the Sound Resource and the Pitch Offset to determine what note to play.

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/S3SoundResources.png)

**S#Offset** - with the # being a number 0 through 7 like the SoundResource List.  Each octave has a list for the notes in the octave and the Offset is the semitone pitch offset from the Sample identified in the previous list. 

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/S3Offset.png)

For example, in the above example, you have two samples that are in the 3rd octave.  I name my samples with the Instrument followed by the Pitch and the Octave.  So, the two samples are for a Steel String Guitar and they are named Steel D3 and Steel A3.  The SoundResources are set to these sample audio files.  The offsets provide information to get the right pitch.  For example, the first Sound Resource is Steel D3 meaning it is using D3 as the starting point.  The Offset is -2 which means if offsets from D3 by two semitones which will result in a C note being played.  The second offset is -1 meaning the second note will be C#, the third has 0 offset meaning it will just play the sample and you will hear a D note, the fourth note is a offset by 1 which means it will play a D#, etc.

**Enable Event** - the Action Synth is listening for an event named this to load in the samples and offsets to play the instrument (in this case a Steel String Guitar).  This event is sent by the Synth Controller Script that allows you to select different instruments or patches to play using the keyboard.  

**Disable Event** - this is the event to turn off the synth.

