using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;
using Vibra_DesktopApp.Views.Modals;

namespace Vibra_DesktopApp.ViewModels.Pages
{
    public partial class UserViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        public User? CurrentUser => ApiManager.GetInstance().GetCurrentUser();

        [ObservableProperty]
        private bool isEditProfileOpen;

        [ObservableProperty]
        private bool isUploadSongOpen;

        [ObservableProperty]
        private bool isEditAlbumOpen;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasMyAlbums))]
        private List<Album> myAlbums = [];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasMyPlaylists))]
        private List<Album> myPlaylists = [];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasFollowArtists))]
        private List<User> followArtists = [];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasMySongs))]
        private List<Song> mySongs = [];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasBills))]
        private List<Payment> bills = [];

        [ObservableProperty] private bool isMenuOpen;

        public bool HasMyAlbums => MyAlbums is { Count: > 0 };
        public bool HasMyPlaylists => MyPlaylists is { Count: > 0 };
        public bool HasFollowArtists => FollowArtists is { Count: > 0 };
        public bool HasMySongs => MySongs is { Count: > 0 };
        public bool HasBills => Bills is { Count: > 0 };

        // Edit profile fields
        [ObservableProperty] private string? profileName;
        [ObservableProperty] private string? profileDescription;
        [ObservableProperty] private string? profileGender;
        [ObservableProperty] private DateTime? profileBirth;
        [ObservableProperty] private string? profileAvatarPreviewPath;
        private string? _profileAvatarFilePath;

        // Upload song fields
        [ObservableProperty] private List<Category> allCategories = [];
        [ObservableProperty] private bool isCategoryDropdownOpen;
        [ObservableProperty] private int? selectedCategoryId;
        [ObservableProperty] private string? selectedCategoryName;

        [ObservableProperty] private bool isAlbumDropdownOpen;
        [ObservableProperty] private int? selectedAlbumId;
        [ObservableProperty] private string? selectedAlbumName;
        [ObservableProperty] private string? songDescription;
        [ObservableProperty] private int? songPrice;
        [ObservableProperty] private string? songThumbnailPreviewPath;
        private string? _songThumbnailFilePath;
        [ObservableProperty] private string? selectedSongFileName;
        private string? _songFilePath;
        [ObservableProperty] private string? selectedLyricFileName;
        private string? _lyricFilePath;

        // Edit album fields
        [ObservableProperty] private Album? albumEditData;
        [ObservableProperty] private string? albumName;
        [ObservableProperty] private string? albumDescription;
        [ObservableProperty] private int? albumPrice;
        [ObservableProperty] private string? albumThumbnailPreviewPath;
        private string? _albumThumbnailFilePath;

        public UserViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            _ = RefreshAsync();
        }

        partial void OnIsEditProfileOpenChanged(bool value)
        {
            if (value)
                LoadProfileDraftFromCurrentUser();
        }

        partial void OnIsUploadSongOpenChanged(bool value)
        {
            if (value)
                _ = EnsureUploadSongDataAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            try
            {
                // User-specific data (match Vue fetchUserData)
                var albumsTask = ApiManager.GetInstance().HttpGetAsync<List<Album>>("profile/list-album");
                var mySongsTask = ApiManager.GetInstance().HttpGetAsync<List<Song>>("profile/list-song");

                // Other sections
                var playlistsTask = ApiManager.GetInstance().HttpGetAsync<List<Album>>("library/list-playlist?type=2");
                var followArtistsTask = ApiManager.GetInstance().HttpGetAsync<List<User>>("library/list-artist");

                // these are from your Vue. If your API path differs, change here.
                var billsTask = ApiManager.GetInstance().HttpGetAsync<List<Payment>>("profile/payment-history");

                await Task.WhenAll(albumsTask, playlistsTask, followArtistsTask, billsTask, mySongsTask);

                MyAlbums = albumsTask.Result ?? [];
                MyPlaylists = playlistsTask.Result ?? [];
                FollowArtists = followArtistsTask.Result ?? [];
                Bills = billsTask.Result ?? [];
                MySongs = mySongsTask.Result ?? [];

                if (IsUploadSongOpen)
                    await EnsureUploadSongDataAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(ex.Message));
            }
        }

        [RelayCommand]
        private async Task PlaySongAsync(Song song)
        {
            if (song == null) return;
            await SongManager.GetInstace().PlayOrPauseThisSongAsync(song);
        }

        [RelayCommand]
        private void EnqueueSong(Song song)
        {
            if (song == null) return;
            SongManager.GetInstace().Enqueue(song);
        }

        [RelayCommand]
        private void OpenAlbum(Album album)
        {
            if (album == null) return;
            _mainVM.NavigateTo(new AlbumViewModel(_mainVM, album), NavigationItem.Album);
        }

        [RelayCommand]
        private void OpenArtist(User artist)
        {
            if (artist == null) return;
            _mainVM.NavigateTo(new ArtistViewModel(_mainVM, artist), NavigationItem.Artist);
        }

        [RelayCommand]
        private async Task CreateAlbumAsync()
        {
            try
            {
                await ApiManager.GetInstance().HttpGetNoDataAsync("profile/create-album");
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(ex.Message));
            }
        }

        [RelayCommand]
        private void EditProfile()
        {
            IsMenuOpen = false;
            LoadProfileDraftFromCurrentUser();

            var w = new EditProfileWindow
            {
                DataContext = this,
                Owner = Application.Current?.MainWindow,
            };

            w.ShowDialog();
        }

        [RelayCommand]
        private void UploadSong()
        {
            IsMenuOpen = false;

            if (MyAlbums is null || MyAlbums.Count == 0)
            {
                MessageBox.Show("Vui lòng tạo ít nhất một Album để chứa bài hát!");
                return;
            }

            _ = EnsureUploadSongDataAsync();

            var w = new UploadSongWindow
            {
                DataContext = this,
                Owner = Application.Current?.MainWindow,
            };

            w.ShowDialog();
        }

        [RelayCommand]
        private void EditAlbum(Album album)
        {
            if (album == null) return;

            AlbumEditData = album;
            AlbumName = album.name;
            AlbumDescription = album.description;
            AlbumPrice = album.price;
            AlbumThumbnailPreviewPath = album.thumbnail_path;
            _albumThumbnailFilePath = null;

            var w = new EditAlbumWindow
            {
                DataContext = this,
                Owner = Application.Current?.MainWindow,
            };

            w.ShowDialog();
        }

        [RelayCommand]
        private void CloseWindow(Window window)
        {
            window?.Close();
        }

        private void LoadProfileDraftFromCurrentUser()
        {
            var u = CurrentUser;
            ProfileName = u?.name;
            ProfileDescription = u?.description;
            ProfileGender = string.IsNullOrWhiteSpace(u?.gender) ? "Giới tính khác" : u!.gender;
            ProfileBirth = u?.birth ?? new DateTime(2000, 10, 10);
            ProfileAvatarPreviewPath = u?.avatar_path;
            _profileAvatarFilePath = null;
        }

        private async Task EnsureUploadSongDataAsync()
        {
            try
            {
                if (AllCategories.Count == 0)
                {
                    var categories = await ApiManager.GetInstance().HttpGetAsync<List<Category>>("category/index").ConfigureAwait(false);
                    AllCategories = categories ?? [];
                }

                // default selections
                if (SelectedAlbumId is null && MyAlbums.Count > 0)
                {
                    var first = MyAlbums[0];
                    SelectedAlbumId = first.id;
                    SelectedAlbumName = first.name;
                }

                if (SelectedCategoryId is null && AllCategories.Count > 0)
                {
                    var first = AllCategories[0];
                    SelectedCategoryId = first.id;
                    SelectedCategoryName = first.name;
                }
            }
            catch
            {
                // keep UI usable even if categories endpoint is unavailable
            }
        }

        [RelayCommand]
        private void ToggleCategoryDropdown()
        {
            IsCategoryDropdownOpen = !IsCategoryDropdownOpen;
            if (IsCategoryDropdownOpen)
                IsAlbumDropdownOpen = false;
        }

        [RelayCommand]
        private void ToggleAlbumDropdown()
        {
            IsAlbumDropdownOpen = !IsAlbumDropdownOpen;
            if (IsAlbumDropdownOpen)
                IsCategoryDropdownOpen = false;
        }

        [RelayCommand]
        private void ChooseProfileAvatar()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.webp|All Files|*.*",
                Multiselect = false
            };

            if (dlg.ShowDialog() != true)
                return;

            _profileAvatarFilePath = dlg.FileName;
            ProfileAvatarPreviewPath = dlg.FileName;
        }

        [RelayCommand]
        private async Task SaveProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(ProfileName) || string.IsNullOrWhiteSpace(ProfileDescription) || string.IsNullOrWhiteSpace(ProfileGender))
            {
                MessageBox.Show("Vui lòng điền đủ thông tin!");
                return;
            }

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(ProfileName), "name");
            form.Add(new StringContent(ProfileDescription), "description");
            form.Add(new StringContent(ProfileGender), "gender");
            form.Add(new StringContent((ProfileBirth ?? new DateTime(2000, 10, 10)).ToString("yyyy/MM/dd", CultureInfo.InvariantCulture)), "birth");

            if (!string.IsNullOrWhiteSpace(_profileAvatarFilePath) && File.Exists(_profileAvatarFilePath))
            {
                var fs = File.OpenRead(_profileAvatarFilePath);
                var sc = new StreamContent(fs);
                sc.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                form.Add(sc, "avatar", Path.GetFileName(_profileAvatarFilePath));
            }

            await ApiManager.GetInstance().HttpPostFormAsync<object>("profile/update", form).ConfigureAwait(false);

            var updated = await ApiManager.GetInstance().HttpGetAsync<User>("profile/show").ConfigureAwait(false);
            ApiManager.GetInstance().SetCurrentUser(updated);

            OnPropertyChanged(nameof(CurrentUser));
            if (Application.Current?.Dispatcher is not null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Application.Current.Windows.Count > 0)
                    {
                        foreach (Window w in Application.Current.Windows)
                        {
                            if (w is Views.Modals.EditProfileWindow)
                            {
                                w.Close();
                                break;
                            }
                        }
                    }
                });
            }
        }

        [RelayCommand]
        private void ChooseSongFile()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Audio Files|*.mp3;*.wav;*.flac;*.m4a;*.aac;*.ogg|All Files|*.*",
                Multiselect = false
            };

            if (dlg.ShowDialog() != true)
                return;

            _songFilePath = dlg.FileName;
            SelectedSongFileName = Path.GetFileName(dlg.FileName);
        }

        [RelayCommand]
        private void ChooseLyricFile()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Text Files|*.txt|All Files|*.*",
                Multiselect = false
            };

            if (dlg.ShowDialog() != true)
                return;

            _lyricFilePath = dlg.FileName;
            SelectedLyricFileName = Path.GetFileName(dlg.FileName);
        }

        [RelayCommand]
        private void ChooseSongThumbnail()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.webp|All Files|*.*",
                Multiselect = false
            };

            if (dlg.ShowDialog() != true)
                return;

            _songThumbnailFilePath = dlg.FileName;
            SongThumbnailPreviewPath = dlg.FileName;
        }

        [RelayCommand]
        private void SelectCategory(Category cate)
        {
            if (cate is null) return;
            SelectedCategoryId = cate.id;
            SelectedCategoryName = cate.name;
            IsCategoryDropdownOpen = false;
        }

        [RelayCommand]
        private void SelectAlbum(Album album)
        {
            if (album is null) return;
            SelectedAlbumId = album.id;
            SelectedAlbumName = album.name;
            IsAlbumDropdownOpen = false;
        }

        [RelayCommand]
        private async Task UploadSongConfirmAsync()
        {
            IsCategoryDropdownOpen = false;
            IsAlbumDropdownOpen = false;

            if (string.IsNullOrWhiteSpace(_songThumbnailFilePath) || string.IsNullOrWhiteSpace(_lyricFilePath) || string.IsNullOrWhiteSpace(_songFilePath)
                || SelectedAlbumId is null || SelectedCategoryId is null || SongPrice is null)
            {
                MessageBox.Show("Vui lòng điền đủ thông tin!");
                return;
            }

            if (SongPrice < 2000)
            {
                MessageBox.Show("Giá bài hát tối thiểu là 2000đ!");
                return;
            }

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(SelectedCategoryId.Value.ToString(CultureInfo.InvariantCulture)), "category-id");
            form.Add(new StringContent(SongDescription ?? string.Empty), "description");
            form.Add(new StringContent(SelectedAlbumId.Value.ToString(CultureInfo.InvariantCulture)), "playlist-id");
            form.Add(new StringContent(SongPrice.Value.ToString(CultureInfo.InvariantCulture)), "price");

            form.Add(CreateFileContent(_songFilePath, "audio/*"), "song", Path.GetFileName(_songFilePath));
            form.Add(CreateFileContent(_lyricFilePath, "text/plain"), "lyric", Path.GetFileName(_lyricFilePath));
            form.Add(CreateFileContent(_songThumbnailFilePath, "image/*"), "thumbnail", Path.GetFileName(_songThumbnailFilePath));

            await ApiManager.GetInstance().HttpPostFormAsync<object>("profile/upload-song", form).ConfigureAwait(false);
            await RefreshAsync().ConfigureAwait(false);

            // reset state
            SongDescription = null;
            SongPrice = null;
            SongThumbnailPreviewPath = null;
            _songThumbnailFilePath = null;
            SelectedSongFileName = null;
            _songFilePath = null;
            SelectedLyricFileName = null;
            _lyricFilePath = null;

            if (Application.Current?.Dispatcher is not null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (Window w in Application.Current.Windows)
                    {
                        if (w is Views.Modals.UploadSongWindow)
                        {
                            w.Close();
                            break;
                        }
                    }
                });
            }
        }

        private static HttpContent CreateFileContent(string filePath, string contentType)
        {
            var fs = File.OpenRead(filePath);
            var sc = new StreamContent(fs);
            sc.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return sc;
        }

        [RelayCommand]
        private void ChooseAlbumThumbnail()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.webp|All Files|*.*",
                Multiselect = false
            };

            if (dlg.ShowDialog() != true)
                return;

            _albumThumbnailFilePath = dlg.FileName;
            AlbumThumbnailPreviewPath = dlg.FileName;
        }

        [RelayCommand]
        private async Task SaveAlbumAsync()
        {
            if (AlbumEditData?.id is null)
                return;

            if (string.IsNullOrWhiteSpace(AlbumName) || string.IsNullOrWhiteSpace(AlbumDescription) || AlbumPrice is null)
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin!");
                return;
            }

            if (AlbumPrice < 4000)
            {
                MessageBox.Show("Giá Album tối thiểu là 4000đ!");
                return;
            }

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(AlbumName), "name");
            form.Add(new StringContent(AlbumDescription), "description");
            form.Add(new StringContent(AlbumPrice.Value.ToString(CultureInfo.InvariantCulture)), "price");

            if (!string.IsNullOrWhiteSpace(_albumThumbnailFilePath) && File.Exists(_albumThumbnailFilePath))
            {
                form.Add(CreateFileContent(_albumThumbnailFilePath, "image/*"), "thumbnail", Path.GetFileName(_albumThumbnailFilePath));
            }

            await ApiManager.GetInstance().HttpPostFormAsync<object>($"profile/update-album/{AlbumEditData.id}", form).ConfigureAwait(false);
            await RefreshAsync().ConfigureAwait(false);

            if (Application.Current?.Dispatcher is not null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (Window w in Application.Current.Windows)
                    {
                        if (w is Views.Modals.EditAlbumWindow)
                        {
                            w.Close();
                            break;
                        }
                    }
                });
            }
        }

        [RelayCommand]
        private void ToggleMenu()
        {
            IsMenuOpen = !IsMenuOpen;
        }
    }
}
