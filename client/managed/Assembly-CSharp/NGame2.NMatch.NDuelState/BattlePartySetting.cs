using NGame2.NMessageLog;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NMatch.NDuelState;

public class BattlePartySetting : StateBase
{
	private float _battlePartySettingClosedTime;

	public BattlePartySetting(long vsPlayerId, long groupId)
		: base(vsPlayerId, groupId)
	{
	}

	public override void ChainUpdate()
	{
		if (SprkHonorDuel.SkipPartyScreen(this))
		{
			SprkHonorDuel.SubmitParty(this);
			return;
		}
		base.ChainUpdate();
		if (!SceneAutoBehaviour<DuelManagement>.instance.IsOpened<NGame2.NUI.NWindow.BattlePartySetting>())
		{
			_battlePartySettingClosedTime += Time.deltaTime;
			if (_battlePartySettingClosedTime > 1f)
			{
				SceneAutoBehaviour<DuelManagement>.instance.CancelDuel();
				SingletonUpdater<DuelManager>.instance.CancelCurrentDuel();
			}
		}
	}

	public override void OnBegin()
	{
		if (!SprkHonorDuel.SkipPartyScreen(this))
		{
			SceneAutoBehaviour<DuelManagement>.instance.OpenBattlePartySetting(base.VsPlayerId, base.GroupId);
		}
	}

	public override void CompletePartySetting(long groupId, int[] heroIndices, int leaderHeroIndex)
	{
		if (groupId != base.GroupId)
		{
			base.CompletePartySetting(groupId, heroIndices, leaderHeroIndex);
			return;
		}
		CompletePartySetting completePartySetting = new CompletePartySetting();
		completePartySetting.GroupId = base.GroupId;
		SingletonAuto<MessageLogHandler>.instance.SendToClient(base.VsPlayerId, completePartySetting);
		SingletonUpdater<DuelManager>.instance.ChangeState(this, new CompleteMySetting(base.VsPlayerId, groupId, heroIndices, leaderHeroIndex));
	}

	public override void OnReceive(long senderId, CompletePartySetting data)
	{
		SingletonUpdater<DuelManager>.instance.ChangeState(this, new CompleteVsPlayerSetting(base.VsPlayerId, base.GroupId));
	}
}
