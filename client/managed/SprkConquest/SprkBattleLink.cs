using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public sealed class SprkBattleLink
{
	private sealed class Packet
	{
		public string Name;

		public object Body;
	}

	private readonly object gate = new object();

	private readonly Queue<Packet> outgoing = new Queue<Packet>();

	private readonly Queue<string> incoming = new Queue<string>();

	private readonly TcpClient socket;

	private readonly Func<object, string> serialize;

	private readonly Action<string> warning;

	private readonly int capacity;

	private bool closed;

	public bool Connected
	{
		get
		{
			lock (gate)
			{
				return !closed;
			}
		}
	}

	public SprkBattleLink(TcpClient socket, Func<object, string> serialize, Action<string> warning, int capacity = 1024)
	{
		this.socket = socket;
		this.serialize = serialize;
		this.warning = warning;
		this.capacity = capacity;
		socket.NoDelay = true;
		socket.SendTimeout = 5000;
		Thread thread = new Thread(WriteLoop);
		thread.IsBackground = true;
		thread.Start();
	}

	public void Close()
	{
		lock (gate)
		{
			if (closed)
			{
				return;
			}
			closed = true;
			outgoing.Clear();
			incoming.Clear();
			Monitor.PulseAll(gate);
		}
		socket.Close();
	}

	public void Send(string name, object body)
	{
		lock (gate)
		{
			if (closed)
			{
				return;
			}
			if (outgoing.Count < capacity)
			{
				outgoing.Enqueue(new Packet
				{
					Name = name,
					Body = body
				});
				Monitor.PulseAll(gate);
				return;
			}
		}
		Fail("outgoing queue full");
	}

	public void ReceiveLoop()
	{
		try
		{
			using StreamReader streamReader = new StreamReader(socket.GetStream());
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				if (text.Length > 8388608)
				{
					throw new IOException("Oversized worker message");
				}
				lock (gate)
				{
					if (closed)
					{
						break;
					}
					if (incoming.Count >= capacity)
					{
						throw new IOException("incoming queue full");
					}
					incoming.Enqueue(text);
				}
			}
		}
		catch (Exception ex)
		{
			if (Connected)
			{
				warning("receive: " + ex.Message);
			}
		}
		finally
		{
			Close();
		}
	}

	public bool TryReceive(out string line)
	{
		lock (gate)
		{
			line = ((incoming.Count == 0) ? null : incoming.Dequeue());
			return line != null;
		}
	}

	private void WriteLoop()
	{
		try
		{
			using StreamWriter streamWriter = new StreamWriter(socket.GetStream(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
			{
				AutoFlush = true,
				NewLine = "\r\n"
			};
			while (true)
			{
				Packet packet;
				lock (gate)
				{
					while (!closed && outgoing.Count == 0)
					{
						Monitor.Wait(gate);
					}
					if (closed)
					{
						break;
					}
					packet = outgoing.Dequeue();
				}
				streamWriter.WriteLine(packet.Name + " " + Convert.ToBase64String(Encoding.UTF8.GetBytes(serialize(packet.Body))));
			}
		}
		catch (Exception ex)
		{
			if (Connected)
			{
				warning("send: " + ex.Message);
			}
		}
		finally
		{
			Close();
		}
	}

	private void Fail(string message)
	{
		warning(message + "; reconnect required");
		Close();
	}
}
