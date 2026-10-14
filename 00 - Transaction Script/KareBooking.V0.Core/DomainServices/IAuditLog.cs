namespace KareBooking.V0.Core.DomainServices;

public interface IAuditLog { Task WriteAsync(string message); }
