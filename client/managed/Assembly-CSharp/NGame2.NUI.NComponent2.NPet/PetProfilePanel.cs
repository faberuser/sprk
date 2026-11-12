using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NWindow.NPet;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class PetProfilePanel : MonoBehaviour, IPetPanel
{
	[Serializable]
	public struct ProfileUI
	{
		public bool CanAwaken;

		public GameObject ButtonRoot;

		public GameObject CanAwakenMark;

		public UIButton ButtonDisplay;

		public UIButton ButtonAlreadyDisplay;
	}

	public interface IListener
	{
		void OpenAwakenPanel();
	}

	public UIWidget RootWidget;

	public UISpriteSelector StarSelector;

	public UILabel LabelGrade;

	public UILabel LabelName;

	public UIScrollGrid StatGrid;

	public UIScrollView StoryGrid;

	public UILabel LabelStory;

	public ProfileUI[] ProfileUIs;

	private IDataProvider _dataProvider;

	private PetList.IListener _petlistListener;

	private IListener _listener;

	private List<StatListComponent> _listStatListComponent = new List<StatListComponent>();

	private StatListComponent _statComponent;

	public void Open(IDataProvider dataProvider, PetList.IListener petlistListener, IListener listener)
	{
		Reset();
		_dataProvider = dataProvider;
		_petlistListener = petlistListener;
		_listener = listener;
		base.gameObject.SetActive(value: true);
		Init();
	}

	private void Init()
	{
		SetPetPrfilUi();
		SetStatList();
	}

	private void SetPetPrfilUi()
	{
		PetInfo selectedPetInfo = _dataProvider.GetSelectedPetInfo();
		if (selectedPetInfo == null)
		{
			return;
		}
		CreatureData creatureData = selectedPetInfo.CreatureData;
		if (creatureData != null)
		{
			bool flag = selectedPetInfo.Star != 0;
			UIHelper.EnableGameObject(StarSelector, flag);
			if (flag)
			{
				StarSelector.SelectInt(selectedPetInfo.Star);
			}
			UIHelper.SetLabelText(LabelName, creatureData.Name);
			string creatureGradeColorLocalizationKey = PetHelper.GetCreatureGradeColorLocalizationKey(creatureData.CreatureGrade);
			UIHelper.SetLabelTextLocalizeDirect(LabelGrade, creatureGradeColorLocalizationKey);
			string stringDirect = LocalizationManager.GetStringDirect(creatureData.ArchiveStory);
			UIHelper.SetLabelText(LabelStory, stringDirect);
			if (StoryGrid != null)
			{
				UIPanel component = StoryGrid.gameObject.GetComponent<UIPanel>();
				float num = ((component != null) ? component.height : 240f);
				StoryGrid.contentPivot = ((!((float)LabelStory.height > num)) ? UIWidget.Pivot.Center : UIWidget.Pivot.TopLeft);
				StoryGrid.ResetPosition();
			}
			SetButton();
		}
	}

	private string GetCreatureGradeColorLocalizationKey(CreatureGrade grade)
	{
		return grade switch
		{
			CreatureGrade.Rare => "PET_GRADE_COLOR_RARE_TEXT", 
			CreatureGrade.Hero => "PET_GRADE_COLOR_HERO_TEXT", 
			CreatureGrade.Ancient => "PET_GRADE_COLOR_ANCIENT_TEXT", 
			CreatureGrade.Legend => "PET_GRADE_COLOR_LEGEND_TEXT", 
			_ => "", 
		};
	}

	private void SetButton()
	{
		PetInfo selectedPetInfo = _dataProvider.GetSelectedPetInfo();
		bool flag = selectedPetInfo.CanAwaken();
		bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.PetManager.IsPlacedPet(selectedPetInfo.PetIndex);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ProfileUIs); i++)
		{
			ProfileUI profileUI = ProfileUIs[i];
			bool flag3 = profileUI.CanAwaken == flag;
			UIHelper.EnableGameObject(profileUI.ButtonRoot, flag3);
			if (flag3)
			{
				if (profileUI.CanAwaken)
				{
					UIHelper.EnableGameObject(profileUI.CanAwakenMark, PetHelper.CheckPossiblePetAwaken(selectedPetInfo));
				}
				UIHelper.EnableGameObject(profileUI.ButtonDisplay, !flag2);
				UIHelper.EnableGameObject(profileUI.ButtonAlreadyDisplay, flag2);
			}
		}
	}

	public void SetStatList()
	{
		ClearStatListComp();
		if (StatGrid == null || StatGrid.ClippingPanel == null)
		{
			return;
		}
		PetInfo petInfo = _dataProvider.GetSelectedPetInfo();
		if (petInfo == null)
		{
			return;
		}
		CreatureData creatureData = petInfo.CreatureData;
		if (creatureData != null)
		{
			ArrayFor(creatureData.PetOptionIndices, delegate(int optionIndex)
			{
				SetStatComponent(optionIndex, petInfo.Star);
			});
			ArrayFor(creatureData.PetBonusIndices, delegate(int bonusOptionIndex)
			{
				SetBonusStatComponent(bonusOptionIndex, petInfo.Star);
			});
			if (StatGrid.ScrollView != null)
			{
				StatGrid.ScrollView.ResetPosition();
			}
		}
	}

	private void ArrayFor<T>(T[] array, Action<T> action)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
		{
			T i_argA = array[i];
			Vespa_System_Action_Extension_Method.fire(action, i_argA);
		}
	}

	private void SetStatComponent(int optionIndex, int star)
	{
		if (_statComponent == null)
		{
			_statComponent = StatListComponent.PetStatCreate(StatGrid.ClippingPanel, 5);
		}
		_statComponent.AddPetStat(optionIndex, star);
	}

	private void SetBonusStatComponent(int optionIndex, int star)
	{
		if (_statComponent == null)
		{
			_statComponent = StatListComponent.PetStatCreate(StatGrid.ClippingPanel, 5);
		}
		_statComponent.AddPetBonusStat(optionIndex, star);
	}

	public void Refresh(NGame2.NAccount.UserPetManager.ChangedPetInfo changedInfo)
	{
		if (changedInfo != null && _dataProvider != null)
		{
			if (changedInfo.EChangedPetInfoType == NGame2.NAccount.UserPetManager.EChangedPetInfoType.ChangePetLayout)
			{
				SetButton();
			}
			if (changedInfo.EChangedPetInfoType == NGame2.NAccount.UserPetManager.EChangedPetInfoType.PetInfoChanged)
			{
				Init();
			}
		}
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		Reset();
	}

	public void Reset()
	{
		_petlistListener = null;
		_dataProvider = null;
		_listener = null;
		FactoryBase.SafeReleaseComponent(ref _statComponent);
	}

	private void ClearStatListComp()
	{
		FactoryBase.SafeReleaseComponent(ref _statComponent);
	}

	public void OnClickOpenAwakenPanel()
	{
		if (_petlistListener != null)
		{
			_listener.OpenAwakenPanel();
		}
	}

	public void OnClickArchive()
	{
		if (_petlistListener != null)
		{
			PetInfo selectedPetInfo = _dataProvider.GetSelectedPetInfo();
			if (selectedPetInfo != null)
			{
				_petlistListener.OnClickArchive(selectedPetInfo.PetIndex);
			}
		}
	}

	public void OnClickDisplay()
	{
		if (_petlistListener != null)
		{
			_petlistListener.OnClickSelectPetPlacePopup();
		}
	}
}
