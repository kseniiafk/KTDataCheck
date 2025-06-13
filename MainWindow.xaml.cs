using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace KTDataCheck
{
    public class ValidationMessageColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string message && message.Contains("успешно"))
                return Brushes.Green;
            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }


    public class MainViewModel : INotifyPropertyChanged
    {
        private UserModel _user = new UserModel();
        private string _validationMessage;

        public UserModel User
        {
            get => _user;
            set { _user = value; OnPropertyChanged(); }
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            set { _validationMessage = value; OnPropertyChanged(); }
        }

        public ICommand SubmitCommand { get; }

        public MainViewModel()
        {
            SubmitCommand = new RelayCommand(Submit);
        }

        private void Submit(object parameter)
        {
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var context = new ValidationContext(User);

            if (!Validator.TryValidateObject(User, context, results, true))
            {
                ValidationMessage = string.Join("\n", results.Select(r => r.ErrorMessage));
            }
            else
            {
                ValidationMessage = "Данные успешно сохранены!";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    public class UserModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private string _name;
        private string _email;
        private int _age;
        private string _phone;

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя должно быть от 3 до 50 символов")]
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        [Required(ErrorMessage = "Возраст обязателен")]
        [Range(18, 99, ErrorMessage = "Возраст должен быть от 18 до 99")]
        public int Age
        {
            get => _age;
            set { _age = value; OnPropertyChanged(); }
        }

        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Некорректный формат телефона")]
        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                var context = new ValidationContext(this) { MemberName = columnName };
                var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

                var property = GetType().GetProperty(columnName);
                if (property == null) return null;

                if (!Validator.TryValidateProperty(
                    property.GetValue(this),
                    context, results))
                {
                    return results.First().ErrorMessage;
                }

                return null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}