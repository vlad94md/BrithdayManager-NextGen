namespace BirthdayManager.Service
{
    public interface IEmailSender
    {
        bool SendMail(string toList, string subject, string body);
    }
}
