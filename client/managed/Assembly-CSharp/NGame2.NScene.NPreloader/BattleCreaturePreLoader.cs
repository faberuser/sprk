using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NBattleContext;
using NGame2.NNodeTree;
using NShared;
using NVespa;

namespace NGame2.NScene.NPreloader;

public class BattleCreaturePreLoader : RootPreloader
{
	public static LocalStorageEnum<EAsyncPreloadType> CreaturePreloadValue = new LocalStorageEnum<EAsyncPreloadType>(EAsyncPreloadType.Normal, typeof(BattleCreaturePreLoader).FullName + "DisableCreaturePreloadValue");

	public static LocalStorageEnum<EAsyncPreloadType> EffectPreloadValue = new LocalStorageEnum<EAsyncPreloadType>(EAsyncPreloadType.Delay, typeof(BattleCreaturePreLoader).FullName + "DisableEffectPreloadValue");

	public static LocalStorageEnum<EAsyncPreloadType> SoundPreloadValue = new LocalStorageEnum<EAsyncPreloadType>(EAsyncPreloadType.Delay, typeof(BattleCreaturePreLoader).FullName + "DisableSoundPreloadValue");

	public EnterDungeonData EnterDungeon;

	public override void Begin()
	{
		base.Begin();
		CreaturePreloadType = CreaturePreloadValue.Value;
		EffectPreloadType = EffectPreloadValue.Value;
		SoundPreloadType = SoundPreloadValue.Value;
		CollectDungeonData();
	}

	private void CollectDungeonData()
	{
		if (EnterDungeon != null)
		{
			CollectDungeonData(EnterDungeon, this, base.Preloader);
		}
	}

	public static void CollectDungeonData(EnterDungeonData enterDungeonData, RootPreloader rootPreloader, BasePreloader basePreloader)
	{
		if (enterDungeonData == null)
		{
			return;
		}
		for (int i = 1; i < 10; i++)
		{
			CampaignWaveData data = GameRoot.Instance.CampaignWaveDataContainer.GetData(enterDungeonData.Chapter, enterDungeonData.LinkedDungeon, enterDungeonData.Difficulty, enterDungeonData.IsScenario, i);
			if (data == null)
			{
				break;
			}
			for (int j = 1; j < 100; j++)
			{
				CampaignWaveData.EnvironmentProjectileInfo environmentProjectileInfo = data.GetEnvironmentProjectileInfo(j);
				if (environmentProjectileInfo == null)
				{
					break;
				}
				int projectileIndex = environmentProjectileInfo.ProjectileIndex;
				SkillProjectileData data2 = GameRoot.Instance.SkillProjectileDataContainer.GetData(projectileIndex);
				if (data2 != null)
				{
					int skillIndex = data2.SkillIndex;
					string text = $"ExtraSkill/{skillIndex}";
					if (PackLoad.IsExists(text))
					{
						basePreloader.AddNodeTree(text, -1L);
					}
				}
			}
			List<CampaignWaveData.CreatureDetailInfo> creatureList = data.GetCreatureList();
			for (int k = 0; k < creatureList.Count; k++)
			{
				CampaignWaveData.CreatureDetailInfo creatureDetailInfo = creatureList[k];
				HeroInfo heroInfo = CampaignWaveData.CreateHeroInfoFromDetailInfo(creatureDetailInfo);
				RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, heroInfo, null);
				if (!String_Extension_Method.IsNullOrEmpty(creatureDetailInfo.VisualInfo))
				{
					CreatureVisualData data3 = GameRoot.Instance.CreatureVisualDataContainer.GetData(creatureDetailInfo.VisualInfo);
					if (data3 != null)
					{
						renderFacadeBuilder.SetModelPrefix(data3.ModelPrefix);
					}
				}
				rootPreloader.AddCreature(heroInfo, renderFacadeBuilder, creatureDetailInfo.AddExtraSkill, creatureDetailInfo.IsBoss, loadCirclePortrait: true);
			}
		}
	}
}
