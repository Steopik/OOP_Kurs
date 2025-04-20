using AuthService.Models;

namespace AuthService.Interfaces;


public interface IPendingRegistrationStore
{
    void Save(PendingRegistration registration);
    PendingRegistration? Get(string email);
    void Remove(string email);
}
