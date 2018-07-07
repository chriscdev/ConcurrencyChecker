
# Concurrency Checker

## Introduction
The ConcurrencyChecker is an easy to use library which can be used to firstly identify concurrency problems in your code and secondly to ensure that no future developer uses your code in an unsafe way.

For some people the 'How to' section will be a good place to start, but for most a code sample is worth a thousand words. Please find such a sample with explanation in the 'Tutorial' section.

Available on NuGet: https://www.nuget.org/packages/ConcurrencyChecker

## Features
  The ConcurrencyChecker will do the following checks:
  - async/await deadlocks
  - concurency issues such as: 
    - thread safety
    - state changes
    - race conditions
    - deadlocks

## Which classes to check
  Run the ConcurrencyChecker as an additional test on any class which should handle multiple concurrent threads accessing the SAME instance. 
  
  These cases are and also not limited to:
  - Singletons
  - Shared classes when using dependency injection frameworks
  - Classes with static methods, properties or fields
  - Instances reused accross threads/requests
  - Third party libraries
  

  Use the async deadlock checking to make sure:
  - Your async library will not deadlock if called synchronously for resiliency
  - Test 3rd party libraries for deadlocks
  - Troubleshooting async deadlocks

## How to
  * Create an instance of the class you want to check for possible concurrency issues:
  
    ```
    var instance = Container.Resolve<Foo>();
    ```

    If you are using MEF, Unity, Castle Windsor or any IoC container then make sure you create the instance using the IoC container. The reason for this is that it will
    simulate how the instance will be created in your application. This will make sure that you do not accidentally create a singleton of a class which
    should have been transient!

  * Create an instance of the checker, giving it a class to check for concurrency issues: 
  
    ```
    var checker = new ConcurrencyChecker(instance);
    ```

  * Run the assert given the number of concurrent threads and multiple delegates to execute:
  
    ```
    checker.Assert(4, 
          () => { instance.Add(5); instance.Add(10); },
          () => { instance.Add(5); instance.Minus(5); })
    ```

    Lets say this was a calculator class with the value initialised to 0, I would expect the 4 threads to give me the following values if there were no concurrency issues:
    15,0,15,0

    This will mean that there were no interference and my calculator works as expected. But lets say the total property is a static field, this will result in the following values:
    15,15,30,30

    This means that there is a concurrency problem! On a basic level this is how the ConcurrencyChecker works.

    Please note that more concurrent threads are used the more likely it is to find concurrency issues. I would suggest at least 10 - 20 and higher for classes in
    a multi threaded environment which handles high volume of critical data.
  
    Also the threads gets assigned an action in sequence, hence in this example thread1 = ActionA, thread2 = ActionB, thread3 = ActionA etc.

  * If using a mock framework, add the mock namespaces to the excludeNamespaces parameter:

    Mock frameworks wraps your classes with proxy classes and sometimes this will cause the ConcurrencyChecker to break (and also you do not want to test if the mock framework has a concurrency problem).

    By checking the type of the object it creates, get the namespace and add it to the constructor:

    ```
    var checker = new ConcurrencyChecker(instance, new [] {"Castle.Proxies"}); //Please note that Castle.Proxies are already excluded, this is only an example
    ```

    This notifies the ConcurrencyChecker that it is a proxy type and needs to use the base type (which is your class) to check for concurrency issues.

    By default the NSubstitute namespace is added.

  * Optionally exclude 3rd party labrary namespaces:

    Exclude 3rd party namespaces the same way as excluding proxy namespaces to avoid possible false positives and to optimize the unit tests.

  * Optionally set depth

    Depth refers to the object tree depth that gets created and monitored, the deepter the tree the more nodes to monitor. In some cases a StackOverflow might get thrown if the object tree becomes too large.

    To solve this there is the mechanism of depth. To explain depth further please look at the anonymous type below:

    ```
    new 
    { 
      MemberDepth1 = new             //First depth level
      { 
        MemberDepth2 = new           //Second depth level
        {     
          MemberDepth3 = "End"       //Third depth level
        }
      }
    }
    ```
    
    Most of the times a default depth limit of 5 is more than enough, but increase this if necessary. You can do this via the overloaded constructor or using the property Depth.

  * Optionally set maxExecutionTimeMs

    The maxExecutionTimeMs parameter is used to determine if a deadlock has occurred (if the execution time is longer than the time specified, then either a deadlock occurred or there were some inefficient code introduced). 

    The default is 10s as to not flag a deadlock incorrectly. Increase this if needed either by passing the value to the constructor or by setting the property, but know that no successful unit test is suppose to take that long to execute.

  * Read the exception:

    The ConcurrencyException will contain a detailed report on all of the possible concurrency issues.

  * Add members to ignore:

    Sometimes members will get flagged as problematic which in actual fact are not, especially counters. In cases such as these add the specific members to the ignore list to remove the false positives.

    ```
    var checker new ConcurrencyChecker(instance, true, "Foo->Bar", "Foo->Bar->Member" );
    ```

    Use the exact report member description i.e. Foo->Bar and add it to ignore list when calling the ConcurrencyChecker constructor. 

