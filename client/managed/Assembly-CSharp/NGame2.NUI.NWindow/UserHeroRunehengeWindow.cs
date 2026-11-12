using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class UserHeroRunehengeWindow : BaseWindow, HeroRuneStatPanel.IDataProvider, HeroRuneItemInfo.IListener
{
	[Serializable]
	public class RuneSlotData
	{
		public RuneSlotType Slot;

		public UIButton Button_Slot;

		public UITexture Texture_Icon;

		public UIGame3DWidget Effect_EquipLoop;

		public ItemData ItemData;

		public EquipItemInfo EquipItemInfo;
	}

	[Serializable]
	public class RunePageData
	{
		public enum RunePageState
		{
			Select,
			NotSelect,
			Unavailable
		}

		public int RunePageIndex;

		public GameObject PageRoot;

		public UISpriteSelector SpriteSelector;

		private RunePageState State;

		public void ChangeRunePageState(RunePageState state)
		{
			switch (state)
			{
			case RunePageState.Unavailable:
				UIHelper.EnableGameObject(PageRoot, enable: false);
				break;
			case RunePageState.Select:
			case RunePageState.NotSelect:
				UIHelper.EnableGameObject(PageRoot, enable: true);
				SpriteSelector.SelectByIndex((int)state);
				break;
			}
			State = state;
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Hero/UserHeroRunehenge";

	public HeroRuneStatPanel RuneStatPanel;

	public List<RunePageData> RunePageDatas;

	public List<RuneSlotData> RuneSlotDatas;

	public GameObject HeroRuneItemInfoRoot;

	public UIGrid Grid_RunePage;

	public UILabel Label_HeroName;

	private CachedHeroInfo _heroInfo;

	private HeroRuneItemInfo _runeItemInfo;

	private Dictionary<RuneSlotType, RuneSlotData> _runeSlotDataDic = new Dictionary<RuneSlotType, RuneSlotData>();

	private Dictionary<int, RunePageData> _runePageDataDic = new Dictionary<int, RunePageData>();

	public int HeroIndex
	{
		get
		{
			if (_heroInfo == null)
			{
				return 0;
			}
			return _heroInfo.HeroIndex;
		}
	}

	public int CurrentRunePage { get; set; }

	public void Open(CachedHeroInfo heroInfo)
	{
		base.Open(delegate
		{
			_heroInfo = heroInfo;
			Init();
		});
	}

	private void Init()
	{
		if (RuneStatPanel != null)
		{
			RuneStatPanel.Open(this);
		}
		_runeSlotDataDic.Clear();
		_runePageDataDic.Clear();
		CurrentRunePage = ((_heroInfo.ApplyRunePage == 0) ? RuneHelper.GetDefaultRunePage() : _heroInfo.ApplyRunePage);
		foreach (RuneSlotData runeSlotData in RuneSlotDatas)
		{
			_runeSlotDataDic[runeSlotData.Slot] = runeSlotData;
		}
		foreach (RunePageData runePageData in RunePageDatas)
		{
			_runePageDataDic[runePageData.RunePageIndex] = runePageData;
		}
		SetHeroName();
		Refresh();
		CloseRuneItemInfo();
	}

	public void Refresh()
	{
		SetupRunePage();
		SetupRuneLoopEffect();
		if (RuneStatPanel != null)
		{
			RuneStatPanel.Refresh();
		}
	}

	private void SetupRunePage()
	{
		if (_heroInfo == null)
		{
			return;
		}
		int maxRunePage = RuneHelper.GetMaxRunePage();
		int num = 0;
		for (int i = 1; i <= maxRunePage; i++)
		{
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(_heroInfo.GetRunePageInfos(i)))
			{
				num = Math.Max(i, num);
			}
		}
		for (int j = 1; j <= maxRunePage; j++)
		{
			RunePageData runePageData = GetRunePageData(j);
			if (runePageData != null)
			{
				if (j > num)
				{
					runePageData.ChangeRunePageState(RunePageData.RunePageState.Unavailable);
				}
				else if (j == CurrentRunePage)
				{
					runePageData.ChangeRunePageState(RunePageData.RunePageState.Select);
				}
				else
				{
					runePageData.ChangeRunePageState(RunePageData.RunePageState.NotSelect);
				}
			}
		}
		SetupRuneSlot(_heroInfo.GetRunePageInfos(CurrentRunePage));
		if (Grid_RunePage != null)
		{
			Grid_RunePage.Reposition();
			Grid_RunePage.repositionNow = true;
		}
	}

	public void SetupRuneLoopEffect()
	{
		foreach (KeyValuePair<RuneSlotType, RuneSlotData> item in _runeSlotDataDic)
		{
			UIHelper.EnableGameObject(item.Value.Effect_EquipLoop, item.Value.ItemData != null && item.Value.ItemData.Type == ItemType.PunishmentRune);
		}
	}

	private void SetupRuneSlot(List<HeroRunePageInfo> infos)
	{
		foreach (HeroRunePageInfo info in infos)
		{
			RuneSlotData runeSlotData = GetRuneSlotData((RuneSlotType)info.SlotNum);
			if (runeSlotData == null)
			{
				continue;
			}
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(info.ItemIndex);
			if (data == null)
			{
				UIHelper.SetTexture(runeSlotData.Texture_Icon, null);
			}
			else if (data.Type == ItemType.Rune || data.Type == ItemType.PunishmentRune)
			{
				runeSlotData.ItemData = data;
				runeSlotData.EquipItemInfo = null;
				UIHelper.ResetPlaneAssetTextureAsync(this, runeSlotData.Texture_Icon, UIHelper.GetItemIcon2(data.Code));
				if (data.Type == ItemType.PunishmentRune)
				{
					EquipItemInfo equipItemInfo = new EquipItemInfo();
					equipItemInfo.ItemIndex = info.ItemIndex;
					equipItemInfo.PunishmentRuneOption = _heroInfo.GetPunishmentRuneOptionInfo(info);
					runeSlotData.EquipItemInfo = equipItemInfo;
				}
			}
			else
			{
				runeSlotData.ItemData = null;
				runeSlotData.EquipItemInfo = null;
				UIHelper.SetTexture(runeSlotData.Texture_Icon, null);
			}
		}
	}

	private RuneSlotData GetRuneSlotData(RuneSlotType slot)
	{
		if (_runeSlotDataDic.ContainsKey(slot))
		{
			return _runeSlotDataDic[slot];
		}
		return null;
	}

	private RunePageData GetRunePageData(int pageIndex)
	{
		if (_runePageDataDic.ContainsKey(pageIndex))
		{
			return _runePageDataDic[pageIndex];
		}
		return null;
	}

	public override void Reset()
	{
		base.Reset();
	}

	public override void Close()
	{
		if (RuneStatPanel != null)
		{
			RuneStatPanel.Close();
		}
		FactoryBase.ReleaseComponent(_runeItemInfo);
		_runeItemInfo = null;
		base.Close();
	}

	private void SetHeroName()
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(_heroInfo.HeroIndex);
		if (data != null)
		{
			HeroProfileData data2 = GameRoot.Instance.HeroProfileDataContainer.GetData(_heroInfo.HeroIndex);
			string empty = string.Empty;
			empty = ((data2 == null || String_Extension_Method.IsNullOrEmpty(data2.HeroName)) ? data.Name : LocalizationManager.GetStringDirect(data2.HeroName));
			UIHelper.SetLabelText(Label_HeroName, LocalizationManager.GetStringDirect("RUNE_PAGE_TITLE", empty));
		}
	}

	private void OpenRuneItemInfo(RuneSlotType slotType, ItemData itemData, EquipItemInfo equipItemInfo)
	{
		if (_runeItemInfo == null)
		{
			_runeItemInfo = HeroRuneItemInfo.PrefabAuto.Factory.Create(HeroRuneItemInfoRoot.transform);
		}
		HeroRuneItemInfo runeItemInfo = _runeItemInfo;
		int num = 0;
		runeItemInfo.Open(this, slotType, HeroRuneItemInfo.EMode.ReadOnly, itemData, num, equipItemInfo);
	}

	private void CloseRuneItemInfo()
	{
		if (!(_runeItemInfo == null))
		{
			_runeItemInfo.Close();
		}
	}

	public void OnClickRunePage(GameObject root)
	{
		RunePageData runePageData = RunePageDatas.Find((RunePageData x) => x.PageRoot == root);
		if (runePageData != null && _heroInfo != null)
		{
			CurrentRunePage = runePageData.RunePageIndex;
			Refresh();
		}
	}

	public void OnClickRuneSlot(UIButton button)
	{
		RuneSlotData runeSlotData = RuneSlotDatas.Find((RuneSlotData x) => x.Button_Slot == button);
		if (runeSlotData != null && runeSlotData.ItemData != null)
		{
			OpenRuneItemInfo(runeSlotData.Slot, runeSlotData.ItemData, runeSlotData.EquipItemInfo);
		}
	}

	CachedHeroInfo HeroRuneStatPanel.IDataProvider.GetHeroInfo()
	{
		return _heroInfo;
	}

	void HeroRuneItemInfo.IListener.OnClose(HeroRuneItemInfo caller)
	{
		FactoryBase.ReleaseComponent(_runeItemInfo);
		_runeItemInfo = null;
	}

	void HeroRuneItemInfo.IListener.OnEquip(HeroRuneItemInfo caller)
	{
	}

	void HeroRuneItemInfo.IListener.OnUnequip(HeroRuneItemInfo caller)
	{
	}

	int HeroRuneStatPanel.IDataProvider.GetRunePage()
	{
		return CurrentRunePage;
	}
}
