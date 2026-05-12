using Duende.IdentityModel.OidcClient.Browser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace Vibra_DesktopApp.Services
{
    public class SystemBrowser : IBrowser
    {
        private readonly int _port;

        public SystemBrowser(int port)
        {
            _port = port;
        }

        public async Task<BrowserResult> InvokeAsync(
            BrowserOptions options,
            CancellationToken cancellationToken = default)
        {
            using var listener = new HttpListener();

            listener.Prefixes.Add($"http://127.0.0.1:{_port}/");
            listener.Start();

            Process.Start(new ProcessStartInfo
            {
                FileName = options.StartUrl,
                UseShellExecute = true
            });

            var context = await listener.GetContextAsync();

            var response = context.Response;

            const string responseString =
                "<html><body>Bạn có thể đóng tab này.</body></html>";

            var buffer = Encoding.UTF8.GetBytes(responseString);

            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer);

            response.OutputStream.Close();

            var url = context.Request.Url!.ToString();

            return new BrowserResult
            {
                Response = url,
                ResultType = BrowserResultType.Success
            };
        }
    }
}
