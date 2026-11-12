using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class PetAwakenPanel : MonoBehaviour, IPetPanel
{
	public interface IListener
	{
		void OnClickAwaken();

		void OnClose();
	}

	public UIWidget AwakeRoot;

	public UILabel BeforeStar;

	public UILabel AfterStar;

	public UILabel LabelMaterialCount;

	public UIScrollGrid StatGrid;

	public UISimpleGrid MaterialPetGrid;

	public UISimpleGrid MaterialSoulGrid;

	public UIButton ButtonAwaken;

	public UIWidget MaxAwakenRoot;

	public UISimpleGrid MaxPetGrid;

	public UIScrollGrid MaxStatGrid;

	public Animator Animator;

	private const string DefaultAnimKey = "PetAwakeBasics";

	private const string SuccessAnimKey = "PetAwakeStart";

	private IDataProvider _dataProvider;

	private IListener _listener;

	private PetAwakenData _petAwakenData;

	private StatListComponent _statListComponent;

	private StatListComponent _maxStatListComponent;

	private PetPortraitComponent _petPortraitComponent;

	private ItemIconComponent _materialPetSoulComponent;

	private PetPortraitComponent _maxPetPortraitComponent;

	public bool IsPlayAwakenAnim { get; private set; }

	public void Open(IDataProvider dataProvider, IListener listener)
	{
		Reset();
		_dataProvider = dataProvider;
		_listener = listener;
		base.gameObject.SetActive(value: true);
		Init();
	}

	private void Init()
	{
		SetAwakenPanel(_dataProvider.GetSelectedPetInfo());
	}

	private void SetAwakenPanel(PetInfo petInfo)
	{
		if (petInfo != null)
		{
			bool flag = petInfo.CanAwaken();
			UIHelper.EnableGameObject(AwakeRoot, flag);
			UIHelper.EnableGameObject(MaxAwakenRoot, !flag);
			if (flag)
			{
				_petAwakenData = GameRoot.Instance.PetAwakenDataContainer.GetData(petInfo.PetIndex, petInfo.Star + 1);
				SetAwakenState();
			}
			else
			{
				SetMaxAwaken();
			}
		}
	}

	private void SetAwakenState()
	{
		if (_petAwakenData != null)
		{
			SetStatUpgrade();
			SetPetInfo();
			SetMaterial();
		}
	}

	private void SetMaxAwaken()
	{
		SetMaxPetInfo();
		SetMaxStat();
	}

	private void SetPetInfo()
	{
		PetInfo selectedPetInfo = _dataProvider.GetSelectedPetInfo();
		string text = selectedPetInfo.Star.ToString();
		UIHelper.SetLabelText(BeforeStar, text);
		string text2 = ((selectedPetInfo.Star == 5) ? "@MAX" : (selectedPetInfo.Star + 1).ToString());
		UIHelper.SetLabelText(AfterStar, text2);
		ClearPetPortraitComp();
		_petPortraitComponent = UIHelper.AddGridItem(MaterialPetGrid, PetPortraitComponent.SmallPrefabAuto);
		_petPortraitComponent.Open(selectedPetInfo, null);
	}

	private void SetMaterial()
	{
		ClearPetSoulComp();
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(_petAwakenData.ItemCode1);
		if (dataByCode == null)
		{
			UIHelper.EnableGameObject(LabelMaterialCount, enable: false);
			UIHelper.EnableButton(ButtonAwaken, enable: false, grayscaleIfDisable: true);
			return;
		}
		UIHelper.EnableGameObject(LabelMaterialCount, enable: true);
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(dataByCode.Index);
		int itemCount2 = _petAwakenData.ItemCount1;
		Color color = ((itemCount2 <= itemCount) ? Color.white : Color.red);
		_materialPetSoulComponent = UIHelper.AddGridItem(MaterialSoulGrid, ItemIconComponent.PrefabAuto);
		_materialPetSoulComponent.Open(dataByCode);
		UIHelper.SetLabelColor(LabelMaterialCount, color);
		UIHelper.SetLabelText(LabelMaterialCount, $"{itemCount}/{itemCount2}");
	}

	private void SetMaxStat()
	{
		FactoryBase.SafeReleaseComponent(ref _maxStatListComponent);
		if (MaxStatGrid == null || MaxStatGrid.ClippingPanel == null)
		{
			return;
		}
		PetInfo selectedPetInfo = _dataProvider.GetSelectedPetInfo();
		if (selectedPetInfo == null || selectedPetInfo.Star != 5)
		{
			return;
		}
		CreatureData creatureData = selectedPetInfo.CreatureData;
		if (creatureData == null)
		{
			return;
		}
		sbyte star = selectedPetInfo.Star;
		int[] petOptionIndices = creatureData.PetOptionIndices;
		int[] petBonusIndices = creatureData.PetBonusIndices;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petOptionIndices); i++)
		{
			if (_maxStatListComponent == null)
			{
				_maxStatListComponent = StatListComponent.PetStatCreate(MaxStatGrid.ClippingPanel, 5);
			}
			_maxStatListComponent.AddPetStat(petOptionIndices[i], star);
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(petBonusIndices); j++)
		{
			if (_maxStatListComponent == null)
			{
				_maxStatListComponent = StatListComponent.PetStatCreate(MaxStatGrid.ClippingPanel, 5);
			}
			_maxStatListComponent.AddPetStat(petBonusIndices[j], star);
		}
		if (MaxStatGrid.ScrollView != null)
		{
			MaxStatGrid.ScrollView.ResetPosition();
		}
	}

	private void SetStatUpgrade()
	{
		FactoryBase.SafeReleaseComponent(ref _statListComponent);
		if (StatGrid == null || StatGrid.ClippingPanel == null)
		{
			return;
		}
		PetInfo selectedPetInfo = _dataProvider.GetSelectedPetInfo();
		if (selectedPetInfo == null || selectedPetInfo.Star == 5)
		{
			return;
		}
		CreatureData creatureData = selectedPetInfo.CreatureData;
		if (creatureData == null)
		{
			return;
		}
		sbyte star = selectedPetInfo.Star;
		int[] petOptionIndices = creatureData.PetOptionIndices;
		int[] petBonusIndices = creatureData.PetBonusIndices;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petOptionIndices); i++)
		{
			if (_statListComponent == null)
			{
				_statListComponent = StatListComponent.PetStatCreate(StatGrid.ClippingPanel, 5);
			}
			_statListComponent.AddPetStatChange(petOptionIndices[i], star, star + 1);
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(petBonusIndices); j++)
		{
			if (_statListComponent == null)
			{
				_statListComponent = StatListComponent.PetStatCreate(StatGrid.ClippingPanel, 5);
			}
			_statListComponent.AddPetBonusStatChange(petBonusIndices[j], star, star + 1);
		}
		if (StatGrid.ScrollView != null)
		{
			StatGrid.ScrollView.ResetPosition();
		}
	}

	private void SetOptionStatChange<T>(int[] indices, Func<int, T> getData, Action<T> action)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(indices); i++)
		{
			int arg = indices[i];
			T i_argA = getData(arg);
			Vespa_System_Action_Extension_Method.fire(action, i_argA);
		}
	}

	private void SetMaxPetInfo()
	{
		ClearMaxPetPortraitComp();
		PetInfo selectedPetInfo = _dataProvider.GetSelectedPetInfo();
		_maxPetPortraitComponent = UIHelper.AddGridItem(MaxPetGrid, PetPortraitComponent.SmallPrefabAuto);
		_maxPetPortraitComponent.Open(selectedPetInfo, null);
	}

	public void Refresh(ItemInfo changedInfo)
	{
		if (changedInfo != null && _petAwakenData != null)
		{
			int itemIndex = changedInfo.ItemIndex;
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
			if (data != null && !(data.Code != _petAwakenData.ItemCode1))
			{
				SetMaterial();
			}
		}
	}

	public void Refresh(NGame2.NAccount.UserPetManager.ChangedPetInfo changedInfo)
	{
		if (changedInfo != null && _dataProvider != null && changedInfo.EChangedPetInfoType == NGame2.NAccount.UserPetManager.EChangedPetInfoType.PetInfoChanged)
		{
			PetInfo selectedPetInfo = _dataProvider.GetSelectedPetInfo();
			if (changedInfo.PetIndex == selectedPetInfo.PetIndex)
			{
				selectedPetInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(changedInfo.PetIndex);
				SetAwakenPanel(selectedPetInfo);
			}
		}
	}

	public void PlayDefaultAnim()
	{
		if (Animator != null)
		{
			IsPlayAwakenAnim = false;
			Animator.Play("PetAwakeBasics");
		}
	}

	public void PlaySuccessAnim(Action onEnd = null)
	{
		VespaCoroutine.Start(coPlaySuccessAnim(onEnd), null, null);
	}

	private IEnumerator coPlaySuccessAnim(Action onEnd)
	{
		if (Animator == null)
		{
			yield break;
		}
		IsPlayAwakenAnim = true;
		Animator.StopPlayback();
		Animator.Play("PetAwakeStart");
		PlaySound("Pet.System.Awaken");
		yield return new WaitForSeconds(0.1f);
		AnimatorClipInfo[] currentAnimatorClipInfo = Animator.GetCurrentAnimatorClipInfo(0);
		if (__Extension_Method_ICollection__.COUNT(currentAnimatorClipInfo) != 0)
		{
			float seconds = 0f;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(currentAnimatorClipInfo))
			{
				seconds = currentAnimatorClipInfo[0].clip.length;
			}
			yield return new WaitForSeconds(seconds);
			Vespa_System_Action_Extension_Method.fire(onEnd);
		}
	}

	private void PlaySound(string soundCode)
	{
		SoundStock stock = new SoundStock
		{
			Code = soundCode
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		Reset();
	}

	public void Reset()
	{
		_listener = null;
		_dataProvider = null;
		IsPlayAwakenAnim = false;
		VespaCoroutine.StopAll(this);
		ClearMaxPetPortraitComp();
		ClearPetPortraitComp();
		ClearPetSoulComp();
	}

	private void ClearPetPortraitComp()
	{
		FactoryBase.SafeReleaseComponent(ref _petPortraitComponent);
	}

	private void ClearMaxPetPortraitComp()
	{
		FactoryBase.SafeReleaseComponent(ref _maxPetPortraitComponent);
	}

	private void ClearPetSoulComp()
	{
		FactoryBase.SafeReleaseComponent(ref _materialPetSoulComponent);
	}

	public void OnClickClose()
	{
		if (_listener != null)
		{
			_listener.OnClose();
		}
		Close();
	}

	public void OnClickAwaken()
	{
		if (IsPlayAwakenAnim || _petAwakenData == null)
		{
			return;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(_petAwakenData.ItemCode1);
		if (dataByCode == null)
		{
			return;
		}
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(dataByCode.Index);
		if (_petAwakenData.ItemCount1 > itemCount)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("PET_AWAKE_NO_MATERIAL"));
			return;
		}
		PlaySuccessAnim(delegate
		{
			if (_listener != null)
			{
				_listener.OnClickAwaken();
			}
		});
	}
}
