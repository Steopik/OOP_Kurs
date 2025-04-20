using AuthService.Interfaces;
using AuthService.Models;

public class InMemoryPendingRegistrationStore : IPendingRegistrationStore
{
    private readonly Dictionary<string, PendingRegistration> _registrations = new();

    public void Save(PendingRegistration registration)
    {
        _registrations[registration.Email] = registration;
    }

    public PendingRegistration? Get(string email)
    {
        return _registrations.TryGetValue(email, out var reg) ? reg : null;
    }

    public void Remove(string email)
    {
        _registrations.Remove(email);
    }
}
