namespace KareBooking.V4.Core.DomainServices;

public interface IAuditLog { Task WriteAsync(string message); }
