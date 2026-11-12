using System;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPetPortraitSub;

public class PetGradeSubComponent : BaseSubComponent<PetPortraitComponent>
{
	[Serializable]
	public class PetGradeInfo
	{
		public int Grade;

		public UISprite Sprite_PetGrade;
	}

	public static string PrefabPath = "Prefab2/Component/PetPortraitSub/PetGradeSubComponent";

	public static PrefabAuto<PetGradeSubComponent> PrefabAuto = new PrefabAuto<PetGradeSubComponent>(PrefabPath);

	public PetGradeInfo[] PetGradeInfos;

	public override void Open(PetPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(PetPortraitComponent.ESubComponent.Grade));
	}

	public void SetGrade(byte grade)
	{
		for (int i = 0; i < PetGradeInfos.Length; i++)
		{
			UIHelper.EnableGameObject(PetGradeInfos[i].Sprite_PetGrade, PetGradeInfos[i].Grade == grade);
		}
	}
}
