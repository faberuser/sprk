using System;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2.NMultipleDungeon;

[Serializable]
public class MultipleDungeonEnterCount : BaseEnterCount
{
	public void Init(int chapterIndex)
	{
		SetEnterCount(chapterIndex);
	}

	private void SetEnterCount(int chapterIndex)
	{
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			UnderPrisonInfo underPrisonInfo = GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager.Get(chapterIndex);
			if (underPrisonInfo != null)
			{
				SetResetCountLabel(1, underPrisonInfo.MaxTryCount);
				SetEnterCountLabel(underPrisonInfo.MaxTryCount, underPrisonInfo.CompletedCount);
			}
		}
	}
}
