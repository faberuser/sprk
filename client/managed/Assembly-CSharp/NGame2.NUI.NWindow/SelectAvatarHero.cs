using System;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NRender3D;
using NGame2.NUtil.NCostume;
using NGameAsset.NCreature;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SelectAvatarHero : BaseWindow, AvatarHeroComponent.Listener, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver
{
	public interface Listener
	{
		void ChangeAvatarHero(UIAvatarHelper.AvatarInfo changeAvatarInfo);

		void OpenHeroCostumeView(int heroIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GameOption/SelectAvatarHero";

	[SerializeField]
	private SelectFilter _selectFilter;

	[SerializeField]
	private UILabel _lbNoneHero;

	public UIScrollGrid AvatarGrid;

	public UIButton BtnCustomizing;

	public UIButton BtnSelectAvatar;

	public Transform SelectedHeroRoot;

	public UITexture HeroPreviewTexture;

	private HeroPreview _heroPreview;

	private List<AvatarHeroComponent> _listAvatarHero = new List<AvatarHeroComponent>();

	private HeroPortraitComponent _selectedHeroPortrait;

	private Listener _listener;

	private NGame2.NAccount.UserHeroManager.HeroChangeType[] _costumeTypes;

	private UIAvatarHelper.AvatarInfo _currentAvatarInfo;

	private bool _isChanged;

	public void Open(Listener listener)
	{
		base.Open();
		Init(listener);
	}

	private void Init(Listener listener)
	{
		_listener = listener;
		UIHelper.EnableButton(BtnCustomizing, enable: false);
		UIHelper.EnableButton(BtnSelectAvatar, enable: false);
		if (_selectFilter != null)
		{
			_selectFilter.InitFilter(SelectFilter.SelectType.SELECT_AVATAR, SetHeroList);
		}
		else
		{
			UIHelper.EnableGameObject(_selectFilter, enable: false);
		}
		_currentAvatarInfo = new UIAvatarHelper.AvatarInfo(GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AvatarHeroIndex);
		InitPreview(_currentAvatarInfo, usePlayerInfo: true);
		InitCostumeTypes();
	}

	private void InitPreview(UIAvatarHelper.AvatarInfo avatarInfo, bool usePlayerInfo = false)
	{
		if (!(HeroPreviewTexture == null))
		{
			RenderFacadeBuilder renderFacadeBuilder = CreateRenderFacadeBuilder(avatarInfo.AvatarIndex, usePlayerInfo);
			renderFacadeBuilder.SetPreload(AnimationKeyData.EmptyPreloadSet);
			UIHelper.InitHero3dPreview(ref _heroPreview, HeroPreviewTexture, avatarInfo.HeroIndex, 0, renderFacadeBuilder, "Customizing", perspective: false);
		}
	}

	private RenderFacadeBuilder CreateRenderFacadeBuilder(int avatarIndex, bool usePlayerInfo)
	{
		if (usePlayerInfo && GlobalAutoBehaviour<AccountManager>.instance.UserManager.PlayerAvatarHeroInfo == null)
		{
			return UIAvatarHelper.CreateRenderFacadeBuilder(avatarIndex);
		}
		UIAvatarHelper.AvatarInfo avatarInfo = new UIAvatarHelper.AvatarInfo(avatarIndex);
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(avatarInfo.HeroIndex);
		if (usePlayerInfo)
		{
			cachedHeroInfo = cachedHeroInfo.CreatePlayerAvatarHeroInfo(GlobalAutoBehaviour<AccountManager>.instance.UserManager.PlayerAvatarHeroInfo);
		}
		return new RenderFacadeBuilder(null, TeamType.Left, cachedHeroInfo);
	}

	private void InitCostumeTypes()
	{
		if (_costumeTypes == null)
		{
			_costumeTypes = new NGame2.NAccount.UserHeroManager.HeroChangeType[5]
			{
				NGame2.NAccount.UserHeroManager.HeroChangeType.Costume,
				NGame2.NAccount.UserHeroManager.HeroChangeType.HairCostume,
				NGame2.NAccount.UserHeroManager.HeroChangeType.WeaponCostume,
				NGame2.NAccount.UserHeroManager.HeroChangeType.AccessoryCostume,
				NGame2.NAccount.UserHeroManager.HeroChangeType.HideUniqueWeapon
			};
		}
	}

	public override void Reset()
	{
		base.Reset();
		ClosePreview();
		FactoryBase.ReleaseAndClearList(_listAvatarHero);
		RemoveSelectedHero();
	}

	private void ClosePreview()
	{
		UIHelper.CloseHero3dPreview(ref _heroPreview, HeroPreviewTexture);
	}

	private void SetHeroList()
	{
		FactoryBase.ReleaseAndClearList(_listAvatarHero);
		foreach (CachedHeroInfo item in GlobalAutoBehaviour<AccountManager>.instance.HeroManager)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(item.HeroIndex);
			if (data == null)
			{
				continue;
			}
			CostumeData costumeData = GameRoot.Instance.CostumeDataContainer.GetData(item.CostumeIndex);
			if (costumeData == null)
			{
				costumeData = GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(item.HeroIndex, item.Star);
			}
			UIAvatarHelper.AvatarInfo avatarInfo = new UIAvatarHelper.AvatarInfo(costumeData);
			SelectFilter.ComboBoxType mainComboBoxCurrentKey = _selectFilter.MainComboBoxCurrentKey;
			if (mainComboBoxCurrentKey == SelectFilter.ComboBoxType.ALL || mainComboBoxCurrentKey == SelectFilter.ComboBoxType.CREATURE)
			{
				CreatureTagType subComboBoxCurrentKey = (CreatureTagType)_selectFilter.SubComboBoxCurrentKey;
				if (_selectFilter.SubComboBoxCurrentKey == 0 || subComboBoxCurrentKey == data.TagType)
				{
					AddComponentAvatarGrid(avatarInfo);
				}
			}
		}
		UIHelper.RepositionGrid(AvatarGrid);
		UIHelper.EnableGameObject(_lbNoneHero, _listAvatarHero.Count <= 0);
	}

	private void AddComponentAvatarGrid(UIAvatarHelper.AvatarInfo avatarInfo)
	{
		if (GameRoot.Instance.CreatureDataContainer.GetData(avatarInfo.HeroIndex) != null && GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(avatarInfo.HeroIndex) != null)
		{
			AvatarHeroComponent avatarHeroComponent = UIHelper.AddGridItem(AvatarGrid, AvatarHeroComponent.PrefabAuto);
			avatarHeroComponent.Open(avatarInfo.AvatarIndex, avatarInfo.HeroIndex, this);
			_listAvatarHero.Add(avatarHeroComponent);
		}
	}

	public override void OnSystemBackButton()
	{
		OnClickExitButton();
	}

	public void OnClickExitButton()
	{
		Close();
	}

	private void RemoveSelectedHero()
	{
		FactoryBase.SafeReleaseComponent(ref _selectedHeroPortrait);
	}

	public void OnClickHeroCustomizing()
	{
		if (_listener != null)
		{
			_listener.OpenHeroCostumeView(_currentAvatarInfo.HeroIndex);
		}
	}

	public void OnClickConfirmAvatarHero()
	{
		if (_listener != null)
		{
			_listener.ChangeAvatarHero(_currentAvatarInfo);
		}
	}

	public void ScrollToSelectedComponent()
	{
		foreach (AvatarHeroComponent item in _listAvatarHero)
		{
			if (item.HeroIndex == _selectedHeroPortrait.HeroInfo.HeroIndex)
			{
				AvatarGrid.ScrollTo(item, 0f, 0f);
				break;
			}
		}
	}

	public CachedHeroInfo GetSelectedHeroInfo()
	{
		if (_selectedHeroPortrait == null)
		{
			return null;
		}
		return _selectedHeroPortrait.HeroInfo;
	}

	public void SelectAvatar(int avatarHeroIndex)
	{
		_currentAvatarInfo = new UIAvatarHelper.AvatarInfo(avatarHeroIndex);
		InitPreview(_currentAvatarInfo);
		UIHelper.EnableButton(BtnCustomizing, enable: true);
		UIHelper.EnableButton(BtnSelectAvatar, !PlayerAvatarInfo.IsEqualCurrentPlayerAvatarHero(_currentAvatarInfo));
	}

	void AvatarHeroComponent.Listener.OnClick(int avatarHeroIndex)
	{
		SelectAvatar(avatarHeroIndex);
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		if (changedInfo.HeroIndex == _currentAvatarInfo.HeroIndex && Array.Exists(_costumeTypes, (NGame2.NAccount.UserHeroManager.HeroChangeType type) => type == changedInfo.ChangeType))
		{
			CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(changedInfo.HeroIndex);
			UIAvatarHelper.AvatarInfo avatarInfo = GetAvatarInfo(heroInfo);
			if (_currentAvatarInfo.AvatarIndex != avatarInfo.AvatarIndex)
			{
				_currentAvatarInfo = avatarInfo;
			}
			_isChanged = true;
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_isChanged)
		{
			_isChanged = false;
			UpdateHeroInfo();
		}
	}

	private void UpdateHeroInfo()
	{
		InitPreview(_currentAvatarInfo);
		UIHelper.EnableButton(BtnSelectAvatar, !PlayerAvatarInfo.IsEqualCurrentPlayerAvatarHero(_currentAvatarInfo));
	}

	private UIAvatarHelper.AvatarInfo GetAvatarInfo(HeroInfo heroInfo)
	{
		CostumeData costumeData = null;
		costumeData = ((heroInfo.CostumeIndex == 0) ? GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(heroInfo.HeroIndex, heroInfo.Star) : GameRoot.Instance.CostumeDataContainer.GetData(heroInfo.CostumeIndex));
		return new UIAvatarHelper.AvatarInfo(costumeData);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
