using System;
using System.Collections.Generic;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPetPortraitSub;

public class SelectSubComponent : BaseSubComponent<PetPortraitComponent>
{
	public enum ESelectType
	{
		Select_Checked,
		Select_BorderBook
	}

	[Serializable]
	public class SelectUiData
	{
		public ESelectType SelectType;

		public UIWidget Root;
	}

	public static string PrefabPath = "Prefab2/Component/PetPortraitSub/SelectSubComponent";

	public static PrefabAuto<SelectSubComponent> PrefabAuto = new PrefabAuto<SelectSubComponent>(PrefabPath);

	public List<SelectUiData> SelectUiList = new List<SelectUiData>();

	public override void Open(PetPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(PetPortraitComponent.ESubComponent.Select));
	}

	public void SetSelect(ESelectType selectType)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(SelectUiList); i++)
		{
			SelectUiData selectUiData = SelectUiList[i];
			UIHelper.EnableGameObject(selectUiData.Root, selectUiData.SelectType == selectType);
		}
	}
}
