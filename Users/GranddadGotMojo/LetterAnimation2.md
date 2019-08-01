# Letter Animation

This script runs inside of an animated object that has the 37 panes in it.  The 26 letters of the alphabet, 10 numbers and a blank tile.  The animation is 37 frames long with each frame bringing a different letter/number to the front of the animation.  This script allows you to select the letter that is displayed.  

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/LetterAnimaton.png)

**Letter Event** - The Grouping this letter belongs to.  Like a word or row on a display screen

**Letter Position** - the letter position within the grouping.

**Enable Event** - event listened for to start the script.  By default the script starts and doesn't need the enable event in most cases.

**Disable Event** - event listened for to stop the script.

A good example how to use this would be a scoreboard.  The following shows the scoreboard in edit.  It has 8 rows with a number of tiles for letters.  Each "0" on the scoreboard is a single animated object with this script running it.  

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/LetterAnimation2.png)

This is how it looks in the Experience when you visit it.  Notice how all the tiles have been set by their respective embedded Letter Animation Scripts.  

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/LetterAnimation3.png)

How this works is that another script is sending all the messages telling these letter tiles to play a specific frame in the animation to bring the right letter to the front.  An example of this type of script is the LettersDisplayPhrase.cs script contained in this documentation.  Basically, it sends a message LetterSent0 custom reflective message containing the letter to display (in this case the first letter in the first row which is listening to it), the letter R to display.  The LetterAnimation2.cs script then plays the frame 17 in the animation that has the pane with the Letter R moved to the front by the animation.  The second letter in the first row is listening for the LetterSent0 message too, but was configured to have a letter position of 1 instead of 0 and it is told to display the letter A.
