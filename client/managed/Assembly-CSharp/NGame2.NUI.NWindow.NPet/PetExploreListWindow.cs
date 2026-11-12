using System.Collections.Generic;
using NGame2.NEventTrigger;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.NPet;
using NShared;
using NShared.NEvent;
using NShared.PetCancelExplore;
using NShared.PetEndExplore;
using NShared.PetStartExplore;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NPet;

public class PetExploreListWindow : BaseWindow, PetManagement.IPetExploreWebListener
{
	public interface IListener : PetExploreListComponent.IListener
	{
	}

	public static string PrefabPath = "Prefab2/Window/Pet/PetExploreListWindow";

	public UIScrollGrid ExploreScrollGrid;

	private List<PetExploreListComponent> _listComponents = new List<PetExploreListComponent>();

	private IListener _listener;

	public void Open(IListener listener)
	{
		_listener = listener;
		SceneAutoBehaviour<PetManagement>.instance.AddPetExploreWebListener(this);
		Open(delegate
		{
			SetExploreListComponent();
			OpenAdventureUI triggerEvent = new OpenAdventureUI();
			SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
		}, delegate
		{
		});
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ReleaseExploreListComponent();
		SceneAutoBehaviour<PetManagement>.instance.RemovePetExploreWebListener(this);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listComponents); i++)
		{
			PetExploreListComponent petExploreListComponent = _listComponents[i];
			if (petExploreListComponent != null)
			{
				petExploreListComponent.OnUpdate();
			}
		}
	}

	private List<PetAdventureData> GetExploreDatatList()
	{
		return GameRoot.Instance.PetAdventureDataContainer.GetDataListByType(EPetAdventureType.Explore);
	}

	private void SetExploreListComponent()
	{
		ReleaseExploreListComponent();
		List<PetAdventureData> exploreDatatList = GetExploreDatatList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(exploreDatatList); i++)
		{
			PetAdventureData petAdventureData = exploreDatatList[i];
			if (petAdventureData != null)
			{
				PetExploreListComponent petExploreListComponent = UIHelper.AddGridItem(ExploreScrollGrid, PetExploreListComponent.PrefabAuto);
				if (!(petExploreListComponent == null))
				{
					petExploreListComponent.Open(petAdventureData, _listener);
					_listComponents.Add(petExploreListComponent);
				}
			}
		}
		UIHelper.ResetGridPosition(ExploreScrollGrid);
	}

	private void Refresh()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listComponents); i++)
		{
			PetExploreListComponent petExploreListComponent = _listComponents[i];
			if (!(petExploreListComponent == null))
			{
				petExploreListComponent.Refresh();
			}
		}
	}

	private void ReleaseExploreListComponent()
	{
		FactoryBase.ReleaseAndClearList(_listComponents);
	}

	void PetManagement.IPetExploreWebListener.OnStart(NShared.PetStartExplore.Response reponse)
	{
		Refresh();
	}

	void PetManagement.IPetExploreWebListener.OnCancel(NShared.PetCancelExplore.Response reponse)
	{
		Refresh();
	}

	void PetManagement.IPetExploreWebListener.OnEnd(NShared.PetEndExplore.Response reponse)
	{
		Refresh();
	}
}
