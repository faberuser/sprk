using NGame2.NMessage.NFastHandler;
using NVespa;

namespace NGame2.NMessage;

public class HandlerContainer
{
	private Holder _receiverHolder;

	public HandlerContainer()
	{
		_receiverHolder = ReservePoolUtil.Create<Holder>();
	}

	public void Clear()
	{
		_receiverHolder.Clear();
	}

	public void AddCollector(Holder.ICollector collector)
	{
		collector.Collect(_receiverHolder);
	}
}
