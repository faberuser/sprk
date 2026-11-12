using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenTopHeroInfo : IActivity
{
	private CachedHeroInfo _heroInfo;

	private Hero3DViewPanel.EToggle _toggle;

	public static IActivity Create(string[] values)
	{
		List<CachedHeroInfo> heroInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true);
		if (heroInfoList == null)
		{
			return null;
		}
		CachedHeroInfo heroInfo = null;
		using (List<CachedHeroInfo>.Enumerator enumerator = heroInfoList.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				heroInfo = enumerator.Current;
			}
		}
		return new OpenTopHeroInfo(heroInfo, Hero3DViewPanel.EToggle.Info);
	}

	public OpenTopHeroInfo(CachedHeroInfo heroInfo, Hero3DViewPanel.EToggle toggle)
	{
		_heroInfo = heroInfo;
		_toggle = toggle;
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyHero(_heroInfo, _toggle);
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
