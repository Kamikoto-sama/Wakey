using System;
using System.Diagnostics;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Api.Contracts;
using nanoFramework.SignalR.Client;
using Proxy.Utils;

namespace Proxy.Services;

public sealed class ApiConnection : IDisposable
{
    private readonly HubConnection hubConnection;
    private HubConnectionState State => hubConnection.State;
    private readonly Thread reconnectionThread;
    private readonly ManualResetEvent reconnectionLock;
    private readonly CancellationTokenSource cts;

    public ApiConnection(Settings.Settings settings)
    {
        var connectionOptions = new HubConnectionOptions
        {
            Reconnect = true,
            SslVerification = SslVerification.NoVerification,
            Certificate = new X509Certificate(Ssl.Certificate)
        };
        var url = UrlBuilder.BuildStatusUrl(settings.ApiUrl, settings.ApiKey, ClientType.Proxy);
        var connection = new HubConnection(url, options: connectionOptions);
        connection.Closed += OnClose;
        hubConnection = connection;

        cts = new CancellationTokenSource();
        reconnectionLock = new ManualResetEvent(false);
        reconnectionThread = new Thread(Reconnect);
    }

    public void Start()
    {
        if (State != HubConnectionState.Disconnected)
            return;
        if (reconnectionThread.ThreadState == ThreadState.Unstarted)
            reconnectionThread.Start();

        Debug.WriteLine($"Connecting to: {hubConnection.Uri}");
        hubConnection.Start();
        Debug.WriteLine($"Connected state: {State}");
    }

    public void Send(string methodName, params object[] args)
    {
        if (State != HubConnectionState.Connected)
            return;
        hubConnection.SendCore(methodName, args);
    }

    public void On(string methodName, Type[] parameterTypes, HubConnection.OnInvokeHandler handler)
    {
        hubConnection.On(methodName, parameterTypes, handler);
    }

    private void OnClose(object sender, SignalrEventMessageArgs args)
    {
        Debug.WriteLine($"OnClose: {args.Message}");
        reconnectionLock.Set();
    }

    private void Reconnect()
    {
        while (!cts.IsCancellationRequested)
        {
            reconnectionLock.WaitOne();
            if (cts.IsCancellationRequested)
                break;

            try
            {
                if (State != HubConnectionState.Disconnected)
                {
                    reconnectionLock.Reset();
                    continue;
                }

                Debug.WriteLine("Reconnecting...");
                hubConnection.Stop();
                Start();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failed to reconnect: {e.Format()}");
            }

            Thread.Sleep(5000);
        }
    }

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
        hubConnection.Stop();
        Debug.WriteLine($"{nameof(ApiConnection)} disposed");
    }
}