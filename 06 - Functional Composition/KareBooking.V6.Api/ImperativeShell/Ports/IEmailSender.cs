namespace KareBooking.V6.Api.ImperativeShell.Ports;

public interface IEmailSender { Task SendAsync(string recipient, string message); }
