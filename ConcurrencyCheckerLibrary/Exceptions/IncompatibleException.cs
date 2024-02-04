namespace ConcurrencyCheckerLibrary.Exceptions
{
  /// <summary>
  /// Incompatible type exception
  /// </summary>
  public class IncompatibleException: Exception
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public IncompatibleException(string type, string path): base($"{type} is incompatible, please ignore this type by adding the path {path} to the ignore list.")
    {
      
    }
  }
}
