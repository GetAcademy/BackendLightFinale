namespace KareBooking.V4.Api.ImperativeShell.Ports;

public interface IEmailSender { Task SendAsync(string recipient, string message); }
