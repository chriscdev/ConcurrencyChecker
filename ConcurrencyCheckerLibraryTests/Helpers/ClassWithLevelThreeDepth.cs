namespace ConcurrencyCheckerLibraryTests.Helpers
{
  public class ClassWithLevelThreeDepth
  {
    public ClassWithChildClassPropertyWithConcurrencyIssues ChildPropDepthOne { get; set; } = 
      new ClassWithChildClassPropertyWithConcurrencyIssues
      {
        ChildProp = new ChildClass(1, "one", new {Name = "one"})
      };
  }
}
