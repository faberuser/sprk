using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.Mission;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenPayShop : IActivity
{
	private string PayShopGroupCategory = "";

	public static IActivity Create(string[] values)
	{
		string valueString = ArgParser.GetValueString(values, 0);
		return new OpenPayShop
		{
			PayShopGroupCategory = valueString
		};
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<MissionManagement>.instance.CloseAllActiveWindow(immediately: true);
		SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(PayShopGroupCategory);
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
