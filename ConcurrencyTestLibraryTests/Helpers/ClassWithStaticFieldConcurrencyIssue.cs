namespace ConcurrencyCheckerLibraryTests.Helpers
{
  public class ClassWithStaticFieldConcurrencyIssue
  {
    private static string _name = "John";

    public void ChangeNameTo(string name)
    {
      _name = name;
    }
  }
}
