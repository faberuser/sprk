using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;
using NVespa.NTask;

namespace NGame2.NLobby.NTask;

public class OpenHero : ILobbyTaskCreator
{
	ITask ILobbyTaskCreator.Create(string[] values)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		Hero3DViewPanel.EToggle valueEnum = ArgParser.GetValueEnum(values, 1, Hero3DViewPanel.EToggle.None);
		Hero3DViewPanel.ENextTask valueEnum2 = ArgParser.GetValueEnum(values, 2, Hero3DViewPanel.ENextTask.None);
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(valueInt);
		if (cachedHeroInfo == null)
		{
			return null;
		}
		return SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyHeroAsync(cachedHeroInfo, valueEnum, valueEnum2);
	}
}
