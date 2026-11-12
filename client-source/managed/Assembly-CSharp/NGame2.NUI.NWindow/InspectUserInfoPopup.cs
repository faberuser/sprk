using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class InspectUserInfoPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/InspectUserInfoPopup";

	private Action _onDetailView;

	public UILabel LabelName;

	public UITexture PortraitTexture;

	public UILabel LabelLevel;

	public UILabel LabelGuildName;

	public UILabel LabelEmptyDeck;

	public UISimpleGrid HeroPortraitGrid;

	public UISimpleGrid InspectUserComponentGrid;

	public UITexture Texture_GuildLogo;

	public UITexture Texture_GuildBack;

	public UILabel LabelContentGeneral;

	private List<HeroPortraitComponent> heroPortraitList = new List<HeroPortraitComponent>();

	private List<InspectUserComponent> userComponentList = new List<InspectUserComponent>();

	public static void Open(InspectUserCategory status, CachedAccountInfo accountInfo, DeckInfo deckInfo, GuildInfo guildInfo, int rank, Action onDetailView)
	{
		WindowPrefab<InspectUserInfoPopup> windowPrefab = new WindowPrefab<InspectUserInfoPopup>("Resources/Prefab2/Window/MessagePopup/InspectUserInfoPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(status, accountInfo, deckInfo, guildInfo, rank, onDetailView);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void ResetPortraitList()
	{
		FactoryBase.ReleaseAndClearList(heroPortraitList);
	}

	private void ResetUserComponentList()
	{
		FactoryBase.ReleaseAndClearList(userComponentList);
	}

	public void SetPortraitTexture(int avatarHeroIndex)
	{
		AssetTexture plainTexture = UIHelper.GetPlainTexture(UIAvatarHelper.GetBattlePortraitPath(avatarHeroIndex));
		UIHelper.ResetPlaneAssetTextureAsync(this, PortraitTexture, plainTexture);
	}

	private void OpenInternal(InspectUserCategory status, CachedAccountInfo accountInfo, DeckInfo deckInfo, GuildInfo guildInfo, int rank, Action onDetailView)
	{
		base.Open();
		_onDetailView = onDetailView;
		CachedUserInfo userInfo = accountInfo.UserInfo;
		SetPortraitTexture(userInfo.AvatarHeroIndex);
		UIHelper.SetLabelText(LabelName, userInfo.Nick);
		UIHelper.SetLabelText(LabelLevel, "Lv." + userInfo.TeamLevel);
		if (guildInfo != null)
		{
			UIHelper.SetLabelText(LabelGuildName, guildInfo.Name);
			AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(guildInfo.Logo);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_GuildLogo, guildLogoIcon);
			int num = guildInfo.Back;
			if (num <= 0 || num > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
			{
				num = 1;
			}
			AssetTexture guildBackGround2D = UIHelper.GetGuildBackGround2D(num);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_GuildBack, guildBackGround2D);
		}
		else
		{
			UIHelper.SetLabelText(LabelGuildName, string.Empty);
			UIHelper.ClearPlaneAssetTexture(Texture_GuildLogo);
			UIHelper.ClearPlaneAssetTexture(Texture_GuildBack);
		}
		UIHelper.EnableGameObject(LabelEmptyDeck, deckInfo == null);
		if (deckInfo != null)
		{
			for (int i = 0; i < 4; i++)
			{
				CachedHeroInfo value = null;
				if (accountInfo.HeroInfos.TryGetValue(deckInfo.GetHeroIndex(i), out value) && value != null)
				{
					HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroPortraitGrid, HeroPortraitComponent.PrefabAuto);
					heroPortraitComponent.Open(value, null);
					heroPortraitList.Add(heroPortraitComponent);
				}
			}
		}
		UIHelper.RepositionGrid(HeroPortraitGrid);
		InspectUserData[] categoryData = GameRoot.Instance.InspectUserDataContainer.GetCategoryData(status);
		foreach (InspectUserData inspectUserData in categoryData)
		{
			if (inspectUserData.Type == InspectUserType.Title)
			{
				UIHelper.SetLabelText(LabelContentGeneral, LocalizationManager.GetStringDirect(inspectUserData.StringKey));
				continue;
			}
			InspectUserComponent inspectUserComponent = UIHelper.AddGridItem(InspectUserComponentGrid, InspectUserComponent.PrefabAuto);
			inspectUserComponent.Open(inspectUserData, accountInfo, rank);
			userComponentList.Add(inspectUserComponent);
		}
		UIHelper.RepositionGrid(InspectUserComponentGrid);
	}

	public override void Reset()
	{
		base.Reset();
		ResetPortraitList();
		ResetUserComponentList();
	}

	public void ClickCancel()
	{
		Close();
	}

	public void ClickDetailView()
	{
		Action onDetailView = _onDetailView;
		Close();
		Vespa_System_Action_Extension_Method.fire(onDetailView);
	}
}
