# Letters Display Phrase

This script is used with the LetterAnimation object and it's LetterAnimation2.cs script.  It is used to display phrases on a chat board based on a phrase typed in the chat window.

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/phrase.png)

**Valid User List** - listens for a list of valid users sent by the Custom Security Sender Script.  If this field has the keyword ALL in it, it does not apply a Valid Users security list and any user in the experience can type in chat to display messages on a message board.

**Letter Event To Send** - this is the event that the Letter Animations and LetterAnimation2.cs script are listening for.  In this case, the Letters Display Phrase script will send the custom reflective message Phrase1 along with the letter and position to display.  The individual letter objects have been configured in the message board.

For example, this is a simple 4 letter message board.  To start with it is all sent to blank letters.

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/Phrase2.png)

Each letter is set to display to a message for which letter to display.  For example, the first letter tile is set to listen for the Phrase1 message (which is being sent by this Letter Phrase Display script) and it is looking for the 1st letter in the phrase (i.e. Letter Position 0).  The second tile would have the same Letter Event (Phrase1), but, would be looking for the second letter (i.e. Letter Position 1) ... the array storing them is 0 based.

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/phrase1.png)

The person wanting to display a message on this simple message board does it from a chat command.  The enter the keyword /phrase and then the 4 word message they want to display.  In this case MOJO.

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/Phrase3.png)

When they hit enter the Letters Display Phrase script which is subscribing to chat, gets the message and sends 4 custom reflective messages (one for each letter on the message board).  This results in this displayed on the message board.

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/phrase4.png)

