namespace ConcurrencyCheckerLibraryTests.Helpers
{
  public class ClassWithTypeChangeConcurrencyIssue
  {
    private IFoo _foo = new ClassWithTypeChangeConcurrencyIssue.Foo1();

    public void SetFoo(IFoo foo)
    {
      _foo = foo;
    }

    public interface IFoo
    {
      string Bar { get; set; }
    }

    public class Foo1 : IFoo
    {
      public string Bar { get; set; }
    }

    public class Foo2 : IFoo
    {
      public string Bar { get; set; }
    }
  }
}
