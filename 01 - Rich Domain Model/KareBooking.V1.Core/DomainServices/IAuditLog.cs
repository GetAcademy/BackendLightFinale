namespace KareBooking.V1.DomainServices;

public interface IAuditLog { Task WriteAsync(string message); }
