
using System;
using System.Collections.Generic;
using Sansar.Script;
using Sansar.Simulation;
using Sansar.Utility;
using EvoAv.Promises;

namespace EvoAv.Promises.Utility {

  public class DeThrottler<T> {
    int Max;
    double PerSeconds;

    class QueuedItem {
      public Func<IPromise<T>> Action;
      public Promise<T> Promise;
    }
    Queue<QueuedItem> Queue = new Queue<QueuedItem>();

    Queue<DateTime> Throttle = new Queue<DateTime>();

    public bool IsActive {get {
      return Throttle.Count > 0;
    }}
    public DeThrottler(int max, double perSeconds) {
      Max = max;
      PerSeconds = perSeconds;
    }

    public IPromise<T> Enqueue(Func<IPromise<T>> action, bool cull = false) {
      QueuedItem qi = new QueuedItem();
      qi.Action = action;
      qi.Promise = new Promise<T>();
      if (Throttle.Count < Max) {
        Queue.Enqueue(qi);
        Next();
      } else if (!cull) {
        Queue.Enqueue(qi);
      } else {
        return Promise<T>.Rejected(new Exception("Throttle limit reached"));
      }
      return qi.Promise;
    }

    public IPromise Enqueue(Action action, bool cull = false) {
      Promise promise = new Promise();
      IPromise<T> p = Enqueue(() => {
        action();
        return Promise<T>.Resolved(default(T));
      }, cull);
      p.Then((T a) => promise.Resolve(), promise.Reject);
      return promise;
    }

    void Next() {
      if (Queue.Count == 0) return;
      QueuedItem qi = Queue.Dequeue();
      if (qi == null) return;
      Throttle.Enqueue(DateTime.Now);
      Promise<T> promise = qi.Promise;
      Timer.Create(TimeSpan.FromSeconds(PerSeconds), () => {
        Throttle.Dequeue();
        Next();
      });
      qi.Action().Then(promise.Resolve, promise.Reject);
    }
  }

}