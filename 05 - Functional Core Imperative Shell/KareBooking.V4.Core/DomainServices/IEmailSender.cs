namespace KareBooking.V4.DomainServices;

public interface IEmailSender { Task SendAsync(string recipient, string message); }
