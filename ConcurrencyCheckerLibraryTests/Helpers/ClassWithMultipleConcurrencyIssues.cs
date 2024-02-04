using System;

namespace ConcurrencyCheckerLibraryTests.Helpers
{
  public class ClassWithMultipleConcurrencyIssues
  {
    private string _name = "John";
    private string _surname = "Doe";
    public int Age { get; set; }
    private DateTime _dob = new DateTime(1986, 1, 1);
    public string Occupation { get; set; } = "Farmer";

    public void ChangeNameAndSurnameTo(string name, string surname)
    {
      _name = name;
      _surname = surname;
    }

    public void ChangeDob(DateTime dob)
    {
      _dob = dob;
    }
  }
}
