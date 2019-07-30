The gotMojo Media Screen is used in Sansar for playing Youtube & other URL based Videos.

## Major Features
* Control via Chat Window or Controls on Bottom of the Screen
* Plays all Types of URL based Videos
* Automatically plays in Full Screen Mode for Youtube Videos
* Ad Free in Full Screen Mode
* Pause Videos Full Screen Videos
* Resume Videos from where Paused for Full Screen Videos
* Stop Video for Full Screen Videos
* Special Force Video to handle certain conditions (try if video returns video unavailable)
* Preconfigure up to 15 videos that can be played via 
    - The play1 - play15 chat commands 
    - The Control Buttons 1 through 15 on the Bottom of the Screen
* Load New Videos via Chat Dialog (/stream URL & /video URL)
* Access Control List where you can set who you want to be able to change Videos in Chat
* Control Volume of Video while playing

## Setting Up the gotMojo Media Screen

The Media Player is preconfigured.  You can change some of the options via script configuration.

![](https://github.com/mojoD/Sansar/blob/master/images/YoutubeViewerConfig2.png)

The fields you can configure are:
* **Script**  - select YoutubeViewer from the drop down list.
* **UsersToListenTo** - type the list of users that you want to be able to control the Media Player.  If you want anybody to control it the type in ALL instead of a list of users.  Defaults to ALL when you initially load it.
* **Height** - Height of the logical screen on the media screen surface.  Only use this if you are not using the script in the gotMojo Media Screen.
* **Width** - Width of the logical screen on the media screen surface.  Only use this if you are not using the script in the gotMojo Media Screen.
* **Play1 - Play15** - enter up to 15 Video URLs to be preloaded to play.

## Using the gotMojo Media Screen

The Media Screen can be controlled via a Chat Window Interface or using the Control Console buttons on the bottom of the screen.  The chat commands to control it are:

* **/video YoutubeVideoURL** - such as _**/video https://www.youtube.com/watch?v=rS1VmaB9JnM**_ This will start the video playing on the Media Screen.
* **/play1** - is an example how to play the preconfigured video in slot play1 in the configured script.
* **/pause** - will pause the video that is playing.
* **/resume** - will resume playing the paused video.
* **/stop** - stops the video playing and the media screen goes black.
* **/volume 30** - changes the volume.  Volume is from 0 to 100.  The video starts with volume at 50.  So, volume 30 lowers the volume and volume 60 raises the volume.  Volume 0 mutes the volume.
* **/commands** - displays the commands in chat with what they do.

**Special Command to Force Videos**
When uploading videos some people set their permissions to not allow people to embed their videos outside of Youtube.  They can only be played from within the Youtube Site.  If you have one of these videos you will get the following error screen:

![](https://github.com/mojoD/Sansar-Custom-Scripts/blob/master/images/videounavailable.png)

In order to play this type of video in the gotMojo Media Player there is a special command:
* **/forcevideo YoutubeVideoURL** - will play this type of video from the Youtube Site in a non full screen mode.

## Using the Control Panel on the Media Player

The Controls on the Bottom of the panel can be used instead of chat commmands:

![](https://github.com/mojoD/Sansar-Custom-Scripts/blob/master/images/mediaplayer.png)

Here is a Youtube Video showing all the features:
* https://www.youtube.com/watch?v=B2qQoMjWpWo



