using NGame2.NUI.NWindow;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.PlayRecord.DisplayValue;

public class ChallengeRaidDifficultyNameValueComponent : MonoBehaviour, IDisplayValueComponent
{
	public const string PrefabPath = "Prefab2/Component/PlayRecord/DisplayValue/ChallengeRaidDifficultyNameValueComponent";

	public static PrefabAuto<ChallengeRaidDifficultyNameValueComponent> PrefabAuto = new PrefabAuto<ChallengeRaidDifficultyNameValueComponent>("Prefab2/Component/PlayRecord/DisplayValue/ChallengeRaidDifficultyNameValueComponent");

	public UILabel ValueLabel;

	void IDisplayValueComponent.Open(PlayRecordData playRecordData, string recordValue)
	{
		int index = 0;
		foreach (PlayRecordCondition condition in playRecordData.GetConditions())
		{
			if (condition.Key == "RaidIndex" && __Extension_Method_ICollection__.COUNT(condition.Value) > 0)
			{
				index = Vespa_String_Extension_Method.TOINT(condition.Value[0], 0);
				break;
			}
		}
		int level = Vespa_String_Extension_Method.TOINT(recordValue, 0);
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(index, level);
		if (data == null)
		{
			UIHelper.SetLabelText(ValueLabel, "-");
			return;
		}
		ChallengeRaidDiff challengeRaidDiff = RaidHelper.ChallengeRaidDiffClamp(data.Level);
		UIHelper.SetLabelText(ValueLabel, challengeRaidDiff.ToString());
	}
}
