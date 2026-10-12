namespace KareBooking.V0.DomainServices;

public interface IEmailSender { Task SendAsync(string recipient, string message); }
