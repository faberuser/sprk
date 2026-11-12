using System;
using System.Collections.Generic;
using NGame2.NUI.NHelper;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class HeroRunehengeIcon : MonoBehaviour, HeroPresetStorageWebManager.ILoadPresetListener
{
	public interface IDataProvider
	{
		CachedHeroInfo GetHeroInfo();
	}

	[Serializable]
	public class SlotData
	{
		public RuneSlotType Slot;

		public GameObject Root;
	}

	public List<SlotData> SlotDatas;

	private IDataProvider _dataProvider;

	private Dictionary<RuneSlotType, HeroRunehengeIconSlotComponent> _runeHengeIconSlotCompDic = new Dictionary<RuneSlotType, HeroRunehengeIconSlotComponent>();

	public void Open(IDataProvider dataProvider)
	{
		_dataProvider = dataProvider;
		SingletonAuto<HeroPresetStorageWebManager>.instance.AddLoadPresetListener(this);
		foreach (SlotData slotData in SlotDatas)
		{
			if (slotData != null && !_runeHengeIconSlotCompDic.ContainsKey(slotData.Slot))
			{
				HeroRunehengeIconSlotComponent value = HeroRunehengeIconSlotComponent.PrefabAuto.Factory.Create(slotData.Root.transform);
				_runeHengeIconSlotCompDic.Add(slotData.Slot, value);
			}
		}
		Refresh();
	}

	public void Close()
	{
		SingletonAuto<HeroPresetStorageWebManager>.instance.RemoveLoadPresetListener(this);
		FactoryBase.ReleaseAndClearList(_runeHengeIconSlotCompDic);
	}

	public void Refresh()
	{
		if (_dataProvider == null)
		{
			return;
		}
		List<HeroRunePageInfo> appliedRunePageInfoList = RuneHelper.GetAppliedRunePageInfoList(_dataProvider.GetHeroInfo());
		if (__Extension_Method_ICollection__.IsNullOrEmpty(appliedRunePageInfoList))
		{
			foreach (KeyValuePair<RuneSlotType, HeroRunehengeIconSlotComponent> item in _runeHengeIconSlotCompDic)
			{
				HeroRunehengeIconSlotComponent value = item.Value;
				if (!(value == null))
				{
					value.Refresh(RuneColorType.None);
				}
			}
			return;
		}
		Dictionary<RuneSlotType, RuneColorType> dictionary = new Dictionary<RuneSlotType, RuneColorType>();
		foreach (HeroRunePageInfo item2 in appliedRunePageInfoList)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(item2.ItemIndex);
			RuneSlotType slotNum = (RuneSlotType)item2.SlotNum;
			dictionary[slotNum] = RuneHelper.GetRuneColorType(data);
		}
		foreach (KeyValuePair<RuneSlotType, HeroRunehengeIconSlotComponent> item3 in _runeHengeIconSlotCompDic)
		{
			RuneSlotType key = item3.Key;
			HeroRunehengeIconSlotComponent value2 = item3.Value;
			if (!(value2 == null))
			{
				if (dictionary.ContainsKey(key))
				{
					value2.Refresh(dictionary[key]);
				}
				else
				{
					value2.Refresh(RuneColorType.None);
				}
			}
		}
	}

	void HeroPresetStorageWebManager.ILoadPresetListener.OnBeforeLoadPreset()
	{
	}

	void HeroPresetStorageWebManager.ILoadPresetListener.OnAfterLoadPreset()
	{
		Refresh();
	}
}
