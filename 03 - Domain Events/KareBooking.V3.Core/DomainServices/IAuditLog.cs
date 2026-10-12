namespace KareBooking.V3.DomainServices;

public interface IAuditLog { Task WriteAsync(string message); }
