namespace KareBooking.V5.DomainServices;

public interface IAuditLog { Task WriteAsync(string message); }
