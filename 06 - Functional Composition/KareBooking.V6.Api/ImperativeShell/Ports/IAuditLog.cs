namespace KareBooking.V6.Api.ImperativeShell.Ports;

public interface IAuditLog { Task WriteAsync(string message); }
