using System;
using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class GuildArenaHistoryComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/GuildArena/GuildArenaHistoryComponent";

	public static PrefabAuto<GuildArenaHistoryComponent> PrefabAuto = new PrefabAuto<GuildArenaHistoryComponent>("Prefab2/Component/GuildArena/GuildArenaHistoryComponent");

	public UISprite SpriteWin;

	public UILabel LabelLeftNick;

	public UITexture heroTextureLeft;

	public UILabel LabelRightNick;

	public UITexture heroTextureRight;

	public UILabel LabelWin;

	public UILabel LabelTime;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
	}

	private string GetLocalKeyDefenseTeam(int deckIndex)
	{
		return LocalizationManager.GetStringDirect($"DEFENCE_TEAM_NUM{deckIndex}");
	}

	public void Init(GuildArenaMemberRecordInfo recordInfo, GuildArenaRecordType recordType)
	{
		if (recordInfo == null || recordInfo.LeftTeam == null || recordInfo.RightTeam == null)
		{
			return;
		}
		string localKeyDefenseTeam = GetLocalKeyDefenseTeam(recordInfo.DeckIndex);
		switch (recordType)
		{
		case GuildArenaRecordType.MyRecord:
		{
			UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
			if (userManager == null)
			{
				return;
			}
			string serverGroup = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo().ServerGroup;
			bool flag = true;
			if (recordInfo.LeftTeam.AccountId == userManager.Id && recordInfo.LeftTeam.ServerGroup == serverGroup)
			{
				flag = true;
			}
			else if (recordInfo.RightTeam.AccountId == userManager.Id && recordInfo.RightTeam.ServerGroup == serverGroup)
			{
				flag = false;
			}
			UIHelper.SetLabelText(LabelLeftNick, flag ? recordInfo.LeftTeam.Nick : $"{recordInfo.RightTeam.Nick}\r\n{localKeyDefenseTeam}");
			UIHelper.SetLabelText(LabelRightNick, flag ? $"{recordInfo.RightTeam.Nick}\r\n{localKeyDefenseTeam}" : recordInfo.LeftTeam.Nick);
			InitAvatar(recordInfo.LeftTeam, flag ? heroTextureLeft : heroTextureRight);
			InitAvatar(recordInfo.RightTeam, flag ? heroTextureRight : heroTextureLeft);
			if (flag)
			{
				string stringDirect3 = LocalizationManager.GetStringDirect((recordInfo.Win == 0) ? "GUILD_ARENA_ATTACK_LOSE" : "GUILD_ARENA_ATTACK_WIN");
				UIHelper.SetLabelText(LabelWin, stringDirect3);
				UIHelper.SetWidgetGrayScale(SpriteWin, recordInfo.Win == 0);
			}
			else
			{
				string stringDirect4 = LocalizationManager.GetStringDirect((recordInfo.Win == 0) ? "GUILD_ARENA_DEFENCE_WIN" : "GUILD_ARENA_DEFENCE_LOSE");
				UIHelper.SetLabelText(LabelWin, stringDirect4);
				UIHelper.SetWidgetGrayScale(SpriteWin, recordInfo.Win != 0);
			}
			break;
		}
		case GuildArenaRecordType.MyTeam:
		{
			UIHelper.SetLabelText(LabelLeftNick, recordInfo.LeftTeam.Nick);
			UIHelper.SetLabelText(LabelRightNick, $"{recordInfo.RightTeam.Nick}\r\n{localKeyDefenseTeam}");
			InitAvatar(recordInfo.LeftTeam, heroTextureLeft);
			InitAvatar(recordInfo.RightTeam, heroTextureRight);
			string stringDirect2 = LocalizationManager.GetStringDirect((recordInfo.Win == 0) ? "GUILD_ARENA_ATTACK_LOSE" : "GUILD_ARENA_ATTACK_WIN");
			UIHelper.SetLabelText(LabelWin, stringDirect2);
			UIHelper.SetWidgetGrayScale(SpriteWin, recordInfo.Win == 0);
			break;
		}
		case GuildArenaRecordType.EnemyTeam:
		{
			UIHelper.SetLabelText(LabelLeftNick, $"{recordInfo.RightTeam.Nick}\r\n{localKeyDefenseTeam}");
			UIHelper.SetLabelText(LabelRightNick, recordInfo.LeftTeam.Nick);
			InitAvatar(recordInfo.LeftTeam, heroTextureRight);
			InitAvatar(recordInfo.RightTeam, heroTextureLeft);
			string stringDirect = LocalizationManager.GetStringDirect((recordInfo.Win == 0) ? "GUILD_ARENA_DEFENCE_WIN" : "GUILD_ARENA_DEFENCE_LOSE");
			UIHelper.SetLabelText(LabelWin, stringDirect);
			UIHelper.SetWidgetGrayScale(SpriteWin, recordInfo.Win != 0);
			break;
		}
		}
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(recordInfo.UpdatedTime, out var o_dateTime))
		{
			UIHelper.EnableGameObject(LabelTime, enable: true);
			TimeSpan timeSpan = NGame2.NUI.NWindow.Common.GetUTCNow() - o_dateTime;
			if ((int)timeSpan.TotalDays >= 1)
			{
				UIHelper.SetLabelText(LabelTime, LocalizationManager.GetString(Match.MATCH_HISTORY_TIME_DAY, (int)timeSpan.TotalDays));
			}
			else if ((int)timeSpan.TotalHours >= 1)
			{
				UIHelper.SetLabelText(LabelTime, LocalizationManager.GetString(Match.MATCH_HISTORY_TIME_HOUR, (int)timeSpan.TotalHours));
			}
			else
			{
				UIHelper.SetLabelText(LabelTime, LocalizationManager.GetString(Match.MATCH_HISTORY_TIME_MINUTE, (int)timeSpan.TotalMinutes));
			}
		}
		else
		{
			UIHelper.EnableGameObject(LabelTime, enable: false);
		}
	}

	private void InitAvatar(GuildMemberServerInfo memberServerInfo, UITexture heroTexture)
	{
		string iconPath = UIAvatarHelper.GetIconPath(memberServerInfo.AvatarHeroIndex);
		UIHelper.SetBundleTexture(heroTexture.GetComponent<UIBundleLoader>(), iconPath);
	}
}
