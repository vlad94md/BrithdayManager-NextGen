using System.Collections.Generic;
using System.Web;

namespace BirthdayManager.ViewModels
{
    public class ImportUsersViewModel
    {
        public HttpPostedFileBase File { get; set; }

        public List<string> Messages { get; set; } = new List<string>();
    }
}