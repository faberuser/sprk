using System;
using System.Collections.Generic;
using NGame2.NMatch.NDuelState;
using NGame2.NMessageLog;
using NGame2.NNotify;
using NGame2.NUI.NManager;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NMatch;

public class DuelManager : SingletonUpdater<DuelManager>
{
	private class PingData
	{
		public long PlayerId;

		public long GroupId;

		public int SendTick;

		public bool PongReceived;
	}

	public Dictionary<long, int> LastRequestTickDic = new Dictionary<long, int>();

	private StateBase _pendingState;

	private long _canDuelTargetId;

	private Action<long, CanDuelResponse> _onCanDuelResponse;

	private PingData _pingData;

	public StateBase DuelState { get; private set; }

	public DuelManager()
	{
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<CanDuelRequest>
		{
			Handler = OnReceive
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<CanDuelResponse>
		{
			Handler = OnReceive
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<NShared.NMessage.NProtocol.RequestDuel>
		{
			Handler = OnReceive
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<AcceptDuel>
		{
			Handler = OnReceive
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<CancelDuel>
		{
			Handler = OnReceive
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<CompletePartySetting>
		{
			Handler = OnReceive
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<NShared.NMessage.NProtocol.BeginDuel>
		{
			Handler = OnReceive
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<DuelPing>
		{
			Handler = OnReceive
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<DuelPong>
		{
			Handler = OnReceive
		});
		DuelState = new None();
		GlobalAutoBehaviour<GuiManager>.instance.Add("DuelManager", this, delegate
		{
			GUILayout.Label("State: " + DuelState.GetType().Name);
			GUILayout.Label("VsPlayerId: " + DuelState.VsPlayerId);
			GUILayout.Label("GroupId: " + DuelState.GroupId);
		});
	}

	public void OnSceneChanged()
	{
		CancelCurrentDuel();
		if (SceneAutoBehaviour<DuelManagement>.isValidInstance)
		{
			SceneAutoBehaviour<DuelManagement>.instance.CancelDuel();
		}
	}

	public override void ChainUpdate()
	{
		if (_pendingState != null)
		{
			StateBase pendingState = _pendingState;
			_pendingState = null;
			ChangeState(DuelState, pendingState);
		}
		DuelState.ChainUpdate();
		UpdatePing();
	}

	private void UpdatePing()
	{
		if (DuelState.GroupId == 0L)
		{
			_pingData = null;
		}
		else if (_pingData == null)
		{
			_pingData = new PingData();
			_pingData.PlayerId = DuelState.VsPlayerId;
			_pingData.GroupId = DuelState.GroupId;
			_pingData.SendTick = UnitySystemTimer.tick;
			DuelPing duelPing = new DuelPing();
			duelPing.GroupId = DuelState.GroupId;
			SingletonAuto<MessageLogHandler>.instance.SendToClient(_pingData.PlayerId, duelPing);
		}
		else if (DuelState.VsPlayerId != _pingData.PlayerId || DuelState.GroupId != _pingData.GroupId)
		{
			_pingData = null;
		}
		else
		{
			if (UnitySystemTimer.tick - _pingData.SendTick < 10000)
			{
				return;
			}
			if (_pingData.PongReceived)
			{
				_pingData = null;
				return;
			}
			CancelCurrentDuel();
			if (SceneAutoBehaviour<DuelManagement>.isValidInstance)
			{
				SceneAutoBehaviour<DuelManagement>.instance.CancelDuel();
			}
		}
	}

	public void ChangeState(StateBase curState, StateBase newState, bool pending = false)
	{
		if (DuelState != curState)
		{
			return;
		}
		if (pending)
		{
			_pendingState = newState;
			return;
		}
		if (DuelState != null)
		{
			DuelState.OnEnd();
		}
		DuelState = newState;
		if (DuelState != null)
		{
			DuelState.OnBegin();
		}
	}

	public void SendCancelDuel(long receiverId, long groupId, bool isReject)
	{
		CancelDuel cancelDuel = new CancelDuel();
		cancelDuel.GroupId = groupId;
		cancelDuel.IsReject = isReject;
		SingletonAuto<MessageLogHandler>.instance.SendToClient(receiverId, cancelDuel);
	}

	public void SendCanDuel(long receiverId, Action<long, CanDuelResponse> onResponse)
	{
		_canDuelTargetId = receiverId;
		_onCanDuelResponse = onResponse;
		CanDuelRequest data = new CanDuelRequest();
		SingletonAuto<MessageLogHandler>.instance.SendToClient(receiverId, data);
	}

	public void RequestDuel(long receiverId, long groupId)
	{
		DuelState.Request(receiverId, groupId);
	}

	public void AcceptDuel(long requestId, long groupId)
	{
		DuelState.AcceptDuel(requestId, groupId);
	}

	public void CompletePartySetting(long groupId, int[] heroIndices, int leaderHeroIndex)
	{
		if (SprkDispatchSelection.CanUseHeroes(heroIndices))
		{
			DuelState.CompletePartySetting(groupId, heroIndices, leaderHeroIndex);
		}
	}

	public void CancelCurrentDuel()
	{
		DuelState.CancelCurrentDuel();
	}

	public bool CanDuel(long vsPlayerId, bool isMyRequest)
	{
		if (!InviteNotifyManager.IsOpenableNoty(vsPlayerId))
		{
			return false;
		}
		if (isMyRequest)
		{
			int vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(LastRequestTickDic, vsPlayerId, 0);
			if (UnitySystemTimer.SystemTick - vALUE < 10000)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsPlayDuelMatch()
	{
		if (DuelState is None)
		{
			return false;
		}
		return true;
	}

	private void OnReceive(long senderId, CanDuelRequest data)
	{
		DuelState.OnReceive(senderId, data);
	}

	private void OnReceive(long senderId, CanDuelResponse data)
	{
		if (_onCanDuelResponse != null && senderId == _canDuelTargetId)
		{
			Vespa_System_Action_Extension_Method.fire(_onCanDuelResponse, senderId, data);
			_onCanDuelResponse = null;
		}
	}

	private void OnReceive(long senderId, NShared.NMessage.NProtocol.RequestDuel data)
	{
		SprkHonorDuel.ForgetCancel(senderId);
		DuelState.OnReceive(senderId, data);
	}

	private void OnReceive(long senderId, AcceptDuel data)
	{
		DuelState.OnReceive(senderId, data);
	}

	private void OnReceive(long senderId, CancelDuel data)
	{
		SprkHonorDuel.NoteCancel(senderId, data);
		DuelState.OnReceive(senderId, data);
	}

	private void OnReceive(long senderId, CompletePartySetting data)
	{
		DuelState.OnReceive(senderId, data);
	}

	private void OnReceive(long senderId, NShared.NMessage.NProtocol.BeginDuel data)
	{
		DuelState.OnReceive(senderId, data);
	}

	private void OnReceive(long senderId, DuelPing data)
	{
		DuelPong duelPong = new DuelPong();
		duelPong.GroupId = DuelState.GroupId;
		SingletonAuto<MessageLogHandler>.instance.SendToClient(senderId, duelPong);
	}

	private void OnReceive(long senderId, DuelPong data)
	{
		if (_pingData != null && _pingData.PlayerId == senderId && (data.GroupId == 0L || data.GroupId == _pingData.GroupId))
		{
			_pingData.PongReceived = true;
		}
	}
}
