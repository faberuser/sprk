using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NMessage;

public class OperationMessage<TBattleCallback> : SmallPooling<OperationMessage<TBattleCallback>>, ISmallPoolingItem, IMessage where TBattleCallback : BaseBattleCallback
{
	private OperationRuntime _operationRuntime;

	public bool IsDummy { get; set; }

	public TBattleCallback Callback { get; private set; }

	public int FireSkillId
	{
		get
		{
			if (_operationRuntime != null)
			{
				return _operationRuntime.FireSkillCallback.SkillId;
			}
			return 0;
		}
	}

	public int OperationNo
	{
		get
		{
			if (_operationRuntime != null)
			{
				return _operationRuntime.OperationInfo.OperationNo;
			}
			return 0;
		}
	}

	public long OperationUid { get; private set; }

	public int FireSkillCreatureUid
	{
		get
		{
			if (_operationRuntime != null)
			{
				return _operationRuntime.FireSkillCallback.CreatureUid;
			}
			return 0;
		}
	}

	public int TargetCreatureUid { get; private set; }

	public Vector3 TargetPosition
	{
		get
		{
			if (_operationRuntime != null)
			{
				return _operationRuntime.OperationInfo.TargetPosition;
			}
			return Vector3.zero;
		}
	}

	public ILocalTimer OperationTimer
	{
		get
		{
			if (_operationRuntime != null)
			{
				return _operationRuntime.OperationTimer;
			}
			return null;
		}
	}

	void ISmallPoolingItem.OnCreateNew()
	{
	}

	void ISmallPoolingItem.OnDestroy()
	{
	}

	void ISmallPoolingItem.OnCreate()
	{
	}

	void ISmallPoolingItem.OnRelease()
	{
		Callback = null;
		SmallPooling<OperationRuntime>.ReleaseInstance(_operationRuntime);
		_operationRuntime = null;
	}

	void IMessage.ReleaseSelf()
	{
		SmallPooling<OperationMessage<TBattleCallback>>.ReleaseInstance(this);
	}

	public void Open(TBattleCallback callback, OperationRuntime operationRuntime)
	{
		Callback = callback;
		_operationRuntime = SmallPooling<OperationRuntime>.CaptureInstance(operationRuntime);
		if (callback is ReserveOperation)
		{
			ReserveOperation reserveOperation = callback as ReserveOperation;
			TargetCreatureUid = _operationRuntime.OperationInfo.TargetCreatureUid;
			OperationUid = reserveOperation.OperationUid;
		}
		else if (callback is FlyingOperation)
		{
			FlyingOperation flyingOperation = callback as FlyingOperation;
			TargetCreatureUid = (int)flyingOperation.TargetCreatureUid;
			OperationUid = flyingOperation.OperationUid;
		}
		else if (callback is BeginOperation)
		{
			BeginOperation beginOperation = callback as BeginOperation;
			TargetCreatureUid = beginOperation.CreatureUid;
			OperationUid = beginOperation.OperationUid;
		}
		else if (callback is EndOperation)
		{
			EndOperation endOperation = callback as EndOperation;
			TargetCreatureUid = endOperation.CreatureUid;
			OperationUid = endOperation.OperationUid;
		}
		else if (callback is CancelOperation)
		{
			CancelOperation cancelOperation = callback as CancelOperation;
			TargetCreatureUid = cancelOperation.CreatureUid;
			OperationUid = cancelOperation.OperationUid;
		}
		else if (callback is ResistOperation)
		{
			ResistOperation resistOperation = callback as ResistOperation;
			TargetCreatureUid = resistOperation.CreatureUid;
			OperationUid = resistOperation.OperationUid;
		}
	}

	public float Wakeup()
	{
		return 0f + (float)FireSkillId + (float)OperationNo + (float)OperationUid + (float)FireSkillCreatureUid + (float)TargetCreatureUid + TargetPosition.x;
	}
}
