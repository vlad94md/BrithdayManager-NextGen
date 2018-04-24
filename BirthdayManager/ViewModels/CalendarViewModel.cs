using System.Collections.Generic;

namespace BirthdayManager.ViewModels
{
    public class CalendarViewModel
    {
        public List<CalendarArrangementViewModel> UpcommingBirthdays { get; set; }
        public List<CalendarArrangementViewModel> RecentPastBirthdays { get; set; }
    }
}