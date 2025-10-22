using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ToDoList.App.Models;

namespace ToDoList.App.Services
{
    public class JsonStorageService : IStorageService
    {
        private readonly string _folder;
        private readonly string _filePath;

        private static readonly JsonSerializerOptions _opts = new()
        {
            WriteIndented = true
        };

        public JsonStorageService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _folder = Path.Combine(appData, "ToDoList");
            Directory.CreateDirectory(_folder);
            _filePath = Path.Combine(_folder, "tasks.json");
        }

        public async Task<List<TaskItem>> LoadAsync()
        {
            if (!File.Exists(_filePath)) return new List<TaskItem>();
            await using var s = File.OpenRead(_filePath);
            return await JsonSerializer.DeserializeAsync<List<TaskItem>>(s) ?? new List<TaskItem>();
        }

        public async Task SaveAsync(IEnumerable<TaskItem> tasks)
        {
            await using var s = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(s, tasks, _opts);
        }
    }
}
