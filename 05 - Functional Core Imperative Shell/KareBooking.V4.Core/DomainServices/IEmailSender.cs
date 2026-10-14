namespace KareBooking.V4.Core.DomainServices;

public interface IEmailSender { Task SendAsync(string recipient, string message); }
