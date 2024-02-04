using ConcurrencyCheckerLibrary;
using ConcurrencyCheckerLibrary.Exceptions;
using ConcurrencyCheckerLibraryTests.Helpers;

namespace ConcurrencyCheckerLibraryTests
{
  [TestFixture]
  public class ConcurrencyCheckerTests
  {
    [Test]
    public void AssertRun_Given_ClassWithStaticFieldConcurrencyIssue_Should_ThrowException()
    {
      var instance = new ClassWithStaticFieldConcurrencyIssue();
      var finder = new ConcurrencyChecker(instance);

      instance.ChangeNameTo("John");

      Assert.Throws<ConcurrencyException>(() => finder.Assert(5, 
        () => instance.ChangeNameTo("Jane"),
        () => instance.ChangeNameTo("Peter")));
    }

    [Test]
    public void Run_Given_ClassWithChildClassesAndConcurrencyIssues_With_ChangeMultiplePropertyOnChildArray_Should_ReturnReportWithIssue()
    {
      var instance =
        new ClassWithChildClassesAndConcurrencyIssues(new ChildClass(0, "zero", new {First = 0}), null)
        {
          ChildEnumerableProp = new List<ChildClass>
          {
            new ChildClass(1, "one", new {First = 1}),
            new ChildClass(2, "two", new {First = 2})
          }
        };


      var finder = new ConcurrencyChecker(instance);
      instance.ChildProp = new ChildClass(3, "three", new {First = 3});

      var report = finder.Run(20,
        () =>
        {
          instance.ChildEnumerableProp[0].IntegerProp = 5;
          instance.ChildEnumerableProp[0].StringProp = "Five";
          instance.ChildEnumerableProp[0].ObjectProp = new {First = 5};
        },
        () =>
        {
          instance.ChildEnumerableProp[0].IntegerProp = 100;
          instance.ChildEnumerableProp[0].StringProp = "Hundred";
          instance.ChildEnumerableProp[0].ObjectProp = new {First = 100};
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

    [Test]
    public void Run_Given_ClassWithChildClassesAndConcurrencyIssues_With_ChangeMultiplePropertyOnMultipleChildArray_And_FullReportAsFalse_Should_ReturnReportWithIssue()
    {
      var instance =
        new ClassWithChildClassesAndConcurrencyIssues(new ChildClass(0, "zero", new {First = 0}), null)
        {
          ChildEnumerableProp = new List<ChildClass>
          {
            new ChildClass(1, "one", new {First = 1}),
            new ChildClass(2, "two", new {First = 2})
          }
        };


      var finder = new ConcurrencyChecker(instance);
      instance.ChildProp = new ChildClass(3, "three", new {First = 3});

      var report = finder.Run(20,
        () =>
        {
          instance.ChildEnumerableProp[0].IntegerProp = 5;
          instance.ChildEnumerableProp[0].StringProp = "Five";
          instance.ChildEnumerableProp[0].ObjectProp = new {First = 5};
          instance.ChildEnumerableProp[1].IntegerProp = 6;
          instance.ChildEnumerableProp[1].StringProp = "Six";
          instance.ChildEnumerableProp[1].ObjectProp = new {First = 6};
        },
        () =>
        {
          instance.ChildEnumerableProp[0].IntegerProp = 100;
          instance.ChildEnumerableProp[0].StringProp = "Hundred";
          instance.ChildEnumerableProp[0].ObjectProp = new {First = 100};
          instance.ChildEnumerableProp[1].IntegerProp = 80;
          instance.ChildEnumerableProp[1].StringProp = "Eighty";
          instance.ChildEnumerableProp[1].ObjectProp = new {First = 80};
        });

      Console.WriteLine(report);

      Assert.IsNotNull(report);
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassesAndConcurrencyIssues->ChildEnumerableProp[0]->IntegerProp: Reference and actual number of value changes does not match."));
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassesAndConcurrencyIssues->ChildEnumerableProp[0]->StringProp: Reference and actual number of value changes does not match."));
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassesAndConcurrencyIssues->ChildEnumerableProp[0]->ObjectProp: Reference and actual number of value changes does not match."));
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassesAndConcurrencyIssues->ChildEnumerableProp[1]->IntegerProp: Reference and actual number of value changes does not match."));
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassesAndConcurrencyIssues->ChildEnumerableProp[1]->StringProp: Reference and actual number of value changes does not match."));
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassesAndConcurrencyIssues->ChildEnumerableProp[1]->ObjectProp: Reference and actual number of value changes does not match."));
    }

    [Test]
    public void Run_Given_ClassWithChildClassesAndConcurrencyIssues_With_ChangeMultiplePropertyOnMultipleChildArray_Should_ReturnReportWithIssue()
    {
      var instance =
        new ClassWithChildClassesAndConcurrencyIssues(new ChildClass(0, "zero", new {First = 0}), null)
        {
          ChildEnumerableProp = new List<ChildClass>
          {
            new ChildClass(1, "one", new {First = 1}),
            new ChildClass(2, "two", new {First = 2})
          }
        };


      var finder = new ConcurrencyChecker(instance);
      instance.ChildProp = new ChildClass(3, "three", new {First = 3});

      var report = finder.Run(20,
        () =>
        {
          instance.ChildEnumerableProp[0].IntegerProp = 5;
          instance.ChildEnumerableProp[0].StringProp = "Five";
          instance.ChildEnumerableProp[0].ObjectProp = new {First = 5};
          instance.ChildEnumerableProp[1].IntegerProp = 6;
          instance.ChildEnumerableProp[1].StringProp = "Six";
          instance.ChildEnumerableProp[1].ObjectProp = new {First = 6};
        },
        () =>
        {
          instance.ChildEnumerableProp[0].IntegerProp = 100;
          instance.ChildEnumerableProp[0].StringProp = "Hundred";
          instance.ChildEnumerableProp[0].ObjectProp = new {First = 100};
          instance.ChildEnumerableProp[1].IntegerProp = 80;
          instance.ChildEnumerableProp[1].StringProp = "Eighty";
          instance.ChildEnumerableProp[1].ObjectProp = new {First = 80};
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

    [Test]
    public void Run_Given_ClassWithChildClassesAndConcurrencyIssues_With_ChangePropertyOnChild_Should_ReturnReportWithIssue()
    {
      var instance = new ClassWithChildClassesAndConcurrencyIssues(new ChildClass(0, "zero", new {First = 0}), new[]
      {
        new ChildClass(1, "one", new {First = 1}),
        new ChildClass(2, "two", new {First = 2})
      });

      var finder = new ConcurrencyChecker(instance);
      instance.ChildProp = new ChildClass(3, "three", new {First = 3});

      var report = finder.Run(20, () =>
      {
        instance.ChildProp.IntegerProp = 99;
        Thread.Sleep(0);
      }, () => { instance.ChildProp.IntegerProp = 100; });

      Console.WriteLine(report);

      Assert.IsNotNull(report);
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassesAndConcurrencyIssues->ChildProp->IntegerProp: Reference and actual number of value changes does not match."));
    }

    [Test]
    public void Run_Given_ClassWithChildClassesAndConcurrencyIssues_With_ChangePropertyOnChildArray_Should_ReturnReportWithIssue()
    {
      var instance =
        new ClassWithChildClassesAndConcurrencyIssues(new ChildClass(0, "zero", new {First = 0}), null)
        {
          ChildEnumerableProp = new List<ChildClass>
          {
            new ChildClass(1, "one", new {First = 1}),
            new ChildClass(2, "two", new {First = 2})
          }
        };


      var finder = new ConcurrencyChecker(instance);
      instance.ChildProp = new ChildClass(3, "three", new {First = 3});

      var report = finder.Run(20, () => { instance.ChildEnumerableProp[0].IntegerProp = 5; },
        () => { instance.ChildEnumerableProp[0].IntegerProp = 100; });

      Console.WriteLine(report);

      Assert.IsNotNull(report);
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassesAndConcurrencyIssues->ChildEnumerableProp[0]->IntegerProp: Reference and actual number of value changes does not match."));
    }

    [Test]
    public void Run_Given_ClassWithChildClassesAndConcurrencyIssues_With_ChildEnumerableFieldWithIssue_Should_ReturnReportWithIssue()
    {
      var instance = new ClassWithChildClassesAndConcurrencyIssues(new ChildClass(0, "zero", new {First = 0}), new[]
      {
        new ChildClass(1, "one", new {First = 1}),
        new ChildClass(2, "two", new {First = 2})
      });

      var finder = new ConcurrencyChecker(instance);

      var report = finder.Run(5,
        () => instance.SetChildEnumerableField(new[] {new ChildClass(99, "ninety nine", new {First = 99})}),
        () => instance.SetChildEnumerableField(new[] {new ChildClass(100, "hundred", new {First = 100})}));

      Console.WriteLine(report);

      Assert.IsNotNull(report);
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassesAndConcurrencyIssues->_childEnumerableField: Reference and actual number of value changes does not match."));
    }

    [Test]
    public void Run_Given_ClassWithChildClassesAndConcurrencyIssues_With_ChildEnumerablePropertyWithIssue_Should_ReturnReportWithIssue()
    {
      var instance = new ClassWithChildClassesAndConcurrencyIssues(new ChildClass(0, "zero", new {First = 0}), new[]
      {
        new ChildClass(1, "one", new {First = 1}),
        new ChildClass(2, "two", new {First = 2})
      });

      var finder = new ConcurrencyChecker(instance);

      var report = finder.Run(5,
        () => instance.ChildEnumerableProp = new List<ChildClass> {new ChildClass(99, "ninety nine", new {First = 99})},
        () => instance.ChildEnumerableProp = new List<ChildClass> {new ChildClass(100, "hundred", new {First = 100})});

      Console.WriteLine(report);

      Assert.IsNotNull(report);
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassesAndConcurrencyIssues->ChildEnumerableProp: Reference and actual number of value changes does not match."));
    }

    [Test]
    public void Run_Given_ClassWithChildClassesAndConcurrencyIssues_With_ClassFieldWithIssue_Should_ReturnReportWithIssue()
    {
      var instance = new ClassWithChildClassesAndConcurrencyIssues(new ChildClass(0, "zero", new {First = 0}), new[]
      {
        new ChildClass(1, "one", new {First = 1}),
        new ChildClass(2, "two", new {First = 2})
      });

      var finder = new ConcurrencyChecker(instance);

      var report = finder.Run(5, () => instance.SetChildField(new ChildClass(99, "ninety nine", new {First = 99})),
        () => instance.SetChildField(new ChildClass(100, "hundred", new {First = 100})));

      Console.WriteLine(report);

      Assert.IsNotNull(report);
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassesAndConcurrencyIssues->_childField: Reference and actual number of value changes does not match."));
    }

    [Test]
    public void Run_Given_ClassWithChildClassesAndConcurrencyIssues_With_ClassPropWithIssue_Should_ReturnReportWithIssue()
    {
      var instance = new ClassWithChildClassesAndConcurrencyIssues(new ChildClass(0, "zero", new {First = 0}), new[]
      {
        new ChildClass(1, "one", new {First = 1}),
        new ChildClass(2, "two", new {First = 2})
      });

      var finder = new ConcurrencyChecker(instance);

      var report = finder.Run(5, () => instance.ChildProp = new ChildClass(99, "ninety nine", new {First = 99}),
        () => instance.ChildProp = new ChildClass(100, "hundred", new {First = 100}));

      Console.WriteLine(report);

      Assert.IsNotNull(report);
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassesAndConcurrencyIssues->ChildProp: Reference and actual number of value changes does not match."));
    }

    [Test]
    public void Run_Given_ClassWithChildClassesAndConcurrencyIssues_With_InitialisingAddingToChildEnumerableProperty_Should_ReturnReportWithIssue()
    {
      var instance = new ClassWithChildClassesAndConcurrencyIssues(new ChildClass(0, "zero", new {First = 0}), new[]
      {
        new ChildClass(1, "one", new {First = 1}),
        new ChildClass(2, "two", new {First = 2})
      });

      var finder = new ConcurrencyChecker(instance);

      var report = finder.Run(5,
        () =>
        {
          instance.ChildEnumerableProp = new List<ChildClass>();
          instance.ChildEnumerableProp.Add(new ChildClass(99, "ninety nine", new {First = 99}));
        },
        () =>
        {
          instance.ChildEnumerableProp = new List<ChildClass>();
          instance.ChildEnumerableProp.AddRange(new[]
            {new ChildClass(100, "hundred", new {First = 100}), new ChildClass(100, "hundred", new {First = 100})});
        });

      Console.WriteLine(report);

      Assert.IsNotNull(report);
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassesAndConcurrencyIssues->ChildEnumerableProp: Reference and actual number of value changes does not match."));
    }

    [Test]
    public void Run_Given_ClassWithChildClassPropertyAndConcurrencyIssues_With_ChangePropertyOnChild_Should_ReturnReportWithIssue()
    {
      var instance = new ClassWithChildClassPropertyWithConcurrencyIssues();

      var finder = new ConcurrencyChecker(instance);
      instance.ChildProp = new ChildClass(1, "one", new {First = 1});

      var report = finder.Run(20, () =>
      {
        instance.ChildProp.IntegerProp = 2;
        Thread.Sleep(0);
      }, () => { instance.ChildProp.IntegerProp = 3; });

      Console.WriteLine(report);

      Assert.IsNotNull(report);
      Assert.IsTrue(report.Contains(
        "ClassWithChildClassPropertyWithConcurrencyIssues->ChildProp->IntegerProp: Reference and actual number of value changes does not match."));
    }

    [Test]
    public void Run_Given_ClassWithFieldConcurrencyIssue_Should_ReturnReportWithIssue()
    {
      var instance = new ClassWithFieldConcurrencyIssue();
      var finder = new ConcurrencyChecker(instance);

      var report = finder.Run(10, () => instance.ChangeNameTo("Jane"), () => instance.ChangeNameTo("Peter"));

      Console.WriteLine(report);

      Assert.IsNotNull(report);
      Assert.IsTrue(report.Contains(
        "ClassWithFieldConcurrencyIssue->_name: Reference and actual number of value changes does not match."));
      Assert.IsTrue(report.Contains(
        "ClassWithFieldConcurrencyIssue->_name: Reference Value Changes: \n0: String = John\n1: String = Jane"));
      Assert.IsTrue(report.Contains(
        "ClassWithFieldConcurrencyIssue->_name: Actual Value Changes: \n0: String = John\n1: String = Jane\n2: String = Peter"));
    }

    [Test]
    public void Run_Given_ClassWithMultipleConcurrencyIssues_Should_ReturnReportWithIssues()
    {
      var instance = new ClassWithMultipleConcurrencyIssues();
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
        "ClassWithMultipleConcurrencyIssues->_surname: Reference Value Changes: \n0: String = Doe\n1: String ="));
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
    public void Run_Given_ClassWithNoConcurrencyIssue_When_Calling_AsyncMethod_Should_ReturnWithNoIssues()
    {
      var instance = new ClassWithNoConcurrencyIssues();
      var finder = new ConcurrencyChecker(instance);

      var report = finder.Run(5, async () => await instance.DoSomeWorkAndReturnResponseAsync(),
        async () => await instance.DoSomeWorkAndReturnResponseAsync());

      Console.WriteLine(report);

      Assert.IsNull(report);
    }

    [Test]
    public void Run_Given_ClassWithNoConcurrencyIssue_When_Calling_Method_Should_ReturnWithNoIssues()
    {
      var instance = new ClassWithNoConcurrencyIssues();
      var finder = new ConcurrencyChecker(instance);

      var report = finder.Run(5, () => instance.DoSomeWorkAndReturnResponse(),
        () => instance.DoSomeWorkAndReturnResponse());

      Console.WriteLine(report);

      Assert.IsNull(report);
    }

    [Test]
    public void Run_Given_ClassWithNoConcurrencyIssue_When_Calling_StaticMethod_Should_ReturnWithNoIssues()
    {
      var instance = new ClassWithNoConcurrencyIssues();
      var finder = new ConcurrencyChecker(instance);

      var report = finder.Run(5, () => ClassWithNoConcurrencyIssues.DoSomeWorkAndReturnResponseStatic(),
        () => ClassWithNoConcurrencyIssues.DoSomeWorkAndReturnResponseStatic());

      Console.WriteLine(report);

      Assert.IsNull(report);
    }

    [Test]
    public void Run_Given_ClassWithPropertyConcurrencyIssues_Should_ReturnReportWithIssues()
    {
      var instance = new ClassWithPropertiesConcurrencyIssue();
      var finder = new ConcurrencyChecker(instance);

      var report = finder.Run(4, () => instance.ChangeAddress("Ring Road", "Johannesburg"),
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
    public void Run_Given_ClassWithStaticFieldConcurrencyIssue_Should_ReturnReportWithIssue()
    {
      var instance = new ClassWithStaticFieldConcurrencyIssue();
      var finder = new ConcurrencyChecker(instance);

      instance.ChangeNameTo("John");
      var report = finder.Run(5, () => instance.ChangeNameTo("Jane"), () => instance.ChangeNameTo("Peter"));

      Console.WriteLine(report);

      Assert.IsNotNull(report);
      Assert.IsTrue(report.Contains(
        "ClassWithStaticFieldConcurrencyIssue->_name: Reference and actual number of value changes does not match."));
      Assert.IsTrue(report.Contains(
        "ClassWithStaticFieldConcurrencyIssue->_name: Reference Value Changes: \n0: String = John\n1: String = Jane"));
      Assert.IsTrue(report.Contains(
        "ClassWithStaticFieldConcurrencyIssue->_name: Actual Value Changes: \n0: String = John\n1: String = Jane\n2: String = Peter"));
    }

    [Test]
    public void Run_Given_ClassWithPropertyConcurrencyIssues_And_IgnoreMemberPath_Should_ReturnReportWithNoIssues()
    {
      var instance = new ClassWithPropertiesConcurrencyIssue();
      var finder = new ConcurrencyChecker(instance, "ClassWithPropertiesConcurrencyIssue->AddressLine1");

      var report = finder.Run(4, () => instance.ChangeAddress("Ring Road", "Johannesburg"),
        () => instance.ChangeAddress("Justice Street", "Polokwane"));

      Console.WriteLine(report);

      Assert.IsNotNull(report);
      Assert.IsFalse(report.Contains(
        "ClassWithPropertiesConcurrencyIssue->AddressLine1: Reference and actual number of value changes does not match."));
      Assert.IsFalse(report.Contains(
        "ClassWithPropertiesConcurrencyIssue->AddressLine1: Reference Value Changes: \n0: String = 111 Church Street\n1: String = Ring Road"));
      Assert.IsFalse(report.Contains(
        "ClassWithPropertiesConcurrencyIssue->AddressLine1: Actual Value Changes: \n0: String = 111 Church Street\n1: String = Ring Road\n2: String = Justice Street"));
      Assert.IsTrue(report.Contains(
        "ClassWithPropertiesConcurrencyIssue->AddressLine2: Reference and actual number of value changes does not match."));
      Assert.IsTrue(report.Contains(
        "ClassWithPropertiesConcurrencyIssue->AddressLine2: Reference Value Changes: \n0: String = Pretoria\n1: String = Johannesburg"));
      Assert.IsTrue(report.Contains(
        "ClassWithPropertiesConcurrencyIssue->AddressLine2: Actual Value Changes: \n0: String = Pretoria\n1: String = Johannesburg\n2: String = Polokwane"));
    }

    [Test]
    public void Run_Given__ClassWithPropertyConcurrencyIssues_And_MultipleIgnoreMemberPath_Should_ReturnReportWithNoIssues()
    {
      var instance = new ClassWithPropertiesConcurrencyIssue();
      var finder = new ConcurrencyChecker(instance, "ClassWithPropertiesConcurrencyIssue->AddressLine1", "ClassWithPropertiesConcurrencyIssue->AddressLine2" );

      var report = finder.Run(4, () => instance.ChangeAddress("Ring Road", "Johannesburg"),
        () => instance.ChangeAddress("Justice Street", "Polokwane"));

      Console.WriteLine(report);

      Assert.IsNull(report);
    }

    [Test]
    public void Run_Given_ClassWithChildClassesAndConcurrencyIssues_With_ChangePropertyOnChildArray_AndIgnoreMember_Should_ReturnReportWithIssue()
    {
      var instance =
        new ClassWithChildClassesAndConcurrencyIssues(new ChildClass(0, "zero", new { First = 0 }), null)
        {
          ChildEnumerableProp = new List<ChildClass>
          {
            new ChildClass(1, "one", new {First = 1}),
            new ChildClass(2, "two", new {First = 2})
          }
        };

      var finder = new ConcurrencyChecker(instance, "ClassWithChildClassesAndConcurrencyIssues->ChildEnumerableProp[0]->IntegerProp");
      instance.ChildProp = new ChildClass(3, "three", new { First = 3 });

      var report = finder.Run(20, () => { instance.ChildEnumerableProp[0].IntegerProp = 5; },
        () => { instance.ChildEnumerableProp[0].IntegerProp = 100; });

      Console.WriteLine(report);

      Assert.IsNull(report);
    }

    [Test]
    public void AssertRun_Given_ClassWithAsyncDeadlock_Should_ThrowException()
    {
      var instance = new ClassWithAsyncDeadlock();
      var finder = new ConcurrencyChecker(instance, null, 500);

      var error = Assert.Throws<ConcurrencyException>(() => finder.Assert(10, () => instance.ShouldDeadlock()));

      Assert.AreEqual("Possible deadlock detected. Make sure that you do not use .Wait() or .Result on async methods.", error.Message);
    }

    [Test]
    public void AssertRun_Given_ClassWithNoAsyncDeadlock_Should_Succeed()
    {
      var instance = new ClassWithAsyncDeadlock();
      var finder = new ConcurrencyChecker(instance, null, 2000);

      finder.Assert(1, () => instance.ShouldNotDeadLock());
    }

    [Test]
    public void AssertRun_Given_ClassWithDictionaryAsyncDeadlock_Should_ThrowException()
    {
      var instance = new ClassWithAsyncDeadlock();
      var finder = new ConcurrencyChecker(instance, null, 500);

      var error = Assert.Throws<ConcurrencyException>(() => finder.Assert(5, async () => await instance.DictionaryNotThreadSafeShouldLock()));

      Assert.AreEqual("Possible deadlock detected. Make sure that you do not use .Wait() or .Result on async methods.", error.Message);
    }

    [Test]
    public void Run_Given_ClassWithDictionaryConcurrencyIssues_With_ChangePropertyOnChildDictionary_Should_ThrowConcurrencyException()
    {
      var instance = new ClassWithDictionaryConcurrencyIssues();

      var finder = new ConcurrencyChecker(instance, null, 2000);

      var error = Assert.Throws<ConcurrencyException>(() => finder.Assert(2000, () => { instance.UpdateCache("Existing", 1); },
        () => { instance.UpdateCache("Existing", 2); }));

      Console.WriteLine(error.Message);
      Assert.IsTrue(error.Message.Contains("ClassWithDictionaryConcurrencyIssues->Cache: Possible concurrency issue because of the following exception: Collection was modified; enumeration operation may not execute."));
    }

    [Test]
    public void Run_Given_ClassWithDictionaryConcurrencyIssues_With_AddChangePropertyOnChildDictionary_Should_Succeed()
    {
      var instance = new ClassWithDictionaryConcurrencyIssues();

      var finder = new ConcurrencyChecker(instance, null, 2000);

      var error = Assert.Throws<ConcurrencyException>(() => finder.Assert(2000, () => { instance.AddToCache(Guid.NewGuid().ToString(), 1); },
        () => { Thread.Sleep(1); }));

      Console.WriteLine(error.Message);
      Assert.IsTrue(error.Message.Contains("ClassWithDictionaryConcurrencyIssues->Cache: Possible concurrency issue because of the following exception: Collection was modified; enumeration operation may not execute."));
    }

    [Test]
    public void Run_Given_ClassWithArrayConcurrencyIssues_With_ChangePropertyOnChildArray_Should_Succeed()
    {
      var instance = new ClassWithArrayConcurrencyIssues();

      var finder = new ConcurrencyChecker(instance, null, 2000);

      finder.Assert(600, () => { instance.Names[0] = "Jane"; },
        () =>
        {
          var name = instance.Names[0];
        });
    }

    [Test]
    public void Run_Given_ClassWithArrayConcurrencyIssues_With_DoNotChangePropertyOnChildArray_Should_ThrowConcurrencyException()
    {
      var instance = new ClassWithArrayConcurrencyIssues();

      var finder = new ConcurrencyChecker(instance, null, 2000);

      var error = Assert.Throws<ConcurrencyException>(() => finder.Assert(2000, () => { instance.Names[0] = "Jane"; },
        () => { instance.Names[0] = "Dave"; }));

      Console.WriteLine(error.Message);
    }

    [Test]
    public void Run_Given_ClassWithObjectArrayConcurrencyIssues_With_ChangePropertyOnChildArray_Should_ThrowConcurrencyException()
    {
      var instance = new ClassWithObjectArrayConcurrencyIssues();

      var finder = new ConcurrencyChecker(instance, null, 2000, 2);

      var error = Assert.Throws<ConcurrencyException>(() => finder.Assert(2000, () => { instance.Names[0] = new {Name = "Jane"}; },
        () => { instance.Names[0] = "Dave"; }));

      Console.WriteLine(error.Message);
    }

    [Test]
    public void Run_Given_ClassWithLevelThreeDepth_With_DepthAs3AndConcurrencyIssues_Should_ThrowConcurrencyException()
    {
      var instance = new ClassWithLevelThreeDepth();

      var finder = new ConcurrencyChecker(instance, null, 2000, 3);

      var error = Assert.Throws<ConcurrencyException>(() => finder.Assert(1500, () =>
        {
          instance.ChildPropDepthOne.ChildProp.StringProp ="two";
        },
        () => { instance.ChildPropDepthOne.ChildProp.StringProp = "three"; }));

      Console.WriteLine(error.Message);
    }

    [Test]
    public void Run_Given_ClassWithLevelThreeDepth_With_DepthAs2ConcurrencyIssues_Should_ThrowConcurrencyException()
    {
      var instance = new ClassWithLevelThreeDepth();

      var finder = new ConcurrencyChecker(instance, null, 2000, 2);

      finder.Assert(800, () =>
        {
          instance.ChildPropDepthOne.ChildProp.StringProp = "two";
        },
        () => { instance.ChildPropDepthOne.ChildProp.StringProp = "three"; });
    }

    [Test]
    public void Run_Given_ClassWithLevelThreeDepth_With_DepthAs1ConcurrencyIssues_Should_ThrowConcurrencyException()
    {
      var instance = new ClassWithLevelThreeDepth();

      var finder = new ConcurrencyChecker(instance, null, 2000, 1);

      finder.Assert(800, () =>
        {
          instance.ChildPropDepthOne.ChildProp.StringProp = "two";
        },
        () => { instance.ChildPropDepthOne.ChildProp.StringProp = "three"; });
    }

    [Test]
    public void Assert_Given_ClassWithTypeChangeConcurrencyIssue_With_ChangeType_Should_ThrowConcurrencyException()
    {
      var instance = new ClassWithTypeChangeConcurrencyIssue();

      var finder = new ConcurrencyChecker(instance, null, 2000000);

      var error = Assert.Throws<ConcurrencyException>(() => finder.Assert(10, 
        () => { instance.SetFoo(new ClassWithTypeChangeConcurrencyIssue.Foo1()); },
        () => { instance.SetFoo(new ClassWithTypeChangeConcurrencyIssue.Foo2()); }));

      Console.WriteLine(error.Message);

      Assert.IsNotNull(error);
      Assert.IsTrue(error.Message.Contains(
        "ClassWithTypeChangeConcurrencyIssue->_foo: Reference and actual number of value changes does not match."));
    }
  }
}