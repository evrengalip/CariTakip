using Data.Context;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Security.Claims;

namespace CariTakip.Services
{
    public class ChangeLogEmployeeItemService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangeLogEmployeeItemService(DatabaseContext databaseContext, IHttpContextAccessor httpContextAccessor)
        {
            _databaseContext = databaseContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public void LogChangeEmployeeItem(string action, EmployeeItem oldEmployeeItem, EmployeeItem newEmployeeItem)
        {
            var userId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));


            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var changeLog = new ChangeLogEmployeeItem
            {
                Action = action,
                OldValues = oldEmployeeItem != null ? JsonConvert.SerializeObject(oldEmployeeItem, settings) : null,
                NewValues = newEmployeeItem != null ? JsonConvert.SerializeObject(newEmployeeItem, settings) : null,
                ChangeDate = DateTime.Now,
                UserId = userId,
                EmployeeItemId = newEmployeeItem?.EmployeeItemId ?? oldEmployeeItem.EmployeeItemId
            };

            _databaseContext.ChangeLogEmployeeItems.Add(changeLog);
            _databaseContext.SaveChanges();
        }


    }
}