using NNguiExtension;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class EquipRuneSlotComponent : LinkFactoryComponent
{
	public enum ESize
	{
		Small,
		Large
	}

	public interface Listener
	{
		void OnClick(EquipRuneSlotComponent equipIconComponent);
	}

	public const string PrefabPath = "Prefab2/Component/Rune/EquipRuneSlotComponent";

	public static PrefabAuto<EquipRuneSlotComponent> PrefabAuto = new PrefabAuto<EquipRuneSlotComponent>("Prefab2/Component/Rune/EquipRuneSlotComponent");

	public GameObject OpenedObject;

	public GameObject LockedObject;

	public UITexture IconTexture;

	public UILabel LockedLabel;

	public UISpriteSelector BackgroundSelector;

	public UISpriteSelector GradeSelector;

	private Listener _listener;

	public int SlotIndex { get; private set; }

	public int RuneItemIndex { get; private set; }

	public ItemData<RuneItemData> RuneItemData { get; private set; }

	public ESize Size { get; private set; }

	public EquipPartType PartType { get; private set; }

	public bool Open(int slotIndex, int runeItemIndex, EquipPartType partType, ESize size, Listener listener)
	{
		_listener = listener;
		SlotIndex = slotIndex;
		RuneItemIndex = runeItemIndex;
		RuneItemData = GameRoot.Instance.ItemDataContainer.GetData<RuneItemData>(RuneItemIndex);
		PartType = partType;
		Size = size;
		InitIconTexture();
		InitBackground();
		InitGrade();
		return true;
	}

	private void InitIconTexture()
	{
		UIHelper.EnableGameObject(OpenedObject, enable: true);
		UIHelper.EnableGameObject(LockedObject, enable: false);
		UIHelper.EnableGameObject(LockedLabel, Size == ESize.Large);
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
