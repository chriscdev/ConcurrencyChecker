using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConcurrencyCheckerLibrary.Extensions;

namespace ConcurrencyCheckerLibrary.Models
{
  internal class MemberTracker<TM>
  {
    public MemberTracker(object instance, MemberInfo memberInfo, int index)
    {
      Instance = instance;
      MemberInfo = memberInfo;
      ReferenceValues = new List<TM>();
      Values = new List<TM>();
      Index = index;
    }

    public MemberTracker(object instance, MemberInfo memberInfo, TM initialValue, int index)
    {
      Instance = instance;
      MemberInfo = memberInfo;
      ReferenceValues = new List<TM> { initialValue };
      Values = new List<TM> { initialValue };
      Index = index;
    }

    public string Name => MemberInfo.Name;

    public MemberInfo MemberInfo { get; set; }
    public List<TM> ReferenceValues { get; set; }
    public List<TM> Values { get; set; }
    public List<MemberTracker<TM>> Children { get; set; }
    public int ReferenceValueChanges => ReferenceValues.Count > 0 ? ReferenceValues.Count - 1 : 0;
    public int ValueChanges => Values.Count > 0 ? Values.Count - 1 : 0;
    public object Instance { get; set; }
    public int Index { get; set; }
    public Exception Exception { get; private set; }

    public TM GetValueFromInstance()
    {
      var fieldInfo = MemberInfo as FieldInfo;
      if (fieldInfo != null)
        return (TM)fieldInfo.GetValue(Instance);

      var propInfo = MemberInfo as PropertyInfo;
      if (propInfo != null)
        return (TM) propInfo.GetValue(Instance, null);

      return (TM) Instance;
    }

    public void AddValue(TM value, bool useReference = false)
    {
      if (useReference)
        ReferenceValues.Add(value);
      else
        Values.Add(value);
    }

    public TM GetCurrentValue(bool useReference = false)
    {
      return useReference ? ReferenceValues.Last() : Values.Last();
    }

    public int GetNumberOfChanges(bool useReference = false)
    {
      return useReference
        ? ReferenceValueChanges
        : ValueChanges;
    }

    public bool EqualToCurrentValue(TM value, bool useReference = false)
    {
      try
      {
      var currentValue = GetCurrentValue(useReference);

      if (value == null)
        return currentValue == null;

      if (value.IsDictionaryType())
        return ((IDictionary)value).Values.Cast<object>().SequenceEqual(((IDictionary)currentValue).Values.Cast<object>());

      if (value.IsEnumerableType() && currentValue != null)
        return ((IEnumerable<object>) value).SequenceEqual((IEnumerable<object>)currentValue);

        return value.Equals(currentValue);
      }
      catch (Exception ex)
      {
        Exception = ex;
        return true;
      }
    }
  }
}
