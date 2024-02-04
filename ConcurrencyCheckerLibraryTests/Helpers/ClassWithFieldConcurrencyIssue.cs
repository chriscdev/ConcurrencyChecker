using System;

namespace ConcurrencyCheckerLibraryTests.Helpers
{
  public class ClassWithFieldConcurrencyIssue
  {
    private string _name = "John";
    private string _surname = "Doe";
    private int age = 31;
    private DateTime _dob = new DateTime(1986, 1, 1);
    private string Occupation = "Farmer";

    public void ChangeNameTo(string name)
    {
      _name = name;
    }
  }
}
