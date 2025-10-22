using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ToDoList.App.Views
{
    public partial class TaskDialog : Window
    {
        public class TaskDraft
        {
            public string DialogTitle { get; set; } = "Задача";
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public DateTimeOffset? Deadline { get; set; }
        }

        public TaskDialog()
        {
            InitializeComponent();
        }

        public void OnOk(object? sender, RoutedEventArgs e)
        {
            Close(DataContext as TaskDraft);
        }

        public void OnCancel(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }
    }
}
