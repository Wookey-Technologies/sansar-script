# Trigger Proximity Detector

This script is put in an object and it senses when avatar is within a certain range of that object.  It measures range from the center of the object.  When the user is within that range it sends a message you define.  Typically you would use this script in an object if you wanted to start another script when a person was nearby, but, you didn't want to have to use a Trigger Volume to start the script.  It was derived from a Simple Script that Gindipple originally wrote.

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/TriggerProximityDetector.png)

**On Detection** - message to send if avatar is within range.

**Seconds to Ignore** - currently not used.

**Seconds between scans** - it only scans as often as you set this property to.  

**Detection Range** - how many meters away does the Avatar have to be before it is in range.  This is measured from the center of the model the script is in.

**Enable** - this script will start when it receives this message.

**Disable** - this script will stop if it receives this message.

**Start Enabled** - whether the script should start enabled thereby not requiring a message to start it.
