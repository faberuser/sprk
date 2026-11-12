using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class HeroSkillListPanel : MonoBehaviour, IHeroPanel, HeroSkillSubComponent.Listener, IUserObserver<ItemInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver<UserManager.UserStatInfo>
{
	public interface Listener
	{
		void OnSelectSkillComponent(int skillSlotIndex, int linkedIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Hero/HeroSkillListPanel";

	public static PrefabAuto<HeroSkillListPanel> PrefabAuto = new PrefabAuto<HeroSkillListPanel>("Resources/Prefab2/Component/Hero/HeroSkillListPanel");

	public UIWidget RootWidget;

	public UIScrollGrid SkillListGrid;

	public UISimpleGrid SkillBookGrid;

	public UIButton ButtonAllMax;

	private List<Component> _componentList = new List<Component>();

	private List<UpgradeBookItemIconComponent> _bookComponentList = new List<UpgradeBookItemIconComponent>();

	private Listener _listener;

	private bool _isMine;

	public CachedHeroInfo CachedHeroInfo { get; private set; }

	public void Open(CachedHeroInfo cachedHeroInfo, Listener listener, bool isMine = true)
	{
		Reset();
		base.gameObject.SetActive(value: true);
		CachedHeroInfo = cachedHeroInfo;
		_listener = listener;
		_isMine = isMine;
		SetupSkillBookList();
		SetupSkillSubComponent();
		RaiseEventEnterHeroSkillListPage();
		SprkSkillUpgradeAll.Attach(this);
	}

	private void RaiseEventEnterHeroSkillListPage()
	{
		if (_isMine)
		{
			EnterHeroSkillListPage triggerEvent = new EnterHeroSkillListPage();
			SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
		}
	}

	public void Reset()
	{
		SprkSkillUpgradeAll.Release(this);
		_listener = null;
		FactoryBase.ReleaseAndClearList(_componentList);
		FactoryBase.ReleaseAndClearList(_bookComponentList);
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		Reset();
		RecommendTranscendContainer instance = SceneAutoBehaviour<MyHeroManagement>.instance.GetInstance<RecommendTranscendContainer>();
		if (instance != null && instance.IsOpen())
		{
			instance.Close();
		}
	}

	public void Refresh()
	{
		if (base.gameObject.activeSelf)
		{
			UpdateSkillComponent();
		}
	}

	private void SetupSkillBookList()
	{
		if (_isMine)
		{
			for (int i = 0; i < 4; i++)
			{
				UpgradeBookItemIconComponent upgradeBookItemIconComponent = UIHelper.AddGridItem(SkillBookGrid, UpgradeBookItemIconComponent.PrefabAuto);
				Helper.SkillBookInfo skillBookInfo = Helper.GetSkillBookInfo(CachedHeroInfo, i + 1);
				upgradeBookItemIconComponent.Open(skillBookInfo.BookItemData, skillBookInfo.CurItemCount, skillBookInfo.CreatureTagType, i + 1);
				_bookComponentList.Add(upgradeBookItemIconComponent);
			}
			UIHelper.RepositionGrid(SkillBookGrid);
		}
	}

	private void SetupSkillSubComponent()
	{
		for (int i = 0; i < 4; i++)
		{
			HeroSkillSubComponent heroSkillSubComponent = UIHelper.AddGridItem(SkillListGrid, HeroSkillSubComponent.PrefabAuto);
			heroSkillSubComponent.Open(CachedHeroInfo, i + 1, this, _isMine);
			_componentList.Add(heroSkillSubComponent);
		}
		UIHelper.RepositionGrid(SkillListGrid);
	}

	void HeroSkillSubComponent.Listener.OnSelectSkillComponent(HeroSkillSubComponent caller)
	{
		if (_listener != null)
		{
			_listener.OnSelectSkillComponent(caller.SkillSlotIndex, caller.LinkedIndex);
		}
	}

	void HeroSkillSubComponent.Listener.OnClickSwitchLinkedSkill(HeroSkillSubComponent comp)
	{
	}

	private void UpdateSkillComponent()
	{
		_componentList.ForEach(delegate(Component comp)
		{
			if (comp is SkillSubComponent skillSubComponent)
			{
				skillSubComponent.Refresh();
			}
		});
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo itemInfo)
	{
		if (itemInfo == null)
		{
			SprkSkillUpgradeAll.Changed(this);
			return;
		}
		if (CachedHeroInfo == null)
		{
			SprkSkillUpgradeAll.Changed(this);
			return;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemInfo.ItemIndex);
		if (data == null || data.Type != ItemType.UpgradeBook)
		{
			SprkSkillUpgradeAll.Changed(this);
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_bookComponentList); i++)
		{
			UpgradeBookItemIconComponent upgradeBookItemIconComponent = _bookComponentList[i];
			if (Vespa_Common_Class_Extension_Method.EQUALS(data, _bookComponentList[i].ItemData))
			{
				upgradeBookItemIconComponent.SetItemCount(itemInfo.Count, CountLabelTween: true);
			}
		}
		SprkSkillUpgradeAll.Changed(this);
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo info)
	{
		if (CachedHeroInfo != null && info.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.SkillExtended)
		{
			UpdateSkillExtend(info.HeroIndex, info.SkillSlotIndex, info.NewValue);
		}
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
	}

	private SkillSubComponent GetSkillSubComponent(int heroIndex, int skillSlotIndex)
	{
		return _componentList.Find(delegate(Component comp)
		{
			if (!(comp is SkillSubComponent skillSubComponent))
			{
				return false;
			}
			return skillSubComponent.HeroInfo.HeroIndex == heroIndex && skillSubComponent.SkillSlotIndex == skillSlotIndex;
		}) as SkillSubComponent;
	}

	private void UpdateSkillExtend(int heroIndex, int skillSlotIndex, int skillExtend)
	{
		GetSkillSubComponent(heroIndex, skillSlotIndex)?.SetSkillExtend(skillExtend);
		SprkSkillUpgradeAll.Changed(this);
	}
}
