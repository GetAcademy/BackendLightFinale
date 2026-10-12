namespace KareBooking.V1.DomainServices;

public interface IEmailSender { Task SendAsync(string recipient, string message); }
