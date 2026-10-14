namespace KareBooking.V2.Core.DomainServices;

public interface IEmailSender { Task SendAsync(string recipient, string message); }