## Tutorials

### Checking for concurrency and async deadlock issues
  This tutorial is a basic code example to explain the usage of the framework for checking for concurrency and async deadlock issues.

  1. Create a class which will have a potential concurrency problem when running in a multi threaded environment:

	  ```
	  public class ClassWithFieldConcurrencyIssue
	  {
		  private string _name = "Foo";

		  public void ChangeNameTo(string name)
		  {
		    _name = name;
		  }
	  }
	  ```
  2. You can use the unit testing framework of your choice. In this example NUNIT is used:

	  ```
	  [TestFixture]
	  public class ConcurrencyCheckerTests 
	  {
		  [Test]
		  public void Run_Given_ClassWithFieldConcurrencyIssue_Should_ReturnReportWithIssue()
		  {
			  //You will add the code of the next step in here
		  }
	  }
	  ```

  3. Now use the ConcurrencyChecker to see if the class has any concurrency issues when running in a multi threaded environment (spoiler, it will!):
  
	  ```
	  var instance = new ClassWithFieldConcurrencyIssue();
	
	  var checker = new ConcurrencyChecker(instance);
	
	  checker.Assert(10, 
		  () => instance.ChangeNameTo("Jane"), 
		  () => instance.ChangeNameTo("Peter"));
	  ```
	
	  What did we do here? We kicked off 10 concurrent threads of which 5 will be running the action instance.ChangeNameTo("Jane") and the other 5 will be running the action instance.ChangeNameTo("Peter"). 

### Checking only for async deadlock issues
  This tutorial is a basic code example to explain the usage of the framework for checking async deadlock issues only. This is handy to check only async code for any possible deadlocks.

  It is especially useful when you are using asynchronous libraries in a legacy application that hasn't fully implemented async/await all the way up.

  1. Create a class which will have a potential async deadlock problem when running in a single threaded synchronization context:

	  ```
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
	  ```
  2. You can use the unit testing framework of your choice. In this example NUNIT is used:

	  ```
	  [TestFixture]
	  public class AsyncDeadlockOnlyTests 
	  {
		  [Test]
      public void Run_Given_ClassWithAsyncDeadlockIssue_Should_Deadlock()
      {
        var instance = new ClassWithAsyncDeadlockIssue();
        Assert.Throws<ConcurrencyException>(() => ConcurrencyChecker.AssertAsyncDeadlocksOnly(() => instance.Foo()));
      }
	  }
	  ```
	
	  What did we do here? Under the hood a single threaded synchronization context is created which the code gets executed on. This simulates the synchronization contexts of ASP.NET (not ASP.NET CORE), WPF and Windows Forms.

    The explanation of why the deadlock happens is beyond the scope of this readme, but please go read more on this excellent blog post by Stephen Cleary: https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html

  3. Fix the code by using adding .ConfigureAwait(false) to the Task.Delay(20)

    ```
	  public class ClassWithAsyncDeadlockIssue
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
	  ```
    
    What is this magic? All we did here was configuring the task not to use the current synchonization context when resuming execution.


## Troubleshooting

  * The test freezes and when I debug it throws a stack overflow exception: 
  
    Decrease the depth parameter.

## About the ConcurrencyChecker

This library was created after our development team experienced a concurrency bug which was live for several months before it was picked up. 

After writing all of the unit tests to simulate the problem, I had the idea to change the tests into a generic library that can be used by other projects and teams.

Still the best way to avoid and find problems is by implementing good programming practices and code reviews!

## License

This project is licensed under the MIT License - see the [License.md](License.md) file for details

## Contact  

Please feel free to drop me a mail at chrisc.development@gmail.com 