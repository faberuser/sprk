using System;
using NGame2.NGuild;
using NGame2.NParty;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NParty;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class RaidReturnState : LobbyScene2.IReturnState
{
	public RaidData _raidData;

	public string BgmCode => string.Empty;

	public int[] AffixIndices { get; set; }

	void LobbyScene2.IReturnState.Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		if (!RaidPartySetting.IsMultiRaid)
		{
			ExecuteForSingle(info, onEnd);
		}
		else
		{
			if (!GlobalAutoBehaviour<PartyManager>.instance.IsValidParty())
			{
				return;
			}
			PartyInfo partyInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
			if (GlobalAutoBehaviour<PartyManager>.instance.IsPartyRoomMaster())
			{
				GlobalAutoBehaviour<PartyManager>.instance.SendRaidBattleEndToServer();
			}
			GlobalAutoBehaviour<PartyManager>.instance.ChangeMemberStatus(MemberStatus.PartyRoom);
			GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.OnReturnRoom();
			if (_raidData != null && _raidData.Type == RaidType.Challenge)
			{
				SceneAutoBehaviour<RaidManagement>.instance.OpenChallengeRaidRanking();
				SceneAutoBehaviour<RaidManagement>.instance.RefreshChallengeRaidInfo();
				if (!SceneAutoBehaviour<RaidManagement>.instance.IsAbleEnter())
				{
					GlobalAutoBehaviour<PartyManager>.instance.SendLeaveParty();
					SceneAutoBehaviour<RaidManagement>.instance.ShowPopupBattleEnd();
					return;
				}
			}
			if ((_raidData != null && _raidData.Type == RaidType.GuildSuppress) || _raidData.Type == RaidType.GuildSuppressSingle)
			{
				UserGuildManager myGuildManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager;
				if (myGuildManager == null)
				{
					return;
				}
				GuildSuppressManager guildSuppressManager = myGuildManager.GuildSuppressManager;
				if (guildSuppressManager == null)
				{
					return;
				}
				int level = _raidData.Level;
				if (_raidData.Type == RaidType.GuildSuppressSingle)
				{
					level = GameRoot.Instance.GuildSuppressSessionDataContainer.GetMultiLevel(_raidData.Level);
				}
				GuildSuppressPlayInfo guildSuppressPlayInfo = guildSuppressManager.GetGuildSuppressPlayInfo(_raidData.Index, level);
				if (guildSuppressPlayInfo == null)
				{
					return;
				}
				int dungeonIndexByLevel = GameRoot.Instance.GuildSuppressSessionDataContainer.GetDungeonIndexByLevel(guildSuppressPlayInfo.SessionIndex, guildSuppressPlayInfo.Level);
				if (guildSuppressPlayInfo.IsDead())
				{
					bool killedByMe = guildSuppressPlayInfo.KilledByMe;
					GlobalAutoBehaviour<PartyManager>.instance.SendLeaveParty();
					SceneAutoBehaviour<GuildSuppressManagement>.instance.OpenGuildSuppressDungeonList(guildSuppressPlayInfo.RaidIndex, killedByMe, guildSuppressPlayInfo);
					return;
				}
				GuildSuppressDungeonData data = GameRoot.Instance.GuildSuppressDungeonDataContainer.GetData(dungeonIndexByLevel);
				if (data != null)
				{
					SceneAutoBehaviour<GuildSuppressManagement>.instance.OpenGuildSuppressPartySetting(data, _raidData.Index, _raidData.Level, SceneAutoBehaviour<GuildSuppressManagement>.instance.OpenGuildSuppressDungeonList);
				}
			}
			else
			{
				SceneAutoBehaviour<RaidManagement>.instance.OpenRaidPartySetting(partyInfo.RaidIndex, partyInfo.RaidLevel);
				Vespa_System_Action_Extension_Method.fire(onEnd);
			}
		}
	}

	private void ExecuteForSingle(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenNewbieShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Beginner.ToString());
		}
		if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenEventShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Event.ToString());
		}
		else
		{
			if (_raidData != null && _raidData.Type == RaidType.ChallengeSingle)
			{
				SceneAutoBehaviour<RaidManagement>.instance.OpenChallengeRaidRanking();
				SceneAutoBehaviour<RaidManagement>.instance.RefreshChallengeRaidInfo();
				if (!SceneAutoBehaviour<RaidManagement>.instance.IsAbleEnter())
				{
					SceneAutoBehaviour<RaidManagement>.instance.ShowPopupBattleEnd();
					return;
				}
			}
			SceneAutoBehaviour<RaidManagement>.instance.OpenSingleRaidPartySetting(_raidData.Index, _raidData.Level, AffixIndices, isReturnToBattle: true);
		}
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
