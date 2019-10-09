# Iterators in Sansar

Writing your own iterators allows you to iterate through data/objects in a way that is memory efficient. It is essentially a function that keeps the state about the number of times it was called, does not need to store the entire list of data, and allows it to be used in interative operations, such as `foreach`. Iterators allow to potentially iterate through an infinite (or indefinite) number of items.

Iterators use `yield` to hand over the next value in the interation, which pauses the code until the next item is asked for. Because Sansar uses a subset of C# that prohibits the use of `yield` there is no way to create your own iterators. Or is there? 😉

Using `StartCoroutine`, `WaitForSignal`, and `WaitFor`, I have managed to create a generic iterator that instead of using the `yield` operator, a `yield()` callback is provided to hand over the next value.

## Usage

Include the interator classes in your project and upload your script with that file. To do that, you upload your script as a `JSON`, such as this:

**IteratorExample.json**
```JSON
{
  "source": [
    "IteratorExample.cs",
    "../Iterator.cs"
  ]
}
```

Create your Sansar script with a single helper function to create the iterator:

**Iterator.cs**
```csharp
using Sansar.Simulation;
using Sansar.Script;
using System;
using System.Collections.Generic;
using EvoAv.Iterators;

public class IteratorExample : SceneObjectScript {

  IEnumerable<T> CreateIterator<T>(Action<Action<T>> collector) {
    return new Iterator<T>(StartCoroutine, WaitForSignal, WaitFor, collector);
  }

  override public void Init() {

  }

}
```

Now you can create your own interator. The following example iterates through a counter indefinitely. Don't worry about the `while(true)` in there, `yield()` pauses the code each time, and the loop ends when you destroy the interator.

```csharp
    IEnumerable<int> counter = CreateIterator((Action<int> yield) => {
      int i = 0;
      while (true) {
        yield(++i);
      }
    });
```

And then you can use the `counter` like any `IEnumerable` iterator. The following `foreach` will cycle through the iterator until a second passes. 

```csharp
    int count = 0;
    DateTime start = DateTime.Now;
    foreach (int i in counter) {
      if (DateTime.Now.Subtract(start).TotalSeconds > 1) {
        count = i;
        break;
      }
    }
```

In my tests I was able to make about 4000 iterations a second, which makes its performance footprint equivalent to a raycast.