using System;
using NGame2.NMessage.NFastHandler.NCallback;
using NGame2.NMessage.NFastHandler.NEtc;
using NGame2.NMessage.NFastHandler.NOperation;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NMessage.NFastHandler;

public class Manager : SceneAutoBehaviour<Manager>
{
	private bool _initialized;

	private IncreaseDic<Type, IHandler> _dic = new IncreaseDic<Type, IHandler>();

	protected override void Awake()
	{
		base.Awake();
		Initialize();
	}

	private void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			AddFastHandler(new HandlerBeginFireSkill());
			AddFastHandler(new HandlerBeginMove());
			AddFastHandler(new HandlerChangeState());
			AddFastHandler(new HandlerChangeMp());
			AddFastHandler(new HandlerDamage());
			AddFastHandler(new HandlerDestroyCreature());
			AddFastHandler(new HandlerEndFireSkill());
			AddFastHandler(new HandlerEnterCreature());
			AddFastHandler(new HandlerEventTrigger());
			AddFastHandler(new HandlerFireSkill());
			AddFastHandler(new HandlerHeal());
			AddFastHandler(new HandlerLeaveCreature());
			AddFastHandler(new HandlerSetPosition());
			AddFastHandler(new HandlerShieldBegin());
			AddFastHandler(new HandlerShieldEnd());
			AddFastHandler(new HandlerShieldModified());
			AddFastHandler(new HandlerSpawnCreature());
			AddFastHandler(new HandlerSpawnProjectile());
			AddFastHandler(new HandlerWaveReady());
			AddFastHandler(new HandlerBeginOperation());
			AddFastHandler(new HandlerCancelOperation());
			AddFastHandler(new HandlerEndOperation());
			AddFastHandler(new HandlerFlyingOperation());
			AddFastHandler(new HandlerReserveOperation());
			AddFastHandler(new HandlerResistOperation());
			AddFastHandler(new HandlerBuffIconMessage());
			AddFastHandler(new HandlerCommandMessage());
			AddFastHandler(new HandlerCreatureEventMessage());
			AddFastHandler(new HandlerWidgetMessage());
			AddFastHandler(new HandlerKarmaCardPickOpen());
		}
	}

	public void Handle(IMessage message)
	{
		Initialize();
		if (message != null)
		{
			_dic.GETVALUE(message.GetType(), null)?.Handle(message);
		}
	}

	public void ClearEnd()
	{
		for (int i = 0; i < _dic.Count; i++)
		{
			_dic.GetValueByIndex(i).ClearEnd();
		}
	}

	private void AddFastHandler(IHandler handler)
	{
		_dic.Add(handler.HandleType, handler);
	}

	public GenericReceiver<TMessage> CreateReceiver<TMessage>(IVerifier verifier, Action<TMessage> onMessage) where TMessage : class, IMessage
	{
		Initialize();
		if (!(_dic.GETVALUE(typeof(TMessage), null) is FastHandler<TMessage> fastHandler))
		{
			VespaDebug.LogError("Invalid Message FastHandler: " + typeof(TMessage).FullName);
			return null;
		}
		return fastHandler.AddReceiverAll(verifier, onMessage);
	}

	public GenericReceiver<TMessage> CreateReceiverByValue<TMessage>(IVerifier verifier, Action<TMessage> onMessage, EValueType valueType, long value) where TMessage : class, IMessage
	{
		Initialize();
		if (!(_dic.GETVALUE(typeof(TMessage), null) is FastHandler<TMessage> fastHandler))
		{
			VespaDebug.LogError("Invalid Message FastHandler: " + typeof(TMessage).FullName);
			return null;
		}
		return fastHandler.AddReceiverByValue(verifier, onMessage, valueType, value);
	}
}
