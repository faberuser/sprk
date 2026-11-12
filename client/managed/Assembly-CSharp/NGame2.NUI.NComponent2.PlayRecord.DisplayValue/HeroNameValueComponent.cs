using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.PlayRecord.DisplayValue;

public class HeroNameValueComponent : MonoBehaviour, IDisplayValueComponent
{
	public const string PrefabPath = "Prefab2/Component/PlayRecord/DisplayValue/HeroNameValueComponent";

	public static PrefabAuto<HeroNameValueComponent> PrefabAuto = new PrefabAuto<HeroNameValueComponent>("Prefab2/Component/PlayRecord/DisplayValue/HeroNameValueComponent");

	public UILabel ValueLabel;

	void IDisplayValueComponent.Open(PlayRecordData playRecordData, string recordValue)
	{
		int key = Vespa_String_Extension_Method.TOINT(recordValue, 0);
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(key);
		string text = ((data != null) ? data.Name : "-");
		UIHelper.SetLabelText(ValueLabel, text);
	}
}
