using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;
using Vibra_DesktopApp.Views;

namespace Vibra_DesktopApp.ViewModels.Pages
{
    public partial class InterestViewModel : ObservableObject
    {
        private readonly IndexViewModel _indexVM;
        private bool _hasLoaded;

        [ObservableProperty] private int _step = 1;
        [ObservableProperty] private bool _isLoading = true;
        [ObservableProperty] private bool _isSubmitting;

        [ObservableProperty] private ObservableCollection<SelectableCategoryItem> _categories = new();
        [ObservableProperty] private ObservableCollection<SelectableHobbyItem> _hobbies = new();

        [ObservableProperty] private int _selectedCategoryCount;
        [ObservableProperty] private int _selectedHobbyCount;

        public bool IsStep1 => Step == 1;
        public bool IsStep2 => Step == 2;

        public string TitleText => IsStep1 ? "Chọn thể loại bạn thích" : "Chọn sở thích của bạn";
        public string SubtitleText => IsStep1 ? "Bạn có thể chọn nhiều thể loại" : "Điều này giúp cá nhân hoá trải nghiệm của bạn";

        public string HobbyHintText
            => SelectedHobbyCount > 0
                ? $"Đã chọn {SelectedHobbyCount} sở thích"
                : "Chọn ít nhất một sở thích để tiếp tục";

        public bool IsPrimaryActionEnabled
            => IsStep1 ? SelectedCategoryCount > 0 : SelectedHobbyCount > 0;

        public string PrimaryActionText => IsStep1 ? "Tiếp tục" : "Hoàn thành";

        public InterestViewModel(IndexViewModel indexVM)
        {
            _indexVM = indexVM ?? throw new ArgumentNullException(nameof(indexVM));
        }

        partial void OnStepChanged(int value)
        {
            if (value != 1 && value != 2)
                Step = 1;

            OnPropertyChanged(nameof(IsStep1));
            OnPropertyChanged(nameof(IsStep2));
            OnPropertyChanged(nameof(TitleText));
            OnPropertyChanged(nameof(SubtitleText));
            OnPropertyChanged(nameof(PrimaryActionText));
            OnPropertyChanged(nameof(IsPrimaryActionEnabled));
        }

        partial void OnSelectedCategoryCountChanged(int value)
        {
            OnPropertyChanged(nameof(IsPrimaryActionEnabled));
        }

        partial void OnSelectedHobbyCountChanged(int value)
        {
            OnPropertyChanged(nameof(IsPrimaryActionEnabled));
            OnPropertyChanged(nameof(HobbyHintText));
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (_hasLoaded)
                return;

            _hasLoaded = true;
            IsLoading = true;

            try
            {
                var api = ApiManager.GetInstance();

                var categoriesTask = api.HttpGetAsync<List<Category>>("category/index");
                var hobbiesTask = api.HttpGetAsync<List<HobbyDto>>("home/list-hobby");

                await Task.WhenAll(categoriesTask, hobbiesTask).ConfigureAwait(false);

                var categoryList = (await categoriesTask.ConfigureAwait(false)) ?? [];
                var hobbyList = (await hobbiesTask.ConfigureAwait(false)) ?? [];

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Categories = new ObservableCollection<SelectableCategoryItem>(
                        categoryList.Select(c => new SelectableCategoryItem(c)));

                    Hobbies = new ObservableCollection<SelectableHobbyItem>(
                        hobbyList.Select(h => new SelectableHobbyItem(h.id, h.name)));
                });
                RecalculateSelectedCounts();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void NextStep()
        {
            if (IsStep1 && SelectedCategoryCount > 0)
                Step = 2;
        }

        [RelayCommand]
        private void PrevStep()
        {
            Step = 1;
        }

        [RelayCommand]
        private async Task PrimaryActionAsync()
        {
            if (IsStep1)
            {
                NextStep();
                return;
            }

            await SubmitAllAsync().ConfigureAwait(false);
        }

        [RelayCommand]
        private void ToggleCategory(SelectableCategoryItem item)
        {
            if (item is null)
                return;

            item.IsSelected = !item.IsSelected;
            RecalculateSelectedCounts();
        }

        [RelayCommand]
        private void ToggleHobby(SelectableHobbyItem item)
        {
            if (item is null)
                return;

            item.IsSelected = !item.IsSelected;
            RecalculateSelectedCounts();
        }

        private async Task SubmitAllAsync()
        {
            if (SelectedHobbyCount <= 0)
                return;

            IsSubmitting = true;
            try
            {
                var cateIds = string.Join(",", Categories.Where(x => x.IsSelected).Select(x => x.Category.id).Where(x => x is not null));
                var hobbyIds = string.Join(",", Hobbies.Where(x => x.IsSelected).Select(x => x.Id).Where(x => x is not null));

                var url = $"home/save-interested?category_id={Uri.EscapeDataString(cateIds)}&hobby_id={Uri.EscapeDataString(hobbyIds)}";

                await ApiManager.GetInstance().HttpGetNoDataAsync(url).ConfigureAwait(false);

                //MessageBox.Show("Cài đặt sở thích thành công!");

                await Application.Current.Dispatcher.InvokeAsync(OpenMainWindow);
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() => MessageBox.Show(ex.Message));
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        private void RecalculateSelectedCounts()
        {
            SelectedCategoryCount = Categories.Count(x => x.IsSelected);
            SelectedHobbyCount = Hobbies.Count(x => x.IsSelected);
        }

        private void OpenMainWindow()
        {
            // Important: show the new window BEFORE closing IndexWindow.
            // If IndexWindow is the last open window, closing it first will shut down the app.
            var mainWindow = new MainWindow();
            mainWindow.Show();

            Application.Current.MainWindow = mainWindow;

            var indexWindow = Application.Current.Windows.OfType<IndexWindow>().FirstOrDefault();
            indexWindow?.Close();
        }

        public partial class SelectableCategoryItem : ObservableObject
        {
            public Category Category { get; }

            [ObservableProperty] private bool _isSelected;

            public SelectableCategoryItem(Category category)
            {
                Category = category;
            }
        }

        public partial class SelectableHobbyItem : ObservableObject
        {
            public int? Id { get; }
            public string? Name { get; }

            [ObservableProperty] private bool _isSelected;

            public SelectableHobbyItem(int? id, string? name)
            {
                Id = id;
                Name = name;
            }
        }

        private sealed class HobbyDto
        {
            public int? id { get; set; }
            public string? name { get; set; }
        }
    }
}
