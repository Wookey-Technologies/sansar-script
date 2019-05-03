# sansar-script

Hello and welcome!

This public repository contains C# scripts and assets intended for use within Sansar.  Inside you 
will find examples, samples, tutorials and more.

Some materials are intended for people new to Sansar and new to C# while others will likely only make
sense to seasoned developers.

The easiest way to create interactive content in Sansar is to make use the built-in "Scene Scripts Library".
This library was formerly referred to as "Simple Scripts" so you may notice that term used instead.
The instructions on this page do not focus on the use of these built-in scripts, but are instead intended to
help creators write their own scripts.

Below the table of contents, there are instructions on how to start making experiences with scripts.
They can also be handy for anyone trying to get any of the included examples or tutorials in this
repository.

And further down on the page is an introduction to scripting in Sansar.  This assumes a basic
knowledge of modern programming principles and C# and can be quite technical at times.  But there is
no need to fully understand all of this.  

Get in and start tinkering and you will probably be able to sort out how do many things without
needing to be an expert in C#!

Also all are encouraged to ask questions and follow the `#scripting` channel in 
[Sansar Official Discord!](http://discord.gg/sansarofficial)

Happy scripting!


## Table of contents

**Examples** - contains a copy of the example scripts that are distributed with Sansar.  By default
these can be found in your `C:\Program Files\Sansar\Client\ScriptApi\Examples` directory.

**Suggestions** - contains ideas for future examples and tutorials.  We will periodically review the
suggestions left in this folder and try to turn them into examples or tutorials if we can.

**Tutorials** - hopefully this is self-explanatory enough.  Look in this directory if you're trying to
get started.
Tutorials are different than examples in that they may include binary assets or other resources you may
need to follow along.

**Users** - this is the dumping ground for scripts from individuals within and external to the Sansar
development team.


## How to create an experience and use a script in Sansar

The first step to creating a new experience in Sansar is to find the Build menu and create a new
experience.  I would suggest starting from the Base Template so you have a scene that you can build
and rebuild quickly, which often saves a lot of time while working on scripts.

Once you are editing an experience, the first step for scripts is to import them into your inventory.
After that you can hook them up to specific objects in your scene by adding a script component to the
object or by dragging the script out of your inventory and dropping it on the object in your scene.

After that, you need to build your scene to see your script in action!

### Importing

1. Download an example script or tutorial from this repository.
2. In the Sansar Editor, choose:
  **Import** -> **Script**  and choose the script.
3. Hit the red import button.

This will compile the script and import it to your inventory.  To find it, make sure your inventory
is open by going to:
  **Tools** -> **Inventory**  at the top of the screen.

You may also need to make sure your filtering options are set to see your own creations.  To do this
change the Inventory window first filter to "Created" or "All sources" and the second filter to 
"Scripts" or "All types".  If the last filter is "Newest" then the script you just imported should
now be in the top left corner of the Inventory window.

### Attaching a script to an object

There are several ways to attach a script to an object in the world.  The basic method is to drag
the script from your inventory and drop it onto the object.  This will automatically add a new script
component to the object and assign it to use your script.

Alternatively, you can right click on an object in the world or in the "Scene Objects" view and choose:
  **Add** -> **Script**  to add a script component to your object.
Then you will need to look at the Properties panel for that object, find the script component and set
the "Script" property to your newly imported script.

### Building the scene

Once the script is attached to an object in the scene and you are ready to test it, hit the "Build"
button to save, build and upload your scene.  Then choose the "Visit Now" option to see it in action.

When you have tested it and are ready to go back to the editor to make further changes, go back to
the "Create" menu on the left-hand side and choose the "Edit this scene" option.

Depending on scene complexity, you may find this build/play loop takes a while.  Make sure to change
the following scene settings to make your iteration time as fast as possible:

  **Tools** -> **Scene settings**
  *  ->  **Light**:  Change "Global Illum Quality" to "No processing"
  *  ->  **Background Sound**:  Change "Compute Reverb" to "Off"


## Getting started

What follows here is a quick summary that might help you get started a bit faster if you are
already familiar with C# syntax and object oriented programming principles.

Most scripts will want to define a new class and derive from the `SceneObjectScript` base class in
the `Sansar.Simulation` namespace like so:

```c#
public class ExampleScript : Sansar.Simulation.SceneObjectScript
{
    public override void Init() {}
}
```

To save you from having to type `Sansar.Simulation` everywhere you'll probably want to 
start most files with a using statement or two:

```c#
using Sansar.Simulation;

public class ExampleScript : SceneObjectScript
{
    public override void Init() {}
}
```

That's technically a full Sansar script!  However, since it does absolutely nothing at all let's
go ahead and update it before we try using it in a scene:

```c#
using Sansar.Script;
using Sansar.Simulation;

public class HelloSansarScript : SceneObjectScript
{
    public override void Init()
    {
        Log.Write("Hello Sansar!");
    }
}
```

So as you can see, the `Sansar.Script` namespace includes a log function to print out messages.  
These messages go to the debug console and are only visible to the owner of the scene.

So if we drop that into the example above we will get a script that will write a message to the
debug console at script initialization time.  Go ahead and try this out and read on to find out
how to see these messages!


### How to use the debug console

See the above instructions on how to import the script and attach it to an object in the scene.
Then when you build and run the scene and press `Ctrl+d` you will see `Hello Sansar!` in the
console.

This logging functionality can also be used to write yellow warning text and red error text to
the debug console like so:

```c#
Log.Write(LogLevel.Info, "Information text");
Log.Write(LogLevel.Warning, "Yellow warning text");
Log.Write(LogLevel.Error, "Red error text");
```

For the complete set of logging functionaly check the API documentation in your Sansar installation:
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\Sansar.Script\Log.html`


### How to create properties that can be modified in the editor

In general, public script variables will show up in the editor on the properties panel for the
object that has the script on it.  More specifically there are a limited set of property types
that are supported by the Sansar editor.  Here is a sample script with one of each type of
supported property:

```c#
using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class PropertiesExampleScript : SceneObjectScript
{
    public bool BoolProperty;
    public int IntProperty;
    public float FloatProperty;
    public double DoubleProperty;
    public string StringProperty;
    public Vector VectorProperty;
    public Quaternion QuaternionProperty;
    public Color ColorProperty;
    public Interaction InteractionProperty;
    public ClusterResource ClusterResourceProperty;
    public SoundResource SoundResourceProperty;

    public override void Init() {}
}
```

Note the addition of the `using Sansar;` line so that all of these types can be referenced without
further qualification.

#### Descriptions, default values, ranges and tooltips

In addition there are custom C# properties that can be used to set default values, assign ranges,
override the display name and define a tooltip for each property.  Here are a few examples of these:

```c#
    [Tooltip("The tooltip for this string property")]
    [DefaultValue("default string")]
    [DisplayName("The String Property")]
    public string MyStringProperty;

    [DefaultValue(3)]
    [Range(0,5)
    public int MyRangedIntProperty;

    [Tooltip("Custom object gravity multiplier")]
    [DefaultValue(1.0f)]
    [Range(-2.0f, 2.0f)]
    [DisplayName("Gravity Multiplier")]
    public float GravityFactor;

    [Tooltip("The pivot point of the rotation, in object local space.")]
    [DisplayName("Object Rotation Pivot")]
    [DefaultValue("<0,0,1>")]
    public Vector RotationPivot;

    [Tooltip("The color of the light for Mode A")]
    [DisplayName("Mode A Color")]
    [DefaultValue("(1,0.8,0.5,1)")]
    public Sansar.Color ColorModeA;
```

Note that the `Vector` type requires the `<>` brackets for correct default value parsing, while the
`Color` type uses `()` parenthesis.

Quaternions are also supported and they use the `[]` brackets but you most likely would want to use a 
`Vector` for a rotation property and then initialize your script rotation from Euler angles since 
typing in Quaternion does not come easily to most users.  This could be done like so:
```c#
Quaternion q = Quaternion.FromEulerAngles(Mathf.RadiansPerDegree * MyVectorRotationProperty);
```

The `Interaction` type above is a string in the editor but also enables the object to be clickable
in world.  More on that below.

The `ClusterResource` and `SoundResource` properties allow scripts to interact with and spawn other
objects in your inventory.  There is no way to specify default values for these types and the editor
will present the list of objects of that type in the user's inventory.

#### Limits and arrays

Scripts have a maximum number of properties that can be exposed to the editor.  The exact number is
dependent on the base class type for your script.

For the examples above that derive from `SceneObjectScript` the limit is 20 properties.
Scripts that derive from `ObjectScript` have a limit of 10 properties.

One way to get beyond the 20 property limit is to use array types.  The syntax for these
uses the C# `List` like so:

```c#
using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System.Collections.Generic;

public class ArrayPropertiesExampleScript : SceneObjectScript
{
    public List<bool> BoolValues;
    public List<int> IntValues;
    public List<float> FloatValues;
    // etc.
}
```

Note that all of the property types previously introduced will work as arrays although I would not
recommend using the `Interaction` type in an array since only one will function on an object!


### How to see text in world

There are a few different ways to get messages from a script into the world, depending on what you
are trying to do.

#### Debug console messages

Messages can be written to the debug console (Ctrl+d) using the `Log.Write` function.  These are
only visible to you, the creator and owner of the scene.

#### Nearby chat messages

Messages can be broadcast to nearby chat via `ScenePrivate.Chat.MessageAllUsers`.

#### Direct messages and private messages

Messages can be sent to an individual as a direct message from `agent.SendChat` where `agent` is an
instance of `AgentPrivate`.  Usually the `agent` is obtained from a `SessionId` that comes as part
of the data from an event callback.  For example using the above `InteractionProperty` that would be:

```c#
InteractionProperty.Subscribe((InteractionData data) =>
{
    AgentPrivate agent = ScenePrivate.FindAgent(data.AgentId);
    if (agent != null)
        agent.SendChat("Hello from script!");
});
```

#### Modal dialogs

Messages can also be presented to the user in a modal dialog like so:

```c#
InteractionProperty.Subscribe((InteractionData data) =>
{
    AgentPrivate agent = ScenePrivate.FindAgent(data.AgentId);
    if (agent != null) {
        agent.Client.UI.ModalDialog.Show("Choose your destiny!", "No", "Yes!", (opc) =>
        {
            if (agent.Client.UI.ModalDialog.Response == "Yes!")
                agent.SendChat("You pressed yes!");
            else
                agent.SendChat("You pressed no.");
        });
    }
});
```

#### In-world interaction text

And also interactions can have custom and changeable prompt messages that show up as mouse-over, hover text
in-world.  See below for details on interaction.


### How to make something clickable (using Interaction)

Objects that have an `Interaction` property will be clickable in-world by default for all users:

```c#
public Interaction MyInteraction;
```

This will display hover text and show a green highlight on the object when a user is pointing at it.  The 
hover text can be configured in the editor by editing the `MyInteraction` field on the properties panel.

Note that objects can only have a single interaction property but the interaction hover text can be changed
on the fly using the `Interaction.SetPrompt` function.

Lastly, scripts can add a custom interaction at runtime if they wish.

```c#
using Sansar.Script;
using Sansar.Simulation;

public class AddInteractionScript : SceneObjectScript
{
    public string Title;

    public override void Init()
    {
        ObjectPrivate.AddInteractionData addData = (ObjectPrivate.AddInteractionData) WaitFor(ObjectPrivate.AddInteraction, Title, true);

        addData.Interaction.Subscribe((InteractionData data) =>
        {
            AgentPrivate agent = ScenePrivate.FindAgent(data.AgentId);

            if (agent != null)
            {
                agent.SendChat($"Hello from {Title}!");
            }
        });
    }
}
```

Also note that both the interaction text and the interaction itself can be customized globally or per user.

For the complete set of functionaly related to Interaction, check the API documentation in your Sansar installation:
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\Sansar.Simulation\Interaction.html`


### How to control animations

Sansar supports animations in model FBX files.  These are authored and exported from Maya, Blender, etc. and then
imported to Sansar.  These files can contain one or more animations which can all be accessed and controlled by the scripting
API using the `Sansar.Simulation.AnimationComponent`.

Note:  All animations imported to Sansar are resampled to 30fps.  You will want to export animations at 30fps if you plan
to have precise frame control from your code.

The first task when working with the components in Sansar is to acquire the component from the object.  This can be done
from a few different API endpoints but a common way is as follows:

```c#
AnimationComponent animComp;
ObjectPrivate.TryGetFirstComponent(out animComp);
```

If the script is running on an object that does not have animations, the above code will fail to acquire an animation component.
A script can safely playback an animation if it exists with a little error checking:

```c#
using Sansar.Script;
using Sansar.Simulation;

public class AnimationScript : SceneObjectScript
{
    public override void Init()
    {
        AnimationComponent animComp;
        if (ObjectPrivate.TryGetFirstComponent(out animComp))
            animComp.DefaultAnimation.Play();
        else
            Log.Write(LogLevel.Warning, "AnimationScript not running on an animated object!");
    }
}
```

As you can see above, a quick way to access the animation from the `AnimationComponent` instance is through the 
`DefaultAnimation` member.  

If you wish to control playback more precisely, your script will need to use the `AnimationParameters` struct.  
For example if you wanted to loop a certain animation from frame 10 to frame 50, playback would be done like so:

```c#
AnimationParameters animParams = animComp.DefaultAnimation.GetParameters();
animParams.PlaybackMode = AnimationPlaybackMode.Loop;
animParams.ClampToRange = true;
animParams.RangeStartFrame = 10;
animParams.RangeEndFrame = 50;
animComp.DefaultAnimation.Play(animParams);
```

You can also create a new animation parameters struct if you prefer but getting the parameters from the animation
as is done above will preserve any other editor settings.  In this case, notably the playback speed from the editor
would be preserved since the code is not overwriting that member.  This will allow you to adjust the playback speed
without having to recompile the script.

If the object has multiple animations in the FBX, your script will need to use the 
`GetAnimation(string)` or `GetAnimations()` interface instead of simply accessing the `DefaultAnimation`.

For the complete set of functionaly related to animations, check the API documentation
in your Sansar installation:
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\Sansar.Simulation\AnimationComponent.html`
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\Sansar.Simulation\Animation.html`


### How to turn lights on and off

Lights, like animation, has its own component for script control namely the `LightComponent`.  Acquiring access
to the light component can be done in a very similar way:

```c#
LightComponent lightComp;
ObjectPrivate.TryGetFirstComponent(out lightComp);
```

There are a few things to keep in mind with lights.  Unlike animations that exist for the sake of movement, 
lights can potentially be optimized in the scene build process.  Because of this, it is necessary to set the
"Scriptable" flag "On" in the editor for each light that your script may want to adjust.  Your script can
detect whether a light is scriptable using the `IsScriptable` flag.

Here is a script that can safely turn a light off on an object:

```c#
using Sansar.Script;
using Sansar.Simulation;

public class LightOffScript : SceneObjectScript
{
    public override void Init()
    {
        LightComponent lightComp;
        if (!ObjectPrivate.TryGetFirstComponent(out lightComp))
            Log.Write(LogLevel.Warning, "LightScript not running on an object with a light!")
        else if (!lightComp.IsScriptable)
            Log.Write(LogLevel.Warning, "LightScript not running on an object with a scriptable light!")
        else
            lightComp.SetColorAndIntensity(Color.Black, 0.0f);  // turn the light "off"
    }
}
```

Lights in Sansar combine the color and intensity into a single value that is applied to the scene.  
As a result, the script API does not have the ability to manipulate them independently.  Also the color
you apply to a light might be different than the color you retrieve when you query the light but that
comes with the territory.  The script API uses the nomenclature `GetNormalizedColor` and 
`GetRelativeIntensity` vs. `SetColorAndIntensity` to reinforce that idea.

Note that shadow casting lights are expensive to render and can be a considerable performance drain on
lower end machines.  Consider reducing a scene to a single shadow casting light if you or your users
are experiencing a low framerate.

For the complete set of functionaly related to light components, check the API documentation
in your Sansar installation:
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\Sansar.Simulation\LightComponent.html`


### How to control physical objects

Physical objects in Sansar are those that have physics collision and can be controlled by the physics
engine.  In the editor properties or object structure, this will appear as a "volume" and the object
itself will have a "motion type" property, along with "density" and "friction".

In the script API, all physics objects are manipulated from the `RigidBodyComponent`.  Similarly to
the other components, a common way to get access to the rigid body component is as follows:

```c#
RigidBodyComponent rbComp;
ObjectPrivate.TryGetFirstComponent(out rbComp);
```

A slightly more robust script that will apply an upwards impulse to a dynamic physics object when it
is clicked would be as follows:

```c#
using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class RigidBodyImpulseScript : SceneObjectScript
{
    public Interaction MyInteraction;

    private RigidBodyComponent _rb;

    public override void Init()
    {
        if (ObjectPrivate.TryGetFirstComponent(out _rb))
        {
            _rb.SetCanGrab(false);  // Disable grabbing for this object

            if (_rb.GetMotionType() == RigidBodyMotionType.MotionTypeDynamic)
            {
                MyInteraction.Subscribe((InteractionData data) =>
                {
                    _rb.AddLinearImpulse(Vector.Up * 100.0f);
                });
            }
            else
                Log.Write(LogLevel.Warning, "RigidBodyImpulseScript not running on a dynamic object!")
        }
        else
            Log.Write(LogLevel.Warning, "RigidBodyImpulseScript not running on an object with a physics volume!")
    }
}
```

For the complete set of functionaly related to rigid body components, check the API documentation
in your Sansar installation:
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\Sansar.Simulation\RigidBodyComponent.html`

#### The thing about motion type...

The "motion type" property is extremely important when it comes to physics objects and it greatly
affects how the object behaves and what can be done to the object from the script API.  

The "static" motion type indicates that the object will not ever be moving.  This is considered the
most restrictive motion type.  In fact static objects built into the scene are assumed never to move
and are potentially optimized by the build process, so script manipulation of static objects is not
possible.

The "keyframed" motion type indicates that the object will only move when explicitly moved from
script.  This is the next most restrictive motion type.  When keyframed objects are moved, 
they will not stop or otherwise be affected by any other collisions.  They will simply move through 
everything and push avatars and dynamic objects out of the way.

The "dynamic" motion type indicates that the object will be subject to gravity and other phsyical
interactions.  This is the least restrictive motion type.  Dynamic objects will fall, roll and
slide depending on what forces get applied to them in the scene.  They will collide with static
and keyframed objects and avatars.

Motion types can be changed from script but only to a more restrictive motion type than the initial
import or scene settings allow.  So for example, an object imported as "dynamic" can be set to
"keyframed" from script but an object imported as "static" can not be set to "keyframed" or "dynamic".


### How to move non-physical objects

Objects that have no collision are considered non-physical objects in Sansar.  These can be moved
with the Mover API if they are configured to be allowed to move.  Much like "static" objects above,
objects that are not configured for movement are assumed never to move and are potentially optimized 
by the build process.

To configure an object for movement, set the "Movable From Script" attribute to "On".

Also note that the Mover API can drive "keyframed" physics objects but can not drive "dynamic" or "static"
physics objects.  So if your object does have a physics volume and you wish to configure it for movement,
make sure to set the motion type to "keyframed" as well as the "Movable From Script" attribute.

Properly configured objects can then be immediately moved using any of these functions:

```c#
ObjectPrivate.Mover.AddMove(position, rotation);
ObjectPrivate.Mover.AddTranslate(position);
ObjectPrivate.Mover.AddRotate(rotation);
```

The above functions will result in immediate movement from the object's current position and orientation
to the specified location.  Use the following interfaces if you prefer to have more gradual movement over
time:

```c#
ObjectPrivate.Mover.AddMove(position, rotation, seconds, moveMode);
ObjectPrivate.Mover.AddTranslate(position, seconds, moveMode);
ObjectPrivate.Mover.AddRotate(rotation, seconds, moveMode);
```

The additional arguments are the length of time and specified mode for the movement.  Exactly which
move mode you choose will depend on the use case but the options are 
"linear", "ease-in", "ease-out" and "smoothstep".

Note that the mover functions like a queue behind the scenes and executes all commands sequentially.
In this way it is possible to make a simple behavior to move an object through a set of points:

```c#
ObjectPrivate.Mover.AddTranslate(point1, 5.0, MoveMode.Linear);
ObjectPrivate.Mover.AddTranslate(point2, 5.0, MoveMode.Linear);
ObjectPrivate.Mover.AddTranslate(point3, 5.0, MoveMode.Linear);
ObjectPrivate.Mover.AddTranslate(point4, 5.0, MoveMode.Linear);
```

In addition, the `WaitFor` function will run the instructions in a coroutine until the movement has
completed.  So an object can be safely set to patrol around a few points indefinitely like so:

```c#
using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class PatrolMoverScript : SceneObjectScript
{
    public Vector Point1;
    public Vector Point2;
    public Vector Point3;
    public Vector Point4;

    [DefaultValue(1.0f)]
    public float MoveTime;

    public override void Init()
    {
        StartCoroutine(PatrolUpdate);
    }

    void PatrolUpdate()
    {
        while (true)
        {
            ObjectPrivate.Mover.AddTranslate(Point1, MoveTime, MoveMode.Linear);
            ObjectPrivate.Mover.AddTranslate(Point2, MoveTime, MoveMode.Linear);
            ObjectPrivate.Mover.AddTranslate(Point3, MoveTime, MoveMode.Linear);
            WaitFor(ObjectPrivate.Mover.AddTranslate, Point4, MoveTime, MoveMode.Linear);
        }
    }
}
```

A slightly fancier version to rotate the object to face its next goal position before translation
and move the object at a constanst speed might be something like this:

```c#
using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System.Collections.Generic;

public class PatrolTurnMoverScript : SceneObjectScript
{
    public List<Vector> PatrolPoints;

    [DefaultValue(1.0f)]
    public float MoveSpeed;

    [DefaultValue("<0,1,0>")]
    public Vector WorldObjectForward;

    public override void Init()
    {
        if ((PatrolPoints.Count > 1) && (MoveSpeed > 0.0f))
            StartCoroutine(PatrolUpdate);
    }

    void PatrolUpdate()
    {
        int current = 0;
        int next = current + 1;

        // Start the object on the first patrol point
        ObjectPrivate.Mover.AddTranslate(PatrolPoints[current]);

        while (true)
        {
            // Calculate direction to next patrol point
            Vector toNext = PatrolPoints[next] - PatrolPoints[current];

            // Compute a world space rotation for this object to point at the next patrol point
            Quaternion rotation = Quaternion.ShortestRotation(WorldObjectForward, toNext.Normalized());
            ObjectPrivate.Mover.AddRotate(rotation);  // Immediately turn to face

            // Compute the time based on the distance and move speed
            float moveTime = toNext.Length / MoveSpeed;

            // Move the object to the next patrol point
            WaitFor(ObjectPrivate.Mover.AddTranslate, PatrolPoints[next], moveTime, MoveMode.Linear);

            // Increment to the next patrol point
            current = next;
            next = (next + 1) % PatrolPoints.Count;
        }
    }
}
```

The queue can also be cleared using the `StopAndClear` function to interrupt the movement.

For the full interface, check the API documentation in your Sansar installation:
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\Sansar.Simulation\Mover.html`
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\Sansar.Simulation\MoveMode.html`


### How to play sounds

There are a variety of ways to play sounds in Sansar.  Sounds can be played through a specific audio emitter
in a scene, or they can be played spatialized or non-spatialized for a target avatar for everyone in the scene.

Spatialized sound is sound that comes from a specific position in the world.  It is directional and may not
be heard by avatars that are far away.  Non-spatialized sounds are played at equal volume regardless of where
the listener might be in the scene.

Scripts can not directly reference audio resources without configuration from the editor.  Here is a sample
script to play a sound when an object is clicked:

```c#
using Sansar.Script;
using Sansar.Simulation;

public class SoundScript : SceneObjectScript
{
    public SoundResource Sound;

    [DefaultValue(80.0f)]
    [Range(0.0f, 100.0f)]
    public float Loudness;

    public override void Init()
    {
        // Check to make sure a sound has been configured in the editor
        if (Sound == null)
        {
            Log.Write("SoundScript has no configured sound to play!")
            return;
        }

        ObjectPrivate.AddInteractionData addData = (ObjectPrivate.AddInteractionData) WaitFor(ObjectPrivate.AddInteraction, "Play sound", true);

        addData.Interaction.Subscribe((InteractionData data) =>
        {
            PlaySettings playSettings = PlaySettings.PlayOnce;
            playSettings.Loudness = (60.0f * (Loudness / 100.0f)) - 48.0f;  // Convert percentage to decibels (dB)

            ScenePrivate.PlaySound(Sound, playSettings);
        });
    }
}
```

In this example we are playing the sound in a non-spatialized way for all users in the scene.  Here are alternate
ways to play back the sound for a specific user or spatialized for all users:

```c#
ScenePrivate.PlaySoundAtPosition(Sound, someVector, playSettings);  // spatialized, audible for all

agent.PlaySound(Sound, playSettings);  // non-spatialized, only for the agent
agent.PlaySoundAtPosition(Sound, someVector, playSettings);  // spatialized, only for the agent
```

Playback on a specific audio emitter in the scene requires acquiring the corresponding `AudioComponent`.  Since
this is already familiar to you from the animation, light and rigid body component examples above, let's also expand
this example to start and stop a looping sound, which introduces the concept of a play handle:

```c#
using Sansar.Script;
using Sansar.Simulation;

public class LoopingSoundComponentScript : SceneObjectScript
{
    public SoundResource LoopingSound;

    [DefaultValue(80.0f)]
    [Range(0.0f, 100.0f)]
    public float Loudness;

    private AudioComponent _audio = null;
    private PlayHandle _playHandle = null;

    public override void Init()
    {
        // Check to make sure a sound has been configured in the editor
        if (LoopingSound == null)
        {
            Log.Write("LoopingSoundComponentScript has no configured sound to play!")
            return;
        }

        if (!ObjectPrivate.TryGetFirstComponent(out _audio))
        {
            Log.Write("LoopingSoundComponentScript is on an object that does not have an audio emitter.")
            return;
        }

        ObjectPrivate.AddInteractionData addData = (ObjectPrivate.AddInteractionData) WaitFor(ObjectPrivate.AddInteraction, "Play sound", true);

        addData.Interaction.Subscribe((InteractionData data) =>
        {
            // If not sound is playing, start one up
            if (_playHandle == null)
            {
                PlaySettings playSettings = PlaySettings.Looped;
                playSettings.Loudness = (60.0f * (Loudness / 100.0f)) - 48.0f;  // Convert percentage to decibels (dB)

                _playHandle = _audio.PlaySoundOnComponent(LoopingSound, playSettings);
            }
            // Else if a sound is playing, stop it
            else
            {
                if (_playHandle.IsPlaying())
                    _playHandle.Stop();

                _playHandle = null;
            }
        });
    }
}
```

As you can see from the above sample, play handles are returned by the sound play interfaces and they can
be used to control or manipulate a previously played sound.

For the complete set of functionaly related to sounds, check the API documentation
in your Sansar installation:
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\Sansar.Simulation\AudioComponent.html`
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\Sansar.Simulation\PlayHandle.html`

Also check the `PlaySound` and `PlaySoundAtPosition` functions on these classes:
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\Sansar.Simulation\AgentPrivate.html`
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\Sansar.Simulation\ScenePrivate.html`


#### Audio play settings

In the above examples we start with the `PlayOnce` or `Looped` settings and then adjusted the other attributes
as needed.  This is the recommended way to set up these audio calls.

Note that the `Loudness` setting is expected to be in decibels (dB) but most non-sound designers prefer to work
in percentages so we end up doing a little math to convert from percentages to dB for the play settings.

The complete documentation can be found here:
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\Sansar.Simulation\PlaySettings.html`


### How to control the media source

Like sounds, the media source for a scene can be controlled globally or on a per-user basis.  Scenes only
support a single media source though, and there is no functionality to save off a screenshot or otherwise
freeze a media source to an object.  When the media source updates, all surfaces that use the media source
material will update to reflect the new data.

The main two interfaces that change the media source can be accessed like so:

```c#
ScenePrivate.OverrideMediaSource("https://www.sansar.com/");  // scene-wide, all users will see this
agent.OverrideMediaSource("https://atlas.sansar.com/experiences/sansar-studios/");  // only for this agent
```

Here is a sample script to change the media source for a specific user when they interact with the object:

```c#
using Sansar.Script;
using Sansar.Simulation;

public class PrivateMediaSourceScript : SceneObjectScript
{
    [DefaultValue("https://www.sansar.com/")]
    public string PublicMedia;

    [DefaultValue("https://atlas.sansar.com/experiences/sansar-studios/")]
    public string PrivateMedia;

    public override void Init()
    {
        // Set the public media source for the scene
        ScenePrivate.OverrideMediaSource(PublicMedia);

        // Override the media source to the private media source for each user that clicks on this object

        ObjectPrivate.AddInteractionData addData = (ObjectPrivate.AddInteractionData) WaitFor(ObjectPrivate.AddInteraction, "Show media", true);

        addData.Interaction.Subscribe((InteractionData data) =>
        {
            AgentPrivate agent = ScenePrivate.FindAgent(data.AgentId);

            if (agent != null)
            {
                agent.OverrideMediaSource(PrivateMedia);
            }
        }
    }
}
```

The complete reference for these functions can be found on these classes:
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\Sansar.Simulation\AgentPrivate.html`
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\Sansar.Simulation\ScenePrivate.html`


### How to implement chat commands

Sometimes it can be handy to set up scripts to respond to chat commands.  Scripts can do this by 
subscribing to the default chat channel for the scene and parsing the messages, like so:

```c#
ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, (ChatData data) => {
    if (data.Message == "hello script")
        ScenePrivate.Chat.MessageAllUsers("hi!");
});
```

Depending on the type of commands and control being put into chat commands, it is often a good
idea to put some level of restriction based on who sent the command.  So for example here is a
script that changes world gravity to a low value when the scene owner types "/setlowgrav" but
will ignore that message from any other user.

```c#
using Sansar.Script;
using Sansar.Simulation;

public class LogGravityChatCommandScript : SceneObjectScript
{
    public override void Init()
    {
        ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, (ChatData data) => 
        {
            if (data.Message == "/setlowgrav")
            {
                AgentPrivate agent = ScenePrivate.FindAgent(data.SourceId);

                // If the agent is the scene owner
                if ((agent != null) && (agent.AgentInfo.AvatarUuid == ScenePrivate.SceneInfo.AvatarUuid))
                {
                    // Set the gravity to 15% of earth gravity
                    ScenePrivate.SetGravity(0.15f * 9.81f);

                    // Send a private acknowledgement message back to the user
                    agent.SendChat("You set low gravity!");
                }
            }
        });
    }
}
```

The complete reference for the chat system can be found here:
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\Sansar.Simulation\Chat.html`


### How to put multiple scripts together into a single inventory item

As scene and script complexity grows, it is often handy to be able to share code between multiple
scripts or to define common interfaces for inter-script communication.  Or it can just be a convenient way
for projects to store all of their scripts within a single inventory resource.

Script resources that contain multiple scripts are referred to as "Script Assemblies" and there are two
ways to make them.

The easiest way is to define a namespace in your file and then to declare multiple classes within that
namespace:

```c#
using Sansar.Script;
using Sansar.Simulation;

namespace MyCustomNamespace
{
    public class Script1 : SceneObjectScript
    {
        public override void Init();
    }

    public class Script2 : SceneObjectScript
    {
        public override void Init();
    }

    // etc.
}
```

The import process for this script remains the same.  When the script is assigned to an object in the
scene, there will be an additional UI element to choose either `Script1` or `Script2` from the namespace.

If you prefer to distribute your scripts across multiple files then the setup will be a little different.
The classes will still need to be within your custom namespace but each one can sit in a separate file.
For this same example code let's say we define `Script1.cs` and `Script2.cs` in the same directory.
In order to import them as one into Sansar, a small JSON project file is required.  

The `MyCustomNamespace.json` file contents would be as follows:

```json
{
  "source": [
    "Script1.cs",
    "Script2.cs"
  ]
}
```

Importing this script assembly is then done by importing the JSON file instead of the C# files.

There are a few C# properties to improve the usage of a script assembly.  Namely you can override the
auto-generated name (usually to remove the namespace) and set the default class.  So for example if
you wanted `Script2` to be the default script selected when this assembly is put on an object and you
wanted to call it "Master Control Script" instead of "Script2", you would define it like so:

```c#
using Sansar.Script;
using Sansar.Simulation;

namespace MyCustomNamespace
{
    [Tooltip("This is the master control script.")]
    [DisplayName("Master Control Script")]
    [DefaultScript]
    public class Script2 : SceneObjectScript
    {
        public override void Init();
    }
}
```

Another way to shorten the name that shows up in the editor by just removing the namespace is:

```
namespace MyCustomNamespace
{
    [DisplayName(nameof(Script2))]
    public class Script2 : SceneObjectScript
    {
        public override void Init();
    }
}
```


### How to send and receive messages between scripts

Scripts can communicate with other scripts using messages.  These messages are broadcast throughout the
scene and can also include additional data.  This can be a good way to distribute scripting functionality
within your own scripts, but also to communicate with scripts written by other developers.  In addition,
it is the main mechanism that the so-called "Simple Scripts" use for communication.  
See the below section on simple script messaging for more information about this.

The two pieces to this are sending and receiving script messages, which is done through these interfaces:

```c#
PostScriptEvent("my_event");  // send "my_event"
SubscribeToScriptEvent("my_event", (ScriptEventData data) => {});  // listen for "my_event"
```

A pair of script instances could use this event to make a button that turns a light off:

```c#
using Sansar.Script;
using Sansar.Simulation;

namespace MessagingScripts
{
    public class SendMessageScript : SceneObjectScript
    {
        public override void Init()
        {
            ObjectPrivate.AddInteractionData addData = (ObjectPrivate.AddInteractionData) WaitFor(ObjectPrivate.AddInteraction, "Show media", true);

            addData.Interaction.Subscribe((InteractionData data) =>
            {
                // Send the "button_pressed" message
                PostScriptEvent("button_pressed")
            });
        }
    }

    public class ReceiveMessageScript : SceneObjectScript
    {
        private LightComponent _light;

        public override void Init()
        {
            if (!ObjectPrivate.TryGetFirstComponent(out _light))
                Log.Write("ReceiveMessageScript couldn't find light!");
            else if (!_light.IsScriptable)
                Log.Write("ReceiveMessageScript couldn't find scriptable light!'");
            else
            {
                // Listen for the "button_pressed" message
                SubscribeToScriptEvent("button_pressed", (ScriptEventData data) =>
                {
                    // Turn off the light
                    _light.SetColorAndIntensity(Color.Black, 0.0f);
                });
            }
        }
    }
}
```

Note also that declaring the namespace allows the script to include more than one class which is a nice
way to package up interdependent scripts into a single script assembly.


### How to connect your scripts to simple scripts

Messaging to and from "Simple Scripts" is exactly the same as messaging within your own scripts with the
added requirement of a specific data payload interface.  Simple scripts rely on this data payload to 
extract and reference specific objects and agents.

The interface of incoming simple script message data payloads is as follows:

```c#
public interface ISimpleData
{
    AgentInfo AgentInfo { get; }
    ObjectId ObjectId { get; }
    ObjectId SourceObjectId { get; }

    // Extra data
    Reflective ExtraData { get; }
}
```

The `AgentInfo` data conveys the agent that triggered this message.  Note that this is not always set
since some messages originate from objects that might not have been triggered by avatar interactions,
such as a timer or an object entering a trigger volume.

The `ObjectId` data is reliably set and contains the Id of the object that triggered the message.  
For example in a trigger volume collision, the Id of the object that entered the trigger volume would be
in this field.

The `SourceObjectId` data comes from the object that sent this message.  In the same trigger volume
example above, this would be the Id of the trigger volume itself.

Here is an example that writes to chat when the "on" message is sent from a simple script:

```c#
using Sansar.Script;
using Sansar.Simulation;

public class SimpleListenerScript : SceneObjectScript
{
    public interface ISimpleData
    {
        AgentInfo AgentInfo { get; }
        ObjectId ObjectId { get; }
        ObjectId SourceObjectId { get; }

        // Extra data
        Reflective ExtraData { get; }
    }

    public override void Init()
    {
        // Listen for the 'on' message
        SubscribeToScriptEvent("on", (ScriptEventData data) =>
        {
            ISimpleData idata = data.Data.AsInterface<ISimpleData>();
            if (idata == null)
            {
                ScenePrivate.Chat.MessageAllUsers("The 'on' message does not have a simple script payload!");
            }
            else
            {
                ObjectPrivate obj = ScenePrivate.FindObject(idata.ObjectId);
                ScenePrivate.Chat.MessageAllUsers("The 'on' message simple script payload came from " + obj.Name);
            }
        });
    }
}
```

Sending a message back to simple scripts requires the creation of a payload that supports that same interface.
One way to do this is to use that exact interface mapped into a `SimpleData` class:

```c#
using Sansar.Script;
using Sansar.Simulation;

public class SimpleSenderScript : SceneObjectScript
{
    public interface ISimpleData
    {
        AgentInfo AgentInfo { get; }
        ObjectId ObjectId { get; }
        ObjectId SourceObjectId { get; }

        // Extra data
        Reflective ExtraData { get; }
    }

    public class SimpleData : Reflective, ISimpleData
    {
        public SimpleData(ScriptBase script) { ExtraData = script; }
        public AgentInfo AgentInfo { get; set; }
        public ObjectId ObjectId { get; set; }
        public ObjectId SourceObjectId { get; set; }

        public Reflective ExtraData { get; }
    }

    public override void Init()
    {
        ObjectPrivate.AddInteractionData addData = (ObjectPrivate.AddInteractionData) WaitFor(ObjectPrivate.AddInteraction, Title, true);

        addData.Interaction.Subscribe((InteractionData data) =>
        {
            // Create the simple script message data payload
            SimpleData sd = new SimpleData(this);
            sd.ObjectId = ObjectPrivate.ObjectId;
            sd.SourceObjectId = ObjectPrivate.ObjectId;

            // Include the agent info for the avatar that triggered this event
            AgentPrivate agent = ScenePrivate.FindAgent(data.AgentId);
            if (agent != null)
            {
                sd.AgentInfo = agent.AgentInfo;
                sd.ObjectInfo = agent.AgentInfo.ObjectId;
            }

            // Send the "on" message with the SimpleData payload
            PostScriptEvent("on", sd);
        });
    }
}
```

The `Reflective` type above is a base interface that exists just for the purposes of being able to use a
common base type when communicating between scripts and interfaces.  We'll be using it more in the 
sections about working with other scripts in the scene so for now just know that it exists and plays a
role in inter-script communication.

The implementation of the simple script data payload interface and all of the other simple script helper 
functions can be found in your Sansar client installation:
* `C:\Program Files\Sansar\Client\ScriptApi\Examples\ScriptLibrary\LibraryBase.cs`


### How to find other scripts in the scene

The name of the mechanism within Sansar that allows one script to find another script within a scene is
`Reflective`.  Once a script interface is located, the calling script can make direct function calls into
the receiving script without the need to send messages back and forth.  It also allows for the 
distribution of properties across multiple script components and can be easier for iteration.

Doing this requires a two-part setup, namely the receiving script needs to register itself and the calling
script needs to know how to find the receiver.

On the receiving side, the class needs to register the reflective interface.  This is done using the 
`[RegisterReflective]` attribute like so:

```c#
using Sansar.Script;
using Sansar.Simulation;

[RegisterReflective]
public class ReflectiveReceiverScript : SceneObjectScript
{
    public Interaction Button;

    public override void Init() {}

    public void SetButtonEnabled(bool enabled)
    {
        Button.SetEnabled(enabled);
    }
}
```

Then on the calling side we query the reflective interfaces to try to find one that matches what we are
looking for.  This could be done like so:


```c#
using Sansar.Script;
using Sansar.Simulation;
using System.Linq;

public class ReflectiveCallerScript : SceneObjectScript
{
    public interface IButton { void SetButtonEnabled(bool enabled); }

    public override void Init()
    {
        IButton[] buttons = ScenePrivate.FindReflective<IButton>("ReflectiveReceiverScript").ToArray();
        foreach (IButton b in buttons)
        {
            b.SetButtonEnabled(true);
        }
    }
}
```

Note the coupling of `[RegisterReflective]` with `FindReflective`.  This can be done across script and
script assembly bounds and is very open ended.  In fact any script with the matching name that has the
defined interface or a superset of the defined interface will be located.

If the receiver script is within the same C# file or within the same script assembly, there is no need
to declare a special reflective interface and the class type and name can be used directly.  In this
case it would be:

```c#
var buttons = ScenePrivate.FindReflective<ReflectiveReceiverScript>("ReflectiveReceiverScript").ToArray();
```


### How to make rest API calls from script

### How to listen for trigger volume events


## Gotchas

### Set functions

### Throttle exceptions



## Scripting documentation

The full API documentation comes with the Sansar installation and should be available for most users here:
* `C:\Program Files\Sansar\Client\ScriptApi\Documentation\index.html`

If you installed Sansar to a different directory you'll need to browse for the documentation in your Sansar folder.

These docs can be intimidating at first but once you get the hang of where to find things you will get
used to using them as a reference.


### Brief summary of Sansar namespaces

The main namespaces in the Sansar scripting system are `Sansar`, `Sansar.Script` and `Sansar.Simulation`.

The main `Sansar` namespace includes the base math constants and functions in `Mathf` and a few types
for working with colors (`Color`), rotations (`Quaternion`) and vectors (`Vector`).

The `Sansar.Script` namespace includes the base types used throughout the script API and access to
features such as log messages for debugging.  

Notable `Sansar.Script` interfaces include:
 
* **`Log.Write`** - print messages to the debug console (use Ctrl+d to view)
* **`ComponentId`** - struct that uniquely identifies a component on a particular object
* **`ObjectId`** - struct to uniquely identify an object
* **`SessionId`** - struct to uniquely identify a user for a single visit to your experience
* **`ICoroutine`** - interface for signalling or aborting coroutines
* **`Timer`** - for one-time or repeating events

`Sansar.Simulation` is the namespace that contains all of the heavy weight classes for working with
avatars (known as 'agents'), the scene itself and all of the various components on objects in the scene.
If you're having trouble finding something in the script API, it is most likely in this namespace
somewhere.

Notable `Sansar.Simulation` interfaces include:

* **`AgentPrivate`** - this is the main class for interaction with the avatars visiting your scene, such
 as playing sounds, getting hand positions and sending direct messages.  Also the `AgentInfo` struct
 includes the unique `AvatarUuid` to identify a player.
* **`AgentPrivate.Client`** - for handling input and teleporting avatars around
* **`AgentPrivate.Client.UI.ModalDialog`** - for modal text windows with one or two buttons.
* **`ObjectPrivate`** - access object position and retrieve components
* **`ObjectPrivate.Mover`** - for moving non-physical objects around
* **`ScenePrivate`** - find agents, other scripts, objects, spawn things, adjust gravity, etc.
* **`ScenePrivate.Chat`** - general nearby chat
* **`SceneObjectScript`** - base class for most scripts

In addition to those, `Sansar.Simulation` also includes the following components:

* **`AnimationComponent`** - for controlling animations on animated objects
* **`AudioComponent`** - for controlling sounds on objects
* **`LightComponent`** - for controlling lights
* **`RigidBodyComponent`** - to adjust physics properties or apply forces or otherwise move physical objects


### AgentPrivate vs. AgentPublic, etc.

If you look at the documentation you will notice that `AgentPrivate` has a corresponding `AgentPublic`
and `ScenePrivate` has a corresponding `ScenePublic`.  These are in place for a future time when
people will be able to attach scripts to their own avatars and take them into other people's scenes
and they can be largely ignored for now.  But the idea is that scene creators will have more access
to info and more ability to manipulate and probe the scene than scripts brought in by users.

You can note, for example, if you make your script class derive from `ObjectScript` instead of
`SceneObjectScript` that it will only have access to `ScenePublic` instead of `ScenePrivate`, which
has a far more limited interface.
