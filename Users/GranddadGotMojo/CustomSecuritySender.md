# Custom Security Sender

This script provides a general way to pass lists of users to other scripts that apply this security.  For example, you would use this script to identify a name of the security list and the people that you are adding to the list.  Then another script would be listening for a security reflective message to be sent identified in the Event to Send Security public variable by this script and then have internal scripting that implemented that security (i.e. ignore or execute commands when pushing a button).

![](https://github.com/mojoD/Sansar-Simple-And-Reflex-Script-Integration/blob/master/images/CustomSecuritySender.png)

**Valid User List** - Users on the Access List.  This is either a list of Avatar names separated by commas or the keyword ALL which means no security is being applied.

**Event to Send Security** - name of the Security List that is being listened for by other scripts that actually implement the security.
