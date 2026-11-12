using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class HeroLinkedSkillIconComponent : UIScrollItemBehaviour, InitOnCreate, HeroSkillIconComponent.Listener
{
	public interface Listener
	{
		void OnSelectSkillComponent(HeroSkillIconComponent comp);
	}

	public const string PrefabPath = "Prefab2/Component/Icon/HeroLinkedSkillIconComponent";

	public const string PrefabPath3 = "Prefab2/Component/Icon/HeroLinked3SkillIconComponent";

	public static PrefabAuto<HeroLinkedSkillIconComponent> PrefabAuto = new PrefabAuto<HeroLinkedSkillIconComponent>("Prefab2/Component/Icon/HeroLinkedSkillIconComponent");

	public static PrefabAuto<HeroLinkedSkillIconComponent> PrefabAuto3 = new PrefabAuto<HeroLinkedSkillIconComponent>("Prefab2/Component/Icon/HeroLinked3SkillIconComponent");

	public List<HeroSkillIconComponent> SkillIconComponents;

	private Listener _listener;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open(HeroInfo heroInfo, int skillSlotIndex, Listener listener)
	{
		Helper.SkillBasicInfo[] skillBasicInfos = Helper.GetSkillBasicInfos(heroInfo, skillSlotIndex);
		Open(heroInfo, skillBasicInfos, listener);
	}

	public void Open(HeroInfo heroInfo, Helper.SkillBasicInfo[] skillInfos, Listener listener)
	{
		_listener = listener;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(SkillIconComponents); i++)
		{
			HeroSkillIconComponent heroSkillIconComponent = SkillIconComponents[i];
			if (__Extension_Method_ICollection__.COUNT(skillInfos) <= i)
			{
				UIHelper.EnableGameObject(heroSkillIconComponent, enable: false);
				continue;
			}
			UIHelper.EnableGameObject(heroSkillIconComponent, this);
			heroSkillIconComponent.Open(heroInfo, skillInfos[i], this);
		}
	}

	void HeroSkillIconComponent.Listener.OnSelectSkillComponent(HeroSkillIconComponent comp)
	{
		if (_listener != null)
		{
			_listener.OnSelectSkillComponent(comp);
		}
	}
}
