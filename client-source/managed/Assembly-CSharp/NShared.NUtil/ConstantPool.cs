using System.Collections.Generic;

namespace NShared.NUtil;

public class ConstantPool
{
	private readonly object _queueSync = new object();

	private readonly Queue<ConstantInfo> _queue = new Queue<ConstantInfo>();

	public ConstantInfo Alloc(long value)
	{
		ConstantInfo constantInfo = null;
		if (_queue.Count > 0)
		{
			lock (_queueSync)
			{
				if (_queue.Count > 0)
				{
					constantInfo = _queue.Dequeue();
				}
			}
			constantInfo?.Set(value);
		}
		if (constantInfo == null)
		{
			constantInfo = new ConstantInfo(value);
		}
		constantInfo.Pool = this;
		return constantInfo;
	}

	public void Release(ConstantInfo info)
	{
		if (info == null)
		{
			return;
		}
		lock (info)
		{
			if (info.Pool != this)
			{
				return;
			}
			info.Pool = null;
		}
		lock (_queueSync)
		{
			_queue.Enqueue(info);
		}
	}
}
