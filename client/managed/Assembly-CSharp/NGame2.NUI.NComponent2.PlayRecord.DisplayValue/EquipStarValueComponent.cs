using System;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.PlayRecord.DisplayValue;

public class EquipStarValueComponent : MonoBehaviour, IDisplayValueComponent
{
	[Serializable]
	public class LimitBreakUi
	{
		public UIWidget Root;

		public UILabel Label_StarCount;
	}

	public const string PrefabPath = "Prefab2/Component/PlayRecord/DisplayValue/EquipStarValueComponent";

	public static PrefabAuto<EquipStarValueComponent> PrefabAuto = new PrefabAuto<EquipStarValueComponent>("Prefab2/Component/PlayRecord/DisplayValue/EquipStarValueComponent");

	[Header("[5성 이하 표기]")]
	public Transform[] Star;

	[Header("[6성 이상 표기]")]
	public LimitBreakUi LimitBreak;

	void IDisplayValueComponent.Open(PlayRecordData playRecordData, string recordValue)
	{
		int num = Vespa_String_Extension_Method.TOINT(recordValue, 0);
		if (num <= 5)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Star); i++)
			{
				bool enable = i + 1 == num;
				UIHelper.EnableGameObject(Star[i], enable);
			}
			UIHelper.EnableGameObject(LimitBreak.Root, enable: false);
		}
		else
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(Star); j++)
			{
				UIHelper.EnableGameObject(Star[j], enable: false);
			}
			UIHelper.EnableGameObject(LimitBreak.Root, enable: true);
			UIHelper.SetLabelText(LimitBreak.Label_StarCount, num);
		}
	}
}
