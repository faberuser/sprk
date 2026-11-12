using System.Collections.Generic;
using NGame2.NMessage.NFastHandler;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NMessage;

public class GlobalManager : SingletonAuto<GlobalManager>
{
	public interface IListener
	{
		void Listen(IMessage message);
	}

	private List<IListener> _listenerList = new List<IListener>();

	private Queue<IMessage> _postMessageQueue = new Queue<IMessage>(8192);

	public void AddListerer(IListener listener)
	{
		if (!_listenerList.Contains(listener))
		{
			_listenerList.Add(listener);
		}
	}

	public void RemoveListener(IListener listener)
	{
		for (int i = 0; i < _listenerList.Count; i++)
		{
			if (_listenerList[i] == listener)
			{
				_listenerList[i] = null;
			}
		}
	}

	public void PostOperationMessage<TBattleCallback>(TBattleCallback callback, OperationRuntime operationRuntime) where TBattleCallback : BaseBattleCallback
	{
		if (operationRuntime != null)
		{
			OperationMessage<TBattleCallback> operationMessage = SmallPooling<OperationMessage<TBattleCallback>>.CreateInstance();
			operationMessage.Open(callback, operationRuntime);
			PostMessage(operationMessage);
		}
	}

	public void PostMessage(IMessage message)
	{
		_postMessageQueue.Enqueue(message);
	}

	public void HandleDirect(IMessage message)
	{
		Handle(message, releaseMsg: false);
	}

	private void Handle(IMessage message, bool releaseMsg)
	{
		SceneAutoBehaviour<Manager>.instance.Handle(message);
		for (int i = 0; i < _listenerList.Count; i++)
		{
			IListener listener = _listenerList[i];
			if (Vespa_Common_Class_Extension_Method.IsUnityNull(listener))
			{
				_listenerList.RemoveAt(i);
				i--;
			}
			else
			{
				listener.Listen(message);
			}
		}
		if (releaseMsg)
		{
			message.ReleaseSelf();
		}
	}

	public void ChainUpdate()
	{
		HandlePostMessage();
		if (SceneAutoBehaviour<BattleCallbackManager>.isValidInstance)
		{
			SceneAutoBehaviour<BattleCallbackManager>.instance.ChainUpdate();
		}
		if (SceneAutoBehaviour<Manager>.isValidInstance)
		{
			SceneAutoBehaviour<Manager>.instance.ClearEnd();
		}
	}

	private void HandlePostMessage()
	{
		while (_postMessageQueue.Count > 0)
		{
			IMessage message = _postMessageQueue.Dequeue();
			Handle(message, releaseMsg: true);
		}
	}
}
