using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace NVespa;

public abstract class CustomStreamBase : Stream
{
	public class ChunkInfo
	{
		public byte[] Buffer;

		public long Offset;

		public int Count;
	}

	protected readonly Queue<ChunkInfo> _chunkList = new Queue<ChunkInfo>();

	protected long _totalLength;

	protected long _currentLength;

	protected long _position;

	protected int _lastReadChunkOffset;

	protected int _accumedLength;

	protected int _maxBufferCount2;

	protected int _curBufferCount;

	protected Queue<ChunkInfo> _recvQueue = new Queue<ChunkInfo>();

	public bool WriteCompleted => _currentLength >= _totalLength;

	public bool IsEOF => _position >= _totalLength;

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => true;

	public override long Length => _totalLength;

	public override long Position
	{
		get
		{
			return _position;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public long ReadableLength => _currentLength - _position;

	public CustomStreamBase(long length)
	{
		_totalLength = length;
	}

	protected virtual void ReleaseChunk()
	{
		_chunkList.Dequeue();
		_lastReadChunkOffset = 0;
		lock (_recvQueue)
		{
			_curBufferCount--;
		}
	}

	protected virtual void Advance(int offset)
	{
		_lastReadChunkOffset += offset;
		_position += offset;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		ChunkInfo chunkInfo = new ChunkInfo();
		chunkInfo.Buffer = new byte[buffer.Length];
		chunkInfo.Offset = offset;
		chunkInfo.Count = count;
		Buffer.BlockCopy(buffer, 0, chunkInfo.Buffer, 0, buffer.Length);
		lock (_recvQueue)
		{
			_recvQueue.Enqueue(chunkInfo);
			_curBufferCount++;
			if (_curBufferCount > _maxBufferCount2)
			{
				_maxBufferCount2 = _curBufferCount;
			}
		}
	}

	public bool CheckCanRead(int readLength)
	{
		lock (_recvQueue)
		{
			while (_recvQueue.Count > 0)
			{
				ChunkInfo chunkInfo = _recvQueue.Dequeue();
				_chunkList.Enqueue(chunkInfo);
				_currentLength += chunkInfo.Count;
			}
		}
		if (WriteCompleted)
		{
			return true;
		}
		if (ReadableLength >= readLength)
		{
			return true;
		}
		return false;
	}

	public void WaitCanRead(int readLength)
	{
		while (true)
		{
			lock (_recvQueue)
			{
				while (_recvQueue.Count > 0)
				{
					ChunkInfo chunkInfo = _recvQueue.Dequeue();
					_chunkList.Enqueue(chunkInfo);
					_currentLength += chunkInfo.Count;
				}
			}
			if (!WriteCompleted && ReadableLength < readLength)
			{
				Thread.Sleep(10);
				continue;
			}
			break;
		}
	}

	public override int ReadByte()
	{
		if (IsEOF)
		{
			return -1;
		}
		ChunkInfo chunkInfo = _chunkList.Peek();
		if (chunkInfo.Count - _lastReadChunkOffset <= 0)
		{
			ReleaseChunk();
			chunkInfo = _chunkList.Peek();
		}
		byte result = chunkInfo.Buffer[chunkInfo.Offset + _lastReadChunkOffset];
		Advance(1);
		return result;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = 0;
		while (count > 0)
		{
			if (_chunkList.Count == 0)
			{
				if (!IsEOF && num <= 0)
				{
				}
				break;
			}
			ChunkInfo chunkInfo = _chunkList.Peek();
			int num2 = chunkInfo.Count - _lastReadChunkOffset;
			if (num2 <= 0)
			{
				ReleaseChunk();
				continue;
			}
			try
			{
				int num3 = System.Math.Min(num2, count);
				Buffer.BlockCopy(chunkInfo.Buffer, Convert.ToInt32(chunkInfo.Offset) + _lastReadChunkOffset, buffer, offset, num3);
				Advance(num3);
				_accumedLength += num3;
				num += num3;
				offset += num3;
				count -= num3;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		return num;
	}

	public override void Flush()
	{
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}
}
