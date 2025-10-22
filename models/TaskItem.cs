using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ToDoList.App.Models
{
    
public class TaskItem : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private string? _description;
        private DateTimeOffset? _deadline;
        private bool _isCompleted;
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public string Title
        {
            get => _title;
            set { if (_title != value) { _title = value; OnPropertyChanged(nameof(Title)); } }
        }
        public string? Description
        {
            get => _description;
            set { if (_description != value) { _description = value; OnPropertyChanged(nameof(Description)); } }
        }
        public DateTimeOffset? Deadline
        {
            get => _deadline;
            set { if (_deadline != value) { _deadline = value; OnPropertyChanged(nameof(Deadline)); } }
        }
        public bool IsCompleted
        {
            get => _isCompleted;
            set { if (_isCompleted != value) { _isCompleted = value; OnPropertyChanged(nameof(IsCompleted)); } }
        }
        [JsonIgnore]
        public bool IsOverdue =>
            !IsCompleted && Deadline is DateTimeOffset d && d.Date < DateTimeOffset.Now.Date;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
