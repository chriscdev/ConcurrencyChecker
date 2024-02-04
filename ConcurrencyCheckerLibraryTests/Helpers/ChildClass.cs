namespace ConcurrencyCheckerLibraryTests.Helpers
{
  public class ChildClass
  {
    public int IntegerProp { get; set; }
    private int _integerField;
    public string StringProp { get; set; }
    private string _stringField;
    public object ObjectProp { get; set; }
    private object _objectField;

    public ChildClass(int integerField, string stringField, object objectField)
    {
      _integerField = integerField;
      _stringField = stringField;
      _objectField = objectField;
    }
  }
}
