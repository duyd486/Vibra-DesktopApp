using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Vibra_DesktopApp.ViewModels.Pages;

namespace Vibra_DesktopApp.ViewModels.Components
{
    public partial class HeaderViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        [ObservableProperty] private string _searchText;
        [ObservableProperty] private bool _isColorMenuOpen;
        public IReadOnlyList<string> ColorList { get; }

        // Selected color string (hex). Two-way bound to the ListBox SelectedItem.
        [ObservableProperty] private string _selectedColor;

        // Dark mode flag
        [ObservableProperty] private bool _isDarkMode;

        public HeaderViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));

            ColorList = new List<string>
            {
                "#BC4D15",
                "#645283",
                "#a8bfc9",
                "#CD5C5C",
                "#a3b18a",
                "#9e9fa5",
                "#926F4F",
                "#FEA7A0",
                "#c3a995",
                "#44B78B",
            };

            // Initialize selected color from app resources if present
            if (Application.Current?.Resources["CurrentColorString"] is string s)
            {
                SelectedColor = s;
            }
            else if (Application.Current?.Resources["CurrentColor"] is SolidColorBrush brush)
            {
                SelectedColor = brush.Color.ToString();
            }
            else
            {
                SelectedColor = ColorList[0];
                ApplyColorToResources(SelectedColor);
            }

            // Initialize dark mode from app resources if present
            if (Application.Current?.Resources["IsDarkMode"] is bool dark)
            {
                IsDarkMode = dark;
            }
            else
            {
                // default to dark for this app
                IsDarkMode = true;
                ApplyTheme(IsDarkMode);
            }

            SearchText = string.Empty;
            IsColorMenuOpen = false;
        }

        partial void OnSelectedColorChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            ApplyColorToResources(value);

            // close popup after selecting
            IsColorMenuOpen = false;
        }

        partial void OnIsDarkModeChanged(bool value)
        {
            ApplyTheme(value);
        }

        private void ApplyTheme(bool isDark)
        {
            try
            {
                Application.Current.Resources["IsDarkMode"] = isDark;

                // simple theme brushes - replace or extend with full dictionaries as needed
                var windowBg = isDark ? (Brush)new SolidColorBrush(Color.FromRgb(18, 18, 18)) : new SolidColorBrush(Color.FromRgb(250, 250, 250));
                var controlBg = isDark ? (Brush)new SolidColorBrush(Color.FromRgb(31, 31, 31)) : new SolidColorBrush(Color.FromRgb(245, 245, 245));
                var textBrush = isDark ? BrushFromHex("#FFE5D6") : (Brush)Brushes.Black;

                // Freeze brushes for performance
                if (windowBg is SolidColorBrush sb1) sb1.Freeze();
                if (controlBg is SolidColorBrush sb2) sb2.Freeze();

                Application.Current.Resources["WindowBackgroundBrush"] = windowBg;
                Application.Current.Resources["ControlBackgroundBrush"] = controlBg;
                Application.Current.Resources["AppTextBrush"] = textBrush;
            }
            catch
            {
                // swallow - theme toggling should not throw
                MessageBox.Show("Error Applying Theme");
            }
        }

        private SolidColorBrush BrushFromHex(string hex)
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        private void ApplyColorToResources(string colorString)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorString);
                var brush = new SolidColorBrush(color);
                brush.Freeze();

                Application.Current.Resources["CurrentColor"] = brush;
                Application.Current.Resources["CurrentColorString"] = colorString;

                OnPropertyChanged(nameof(SelectedColor));
            }
            catch
            {
                // ignore invalid color strings
            }
        }

        [RelayCommand]
        public void ToggleColorMenu()
        {
            IsColorMenuOpen = !IsColorMenuOpen;
        }

        [RelayCommand]
        public void SetCurrentColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return;

            SelectedColor = color;
        }

        [RelayCommand]
        public void ToggleDarkMode()
        {
            IsDarkMode = !IsDarkMode;
        }

        [RelayCommand]
        public void UserClick()
        {
            _mainVM.NavigateTo(new UserViewModel(_mainVM));
        }

        [RelayCommand]
        public void Home()
        {
            _mainVM.NavigateTo(new HomeViewModel(_mainVM));
        }

        [RelayCommand]
        public void SearchBoxFocused()
        {
            SearchText = string.Empty;
            _mainVM.NavigateTo(new SearchViewModel(_mainVM, string.Empty));
        }

        [RelayCommand]
        public void Search()
        {
            _mainVM.NavigateTo(new SearchViewModel(_mainVM, SearchText ?? string.Empty));
        }

        [RelayCommand]
        public void OpenCategories()
        {
            _mainVM.NavigateTo(new HomeViewModel(_mainVM));
        }

        [RelayCommand]
        public void Logout()
        {
            _mainVM.NavigateTo(new HomeViewModel(_mainVM));
        }
    }
}
