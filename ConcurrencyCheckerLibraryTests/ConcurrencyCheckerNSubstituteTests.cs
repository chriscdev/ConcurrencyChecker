using ConcurrencyCheckerLibrary;
using ConcurrencyCheckerLibraryTests.Helpers;
using NSubstitute;

namespace ConcurrencyCheckerLibraryTests
{
  [TestFixture]
  public class ConcurrencyCheckerNSubstituteTests
  {
    [Test]
    public void Run_Given_ClassUsingNSubstitute_Should_ReturnReportWithIssues()
    {
      var instance = Substitute.For<ClassWithPropertiesConcurrencyIssue>();
      var finder = new ConcurrencyChecker(instance);

      var report = finder.Run(10, () => instance.ChangeAddress("Ring Road", "Johannesburg"),
        () => instance.ChangeAddress("Justice Street", "Polokwane"));

      Console.WriteLine(report);

      Assert.IsNotNull(report);
      Assert.IsTrue(report.Contains(
        "ClassWithPropertiesConcurrencyIssue->AddressLine1: Reference and actual number of value changes does not match."));
      Assert.IsTrue(report.Contains(
        "ClassWithPropertiesConcurrencyIssue->AddressLine1: Reference Value Changes: \n0: String = 111 Church Street\n1: String = Ring Road"));
      Assert.IsTrue(report.Contains(
        "ClassWithPropertiesConcurrencyIssue->AddressLine1: Actual Value Changes: \n0: String = 111 Church Street\n1: String = Ring Road\n2: String = Justice Street"));
      Assert.IsTrue(report.Contains(
        "ClassWithPropertiesConcurrencyIssue->AddressLine2: Reference and actual number of value changes does not match."));
      Assert.IsTrue(report.Contains(
        "ClassWithPropertiesConcurrencyIssue->AddressLine2: Reference Value Changes: \n0: String = Pretoria\n1: String = Johannesburg"));
      Assert.IsTrue(report.Contains(
        "ClassWithPropertiesConcurrencyIssue->AddressLine2: Actual Value Changes: \n0: String = Pretoria\n1: String = Johannesburg\n2: String = Polokwane"));
    }

    [Test]
    public void Run_Given_ClassWithMultipleConcurrencyIssuesAndUsingNsubstitute_Should_ReturnReportWithIssues()
    {
      var instance = Substitute.For<ClassWithMultipleConcurrencyIssues>();
      var finder = new ConcurrencyChecker(instance);

      var report = finder.Run(5,
        () =>
        {
          instance.ChangeNameAndSurnameTo("Jane", "");
          instance.ChangeDob(new DateTime(1986, 2, 3));
          instance.Age = 33;
        },
        () =>
        {
          instance.ChangeNameAndSurnameTo("Some", "One");
          instance.ChangeDob(new DateTime(1987, 2, 3));
          instance.Age = 35;
        });

      Console.WriteLine(report);

      Assert.IsNotNull(report);
      Assert.IsTrue(report.Contains(
        "ClassWithMultipleConcurrencyIssues->_name: Reference and actual number of value changes does not match."));
      Assert.IsTrue(report.Contains(
        "ClassWithMultipleConcurrencyIssues->_name: Reference Value Changes: \n0: String = John\n1: String = Jane"));
      Assert.IsTrue(report.Contains(
        "ClassWithMultipleConcurrencyIssues->_name: Actual Value Changes: \n0: String = John\n1: String = Jane\n2: String = Some"));
      Assert.IsTrue(report.Contains(
        "ClassWithMultipleConcurrencyIssues->_surname: Reference and actual number of value changes does not match."));
      Assert.IsTrue(report.Contains(
        "ClassWithMultipleConcurrencyIssues->_surname: Reference Value Changes: \n0: String = Doe\n1: String = "));
      Assert.IsTrue(report.Contains(
        "ClassWithMultipleConcurrencyIssues->_surname: Actual Value Changes: \n0: String = Doe\n1: String = \n2: String = One"));
      Assert.IsTrue(report.Contains(
        "ClassWithMultipleConcurrencyIssues->_dob: Reference and actual number of value changes does not match."));
      Assert.IsTrue(report.Contains(
        $"ClassWithMultipleConcurrencyIssues->_dob: Reference Value Changes: \n0: DateTime = {new DateTime(1986, 1, 1)}\n1: DateTime = {new DateTime(1986, 2, 3)}"));
      Assert.IsTrue(report.Contains(
        $"ClassWithMultipleConcurrencyIssues->_dob: Actual Value Changes: \n0: DateTime = {new DateTime(1986, 1, 1)}\n1: DateTime = {new DateTime(1986, 2, 3)}\n2: DateTime = {new DateTime(1987, 2, 3)}"));
      Assert.IsTrue(report.Contains(
        "ClassWithMultipleConcurrencyIssues->Age: Reference and actual number of value changes does not match."));
      Assert.IsTrue(report.Contains("ClassWithMultipleConcurrencyIssues->Age: Reference Value Changes: \n0: Int32 = 0\n1: Int32 = 33"));
      Assert.IsTrue(report.Contains(
        "ClassWithMultipleConcurrencyIssues->Age: Actual Value Changes: \n0: Int32 = 0\n1: Int32 = 33\n2: Int32 = 35"));
    }

    [Test]
    public void Run_Given_ClassWithChildClassesAndConcurrencyIssuesUsingNsubstitute_With_ChangeMultiplePropertyOnChildArray_Should_ReturnReportWithIssue()
    {
      var childClass = Substitute.For<ChildClass>(0, "zero", new { First = 0 });
      var instance =
        new ClassWithChildClassesAndConcurrencyIssues(childClass, null)
        {
          ChildEnumerableProp = new List<ChildClass>
          {
            new ChildClass(1, "one", new {First = 1}),
            new ChildClass(2, "two", new {First = 2})
          }
        };


      var finder = new ConcurrencyChecker(instance);
      instance.ChildProp = new ChildClass(3, "three", new { First = 3 });

      var report = finder.Run(20,
        () =>
        {
          instance.ChildEnumerableProp[0].IntegerProp = 5;
          instance.ChildEnumerableProp[0].StringProp = "Five";
          instance.ChildEnumerableProp[0].ObjectProp = new { First = 5 };
        },
        () =>
        {
          instance.ChildEnumerableProp[0].IntegerProp = 100;
          instance.ChildEnumerableProp[0].StringProp = "Hundred";
          instance.ChildEnumerableProp[0].ObjectProp = new { First = 100 };
        });

      Console.WriteLine(report);

      Assert.IsNotNull(report);
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassesAndConcurrencyIssues->ChildEnumerableProp[0]->IntegerProp: Reference and actual number of value changes does not match."));
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassesAndConcurrencyIssues->ChildEnumerableProp[0]->StringProp: Reference and actual number of value changes does not match."));
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassesAndConcurrencyIssues->ChildEnumerableProp[0]->ObjectProp: Reference and actual number of value changes does not match."));
    }
  }
}
