namespace KareBooking.V5.Core.DomainServices;

public interface IEmailSender { Task SendAsync(string recipient, string message); }
