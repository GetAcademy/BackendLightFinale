namespace KareBooking.V3.Core.DomainServices;

public interface IEmailSender { Task SendAsync(string recipient, string message); }
