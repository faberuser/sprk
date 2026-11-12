using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class DecoratorFraction : LinkFactoryComponent
{
	public const string PrefabPath = "Prefab2/WorldMap/Node/Decorator/DecoratorFraction";

	public static PrefabAuto<DecoratorFraction> PrefabAuto = new PrefabAuto<DecoratorFraction>("Prefab2/WorldMap/Node/Decorator/DecoratorFraction");

	public UILabel NameLabel;

	public UITexture ItemTexture;

	private void SetRewardFraction(int chapterIndex, int dungeonIndex)
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return;
		}
		int maxAvailableDifficulty = Helper.GetMaxAvailableDifficulty(chapterIndex, dungeonIndex);
		int rewardIndexOfDifficulty = data.GetRewardIndexOfDifficulty((DungeonDifficultyType)maxAvailableDifficulty);
		RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(rewardIndexOfDifficulty);
		if (data2 == null)
		{
			return;
		}
		List<string> rewardItemCodeList = data2.GetRewardItemCodeList();
		int num = 0;
		ItemData itemData = null;
		foreach (string item in rewardItemCodeList)
		{
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(item);
			if (dataByCode != null && dataByCode.Grade > num)
			{
				num = dataByCode.Grade;
				itemData = dataByCode;
			}
		}
		if (itemData != null)
		{
			UIHelper.ResetPlaneAssetTextureAsync(this, ItemTexture, UIHelper.GetItemIcon2(itemData.Code));
		}
	}

	public void Open(int chapterIndex, int dungeonIndex, bool canVisit)
	{
		GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			if (__Extension_Method_ICollection__.COUNT(data.Code) == 0)
			{
				UIHelper.SetLabelText(NameLabel, $"{chapterIndex}-{dungeonIndex}");
			}
			else
			{
				UIHelper.SetLabelText(NameLabel, LocalizationManager.GetStringDirect(data.Code[0]));
			}
			SetRewardFraction(chapterIndex, dungeonIndex);
		}
	}
}
