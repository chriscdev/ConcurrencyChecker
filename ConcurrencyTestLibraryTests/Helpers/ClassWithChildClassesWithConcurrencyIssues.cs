using System.Collections.Generic;

namespace ConcurrencyCheckerLibraryTests.Helpers
{
  internal class ClassWithChildClassesAndConcurrencyIssues
  {
    private ChildClass _childField;
    public ChildClass ChildProp { get; set; }
    private IEnumerable<ChildClass> _childEnumerableField;
    public List<ChildClass> ChildEnumerableProp { get; set; }

    public ClassWithChildClassesAndConcurrencyIssues(ChildClass childField, IEnumerable<ChildClass> childEnumerableField)
    {
      _childField = childField;
      _childEnumerableField = childEnumerableField;
    }

    public void SetChildField(ChildClass newChildClass)
    {
      _childField = newChildClass;
    }

    public void SetChildEnumerableField(IEnumerable<ChildClass> newChildClassEnumerable)
    {
      _childEnumerableField = newChildClassEnumerable;
    }
  }
}
