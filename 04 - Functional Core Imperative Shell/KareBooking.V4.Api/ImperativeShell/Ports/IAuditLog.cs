namespace KareBooking.V4.Api.ImperativeShell.Ports;

public interface IAuditLog { Task WriteAsync(string message); }
