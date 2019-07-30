# Logic Sequence Checker Multiple

The **Logic Sequence Checker Multiple** script checks against multiple codes and sends out a messages whether it matched or didn't match a code.  Unlike the original Logic Sequence Checker this one does not send out messages after it receives each event (i.e. element in a code), but, rather it waits until the entire sequence has been delivered and then checks against all the sequences that are configured to test for.    

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/LogicSequenceCheckerMultiple.png)

**Sequence Name** - the name of the Sequence.

**Base Event Name** - the prefix of events that this script is listening for.  In this case it is "code".

**Number of Events** - The number of events to listen for.  In this case it is 12.  This means that the script will be listening for events with the names "code1" thru "code12".

**Sequence** - a comma delimited string that contains the sequence to check.  In this first example (Sequence1), "Match1,Fail1,code1,code2,code3,code4".

**Matched Event** - the first field is the name of the event that is sent if the incoming events match this Sequence.

**Failed Event** - the second field is the name of the event that is sent if the incoming messages do not match the Sequence.

**Codes To Match** - all the entries after the Failed Event is the code to be actually matched.  This script is paired with a Trigger script that would be sending events.  For example, it could be a keypad trigger script that allows an person to press keys on it that send messages associated with the numbers on the keypad.  The avatar would press the 1 key on the keypad and it would be programmed to send "code1" and so on for 12 keys on the keypad.  

The logic sequence checker would be listening for these events and if the person clicked key1, key2, key3 and key4 in succession it would match the Sequence1 that the Logic Sequence Checker is checking against and the Logic Sequence Checker Script would send the Matched Event which in this case is "Match1".  Another Script, say a door script, could be listening for this "Match1" event and open a door.  It also goes and checks the other non-blank sequences (i.e. Sequence2, Sequence3, Sequence4).  Since none of these match the incoming code sequence their failed Events are sent.  In other words, four messages are sent Match1, Fail2, Fail3 & Fail4 when checking the first code.  

All the codes entered in the Sequence strings must have the same number of code elements.  For example, Sequence1 has 4 code elements entered (code1, code2, code3, code4).  This means that the script expects all the Sequences to have 4 code elements too.  After it has received the four elements from the sending script (i.e. keypad), it checks to see if any of the Sequence Strings match the four code elements and then it resets the script to listen for the next 4 elements.  In other words, you enter a code of 4 numbers the script processes and then resets so that you can enter the next 4 codes for it to check.  Remember it is not limited to 4 codes, but rather, however many code elements you have in the Sequence1, the other Sequences have to have the same number of code elements.  
