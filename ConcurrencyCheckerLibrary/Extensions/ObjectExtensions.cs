using Newtonsoft.Json;
using System.Reflection;

namespace ConcurrencyCheckerLibrary.Extensions
{
  /// <summary>
  /// Object Extensions
  /// </summary>
  internal static class ObjectExtensions
  {
    public static IDictionary<MemberInfo, object> GetMembersAndValues(this object obj, bool deep = true)
    {
      var memberPairs = new Dictionary<MemberInfo, object>();

      foreach (var field in obj.GetType().GetRuntimeFields().Where(f => !f.Name.EndsWith("k__BackingField")))
      {
        var value = field.GetValue(obj);
        if (deep && (value?.IsComplexType() ?? false))
        {
          foreach (var memberValuePair in value.GetMembersAndValues())
            memberPairs.Add(memberValuePair.Key, memberValuePair.Value);
        }
        else
          memberPairs.Add(field, value);
      }

      foreach (var prop in obj.GetType().GetRuntimeProperties())
      {
        var value = prop.GetValue(obj, null);
        if (deep && (value?.IsComplexType() ?? false))
        { 
          foreach (var memberValuePair in value.GetMembersAndValues())
            memberPairs.Add(memberValuePair.Key, memberValuePair.Value);
        }
        else
          memberPairs.Add(prop, value);
      }

      return memberPairs;
    }

    public static string Serialize(this object obj, Formatting formatting = Formatting.Indented)
    {
      if (obj.IsPrimitiveType())
      {
        return obj.ToString();
      }

      return JsonConvert.SerializeObject(obj, formatting);
    }

    public static bool IsPrimitiveType(this object obj)
    {
      var type = obj.GetType();
      return type.IsPrimitive || obj is string || obj is DateTime;
    }

    public static bool IsComplexType(this object obj)
    {
      return !(obj.GetType().IsPrimitive || obj is string || obj is DateTime);
    }

    public static bool IsEnumerableType(this object obj)
    {
      return !IsPrimitiveType(obj) && obj.GetType().GetInterface("IEnumerable") != null;
    }

    public static bool IsDictionaryType(this object obj)
    {
      return obj.GetType().GetInterface("IDictionary") != null;
    }

    public static Type GetRealType(this object obj, params string[] proxyNameSpaces)
    {
      var objType = obj.GetType();

      return proxyNameSpaces.Any(p => p == objType.Namespace) ? objType.BaseType : objType;
    }

    //public static bool IsSystemType(this object obj)
    //{
    //  return obj.GetType().IsPrimitive || obj is string || obj is DateTime;
    //}
  }
}
