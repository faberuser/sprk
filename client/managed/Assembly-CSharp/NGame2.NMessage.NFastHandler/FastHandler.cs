using System;
using System.Collections.Generic;
using NVespa;

namespace NGame2.NMessage.NFastHandler;

public abstract class FastHandler<TMessage> : IHandler where TMessage : class, IMessage
{
	protected List<GenericReceiver<TMessage>> _recvAllList = new List<GenericReceiver<TMessage>>();

	protected IncreaseDic<Tuple<EValueType, long>, List<IReceiver>> _recvByValueDic = new IncreaseDic<Tuple<EValueType, long>, List<IReceiver>>(NGame2.NMessage.NFastHandler.FastHandlerTupleComparer.Default);

	Type IHandler.HandleType => typeof(TMessage);

	private static List<IReceiver> CreateList()
	{
		return SingletonAuto<ReservePool<List<IReceiver>>>.instance.Create();
	}

	private static void ReleaseList(List<IReceiver> list)
	{
		ReservePoolUtil.ReleaseList(list);
	}

	bool IHandler.Handle(IMessage message)
	{
		TMessage val = message.CastMessage<TMessage>();
		if (val == null)
		{
			return false;
		}
		OnMessageAll(val);
		return OnMessage(val);
	}

	void IHandler.ClearEnd()
	{
		for (int num = _recvAllList.Count - 1; num >= 0; num--)
		{
			if (!_recvAllList[num].ISVALID())
			{
				_recvAllList.RemoveAt(num);
			}
		}
		for (int num2 = _recvByValueDic.Count - 1; num2 >= 0; num2--)
		{
			List<IReceiver> valueByIndex = _recvByValueDic.GetValueByIndex(num2);
			for (int num3 = valueByIndex.Count - 1; num3 >= 0; num3--)
			{
				if (!(valueByIndex[num3] as GenericReceiver<TMessage>).ISVALID())
				{
					valueByIndex.RemoveAt(num3);
				}
			}
			if (valueByIndex.Count == 0)
			{
				_recvByValueDic.RemoveByIndex(num2);
				ReleaseList(valueByIndex);
			}
		}
	}

	public GenericReceiver<TMessage> AddReceiverAll(IVerifier verifier, Action<TMessage> onMessage)
	{
		GenericReceiver<TMessage> genericReceiver = new GenericReceiver<TMessage>(verifier, onMessage);
		_recvAllList.Add(genericReceiver);
		return genericReceiver;
	}

	public GenericReceiver<TMessage> AddReceiverByValue(IVerifier verifier, Action<TMessage> onMessage, EValueType type, long value)
	{
		Tuple<EValueType, long> key = Tuple.Create(type, value);
		List<IReceiver> list = _recvByValueDic.GETVALUE(key, null);
		if (list == null)
		{
			list = CreateList();
			_recvByValueDic.Add(key, list);
		}
		GenericReceiver<TMessage> genericReceiver = new GenericReceiver<TMessage>(verifier, onMessage);
		list.Add(genericReceiver);
		return genericReceiver;
	}

	protected virtual void OnMessageAll(TMessage message)
	{
		for (int i = 0; i < _recvAllList.Count; i++)
		{
			GenericReceiver<TMessage> genericReceiver = _recvAllList[i];
			if (!genericReceiver.ISVALID())
			{
				_recvAllList.RemoveAt(i);
				i--;
			}
			else
			{
				genericReceiver.OnMessage(message);
			}
		}
	}

	protected void OnMessageByValue(TMessage message, EValueType valueType, long value)
	{
		Tuple<EValueType, long> key = Tuple.Create(valueType, value);
		List<IReceiver> vALUE = _recvByValueDic.GETVALUE(key, null);
		if (vALUE == null)
		{
			return;
		}
		for (int i = 0; i < vALUE.Count; i++)
		{
			GenericReceiver<TMessage> genericReceiver = vALUE[i] as GenericReceiver<TMessage>;
			if (!genericReceiver.ISVALID())
			{
				vALUE.RemoveAt(i);
				i--;
			}
			else
			{
				genericReceiver.OnMessage(message);
			}
		}
	}

	protected abstract bool OnMessage(TMessage message);
}
