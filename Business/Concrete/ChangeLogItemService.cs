using Newtonsoft.Json;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Entities.Entities;
using Data.Context;

namespace CariTakip.Services
{
    public class ChangeLogItemService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangeLogItemService(DatabaseContext databaseContext, IHttpContextAccessor httpContextAccessor)
        {
            _databaseContext = databaseContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public void LogItemChange(string action, Item oldItem, Item newItem)
        {
            var userId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var changeLogItem = new ChangeLogItem
            {
                Action = action,
                OldValues = oldItem != null ? JsonConvert.SerializeObject(oldItem) : null,
                NewValues = newItem != null ? JsonConvert.SerializeObject(newItem) : null,
                ChangeDate = DateTime.Now,
                UserId = userId,
                ItemId = newItem?.ItemId ?? oldItem?.ItemId
            };

            _databaseContext.ChangeLogItems.Add(changeLogItem);
            _databaseContext.SaveChanges();
        }
    }
}