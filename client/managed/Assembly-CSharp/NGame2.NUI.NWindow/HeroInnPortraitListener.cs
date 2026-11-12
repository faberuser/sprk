using System;

namespace NGame2.NUI.NWindow;

public class HeroInnPortraitListener : HeroInnPortrait.Listener
{
	private Action<int> _onClickPortrait;

	public HeroInnPortraitListener(Action<int> onClickPortrait)
	{
		_onClickPortrait = onClickPortrait;
	}

	void HeroInnPortrait.Listener.OnClickPortrait(int heroIndex)
	{
		Vespa_System_Action_Extension_Method.fire(_onClickPortrait, heroIndex);
	}
}
