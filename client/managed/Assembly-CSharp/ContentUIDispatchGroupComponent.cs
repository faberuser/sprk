using NGame2.NUI;
using NGame2.NUtil.NWrappedInfo;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;

public class ContentUIDispatchGroupComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClick(ContentUIDispatchGroupComponent comp);
	}

	public const string PATH_PREFAB = "Resources/Prefab2/Window/DispatchBattle/ContentUIDispatchGroupComponent";

	public static bool IsExistReadyToCompleteInGroupList;

	public UILabel LabelName;

	public UISprite SpriteArrow;

	public UISprite SpriteReadyToComplete;

	private int _deckNumber;

	private WCL_DispatchBattleInfo _info;

	private bool _isSelected;

	private bool _isOpened;

	private Listener _listener;

	public bool IsSelected => _isSelected;

	public int DeckNumber => _deckNumber;

	public void OnClick()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}

	public void Open(Listener listener)
	{
		_listener = listener;
		UpdateUI();
	}

	public void Open(int deckNumber, WCL_DispatchBattleInfo info, Listener listener)
	{
		_deckNumber = deckNumber;
		_info = info;
		_listener = listener;
		UpdateUI();
	}

	public void UpdateReadyToComplete()
	{
		if (_info != null && SpriteReadyToComplete != null)
		{
			SpriteReadyToComplete.gameObject.SetActive(IsReadyToComplete());
		}
	}

	public void SetSelected(bool isSelected)
	{
		_isSelected = isSelected;
		UIHelper.EnableGameObject(SpriteArrow, isSelected);
		UIHelper.EnableGameObject(SpriteReadyToComplete, IsShowReadyToComplete());
	}

	public void SetOpenedState(bool isOpen)
	{
		_isOpened = isOpen;
		UIHelper.EnableGameObject(SpriteReadyToComplete, IsShowReadyToComplete());
	}

	public void SetValues(int deckNumber, WCL_DispatchBattleInfo info, bool isSelected)
	{
		_deckNumber = deckNumber;
		_info = info;
		_isSelected = isSelected;
	}

	public void CopyTo(ref ContentUIDispatchGroupComponent comp)
	{
		comp.SetValues(_deckNumber, _info, IsSelected);
	}

	public bool IsReadyToComplete()
	{
		if (_info == null)
		{
			return false;
		}
		return _info.State == DispatchState.ReadyToComplete;
	}

	public bool IsShowReadyToComplete()
	{
		if (_isSelected && _isOpened)
		{
			return false;
		}
		if (_isSelected && !_isOpened && !IsExistReadyToCompleteInGroupList)
		{
			return false;
		}
		if (!_isSelected && !IsReadyToComplete())
		{
			return false;
		}
		return true;
	}

	private void UpdateUI()
	{
		if (LabelName != null)
		{
			UIHelper.SetLabelText(LabelName, $"{LocalizationManager.GetString(DispatchBattle.DISPATCH_DETACHMENT)} {_deckNumber.ToString()}");
		}
		if (SpriteArrow != null)
		{
			SpriteArrow.gameObject.SetActive(IsSelected);
		}
		UIHelper.EnableGameObject(SpriteReadyToComplete, IsShowReadyToComplete());
	}
}
