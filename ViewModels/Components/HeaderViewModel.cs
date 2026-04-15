using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.ViewModels.Pages;

namespace Vibra_DesktopApp.ViewModels.Components
{
    public partial class HeaderViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        [ObservableProperty] private string _searchText;
        [ObservableProperty] private bool _isColorMenuOpen;
        public IReadOnlyList<string> ColorList { get; }
        [ObservableProperty] private string _selectedColor;
        [ObservableProperty] private bool _isDarkMode;

        // Navigation state properties
        public NavigationItem CurrentNavigationItem => _mainVM.CurrentNavigationItem;

        public HeaderViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));

            ColorList = new List<string>
            {
                "#BC4D15", "#645283", "#a8bfc9", "#CD5C5C", "#a3b18a",
                "#9e9fa5", "#926F4F", "#FEA7A0", "#c3a995", "#44B78B",
            };

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

            if (Application.Current?.Resources["IsDarkMode"] is bool dark)
            {
                IsDarkMode = dark;
            }
            else
            {
                IsDarkMode = true;
                ApplyTheme(IsDarkMode);
            }

            SearchText = string.Empty;
            IsColorMenuOpen = false;

            // Subscribe to navigation changes
            _mainVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.CurrentNavigationItem))
                {
                    OnPropertyChanged(nameof(CurrentNavigationItem));
                }
            };
        }

        partial void OnSelectedColorChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            ApplyColorToResources(value);
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

                var windowBg = isDark ? (Brush)new SolidColorBrush(Color.FromRgb(20, 20, 24)) : new SolidColorBrush(Color.FromRgb(236, 234, 234));
                var controlBg = isDark ? (Brush)new SolidColorBrush(Color.FromRgb(31, 31, 31)) : new SolidColorBrush(Color.FromRgb(250, 250, 250));
                var textBrush = isDark ? (Brush)new SolidColorBrush(Color.FromRgb(247, 244, 239)) : new SolidColorBrush(Color.FromRgb(32, 32, 34));

                if (windowBg is SolidColorBrush sb1) sb1.Freeze();
                if (controlBg is SolidColorBrush sb2) sb2.Freeze();

                Application.Current.Resources["WindowBackgroundBrush"] = windowBg;
                Application.Current.Resources["ControlBackgroundBrush"] = controlBg;
                Application.Current.Resources["AppTextBrush"] = textBrush;
            }
            catch
            {
                // swallow - theme toggling should not throw
            }
        }

        private void ApplyColorToResources(string colorString)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorString);
                var brush = new SolidColorBrush(color);
                brush.Freeze();

                var darkBrush = new SolidColorBrush(Darken(color, 0.5));
                darkBrush.Freeze();

                var brightBrush = new SolidColorBrush(Brighten(color, 0.40));
                brightBrush.Freeze();

                Application.Current.Resources["CurrentColor"] = brush;
                Application.Current.Resources["CurrentColorString"] = colorString;
                Application.Current.Resources["DarkCurrentColor"] = darkBrush;
                Application.Current.Resources["BrightCurrentColor"] = brightBrush;

                OnPropertyChanged(nameof(SelectedColor));
            }
            catch
            {
                // ignore invalid color strings
            }
        }

        private static Color Darken(Color color, double factor)
        {
            factor = Math.Clamp(factor, 0d, 1d);
            return Color.FromArgb(
                color.A,
                (byte)Math.Clamp((int)Math.Round(color.R * factor), 0, 255),
                (byte)Math.Clamp((int)Math.Round(color.G * factor), 0, 255),
                (byte)Math.Clamp((int)Math.Round(color.B * factor), 0, 255));
        }

        private static Color Brighten(Color color, double amount)
        {
            amount = Math.Clamp(amount, 0d, 1d);
            return Color.FromArgb(
                color.A,
                (byte)Math.Clamp((int)Math.Round(color.R + (255 - color.R) * amount), 0, 255),
                (byte)Math.Clamp((int)Math.Round(color.G + (255 - color.G) * amount), 0, 255),
                (byte)Math.Clamp((int)Math.Round(color.B + (255 - color.B) * amount), 0, 255));
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
            _mainVM.NavigateTo(new UserViewModel(_mainVM), NavigationItem.User);
        }

        [RelayCommand]
        public void Home()
        {
            _mainVM.NavigateTo(new HomeViewModel(_mainVM), NavigationItem.Home);
        }

        [RelayCommand]
        public void SearchBoxFocused()
        {
            SearchText = string.Empty;
            _mainVM.NavigateTo(new SearchViewModel(_mainVM, string.Empty), NavigationItem.Search);
        }

        [RelayCommand]
        public void Search()
        {
            _mainVM.NavigateTo(new SearchViewModel(_mainVM, SearchText ?? string.Empty), NavigationItem.Search);
        }

        [RelayCommand]
        public void OpenCategories()
        {
            _mainVM.NavigateTo(new CategoriesViewModel(_mainVM), NavigationItem.Categories);
        }

        [RelayCommand]
        public void Logout()
        {
            _mainVM.NavigateTo(new HomeViewModel(_mainVM), NavigationItem.Home);
        }

        [RelayCommand]
        private void MinimizeWindow()
        {
            var window = Application.Current?.MainWindow;
            if (window is null)
                return;

            window.WindowState = WindowState.Minimized;
        }

        [RelayCommand]
        private void ToggleMaximizeWindow()
        {
            var window = Application.Current?.MainWindow;
            if (window is null)
                return;

            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        [RelayCommand]
        private void CloseWindow()
        {
            Application.Current?.Shutdown();
        }
    }
}
