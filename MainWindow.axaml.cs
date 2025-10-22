using System;
using Avalonia.Controls;
using ToDoList.App.ViewModels;
using ToDoList.App.Views;
using ToDoList.App.Models;

namespace ToDoList.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OnAddClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm is null) return;

            var draft = new TaskDialog.TaskDraft
            {
                DialogTitle = "Новая заметка",
                Title = "",
                Description = "",
                Deadline = null
            };

            var dlg = new TaskDialog { DataContext = draft };
            var result = await dlg.ShowDialog<TaskDialog.TaskDraft?>(this);
            if (result is null) return;

            // добавляем через VM
            var item = new TaskItem
            {
                Title = result.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(result.Description) ? null : result.Description.Trim(),
                Deadline = result.Deadline,
                CreatedAt = DateTimeOffset.Now
            };

            vm.Tasks.Add(item);
            vm.ResortAndRefresh();
            await vm.SaveAsyncSafe();
        }

        private async void OnEditClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm is null) return;

            // текущая карточка — DataContext элемента
            if ((sender as Control)?.DataContext is not TaskItem item) return;

            // черновик с копией значений
            var draft = new TaskDialog.TaskDraft
            {
                DialogTitle = "Редактировать заметку",
                Title = item.Title,
                Description = item.Description,
                Deadline = item.Deadline
            };

            var dlg = new TaskDialog { DataContext = draft };
            var result = await dlg.ShowDialog<TaskDialog.TaskDraft?>(this);
            if (result is null) return; // отмена

            // применяем изменения
            item.Title = result.Title.Trim();
            item.Description = string.IsNullOrWhiteSpace(result.Description) ? null : result.Description.Trim();
            item.Deadline = result.Deadline;

            vm.ResortAndRefresh();
            await vm.SaveAsyncSafe();
        }
    }
}
