using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Vibra_DesktopApp.Singleton;

namespace Vibra_DesktopApp.Behaviors
{
    public static class LazyImage
    {
        private static readonly HttpClient _httpClient = new();
        private static readonly ConcurrentDictionary<string, Task<ImageSource?>> _cache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly SemaphoreSlim _downloadGate = new(initialCount: 4, maxCount: 4);

        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
            "Source",
            typeof(string),
            typeof(LazyImage),
            new PropertyMetadata(null, OnSourceChanged));

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.RegisterAttached(
            "Placeholder",
            typeof(ImageSource),
            typeof(LazyImage),
            new PropertyMetadata(null));

        public static readonly DependencyProperty DecodePixelWidthProperty = DependencyProperty.RegisterAttached(
            "DecodePixelWidth",
            typeof(int),
            typeof(LazyImage),
            new PropertyMetadata(0));

        public static readonly DependencyProperty DecodePixelHeightProperty = DependencyProperty.RegisterAttached(
            "DecodePixelHeight",
            typeof(int),
            typeof(LazyImage),
            new PropertyMetadata(0));

        private static readonly DependencyProperty RequestIdProperty = DependencyProperty.RegisterAttached(
            "RequestId",
            typeof(long),
            typeof(LazyImage),
            new PropertyMetadata(0L));

        public static string? GetSource(DependencyObject element) => (string?)element.GetValue(SourceProperty);
        public static void SetSource(DependencyObject element, string? value) => element.SetValue(SourceProperty, value);

        public static ImageSource? GetPlaceholder(DependencyObject element) => (ImageSource?)element.GetValue(PlaceholderProperty);
        public static void SetPlaceholder(DependencyObject element, ImageSource? value) => element.SetValue(PlaceholderProperty, value);

        public static int GetDecodePixelWidth(DependencyObject element) => (int)element.GetValue(DecodePixelWidthProperty);
        public static void SetDecodePixelWidth(DependencyObject element, int value) => element.SetValue(DecodePixelWidthProperty, value);

        public static int GetDecodePixelHeight(DependencyObject element) => (int)element.GetValue(DecodePixelHeightProperty);
        public static void SetDecodePixelHeight(DependencyObject element, int value) => element.SetValue(DecodePixelHeightProperty, value);

        private static long GetRequestId(DependencyObject element) => (long)element.GetValue(RequestIdProperty);
        private static void SetRequestId(DependencyObject element, long value) => element.SetValue(RequestIdProperty, value);

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Image image)
                return;

            var requestId = GetRequestId(image) + 1;
            SetRequestId(image, requestId);

            var placeholder = GetPlaceholder(image);
            if (placeholder is not null)
                image.Source = placeholder;

            if (!image.IsLoaded)
            {
                RoutedEventHandler? loaded = null;
                loaded = (_, _) =>
                {
                    image.Loaded -= loaded;
                    QueueLoad(image, requestId);
                };
                image.Loaded += loaded;
                return;
            }

            QueueLoad(image, requestId);
        }

        private static void QueueLoad(Image image, long requestId)
        {
            // Let the UI finish rendering first.
            image.Dispatcher.BeginInvoke(async () =>
            {
                if (GetRequestId(image) != requestId)
                    return;

                var src = GetSource(image);
                if (string.IsNullOrWhiteSpace(src))
                    return;

                var decodeW = GetDecodePixelWidth(image);
                var decodeH = GetDecodePixelHeight(image);

                try
                {
                    var imageSource = await GetOrLoadAsync(src, decodeW, decodeH).ConfigureAwait(false);
                    if (imageSource is null)
                        return;

                    await image.Dispatcher.InvokeAsync(() =>
                    {
                        if (GetRequestId(image) != requestId)
                            return;

                        image.Source = imageSource;
                    }, DispatcherPriority.Render);
                }
                catch
                {
                    // Ignore image failures (keep placeholder).
                }

            }, DispatcherPriority.Background);
        }

        private static async Task<ImageSource?> GetOrLoadAsync(string src, int decodePixelWidth, int decodePixelHeight)
        {
            var absolute = ApiManager.GetInstance().ToAbsoluteUrl(src) ?? src;

            if (!Uri.TryCreate(absolute, UriKind.RelativeOrAbsolute, out var uri))
                return null;

            // Local pack/resource/file URIs: let WPF handle without HTTP.
            if (!uri.IsAbsoluteUri || uri.Scheme.Equals(Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals("pack", StringComparison.OrdinalIgnoreCase))
            {
                return LoadBitmapFromUri(uri, decodePixelWidth, decodePixelHeight);
            }

            if (uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                var key = uri.AbsoluteUri;
                return await _cache.GetOrAdd(key, _ => LoadHttpBitmapAsync(uri, decodePixelWidth, decodePixelHeight)).ConfigureAwait(false);
            }

            return LoadBitmapFromUri(uri, decodePixelWidth, decodePixelHeight);
        }

        private static ImageSource? LoadBitmapFromUri(Uri uri, int decodePixelWidth, int decodePixelHeight)
        {
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;

                if (decodePixelWidth > 0)
                    bmp.DecodePixelWidth = decodePixelWidth;
                if (decodePixelHeight > 0)
                    bmp.DecodePixelHeight = decodePixelHeight;

                bmp.UriSource = uri;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        private static async Task<ImageSource?> LoadHttpBitmapAsync(Uri uri, int decodePixelWidth, int decodePixelHeight)
        {
            await _downloadGate.WaitAsync().ConfigureAwait(false);
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(uri).ConfigureAwait(false);

                using var ms = new MemoryStream(bytes);
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;

                if (decodePixelWidth > 0)
                    bmp.DecodePixelWidth = decodePixelWidth;
                if (decodePixelHeight > 0)
                    bmp.DecodePixelHeight = decodePixelHeight;

                bmp.StreamSource = ms;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return null;
            }
            finally
            {
                _downloadGate.Release();
            }
        }
    }
}
