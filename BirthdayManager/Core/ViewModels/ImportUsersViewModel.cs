using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BirthdayManager.Core.ViewModels
{
    public class ImportUsersViewModel
    {
        public HttpPostedFileBase File { get; set; }

        public List<string> Messages { get; set; } = new List<string>();
    }
}