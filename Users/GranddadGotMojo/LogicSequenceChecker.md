# Logic Sequence Checker

The Logic Sequence Checker can be used for things like pass codes or games where sequence is important.  

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/LogicSequenceChecker.png)

**Sequence Name** - the name of the Sequence.

**Matched Event** - the name of the event that is sent if the incoming messages match the Sequence to be checked which is entered in the Sequence field.

**Failed Event** - the name of the event that is sent if the incoming messages do not match the Sequence to be checked that is entered in the Sequence field.

**Reset Code Event** - the name of the event that will cause the Logic Sequence Checker to reset and being listening to new events to match to the Sequence that is entered in the Sequence field. 

**Base Event Name** - the prefix of events that this script is listening for.  In this case it is "code".

**Number of Events** - The number of events to listen for.  In this case it is 12.  This means that the script will be listening for events with the names "code1" thru "code12".

**Sequence** - a comma delimited string that contains the sequence to check.  In this example, "code1,code3,code8,code5".

This script is paired with a Trigger script that would be sending events.  For example, it could be a keypad trigger script that allows an avatar to press keys on it that send messages associated with the numbers on the keypad.  The avatar would press the 1 key on the keypad and it would be programmed to send "code1" and so on for 10 keys.  The logic sequence checker would be listening for these commands and if the person clicked key1, key3, key8 and key5 in succession it would match the Sequence that the Logic Sequence Checker is checking against and the Logic Sequence Checker Script would send the Matched Event which in this case is "Unlocked".  Another Script, say a door script, could be listening for this "Unlocked" event and open a door.  If it didn't match the Sequence it would have sent the Failed Event (i.e. "Locked") and scripts listening for this would act appropriately.

If the Script determines that the check failed it resets the script so that you can try again.  Also, if it detects the Reset Code Event it will reset the script so you can begin sending messages for it to check.
