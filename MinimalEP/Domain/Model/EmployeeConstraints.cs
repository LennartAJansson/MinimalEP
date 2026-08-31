namespace MinimalEP.Domain.Model;

public static class EmployeeConstraints
{
  public const int EmailMaxLength = 256;
  public const int NameMaxLength = 100;
  public const int PositionMaxLength = 100;
  public const int PhoneNumberMaxLength = 30;
  public const int StreetMaxLength = 200;
  public const int PostalCodeMaxLength = 20;
  public const int CityMaxLength = 100;
  public const int MinimumAge = 16;
  public const int MaximumAge = 100;
}
