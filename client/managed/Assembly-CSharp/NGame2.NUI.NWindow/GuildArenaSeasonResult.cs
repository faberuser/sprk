using NGame2.NGuild;
using NGameAsset;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildArenaSeasonResult : BaseWindow
{
	public interface Listener
	{
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GuildArena/GuildArenaSeasonResult";

	public UILabel Label_SeasionEndTitle;

	public UILabel Label_Rank;

	public UILabel Label_GuildName;

	public UILabel Label_Level;

	public UILabel Label_Score;

	public UILabel Label_Record;

	public UILabel Label_Ruby;

	public UILabel Label_Point;

	public GameObject RubyRoot;

	public UITexture Text_GuildLogo;

	public UITexture Text_GuildBg;

	public UITexture Text_CountryFlag;

	public UIGrid Grid_Reward;

	public UISpriteSelector Sprite_TierRank;

	private Listener _listener;

	private GuildArenaSessionInfo _sessionInfo;

	public const string UI_GuildArena_BattleEnd = "UI2.Guild.GuildBattleEnd";

	public void Open(Listener listener)
	{
		_listener = listener;
		_sessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSessionInfo();
		Open(delegate
		{
			Init();
		}, null);
	}

	public void Init()
	{
		InitLabels();
		InitTexttures();
		PlaySound();
	}

	private void InitLabels()
	{
		string guildArenaSeasonDisplayName = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSeasonDisplayName();
		UIHelper.SetLabelText(Label_SeasionEndTitle, LocalizationManager.GetStringDirect("GUILD_ARENA_SEASON_END", guildArenaSeasonDisplayName));
		GuildArenaRankerInfo myGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo();
		if (myGuildInfo == null)
		{
			return;
		}
		string text = myGuildInfo.Name;
		UIHelper.SetLabelText(Label_GuildName, text);
		string text2 = $"Lv.{GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Level}";
		UIHelper.SetLabelText(Label_Level, text2);
		string stringDirect = LocalizationManager.GetStringDirect("HONOR_RANK_NUM", myGuildInfo.Rank);
		UIHelper.SetLabelText(Label_Rank, (myGuildInfo.Rank > 0) ? stringDirect : "-");
		int seasonScore = myGuildInfo.SeasonScore;
		string text3 = $"{seasonScore}{LocalizationManager.GetString(Match.SCORE_POSTFIX)}";
		UIHelper.SetLabelText(Label_Score, (seasonScore > 0) ? text3 : "-");
		string stringDirect2 = LocalizationManager.GetStringDirect("GUILD_ARENA_RECORD_INFO", myGuildInfo.SeasonWin, myGuildInfo.SeasonDraw, myGuildInfo.SeasonLose);
		bool flag = myGuildInfo.SeasonWin == 0 && myGuildInfo.SeasonDraw == 0 && myGuildInfo.SeasonLose == 0;
		UIHelper.SetLabelText(Label_Record, (!flag) ? stringDirect2 : "-");
		GuildArenaSeasonType seasonType = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.SeasonType;
		int tierIndex = myGuildInfo.TierIndex;
		GuildArenaTierData data = GameRoot.Instance.GuildArenaTierDataContainer.GetData(myGuildInfo.TierIndex);
		if (data == null)
		{
			return;
		}
		if (data.InTopTier)
		{
			tierIndex = 0;
		}
		GuildArenaSeasonRewardData guildArenaSeasonRewardData = GameRoot.Instance.GuildArenaSeasonRewardDataContainer.GetGuildArenaSeasonRewardData(tierIndex, myGuildInfo.Rank);
		if (guildArenaSeasonRewardData != null)
		{
			int num = guildArenaSeasonRewardData.SeasonRubyReward;
			int num2 = guildArenaSeasonRewardData.SeasonPointReward;
			if (seasonType == GuildArenaSeasonType.Free)
			{
				num = guildArenaSeasonRewardData.FreeSeasonRubyReward;
				num2 = guildArenaSeasonRewardData.FreeSeasonPointReward;
			}
			UIHelper.EnableGameObject(RubyRoot, seasonType == GuildArenaSeasonType.Regular);
			Grid_Reward.Reposition();
			UIHelper.SetLabelText(Label_Ruby, num);
			UIHelper.SetLabelText(Label_Point, num2);
		}
	}

	private void InitTexttures()
	{
		GuildArenaRankerInfo myGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo();
		if (myGuildInfo != null)
		{
			AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2((myGuildInfo.Logo <= 0) ? 1 : myGuildInfo.Logo);
			UIHelper.ResetPlaneAssetTextureAsync(this, Text_GuildLogo, guildLogoIcon);
			byte b = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Back;
			if (b <= 0 || b > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
			{
				b = 1;
			}
			AssetTexture guildBackGround2D = UIHelper.GetGuildBackGround2D(b);
			UIHelper.ResetPlaneAssetTextureAsync(this, Text_GuildBg, guildBackGround2D);
			string countryCode = myGuildInfo.CountryCode;
			CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
			if (dataByCode != null)
			{
				AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
				UIHelper.ResetPlaneAssetTextureAsync(this, Text_CountryFlag, plainTexture);
			}
			Sprite_TierRank.Select(GetTierRank(myGuildInfo.TierIndex, myGuildInfo.Rank));
		}
	}

	private string GetTierRank(int tierIndex, int rank)
	{
		if (tierIndex >= 80 && rank == 1)
		{
			return "Champion";
		}
		if (tierIndex >= 80)
		{
			return "Challenger";
		}
		if (tierIndex >= 70)
		{
			return "RoyalMaster";
		}
		if (tierIndex >= 60)
		{
			return "Master";
		}
		if (tierIndex >= 50)
		{
			return "Diamond";
		}
		if (tierIndex >= 40)
		{
			return "Platinum";
		}
		if (tierIndex >= 30)
		{
			return "Gold";
		}
		if (tierIndex >= 20)
		{
			return "Silver";
		}
		return "Bronze";
	}

	private void PlaySound()
	{
		SoundStock stock = new SoundStock
		{
			Code = "UI2.Guild.GuildBattleEnd"
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
	}
}
