using System.Collections.Generic;

namespace BirthdayManager.Core.ViewModels
{
    public class CalendarViewModel
    {
        public List<CallendarArrangementViewModel> UpcommingBirthdays { get; set; }
        public List<CallendarArrangementViewModel> RecentPastBirthdays { get; set; }
    }
}