using System;
using System.Collections.Generic;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NMessage.NFastHandler;

public class Holder
{
	public interface ICollector
	{
		void Collect(Holder holder);
	}

	private List<IReceiver> _receiverList = new List<IReceiver>();

	public void Clear()
	{
		for (int i = 0; i < _receiverList.Count; i++)
		{
			_receiverList[i].Close();
		}
		_receiverList.Clear();
	}

	public void Add<TMessage>(IVerifier verifier, Action<TMessage> onMessage) where TMessage : class, IMessage
	{
		GenericReceiver<TMessage> genericReceiver = SceneAutoBehaviour<Manager>.instance.CreateReceiver(verifier, onMessage);
		if (genericReceiver != null)
		{
			_receiverList.Add(genericReceiver);
		}
	}

	public void AddByCreatureUid<TMessage>(IVerifier verifier, Action<TMessage> onMessage, int uid) where TMessage : class, IMessage
	{
		GenericReceiver<TMessage> genericReceiver = SceneAutoBehaviour<Manager>.instance.CreateReceiverByValue(verifier, onMessage, EValueType.CreatureUid, uid);
		if (genericReceiver != null)
		{
			_receiverList.Add(genericReceiver);
		}
	}

	public void AddBySkillIndex<TMessage>(IVerifier verifier, Action<TMessage> onMessage, int skillIndex) where TMessage : class, IMessage
	{
		GenericReceiver<TMessage> genericReceiver = SceneAutoBehaviour<Manager>.instance.CreateReceiverByValue(verifier, onMessage, EValueType.SkillIndex, skillIndex);
		if (genericReceiver != null)
		{
			_receiverList.Add(genericReceiver);
		}
	}

	public void AddByOperationUid<TMessage>(IVerifier verifier, Action<TMessage> onMessage, long uid) where TMessage : class, IMessage
	{
		GenericReceiver<TMessage> genericReceiver = SceneAutoBehaviour<Manager>.instance.CreateReceiverByValue(verifier, onMessage, EValueType.OperationUid, uid);
		if (genericReceiver != null)
		{
			_receiverList.Add(genericReceiver);
		}
	}

	public int GetCount()
	{
		return _receiverList.Count;
	}
}
