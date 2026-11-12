using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.PlayRecord.DisplayValue;

public class DateValueComponent : MonoBehaviour, IDisplayValueComponent
{
	public const string PrefabPath = "Prefab2/Component/PlayRecord/DisplayValue/DateValueComponent";

	public static PrefabAuto<DateValueComponent> PrefabAuto = new PrefabAuto<DateValueComponent>("Prefab2/Component/PlayRecord/DisplayValue/DateValueComponent");

	public UILabel ValueLabel;

	public string DateFormat = "yyyy-MM-dd";

	void IDisplayValueComponent.Open(PlayRecordData playRecordData, string recordValue)
	{
		if (String_Extension_Method.IsNullOrEmpty(recordValue))
		{
			UIHelper.SetLabelText(ValueLabel, "-");
			return;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(recordValue, out var o_dateTime))
		{
			UIHelper.SetLabelText(ValueLabel, "-");
			return;
		}
		o_dateTime = UIHelper.GetClientTime(o_dateTime);
		UIHelper.SetLabelText(ValueLabel, o_dateTime.ToString(DateFormat));
	}
}
