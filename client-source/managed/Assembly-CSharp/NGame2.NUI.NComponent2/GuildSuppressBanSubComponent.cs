using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GuildSuppressBanSubComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/GuildSuppress/GuildSuppressBanSubComponent";

	public static PrefabAuto<GuildSuppressBanSubComponent> PrefabAuto = new PrefabAuto<GuildSuppressBanSubComponent>("Prefab2/Component/GuildSuppress/GuildSuppressBanSubComponent");

	private List<ChallengeRaidBanInfoComponent> _banComponentList = new List<ChallengeRaidBanInfoComponent>();

	public UIGrid Grid;

	public UIBundleLoader BossTexture;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.ReleaseAndClearList(_banComponentList);
	}

	public void Open(int dungeonIndex)
	{
		GuildSuppressDungeonData data = GameRoot.Instance.GuildSuppressDungeonDataContainer.GetData(dungeonIndex);
		if (data == null)
		{
			return;
		}
		int banIndex = data.BanIndex;
		BanRuleData data2 = GameRoot.Instance.BanRuleDataContainer.GetData(banIndex);
		if (data2 == null)
		{
			return;
		}
		RaidBanRuleData[] banRuleDataAry = data2.GetBanRuleDataAry();
		foreach (RaidBanRuleData raidBanRuleData in banRuleDataAry)
		{
			if (raidBanRuleData.BanType == RaidBanType.None)
			{
				continue;
			}
			if (raidBanRuleData.BanType == RaidBanType.CodeName)
			{
				string[] array = raidBanRuleData.BanValue.Split(',');
				foreach (string banValue in array)
				{
					ChallengeRaidBanInfoComponent challengeRaidBanInfoComponent = ChallengeRaidBanInfoComponent.PrefabAuto.Factory.Create(Grid.transform);
					challengeRaidBanInfoComponent.Open(null, raidBanRuleData.BanType, banValue);
					_banComponentList.Add(challengeRaidBanInfoComponent);
				}
			}
			else
			{
				ChallengeRaidBanInfoComponent challengeRaidBanInfoComponent2 = ChallengeRaidBanInfoComponent.PrefabAuto.Factory.Create(Grid.transform);
				challengeRaidBanInfoComponent2.Open(null, raidBanRuleData.BanType, raidBanRuleData.BanValue);
				_banComponentList.Add(challengeRaidBanInfoComponent2);
			}
		}
		Grid.Reposition();
		if (BossTexture != null)
		{
			UIHelper.SetBundleTexture(BossTexture, data.IconPath);
		}
	}
}
