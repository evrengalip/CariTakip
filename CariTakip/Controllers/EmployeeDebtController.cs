using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using CariTakip.Services;
using Entities.Entities;
using Microsoft.AspNetCore.Authorization;
using Data.Context;

namespace CariTakip.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class EmployeeDebtController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly ChangeLogEmployeeItemService _changeLogEmployeeItemService;

        public EmployeeDebtController(DatabaseContext context, ChangeLogEmployeeItemService changeLogEmployeeItemService)
        {
            _context = context;
            _changeLogEmployeeItemService = changeLogEmployeeItemService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? employeeId)
        {
            ViewBag.Employees = new SelectList(await _context.Employees.ToListAsync(), "EmployeeId", "Name");
            ViewBag.Items = await _context.Items.ToListAsync();

            if (employeeId.HasValue)
            {
                var employeeItems = await _context.EmployeeItems
                    .Where(ei => ei.EmployeeId == employeeId.Value)
                    .Include(ei => ei.Item)
                    .ToListAsync();

                ViewBag.TotalDebt = employeeItems.Any() ? employeeItems.Sum(ei => ei.Item.Price) - employeeItems.Sum(ei => ei.TotalPaidAmount) : 0;
                ViewBag.TotalPaidAmount = employeeItems.Any() ? employeeItems.Sum(ei => ei.TotalPaidAmount) : 0;
                ViewBag.SelectedEmployeeId = employeeId.Value;

                return View(employeeItems);
            }

            return View(new List<EmployeeItem>());
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeItems(int employeeId)
        {
            if (employeeId <= 0)
            {
                return PartialView("_EmployeeItemsPartial", new List<EmployeeItem>());
            }

            var employeeItems = await _context.EmployeeItems
                .Where(ei => ei.EmployeeId == employeeId)
                .Include(ei => ei.Item)
                .ToListAsync();

            ViewBag.TotalPaidAmount = employeeItems.Any() ? employeeItems.Sum(ei => ei.TotalPaidAmount) : 0;
            ViewBag.TotalDebt = employeeItems.Any() ? employeeItems.Sum(ei => ei.Item.Price) - employeeItems.Sum(ei => ei.TotalPaidAmount) : 0;
            ViewBag.SelectedEmployeeId = employeeId;

            return PartialView("_EmployeeItemsPartial", employeeItems);
        }

        [HttpGet]
        public IActionResult Export(int employeeId)
        {
            if (employeeId <= 0)
            {
                return BadRequest("Invalid employee ID.");
            }

            var items = _context.EmployeeItems
                                .Include(ei => ei.Item)
                                .Where(ei => ei.EmployeeId == employeeId)
                                .ToList();

            if (!items.Any())
            {
                return BadRequest("No data found for the selected employee.");
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("EmployeeItems");

                worksheet.Cells[1, 1].Value = "Item Name";
                worksheet.Cells[1, 2].Value = "Price";
                worksheet.Cells[1, 3].Value = "Total Paid Amount";
                worksheet.Cells[1, 4].Value = "Date Taken";
                worksheet.Cells[1, 5].Value = "Payment Date";
                worksheet.Cells[1, 6].Value = "Remaining Debt";

                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    worksheet.Cells[i + 2, 1].Value = item.Item?.Name ?? "N/A";
                    worksheet.Cells[i + 2, 2].Value = item.Item?.Price.ToString("C", CultureInfo.CurrentCulture) ?? "0";
                    worksheet.Cells[i + 2, 3].Value = item.TotalPaidAmount.ToString("C", CultureInfo.CurrentCulture);
                    worksheet.Cells[i + 2, 4].Value = item.DateTaken.ToShortDateString();
                    worksheet.Cells[i + 2, 5].Value = item.PaymentDate?.ToString("dd.MM.yyyy") ?? "N/A";
                    worksheet.Cells[i + 2, 6].Value = (item.Item != null ? item.Item.Price - item.TotalPaidAmount : 0).ToString("C", CultureInfo.CurrentCulture);
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"EmployeeItems_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeItemDetails(int id)
        {
            var employeeItem = await _context.EmployeeItems
                .Include(ei => ei.Item)
                .FirstOrDefaultAsync(ei => ei.EmployeeItemId == id);

            if (employeeItem == null)
            {
                return NotFound();
            }

            var result = new
            {
                employeeItemId = employeeItem.EmployeeItemId,
                itemName = employeeItem.Item?.Name ?? "N/A",
                price = employeeItem.Item?.Price.ToString("C") ?? "0",
                paidAmount = employeeItem.PaidAmount.ToString("C"),
                totalpaidAmount = employeeItem.TotalPaidAmount.ToString("C"),
                paymentDate = employeeItem.PaymentDate?.ToString("yyyy-MM-dd") ?? "N/A"
            };

            return Json(result);
        }

        [HttpGet]
        public IActionResult Create(int employeeId, int itemId)
        {
            var employeeItem = new EmployeeItem
            {
                EmployeeId = employeeId,
                ItemId = itemId
            };
            return View(employeeItem);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeItem model)
        {
            if (ModelState.IsValid)
            {
                _context.EmployeeItems.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { employeeId = model.EmployeeId });
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditEmployeeItem(EmployeeItem model)
        {
            var employeeItem = await _context.EmployeeItems
                .Include(ei => ei.Item)
                .FirstOrDefaultAsync(ei => ei.EmployeeItemId == model.EmployeeItemId);

            if (employeeItem == null)
            {
                return NotFound();
            }

            decimal previousPaidAmount = employeeItem.PaidAmount;

            employeeItem.PaidAmount = model.PaidAmount;
            employeeItem.PaymentDate = model.PaymentDate;
            employeeItem.TotalPaidAmount = model.PaidAmount + previousPaidAmount;

            if (previousPaidAmount != model.PaidAmount)
            {
                var paymentHistory = new PaymentHistory
                {
                    EmployeeItemId = employeeItem.EmployeeItemId,
                    PaidAmount = model.PaidAmount,
                    RemainingDebt = employeeItem.Item.Price - employeeItem.TotalPaidAmount,
                    PaymentDate = model.PaymentDate ?? DateTime.Now
                };

                _context.PaymentHistories.Add(paymentHistory);
            }

            _context.Update(employeeItem);
            await _context.SaveChangesAsync();
            _changeLogEmployeeItemService.LogChangeEmployeeItem("Edit", employeeItem, model);

            return RedirectToAction(nameof(Index), new { employeeId = employeeItem.EmployeeId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEmployeeItem(int id)
        {
            var employeeItem = await _context.EmployeeItems.FindAsync(id);

            if (employeeItem == null)
            {
                return NotFound();
            }

            _context.EmployeeItems.Remove(employeeItem);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
