using System.Collections.Generic;
using NGame2.NContentsDefine;
using NGame2.NUI.NManager;
using NGameAsset;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class MatchRankerComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public interface Listener
	{
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Match/MatchRankerComponent";

	public const string ServerTotalPrefabPath = "Resources/Prefab2/Component/Match/MatchServerTotalRankerComponent";

	public static PrefabAuto<MatchRankerComponent> PrefabAuto = new PrefabAuto<MatchRankerComponent>("Resources/Prefab2/Component/Match/MatchRankerComponent");

	public static PrefabAuto<MatchRankerComponent> PrefabServerTotalAuto = new PrefabAuto<MatchRankerComponent>("Resources/Prefab2/Component/Match/MatchServerTotalRankerComponent");

	public UITexture PortraitTexture;

	public UIBundleLoader BundleTexPortrait;

	public UILabel Label_Rank;

	public UILabel Label_Name;

	public UISpriteSelector RankSpriteSelector;

	public UILabel Label_WorldRankScore;

	public UILabel Label_ServerRankScore;

	public UITexture Texture_CountryFlag;

	public UISimpleGrid HeroPortraitGrid;

	public GameObject WorldRankScoreRoot;

	public GameObject ServerRankScoreRoot;

	private Listener _listener;

	private MatchPvpType _pvpType;

	private List<HeroPortraitTextureName> _HeroPortraitComponentList = new List<HeroPortraitTextureName>();

	public MatchRankerInfo RankerInfo { get; private set; }

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		ClearRankerInfo();
	}

	private void ResetHeroPortraitList()
	{
		FactoryBase.ReleaseAndClearList(_HeroPortraitComponentList);
	}

	public void SetPortraitTexture(int avatarHeroIndex)
	{
		string iconPath = UIAvatarHelper.GetIconPath(avatarHeroIndex);
		UIHelper.SetBundleTexture(BundleTexPortrait, iconPath);
	}

	public bool Open(MatchRankerInfo rankerInfo, MatchPvpType pvpType, Listener listener, bool isGlobal)
	{
		ClearRankerInfo();
		ResetHeroPortraitList();
		RankerInfo = rankerInfo;
		_listener = listener;
		_pvpType = pvpType;
		if (rankerInfo == null || (rankerInfo != null && rankerInfo.UserInfo == null))
		{
			return false;
		}
		UIHelper.SetLabelText(Label_Name, rankerInfo.UserInfo.Nick);
		VisualTierRank visualTierRank = VisualTierRank.CreateFromServer(rankerInfo.Rank);
		SetRank(visualTierRank, rankerInfo.MatchScore);
		SetPortraitTexture(rankerInfo.UserInfo.AvatarHeroIndex);
		SetScore(isGlobal, rankerInfo.MatchScore, rankerInfo.CountryCode);
		if (rankerInfo.HeroInfos != null)
		{
			DeckHeroInfo[] heroInfos = rankerInfo.HeroInfos;
			foreach (DeckHeroInfo deckHeroInfo in heroInfos)
			{
				if (deckHeroInfo.Index != 0)
				{
					HeroPortraitTextureName heroPortraitTextureName = UIHelper.AddGridItem(HeroPortraitGrid, HeroPortraitTextureName.PrefabAuto);
					heroPortraitTextureName.Open(deckHeroInfo.Index, deckHeroInfo.Star, 0, printName: false);
					_HeroPortraitComponentList.Add(heroPortraitTextureName);
				}
			}
			UIHelper.RepositionGrid(HeroPortraitGrid);
		}
		return true;
	}

	public void OnClickComponent()
	{
		if (RankerInfo != null && RankerInfo.UserInfo != null)
		{
			InspectUserManagement.InspectUserData inspectUser = new InspectUserManagement.InspectUserData(RankerInfo.UserInfo, InspectUserCategory.Arena);
			SceneAutoBehaviour<InspectUserManagement>.instance.OpenMenuPopup(inspectUser);
		}
	}

	private void SetRank(VisualTierRank visualTierRank, int score)
	{
		if (visualTierRank.Value <= 0 || score <= 0)
		{
			UIHelper.SetLabelText(Label_Rank, "-");
			UIHelper.EnableGameObject(RankSpriteSelector, enable: false);
			UIHelper.EnableGameObject(Label_Rank, enable: true);
		}
		else if (visualTierRank.Value <= 3)
		{
			if (RankSpriteSelector != null)
			{
				RankSpriteSelector.Select(visualTierRank.Value.ToString());
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: true);
			UIHelper.EnableGameObject(Label_Rank, enable: false);
		}
		else
		{
			string rankLabelText = UIHelper.GetRankLabelText(visualTierRank);
			UIHelper.SetLabelText(Label_Rank, rankLabelText);
			UIHelper.EnableGameObject(RankSpriteSelector, enable: false);
			UIHelper.EnableGameObject(Label_Rank, enable: true);
		}
	}

	private void SetScore(bool isGlobal, int matchScore, string countryCode)
	{
		UIHelper.EnableGameObject(WorldRankScoreRoot, isGlobal);
		UIHelper.EnableGameObject(ServerRankScoreRoot, !isGlobal);
		int num = ((matchScore >= 0) ? matchScore : 0);
		string text = LocalizationManager.GetString(Match.SCORE_POSTFIX);
		int num2 = num;
		string text2 = Vespa_Int_Extension_Method.ToCommaString(num2) + text;
		if (isGlobal)
		{
			UIHelper.SetLabelText(Label_WorldRankScore, text2);
			CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
			if (dataByCode != null)
			{
				AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
				UIHelper.ResetPlaneAssetTextureAsync(this, Texture_CountryFlag, plainTexture);
			}
		}
		else
		{
			UIHelper.SetLabelText(Label_ServerRankScore, text2);
		}
	}

	private void ClearRankerInfo()
	{
		UIHelper.EnableGameObject(WorldRankScoreRoot, enable: false);
		UIHelper.EnableGameObject(ServerRankScoreRoot, enable: true);
		UIHelper.SetLabelText(Label_WorldRankScore, string.Empty);
		UIHelper.SetLabelText(Label_ServerRankScore, string.Empty);
		UIHelper.SetLabelText(Label_Name, string.Empty);
		UIHelper.SetLabelText(Label_Rank, string.Empty);
		UIHelper.ClearPlaneAssetTexture(Texture_CountryFlag);
	}
}
