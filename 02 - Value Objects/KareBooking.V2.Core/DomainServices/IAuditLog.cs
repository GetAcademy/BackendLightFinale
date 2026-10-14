namespace KareBooking.V2.Core.DomainServices;

public interface IAuditLog { Task WriteAsync(string message); }
