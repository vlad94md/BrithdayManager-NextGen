using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using BirthdayManager.Core.Models;
using BirthdayManager.Core.ViewModels;
using BirthdayManager.Persistence;
using ClosedXML.Excel;

namespace BirthdayManager.Controllers.Api
{
    public class ImportController : ApiController
    {
        private ApplicationDbContext _context;

        public ImportController()
        {
            _context = new ApplicationDbContext();
        }

        [HttpPost]
        [Route("api/import/balance")]
        public IHttpActionResult ImportUsersBalance()
        {

            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Excel file should be selected.");
            }

            HttpPostedFile postedFile = HttpContext.Current.Request.Files["file"];

            if (postedFile == null || postedFile.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                return BadRequest("Excel file should be selected.");
            }


            var userBalanceList = new Dictionary<ApplicationUser, decimal>();

            using (var workbook = new XLWorkbook(postedFile.InputStream))
            {
                IXLWorksheet worksheet = GetWorksheetByNameOrDefault(workbook);

                foreach (IXLRow currentRow in worksheet.RowsUsed())
                {
                    string username = (string)currentRow.Cell(1).Value;

                    if (!Decimal.TryParse(currentRow.Cell(2).Value.ToString(), out var balance))
                    {
                        return BadRequest($"For {username} balance is in incorect format. The import process is stoped and rollback");
                    }

                    if (string.IsNullOrEmpty(username) || balance == 0)
                    {
                        continue;
                    }

                    var user = _context.Users.FirstOrDefault(x => x.UserName == username);
                    if (user == null)
                    {
                        return BadRequest($"{username} was not found in the system. The import process is stoped and rollback");
                    }

                    userBalanceList.Add(user, balance);
                }
            }

            foreach (var item in userBalanceList)
            {
                item.Key.Balance = item.Value;
            }

            _context.SaveChanges();
            return Ok("Import finished successfull.");
        }

        private static IXLWorksheet GetWorksheetByNameOrDefault(XLWorkbook workbook, string sheetName = null)
        {
            IXLWorksheet worksheet;

            IEnumerable<IXLWorksheet> xlWorksheets =
                workbook.Worksheets.Where(w =>
                {
                    var lowerSheetName = string.IsNullOrEmpty(sheetName) ? sheetName : sheetName.ToLower();
                    return string.CompareOrdinal(w.Name.ToLower(), lowerSheetName) == 0;
                });

            if (!string.IsNullOrEmpty(sheetName) && xlWorksheets.Any())
            {
                worksheet = xlWorksheets.First();
            }
            else
            {
                worksheet = workbook.Worksheets.First();
            }

            return worksheet;
        }
    }
}
