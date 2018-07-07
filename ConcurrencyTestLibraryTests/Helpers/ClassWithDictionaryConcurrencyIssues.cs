using System.Collections.Generic;

namespace ConcurrencyCheckerLibraryTests.Helpers
{
  public class ClassWithDictionaryConcurrencyIssues
  {
    protected IDictionary<string, int> Cache { get; } = new Dictionary<string, int> {{ "Existing", 0}};

    public void AddToCache(string key, int value)
    {
      Cache.Add(key, value); 
    }

    public void UpdateCache(string key, int value)
    {
      Cache[key] = value;
    }
  }
}
