using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NWindow;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class PetIncubatorSlotIconComponent : UIScrollItemBehaviour, InitOnCreate, IInspectorGuiDrawer
{
	public new interface IListener
	{
		void OnClick(PetIncubatorSlotIconComponent petIncubatorSlotIconComponent);
	}

	public enum ESlotOpenState
	{
		Open,
		CanOpen,
		Disable
	}

	public static string PrefabPath = "Prefab2/Component/Pet/PetIncubatorSlotIconComponent";

	public static PrefabAuto<PetIncubatorSlotIconComponent> PrefabAuto = new PrefabAuto<PetIncubatorSlotIconComponent>(PrefabPath);

	public UIWidget WidgetThis;

	public UIBundleLoader UIBundleLoader_IncubatorIcon;

	public UISpriteSelector SpriteSelector_Frame;

	public UISprite Sprite_CanOpen;

	public UISprite Sprite_Selected;

	public UIWidget Widget_GaugeRoot;

	public UIClipSprite GaugeFillAmount;

	private IListener _listener;

	private bool _selected;

	public int SlotIndex { get; private set; }

	public PetIncubatorSlotInfo SlotInfo { get; set; }

	public bool IsSelected => _selected;

	public ESlotOpenState SlotOpenState { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		Close();
	}

	public void Open(IListener listener, int slotIndex, PetIncubatorSlotInfo slotInfo, ESlotOpenState slotOpenState)
	{
		_listener = listener;
		SlotIndex = slotIndex;
		SlotInfo = slotInfo;
		SlotOpenState = slotOpenState;
		SetupInfo();
	}

	public void ResetPortrait(PetIncubatorSlotInfo slotInfo, ESlotOpenState slotOpenState)
	{
		SlotInfo = slotInfo;
		SlotOpenState = slotOpenState;
		UIHelper.EnableGameObject(UIBundleLoader_IncubatorIcon, enable: false);
		SetupInfo();
	}

	private void SetupInfo()
	{
		SetSlotImage();
		SetSlotFrame();
		SetState();
		SetGauge();
	}

	public void Close()
	{
		_selected = false;
		UIHelper.SetWidgetGrayScale(WidgetThis, grayScale: false);
		UIHelper.SetWidgetGrayScale(Sprite_CanOpen, grayScale: false);
		UIHelper.EnableGameObject(Sprite_Selected, enable: false);
	}

	public void SetSelection(bool select)
	{
		_selected = select;
		UIHelper.EnableGameObject(Sprite_Selected, _selected);
	}

	public void SetSlotImage()
	{
		if (SlotInfo == null && SlotOpenState != ESlotOpenState.Open)
		{
			UIHelper.EnableGameObject(UIBundleLoader_IncubatorIcon, enable: false);
			return;
		}
		UIHelper.EAssetTextureAssign assetTexAssign = UIHelper.EAssetTextureAssign.None;
		string petIncubatorIconPath = UIHelper.GetPetIncubatorIconPath(SlotInfo.MountingIndex);
		UIHelper.SetBundleTexture(UIBundleLoader_IncubatorIcon, petIncubatorIconPath, assetTexAssign);
		UIHelper.EnableGameObject(UIBundleLoader_IncubatorIcon, enable: true);
	}

	public void SetSlotFrame()
	{
		if (SlotInfo == null)
		{
			UIHelper.EnableGameObject(SpriteSelector_Frame, enable: false);
			return;
		}
		PetIncubatorData data = GameRoot.Instance.PetIncubatorDataContainer.GetData(SlotInfo.MountingIndex);
		if (data == null)
		{
			UIHelper.EnableGameObject(SpriteSelector_Frame, enable: false);
			return;
		}
		SpriteSelector_Frame.Select(data.Grade.ToString());
		UIHelper.EnableGameObject(SpriteSelector_Frame, enable: false);
	}

	public void SetState()
	{
		switch (SlotOpenState)
		{
		case ESlotOpenState.Disable:
			UIHelper.SetWidgetGrayScale(WidgetThis, grayScale: true);
			UIHelper.SetWidgetGrayScale(Sprite_CanOpen, grayScale: true);
			UIHelper.EnableGameObject(Sprite_CanOpen, enable: true);
			break;
		case ESlotOpenState.CanOpen:
			UIHelper.SetWidgetGrayScale(WidgetThis, grayScale: true);
			UIHelper.SetWidgetGrayScale(Sprite_CanOpen, grayScale: false);
			UIHelper.EnableGameObject(Sprite_CanOpen, enable: true);
			break;
		case ESlotOpenState.Open:
			UIHelper.SetWidgetGrayScale(WidgetThis, grayScale: false);
			UIHelper.SetWidgetGrayScale(Sprite_CanOpen, grayScale: false);
			UIHelper.EnableGameObject(Sprite_CanOpen, enable: false);
			break;
		}
	}

	public void SetGauge()
	{
		if (SlotOpenState != ESlotOpenState.Open || SlotInfo == null || SlotInfo.SetItemIndex == 0)
		{
			UIHelper.EnableGameObject(Widget_GaugeRoot, enable: false);
			return;
		}
		float incubatorRemainTimePercent = PetHelper.GetIncubatorRemainTimePercent(SlotInfo);
		if (incubatorRemainTimePercent == -1f)
		{
			UIHelper.EnableGameObject(Widget_GaugeRoot, enable: false);
			return;
		}
		UIHelper.EnableGameObject(Widget_GaugeRoot, enable: true);
		UIHelper.SetSpriteFillAmount(GaugeFillAmount, incubatorRemainTimePercent);
	}

	public void OnClickPortrait()
	{
		if (!_selected)
		{
			if (SlotOpenState == ESlotOpenState.Disable)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("PETINCUBATOR_SLOT_NOT_AVAILABLE"));
			}
			else if (_listener != null)
			{
				_listener.OnClick(this);
			}
		}
	}
}
