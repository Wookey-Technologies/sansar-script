using Sansar.Script;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EvoAv.Iterators {

  public class Iterator<T> : IEnumerable<T> {
    
    Action<Action<T>> collector;
    Func<Action, Action<OperationCompleteEvent>, ICoroutine> StartCoroutine;
    Func<int> WaitForSignal;
    Action<ICoroutine> WaitFor;
    
    public Iterator(Func<Action, Action<OperationCompleteEvent>, ICoroutine> sc, Func<int> wfs, Action<ICoroutine> wf, Action<Action<T>> col)
    {
      collector = col;
      StartCoroutine = sc;
      WaitForSignal = wfs;
      WaitFor = wf;
    }

    public IEnumerator<T> GetEnumerator() {
      return new IteratorEnum<T>(StartCoroutine, WaitForSignal, WaitFor, collector);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
  }
  
  public class IteratorEnum<T> : IEnumerator<T>
  {
    Action<ICoroutine> WaitFor;
    Func<Action, Action<OperationCompleteEvent>, ICoroutine> StartCoroutine;
    Func<int> WaitForSignal;
    ICoroutine coroutine;
    Queue<Action<T, bool>> WaitQueue = new Queue<Action<T, bool>>();
    T curr = default(T);
    int signalCount = 0;
    public T Current {get {
      return curr;
    }}
    object IEnumerator.Current {get {
      return Current;
    }}

    bool IsAlive { get {
      return coroutine != null && coroutine.IsAlive;
    }}

    Action<Action<T>> collector;
    public IteratorEnum(Func<Action, Action<OperationCompleteEvent>, ICoroutine> sc, Func<int> wfs, Action<ICoroutine> wf, Action<Action<T>> col)
    {
      StartCoroutine = sc;
      WaitForSignal = wfs;
      WaitFor = wf;
      curr = default(T);
      collector = col;
    }

    void Yield(T item)
    {
      if (!IsAlive) throw new Exception("Calling yield callback outside Iterator scope is not allowed.");
      WaitQueue.Dequeue()(item, false);
      if (--signalCount == 0) {
        coroutine.ResetSignals();
        WaitForSignal();
      }
    }

    public bool MoveNext()
    {
      if (coroutine == null) Reset();
      if (coroutine.IsAlive)
      {
        bool isEnd = false;
        ICoroutine wait = StartCoroutine(() => WaitForSignal(), null);
        WaitQueue.Enqueue((T found, bool end) => {
          curr = found;
          isEnd = end;
          wait.Signal();
        });
        signalCount++;
        coroutine.Signal();
        WaitFor(wait);
        return !isEnd && IsAlive;
      }
      return false;
    }

    public void Reset()
    {
      Dispose();
      signalCount = 0;
      curr = default(T);
      coroutine = StartCoroutine(() => {
        collector(Yield);     
        T temp = default(T);
        while(WaitQueue.Count > 0) WaitQueue.Dequeue()(temp, true); 
      }, null);
    }

    public void Dispose()
    {
      if (coroutine != null)
      {
        coroutine.Abort();
        coroutine = null;
        T temp = default(T);
        while(WaitQueue.Count > 0) WaitQueue.Dequeue()(temp, true); 
      }
    }
  }
}