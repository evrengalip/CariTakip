using Business.Abstract;
using Business.Concrete;
using CariTakip.Services;
using Entities.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Linq;
using System.Threading.Tasks;

namespace CariTakip.Controllers
{
    [Authorize(Roles = "Admin,User")]
    [Authorize]
    public class ItemController : Controller
    {
        private readonly IItemService _itemService;
        private readonly ChangeLogItemService _changeLogItemService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ItemController(IItemService itemService, ChangeLogItemService changeLogItemService, IWebHostEnvironment webHostEnvironment)
        {
            _itemService = itemService;
            _changeLogItemService = changeLogItemService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _itemService.GetAllItemsAsync();
            return View(items);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Item model, IFormFile imageFile)
        {
           
                // Tüm öğeleri al ve listeye dönüştür
                var items = await _itemService.GetAllItemsAsync();
                if (items.Any(x => x.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError(nameof(model.Name), "Item already exists.");
                    return View(model);
                }

                if (imageFile != null && imageFile.Length > 0)
                {
                    // Dosya adını ve dosya yolunu belirle
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    var filePath = Path.Combine(uploads, fileName);

                    // Dosyayı kaydet
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    model.ImagePath = $"/images/{fileName}";
                }
                else
                {
                    model.ImagePath = null;
                }

                await _itemService.AddItemAsync(model);
                _changeLogItemService.LogItemChange("Create", null, model);

                return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _itemService.GetItemByIdAsync(id.Value);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Item model, IFormFile imageFile)
        {
            if (id != model.ItemId)
            {
                return NotFound();
            }

            var oldItem = await _itemService.GetItemByIdAsync(id);
            if (oldItem == null)
            {
                return NotFound();
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                // Dosya adını ve dosya yolunu belirle
                var fileName = Path.GetFileName(imageFile.FileName);
                var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                var filePath = Path.Combine(uploads, fileName);

                // Dosyayı kaydet
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                model.ImagePath = $"/images/{fileName}";
            }
            else
            {
                // Yeni bir resim yüklenmediğinde eski resim yolunu koru
                model.ImagePath = oldItem.ImagePath;
            }

            try
            {
                await _itemService.UpdateItemAsync(model);
                _changeLogItemService.LogItemChange("Edit", oldItem, model);
            }
            catch (InvalidOperationException)
            {
                if (await _itemService.GetItemByIdAsync(id) == null)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _itemService.GetItemByIdAsync(id.Value);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _itemService.DeleteItemAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
            [HttpGet]
        public async Task<IActionResult> Export()
        {
            var items = await _itemService.GetAllItemsAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Items");

            // Sütun başlıkları
            worksheet.Cells[1, 1].Value = "Item Name";
            worksheet.Cells[1, 2].Value = "Price";

            // Veriler
            for (int i = 0; i < items.Count(); i++)
            {
                var item = items.ElementAt(i);
                worksheet.Cells[i + 2, 1].Value = item.Name;
                worksheet.Cells[i + 2, 2].Value = item.Price;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var excelData = package.GetAsByteArray();

            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Items.xlsx");
        }
    }
}
