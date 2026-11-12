using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PlayRecordTitleComponent : MonoBehaviour, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/PlayRecord/PlayRecordTitleComponent";

	public static PrefabAuto<PlayRecordTitleComponent> PrefabAuto = new PrefabAuto<PlayRecordTitleComponent>("Prefab2/Component/PlayRecord/PlayRecordTitleComponent");

	public UILabel TitleLabel;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open(string titleKey)
	{
		string stringDirect = LocalizationManager.GetStringDirect(titleKey);
		UIHelper.SetLabelText(TitleLabel, stringDirect);
	}
}
