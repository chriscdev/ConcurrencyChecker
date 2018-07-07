using ConcurrencyCheckerLibrary;
using ConcurrencyCheckerLibrary.Exceptions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ConcurrencyCheckerLibraryTests
{
  [TestFixture]
  public class ReadmeTutorialTests
  {
    [Test]
    public void Run_Given_ClassWithAsyncDeadlockIssue_Should_Deadlock()
    {
      var instance = new ClassWithAsyncDeadlockIssue();
      Assert.Throws<ConcurrencyException>(() => ConcurrencyChecker.AssertAsyncDeadlocksOnly(() => instance.Foo()));
    }

    [Test]
    public void Run_Given_ClassWithAsyncDeadlockIssueFix_Should_Deadlock()
    {
      var instance = new ClassWithAsyncDeadlockIssueFix();
      ConcurrencyChecker.AssertAsyncDeadlocksOnly(() => instance.Foo());
    }

    public class ClassWithAsyncDeadlockIssue
    {
      public void Foo()
      {
        _methodThatWillDeadlock().Wait();
      }

      private async Task _methodThatWillDeadlock()
      {
        await Task.Delay(20);
      }
    }

    public class ClassWithAsyncDeadlockIssueFix
    {
      public void Foo()
      {
        _methodThatWillDeadlock().Wait();
      }

      private async Task _methodThatWillDeadlock()
      {
        await Task.Delay(20).ConfigureAwait(false);
      }
    }
  }
}
