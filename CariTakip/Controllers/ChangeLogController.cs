using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CariTakip.Models;
using Microsoft.AspNetCore.Authorization;
using Entities.Entities;
using Data.Context;
using OfficeOpenXml;

namespace CariTakip.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ChangeLogController : Controller
    {
        private readonly DatabaseContext _context;

        public ChangeLogController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var employeeLogs = await _context.ChangeLogEmployees
                                              .Include(cl => cl.User)
                                              .Include(cl => cl.Employee)
                                              .Select(cl => new CombinedChangeLog
                                              {
                                                  Action = cl.Action,
                                                  OldValues = cl.OldValues,
                                                  NewValues = cl.NewValues,
                                                  ChangeDate = cl.ChangeDate,
                                                  User = cl.User.Username,
                                                  EmployeeName = cl.Employee != null ? cl.Employee.Name : null,
                                                  ItemName = null,
                                                  LogType = "Employee"
                                              })
                                              .ToListAsync();

            var itemLogs = await _context.ChangeLogItems
                                          .Include(cli => cli.User)
                                          .Include(cli => cli.Item)
                                          .Select(cli => new CombinedChangeLog
                                          {
                                              Action = cli.Action,
                                              OldValues = cli.OldValues,
                                              NewValues = cli.NewValues,
                                              ChangeDate = cli.ChangeDate,
                                              User = cli.User.Username,
                                              EmployeeName = null,
                                              ItemName = cli.Item != null ? cli.Item.Name : null,
                                              LogType = "Item"
                                          })
                                          .ToListAsync();

            var employeeitemLogs = await _context.ChangeLogEmployeeItems
                                          .Include(cli => cli.User)
                                          .Include(cli => cli.EmployeeItem)
                                          .Select(cli => new CombinedChangeLog
                                          {
                                              Action = cli.Action,
                                              OldValues = cli.OldValues,
                                              NewValues = cli.NewValues,
                                              ChangeDate = cli.ChangeDate,
                                              User = cli.User.Username,
                                              EmployeeName = cli.EmployeeItem != null ? cli.EmployeeItem.Employee.Name : null,
                                              ItemName = cli.EmployeeItem != null ? cli.EmployeeItem.Item.Name : null,
                                              LogType = "EmployeeItem"
                                          })
                                          .ToListAsync();

            var viewModel = new CombinedChangeLogViewModel
            {
                Logs = employeeLogs.Concat(itemLogs).Concat(employeeitemLogs).OrderByDescending(log => log.ChangeDate).ToList()
            };

            return View(viewModel);
        }

        public IActionResult ExportExcel()
        {
            var logs = _context.ChangeLogEmployees
                .Include(cl => cl.User)
                .Include(cl => cl.Employee)
                .Select(cl => new CombinedChangeLog
                {
                    Action = cl.Action,
                    OldValues = cl.OldValues,
                    NewValues = cl.NewValues,
                    ChangeDate = cl.ChangeDate,
                    User = cl.User.Username,
                    EmployeeName = cl.Employee != null ? cl.Employee.Name : null,
                    ItemName = null,
                    LogType = "Employee"
                })
                .ToList();

            logs.AddRange(_context.ChangeLogItems
                .Include(cli => cli.User)
                .Include(cli => cli.Item)
                .Select(cli => new CombinedChangeLog
                {
                    Action = cli.Action,
                    OldValues = cli.OldValues,
                    NewValues = cli.NewValues,
                    ChangeDate = cli.ChangeDate,
                    User = cli.User.Username,
                    EmployeeName = null,
                    ItemName = cli.Item != null ? cli.Item.Name : null,
                    LogType = "Item"
                })
                .ToList());

            logs.AddRange(_context.ChangeLogEmployeeItems
                .Include(cli => cli.User)
                .Include(cli => cli.EmployeeItem)
                .Select(cli => new CombinedChangeLog
                {
                    Action = cli.Action,
                    OldValues = cli.OldValues,
                    NewValues = cli.NewValues,
                    ChangeDate = cli.ChangeDate,
                    User = cli.User.Username,
                    EmployeeName = cli.EmployeeItem != null ? cli.EmployeeItem.Employee.Name : null,
                    ItemName = cli.EmployeeItem != null ? cli.EmployeeItem.Item.Name : null,
                    LogType = "EmployeeItem"
                })
                .ToList());

            var viewModel = new CombinedChangeLogViewModel
            {
                Logs = logs.OrderByDescending(log => log.ChangeDate).ToList()
            };

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("ChangeLogs");

                worksheet.Cells[1, 1].Value = "Change Date";
                worksheet.Cells[1, 2].Value = "Action";
                worksheet.Cells[1, 3].Value = "User";
                worksheet.Cells[1, 4].Value = "Employee Name";
                worksheet.Cells[1, 5].Value = "Item Name";
                worksheet.Cells[1, 6].Value = "Old Values";
                worksheet.Cells[1, 7].Value = "New Values";
                worksheet.Cells[1, 8].Value = "Log Type";

                int row = 2;
                foreach (var log in viewModel.Logs)
                {
                    worksheet.Cells[row, 1].Value = log.ChangeDate.ToString("dd-MM-yyyy HH:mm");
                    worksheet.Cells[row, 2].Value = log.Action;
                    worksheet.Cells[row, 3].Value = log.User;
                    worksheet.Cells[row, 4].Value = log.EmployeeName;
                    worksheet.Cells[row, 5].Value = log.ItemName;
                    worksheet.Cells[row, 6].Value = log.OldValues;
                    worksheet.Cells[row, 7].Value = log.NewValues;
                    worksheet.Cells[row, 8].Value = log.LogType;
                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ChangeLogs.xlsx");
            }
        }


    }
}
