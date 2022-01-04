// See https://aka.ms/new-console-template for more information
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Melodi.Networking;
using Recimg;
using Status;

Main();

[STAThread]
static void Main()
{
    Environment.CurrentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
    Console.CursorVisible = false;
    ConsoleRenderer.Init();
    ImgRend.DrawMode = "fill";

    if (!File.Exists("clientID.d"))
    {
        Console.WriteLine("Enter your application's client id");
        File.WriteAllText("clientID.d", Console.ReadLine());
    }

    string token = File.Exists("token.d") ? File.ReadAllText("token.d") : Authenticate();

    while (true)
    {
        try
        {
            WebClient mainClient = new();
            mainClient.Headers[HttpRequestHeader.ContentType] = "application/json";
            mainClient.Headers[HttpRequestHeader.Authorization] = "Bearer " + token;
            string resp = mainClient.DownloadString("https://api.spotify.com/v1/me/player/currently-playing");
            SpotifyRoot root = SpotifyRoot.Parse(resp);
            File.Delete("temp.png");
            if (root != null)
            {
                mainClient.DownloadFile(root.item.album.images.First().url, "temp.png");
                ImgRend.Size = (Console.WindowHeight * 2) - 2;
                ImgRend.Sweep(root);
            }
        }
        catch (Exception e)
        {
            if (e.Message.Contains("401"))
                token = Authenticate();
        }

        Thread.Sleep(1000 / 2);
    }
}

static void OpenBrowser(string url)
{
    try
    {
        Process.Start(url);
    }
    catch
    {
        // hack because of this: https://github.com/dotnet/corefx/issues/10361
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
        else
        {
            throw;
        }
    }
}

static string Authenticate()
{
    HTTPServer authServer = new(typeof(ServerManager), "http://localhost:8888/");
    authServer.Start();

    string clientID = File.ReadAllText("clientID.d");
    string clientScopes = "user-read-playback-state";
    string clientRedirect = "http://localhost:8888/callback";
    string url = $"https://accounts.spotify.com/authorize";
    url += $"?response_type=token";
    url += $"&client_id={Uri.EscapeDataString(clientID)}";
    url += $"&scope={Uri.EscapeDataString(clientScopes)}";
    url += $"&redirect_uri={Uri.EscapeDataString(clientRedirect)}";

    OpenBrowser(url);

    while (!ServerManager.Recieved) { }
    authServer.Stop();
    string token = ServerManager.Token;
    ServerManager.Reset();
    File.WriteAllText("token.d", token);
    return token;
}

public static class ServerManager
{
    public static bool Recieved = false;
    public static string Token;
    [HTTPRequest("/callback", "get")]
    public static byte[] MsgRecieved(HttpListenerContext context)
    {
        if (context.Request.RawUrl == "/callback")
        {
            return Encoding.UTF8.GetBytes("Please wait<script>window.location.href = window.location.href.replace('#', '?')</script>");
        }
        else
        {
            Token = context.Request.QueryString["access_token"];
            Recieved = true;
            return Encoding.UTF8.GetBytes("You may now close the window or tab<script>window.close()</script>");
        }
    }

    public static void Reset()
    {
        Recieved = false;
        Token = null;
    }
}