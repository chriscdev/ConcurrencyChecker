using System;
using ConcurrencyCheckerLibrary;
using ConcurrencyCheckerLibrary.Exceptions;
using ConcurrencyCheckerLibraryTests.Helpers;
using NUnit.Framework;

namespace ConcurrencyCheckerLibraryTests
{
  [TestFixture]
  public class ConcurrencyCheckerAsyncTests
  {
    [Test]
    public void Run_Given_ClassWithNoConcurrencyIssue_When_Calling_AsyncMethod_Should_Succeed()
    {
      var instance = new ClassWithNoConcurrencyIssues();

      ConcurrencyChecker.AssertAsyncDeadlocksOnly(async () => await instance.DoSomeWorkAndReturnResponseAsync(), 200);
    }

    [Test]
    public void AssertRun_Given_ClassWithAsyncDeadlock_Should_ThrowException()
    {
      var instance = new ClassWithAsyncDeadlock();

      var error = Assert.Throws<ConcurrencyException>(() => ConcurrencyChecker.AssertAsyncDeadlocksOnly(() => instance.ShouldDeadlock(), 200));

      Assert.AreEqual("Possible deadlock detected. Make sure that you do not use .Wait(), .WaitAny(), .WaitAll(), .GetAwaiter().GetResult() or .Result on async methods.", error.Message);
    }

    [Test]
    public void AssertRun_Given_ClassWithNoAsyncDeadlock_Should_Succeed()
    {
      var instance = new ClassWithAsyncDeadlock();

      ConcurrencyChecker.AssertAsyncDeadlocksOnly(() => instance.ShouldNotDeadLock(), 200);
    }

    [Test]
    public void AssertRun_Given_ClassWithDictionaryAsyncDeadlock_Should_ThrowException()
    {
      var instance = new ClassWithAsyncDeadlock();

      Assert.Throws<AggregateException>(() => ConcurrencyChecker.AssertAsyncDeadlocksOnly(async () => await instance.DictionaryNotThreadSafeShouldLock(), 500));
    }
  }
}
