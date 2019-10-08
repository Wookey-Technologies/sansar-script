using Sansar.Simulation;
using Sansar.Script;
using System;
using System.Collections.Generic;
using EvoAv.Iterators;

namespace EvoExamples {
  public class IteratorExample : SceneObjectScript {
    IEnumerable<T> CreateIterator<T>(Action<Action<T>> collector) {
      return new Iterator<T>(StartCoroutine, WaitForSignal, WaitFor, collector);
    }
    public override void Init() {
      IEnumerable<int> counter = CreateIterator((Action<int> yield) => {
        int i = 0;
        while (true) {
          yield(++i);
        }
      });
      
      int count = 0;
      DateTime start = DateTime.Now;
      foreach (int i in counter) {
        if (DateTime.Now.Subtract(start).TotalSeconds > 1) {
          count = i;
          break;
        }
      }
      Log.Write(count + " iterations per second"); // around 4000
    }
  }
}