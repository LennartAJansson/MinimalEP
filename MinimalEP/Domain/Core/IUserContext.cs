namespace MinimalEP.Domain.Core;

public interface IUserContext
{
  Guid? UserId { get; }
  bool IsInRole(string role);
}
