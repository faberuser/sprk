using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared.RestoreArtifact;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class ArtifactManagement : BaseManagement<ArtifactManagement>, RestoreArtifact.Listener
{
	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<RestoreArtifact>("Resources/Prefab2/Window/Artifact/RestoreArtifact");
	}

	private void Reset()
	{
	}

	public void OpenRestoreArtifact()
	{
		GetInstance<RestoreArtifact>().Open(this);
	}

	void RestoreArtifact.Listener.OnClickRestore(RestoreArtifact caller)
	{
		Request request = new Request
		{
			ArtifactRestoreIndex = caller.ArtifactRestoreIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(Response response)
		{
			if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItem);
				if (response.EquipItem != null)
				{
					RestoreArtifactResultPopup.Open(response.EquipItem, SceneAutoBehaviour<MyHeroManagement>.instance.OpenInventory);
				}
			}
		});
	}
}
