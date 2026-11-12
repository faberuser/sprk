using System.Collections.Generic;
using NGame2.NBattleView.NCreature.NHandler.NCrowdControl;
using NGame2.NMessage;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NBattleView.NCreature.NHandler;

public class CrowdControl : BaseHandler, IHandler
{
	public interface ICcMotion
	{
		void ReleaseCc();

		void ChainUpdate();
	}

	public interface ICcPosition
	{
		void ChainUpdate();
	}

	public class CcDownDuration
	{
		public long OperationUid;

		public bool IsSoloOperation;

		public ILocalTimer LocalTimer;

		public int ElapsedTick;

		public int DurationTick;
	}

	public ChangeState Packet;

	public List<CcDownDuration> DownDurationList = new List<CcDownDuration>();

	private ICcMotion _ccMotion;

	private ICcPosition _ccPosition;

	public static bool IsCrowdControl(ChangeState packet)
	{
		if (!packet.StateAdded)
		{
			return false;
		}
		return GetTopmostCrowdControl(packet.FinalStateType) != CcMotionType.None;
	}

	public static CcMotionType GetTopmostCrowdControl(string[] finalStateType)
	{
		CcMotionType ccMotionType = CcMotionType.None;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(finalStateType); i++)
		{
			string key = finalStateType[i];
			StateData data = GameRoot.Instance.StateDataContainer.GetData(key);
			if (data != null && ccMotionType > data.CcMotionType)
			{
				ccMotionType = data.CcMotionType;
			}
		}
		return ccMotionType;
	}

	public CrowdControl(Creature creature, ChangeState packet)
		: base(creature)
	{
		Packet = packet;
		ArrangeDownOperation(packet);
	}

	private void Close()
	{
		Packet = null;
	}

	private void ArrangeDownOperation(ChangeState packet)
	{
		StateData data = GameRoot.Instance.StateDataContainer.GetData(packet.ChangedStateType);
		if (data == null || data.CcMotionType != CcMotionType.Down)
		{
			return;
		}
		if (packet.StateAdded)
		{
			OperationRuntime operationRuntime = SceneAutoBehaviour<BattleCallbackManager>.instance.GetOperationRuntime(packet.OperationUid);
			if (operationRuntime != null)
			{
				DownDurationList.Add(new CcDownDuration
				{
					OperationUid = packet.OperationUid,
					IsSoloOperation = operationRuntime.IsSoloTimer,
					LocalTimer = operationRuntime.OperationTimer,
					DurationTick = operationRuntime.OperationData.DurationMs
				});
			}
			return;
		}
		for (int i = 0; i < DownDurationList.Count; i++)
		{
			if (DownDurationList[i].OperationUid == packet.OperationUid)
			{
				DownDurationList.RemoveAt(i);
				break;
			}
		}
	}

	public void OnMessageFast(CallbackMessage<ChangeState> message)
	{
		if (message.IsValidMessage())
		{
			message.Use();
			ArrangeDownOperation(message.Callback);
			Packet = message.Callback;
			OnChangeState();
		}
	}

	public void OnMessageFast(CallbackMessage<NShared.SetPosition> message)
	{
		if (message.IsValidMessage())
		{
			if (message.Callback.MethodType == NShared.SetPosition.SetPositionMethodType.Knockback || message.Callback.MethodType == NShared.SetPosition.SetPositionMethodType.Pulling)
			{
				_ccPosition = new CcPositionKnockback(this, message.Callback);
			}
			else if (message.Callback.MethodType == NShared.SetPosition.SetPositionMethodType.Down)
			{
				_ccPosition = new CcPositionDown(this, message.Callback);
			}
		}
	}

	void IHandler.Begin()
	{
		OnChangeState();
	}

	bool IHandler.IsEnd()
	{
		return Packet == null;
	}

	void IHandler.Cancel()
	{
		Close();
	}

	void IHandler.ChainUpdate()
	{
		if (!Vespa_Common_Class_Extension_Method.Interface((IHandler)this).IsEnd())
		{
			for (int i = 0; i < DownDurationList.Count; i++)
			{
				CcDownDuration ccDownDuration = DownDurationList[i];
				ccDownDuration.ElapsedTick += ccDownDuration.LocalTimer.deltaTick;
			}
			if (_ccMotion != null)
			{
				_ccMotion.ChainUpdate();
			}
			if (_ccPosition != null)
			{
				_ccPosition.ChainUpdate();
			}
		}
	}

	private void OnChangeState()
	{
		switch (GetTopmostCrowdControl(Packet.FinalStateType))
		{
		case CcMotionType.Pause:
			_ccMotion = new CcMotionPause(this);
			return;
		case CcMotionType.Down:
			if (!(_ccMotion is CcMotionDown ccMotionDown))
			{
				_ccMotion = new CcMotionDown(this);
			}
			else if (Packet.StateAdded)
			{
				StateData data2 = GameRoot.Instance.StateDataContainer.GetData(Packet.ChangedStateType);
				if (data2 != null && data2.CcMotionType == CcMotionType.Down)
				{
					ccMotionDown.Refresh();
				}
			}
			return;
		case CcMotionType.Stun:
			_ccMotion = new CcMotionStun(this);
			return;
		case CcMotionType.Knockback:
		{
			StateData data = GameRoot.Instance.StateDataContainer.GetData(Packet.ChangedStateType);
			if (Packet.StateAdded && data != null && data.CcMotionType == CcMotionType.Knockback)
			{
				_ccMotion = new CcMotionKnockback(this);
				return;
			}
			break;
		}
		}
		if (_ccMotion != null)
		{
			_ccMotion.ReleaseCc();
			_ccMotion = null;
		}
	}
}
