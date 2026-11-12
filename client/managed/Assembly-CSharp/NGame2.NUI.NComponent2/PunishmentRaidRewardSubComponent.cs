using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUtil;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PunishmentRaidRewardSubComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Raid/PunishmentRaidRewardSubComponent";

	public static PrefabAuto<PunishmentRaidRewardSubComponent> PrefabAuto = new PrefabAuto<PunishmentRaidRewardSubComponent>("Prefab2/Component/Raid/PunishmentRaidRewardSubComponent");

	public UITexture MonsterTexture;

	public UIBaseGrid Grid_RewardItem;

	public UISprite ClearedSprite;

	private List<Component> _rewardCompList = new List<Component>();

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		ClearItemIconList();
		UIPanel[] componentsInChildren = GetComponentsInChildren<UIPanel>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(componentsInChildren); i++)
		{
			UIPanel uIPanel = componentsInChildren[i];
			if (0 < uIPanel.depth && 100 > uIPanel.depth)
			{
				uIPanel.depth += 30;
			}
		}
	}

	public void Open(PunishmentRaidRewardData punishmentRaidRewardData, int infinityLevel)
	{
		PunishmentRaidData data = GameRoot.Instance.PunishmentRaidDataContainer.GetData(punishmentRaidRewardData.RaidIndex, punishmentRaidRewardData.RaidLevel);
		InitRaid(punishmentRaidRewardData, data);
		InitReward(punishmentRaidRewardData, infinityLevel);
	}

	public void InitRaid(PunishmentRaidRewardData punishmentRaidRewardData, PunishmentRaidData punishmentRaidData)
	{
		string imagePath = punishmentRaidData.ImagePath;
		AssetTexture assetTexture = new AssetTexture();
		assetTexture.Open(imagePath);
		UIHelper.ResetPlaneAssetTextureAsync(this, MonsterTexture, assetTexture);
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.IsOpenPunishmentRaid(punishmentRaidData.GroupIndex, EPunishmentDungeonType.Boss);
		GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.GetPunishmentRaidInfo(punishmentRaidData.GroupIndex, punishmentRaidData.RaidIndex, punishmentRaidData.RaidLevel);
		string[] showRecieveSubRewardCondition = punishmentRaidRewardData.ShowRecieveSubRewardCondition;
		bool flag2 = SingletonAuto<ConditionChecker>.instance.Check(showRecieveSubRewardCondition);
		UIHelper.EnableGameObject(ClearedSprite, flag && !flag2);
	}

	public void InitReward(PunishmentRaidRewardData punishmentRaidRewardData, int infinityLevel)
	{
		if (punishmentRaidRewardData != null)
		{
			int infinityRewardGroupIndex = punishmentRaidRewardData.InfinityRewardGroupIndex;
			_rewardCompList = UIHelper.CreateInfinityRewardItemIconComponentList(infinityRewardGroupIndex, infinityLevel, Grid_RewardItem);
			UIHelper.RepositionGrid(Grid_RewardItem);
		}
	}

	private void ClearItemIconList()
	{
		FactoryBase.ReleaseAndClearList(_rewardCompList);
	}
}
