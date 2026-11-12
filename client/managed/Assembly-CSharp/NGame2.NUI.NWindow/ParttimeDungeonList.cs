using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class ParttimeDungeonList : BaseWindowUpdater, DungeonListComponent.Listener, IUserObserver<PlayerPurchaseTimeDurationInfo>, IUserObserver
{
	public interface Listener
	{
		void RequestReloadBoosterInfo();

		void OnClickEnter(PurchaseDungeonData putchaseDungeonData);

		void OnClickBuy(PurchaseDungeonData putchaseDungeonData);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/ParttimeDungeon/ParttimeDungeonList";

	public UIScrollGrid RaidGrid;

	private Listener _listener;

	private List<DungeonListComponent> _componentList = new List<DungeonListComponent>();

	public void Open(Listener listener)
	{
		if (listener == null)
		{
			VespaDebug.LogError("listener == null");
			return;
		}
		base.Open();
		SetUpdatePeriod(1f);
		_listener = listener;
		RequestReloadBoosterInfo();
		InitList();
	}

	private void RequestReloadBoosterInfo()
	{
		_listener.RequestReloadBoosterInfo();
	}

	public override void Reset()
	{
		base.Reset();
		ClearList();
	}

	private void ClearList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	public void InitList()
	{
		ClearList();
		foreach (PurchaseDungeonData allDatum in GameRoot.Instance.PurchaseDungeonDataContainer.GetAllData())
		{
			if (allDatum != null && allDatum.Buyable)
			{
				DungeonListComponent dungeonListComponent = UIHelper.AddGridItem(RaidGrid, DungeonListComponent.PrefabAuto);
				dungeonListComponent.Open(this, allDatum);
				_componentList.Add(dungeonListComponent);
			}
		}
		UIHelper.RepositionGrid(RaidGrid);
	}

	public override void UpdatePeriodically()
	{
		base.UpdatePeriodically();
		UpdateComponents();
	}

	private void UpdateComponents()
	{
		_componentList.ForEach(delegate(DungeonListComponent comp)
		{
			comp.UpdateUI();
		});
	}

	void DungeonListComponent.Listener.OnClickEnter(PurchaseDungeonData purchaseDungeonData)
	{
		if (_listener != null)
		{
			_listener.OnClickEnter(purchaseDungeonData);
		}
	}

	void DungeonListComponent.Listener.OnClickBuy(PurchaseDungeonData purchaseDungeonData)
	{
		if (_listener != null)
		{
			_listener.OnClickBuy(purchaseDungeonData);
		}
	}

	void IUserObserver<PlayerPurchaseTimeDurationInfo>.OnChanged(PlayerPurchaseTimeDurationInfo changedInfo)
	{
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
