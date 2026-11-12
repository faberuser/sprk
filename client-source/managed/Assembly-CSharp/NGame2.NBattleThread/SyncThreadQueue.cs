using System.Collections.Generic;
using NVespa;

namespace NGame2.NBattleThread;

public class SyncThreadQueue<T> : IThreadReceiveQueue<T>, IThreadSendQueue<T>
{
	private object _sync = new object();

	private Queue<T> _recvQueue = new Queue<T>();

	private Queue<T> _sendQueue = new Queue<T>();

	private Queue<T> _syncQueue = new Queue<T>();

	public IThreadReceiveQueue<T> Receiver => this;

	public IThreadSendQueue<T> Sender => this;

	void IThreadReceiveQueue<T>.Sync()
	{
		lock (_sync)
		{
			if (_syncQueue.Count == 0)
			{
				return;
			}
			if (_recvQueue.Count == 0)
			{
				Math.Swap(ref _recvQueue, ref _syncQueue);
				return;
			}
			while (_syncQueue.Count > 0)
			{
				_recvQueue.Enqueue(_syncQueue.Dequeue());
			}
		}
	}

	bool IThreadReceiveQueue<T>.IsEmpty()
	{
		return _recvQueue.Count == 0;
	}

	T IThreadReceiveQueue<T>.Dequeue()
	{
		if (_recvQueue.Count == 0)
		{
			return default(T);
		}
		return _recvQueue.Dequeue();
	}

	void IThreadSendQueue<T>.Enqueue(T callback)
	{
		_sendQueue.Enqueue(callback);
	}

	void IThreadSendQueue<T>.Flush()
	{
		lock (_sync)
		{
			if (_sendQueue.Count == 0)
			{
				return;
			}
			if (_syncQueue.Count == 0)
			{
				Math.Swap(ref _syncQueue, ref _sendQueue);
				return;
			}
			while (_sendQueue.Count > 0)
			{
				_syncQueue.Enqueue(_sendQueue.Dequeue());
			}
		}
	}
}
