using System.Collections.Generic;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GlobalBanPickServerTotalRankingRewardComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Match/GlobalBanPickServerTotalRankingRewardComponent";

	public static PrefabAuto<GlobalBanPickServerTotalRankingRewardComponent> PrefabAuto = new PrefabAuto<GlobalBanPickServerTotalRankingRewardComponent>("Prefab2/Component/Match/GlobalBanPickServerTotalRankingRewardComponent");

	public UISimpleGrid Grid_RewardItem;

	public UILabel RankingLabel;

	private List<UIScrollItemBehaviour> _itemIconComponentList = new List<UIScrollItemBehaviour>();

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		Clear();
	}

	public void SetData(GlobalBanPickTotalRewardData globalBanPickTotalRewardData, bool isRegularSeason)
	{
		Clear();
		string stringDirect = LocalizationManager.GetStringDirect("GUILD_RANKCOUNT", globalBanPickTotalRewardData.Index);
		UIHelper.SetLabelText(RankingLabel, stringDirect);
		if (isRegularSeason)
		{
			SetServerRewardData(globalBanPickTotalRewardData.ServerBuffCode);
		}
	}

	private void SetServerRewardData(string rewardCode)
	{
		ItemIconComponent itemIconComponent = UIHelper.AddGridItem(Grid_RewardItem, ItemIconComponent.PrefabAuto);
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardCode);
		itemIconComponent.Open(dataByCode);
		_itemIconComponentList.Add(itemIconComponent);
		UIHelper.RepositionGrid(Grid_RewardItem);
	}

	private void Clear()
	{
		UIHelper.SetLabelText(RankingLabel, "");
		FactoryBase.ReleaseAndClearList(_itemIconComponentList);
	}
}
