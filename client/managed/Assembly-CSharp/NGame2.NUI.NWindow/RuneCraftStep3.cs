using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RuneCraftStep3 : RuneCraftStepBase, ItemIconComponent.Listener, RuneItemDetailComponent.Listener
{
	[Header("------------------------------------------")]
	[Header("[RuneCraftStep3]")]
	public Transform RightUIRoot;

	public Transform MaterialItemRoot;

	public UIScrollGrid Grid;

	public UIScrollGrid GridDetail;

	public UIButton ButtonNextStep;

	public UILabel Label_Title;

	public UILabel Label_StatType;

	public UILabel Label_StatValue;

	public UILabel Label_NotHaveRune;

	public UILabel Label_EmptySlot;

	public UISpriteSelector DetailModeIconSelector;

	[Header("[Effect]")]
	public MeshRenderer Gauge;

	public UIGame3DWidget EffectHit;

	public UIGame3DWidget EffectFullGauge;

	public UIGame3DWidget EffectFullLoop;

	public UIPlayAssetSound GaugeSound;

	public GameObject FinalCircle;

	private int _curSelectedIndex;

	private bool _isDetailMode;

	private ItemIconComponent _materialItemIconComp;

	private Dictionary<int, Tuple<ItemIconComponent, RuneItemDetailComponent>> _dicItemIconComp = new Dictionary<int, Tuple<ItemIconComponent, RuneItemDetailComponent>>();

	public override void Init()
	{
		base.Init();
		_isDetailMode = false;
		_curSelectedIndex = 0;
		UIHelper.EnableGameObject(RightUIRoot, enable: true);
		UIHelper.EnableGameObject(ButtonNextStep, enable: true);
		UIHelper.EnableGameObject(FinalCircle, enable: true);
		UIHelper.EnableGameObject(ButtonNextStep, enable: true);
		CreateRuneComponents();
		UpdateDetailViewMode();
		UpdateSetSelectItemIconComponent(null);
		UpdateExpectedStat();
		SetGauge(0f);
		UpdateButtonNextStep();
	}

	private void UpdateExpectedStat()
	{
		if (_materialItemIconComp == null)
		{
			UIHelper.SetLabelText(Label_StatType, string.Empty);
			UIHelper.SetLabelText(Label_StatValue, string.Empty);
			UIHelper.EnableGameObject(Label_EmptySlot, enable: true);
			UIHelper.EnableGameObject(Label_Title, enable: false);
			return;
		}
		List<KeyValuePair<Enum, long>> list = (_materialItemIconComp.ItemData as ItemData<RuneItemData>).SubData.GetStatTypeAndValue().ToList();
		string text = LocalizationManager.GetString(list[0].Key);
		PunishmentRuneData data = GameRoot.Instance.PunishmentRuneDataContainer.GetData(OwnerWindow.CachedVoidRuneIndex, _curSelectedIndex);
		PunishmentRuneOptionData data2 = GameRoot.Instance.PunishmentRuneOptionDataContainer.GetData(data.MainOptionIndex);
		string statDisplayValue = StatData.GetStatDisplayValue(list[0].Key, data2.OptionValueMin0);
		string statDisplayValue2 = StatData.GetStatDisplayValue(list[0].Key, data2.OptionValueMax0);
		UIHelper.SetLabelText(Label_StatType, text);
		UIHelper.SetLabelText(Label_StatValue, string.Format("{0}~{1}", statDisplayValue.Replace("%", ""), statDisplayValue2.Replace("+", "")));
		UIHelper.EnableGameObject(Label_EmptySlot, enable: false);
		UIHelper.EnableGameObject(Label_Title, enable: true);
	}

	private void CreateRuneComponents()
	{
		foreach (ItemInfo item in GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.Rune))
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(item.ItemIndex);
			data.GetSubData<RuneItemData>();
			if (CheckCondition(data))
			{
				OwnerWindow.DicMaterialCount.TryGetValue(data.Index, out var value);
				long num = value;
				int count = item.Count;
				long num2 = num;
				int num3 = count;
				long num4 = checked(num3 - num2);
				int num5 = 0;
				if (num4 > num5)
				{
					ItemIconComponent itemIconComponent = UIHelper.AddGridItem(Grid, ItemIconComponent.PrefabAuto);
					itemIconComponent.Open(data, this);
					itemIconComponent.SetItemCount(num4);
					RuneItemDetailComponent runeItemDetailComponent = UIHelper.AddGridItem(GridDetail, RuneItemDetailComponent.PrefabAuto);
					runeItemDetailComponent.Open(item, data, this);
					runeItemDetailComponent.SetItemCount(num4);
					_dicItemIconComp.Add(item.ItemIndex, new Tuple<ItemIconComponent, RuneItemDetailComponent>(itemIconComponent, runeItemDetailComponent));
				}
			}
		}
		UIHelper.RepositionGrid(Grid);
		UIHelper.RepositionGrid(GridDetail);
		UIHelper.EnableGameObject(Label_NotHaveRune, __Extension_Method_ICollection__.IsNullOrEmpty(_dicItemIconComp));
	}

	private bool CheckCondition(ItemData itemData)
	{
		if (itemData.Grade != 6)
		{
			return false;
		}
		bool result = false;
		foreach (PunishmentRuneData allDatum in GameRoot.Instance.PunishmentRuneDataContainer.GetAllData())
		{
			if (allDatum.VoidRuneIndex == OwnerWindow.CachedVoidRuneIndex && allDatum.ApostleBloodIndex == OwnerWindow.CachedApostleBloodIndex && allDatum.LegendRuneIndex == itemData.Index)
			{
				result = true;
			}
		}
		return result;
	}

	public override void Close()
	{
		base.Close();
		UIHelper.SetLabelText(Label_StatType, string.Empty);
		UIHelper.SetLabelText(Label_StatValue, string.Empty);
		UIHelper.EnableGameObject(RightUIRoot, enable: false);
		foreach (KeyValuePair<int, Tuple<ItemIconComponent, RuneItemDetailComponent>> item in _dicItemIconComp)
		{
			FactoryBase.ReleaseComponent(item.Value.Item1);
			item.Value.Item2.Close();
			FactoryBase.ReleaseComponent(item.Value.Item2);
		}
		_dicItemIconComp.Clear();
		FactoryBase.ReleaseComponent(_materialItemIconComp);
		_materialItemIconComp = null;
		SetGauge(0f);
		EffectHit.Stop(immediatelyClear: true);
		EffectFullGauge.Stop(immediatelyClear: true);
		EffectFullLoop.Stop(immediatelyClear: true);
		UIHelper.EnableGameObject(FinalCircle, enable: false);
	}

	private void AddMaterialItemComponent(ItemIconComponent selectedComp)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(selectedComp.ItemData.Index).Locked != 0)
		{
			return;
		}
		if (_materialItemIconComp != null)
		{
			if (_materialItemIconComp.ItemIndex == selectedComp.ItemIndex)
			{
				return;
			}
			FactoryBase.ReleaseComponent(_materialItemIconComp);
			_materialItemIconComp = null;
		}
		_materialItemIconComp = ItemIconComponent.PrefabAuto.Factory.Create(MaterialItemRoot);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_materialItemIconComp.transform);
		_materialItemIconComp.Open(selectedComp.ItemData);
		UpdateSetSelectItemIconComponent(selectedComp);
		UpdateExpectedStat();
		SetGauge(1f);
		EffectHit.Play(null);
		if (EffectFullGauge.IsPlaying)
		{
			EffectFullGauge.Stop(immediatelyClear: true);
		}
		EffectFullGauge.Play(null);
		EffectFullLoop.Play(null);
		GaugeSound.Play();
		UpdateButtonNextStep();
	}

	private void UpdateSetSelectItemIconComponent(ItemIconComponent selectedComp)
	{
		if (selectedComp == null)
		{
			_curSelectedIndex = 0;
		}
		else if (_dicItemIconComp.ContainsKey(selectedComp.ItemIndex))
		{
			if (_curSelectedIndex != 0)
			{
				_dicItemIconComp[_curSelectedIndex].Item1.SetSelected(selected: false);
				_dicItemIconComp[_curSelectedIndex].Item2.SetSelected(selected: false);
			}
			_curSelectedIndex = selectedComp.ItemIndex;
			_dicItemIconComp[_curSelectedIndex].Item1.SetSelected(selected: true);
			_dicItemIconComp[_curSelectedIndex].Item2.SetSelected(selected: true);
		}
	}

	private void UpdateDetailViewMode()
	{
		UIHelper.EnableGameObject(Grid, !_isDetailMode);
		UIHelper.EnableGameObject(GridDetail, _isDetailMode);
		UIHelper.ResetGridPosition(Grid);
		UIHelper.ResetGridPosition(GridDetail);
		DetailModeIconSelector.Select(_isDetailMode ? "Detail" : "Normal");
	}

	private void SetGauge(float value)
	{
		if (!(Gauge == null))
		{
			if (value >= 0.9999f)
			{
				value = 1.1f;
			}
			Gauge.material.SetFloat(_gaugePropertyName, value);
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		AddMaterialItemComponent(itemIconComponent);
	}

	void RuneItemDetailComponent.Listener.OnClick(ItemIconComponent component)
	{
		AddMaterialItemComponent(component);
	}

	public void OnClickChangeViewMode()
	{
		_isDetailMode = !_isDetailMode;
		UpdateDetailViewMode();
	}

	private void UpdateButtonNextStep()
	{
		UIHelper.EnableButton(ButtonNextStep, _materialItemIconComp != null);
	}

	public void OnClickNextStep()
	{
		if (!(_materialItemIconComp == null))
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.IsMaxPunishmentRuneItemCount())
			{
				GlobalCenterMessage.Open(LocalizationManager.GetStringDirect("PUNISHMENT_RUNE_OVER_LIMIT"));
				return;
			}
			UIHelper.EnableGameObject(ButtonNextStep, enable: false);
			OwnerWindow.DicMaterialCount.Clear();
			OwnerWindow.CachedLegendRuneIndex = _curSelectedIndex;
			OwnerWindow.UpdateStep();
			UIHelper.EnableGameObject(OwnerWindow.ButtonHelp, enable: false);
			UIHelper.EnableGameObject(OwnerWindow.LabelPunishmentRuneCount, enable: false);
		}
	}
}
