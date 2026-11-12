using NGame2.NAccount;
using NNguiExtension;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class SkillBookHeroPortraitComponent : LinkFactoryComponent
{
	public enum StatusType
	{
		Learnable,
		NotEnoughItem,
		NotLearn,
		AllLearn
	}

	public const string PrefabPath = "Prefab2/Component/Inventory/SkillBookHeroPortraitComponent";

	public static PrefabAuto<SkillBookHeroPortraitComponent> PrefabAuto = new PrefabAuto<SkillBookHeroPortraitComponent>("Prefab2/Component/Inventory/SkillBookHeroPortraitComponent");

	public GameObject HasIconRoot;

	public GameObject NoIconRoot;

	public UILabelSelector StatusLabelSelector;

	public UIBundleLoader SkillIconBundleLoader;

	public StatusType Status { get; private set; }

	public void Open(CachedHeroInfo heroInfo, int skillSlotIndex, long itemCount)
	{
		GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex).GetSkillExtend(skillSlotIndex);
		Helper.SkillDetailInfo skillDetailInfo = Helper.GetSkillDetailInfo(heroInfo, skillSlotIndex);
		int skillLevel = heroInfo.GetSkillLevel(skillSlotIndex, isUserHero: true);
		int skillExtend = heroInfo.GetSkillExtend(skillSlotIndex);
		if (skillExtend == 3)
		{
			Status = StatusType.AllLearn;
		}
		else if (skillLevel == 0)
		{
			Status = StatusType.NotLearn;
		}
		else
		{
			int extendItemCount = skillDetailInfo.ExtendInfos[skillExtend].ExtendItemCount;
			if (itemCount >= extendItemCount)
			{
				Status = StatusType.Learnable;
			}
			else
			{
				Status = StatusType.NotEnoughItem;
			}
		}
		if (Status != StatusType.AllLearn)
		{
			UIHelper.EnableGameObject(HasIconRoot, enable: true);
			UIHelper.EnableGameObject(NoIconRoot, enable: false);
			string skillIconPath = UIHelper.GetSkillIconPath(skillDetailInfo.BasicInfo.TableIndex, heroInfo);
			UIHelper.SetBundleTexture(SkillIconBundleLoader, skillIconPath);
			StatusLabelSelector.Select(Status.ToString());
		}
		else
		{
			UIHelper.EnableGameObject(HasIconRoot, enable: false);
			UIHelper.EnableGameObject(NoIconRoot, enable: true);
		}
	}
}
