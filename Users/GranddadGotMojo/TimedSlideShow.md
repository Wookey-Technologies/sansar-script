The Timed Slide Show Script is used to play slides that have delays between the slides.  For example, it could play slide 1, wait 5 seconds, play slide 2, wait 10 seconds, play slide 9, wait 1 second, etc.  You can change the order of the slides to be played and the delay after each slide.  It has the ability to play 20 slides.  You put the graphics/textures you want to use on the slide by putting the Slide Show Player mesh into the scene and then changing the materials to the textures you want by choosing them to upload from your computer.  You can sync it to music by adding a simple sound script and using the same starting event to start both the Slide Show Script and the Simple Sound Script.  This makes it fairly easy to set up a Karaoke player.  

Videos:

* [Video of Karaoke using Timed Slide Show Script](https://www.youtube.com/watch?v=F7UjviTiLGU&t=25s)
* [Video of How To Set Up Timed Slide Show](https://www.youtube.com/watch?v=SIYd5Hf43tQ&t=3s)

![](https://github.com/mojoD/Sansar/blob/master/images/Slide1.PNG)

### Loading Materials

You change the slides you want to show by changing the textures once the Slide Show Player has been placed in a scene.

* **Step1** - After placing the Slide Show Player in the Scene, right click the Slide Show Player in the scene objects.  Select the Materials option on the selection menu.  This will bring up the Materials Settings.

![](https://github.com/mojoD/Sansar/blob/master/images/Slide2.PNG)

* **Step2** - choose the slide texture you want to change.  Scroll down on the drop down selection menu and choose "Custom Texture File" and hit enter. 

![](https://github.com/mojoD/Sansar/blob/master/images/Slide3.PNG)

* **Step3** - Change the Albedo Map by selecting browse button.  This will bring up a Windows File Select Dialog for Textures to upload from your PC.

![](https://github.com/mojoD/Sansar/blob/master/images/Slide4.PNG)

* **Step4** - Select the Texture file you want to load into the Slide you are working on.  For best results, your texture should be a PNG file, with dimensions that are divisible by 4.  Also, the screen is a 4:3 ratio.  So, your width should 1/3 bigger than your height.  Typical 4:3 settings are 640 X 480, 1280 X 960, etc.

![](https://github.com/mojoD/Sansar/blob/master/images/Slide5.PNG)

* **Step5** - After you have chosen your file it will be added to the Materials Settings.  You can change any of the other maps at this time as well.  Choose Save Settings to add Texture.

![](https://github.com/mojoD/Sansar/blob/master/images/Slide6.PNG)

* **Step6** - After you have Saved the Material Settings the Import Textures Screen will be shown showing any errors with the upload or verifying it loaded correctly.

![](https://github.com/mojoD/Sansar/blob/master/images/Slide7.PNG)

### Configure Simple Collision Script in Trigger Volume

![](https://github.com/mojoD/Sansar/blob/master/images/Slide8.PNG)

A Simple Collision Script is placed in a Trigger Volume.  When a person enters the Trigger Volume this script will execute.  You choose the Messages to send when the person enters the Trigger Volume and when they leave it.

* **On Agent Collide =>** - Enter the message/event that will be sent when a person enters the Trigger Volume and triggers the Collision Script.  In this example, SlideShowOn is the message sent.

* **On Agent Exit =>** - Enter the message/event that will be sent when a person leaves the Trigger Volume.  In this example, SlideShowOff is the message sent.

### Configure Timed Slide Show Script in Slide Show Player

![](https://github.com/mojoD/Sansar/blob/master/images/Slide9.PNG)

This script starts when it receives the Play Event and stops when it receives the Stop Event.  It will listen for these events and when it sees them (i.e. a person steps into the Trigger Volume sending the SlideShowOn event) it will start or stop the script.  It will play the Slides (which you saw how to put textures on) based on a slide number and a timer which is the delay before the next slide will be shown.

* **Play Event** - event being listened for that starts playing the slide show.

* **Stop Event** - event being listened for that stops playing the slide show.  When the slide show is stopped it shows the first slide on the Slide Show Player Screen.

* **Slides To Play** - a comma delimited string with no spaces that contains the slides to play in order when the slide show plays.  Slide numbers can be in any order and repeated if necessary.  Only 1 through 20 can be entered as there are only 20 slides in the Slide Show Player.

* **Slide Timing** - a comma delimited string with no spaces that is the delay after each slide in whole seconds before the next slide will be shown.  In this example, Slide 1 is shown for a delay of 11 seconds, then Slide 2 is shown for a delay of 20 seconds, etc.  The seconds are whole integers (no decimals).

* **Loop** - identifies if you want the slide show to loop after it has finished playing all the slides in the Slides To Play with the corresponding time delays in Slide Timing.

### Configure Simple Sound in Slide Show Player

![](https://github.com/mojoD/Sansar/blob/master/images/Slide10.PNG)

A common use of a Timed Slide Show is to have it Synchronized with Audio (either a narrative or music).  To do this, you can use the Simple Sound Script which is also placed in the Slide Show Player object.

 * **-> Play Sound** - is the message that the script is listening for to start the audio.  To sync with the Timed Slide Show Script it will listen for the same message from the Trigger Volume (i.e. SlideShowOn).

* **-> Stop Sound** - is the message that the script is listening for to stop the audio playing.  To sync with the Timed Slide Show Script it will listen for the same message from the Trigger Volume (i.e. SlideShowOff).

* **Sound** - the audio file that has previously been loaded into your inventory.  You choose it from a drop down list.

There are many other settings that can be chosen, but, the default settings are fine to build a simple Karaoke type player.
© 2019 GitHub, Inc.
