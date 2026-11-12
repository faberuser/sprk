using NShared;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class CompareStatComponent : MonoBehaviour
{
	public const string PrefabPath = "Prefab2/Component/Stat/CompareStatComponent";

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _beforeValueLabel;

	[SerializeField]
	private UILabel _afterValueLabel;

	public void Open(StatType statType, long beforeValue, long afterValue)
	{
		UIHelper.SetLabelText(_nameLabel, LocalizationManager.GetString(statType));
		UIHelper.SetLabelText(_beforeValueLabel, beforeValue);
		UIHelper.SetLabelText(_afterValueLabel, afterValue);
		if (beforeValue < afterValue)
		{
			UIHelper.SetLabelColor(_afterValueLabel, Color.green);
		}
		else if (beforeValue > afterValue)
		{
			UIHelper.SetLabelColor(_afterValueLabel, Color.red);
		}
		else
		{
			UIHelper.SetLabelColor(_afterValueLabel, Color.white);
		}
	}
}
