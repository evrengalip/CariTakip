using Data.Context;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Security.Claims;

namespace CariTakip.Services
{
    public class ChangeLogEmployeeService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangeLogEmployeeService(DatabaseContext databaseContext, IHttpContextAccessor httpContextAccessor)
        {
            _databaseContext = databaseContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public void LogEmployeeChange(string action, Employee oldEmployee, Employee newEmployee)
        {
            var userId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var changeLog = new ChangeLogEmployee
            {
                Action = action,
                OldValues = oldEmployee != null ? JsonConvert.SerializeObject(oldEmployee) : null,
                NewValues = newEmployee != null ? JsonConvert.SerializeObject(newEmployee) : null,
                ChangeDate = DateTime.Now,
                UserId = userId,
                EmployeeId = newEmployee?.EmployeeId ?? oldEmployee.EmployeeId
            };

            _databaseContext.ChangeLogEmployees.Add(changeLog);
            _databaseContext.SaveChanges();
        }


    }
}
