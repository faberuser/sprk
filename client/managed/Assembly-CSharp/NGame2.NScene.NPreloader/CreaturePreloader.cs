using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NUtil;
using NGame2.NUI;
using NGameAsset;
using NGameAsset.NCreature;
using NGameAsset.NSound;
using NShared;
using NVespa;

namespace NGame2.NScene.NPreloader;

public class CreaturePreloader : BasePreloader
{
	private HeroInfo _heroInfo;

	public override object ExtraData => _heroInfo;

	public CreaturePreloader(RootPreloader rootPreloader)
		: base(rootPreloader)
	{
	}

	public bool Open(HeroInfo heroInfo, RenderFacadeBuilder renderFacadeBuilder, ExtraSkillInfo[] addExtraSkill, bool loadCreatureImage, bool loadCirclePortrait = false)
	{
		if (heroInfo == null || heroInfo.CreatureData == null)
		{
			return false;
		}
		_heroInfo = heroInfo;
		PreloaderName = heroInfo.CreatureData.Name;
		if (loadCreatureImage)
		{
			string battleHeroPortraitPath = UIHelper.GetBattleHeroPortraitPath(heroInfo);
			AddBundleTexPath(battleHeroPortraitPath);
			string heroIconPath = UIHelper.GetHeroIconPath(heroInfo);
			AddBundleTexPath(heroIconPath);
			if (loadCirclePortrait)
			{
				string monsterCirclePortraitPath = UIHelper.GetMonsterCirclePortraitPath(heroInfo.CreatureData.CodeName, heroInfo.Star);
				AddBundleTexPath(monsterCirclePortraitPath);
			}
			List<int> skillIndexList = heroInfo.CreatureData.GetSkillIndexList(includeExtra: false);
			for (int i = 0; i < skillIndexList.Count; i++)
			{
				int num = skillIndexList[i];
				if (num != 0)
				{
					string skillIconPath = UIHelper.GetSkillIconPath(num, heroInfo);
					AddBundleTexPath(skillIconPath);
				}
			}
		}
		ExecuteAction(_rootPreloader.SoundPreloadType, delegate
		{
			string prefix = "DAMAGE";
			string prefix2 = "DOWN";
			string codeName = heroInfo.CreatureData.CodeName;
			string costumeVoicePostfix = SoundHelper.GetCostumeVoicePostfix(heroInfo);
			string soundCode = SoundHelper.GetSoundCode("DEAD", codeName, costumeVoicePostfix);
			string soundCode2 = SoundHelper.GetSoundCode(prefix, codeName, costumeVoicePostfix);
			string soundCode3 = SoundHelper.GetSoundCode(prefix2, codeName, costumeVoicePostfix);
			AddAsyncLoadList(SingletonAuto<SoundContainer>.instance.ReserveSound(soundCode));
			AddAsyncLoadList(SingletonAuto<SoundContainer>.instance.ReserveSound(soundCode2));
			AddAsyncLoadList(SingletonAuto<SoundContainer>.instance.ReserveSound(soundCode3));
		});
		CreatureStock stock = renderFacadeBuilder.GetStock();
		Builder creatureBuilder = SingletonAuto<BuilderContainer>.instance.Get(stock.Code);
		if (creatureBuilder != null)
		{
			ExecuteAction(_rootPreloader.CreaturePreloadType, delegate
			{
				AddAsyncLoad(creatureBuilder.CreateOpenData(stock));
			});
		}
		AddNodeTree(PackHelper.GetPathNameFromCode(heroInfo.CreatureData.CodeName), -1L);
		LoadExtraSkill(heroInfo.CreatureData.ExtraSkill);
		LoadExtraSkill(addExtraSkill);
		return true;
	}

	private void LoadExtraSkill(ExtraSkillInfo[] extraSkills)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(extraSkills); i++)
		{
			LoadExtraSkill(extraSkills[i].SkillIndex);
		}
	}

	private void LoadExtraSkill(int[] extraSkills)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(extraSkills); i++)
		{
			LoadExtraSkill(extraSkills[i]);
		}
	}

	private void LoadExtraSkill(int skillIndex)
	{
		string text = $"ExtraSkill/{skillIndex}";
		if (PackLoad.IsExists(text))
		{
			AddNodeTree(text, -1L);
		}
	}
}
