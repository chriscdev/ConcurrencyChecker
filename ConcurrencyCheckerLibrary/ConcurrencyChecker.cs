using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using ConcurrencyCheckerLibrary.Extensions;
using ConcurrencyCheckerLibrary.Exceptions;
using ConcurrencyCheckerLibrary.Helpers;
using ConcurrencyCheckerLibrary.Models;

namespace ConcurrencyCheckerLibrary
{
  /// <summary>
  /// Concurrency Checker Class.
  /// Test your code for concurrency issues.
  /// </summary>
  public class ConcurrencyChecker : IDisposable
  {
    private MemberTracker<object>[] _members;
    private Task _monitor;
    private CancellationTokenSource _cts;
    private readonly object _instance;
    private readonly string[] _ignoreMemberPaths;
    private readonly string[] _proxyNamespaces = { "Castle.Proxies" };

    /// <summary>
    /// Maximum execution time of the code. Should end before the elapse of the time in milliseconds, this is used to identify possible deadlocks. Defaults to 10s
    /// </summary>
    public int MaxExecutionTimeMs { get; set; }

    /// <summary>
    /// Shows full report, set to false to get summary report. True by default.
    /// </summary>
    public bool FullReport { get; set; } = true;

    /// <summary>
    /// Object tree depth to monitor, some objects can have a massive object tree which can result in StackOverflowExceptions. 
    /// 1 = Only members of the instance, 2 = Members of any direct child objects etc. Defaults to 5.
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// Namespaces that should be excluded. Use this to ignore 3rd party namespaces.
    /// You only have to add the root namespace eg. add MyLibrary instead of MyLibrary.MyFolder.Foo.Bar, this will make sure that all sub namespaces are included.
    /// By default Moq, Castle.Proxies and NSubstitute are added.
    /// </summary>
    public List<string> ExcludedNameSpaces { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="instance">Instance to check for concurrency problems.</param>
    /// <param name="maxExecutionTimeMs">Maximum execution time of the code. Should end before the elapse of the time in milliseconds, this is used to identify possible deadlocks. Defaults to 10s</param>
    /// <param name="depth">Depth to monitor. 1 = Only members of the instance, 2 = Members of any direct child objects etc. Defaults to 3.</param>
    /// <param name="excludeNamespaces">Add namespaces which should be excluded, this is handy to exclude third party namespaces. You only need to add the root namespace.</param>
    public ConcurrencyChecker(object instance, string[] excludeNamespaces = null, int maxExecutionTimeMs = 10000, int depth = 5)
    {
      _instance = instance ?? throw new ArgumentNullException(nameof(instance));
      MaxExecutionTimeMs = maxExecutionTimeMs;
      Depth = depth;
      ExcludedNameSpaces = new List<string>(excludeNamespaces ?? new string[0]) { "Moq", "Castle.Proxies", "NSubstitute" };
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="instance">Instance to check for concurrency problems.</param>
    /// <param name="ignoreMemberPaths">Members paths to ignore.</param>
    public ConcurrencyChecker(object instance, params string[] ignoreMemberPaths)
      : this(instance)
    {
      _ignoreMemberPaths = ignoreMemberPaths;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="instance">Instance to check for concurrency problems.</param>
    /// <param name="maxExecutionTimeMs">Should end before the elapse of the time in milliseconds, this is used to identify possible deadlocks. Defaults to 10s</param>
    /// <param name="depth">Depth to monitor. 1 = Only members of the instance, 2 = Members of any direct child objects etc. Defaults to 5.</param>
    /// <param name="excludeNamespaces">Add namespaces which should be excluded, this is handy to exclude third party namespaces. You only need to add the root namespace.</param>
    /// <param name="ignoreMemberPaths">Members paths to ignore.</param>
    public ConcurrencyChecker(object instance, string[] excludeNamespaces = null, int maxExecutionTimeMs = 10000, int depth = 5, params string[] ignoreMemberPaths)
      : this(instance, excludeNamespaces, maxExecutionTimeMs, depth)
    {
      _ignoreMemberPaths = ignoreMemberPaths;
    }

    /// <summary>
    /// Assert to find possible deadlocks in code that uses async/await
    /// Throws an exception of type <see cref="ConcurrencyException"/> if any errors are found.
    /// </summary>
    /// <exception cref="ConcurrencyException"></exception>
    /// <param name="action">Delegate to execute</param>
    /// <param name="shouldEndBeforeMs">Maximum time in which the code should execute, this is used to determine deadlocks.</param>
    public static void AssertAsyncDeadlocksOnly(Action action, int shouldEndBeforeMs = 2000)
    {
      try
      {
        var cts = new CancellationTokenSource(shouldEndBeforeMs);
        ConcurrentRunner.Start(new[] { action }, 1, cts.Token);
      }
      catch (OperationCanceledException)
      {
        throw new ConcurrencyException("Possible deadlock detected. Make sure that you do not use .Wait(), .WaitAny(), .WaitAll(), .GetAwaiter().GetResult() or .Result on async methods.");
      }
    }

    /// <summary>
    /// Use assert in unit tests to run the concurrency checker, this will check for async/await deadlocks as well
    /// Throws an exception of type <see cref="ConcurrencyException"/> if any errors are found.
    /// </summary>
    /// <exception cref="ConcurrencyException"></exception>
    /// <param name="numberOfConcurrentThreads">Number of concurrent threads to run. The higher the number the more likely a problem will be found.</param>
    /// <param name="actions">Actions to execute concurrently</param>
    /// <example>ConcurrencyChecker.AssertRun(5, new Action(() => instance.ChangeNameTo("Jane")), new Action(() => instance.ChangeNameTo("Peter")))</example>
    public void Assert(int numberOfConcurrentThreads, params Action[] actions)
    {
      var report = Run(numberOfConcurrentThreads, actions);

      if (report != null)
        throw new ConcurrencyException(report);
    }

    /// <summary>
    /// Will run the concurrency checker and will output a report if there were any errors.
    /// It will throw an exception of type <see cref="ConcurrencyException"/> if any errors are found.
    /// </summary>
    /// <param name="numberOfConcurrentThreads">Number of concurrent threads to run. The higher the number the more likely a problem will be found.</param>
    /// <param name="actions">Actions to execute concurrently</param>
    /// <example>ConcurrencyChecker.AssertRun(5, new Action(() => instance.ChangeNameTo("Jane")), new Action(() => instance.ChangeNameTo("Peter")))</example>
    /// <returns>Report if failed or null if successful</returns>
    public string Run(int numberOfConcurrentThreads, params Action[] actions)
    {
      if (numberOfConcurrentThreads < actions.Length)
        throw new ArgumentOutOfRangeException(nameof(numberOfConcurrentThreads), "Number of concurrent threads are less than the number of actions to execute.");

      _cts = new CancellationTokenSource(5000);

      try
      {
        InitializeMembersToMonitor();
      }
      catch (OperationCanceledException)
      {
        return "Initialization failed. This normally happens is you are using a mock framework such as Moq. Please exclude the mock namespace (NSubstitute and Moq is supported out of the box).";
      }

      _cts = new CancellationTokenSource();

      Start(true);

      actions.First()();

      Stop();

      _cts = new CancellationTokenSource(MaxExecutionTimeMs);

      try
      {
        Start();

        ConcurrentRunner.Start(actions, numberOfConcurrentThreads, _cts.Token);

        Stop();

        return GetReport();
      }
      catch (OperationCanceledException)
      {
        return "Possible deadlock detected. Make sure that you do not use .Wait() or .Result on async methods.";
      }
    }

    /// <summary>
    /// Get report after the ConcurrencyChecker.Run finished
    /// </summary>
    /// <returns>Report if failed or null if successful</returns>
    public string GetReport()
    {
      var report = new StringBuilder();
      if (_members != null)
      {
        var objType = _instance.GetRealType(_proxyNamespaces.ToArray());

        foreach (var member in _members)
        {
          report.Append(CompareValuesAndOutputMemberReport(member, objType.Name));
        }
      }

      if (report.Length == 0)
        return null;

      return $"Possible concurrency issues have been found. Please review each member listed below: \n\n{report}";
    }

    /// <summary>
    /// Get report after the ConcurrencyChecker.Run finished
    /// </summary>
    /// <returns>Report if failed or null if successful</returns>
    public string? GetDeadlockReport()
    {
      var report = new StringBuilder();
      if (_members != null)
      {
        var objType = _instance.GetRealType(_proxyNamespaces.ToArray());

        foreach (var member in _members)
        {
          report.Append(CompareValuesAndOutputMemberReport(member, objType.Name));
        }
      }

      if (report.Length == 0)
        return null;

      return $"Possible concurrency issues have been found. Please review each member listed below: \n\n{report}";
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
      Stop();
    }

    private void Stop()
    {
      Thread.Sleep(10);
      _cts.Cancel();
      _monitor.WaitWhen(t => t.Status == TaskStatus.Running);
    }

    private void Start(bool useReferenceValue = false)
    {
      _monitor = Task.Run(() =>
      {
        while (!_cts.Token.IsCancellationRequested)
        {
          MonitorMembers(_members, useReferenceValue);
        }
      });

      _monitor.WaitWhen(t => t.Status != TaskStatus.Running && t.Status != TaskStatus.RanToCompletion);
    }

    private string CompareValuesAndOutputMemberReport(MemberTracker<object> member, string instancePath)
    {
      var memberPath = GetMemberPath(instancePath, member);

      if (_ignoreMemberPaths != null && _ignoreMemberPaths.Any(ignorePath => ignorePath.Equals(memberPath, StringComparison.InvariantCultureIgnoreCase)))
        return string.Empty;

      var sb = new StringBuilder();

      if (member.Exception != null)
      {
        sb.Append($"{memberPath}: Possible concurrency issue because of the following exception: {member.Exception.Message}\n");
      }

      if (member.ReferenceValueChanges != member.ValueChanges)
        sb.Append($"{memberPath}: Reference and actual number of value changes does not match.");

      var childReports = new StringBuilder();

      if (member.Children != null)
      {
        foreach (var child in member.Children)
          childReports.Append(CompareValuesAndOutputMemberReport(child, $"{instancePath}->{member.Name}"));
      }

      for (var i = 0; i < member.ReferenceValues.Count; i++)
      {
        if (!member.ReferenceValues[i]?.Equals(member.Values[i]) ?? member.ReferenceValues[i] != member.Values[i])
          sb.Append($"\n{memberPath}: Reference and actual values does not match at index {i}. Reference = {member.ReferenceValues[i]}, Actual = {member.Values[i]}");
      }

      if (sb.Length == 0)
        return childReports.ToString();

      if (!FullReport)
        return sb.Append($"{childReports}\n").ToString();

      sb.Append($"\n{memberPath}: Reference Value Changes: ");

      for (var i = 0; i < member.ReferenceValues.Count; i++)
      {
        sb.Append($"\n{i}: {member.ReferenceValues[i]?.GetRealType(_proxyNamespaces)?.Name} = {member.ReferenceValues[i]?.Serialize() ?? "null"}");
      }

      sb.Append($"\n{memberPath}: Actual Value Changes: ");

      for (var i = 0; i < member.Values.Count; i++)
      {
        sb.Append($"\n{i}: {member.Values[i]?.GetRealType(_proxyNamespaces)?.Name} = {member.Values[i]?.Serialize() ?? "null"}");
      }

      sb.Append(childReports);

      sb.Append($"\n\n");
      return sb.ToString();
    }

    private static string GetMemberPath(string instancePath, MemberTracker<object> member)
    {
      return member.Index >= 0
        ? $"{instancePath}[{member.Index}]->{member.Name}"
        : $"{instancePath}->{member.Name}";
    }

    private void InitializeMembersToMonitor()
    {
      _members = GetMembersAndValues(_instance).ToArray();
    }

    private static void MonitorMembers(IEnumerable<MemberTracker<object>> members, bool useReferenceValue = false)
    {
      foreach (var member in members)
      {
        var newValue = member.GetValueFromInstance();

        if (!member.EqualToCurrentValue(newValue, useReferenceValue))
        {
          member.AddValue(newValue, useReferenceValue);
        }

        if (member.Children != null && member.Children.Any())
          MonitorMembers(member.Children, useReferenceValue);
      }
    }

    private IEnumerable<MemberTracker<object>> GetMembersAndValues(object obj, int index = -1, int depth = 0)
    {
      var memberTrackers = new List<MemberTracker<object>>();

      depth++;

      if (depth > Depth)
        return memberTrackers;

      _cts.Token.ThrowIfCancellationRequested();

      var objType = obj.GetRealType(_proxyNamespaces.ToArray());

      if (obj.IsPrimitiveType())
        return new[] { CreateMemberTracker(obj, objType, obj, index, depth) };

      foreach (var field in objType.GetRuntimeFields().Where(f => !f.Name.EndsWith("__BackingField") && !f.Name.EndsWith("__Field") && !NamespaceHelper.ContainsNamespace(f.DeclaringType, ExcludedNameSpaces.ToArray())))
      {
        memberTrackers.Add(CreateMemberTracker(obj, field, field.GetValue(obj), index, depth));
      }

      foreach (var prop in objType.GetRuntimeProperties().Where(f => !NamespaceHelper.ContainsNamespace(f.DeclaringType, ExcludedNameSpaces.ToArray())))
      {
        memberTrackers.Add(CreateMemberTracker(obj, prop, prop.GetValue(obj, null), index, depth));
      }

      return memberTrackers;
    }

    private MemberTracker<object> CreateMemberTracker(object instance, MemberInfo memberInfo, object value, int index, int depth)
    {
      var memberTracker = new MemberTracker<object>(instance, memberInfo, value, index);

      if (value == null || value.IsPrimitiveType())
        return memberTracker;

      var enumerableIndex = 0;

      if (value.IsDictionaryType())
      {
        memberTracker.Children = ((IDictionary)value).Values.Cast<object>().SelectMany(keyVal => GetMembersAndValues(keyVal, enumerableIndex++, depth)).ToList();
        return memberTracker;
      }

      if (value.IsEnumerableType())
      {
        memberTracker.Children = ((IEnumerable<object>)value).SelectMany(val => GetMembersAndValues(val, enumerableIndex++, depth)).ToList();
        return memberTracker;
      }

      if (value.IsComplexType() && (!value.GetType().Namespace?.StartsWith("System") ?? false))
      {
        memberTracker.Children = GetMembersAndValues(value, -1, depth).ToList();
        return memberTracker;
      }

      return memberTracker;
    }

    public static void AssertAsyncDeadlocksOnly(object v)
    {
      throw new NotImplementedException();
    }
  }
}
