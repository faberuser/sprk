using NGame2.NAccount;
using NGame2.NUI;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NContents;

public class TutorialChecker : MonoBehaviour
{
	public enum State
	{
		Enable,
		Disable
	}

	public int TutorialIndexToCheck;

	public State SetEnable;

	private void OnEnable()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.TutorialManager != null && GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(TutorialIndexToCheck))
		{
			UIHelper.EnableGameObject(this, SetEnable == State.Enable);
		}
	}
}
