namespace KareBooking.V1.Core.DomainServices;

public interface IAuditLog { Task WriteAsync(string message); }
