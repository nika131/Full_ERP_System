namespace NexusERP.Application.Interfaces.Services
{
    public interface IVerificationEmailSender
    {
        Task SendVerificationCodeAsync(string email, string code);
    }
}