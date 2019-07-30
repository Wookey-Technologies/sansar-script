# Overview

The Animation Controller Script is used to be able to play portions of an animation based on simple script events.  This script was originally intended to be used in the following scenario.  An animator has several animations for a Non-Player Character (NPC), such as, idle,  sit, play guitar, stand up, etc.  They combine them into a single animation in their 3d creation tool (blender, 3ds Max, Maya, etc).  The export them as an FBX and import them into Sansar.  Now you want to be able to play these animations based on commands you send it.  These commands could be sent via chat, PC keyboard or any other Simple Script Trigger.  This script enables you to do this.  Also, this script allows you to string together a series of animations playing one then another and then another (i.e. when one portion of the animation finishes it can call another portion of the animation).  

# Configuration

The Animation Controller Script uses Animation Clips to animate.  These animation clips are strings that you configure that tells the Script a range of frames of the animation to play and how to play it.  The Animation clip has the following format:
* **Animation Event Name** - the clip will run when it "hears" this event
* **Animation Done Event** - when the clip finishes this event is sent
* **Start Frame** - the frame to start the animation playing from
* **Last Frame** - the frame to play the animation to
* **Playback Type** - how to play this animation clip.  The choices are:
  * **playonce** - Plays the clip just one time
  * **loopX** - Where X is a number like Loop5.  Plays the animation X times.  Loop5 will play the animation 5 times.
  * **loop** - with no number after it will loop the animation until it gets another Animation Event.
* **Playback Speed** - 1 is normal speed.  2 is twice as fast. .5 is half as fast.  Use negative numbers to play backwards.
* **Blend Duration** - not yet implemented.

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/npcanimationsNew.png)

The above configured script is an example of a animated Non Playable Character (NPC) that is playing guitar.  In this scenario, the person controlling the animation would send a chat command to control the animation.  A typical scenario for a performance would be:
* Send standIdle command and the NPC would just stand in an idle animation waiting for a new command (i.e. loop).  The animation is from frame 1 to 200 in the uploaded fbx animation.
* Send the sit command.  Which would playonce and play frames 451 to 600 and transition the NPC from a standing position to a sitting position.  After the sit command finished it issues a sitIdle command (the Done Event).
* This triggers the sitIdle event and it plays frames 300 to 450 in a loop and the NPC looks like it is sitting and idle.
* Send a sitPlay command and it plays frames 601 to 750 in a loop and the NPC plays the guitar while sitting.
* Send a stand command and the NPC stops playing the guitar and it plays frame 600 to 451 (basically plays the sit animation in reverse in order to stand).  It plays this once and then sends the standIdle command.
* This triggers the standIdle command which loops frame 1 to 200 again.
* Send a standPlay command which stops the standIdle from playing and plays frames 601 to 750 so the NPC plays the guitar standing up.
* Send a bow command which loops frames 800 to 950 five (5) times (loop5) so the NPC bows 5 times and then sends a standIdle command which sets the NPC to stand and idle again.

If these commands are sent via chat you would need to set up a the Simple Chat Commands Script with all the commands you are going to use somewhat like this.  (only 3 commands shown here).

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/chatcommands.png)
