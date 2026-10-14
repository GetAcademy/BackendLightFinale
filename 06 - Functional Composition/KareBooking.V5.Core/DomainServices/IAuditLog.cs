namespace KareBooking.V5.Core.DomainServices;

public interface IAuditLog { Task WriteAsync(string message); }
