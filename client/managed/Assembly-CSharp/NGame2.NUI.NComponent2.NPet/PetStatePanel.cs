using NGame2.NAccount;
using NGame2.NUI.NWindow.NPayShop;
using NGame2.NUI.NWindow.NPet;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class PetStatePanel : MonoBehaviour, IPetPanel, PetFullPointGaugeComponent.IListener
{
	public interface IListener
	{
		void OnClickPetDetailView(int petIndex);
	}

	public class PetStatePanelInfo
	{
		public IDataProvider IDataProvider { get; set; }

		public PetList.IListener IPetLisListener { get; set; }

		public IListener IListener { get; set; }
	}

	public UIWidget RootWidget;

	public UIButton ButtonMainPet;

	public UISpriteSelector MainPetSpriteSelector;

	public UIButton ButtonPetDetailView;

	public UISimpleGrid HappyPointGrid;

	private IDataProvider _dataProvider;

	private PetList.IListener _listener;

	private PetFullPointGaugeComponent _petFullPointGaugeComponent;

	private bool _isMainPet;

	private PetStatePanelInfo _info;

	private PetInfo SelectPetInfo
	{
		get
		{
			if (_info == null)
			{
				return null;
			}
			if (_info.IDataProvider == null)
			{
				return null;
			}
			return _info.IDataProvider.GetSelectedPetInfo();
		}
	}

	public void Open(IDataProvider dataProvider, PetList.IListener listener)
	{
		Reset();
		_dataProvider = dataProvider;
		_listener = listener;
		base.gameObject.SetActive(value: true);
		Init();
	}

	public void Open(PetStatePanelInfo info)
	{
		Reset();
		_info = info;
		Init();
	}

	private void Init()
	{
		base.gameObject.SetActive(value: true);
		SetMainPet();
		SetHappyPoint();
	}

	private void SetMainPet()
	{
		PetInfo selectPetInfo = SelectPetInfo;
		if (selectPetInfo != null)
		{
			PetMiscInfo petMiscInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.GetPetMiscInfo(PetMiscType.AvatarPetIndex);
			if (petMiscInfo != null)
			{
				int num = Vespa_String_Extension_Method.TOINT(petMiscInfo.MiscValue, 0);
				_isMainPet = num == selectPetInfo.PetIndex;
			}
			if ((bool)MainPetSpriteSelector)
			{
				MainPetSpriteSelector.Select(_isMainPet.ToString());
			}
		}
	}

	private void SetHappyPoint()
	{
		ClearHappyPointComp();
		PetInfo selectPetInfo = SelectPetInfo;
		if (selectPetInfo != null)
		{
			_petFullPointGaugeComponent = UIHelper.AddGridItem(HappyPointGrid, PetFullPointGaugeComponent.PrefabAuto);
			if (!(_petFullPointGaugeComponent == null))
			{
				_petFullPointGaugeComponent.Open(this, selectPetInfo);
			}
		}
	}

	public void Refresh(NGame2.NAccount.UserPetManager.ChangedPetInfo changedInfo)
	{
		if (changedInfo != null)
		{
			PetInfo selectPetInfo = SelectPetInfo;
			if (selectPetInfo != null && changedInfo.PetIndex == selectPetInfo.PetIndex)
			{
				SetMainPet();
			}
		}
	}

	public void Reset()
	{
		_listener = null;
		_dataProvider = null;
		_info = null;
		_isMainPet = false;
		ClearHappyPointComp();
	}

	private void ClearHappyPointComp()
	{
		FactoryBase.SafeReleaseComponent(ref _petFullPointGaugeComponent);
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		Reset();
	}

	public void OnClickMainPet()
	{
		string stringDirect = LocalizationManager.GetStringDirect("PET_MAINPET_POPUP_MAIN_TEXT");
		string stringDirect2 = LocalizationManager.GetStringDirect("PET_MAINPET_POPUP_DESC_TEXT");
		if (_isMainPet)
		{
			stringDirect2 = LocalizationManager.GetStringDirect("PET_MAINPET_RELEASE_NOTICE_DESC");
		}
		PayShopMessage3ButtonPopup.Open(stringDirect, stringDirect2, delegate
		{
			if (_info != null && _info.IPetLisListener != null)
			{
				PetInfo selectPetInfo = SelectPetInfo;
				if (selectPetInfo != null)
				{
					int petIndex = selectPetInfo.PetIndex;
					if (_isMainPet)
					{
						petIndex = 0;
						_isMainPet = false;
					}
					_info.IPetLisListener.OnClickMainPet(petIndex);
				}
			}
		});
	}

	public void OnClickDetailView()
	{
		if (_info != null && _info.IListener != null)
		{
			PetInfo selectPetInfo = SelectPetInfo;
			if (selectPetInfo != null)
			{
				_info.IListener.OnClickPetDetailView(selectPetInfo.PetIndex);
			}
		}
	}

	void PetFullPointGaugeComponent.IListener.OnClick()
	{
	}
}
