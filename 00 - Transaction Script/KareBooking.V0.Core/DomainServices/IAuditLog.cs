namespace KareBooking.V0.DomainServices;

public interface IAuditLog { Task WriteAsync(string message); }
