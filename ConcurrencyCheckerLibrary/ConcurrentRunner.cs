using Nito.AsyncEx;

namespace ConcurrencyCheckerLibrary
{
  internal class ConcurrentRunner
  {
    public static void Start(Action[] actions, int numberOfConcurrentThreads, CancellationToken ct)
    {
      var parallelTaskList = new List<Task>(numberOfConcurrentThreads);

      for (var i = 0; i < numberOfConcurrentThreads; i++)
      {
        var indexOfAction = i % actions.Length;

        var task = Task.Run(() => AsyncContext.Run(actions[indexOfAction]), ct);

        parallelTaskList.Add(task);
      }

      Task.WaitAll(parallelTaskList.ToArray(), ct);
    }

    public static IEnumerable<T> Start<T>(Func<T>[] funcs, int numberOfConcurrentThreads, CancellationToken ct)
    {
      var parallelTaskList = new List<Task<T>>(numberOfConcurrentThreads);

      for (var i = 0; i < numberOfConcurrentThreads; i++)
      {
        var indexOfAction = i % funcs.Length;

        var task = Task.Run(() => AsyncContext.Run(funcs[indexOfAction]), ct);
        parallelTaskList.Add(task);
      }

      return Task.WhenAll(parallelTaskList).Result;
    }

    public static void Start<T>(Func<T>[] funcs, Action<T>[] tests, int numberOfConcurrentThreads, CancellationToken ct)
    {
      var parallelTaskList = new List<Task>(numberOfConcurrentThreads);

      for (var i = 0; i < numberOfConcurrentThreads; i++)
      {
        var indexOfAction = i % funcs.Length;

        var task = Task.Run(() => AsyncContext.Run(funcs[indexOfAction]), ct).ContinueWith(t => tests[indexOfAction](t.Result), ct);
        parallelTaskList.Add(task);
      }

      Task.WaitAll(parallelTaskList.ToArray(), ct);
    }

    public static void Start<T>(Func<Task<T>>[] funcs, Action<T>[] tests, int numberOfConcurrentThreads, CancellationToken ct)
    {
      var parallelTaskList = new List<Task>(numberOfConcurrentThreads);

      for (var i = 0; i < numberOfConcurrentThreads; i++)
      {
        var indexOfAction = i % funcs.Length;

        var task = Task.Run(() => AsyncContext.Run(funcs[indexOfAction]), ct).ContinueWith(t => tests[indexOfAction](t.Result), ct);

        parallelTaskList.Add(task);
      }

      Task.WaitAll(parallelTaskList.ToArray(), ct);
    }
  }
}
