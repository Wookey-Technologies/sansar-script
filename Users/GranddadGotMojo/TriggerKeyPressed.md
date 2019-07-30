# Trigger Key Pressed

This script will send one of 40 key presses from your PC Keyboard as messages.  It assigns 40 keys from your PC Keyboard to event numbers:
* Keys 1 thru 0 on the top of your keyboard (event number 1 – 10)
* Number Keys on Keypad (event number 11 – 20)
* Shift Key + Keys 1 thru 0 on the top of your keyboard (event number 21 – 30)
* Shift Key + Number Keys on Keypad (event number 31 – 40) 

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/TriggerKeyPressed.png)

**Base Event Name** - the prefix for the messages being sent.  In this example it is set to "KeySend".  That means if the person pressed the key number 4 on the top of the PC Keyboard, the event KeySend4 would have been sent.  If they had hit the keypad number 5 the event KeySend16 would have been sent.

