using System.Threading;
using System.Threading.Tasks;

namespace ConcurrencyCheckerLibraryTests.Helpers
{
  internal class ClassWithNoConcurrencyIssues
  {
    private string _prefixToWork = "No issues here.";
    private static string _prefixToWorkStatic = "No issues here.";
    private int _someCount = 1;
    private static int _someCountStatic = 1;

    public string DoSomeWorkAndReturnResponse()
    {
      Thread.Sleep(10);
      return $"{_prefixToWork} All seems good {_someCount}";
    }

    public async Task<string> DoSomeWorkAndReturnResponseAsync()
    {
      await Task.Delay(10);
      return $"{_prefixToWork} All seems good {_someCount}";
    }

    public static string DoSomeWorkAndReturnResponseStatic()
    {
      Thread.Sleep(10);
      return $"{_prefixToWorkStatic} All seems good {_someCountStatic}";
    }
  }
}
