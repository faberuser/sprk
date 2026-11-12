using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class InspectEquipTreasure : BaseWindow, TreasureSlotComponent.Listener, TreasureStatViewer.IDataProvider
{
	[Serializable]
	public class TreasureSlotUI
	{
		public Transform Root_TreasureSlotComponent;

		public UISprite Sprite_Lock;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Hero/InspectEquipTreasure";

	public TreasureSlotUI[] TreasureSlotUIs;

	public UIPanel Panel_Stat;

	public UIScrollView ScrollView_Stat;

	private CachedHeroInfo _selectdCachedHeroInfo;

	private EquipItemInfo _equipItemInfo;

	private List<TreasureSlotComponent> _treasureComponentList = new List<TreasureSlotComponent>();

	private TreasureStatViewer _treasureStatViewer;

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	private void Clear()
	{
		FactoryBase.ReleaseAndClearList(_treasureComponentList);
		ClearStatList();
	}

	private void ClearStatList()
	{
		if (_treasureStatViewer != null)
		{
			_treasureStatViewer.Clear();
		}
	}

	public void Open(EquipItemInfo equipItemInfo, CachedHeroInfo cachedHeroInfo)
	{
		if (cachedHeroInfo == null)
		{
			VespaDebug.LogError("HeroEquipTreasure::Open listener is null");
			return;
		}
		_equipItemInfo = equipItemInfo;
		_selectdCachedHeroInfo = cachedHeroInfo;
		base.Open();
		Init();
	}

	private void Init()
	{
		if (_treasureStatViewer == null)
		{
			_treasureStatViewer = new TreasureStatViewer(this);
		}
		Refresh();
	}

	private void Refresh()
	{
		CreateTreasureSlotComponent();
		SetupStat();
	}

	private void CreateTreasureSlotComponent()
	{
		List<EquipSlotInfo> treasureSlotInfoList = GetTreasureSlotInfoList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(treasureSlotInfoList) && i < __Extension_Method_ICollection__.COUNT(TreasureSlotUIs); i++)
		{
			if (TreasureSlotUIs[i] == null)
			{
				VespaDebug.LogError("TreasureSlotUIs[i] == null");
				continue;
			}
			EquipSlotInfo equipSlotinfo = treasureSlotInfoList[i];
			UIHelper.EnableGameObject(TreasureSlotUIs[i].Sprite_Lock, enable: false);
			TreasureSlotComponent treasureSlotComponent = TreasureSlotComponent.PrefabAuto.Factory.Create(TreasureSlotUIs[i].Root_TreasureSlotComponent);
			treasureSlotComponent.Open(this, equipSlotinfo, _selectdCachedHeroInfo);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(treasureSlotComponent.transform);
			treasureSlotComponent.AdjustToParentWidgetScale();
			_treasureComponentList.Add(treasureSlotComponent);
		}
	}

	private List<EquipSlotInfo> GetTreasureSlotInfoList()
	{
		List<EquipSlotInfo> equipItemInfoList = new List<EquipSlotInfo>();
		Array.ForEach(EquipPartType.Treasure.GetEquipSlotIndices(), delegate(int slotIndex)
		{
			EquipItemInfo equipItemInfoforEquipSlotIndex = GetEquipItemInfoforEquipSlotIndex(slotIndex);
			equipItemInfoList.Add(new EquipSlotInfo(slotIndex, equipItemInfoforEquipSlotIndex));
		});
		return equipItemInfoList;
	}

	private EquipItemInfo GetEquipItemInfoforEquipSlotIndex(int equipSlotIndex)
	{
		if (_selectdCachedHeroInfo == null)
		{
			return null;
		}
		return _selectdCachedHeroInfo.GetEquipItemInfoByPartIndex(equipSlotIndex);
	}

	public void SetupStat()
	{
		_treasureStatViewer.SetupStat();
	}

	public void Refresh(CachedHeroInfo currentHeroInfo)
	{
		if (currentHeroInfo == null || currentHeroInfo != _selectdCachedHeroInfo)
		{
			Close();
			return;
		}
		Clear();
		Refresh();
	}

	public void OnClickClose()
	{
		Close();
	}

	void TreasureSlotComponent.Listener.OnClick(TreasureSlotComponent caller)
	{
	}

	int TreasureStatViewer.IDataProvider.GetMainSlotIndex()
	{
		return EquipPartType.Treasure.GetEquipSlotIndex();
	}

	MonoBehaviour TreasureStatViewer.IDataProvider.GetMonoBehaviour()
	{
		return this;
	}

	List<EquipSlotInfo> TreasureStatViewer.IDataProvider.GetHeroEquippedTreasureList()
	{
		return GetTreasureSlotInfoList();
	}

	UIPanel TreasureStatViewer.IDataProvider.GetPanel()
	{
		return Panel_Stat;
	}

	UIScrollView TreasureStatViewer.IDataProvider.GetScrollView()
	{
		return ScrollView_Stat;
	}
}
