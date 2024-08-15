using CariTakip.Services;
using Data.Context;
using Entities.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace CariTakip.Controllers
{
    [Authorize(Roles = "Admin,User")]
    [Authorize]
    public class EmployeeItemController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly ChangeLogEmployeeItemService _changeLogEmployeeItemService;
        public EmployeeItemController(DatabaseContext context, ChangeLogEmployeeItemService changeLogEmployeeItemService)
        {
            _context = context;
            _changeLogEmployeeItemService = changeLogEmployeeItemService;
        }

        public async Task<IActionResult> Index()
        {
            var employeeItems = await _context.EmployeeItems
                .Include(ei => ei.Employee)
                .Include(ei => ei.Item)
                .ToListAsync();

            return View(employeeItems);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Employees = new SelectList(await _context.Employees.ToListAsync(), "EmployeeId", "Name");
            ViewBag.Items = new SelectList(await _context.Items.ToListAsync(), "ItemId", "Name");

            return View();
        }

        // POST: EmployeeItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeItem model)
        {
            if (model.EmployeeId == 0 || model.ItemId == 0)
            {
                ModelState.AddModelError(string.Empty, "Employee or Item is not specified.");
                ViewBag.Employees = new SelectList(_context.Employees, "EmployeeId", "Name");
                ViewBag.Items = new SelectList(_context.Items, "ItemId", "Name");
                return View(model);
            }

            bool employeeItemExists = _context.EmployeeItems
                .Any(ei => ei.EmployeeId == model.EmployeeId && ei.ItemId == model.ItemId);

            if (employeeItemExists)
            {
                ModelState.AddModelError(string.Empty, "Employee already has that item.");
                ViewBag.Employees = new SelectList(_context.Employees, "EmployeeId", "Name");
                ViewBag.Items = new SelectList(_context.Items, "ItemId", "Name");
                return View(model);
            }

            _context.EmployeeItems.Add(model);
            await _context.SaveChangesAsync();
            _changeLogEmployeeItemService.LogChangeEmployeeItem("Create", null, model);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeItem = await _context.EmployeeItems
                .Include(ei => ei.Employee)
                .Include(ei => ei.Item)
                .FirstOrDefaultAsync(ei => ei.EmployeeItemId == id);

            if (employeeItem == null)
            {
                return NotFound();
            }

            ViewBag.EmployeeName = employeeItem.Employee?.Name ?? "Employee not found";
            ViewBag.ItemName = employeeItem.Item?.Name ?? "Item not found";

            return View(employeeItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeItem model)
        {
            if (id != model.EmployeeItemId)
            {
                return NotFound();
            }

            var employeeItem = await _context.EmployeeItems
                .Include(ei => ei.Employee)
                .Include(ei => ei.Item)
                .FirstOrDefaultAsync(ei => ei.EmployeeItemId == id);

            if (employeeItem == null)
            {
                return NotFound();
            }

            // Önceki ödenen miktarı alalım
            decimal previousPaidAmount = employeeItem.PaidAmount;

            // Modelden gelen verileri güncelleyelim
            employeeItem.PaidAmount = model.PaidAmount;
            employeeItem.PaymentDate = model.PaymentDate;
            employeeItem.TotalPaidAmount = (model.PaidAmount + previousPaidAmount);
            // Eğer ödenen miktar değiştiyse yeni bir PaymentHistory kaydı oluşturalım
            if (previousPaidAmount != model.PaidAmount)
            {
                var paymentHistory = new PaymentHistory
                {
                    EmployeeItemId = employeeItem.EmployeeItemId,
                    PaidAmount = model.PaidAmount,
                    RemainingDebt = (model.Item.Price) - employeeItem.TotalPaidAmount,
                    PaymentDate = model.PaymentDate ?? DateTime.Now // Varsa ödeme tarihini kullan, yoksa şu anki zamanı

                };

                _context.PaymentHistories.Add(paymentHistory);
            }
            
            // Değişiklikleri kaydet
            _context.Update(employeeItem);
            await _context.SaveChangesAsync();
            _changeLogEmployeeItemService.LogChangeEmployeeItem("Edit", employeeItem, model);
            return RedirectToAction(nameof(Index));
        }





        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeItem = await _context.EmployeeItems.FindAsync(id);

            if (employeeItem == null)
            {
                return NotFound();
            }

            return View(employeeItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] int id)
        {
            var employeeItem = await _context.EmployeeItems.FindAsync(id);
            if (employeeItem == null)
            {
                return NotFound();
            }

            // Önce değişiklikleri kaydetmeden önce silme işlemi
            var changeLogs = _context.ChangeLogEmployeeItems
                .Where(cl => cl.EmployeeItemId == id)
                .ToList();

            // Log kayıtlarını silme
            if (changeLogs.Any())
            {
                _context.ChangeLogEmployeeItems.RemoveRange(changeLogs);
                await _context.SaveChangesAsync();
            }

            // EmployeeItem'ı silme
            _context.EmployeeItems.Remove(employeeItem);
            await _context.SaveChangesAsync();

            // Eğer log kayıtlarını değiştirmeyi istiyorsanız, bunu yapabilirsiniz
            // Örneğin, burada 'null' ile birlikte yeni bir log kaydı eklemeyi düşünüyorsanız:
            // _changeLogService.LogChangeEmployeeItem("Delete", employeeItem, null);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Export()
        {
            var employeeItems = await _context.EmployeeItems
                .Include(ei => ei.Employee)
                .Include(ei => ei.Item)
                .ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("EmployeeItems");

                // Başlıkları ekleyin
                worksheet.Cells["A1"].Value = "Employee";
                worksheet.Cells["B1"].Value = "Item";
                worksheet.Cells["C1"].Value = "PaidAmount";
                worksheet.Cells["D1"].Value = "TotalPaidAmount";
                worksheet.Cells["E1"].Value = "DateTaken";
                worksheet.Cells["F1"].Value = "PaymentDate";

                // Format ayarları için başlık hücrelerini kalın yapın
                using (var range = worksheet.Cells["A1:F1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Verileri ekleyin
                for (int i = 0; i < employeeItems.Count; i++)
                {
                    var row = i + 2;
                    worksheet.Cells[row, 1].Value = employeeItems[i].Employee?.Name;
                    worksheet.Cells[row, 2].Value = employeeItems[i].Item?.Name;
                    worksheet.Cells[row, 3].Value = employeeItems[i].PaidAmount;
                    worksheet.Cells[row, 4].Value = employeeItems[i].TotalPaidAmount;
                    worksheet.Cells[row, 5].Value = employeeItems[i].DateTaken;
                    worksheet.Cells[row, 6].Value = employeeItems[i].PaymentDate;

                    // Para birimi formatı (TL)
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "₺ #,##0.00";
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "₺ #,##0.00";

                    // Tarih formatı
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "dd.MM.yyyy";
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "dd.MM.yyyy";
                }

                // Sütunları otomatik olarak sığdırma
                worksheet.Cells["A:F"].AutoFitColumns();

                // Excel dosyasını byte dizisine dönüştürme ve dosyayı döndürme
                var excelData = package.GetAsByteArray();
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeeItems.xlsx");
            }
        }


    }
}