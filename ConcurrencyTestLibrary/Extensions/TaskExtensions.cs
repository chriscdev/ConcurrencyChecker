using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrencyCheckerLibrary.Extensions
{
  internal static class TaskExtensions
  {
    public static void WaitWhen(this Task task, Predicate<Task> waitPredicate, int sleepTillNextCheckMs = 5)
    {
      while (waitPredicate(task))
      {
        Thread.Sleep(sleepTillNextCheckMs);
      }
    }
  }
}
