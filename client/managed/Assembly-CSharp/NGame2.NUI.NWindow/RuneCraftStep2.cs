using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RuneCraftStep2 : RuneCraftStepBase, ItemIconComponent.Listener
{
	[Header("------------------------------------------")]
	[Header("[RuneCraftStep2]")]
	public Transform RightUIRoot;

	public Transform BloodRoot;

	public Transform VoidRuneRoot;

	public UIScrollGrid BloodGrid;

	public UIScrollGrid VoidRuneGrid;

	public UIScrollView SkillInfoScrollView;

	public UIButton ButtonNextStep;

	public UILabel Label_GuideText;

	public UILabel Label_BloodCount;

	public UILabel Label_NotHaveBlood;

	public UILabel Label_NotHaveVoid;

	public UILabel Label_SkillInfo1;

	public UILabel Label_SkillInfo2;

	public UILabel Label_SkillInfo3;

	public GameObject BloodInfo;

	public GameObject VoidRuneInfo;

	public GameObject SkillInfo;

	public GameObject ActivateEffectSlotBlood;

	public GameObject ActivateEffectSlotVoidfRune;

	[Header("[Effect]")]
	public MeshRenderer GaugeLeft;

	public MeshRenderer GaugeRight;

	public UIGame3DWidget EffectHitLeft;

	public UIGame3DWidget EffectHitRight;

	public UIGame3DWidget EffectFullGaugeLeft;

	public UIGame3DWidget EffectFullGaugeRight;

	public UIGame3DWidget EffectFullLoopLeft;

	public UIGame3DWidget EffectFullLoopRight;

	public GameObject LCircle;

	public GameObject RCircle;

	public UIPlayAssetSound GaugeSound;

	private ItemIconComponent _bloodComp;

	private ItemIconComponent _voidRuneComp;

	private List<ItemIconComponent> _listBloodComp = new List<ItemIconComponent>();

	private List<ItemIconComponent> _listVoidRuneComp = new List<ItemIconComponent>();

	public override void Init()
	{
		base.Init();
		UIHelper.EnableGameObject(RightUIRoot, enable: true);
		UIHelper.EnableGameObject(ButtonNextStep, enable: true);
		UIHelper.EnableGameObject(LCircle, enable: true);
		UIHelper.EnableGameObject(RCircle, enable: true);
		UIHelper.EnableGameObject(ButtonNextStep, enable: true);
		SetGaugeLeft(0f);
		SetGaugeRight(0f);
		InitLeftUI();
		CreateBloodComponentsRight();
		CreateVoidRuneComponentsRight();
		UIHelper.EnableGameObject(BloodGrid, enable: false);
		UIHelper.EnableGameObject(VoidRuneGrid, enable: false);
		EnableBloodListRight();
		UpdateButtonNextStep();
	}

	private void InitLeftUI()
	{
		int apostleBloodCount = OwnerWindow.PunishmentRuneItemCountData.ApostleBloodCount;
		UIHelper.SetLabelText(Label_BloodCount, $"0 / {apostleBloodCount}");
	}

	private void CreateBloodComponentsRight()
	{
		foreach (ItemInfo item in GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.ApostleBlood))
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(item.ItemIndex);
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(BloodGrid, ItemIconComponent.PrefabAuto);
			itemIconComponent.Open(data, this);
			int count = item.Count;
			Color? color = null;
			int num = count;
			itemIconComponent.SetItemCount(num, color);
			_listBloodComp.Add(itemIconComponent);
		}
		UIHelper.RepositionGrid(BloodGrid);
		UIHelper.EnableGameObject(Label_NotHaveBlood, __Extension_Method_ICollection__.IsNullOrEmpty(_listBloodComp));
		UIHelper.EnableGameObject(Label_NotHaveVoid, enable: false);
	}

	private void CreateVoidRuneComponentsRight()
	{
		foreach (ItemInfo item in GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.VoidRune))
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(item.ItemIndex);
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(VoidRuneGrid, ItemIconComponent.PrefabAuto);
			itemIconComponent.Open(data, this);
			int count = item.Count;
			Color? color = null;
			int num = count;
			itemIconComponent.SetItemCount(num, color);
			_listVoidRuneComp.Add(itemIconComponent);
		}
		UIHelper.RepositionGrid(VoidRuneGrid);
		UIHelper.EnableGameObject(Label_NotHaveVoid, __Extension_Method_ICollection__.IsNullOrEmpty(_listVoidRuneComp));
		UIHelper.EnableGameObject(Label_NotHaveBlood, enable: false);
	}

	public override void Close()
	{
		base.Close();
		UIHelper.SetLabelText(Label_BloodCount, string.Empty);
		UIHelper.SetLabelText(Label_SkillInfo1, string.Empty);
		UIHelper.SetLabelText(Label_SkillInfo2, string.Empty);
		UIHelper.SetLabelText(Label_SkillInfo3, string.Empty);
		FactoryBase.ReleaseAndClearList(_listBloodComp);
		FactoryBase.ReleaseAndClearList(_listVoidRuneComp);
		FactoryBase.ReleaseComponent(_bloodComp);
		FactoryBase.ReleaseComponent(_voidRuneComp);
		_bloodComp = null;
		_voidRuneComp = null;
		UIHelper.EnableGameObject(RightUIRoot, enable: false);
		SetGaugeLeft(0f);
		SetGaugeRight(0f);
		EffectHitLeft.Stop(immediatelyClear: true);
		EffectHitRight.Stop(immediatelyClear: true);
		EffectFullLoopLeft.Stop(immediatelyClear: true);
		EffectFullLoopRight.Stop(immediatelyClear: true);
		EffectFullGaugeLeft.Stop(immediatelyClear: true);
		EffectFullGaugeRight.Stop(immediatelyClear: true);
		UIHelper.EnableGameObject(LCircle, enable: false);
		UIHelper.EnableGameObject(RCircle, enable: false);
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (itemIconComponent.ItemData.Type == ItemType.ApostleBlood)
		{
			if (Vespa_Common_Class_Extension_Method.EQUALS(itemIconComponent, _bloodComp))
			{
				EnableBloodListRight();
				return;
			}
			if (_bloodComp != null && _bloodComp.ItemIndex == itemIconComponent.ItemIndex)
			{
				return;
			}
			bool flag = CheckEnoughBloodCount(itemIconComponent.ItemData);
			if (!flag)
			{
				GlobalCenterMessage.Open(LocalizationManager.GetStringDirect("PET_AWAKE_NO_MATERIAL"));
				return;
			}
			UpdateLabelBloodCountLeft(flag);
			ReleaseBloodComponentLeft();
			CreateBloodComponentLeft(itemIconComponent.ItemData);
			UpdateBloodSelectedComponentRight(itemIconComponent);
			SetGaugeLeft(1f);
			EffectHitLeft.Play(null);
			if (EffectFullGaugeLeft.IsPlaying)
			{
				EffectFullGaugeLeft.Stop(immediatelyClear: true);
			}
			EffectFullGaugeLeft.Play(null);
			EffectFullLoopLeft.Play(null);
			GaugeSound.Play();
			if (_voidRuneComp == null)
			{
				EnableVoidListRight();
			}
		}
		else if (itemIconComponent.ItemData.Type == ItemType.VoidRune)
		{
			if (Vespa_Common_Class_Extension_Method.EQUALS(itemIconComponent, _voidRuneComp))
			{
				EnableVoidListRight();
				return;
			}
			if (_voidRuneComp != null && _voidRuneComp.ItemIndex == itemIconComponent.ItemIndex)
			{
				return;
			}
			ReleaseVoidComponentLeft();
			CreateVoidComponentLeft(itemIconComponent.ItemData);
			UpdateVoidRuneSelectedComponentRight(itemIconComponent);
			SetGaugeRight(1f);
			EffectHitRight.Play(null);
			if (EffectFullGaugeRight.IsPlaying)
			{
				EffectFullGaugeRight.Stop(immediatelyClear: true);
			}
			EffectFullGaugeRight.Play(null);
			EffectFullLoopRight.Play(null);
			GaugeSound.Play();
		}
		UpdateSkillInfosRight();
		UpdateButtonNextStep();
	}

	private void UpdateSkillInfosRight()
	{
		if (_bloodComp == null || _voidRuneComp == null)
		{
			return;
		}
		PunishmentRuneData punishmentRuneData = null;
		foreach (PunishmentRuneData allDatum in GameRoot.Instance.PunishmentRuneDataContainer.GetAllData())
		{
			if (_bloodComp.ItemIndex == allDatum.ApostleBloodIndex && _voidRuneComp.ItemIndex == allDatum.VoidRuneIndex)
			{
				punishmentRuneData = allDatum;
				break;
			}
		}
		if (punishmentRuneData != null && !__Extension_Method_ICollection__.IsNullOrEmpty(punishmentRuneData.GuideSkillCode))
		{
			UIHelper.EnableGameObject(BloodInfo, enable: false);
			UIHelper.EnableGameObject(VoidRuneInfo, enable: false);
			UIHelper.EnableGameObject(SkillInfo, enable: true);
			UIHelper.SetLabelText(Label_SkillInfo1, LocalizationManager.GetStringDirect(punishmentRuneData.GuideSkillCode[0]));
			UIHelper.SetLabelText(Label_SkillInfo2, LocalizationManager.GetStringDirect(punishmentRuneData.GuideSkillCode[1]));
			UIHelper.SetLabelText(Label_SkillInfo3, LocalizationManager.GetStringDirect(punishmentRuneData.GuideSkillCode[2]));
			SkillInfoScrollView.ResetPosition();
		}
	}

	private void UpdateLabelBloodCountLeft(bool isEnough)
	{
		int apostleBloodCount = OwnerWindow.PunishmentRuneItemCountData.ApostleBloodCount;
		Color color = (isEnough ? OwnerWindow.ColorGoldEnough : OwnerWindow.ColorGoldNotEnough);
		UIHelper.SetLabelColor(Label_BloodCount, color);
		UIHelper.SetLabelText(Label_BloodCount, $"{apostleBloodCount} / {apostleBloodCount}");
	}

	private bool CheckEnoughBloodCount(ItemData itemData)
	{
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(itemData.Code);
		return OwnerWindow.PunishmentRuneItemCountData.ApostleBloodCount <= itemCount;
	}

	private void CreateBloodComponentLeft(ItemData itemData)
	{
		_bloodComp = ItemIconComponent.PrefabAuto.Factory.Create(BloodRoot);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_bloodComp.transform);
		_bloodComp.Open(itemData, this);
	}

	private void CreateVoidComponentLeft(ItemData itemData)
	{
		_voidRuneComp = ItemIconComponent.PrefabAuto.Factory.Create(VoidRuneRoot);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_voidRuneComp.transform);
		_voidRuneComp.Open(itemData, this);
	}

	private void ReleaseBloodComponentLeft()
	{
		if (_bloodComp != null)
		{
			FactoryBase.ReleaseComponent(_bloodComp);
			_bloodComp = null;
		}
	}

	private void ReleaseVoidComponentLeft()
	{
		if (_voidRuneComp != null)
		{
			FactoryBase.ReleaseComponent(_voidRuneComp);
			_voidRuneComp = null;
		}
	}

	private void UpdateBloodSelectedComponentRight(ItemIconComponent itemIconComponent)
	{
		foreach (ItemIconComponent item in _listBloodComp)
		{
			item.SetSelected(selected: false);
		}
		itemIconComponent.SetSelected(selected: true);
	}

	private void UpdateVoidRuneSelectedComponentRight(ItemIconComponent itemIconComponent)
	{
		foreach (ItemIconComponent item in _listVoidRuneComp)
		{
			item.SetSelected(selected: false);
		}
		itemIconComponent.SetSelected(selected: true);
	}

	public void OnClickEmptyBloodSlot()
	{
		EnableBloodListRight();
	}

	private void EnableBloodListRight()
	{
		UIHelper.EnableGameObject(BloodGrid, enable: true);
		UIHelper.EnableGameObject(VoidRuneGrid, enable: false);
		string stringDirect = LocalizationManager.GetStringDirect("RUNE_CRAFT_STEP2_MAIN_DECS_1");
		UIHelper.SetLabelText(Label_GuideText, stringDirect);
		UIHelper.EnableGameObject(ActivateEffectSlotBlood, enable: true);
		UIHelper.EnableGameObject(ActivateEffectSlotVoidfRune, enable: false);
		UIHelper.EnableGameObject(BloodInfo, _bloodComp == null || _voidRuneComp == null);
		UIHelper.EnableGameObject(VoidRuneInfo, enable: false);
		UIHelper.EnableGameObject(SkillInfo, _bloodComp != null && _voidRuneComp != null);
		UIHelper.EnableGameObject(Label_NotHaveBlood, __Extension_Method_ICollection__.IsNullOrEmpty(_listBloodComp));
		UIHelper.EnableGameObject(Label_NotHaveVoid, enable: false);
	}

	public void OnClickEmptyVoidRuneSlot()
	{
		EnableVoidListRight();
	}

	private void EnableVoidListRight()
	{
		UIHelper.EnableGameObject(BloodGrid, enable: false);
		UIHelper.EnableGameObject(VoidRuneGrid, enable: true);
		string stringDirect = LocalizationManager.GetStringDirect("RUNE_CRAFT_STEP2_MAIN_DECS_2");
		UIHelper.SetLabelText(Label_GuideText, stringDirect);
		UIHelper.EnableGameObject(ActivateEffectSlotBlood, enable: false);
		UIHelper.EnableGameObject(ActivateEffectSlotVoidfRune, enable: true);
		UIHelper.EnableGameObject(VoidRuneInfo, _bloodComp == null || _voidRuneComp == null);
		UIHelper.EnableGameObject(BloodInfo, enable: false);
		UIHelper.EnableGameObject(SkillInfo, _bloodComp != null && _voidRuneComp != null);
		UIHelper.EnableGameObject(Label_NotHaveBlood, enable: false);
		UIHelper.EnableGameObject(Label_NotHaveVoid, __Extension_Method_ICollection__.IsNullOrEmpty(_listVoidRuneComp));
	}

	private void SetGaugeLeft(float value)
	{
		if (!(GaugeLeft == null))
		{
			if (value >= 0.9999f)
			{
				value = 1.1f;
			}
			GaugeLeft.material.SetFloat(_gaugePropertyName, value);
		}
	}

	private void SetGaugeRight(float value)
	{
		if (!(GaugeRight == null))
		{
			if (value >= 0.9999f)
			{
				value = 1.1f;
			}
			GaugeRight.material.SetFloat(_gaugePropertyName, value);
		}
	}

	private void UpdateButtonNextStep()
	{
		UIHelper.EnableButton(ButtonNextStep, _bloodComp != null && _voidRuneComp != null);
	}

	public void OnClickNextStep()
	{
		UIHelper.EnableGameObject(ButtonNextStep, enable: false);
		if (!(_bloodComp == null) && !(_voidRuneComp == null))
		{
			if (_voidRuneComp != null)
			{
				OwnerWindow.CachedVoidRuneIndex = _voidRuneComp.ItemIndex;
			}
			if (_bloodComp != null)
			{
				OwnerWindow.CachedApostleBloodIndex = _bloodComp.ItemIndex;
			}
			OwnerWindow.UpdateStep();
		}
	}
}
