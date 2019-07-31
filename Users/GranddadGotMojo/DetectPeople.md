This script is dropped in an object.  It will detect any avatars within a range that you set and send simple messages with the name of the avatar to other scripts that can use that information for whatever you want to.

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/DetectPeople.png)

**Detect Event** - the event that starts the script.  It does a new scan for Avatars every time it sees this message.

**Avatars Detected** - The name of the simple script event that it sends for each avatar that is detected.  You can get the name and other information about the avatar using these simple script messages.

**No One Detected** - it sends this simple script event if no one was within range of the object with this script in it.

**Detection Range** - how many meters from the object it should scan for avatars to be in range to send the simple messages.
