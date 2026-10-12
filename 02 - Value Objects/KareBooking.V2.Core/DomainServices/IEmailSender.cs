namespace KareBooking.V2.DomainServices;

public interface IEmailSender { Task SendAsync(string recipient, string message); }
