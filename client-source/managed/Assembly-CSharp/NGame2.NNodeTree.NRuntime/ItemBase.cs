using System;
using NGame2.NMessage.NFastHandler;
using NVespa;

namespace NGame2.NNodeTree.NRuntime;

public class ItemBase
{
	private Holder _messageHolder;

	public RuntimeInstance Runtime { get; private set; }

	public LocalTimer localTimer => Runtime.LocalTimer;

	public LocalVariable Variable { get; private set; }

	public void OpenFromRuntime(RuntimeInstance runtime, IVariable variable)
	{
		Runtime = runtime;
		Variable = SmallPooling<LocalVariable>.CreateInstance();
		Variable.SetParent(variable);
		Vespa_Common_Class_Extension_Method.Interface((IVariable)Variable).Open(runtime);
	}

	public void CloseFromRuntime()
	{
		ClearMessageHolder();
		Vespa_Common_Class_Extension_Method.Interface((IVariable)Variable).ReleaseSelf();
		Variable = null;
	}

	public void AddOnMessageByCreatureUid<T, TMessage>(IVerifier verifier, Action<T, TMessage> onMessage, int uid) where T : ItemBase where TMessage : class, IMessage
	{
		if (_messageHolder == null)
		{
			_messageHolder = ReservePoolUtil.Create<Holder>();
		}
		_messageHolder.AddByCreatureUid(verifier, delegate(TMessage message)
		{
			Vespa_System_Action_Extension_Method.fire(onMessage, this as T, message);
		}, uid);
	}

	public void AddOnMessageByOperationUid<T, TMessage>(IVerifier verifier, Action<T, TMessage> onMessage, long uid) where T : ItemBase where TMessage : class, IMessage
	{
		if (_messageHolder == null)
		{
			_messageHolder = ReservePoolUtil.Create<Holder>();
		}
		_messageHolder.AddByOperationUid(verifier, delegate(TMessage message)
		{
			Vespa_System_Action_Extension_Method.fire(onMessage, this as T, message);
		}, uid);
	}

	public void ClearMessageHolder()
	{
		if (_messageHolder != null)
		{
			_messageHolder.Clear();
			ReservePoolUtil.Release(_messageHolder);
			_messageHolder = null;
		}
	}
}
