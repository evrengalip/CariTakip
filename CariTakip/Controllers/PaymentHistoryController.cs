using Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CariTakip.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class PaymentHistoryController : Controller
    {
        private readonly DatabaseContext _context;

        public PaymentHistoryController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var paymentHistories = await _context.PaymentHistories
                .Include(ph => ph.EmployeeItem)
                    .ThenInclude(ei => ei.Employee)
                .Include(ph => ph.EmployeeItem)
                    .ThenInclude(ei => ei.Item)
                .OrderByDescending(ph => ph.PaymentDate)
                .ToListAsync();

            return View(paymentHistories);
        }

        [HttpGet]
        public async Task<IActionResult> Export()
        {
            var paymentHistories = await _context.PaymentHistories
                .Include(ph => ph.EmployeeItem)
                    .ThenInclude(ei => ei.Employee)
                .Include(ph => ph.EmployeeItem)
                    .ThenInclude(ei => ei.Item)
                .OrderByDescending(ph => ph.PaymentDate)
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Payment Histories");
                worksheet.Cells["A1"].Value = "Employee Name";
                worksheet.Cells["B1"].Value = "Item Name";
                worksheet.Cells["C1"].Value = "Total Debt";
                worksheet.Cells["D1"].Value = "Paid Amount";
                worksheet.Cells["E1"].Value = "Remaining Debt";
                worksheet.Cells["F1"].Value = "Payment Date";

                for (int i = 0; i < paymentHistories.Count; i++)
                {
                    var history = paymentHistories[i];
                    worksheet.Cells[i + 2, 1].Value = history.EmployeeItem.Employee.Name;
                    worksheet.Cells[i + 2, 2].Value = history.EmployeeItem.Item.Name;
                    worksheet.Cells[i + 2, 3].Value = history.EmployeeItem.Item.Price;
                    worksheet.Cells[i + 2, 4].Value = history.PaidAmount;
                    worksheet.Cells[i + 2, 5].Value = history.RemainingDebt;
                    worksheet.Cells[i + 2, 6].Value = history.PaymentDate.ToString("dd-MM-yyyy HH:mm");
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = "PaymentHistories.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }
    }
}
