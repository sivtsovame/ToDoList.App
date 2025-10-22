using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks; 
using System.Windows.Input;
using Avalonia.Media;
using ToDoList.App.Commands;
using ToDoList.App.Models;
using ToDoList.App.Services;

namespace ToDoList.App.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IStorageService _storage;
        public ObservableCollection<TaskItem> Tasks { get; } = new();
        public ObservableCollection<TaskItem> FilteredTasks { get; } = new();
        private string _newTitle = string.Empty;
        public string NewTitle
        {
            get => _newTitle;
            set
            {
                if (_newTitle != value)
                {
                    _newTitle = value;
                    OnPropertyChanged(nameof(NewTitle));
                    (AddTaskCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string? _newDescription;
        public string? NewDescription
        {
            get => _newDescription;
            set
            {
                if (_newDescription != value)
                {
                    _newDescription = value;
                    OnPropertyChanged(nameof(NewDescription));
                }
            }
        }

        private DateTimeOffset? _newDeadline;
        public DateTimeOffset? NewDeadline
        {
            get => _newDeadline;
            set
            {
                if (_newDeadline != value)
                {
                    _newDeadline = value;
                    OnPropertyChanged(nameof(NewDeadline));
                }
            }
        }
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    ApplyFilter();
                }
            }
        }

        private void ApplyFilter()
        {
            var query = (SearchText ?? string.Empty).Trim().ToLowerInvariant();

            FilteredTasks.Clear();

            var source = string.IsNullOrEmpty(query)
                ? Tasks
                : Tasks.Where(t =>
                       (t.Title ?? string.Empty).ToLowerInvariant().Contains(query) ||
                       (t.Description ?? string.Empty).ToLowerInvariant().Contains(query) ||
                       t.CreatedAt.ToString("dd.MM.yyyy HH:mm").ToLowerInvariant().Contains(query) ||
                       (t.Deadline?.ToString("dd.MM.yyyy") ?? string.Empty).ToLowerInvariant().Contains(query)
                  );

            foreach (var t in source)
                FilteredTasks.Add(t);
        }
        public class ColorOption
        {
            public string Name { get; set; } = string.Empty;   // отображается в ComboBox
            public string Value { get; set; } = string.Empty;  // реальный цвет
        }

        private ColorOption _selectedColor = new() { Name = "Белый", Value = "White" };
        public ColorOption SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (_selectedColor != value)
                {
                    _selectedColor = value;
                    OnPropertyChanged(nameof(SelectedColor));
                    OnPropertyChanged(nameof(SelectedBackgroundBrush));
                }
            }
        }

        public IBrush SelectedBackgroundBrush =>
            string.IsNullOrWhiteSpace(SelectedColor?.Value) ? Brushes.White : Brush.Parse(SelectedColor.Value);

        public List<ColorOption> AvailableColors { get; } = new()
        {
            new() { Name = "Белый",          Value = "White" },
            new() { Name = "Бежевый",        Value = "Beige" },
            new() { Name = "Лавандовый",     Value = "Lavender" },
            new() { Name = "Мятный",         Value = "MintCream" },
            new() { Name = "Голубой",        Value = "AliceBlue" },
            new() { Name = "Медовый",        Value = "Honeydew" },
            new() { Name = "Жёлтый",         Value = "LightYellow" },
            new() { Name = "Розовый",        Value = "MistyRose" },
            new() { Name = "Слоновая кость", Value = "OldLace" },
            new() { Name = "Светло-розовый", Value = "#FFF5F5" }
        };
        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ToggleCompletedCommand { get; }
        public MainViewModel() : this(new JsonStorageService()) { }

        public MainViewModel(IStorageService storage)
        {
            _storage = storage;

            AddTaskCommand        = new RelayCommand(_ => AddTask(), _ => !string.IsNullOrWhiteSpace(NewTitle));
            DeleteTaskCommand     = new RelayCommand(t => DeleteTask(t as TaskItem));
            ToggleCompletedCommand = new RelayCommand(t =>
            {
                if (t is TaskItem item)
                {
                    item.IsCompleted = !item.IsCompleted;
                    Resort();
                    ApplyFilter();
                    _ = SaveAsync();
                }
            });

            _ = LoadAsync();
        }
        private async Task LoadAsync()
        {
            try
            {
                var items = await _storage.LoadAsync();
                foreach (var t in items)
                    if (t.CreatedAt == default)
                        t.CreatedAt = DateTimeOffset.Now;

                Tasks.Clear();
                foreach (var t in items
                    .OrderBy(t => t.IsCompleted)
                    .ThenBy(t => t.Deadline ?? DateTimeOffset.MaxValue)
                    .ThenBy(t => t.Title))
                {
                    Tasks.Add(t);
                }

                ApplyFilter();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка загрузки: " + ex.Message);
            }
        }

        private async Task SaveAsync()
        {
            try { await _storage.SaveAsync(Tasks); }
            catch (Exception ex) { Console.WriteLine("Ошибка сохранения: " + ex.Message); }
        }

        public void Resort()
        {
            // ключ сортировки: если CompletedOnTop=true, тогда завершённые=0, активные=1 (вверх идут завершённые)
            int Key(TaskItem t) => CompletedOnTop ? (t.IsCompleted ? 0 : 1) : (t.IsCompleted ? 1 : 0);

            var sorted = Tasks
                .OrderBy(Key)
                .ThenBy(t => t.Deadline ?? DateTimeOffset.MaxValue)
                .ThenBy(t => t.Title, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            Tasks.Clear();
            foreach (var t in sorted) Tasks.Add(t);
        }

        
        private bool _completedOnTop;
public bool CompletedOnTop
{
    get => _completedOnTop;
    set
    {
        if (_completedOnTop != value)
        {
            _completedOnTop = value;
            OnPropertyChanged(nameof(CompletedOnTop));
            ResortAndRefresh();
        }
    }
}

        private void AddTask()
        {
            if (string.IsNullOrWhiteSpace(NewTitle))
                return;

            var item = new TaskItem
            {
                Title = NewTitle.Trim(),
                Description = string.IsNullOrWhiteSpace(NewDescription) ? null : NewDescription!.Trim(),
                Deadline = NewDeadline,
                CreatedAt = DateTimeOffset.Now
            };

            Tasks.Add(item);
            Resort();
            ApplyFilter();

            NewTitle = string.Empty;
            NewDescription = null;
            NewDeadline = null;

            _ = SaveAsync();
        }

        public void ResortAndRefresh()
        {
            Resort();
            ApplyFilter();
        }

        // безопасное сохранение для code-behind
        public async System.Threading.Tasks.Task SaveAsyncSafe()
        {
            try { await _storage.SaveAsync(Tasks); }
            catch (Exception ex) { Console.WriteLine("Ошибка сохранения: " + ex.Message); }
        }

        private void DeleteTask(TaskItem? item)
        {
            if (item is null) return;
            Tasks.Remove(item);
            ApplyFilter();
            _ = SaveAsync();
        }
    }
}
