using System;
using NGame2.NUtil.NStringProxy;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class RuneStatComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClick(RuneStatComponent equipIconComponent);
	}

	public const string PrefabPath = "Prefab2/Component/Rune/RuneStatComponent";

	public static PrefabAuto<RuneStatComponent> PrefabAuto = new PrefabAuto<RuneStatComponent>("Prefab2/Component/Rune/RuneStatComponent");

	public Transform ExistRoot;

	public Transform EmptyRoot;

	public UITexture IconTexture;

	public UILabel NameLabel;

	public UILabel StatLabel;

	public UISpriteSelector BackgroundSelector;

	public UISpriteSelector GradeSelector;

	private Listener _listener;

	public int SlotIndex { get; private set; }

	public int RuneItemIndex { get; private set; }

	public ItemData RuneItemData { get; private set; }

	public EquipPartType PartType { get; private set; }

	public bool Open(int slotIndex, int runeItemIndex, EquipPartType partType, Listener listener)
	{
		_listener = listener;
		SlotIndex = slotIndex;
		RuneItemIndex = runeItemIndex;
		RuneItemData = GameRoot.Instance.ItemDataContainer.GetData(RuneItemIndex);
		PartType = partType;
		InitStat();
		return true;
	}

	private void InitStat()
	{
		bool flag = RuneItemIndex != 0;
		UIHelper.EnableGameObject(ExistRoot, flag);
		UIHelper.EnableGameObject(EmptyRoot, !flag);
		if (!flag)
		{
			return;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(RuneItemIndex);
		UIHelper.SetLabelText(NameLabel, ItemNameBuilder.GetItemName(data));
		string text = "";
		ItemData<RuneItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<RuneItemData>(RuneItemIndex);
		if (data2 != null)
		{
			text = data2.SubData.GetStatDisplayString((Enum statType) => LocalizationManager.GetString(statType));
		}
		UIHelper.SetLabelText(StatLabel, text);
	}

	private void InitIconTexture()
	{
		if (RuneItemData == null)
		{
			UIHelper.SetTexture(IconTexture, null);
		}
		else
		{
			UIHelper.ResetPlaneAssetTextureAsync(this, IconTexture, UIHelper.GetItemIcon2(RuneItemData.Code));
		}
	}

	private void InitBackground()
	{
		BackgroundSelector.SelectEnum(PartType);
	}

	private void InitGrade()
	{
		if (RuneItemData == null)
		{
			GradeSelector.Show(show: false);
			return;
		}
		int grade = RuneItemData.Grade;
		GradeSelector.Show(show: true);
		GradeSelector.SelectInt(grade);
	}

	public void OnClickIcon()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}
}
