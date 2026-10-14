namespace KareBooking.V1.Core.DomainServices;

public interface IEmailSender { Task SendAsync(string recipient, string message); }
