using NGame2.NGuild;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildArenaSessionResult : BaseWindow
{
	public interface Listener
	{
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GuildArena/GuildArenaSessionResult";

	private Animator _animator;

	public UILabel Label_GuildName;

	public UILabel Label_Score;

	public UILabel Label_NickName;

	public UILabel Label_Record;

	public UILabel Label_AddedRecord;

	public UILabel Label_Point;

	public UITexture Text_GuildLogo;

	public UITexture Text_GuildBg;

	public UITexture Text_CountryFlag;

	public UIBundleLoader Texture_AvatarHero;

	private Listener _listener;

	private GuildArenaSessionInfo _sessionInfo;

	public const string UI_GuildArena_BattleLose = "UI2.Guild.GuildBattleLose";

	public const string UI_GuildArena_BattleWin = "UI2.Guild.GuildBattleWin";

	public void Open(Listener listener)
	{
		_listener = listener;
		Open(delegate
		{
			Init();
		}, null);
	}

	public override void Close()
	{
		if (_sessionInfo.State == GuildArenaSessionState.Ended && _sessionInfo.IsLastSession == 1)
		{
			SceneAutoBehaviour<GuildArenaManagement>.instance.OpenGuildArenaSeasonResult();
		}
		base.Close();
	}

	public void Init()
	{
		_sessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSessionInfo();
		_animator = GetComponentInChildren<Animator>();
		InitLabels();
		InitTexttures();
		PlayResultAnimation();
	}

	private void InitLabels()
	{
		GuildArenaRankerInfo myGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo();
		if (myGuildInfo == null)
		{
			return;
		}
		string text = myGuildInfo.Name;
		int sessionScore = myGuildInfo.SessionScore;
		int seasonScore = myGuildInfo.SeasonScore;
		UIHelper.SetLabelText(Label_GuildName, text);
		UIHelper.SetLabelText(Label_Score, sessionScore);
		UIHelper.SetLabelText(Label_Record, seasonScore);
		int addSeasonScore = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetAddSeasonScore();
		string arg = ((addSeasonScore > 0) ? ("+" + addSeasonScore) : addSeasonScore.ToString());
		UIHelper.SetLabelText(Label_AddedRecord, $"({arg})");
		GuildMemberServerInfo mVPMember = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMVPMember();
		if (mVPMember != null)
		{
			string nick = mVPMember.Nick;
			UIHelper.SetLabelText(Label_NickName, nick);
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("GuildArenaSessionWinReward");
			int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("GuildArenaSessionLoseReward");
			int num = valueInt;
			if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetWinResult() == 0)
			{
				num = valueInt2;
			}
			UIHelper.SetLabelText(Label_Point, num);
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
			GuildMemberServerInfo mVPMember = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMVPMember();
			if (mVPMember != null)
			{
				string iconPath = UIAvatarHelper.GetIconPath(mVPMember.AvatarHeroIndex);
				UIHelper.SetBundleTexture(Texture_AvatarHero, iconPath);
			}
		}
	}

	private void PlayResultAnimation()
	{
		int winResult = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetWinResult();
		if (_animator != null)
		{
			SoundStock stock = default(SoundStock);
			if (winResult == 1)
			{
				stock.Code = "UI2.Guild.GuildBattleWin";
				_animator.Play("Anim_GVG_Result_Victory");
			}
			if (winResult == 0)
			{
				stock.Code = "UI2.Guild.GuildBattleLose";
				_animator.Play("Anim_GVG_Result_Defeat");
			}
			if (winResult == 2)
			{
				stock.Code = "UI2.Guild.GuildBattleWin";
				_animator.Play("Anim_GVG_Result_Draw");
			}
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
		}
	}

	public void OnClickOpenHistory()
	{
		SceneAutoBehaviour<GuildArenaManagement>.instance.OpenGuildArenaHistory();
	}
}
