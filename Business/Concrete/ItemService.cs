using Business.Abstract;
using Data.Abstract;
using Data.Concrete;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;

        public ItemService(IItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        public async Task<IEnumerable<Item>> GetAllItemsAsync()
        {
            return await _itemRepository.GetAllItemsAsync();
        }

        public async Task<Item> GetItemByIdAsync(int id)
        {
            return await _itemRepository.GetItemByIdAsync(id);
        }

        public async Task AddItemAsync(Item item)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                throw new ArgumentException("Employee name is required.");
            }

            var employees = await _itemRepository.GetAllItemsAsync();
            if (employees.Any(e => e.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("An employee with this name already exists.");
            }

            await _itemRepository.AddItemAsync(item);
        }

        public async Task UpdateItemAsync(Item item)
        {
            await _itemRepository.UpdateItemAsync(item);
        }

        public async Task DeleteItemAsync(int id)
        {
            await _itemRepository.DeleteItemAsync(id);
        }
    }
}
