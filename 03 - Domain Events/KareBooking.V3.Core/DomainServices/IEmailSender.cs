namespace KareBooking.V3.DomainServices;

public interface IEmailSender { Task SendAsync(string recipient, string message); }
