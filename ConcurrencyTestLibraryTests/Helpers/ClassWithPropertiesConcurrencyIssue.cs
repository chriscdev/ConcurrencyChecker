namespace ConcurrencyCheckerLibraryTests.Helpers
{
  public class ClassWithPropertiesConcurrencyIssue
  {
    public string AddressLine1 { get; set; } = "111 Church Street";
    public string AddressLine2 { get; set; } = "Pretoria";

    public void ChangeAddress(string addressLine1, string addressLine2)
    {
      AddressLine1 = addressLine1;
      AddressLine2 = addressLine2;
    }
  }
}
