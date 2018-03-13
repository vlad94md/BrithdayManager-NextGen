using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BirthdayManager.Core.Constants
{
    public static class WebValidationErrorMessages
    {
        public const string AmountIsZero = "Amount cannot be zero.";
        public const string InvalidTransactionType = "Invalid transaction type.";
        public const string UserNotFound = "Selected user not found.";
    }
}