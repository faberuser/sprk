using NNguiExtension;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PortalChallengeRaidRewardComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalChallengeRaidRewardComponent";

	public static PrefabAuto<PortalChallengeRaidRewardComponent> PrefabAuto = new PrefabAuto<PortalChallengeRaidRewardComponent>("Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalChallengeRaidRewardComponent");

	public Transform RewardItemIconComponentRootTransform;

	public UISpriteSelector DifficultySpriteSelector;

	private ItemIconComponent _itemIconComponent;

	public void Open(AchievementReward rewardData, int raidLevel)
	{
		Init(rewardData, raidLevel);
	}

	private void Init(AchievementReward rewardData, int raidLevel)
	{
		if (rewardData != null)
		{
			SetRewardItemIconComponent(rewardData);
			SetDifficultySpriteSelector(raidLevel);
		}
	}

	private void SetRewardItemIconComponent(AchievementReward rewardData)
	{
		FactoryBase.ReleaseComponent(_itemIconComponent);
		if (rewardData == null || RewardItemIconComponentRootTransform == null)
		{
			return;
		}
		_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(RewardItemIconComponentRootTransform);
		if (!(_itemIconComponent == null))
		{
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardData.RewardCode);
			if (dataByCode != null)
			{
				_itemIconComponent.Open(dataByCode);
				ItemIconComponent itemIconComponent = _itemIconComponent;
				int rewardValue = rewardData.RewardValue;
				Color? color = null;
				int num = rewardValue;
				itemIconComponent.SetItemCount(num, color);
				_itemIconComponent.ResetPosition();
			}
		}
	}

	private void SetDifficultySpriteSelector(int difficulty)
	{
		if (DifficultySpriteSelector != null)
		{
			DifficultySpriteSelector.Select(difficulty.ToString());
		}
	}
}
