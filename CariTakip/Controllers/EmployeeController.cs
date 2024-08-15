using Business.Abstract;
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
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly ChangeLogEmployeeService _changeLogEmployeeService;

        public EmployeeController(IEmployeeService employeeService, ChangeLogEmployeeService changeLogEmployeeService)
        {
            _employeeService = employeeService;
            _changeLogEmployeeService = changeLogEmployeeService;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return View(employees);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeId,Name,Department,Position,Email")] Employee model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _employeeService.AddEmployeeAsync(model);
                    _changeLogEmployeeService.LogEmployeeChange("Create", null, model);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeId,Name,Department,Position,Email")] Employee model)
        {
            if (id != model.EmployeeId)
            {
                return NotFound();
            }


            try
            {
                var oldEmployee = await _employeeService.GetEmployeeByIdAsync(id);
                await _employeeService.UpdateEmployeeAsync(model);
                _changeLogEmployeeService.LogEmployeeChange("Edit", oldEmployee, model);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _employeeService.GetEmployeeByIdAsync(id) == null)
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

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _employeeService.DeleteEmployeeAsync(id);
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
            var employees = await _employeeService.GetAllEmployeesAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Employees");

            // Sütun başlıkları
            worksheet.Cells[1, 1].Value = "Employee Name";
            worksheet.Cells[1, 2].Value = "Department";
            worksheet.Cells[1, 3].Value = "Position";
            worksheet.Cells[1, 4].Value = "Email";

            // Veriler
            for (int i = 0; i < employees.Count(); i++)
            {
                var employee = employees.ElementAt(i);
                worksheet.Cells[i + 2, 1].Value = employee.Name;
                worksheet.Cells[i + 2, 2].Value = employee.Department;
                worksheet.Cells[i + 2, 3].Value = employee.Position;
                worksheet.Cells[i + 2, 4].Value = employee.Email;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var excelData = package.GetAsByteArray();

            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Employees.xlsx");
        }
    }
}
