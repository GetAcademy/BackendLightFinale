namespace KareBooking.V2.DomainServices;

public interface IAuditLog { Task WriteAsync(string message); }
