namespace ConcurrencyCheckerLibrary.Exceptions
{
  /// <summary>
  /// Exception thrown by the ConcurrencyChecker
  /// </summary>
  public class ConcurrencyException : Exception
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message">Error Message</param>
    public ConcurrencyException(string message) : base(message)
    {
    }
  }
}
