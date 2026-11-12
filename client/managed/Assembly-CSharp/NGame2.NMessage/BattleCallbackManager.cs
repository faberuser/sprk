using System.Collections.Generic;
using NGame2.NEventTrigger;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NMessage;

public class BattleCallbackManager : SceneAutoBehaviour<BattleCallbackManager>, InitOnCreate, IInspectorGuiDrawer
{
	private IncreaseDic<long, OperationRuntime> _operationDic = new IncreaseDic<long, OperationRuntime>(128);

	void InitOnCreate.InitializeSingleton()
	{
	}

	public OperationRuntime GetOperationRuntime(long operationUid)
	{
		return _operationDic.GETVALUE(operationUid, null);
	}

	public void ChainUpdate()
	{
		for (int i = 0; i < _operationDic.Count; i++)
		{
			KeyValuePair<long, OperationRuntime> keyValuePairByIndex = _operationDic.GetKeyValuePairByIndex(i);
			_ = keyValuePairByIndex.Key;
			OperationRuntime value = keyValuePairByIndex.Value;
			if (value.IsUsed && value.IsEnd && value.FrameNumber + 10 < Time.frameCount)
			{
				SmallPooling<OperationRuntime>.ReleaseInstance(value);
				_operationDic.RemoveByIndex(i);
				i--;
			}
		}
	}

	public bool OnCallbackGeneric<TBattleCallback>(TBattleCallback callback) where TBattleCallback : BaseBattleCallback
	{
		CallbackMessage<TBattleCallback> callbackMessage = SmallPooling<CallbackMessage<TBattleCallback>>.CreateInstance();
		callbackMessage.Open(callback);
		SingletonAuto<GlobalManager>.instance.PostMessage(callbackMessage);
		return true;
	}

	public bool OnCallback(FireSkill callback)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(callback.OperationInfos); i++)
		{
			FireSkill.OperationInfo operationInfo = callback.OperationInfos[i];
			OperationRuntime operationRuntime = SmallPooling<OperationRuntime>.CreateInstance();
			operationRuntime.Open(operationInfo, callback);
			_operationDic.RemoveSlow(operationInfo.OperationUid);
			_operationDic.Add(operationInfo.OperationUid, operationRuntime);
		}
		OnSkillFire onSkillFire = new OnSkillFire();
		onSkillFire.SkillId = callback.SkillId;
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(onSkillFire);
		return OnCallbackGeneric(callback);
	}

	public bool OnCallback(ReserveOperation callback)
	{
		OperationRuntime vALUE = _operationDic.GETVALUE(callback.OperationUid, null);
		if (vALUE == null)
		{
			return false;
		}
		vALUE.OnReserve();
		SingletonAuto<GlobalManager>.instance.PostOperationMessage(callback, vALUE);
		return true;
	}

	public bool OnCallback(FlyingOperation callback)
	{
		OperationRuntime vALUE = _operationDic.GETVALUE(callback.OperationUid, null);
		if (vALUE == null)
		{
			return false;
		}
		vALUE.OnFlying();
		SingletonAuto<GlobalManager>.instance.PostOperationMessage(callback, vALUE);
		return true;
	}

	public bool OnCallback(BeginOperation callback)
	{
		OperationRuntime vALUE = _operationDic.GETVALUE(callback.OperationUid, null);
		if (vALUE == null)
		{
			return false;
		}
		vALUE.OnBegin(callback);
		SingletonAuto<GlobalManager>.instance.PostOperationMessage(callback, vALUE);
		return true;
	}

	public bool OnCallback(EndOperation callback)
	{
		OperationRuntime vALUE = _operationDic.GETVALUE(callback.OperationUid, null);
		if (vALUE == null)
		{
			return false;
		}
		vALUE.OnEnd();
		SingletonAuto<GlobalManager>.instance.PostOperationMessage(callback, vALUE);
		return true;
	}

	public bool OnCallback(CancelOperation callback)
	{
		OperationRuntime vALUE = _operationDic.GETVALUE(callback.OperationUid, null);
		if (vALUE == null)
		{
			return false;
		}
		vALUE.OnCancel();
		SingletonAuto<GlobalManager>.instance.PostOperationMessage(callback, vALUE);
		return true;
	}

	public bool OnCallback(ResistOperation callback)
	{
		OperationRuntime vALUE = _operationDic.GETVALUE(callback.OperationUid, null);
		if (vALUE == null)
		{
			return false;
		}
		vALUE.OnResist();
		SingletonAuto<GlobalManager>.instance.PostOperationMessage(callback, vALUE);
		return true;
	}
}
