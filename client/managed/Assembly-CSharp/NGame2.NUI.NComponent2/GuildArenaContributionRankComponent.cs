using NGame2.NAccount;
using NGameAsset;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GuildArenaContributionRankComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public interface Listener
	{
		void OnClickComponent(long accountId);
	}

	public const string PrefabPath = "Prefab2/Component/GuildArena/GuildArenaContributionRankComponent";

	public static PrefabAuto<GuildArenaContributionRankComponent> PrefabAuto = new PrefabAuto<GuildArenaContributionRankComponent>("Prefab2/Component/GuildArena/GuildArenaContributionRankComponent");

	public UITexture Texure_Hero;

	public UITexture Texure_CountryFlag;

	public UILabel RankLabel_Other;

	public UILabel RankLabel_Me;

	public UISpriteSelector RankSpriteSelector;

	public UILabel Label_Nick;

	public UILabel Label_Point;

	public UILabel Label_Record;

	private GuildRaidMemberScoreInfo ScoreInfo;

	private GuildMemberInfo GuildMemberInfo;

	public GameObject MVPRoot;

	private Listener _Listener;

	private GuildArenaMemberScoreInfo _memberScoreInfo;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		Clear();
	}

	public void Open(GuildArenaMemberScoreInfo memberScoreInfo)
	{
		_memberScoreInfo = memberScoreInfo;
		InitHeroImage();
		InitLabels();
		InitRank();
		InitMVP();
		InitCountryFlag();
	}

	private void InitLabels()
	{
		if (_memberScoreInfo != null)
		{
			string nick = _memberScoreInfo.Nick;
			int sessionScore = _memberScoreInfo.SessionScore;
			UIHelper.SetLabelText(Label_Nick, nick);
			UIHelper.SetLabelText(Label_Point, string.Format("+ {0}", (sessionScore >= 0) ? sessionScore.ToString() : "-"));
			string stringDirect = LocalizationManager.GetStringDirect("GUILD_ARENA_ATTACK_SUCCESS_COUNT");
			string stringDirect2 = LocalizationManager.GetStringDirect("DISPATCH_BATTLE.DISPATCH_COUNT", _memberScoreInfo.MatchWinCount);
			string stringDirect3 = LocalizationManager.GetStringDirect("GUILD_ARENA_DEFENCE_SUCCESS_COUNT");
			string stringDirect4 = LocalizationManager.GetStringDirect("DISPATCH_BATTLE.DISPATCH_COUNT", _memberScoreInfo.DefenceWinCount);
			string text = $"{stringDirect} : {stringDirect2} / {stringDirect3} : {stringDirect4}";
			if (_memberScoreInfo.MatchWinCount == 0 && _memberScoreInfo.DefenceWinCount == 0)
			{
				text = "-";
			}
			UIHelper.SetLabelText(Label_Record, text);
		}
	}

	private void InitMVP()
	{
		if (_memberScoreInfo != null)
		{
			UIHelper.EnableGameObject(MVPRoot, _memberScoreInfo.ScoreRank == 1);
		}
	}

	private void InitHeroImage()
	{
		if (_memberScoreInfo != null)
		{
			string iconPath = UIAvatarHelper.GetIconPath(_memberScoreInfo.AvatarHeroIndex);
			if (!String_Extension_Method.IsNullOrEmpty(iconPath))
			{
				AssetTexture plainTexture = UIHelper.GetPlainTexture(iconPath);
				UIHelper.ResetPlaneAssetTextureAsync(this, Texure_Hero, plainTexture);
			}
		}
	}

	private void InitRank()
	{
		if (_memberScoreInfo == null)
		{
			return;
		}
		bool flag = _memberScoreInfo.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		int scoreRank = _memberScoreInfo.ScoreRank;
		if (scoreRank >= 1 && scoreRank <= 3)
		{
			if (RankSpriteSelector != null)
			{
				RankSpriteSelector.Select(scoreRank.ToString());
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: true);
			UIHelper.EnableGameObject(RankLabel_Me, enable: false);
			UIHelper.EnableGameObject(RankLabel_Other, enable: false);
		}
		else
		{
			if (flag)
			{
				UIHelper.SetLabelText(RankLabel_Me, (scoreRank == 0) ? "-" : scoreRank.ToString());
			}
			else
			{
				UIHelper.SetLabelText(RankLabel_Other, (scoreRank == 0) ? "-" : scoreRank.ToString());
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: false);
			UIHelper.EnableGameObject(RankLabel_Me, flag);
			UIHelper.EnableGameObject(RankLabel_Other, !flag);
		}
	}

	private void InitCountryFlag()
	{
		if (_memberScoreInfo != null)
		{
			string countryCode = _memberScoreInfo.CountryCode;
			CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
			if (dataByCode != null)
			{
				AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
				UIHelper.ResetPlaneAssetTextureAsync(this, Texure_CountryFlag, plainTexture);
			}
		}
	}

	private void Clear()
	{
		UIHelper.ClearPlaneAssetTexture(Texure_Hero);
		UIHelper.ClearPlaneAssetTexture(Texure_CountryFlag);
	}
}
