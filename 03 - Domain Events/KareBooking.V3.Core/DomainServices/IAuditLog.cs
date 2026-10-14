namespace KareBooking.V3.Core.DomainServices;

public interface IAuditLog { Task WriteAsync(string message); }
