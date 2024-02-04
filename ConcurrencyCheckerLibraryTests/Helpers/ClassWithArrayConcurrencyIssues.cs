using System.Collections.Generic;

namespace ConcurrencyCheckerLibraryTests.Helpers
{
  public class ClassWithArrayConcurrencyIssues
  {
    public string[] Names { get; set; } = { "John", "Doe" };
  }

  public class ClassWithObjectArrayConcurrencyIssues
  {
    public object[] Names { get; set; } = { new { Name = "John" } };
  }
}
