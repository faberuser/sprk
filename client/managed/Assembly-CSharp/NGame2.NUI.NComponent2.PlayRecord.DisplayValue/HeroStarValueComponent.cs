using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.PlayRecord.DisplayValue;

public class HeroStarValueComponent : MonoBehaviour, IDisplayValueComponent
{
	public const string PrefabPath = "Prefab2/Component/PlayRecord/DisplayValue/HeroStarValueComponent";

	public static PrefabAuto<HeroStarValueComponent> PrefabAuto = new PrefabAuto<HeroStarValueComponent>("Prefab2/Component/PlayRecord/DisplayValue/HeroStarValueComponent");

	public Transform[] Star;

	void IDisplayValueComponent.Open(PlayRecordData playRecordData, string recordValue)
	{
		int num = Vespa_String_Extension_Method.TOINT(recordValue, 0);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Star); i++)
		{
			bool enable = i + 1 == num;
			UIHelper.EnableGameObject(Star[i], enable);
		}
	}
}
