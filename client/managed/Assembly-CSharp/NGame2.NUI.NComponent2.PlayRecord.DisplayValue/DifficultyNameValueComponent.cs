using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.PlayRecord.DisplayValue;

public class DifficultyNameValueComponent : MonoBehaviour, IDisplayValueComponent
{
	public const string PrefabPath = "Prefab2/Component/PlayRecord/DisplayValue/DifficultyNameValueComponent";

	public static PrefabAuto<DifficultyNameValueComponent> PrefabAuto = new PrefabAuto<DifficultyNameValueComponent>("Prefab2/Component/PlayRecord/DisplayValue/DifficultyNameValueComponent");

	public UILabel ValueLabel;

	void IDisplayValueComponent.Open(PlayRecordData playRecordData, string recordValue)
	{
		int num = Vespa_String_Extension_Method.TOINT(recordValue, 0);
		if (num >= 0 && num <= 3)
		{
			switch ((NShared.DungeonDifficultyType)num)
			{
			case NShared.DungeonDifficultyType.Easy:
				UIHelper.SetLabelText(ValueLabel, LocalizationManager.GetString(NShared.NLocalization.Common.DIFFICULTY_EASY));
				break;
			case NShared.DungeonDifficultyType.Normal:
				UIHelper.SetLabelText(ValueLabel, LocalizationManager.GetString(NShared.NLocalization.Common.DIFFICULTY_NORMAL));
				break;
			case NShared.DungeonDifficultyType.Hard:
				UIHelper.SetLabelText(ValueLabel, LocalizationManager.GetString(NShared.NLocalization.Common.DIFFICULTY_HARD));
				break;
			case NShared.DungeonDifficultyType.Hell:
				UIHelper.SetLabelText(ValueLabel, LocalizationManager.GetString(NShared.NLocalization.Common.DIFFICULTY_HELL));
				break;
			}
		}
		else
		{
			VespaDebug.LogError("Invalid dungeon difficulty : " + num);
		}
	}
}
