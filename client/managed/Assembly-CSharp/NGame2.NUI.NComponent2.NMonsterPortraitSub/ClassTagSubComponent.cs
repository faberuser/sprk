using System.Collections.Generic;
using NGame2.NUI.NComponent2.NPortrait;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NMonsterPortraitSub;

public class ClassTagSubComponent : BaseSubComponent<MonsterComponent>
{
	public const string PrefabPath = "Prefab2/Component/MonsterPortraitSub/ClassTagSubComponent";

	public static PrefabAuto<ClassTagSubComponent> PrefabAuto = new PrefabAuto<ClassTagSubComponent>("Prefab2/Component/MonsterPortraitSub/ClassTagSubComponent");

	public List<HeroPortraitComponent.ClassTagInfo> ClassTagInfos = new List<HeroPortraitComponent.ClassTagInfo>();

	public override void Open(MonsterComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(MonsterComponent.ESubComponent.ClassTag));
	}

	public void SetTag(CreatureTagType tagType)
	{
		HeroPortraitComponent.SetClassTag(base.Owner.TableData, ClassTagInfos);
	}
}
