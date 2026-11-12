using System;
using System.Collections.Generic;
using NGame2.NConfig;
using NGame2.NMessage;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NBattleThread.NEventService;

public class MessageRouter
{
	private RollbackManager _rollbackManager = new RollbackManager();

	private Dictionary<Type, Func<BaseBattleCallback, bool>> _dic = new Dictionary<Type, Func<BaseBattleCallback, bool>>();

	public MessageRouter()
	{
		RegisterGeneric<SpawnProjectile>();
		RegisterGeneric<SpawnCreature>();
		RegisterGeneric<DestroyCreature>();
		RegisterGeneric<SetPosition>();
		RegisterGeneric<BeginMove>();
		RegisterGeneric<WaveReady>();
		RegisterGeneric<CastSkill>();
		RegisterGeneric<CancelCastingSkill>();
		RegisterGeneric<BeginFireSkill>();
		Register((FireSkill callback) => SceneAutoBehaviour<BattleCallbackManager>.instance.OnCallback(callback));
		RegisterGeneric<EndFireSkill>();
		RegisterGeneric<Damage>();
		RegisterGeneric<Heal>();
		RegisterGeneric<DamageModified>();
		RegisterGeneric<ChangeMp>();
		RegisterGeneric<ChangeState>();
		RegisterGeneric<ShieldBegin>();
		RegisterGeneric<ShieldEnd>();
		RegisterGeneric<ShieldModified>();
		RegisterGeneric<EventTrigger>();
		RegisterGeneric<KarmaCardSuffle>();
		Register((BeginOperation callback) => SceneAutoBehaviour<BattleCallbackManager>.instance.OnCallback(callback));
		Register((EndOperation callback) => SceneAutoBehaviour<BattleCallbackManager>.instance.OnCallback(callback));
		Register((CancelOperation callback) => SceneAutoBehaviour<BattleCallbackManager>.instance.OnCallback(callback));
		Register((ReserveOperation callback) => SceneAutoBehaviour<BattleCallbackManager>.instance.OnCallback(callback));
		Register((FlyingOperation callback) => SceneAutoBehaviour<BattleCallbackManager>.instance.OnCallback(callback));
		Register((ResistOperation callback) => SceneAutoBehaviour<BattleCallbackManager>.instance.OnCallback(callback));
	}

	private void RegisterGeneric<T>() where T : BaseBattleCallback
	{
		_dic.Add(typeof(T), (BaseBattleCallback callback) => callback is T callback2 && SceneAutoBehaviour<BattleCallbackManager>.instance.OnCallbackGeneric(callback2));
	}

	private void Register<T>(Func<T, bool> func) where T : BaseBattleCallback
	{
		_dic.Add(typeof(T), (BaseBattleCallback callback) => callback is T i_argA && Vespa_System_Action_Extension_Method.fire(func, i_argA));
	}

	public void OnBattleCallback(BaseBattleCallback callback)
	{
		_rollbackManager.OnCallback(callback, out var skipThis);
		if (skipThis)
		{
			return;
		}
		Func<BaseBattleCallback, bool> vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, callback.GetType(), null);
		if (vALUE == null)
		{
			JsonMarshaler.EncodeJsonString(callback);
			return;
		}
		bool flag = Vespa_System_Action_Extension_Method.fire(vALUE, callback);
		if (SingletonAuto<ClientDebugBattle>.instance.LogPacket && SingletonAuto<ClientDebugBattle>.instance.LogThisPacket(callback.GetType()))
		{
			string text = "[" + callback.GetType().Name + "] : " + JsonMarshaler.EncodeJsonString(callback);
			if (!flag)
			{
				text = "(Unhandled) " + text;
			}
		}
	}
}
