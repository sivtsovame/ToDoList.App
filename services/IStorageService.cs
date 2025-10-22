using System.Collections.Generic;
using System.Threading.Tasks;
using ToDoList.App.Models;

namespace ToDoList.App.Services
{
    public interface IStorageService
    {
        Task<List<TaskItem>> LoadAsync();
        Task SaveAsync(IEnumerable<TaskItem> tasks);
    }
}
