using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConcurrencyCheckerLibraryTests.Helpers
{
  public class ClassWithAsyncDeadlock
  {
    private readonly Random _random = new Random();
    private readonly Dictionary<string, string> _dictionary = new Dictionary<string, string> {{"initial", "test"}};

    public bool ShouldDeadlock()
    {
      return DoSomethingAsync().Result;
    }

    public bool ShouldNotDeadLock()
    {
      return DoSomethingWithConfigureAwaitFalseAsync().Result;
    }

    public async Task DictionaryNotThreadSafeShouldLock()
    {
      var taskList = new List<Task>();

      for (var i = 0; i < 10000; i++)
      {
        taskList.Add(AddToDictionaryUnSafeAsync());
      }

      await Task.WhenAll(taskList);
    }

    private async Task<bool> DoSomethingAsync()
    {
      await Task.Delay(10);
      return true;
    }

    private async Task<bool> DoSomethingWithConfigureAwaitFalseAsync()
    {
      await Task.Delay(10).ConfigureAwait(false);
      return true;
    }

    private async Task AddToDictionaryUnSafeAsync()
    {
      await Task.Delay(_random.Next(5)).ConfigureAwait(false);

      var key = Guid.NewGuid().ToString();
      _dictionary.Add(key, "1");

      await Task.Delay(_random.Next(5)).ConfigureAwait(false);

      _dictionary.Remove(key);
    }
  }
}
