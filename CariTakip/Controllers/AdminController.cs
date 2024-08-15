using Data.Context;
using Entities.Entities;
using Entities.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CariTakip.Controllers
{
    //[Authorize(Roles ="admin,manager")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly DatabaseContext _context;

        public AdminController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.Role) // Enum değerleri int olarak sıralanır
                .ToListAsync();

            return View(users);
        }


        [HttpGet]
        public IActionResult EditUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            // Enum değerlerini al
            var roles = Enum.GetValues(typeof(Roles))
                             .Cast<Roles>()
                             .Select(r => new { Value = r.ToString(), Text = r.ToString() })
                             .ToList();

            // SelectList oluştur
            ViewBag.Roles = new SelectList(roles, "Value", "Text", user.Role);

            return View(user);
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, User model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Güncellemeler
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.Role = model.Role;
                user.Locked = model.Locked;

                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                
                
                    return NotFound();
                
              
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public IActionResult Export()
        {
            var users = _context.Users
                .Select(u => new
                {
                    u.FullName,
                    u.Username,
                    u.Email,
                    u.Role,
                    Locked = u.Locked ? "Yes" : "No",
                    CreatedAt = u.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") // Format CreatedAt as string
                })
                .ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Users");
                worksheet.Cells["A1"].LoadFromCollection(users, true);

                // Apply formatting to CreatedAt column if needed (column index 6 in this case)
                int createdAtColumnIndex = 6; // Update to the correct column index
                int startRow = 2; // Starting row of data

                // Set the format for the CreatedAt column
                for (int row = startRow; row <= worksheet.Dimension.End.Row; row++)
                {
                    var cell = worksheet.Cells[row, createdAtColumnIndex];
                    cell.Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss"; // Customize format if needed
                }

                // Auto fit columns
                worksheet.Cells.AutoFitColumns();

                using (var stream = new MemoryStream())
                {
                    package.SaveAs(stream);
                    var fileContent = stream.ToArray();
                    return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
                }
            }
        }

    }
}
