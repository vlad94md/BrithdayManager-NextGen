using System;
using System.Collections.Generic;
using System.Text;

namespace BirthdayManager.Services
{
    public interface IEmailSender
    {
        bool SendMail(string toList, string subject, string body);
    }
}
