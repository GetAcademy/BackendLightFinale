namespace KareBooking.V5.DomainServices;

public interface IEmailSender { Task SendAsync(string recipient, string message); }
