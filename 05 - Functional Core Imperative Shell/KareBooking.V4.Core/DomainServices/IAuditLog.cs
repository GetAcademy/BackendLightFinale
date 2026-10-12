namespace KareBooking.V4.DomainServices;

public interface IAuditLog { Task WriteAsync(string message); }
