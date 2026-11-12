using System.Collections.Generic;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class MatchBanPickRewardComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/Match/MatchBanPickRewardComponent";

	public static PrefabAuto<MatchBanPickRewardComponent> PrefabAuto = new PrefabAuto<MatchBanPickRewardComponent>("Resources/Prefab2/Component/Match/MatchBanPickRewardComponent");

	public UISprite BGSprite;

	public UILabel RewardName;

	public UILabel RewardDesc;

	public UISimpleGrid IconPostion;

	private List<Component> IconComponents;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.ReleaseAndClearList(IconComponents);
	}

	public void Init(MatchBanPickRewardData rewardData)
	{
		UIHelper.SetLabelText(RewardName, LocalizationManager.GetStringDirect(rewardData.Name));
		UIHelper.SetLabelText(RewardDesc, LocalizationManager.GetStringDirect(rewardData.Desc));
		if (rewardData.RewardIndex > 0)
		{
			IconComponents = UIHelper.CreateRewardItemIconComponentList(rewardData.RewardIndex, IconPostion, includeCurrency: true);
		}
		UIHelper.RepositionGrid(IconPostion);
		if ((bool)RewardDesc)
		{
			RewardDesc.UpdateAnchors();
		}
		if ((bool)BGSprite)
		{
			BGSprite.UpdateAnchors();
			BGSprite.Update();
		}
	}
}
