using System;
using System.Linq;

namespace ConcurrencyCheckerLibrary.Helpers
{
  /// <summary>
  /// Namespace helper
  /// </summary>
  public static class NamespaceHelper
  {
    /// <summary>
    /// Get the root namespace of type eg. MyLibrary.MyFolder => MyLibrary
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns>Root namespace </returns>
    public static string GetRootNamespace(Type type)
    {
      return type.Namespace?.Split('.').First();
    }

    /// <summary>
    /// Chexk if type contains any of the namespaces
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="namespaces">Namespaces to check</param>
    /// <returns>If namespace found</returns>
    public static bool ContainsNamespace(Type type, params string[] namespaces)
    {
      var typeNamespace = type.Namespace;

      return typeNamespace != null && namespaces.Any(ns => typeNamespace.Contains(ns));
    }
  }
}
