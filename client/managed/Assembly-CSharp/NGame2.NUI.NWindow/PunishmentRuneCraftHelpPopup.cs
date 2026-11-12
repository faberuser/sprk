using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NComponent;
using NGame2.NUI.NComponent2;
using NGame2.NUtil.NStringProxy;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PunishmentRuneCraftHelpPopup : BaseWindow, PunishmentRuneRecipeComponent.IListener
{
	public const string PrefabPath = "Resources/Prefab2/Window/PunishmentRuneCraft/PunishmentRuneCraftHelpPopup";

	public UIToggle ToggleCommon;

	public UIToggle ToggleBlood_1;

	public UIToggle ToggleBlood_2;

	public GameObject PageCommon;

	public GameObject PageBlood_1;

	public GameObject PageBlood_2;

	public GameObject RuneInfoUI;

	public UIScrollGrid GridRecipeBlood_1;

	public UIScrollGrid GridRecipeBlood_2;

	public UIScrollView ScrollViewCommon;

	public UIScrollView ScrollViewRuneInfo;

	public UITable TableCommon;

	public UITable TableRecipeInfo;

	public int ItemIndexBlood1;

	public int ItemIndexBlood2;

	public Transform RootPunishmentRuneComponent;

	public UILabel LabelNamePunishmentRune;

	public UILabel LabelStatType;

	public UILabel LabelStatValue;

	public UILabel LabelSkillInfo1;

	public UILabel LabelSkillInfo2;

	public UILabel LabelSkillInfo3;

	private List<PunishmentRuneRecipeComponent> _listBloodRecipeComponent_1 = new List<PunishmentRuneRecipeComponent>();

	private List<PunishmentRuneRecipeComponent> _listBloodRecipeComponent_2 = new List<PunishmentRuneRecipeComponent>();

	private ItemIconComponent _punishmentRuneComp;

	private PunishmentRuneRecipeComponent _selectedComp;

	public new void Open()
	{
		base.Open();
		InitCommonPage();
		InitBloodPage1();
		InitBloodPage2();
		ToggleCommon.Set(state: true);
	}

	private void InitCommonPage()
	{
		ScrollViewCommon.ResetPosition();
		VespaCoroutine.Start(coUpdateCommonTable());
	}

	private void InitBloodPage1()
	{
		foreach (PunishmentRuneData allDatum in GameRoot.Instance.PunishmentRuneDataContainer.GetAllData())
		{
			if (allDatum.ApostleBloodIndex == ItemIndexBlood1)
			{
				PunishmentRuneRecipeComponent punishmentRuneRecipeComponent = UIHelper.AddGridItem(GridRecipeBlood_1, PunishmentRuneRecipeComponent.PrefabAuto);
				punishmentRuneRecipeComponent.Open(allDatum, this);
				_listBloodRecipeComponent_1.Add(punishmentRuneRecipeComponent);
			}
		}
		UIHelper.RepositionGrid(GridRecipeBlood_1);
		TableRecipeInfo.Reposition();
	}

	private void InitBloodPage2()
	{
		foreach (PunishmentRuneData allDatum in GameRoot.Instance.PunishmentRuneDataContainer.GetAllData())
		{
			if (allDatum.ApostleBloodIndex == ItemIndexBlood2)
			{
				PunishmentRuneRecipeComponent punishmentRuneRecipeComponent = UIHelper.AddGridItem(GridRecipeBlood_2, PunishmentRuneRecipeComponent.PrefabAuto);
				punishmentRuneRecipeComponent.Open(allDatum, this);
				_listBloodRecipeComponent_2.Add(punishmentRuneRecipeComponent);
			}
		}
		UIHelper.RepositionGrid(GridRecipeBlood_2);
		TableRecipeInfo.Reposition();
	}

	private void UpdatePunishmentRuneInfo(PunishmentRuneData data)
	{
		ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(data.PunishmentRuneIndex);
		FactoryBase.ReleaseComponent(_punishmentRuneComp);
		_punishmentRuneComp = ItemIconComponent.PrefabAuto.Factory.Create(RootPunishmentRuneComponent);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_punishmentRuneComp.transform);
		_punishmentRuneComp.Open(data2);
		_punishmentRuneComp.SetSelected(selected: false);
		UIHelper.SetLabelText(LabelNamePunishmentRune, ItemNameBuilder.GetItemName(data2));
		PunishmentRuneOptionData data3 = GameRoot.Instance.PunishmentRuneOptionDataContainer.GetData(data.MainOptionIndex);
		string text = LocalizationManager.GetString(data3.StatTypes[0]);
		UIHelper.SetLabelText(LabelStatType, text);
		string statDisplayValue = StatData.GetStatDisplayValue(data3.StatTypes[0], data3.OptionValueMin0);
		string statDisplayValue2 = StatData.GetStatDisplayValue(data3.StatTypes[0], data3.OptionValueMax0);
		UIHelper.SetLabelText(LabelStatValue, string.Format("{0}~{1}", statDisplayValue.Replace("%", "").Replace("+", ""), statDisplayValue2.Replace("+", "")));
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(data.GuideSkillCode))
		{
			UIHelper.SetLabelText(LabelSkillInfo1, LocalizationManager.GetStringDirect(data.GuideSkillCode[0]));
			UIHelper.SetLabelText(LabelSkillInfo2, LocalizationManager.GetStringDirect(data.GuideSkillCode[1]));
			UIHelper.SetLabelText(LabelSkillInfo3, LocalizationManager.GetStringDirect(data.GuideSkillCode[2]));
			VespaCoroutine.Start(coUpdateTable());
		}
	}

	private IEnumerator coUpdateTable()
	{
		yield return new WaitForSeconds(0.1f);
		TableRecipeInfo.Reposition();
	}

	private IEnumerator coUpdateCommonTable()
	{
		yield return new WaitForSeconds(0.1f);
		TableCommon.Reposition();
	}

	public void OnToggle()
	{
		if (ToggleBlood_1.value)
		{
			UpdatePageBlood1();
		}
		if (ToggleBlood_2.value)
		{
			UpdatePageBlood2();
		}
		if (ToggleCommon.value)
		{
			ScrollViewCommon.ResetPosition();
		}
		UIHelper.EnableGameObject(PageCommon, ToggleCommon.value);
		UIHelper.EnableGameObject(PageBlood_1, ToggleBlood_1.value);
		UIHelper.EnableGameObject(PageBlood_2, ToggleBlood_2.value);
		UIHelper.EnableGameObject(RuneInfoUI, !ToggleCommon.value);
	}

	private void UpdatePageBlood1()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listBloodRecipeComponent_1); i++)
		{
			if (i == 0)
			{
				_selectedComp = _listBloodRecipeComponent_1[i];
				_selectedComp.SetSelected(select: true);
			}
			else
			{
				_listBloodRecipeComponent_1[i].SetSelected(select: false);
			}
		}
		UIHelper.ResetGridPosition(GridRecipeBlood_1);
		ScrollViewRuneInfo.ResetPosition();
		UpdatePunishmentRuneInfo(_selectedComp.PunishmentRuneData);
	}

	private void UpdatePageBlood2()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listBloodRecipeComponent_2); i++)
		{
			if (i == 0)
			{
				_selectedComp = _listBloodRecipeComponent_2[i];
				_selectedComp.SetSelected(select: true);
			}
			else
			{
				_listBloodRecipeComponent_2[i].SetSelected(select: false);
			}
		}
		UIHelper.ResetGridPosition(GridRecipeBlood_2);
		ScrollViewRuneInfo.ResetPosition();
		UpdatePunishmentRuneInfo(_selectedComp.PunishmentRuneData);
	}

	public override void Close()
	{
		base.Close();
		FactoryBase.ReleaseAndClearList(_listBloodRecipeComponent_1);
		FactoryBase.ReleaseAndClearList(_listBloodRecipeComponent_2);
		_selectedComp = null;
	}

	void PunishmentRuneRecipeComponent.IListener.OnClick(PunishmentRuneRecipeComponent component)
	{
		UpdatePunishmentRuneInfo(component.PunishmentRuneData);
		_selectedComp.SetSelected(select: false);
		_selectedComp = component;
		_selectedComp.SetSelected(select: true);
	}
}
