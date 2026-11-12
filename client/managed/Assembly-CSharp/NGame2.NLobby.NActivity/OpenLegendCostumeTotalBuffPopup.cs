using System.Collections;
using NGame2.NUI.NWindow;

namespace NGame2.NLobby.NActivity;

public class OpenLegendCostumeTotalBuffPopup : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new OpenLegendCostumeTotalBuffPopup();
	}

	IEnumerator IActivity.coStart()
	{
		LegendCostumeTotalBuffPopup.OpenPopup();
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
