using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NVespa.NSocket;
using UnityEngine;

public static class SprkWebSocketTransport
{
	public sealed class Bridge : IDisposable
	{
		private readonly ClientWebSocket websocket = new ClientWebSocket();

		private readonly CancellationTokenSource stop = new CancellationTokenSource();

		private TcpListener listener;

		private TcpClient client;

		private int disposed;

		public int Port { get; private set; }

		public Task Completion { get; private set; }

		public async Task Open(Uri uri, TimeSpan timeout)
		{
			websocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20.0);
			using (CancellationTokenSource opening = CancellationTokenSource.CreateLinkedTokenSource(stop.Token))
			{
				opening.CancelAfter(timeout);
				await websocket.ConnectAsync(uri, opening.Token).ConfigureAwait(continueOnCapturedContext: false);
			}
			listener = new TcpListener(IPAddress.Loopback, 0);
			listener.Start(1);
			Port = ((IPEndPoint)listener.LocalEndpoint).Port;
			Completion = Relay(timeout);
		}

		private async Task Relay(TimeSpan timeout)
		{
			_ = 4;
			try
			{
				Task<TcpClient> accept = listener.AcceptTcpClientAsync();
				if (await Task.WhenAny(accept, Task.Delay(timeout, stop.Token)).ConfigureAwait(continueOnCapturedContext: false) != accept)
				{
					listener.Stop();
					try
					{
						(await accept.ConfigureAwait(continueOnCapturedContext: false)).Close();
					}
					catch
					{
					}
					throw new TimeoutException("Native connector did not attach to WebSocket adapter");
				}
				client = await accept.ConfigureAwait(continueOnCapturedContext: false);
				listener.Stop();
				client.NoDelay = true;
				NetworkStream stream = client.GetStream();
				Task upload = Upload(stream);
				Task download = Download(stream);
				await Task.WhenAny(upload, download).ConfigureAwait(continueOnCapturedContext: false);
				Dispose();
				try
				{
					await Task.WhenAll(upload, download).ConfigureAwait(continueOnCapturedContext: false);
				}
				catch
				{
				}
			}
			catch
			{
				Dispose();
			}
			finally
			{
				Dispose();
			}
		}

		private async Task Upload(NetworkStream stream)
		{
			byte[] buffer = new byte[32768];
			while (!stop.IsCancellationRequested)
			{
				int num = await stream.ReadAsync(buffer, 0, buffer.Length, stop.Token).ConfigureAwait(continueOnCapturedContext: false);
				if (num == 0)
				{
					break;
				}
				await websocket.SendAsync(new ArraySegment<byte>(buffer, 0, num), WebSocketMessageType.Binary, endOfMessage: true, stop.Token).ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		private async Task Download(NetworkStream stream)
		{
			byte[] buffer = new byte[32768];
			while (!stop.IsCancellationRequested)
			{
				WebSocketReceiveResult webSocketReceiveResult = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), stop.Token).ConfigureAwait(continueOnCapturedContext: false);
				if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
				{
					break;
				}
				await stream.WriteAsync(buffer, 0, webSocketReceiveResult.Count, stop.Token).ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		public void Dispose()
		{
			if (Interlocked.Exchange(ref disposed, 1) == 0)
			{
				stop.Cancel();
				if (listener != null)
				{
					listener.Stop();
				}
				if (client != null)
				{
					client.Close();
				}
				websocket.Abort();
				websocket.Dispose();
			}
		}
	}

	private static readonly MethodInfo NativeConnect = typeof(SocketConnector).GetMethod("SprkConnectTcpCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo NativeSession = typeof(SocketConnector).GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo Reserve = typeof(SocketConnector).GetMethod("Reserve", BindingFlags.Instance | BindingFlags.NonPublic);

	public static void SessionClosed(SocketConnector connector, object session)
	{
		if (NativeSession.GetValue(connector) != session)
		{
			return;
		}
		Reserve.Invoke(connector, new object[2]
		{
			(Action)delegate
			{
				if (NativeSession.GetValue(connector) == session)
				{
					connector.Close();
				}
			},
			0
		});
	}

	public static IEnumerator Connect(SocketConnector connector, string address, int port, Action<bool> callback, float timeout)
	{
		if (!Uri.TryCreate(address, UriKind.Absolute, out var result) || (result.Scheme != "ws" && result.Scheme != "wss"))
		{
			return (IEnumerator)NativeConnect.Invoke(connector, new object[4] { address, port, callback, timeout });
		}
		return ConnectWebSocket(connector, result, callback, timeout);
	}

	private static IEnumerator ConnectWebSocket(SocketConnector connector, Uri uri, Action<bool> callback, float timeout)
	{
		connector.Close();
		typeof(SocketConnector).GetProperty("State").GetSetMethod(nonPublic: true).Invoke(connector, new object[1] { ConnectState.Connecting });
		Bridge session = new Bridge();
		bool connected = false;
		try
		{
			Task opening;
			try
			{
				opening = session.Open(uri, TimeSpan.FromSeconds(Math.Max(1f, timeout)));
			}
			catch (Exception error)
			{
				session.Dispose();
				Fail(connector, callback, error);
				yield break;
			}
			while (!opening.IsCompleted)
			{
				if (connector.State != ConnectState.Connecting)
				{
					yield break;
				}
				yield return null;
			}
			if (connector.State != ConnectState.Connecting)
			{
				yield break;
			}
			if (opening.IsFaulted || opening.IsCanceled)
			{
				AggregateException exception = opening.Exception;
				session.Dispose();
				Fail(connector, callback, exception);
				yield break;
			}
			IEnumerator native = (IEnumerator)NativeConnect.Invoke(connector, new object[4]
			{
				"127.0.0.1",
				session.Port,
				(Action<bool>)delegate(bool success)
				{
					connected = success;
					if (success)
					{
						typeof(SocketConnector).GetProperty("Address").GetSetMethod(nonPublic: true).Invoke(connector, new object[1] { uri.AbsoluteUri });
						typeof(SocketConnector).GetProperty("Port").GetSetMethod(nonPublic: true).Invoke(connector, new object[1] { uri.Port });
					}
					if (callback != null)
					{
						callback(success);
					}
				},
				timeout
			});
			while (native.MoveNext())
			{
				yield return native.Current;
			}
		}
		finally
		{
			if (!connected)
			{
				session.Dispose();
			}
		}
	}

	private static void Fail(SocketConnector connector, Action<bool> callback, Exception error)
	{
		connector.Close();
		Debug.LogWarning("SPRK WebSocket connection failed: " + ((error == null) ? "timeout" : error.GetBaseException().Message));
		callback?.Invoke(obj: false);
	}
}
