using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.PlayRecord.DisplayValue;

public class ChapterNameValueComponent : MonoBehaviour, IDisplayValueComponent
{
	public const string PrefabPath = "Prefab2/Component/PlayRecord/DisplayValue/ChapterNameValueComponent";

	public static PrefabAuto<ChapterNameValueComponent> PrefabAuto = new PrefabAuto<ChapterNameValueComponent>("Prefab2/Component/PlayRecord/DisplayValue/ChapterNameValueComponent");

	public UILabel ValueLabel;

	void IDisplayValueComponent.Open(PlayRecordData playRecordData, string recordValue)
	{
		int key = Vespa_String_Extension_Method.TOINT(recordValue, 0);
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(key);
		string text = ((data != null) ? LocalizationManager.GetStringDirect(data.Name) : "-");
		UIHelper.SetLabelText(ValueLabel, text);
	}
}
