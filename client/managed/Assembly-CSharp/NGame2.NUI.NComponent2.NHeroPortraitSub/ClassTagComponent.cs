using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NHeroPortraitSub;

public class ClassTagComponent : BaseSubComponent<HeroPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/HeroPortraitSub/ClassTagComponent";

	public static PrefabAuto<ClassTagComponent> PrefabAuto = new PrefabAuto<ClassTagComponent>(PrefabPath);

	public HeroPortraitComponent.ClassTagInfo[] ClassTagInfo;

	public override void Open(HeroPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(HeroPortraitComponent.ESubComponent.ClassTag));
	}

	public void SetTag(CreatureTagType tagType)
	{
		for (int i = 0; i < ClassTagInfo.Length; i++)
		{
			UIHelper.EnableGameObject(ClassTagInfo[i].Sprite_ClassTag, ClassTagInfo[i].tagType == tagType);
		}
	}
}
