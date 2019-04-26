# sansar-script

Hello and welcome!

This public repository contains C# scripts and assets intended for use within Sansar.  Inside you 
will find examples, samples, tutorials and more.

Some materials are intended for people new to Sansar and new to C# while others will likely only make
sense to seasoned developers.

The easiest way to create interactive content in Sansar is to make use the built-in "Scene Scripts Library".
This library was formerly referred to as "Simple Scripts" so you may notice that term used instead.
The instructions here do not focus on the use of these built-in scripts, but are instead intended to
be helpful in instructing creators in how to write their own scripts.

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

**Tutorials** - hopefully this is self-explanatory enough.  Look here if you're trying to get started.
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

  ->  **Light**:  Change "Global Illum Quality" to "No processing"

  ->  **Background Sound**:  Change "Compute Reverb" to "Off"


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
go ahead and update it before we try using it in a scene.


### How to use the debug console

The `Sansar.Script` namespace includes a log function that can be used to write messages to the
debug console like so:  `Log.Write("Hello Sansar!");`

So if we drop that into the example above we will get a script that will write a message to the
debug console at script initialization time.  Go ahead and try this out!

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

See the above instructions on how to import this script and attach it to an object in the scene.
Then when you build and run the scene and press `Ctrl+d` you will see `Hello Sansar!` in the
console.

For other options with respect to logging try looking 
[here](file:///C:/Program%20Files/Sansar/Client/ScriptApi/Documentation/Sansar.Script/Log.html)


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

In addition there are custom C# properties that can be used to set default values, assign ranges,
override the display name and define a tooltip for each property.  Here are a few examples of these:

```c#
    [Tooltip("The tooltip for this string property")]
    [DefaultValue("default string")]
    [DisplayName("The String Property")]
    public readonly string MyStringProperty;

    [DefaultValue(3)]
    [Range(0,5)
    public readonly int MyRangedIntProperty;

    [Tooltip("Custom object gravity multiplier")]
    [DefaultValue(1.0f)]
    [Range(-2.0f, 2.0f)]
    [DisplayName("Gravity Multiplier")]
    public readonly float GravityFactor;

    [Tooltip("The pivot point of the rotation, in object local space.")]
    [DisplayName("Object Rotation Pivot")]
    [DefaultValue("<0,0,1>")]
    public readonly Vector RotationPivot;

    [Tooltip("The color of the light for Mode A")]
    [DisplayName("Mode A Color")]
    [DefaultValue("(1,0.8,0.5,1)")]
    public readonly Sansar.Color ColorModeA;
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


### How to see text in world

There are a few different ways to get messages from a script into the world, depending on what you
are trying to do.

#### Debug console messages

Messages can be written to the debug console (Ctrl+d) using the `Log.Write` function.

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

Objects that have an `Interaction` property will be clickable in-world for all users by default.

Note that objects can only have a single interaction property but interaction text can be changed on the fly
using the `InteractionProperty.SetPrompt` function.  In fact the text can be customized per user.

Also interactions can be enabled and disabled globally or per user.

Lastly, scripts can add a custom interaction at runtime if they wish.

```c#
using Sansar.Script;
using Sansar.Simulation;

public class AddInteractionScript : SceneObjectScript
{
    public readonly string Title;

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


### How to control animations

### How to control physical objects

### How to move non-physical objects

### How to play sounds

### How to control media streams

### How to communicate with other scripts

### How to find other scripts in the scene


### How to put multiple scripts together



## Gotchas

### Set functions

### Throttle exceptions



## Scripting documentation

The full API documentation comes with the Sansar installation and should be available 
[here](file:///C:/Program%20Files/Sansar/Client/ScriptApi/Documentation/index.html) 
for most users.  If you installed Sansar to a different directory you'll need to browse
for the documentation here: `Sansar/Client/ScriptApi/Documentation/index.html`

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
