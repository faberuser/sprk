using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class OpenEntryMenuShop : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime, Shop.Listener
{
	bool Shop.Listener.OnCloseRequest(Shop closeThis)
	{
		return true;
	}

	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		Vespa_Common_Class_Extension_Method.Interface((ITutorialRuntimeImmediate)this).RunImmediate(window, tutorialData);
		yield break;
	}

	void ITutorialRuntimeImmediate.RunImmediate(TutorialWindow window, TutorialData tutorialData)
	{
		NGame2.NUI.NWindow.EntryMenu instance = SceneAutoBehaviour<EntryMenuManagement>.instance.GetInstance<NGame2.NUI.NWindow.EntryMenu>();
		Shop instance2 = SceneAutoBehaviour<ShopManagement>.instance.GetInstance<Shop>();
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		ShopData data = GameRoot.Instance.ShopDataContainer.GetData(argInt);
		if (data == null)
		{
			VespaDebug.LogError("shop data not found. shopCode=" + argInt);
			return;
		}
		instance2.title.Key = "SHOP_NAME_" + data.Code.ToUpper();
		instance2.Open();
		instance2.Init(this, data, instance.GetNpcData());
	}
}
