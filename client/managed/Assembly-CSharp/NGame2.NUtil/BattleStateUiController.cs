using System;
using NGame2.NBattleField;
using NGame2.NBattleView;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NUI;
using NGame2.NUI.NComponent2.NBattle;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUtil;

public class BattleStateUiController
{
	private int _creatureUid;

	private UISimpleGrid _buffGrid;

	private IncreaseDic<string, BuffIconComponent> _buffDic = new IncreaseDic<string, BuffIconComponent>(16);

	private Holder _receiverHolder = new Holder();

	public void Open(int creatureUid, UISimpleGrid buffGrid)
	{
		_creatureUid = creatureUid;
		_buffGrid = buffGrid;
		CreatureVerifier verifier = new CreatureVerifier(_creatureUid);
		_receiverHolder.AddByCreatureUid<CallbackMessage<ChangeState>>(verifier, OnMessageFast, _creatureUid);
	}

	public void Reset()
	{
		_receiverHolder.Clear();
		_buffGrid = null;
		for (int i = 0; i < _buffDic.Count; i++)
		{
			FactoryBase.ReleaseComponent(_buffDic.GetValueByIndex(i));
		}
		_buffDic.Clear();
		_creatureUid = 0;
	}

	public void ChainUpdate()
	{
		for (int i = 0; i < _buffDic.Count; i++)
		{
			_buffDic.GetValueByIndex(i).ChainUpdate();
		}
	}

	private void OnMessageFast(CallbackMessage<ChangeState> message)
	{
		if (!message.IsValidMessage() || message.Callback.CreatureUid != _creatureUid)
		{
			return;
		}
		StateData data = GameRoot.Instance.StateDataContainer.GetData(message.Callback.ChangedStateType);
		if (data != null && (String_Extension_Method.IsNullOrEmpty(data.IconPath) || !string.Equals(data.IconPath, "None", StringComparison.OrdinalIgnoreCase)) && CheckBuffOption())
		{
			if (message.Callback.StateAdded)
			{
				AddState(message.Callback);
			}
			else
			{
				RemoveState(message.Callback);
			}
		}
	}

	private bool CheckBuffOption()
	{
		if (!SystemOptionData.ShowBuff)
		{
			return false;
		}
		BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(_creatureUid);
		if (creatureCache == null || creatureCache.Uid == 0 || creatureCache.Player == null)
		{
			return false;
		}
		if (creatureCache.Player.IsMe || creatureCache.Player.IsAlly)
		{
			if (!SystemOptionData.ShowBuffAlly)
			{
				return false;
			}
		}
		else if (!SystemOptionData.ShowBuffEnemy)
		{
			return false;
		}
		return true;
	}

	private void AddState(ChangeState callback)
	{
		BuffIconComponent vALUE = _buffDic.GETVALUE(callback.ChangedStateType, null);
		if (vALUE == null)
		{
			vALUE = UIHelper.AddGridItem(_buffGrid, BuffIconComponent.PrefabAuto);
			_buffDic.Add(callback.ChangedStateType, vALUE);
			UIHelper.RepositionGrid(_buffGrid);
			vALUE.Open(callback);
		}
		else if (callback.StackDurationRuleType != StateStackDurationRuleType.Max || vALUE.DurationMs - vALUE.ElapsedTick <= callback.DurationMs)
		{
			vALUE.Open(callback);
		}
	}

	private void RemoveState(ChangeState callback)
	{
		BuffIconComponent vALUE = _buffDic.GETVALUE(callback.ChangedStateType, null);
		if (!(vALUE == null) && vALUE.OperationUid == callback.OperationUid)
		{
			FactoryBase.ReleaseComponent(vALUE);
			_buffDic.RemoveSlow(callback.ChangedStateType);
			UIHelper.RepositionGrid(_buffGrid);
		}
	}
}
