using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenHero : IActivity
{
	private CachedHeroInfo _heroInfo;

	private Hero3DViewPanel.EToggle _toggle;

	private Hero3DViewPanel.ENextTask _nextTask;

	public static IActivity Create(string[] values)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		Hero3DViewPanel.EToggle valueEnum = ArgParser.GetValueEnum(values, 1, Hero3DViewPanel.EToggle.None);
		Hero3DViewPanel.ENextTask valueEnum2 = ArgParser.GetValueEnum(values, 2, Hero3DViewPanel.ENextTask.None);
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(valueInt);
		if (cachedHeroInfo == null)
		{
			return null;
		}
		return new OpenHero(cachedHeroInfo, valueEnum, valueEnum2);
	}

	public OpenHero(CachedHeroInfo heroInfo, Hero3DViewPanel.EToggle toggle, Hero3DViewPanel.ENextTask nextTask)
	{
		_heroInfo = heroInfo;
		_toggle = toggle;
		_nextTask = nextTask;
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyHero(_heroInfo, _toggle, _nextTask);
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
