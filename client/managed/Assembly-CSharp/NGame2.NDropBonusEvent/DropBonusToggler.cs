using NGame2.NUI;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NDropBonusEvent;

public class DropBonusToggler : MonoBehaviour
{
	public Transform ActiveRoot;

	public Transform DeactiveRoot;

	public EventBonusType BonusType;

	public string Condition;

	public UILabel PercentageLabel;

	private float _lastCheckedTime;

	private void Update()
	{
		if (!(Time.time - _lastCheckedTime < 1f))
		{
			_lastCheckedTime = Time.time;
			DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(BonusType, Condition);
			bool flag = effectiveEvent != null;
			UIHelper.EnableGameObject(ActiveRoot, flag);
			UIHelper.EnableGameObject(DeactiveRoot, !flag);
			if (flag)
			{
				UIHelper.SetLabelText(PercentageLabel, effectiveEvent.BonusPercentage);
			}
		}
	}
}
