using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NHelper.NPortal;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ContentsMenuComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClickCategoryButton(ContentsMenuComponent component, PortalRenewalData portalRenewalData);

		bool IsEditMode();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalRenewalComponents/ContentsMenuComponent";

	public const string PrefabPath2 = "Resources/Prefab2/Window/Portal/PortalRenewalComponents/ContentsPinedMenuComponent";

	public static PrefabAuto<ContentsMenuComponent> PrefabAuto = new PrefabAuto<ContentsMenuComponent>("Resources/Prefab2/Window/Portal/PortalRenewalComponents/ContentsMenuComponent");

	public static PrefabAuto<ContentsMenuComponent> PrefabAuto2 = new PrefabAuto<ContentsMenuComponent>("Resources/Prefab2/Window/Portal/PortalRenewalComponents/ContentsPinedMenuComponent");

	public UISpriteSelector ContentsIcon;

	public UISprite ContentsBg;

	public UILabel MenuTitleLabel;

	public UISprite LockedSprite;

	[Header("[Event Buff Group]")]
	public UISprite EventFlagSprite;

	[Header("[Contents Notice State Group]")]
	public Transform NoticeStateIconRoot;

	public UISpriteSelector StateIconSelector;

	[Header("[Select Animation & Tween]")]
	public Transform SelectAnimation;

	public TweenAlpha SelectTween;

	[Header("[Openning Effect]")]
	public TweenRotation TwRot;

	[Header("[Color Template]")]
	public Color IconDisableColor;

	public Color BgDisableColor;

	private Listener _listener;

	private List<PortalRenewal.NoticeState> _childNoticeList = new List<PortalRenewal.NoticeState>();

	private float _effectDelay;

	public PortalRenewalData _portalRenewalData { get; private set; }

	public PortalRenewal.LockedState LockedState { get; private set; }

	public PortalRenewal.NoticeState NoticeState { get; private set; }

	public PortalRenewalCategory Category
	{
		get
		{
			if (_portalRenewalData == null)
			{
				return PortalRenewalCategory.NONE;
			}
			return _portalRenewalData.Category;
		}
	}

	public void Open(PortalRenewalData portalRenewalData, Listener listener = null, float effectDelay = 0f)
	{
		_portalRenewalData = portalRenewalData;
		_listener = listener;
		LockedState = PortalRenewal.LockedState.NONE;
		_effectDelay = effectDelay;
		Init();
	}

	private void Init()
	{
		base.transform.name = _portalRenewalData.Category.ToString();
		if (_portalRenewalData.SubCategory != PortalRenewalSubCategory.WorldBoss)
		{
			UIHelper.SetLabelText(MenuTitleLabel, LocalizationManager.GetStringDirect(_portalRenewalData.MenuButtonLocalkey));
		}
		else
		{
			UIHelper.SetLabelText(MenuTitleLabel, string.Empty);
		}
		SetContentsMenuSprite();
		CheckLockedState();
		CheckEventFlag();
		SetSelectedHighLight(PortalRenewal.MenuEditMode.IDLE, isSelected: false);
		CheckChildNoticeState();
		SetOpenningEffect();
	}

	public void SetContentsMenuSprite()
	{
		ContentsIcon.SelectEnum(_portalRenewalData.Category);
	}

	public void CheckEventFlag()
	{
		if (LockedState != PortalRenewal.LockedState.NONE)
		{
			UIHelper.EnableGameObject(EventFlagSprite, enable: false);
			return;
		}
		IList<PortalRenewalData> data = GameRoot.Instance.PortalRenewalDataContainer.GetData(_portalRenewalData.Category);
		bool enable = false;
		foreach (PortalRenewalData item in data)
		{
			if (item.SubCategory != PortalRenewalSubCategory.MENUBUTTON && SingletonAuto<ConditionChecker>.instance.Check(item.OpenCondition) && (item.TutorialIndex <= 0 || GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(item.TutorialIndex)) && EventConditionChecker(item.EventBuffCondition))
			{
				enable = true;
				break;
			}
		}
		UIHelper.EnableGameObject(EventFlagSprite, enable);
	}

	public void CheckChildNoticeState(bool isForce = false)
	{
		if (!isForce && _listener.IsEditMode())
		{
			return;
		}
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_childNoticeList);
		if (LockedState != PortalRenewal.LockedState.NONE)
		{
			UIHelper.EnableGameObject(NoticeStateIconRoot, enable: false);
			return;
		}
		foreach (PortalRenewalData datum in GameRoot.Instance.PortalRenewalDataContainer.GetData(_portalRenewalData.Category))
		{
			if (datum != null && datum.SubCategory != PortalRenewalSubCategory.MENUBUTTON && SingletonAuto<ConditionChecker>.instance.Check(datum.ShowCondition) && PortalHelper.GetLockedReason(datum) == PortalRenewal.LockedState.NONE)
			{
				PortalRenewal.NoticeState contentsNoticeState = PortalHelper.GetContentsNoticeState(datum, updateState: false);
				if (contentsNoticeState != PortalRenewal.NoticeState.NONE)
				{
					_childNoticeList.Add(contentsNoticeState);
				}
			}
		}
		UpdateNoticeState();
	}

	private void SetOpenningEffect()
	{
		if (!_listener.IsEditMode() && !(TwRot == null))
		{
			base.transform.localEulerAngles = TwRot.from;
			TwRot.delay = _effectDelay;
			TwRot.PlayForwardReset();
		}
	}

	private void UpdateNoticeState()
	{
		PortalRenewal.NoticeState noticeState = PortalRenewal.NoticeState.NONE;
		foreach (PortalRenewal.NoticeState childNotice in _childNoticeList)
		{
			if (noticeState < childNotice)
			{
				noticeState = childNotice;
			}
			if (noticeState >= PortalRenewal.NoticeState.UNCONFIRMED_AND_NEW)
			{
				break;
			}
		}
		ChangeNoticeState(noticeState);
	}

	public void ChangeNoticeState(PortalRenewal.NoticeState noticeState)
	{
		NoticeState = noticeState;
		UIHelper.EnableGameObject(NoticeStateIconRoot, NoticeState != PortalRenewal.NoticeState.NONE);
		StateIconSelector.SelectEnum(NoticeState);
	}

	private bool EventConditionChecker(string[] values)
	{
		for (int i = 0; i + 1 < __Extension_Method_ICollection__.COUNT(values); i += 2)
		{
			EventBonusType valueEnum = ArgParser.GetValueEnum(values, i, EventBonusType.None);
			string valueString = ArgParser.GetValueString(values, i + 1);
			if (GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(valueEnum, valueString) != null)
			{
				return true;
			}
		}
		return false;
	}

	public void CheckLockedState()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_portalRenewalData.OpenCondition) && !SingletonAuto<ConditionChecker>.instance.Check(_portalRenewalData.OpenCondition))
		{
			ChangeLockState(PortalRenewal.LockedState.CONDITION_LOCKED);
		}
		else if (_portalRenewalData.TutorialIndex > 0 && !GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(_portalRenewalData.TutorialIndex))
		{
			ChangeLockState(PortalRenewal.LockedState.TUTORIAL_LOCKED);
		}
		else
		{
			ChangeLockState(PortalRenewal.LockedState.NONE);
		}
	}

	public void ChangeLockState(PortalRenewal.LockedState lockedState)
	{
		UIHelper.EnableGameObject(LockedSprite, lockedState != PortalRenewal.LockedState.NONE);
		LockedState = lockedState;
		DisableContentsSpirtes(lockedState != PortalRenewal.LockedState.NONE);
	}

	private void DisableContentsSpirtes(bool disable)
	{
		UISprite targetSprite = ContentsIcon.GetTargetSprite();
		if (disable)
		{
			targetSprite.color = IconDisableColor;
			ContentsBg.color = BgDisableColor;
		}
		else
		{
			targetSprite.color = Color.white;
			ContentsBg.color = Color.white;
		}
	}

	public void SetSelectedHighLight(PortalRenewal.MenuEditMode editMode, bool isSelected)
	{
		switch (editMode)
		{
		case PortalRenewal.MenuEditMode.IDLE:
			UIHelper.EnableGameObject(SelectTween, enable: false);
			UIHelper.EnableGameObject(SelectAnimation, isSelected);
			break;
		case PortalRenewal.MenuEditMode.RUN:
			UIHelper.EnableGameObject(SelectAnimation, enable: false);
			UIHelper.EnableGameObject(SelectTween, isSelected);
			if (isSelected)
			{
				SelectTween.PlayForwardReset();
			}
			break;
		}
	}

	public void OnClickCategoryButton()
	{
		if (_listener != null)
		{
			_listener.OnClickCategoryButton(this, _portalRenewalData);
		}
	}
}
