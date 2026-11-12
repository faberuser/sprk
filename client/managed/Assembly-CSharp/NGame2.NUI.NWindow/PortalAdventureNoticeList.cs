using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class PortalAdventureNoticeList : BaseWindow, PortalComponent2.Listener
{
	public const string PrefabAdventureNoticePath = "Resources/Prefab2/Window/Portal/PortalAdventureNoticeList";

	private Action<int, int> _onClickSelectable;

	private Action _onClickDisabled;

	private Action _onClose;

	private int _chapterIndex;

	private int _dungeonIndex;

	private const int REFRESH_TIME = 12;

	private LocalStorageDateTime LastRequestAdvTime = new LocalStorageDateTime(DateTime.Now, typeof(PortalAdventureNoticeList).FullName + "_LastRequestAdvTime");

	protected List<AdventureNoticeComponent> _componentList = new List<AdventureNoticeComponent>();

	public UIScrollGrid ComponentGrid;

	public UILabel Label_Title;

	public static int OriginChapterIndex;

	public static int OriginDungeonIndex;

	public void Open(int ChapterIdx, int DungeonIdx, Action onOpen, Action<int, int> onClickSelectable, Action onClickDisabled, Action onClose)
	{
		_chapterIndex = ChapterIdx;
		_dungeonIndex = DungeonIdx;
		_onClickSelectable = onClickSelectable;
		_onClickDisabled = onClickDisabled;
		_onClose = onClose;
		Open(onOpen, Init);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	private void OnDisable()
	{
		if (_onClose != null)
		{
			Vespa_System_Action_Extension_Method.fire(_onClose);
		}
	}

	private void Init()
	{
		SetTitle();
		SetPotal();
	}

	public void SetTitle()
	{
		PortalData data = GameRoot.Instance.PortalDataContainer.GetData(_chapterIndex, _dungeonIndex);
		if (data != null && !String_Extension_Method.IsNullOrEmpty(data.TitleDescKey))
		{
			UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect(data.TitleDescKey));
		}
	}

	public void SetPotal()
	{
		IList<PortalData> allData = GameRoot.Instance.PortalDataContainer.GetAllData();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
		{
			PortalData portalData = allData[i];
			if ((!portalData.Hidden || Helper.CheckDropBonusEventNode(portalData.ChapterIndex, portalData.DungeonIndex)) && portalData.hasParent() && portalData.ParentNode[0] == _chapterIndex && portalData.ParentNode[1] == _dungeonIndex && !String_Extension_Method.IsNullOrEmpty(portalData.AdventureNoticeCode) && NGame2.NUI.Common.CheckContentsOpenCondition(portalData.Condition))
			{
				AdventureNoticeData data = GameRoot.Instance.AdventureNoticeDataContainer.GetData(portalData.AdventureNoticeCode);
				AdventureNoticeComponent adventureNoticeComponent = UIHelper.AddGridItem(ComponentGrid, AdventureNoticeComponent.PrefabAutoAdventureNotice);
				adventureNoticeComponent.Open(this, portalData.ChapterIndex, portalData.DungeonIndex, portalData.Name, portalData.ImagePath, data);
				_componentList.Add(adventureNoticeComponent);
			}
		}
		UIHelper.RepositionGrid(ComponentGrid);
	}

	public void LockClick(bool isLock)
	{
		foreach (AdventureNoticeComponent component in _componentList)
		{
			component.LockClick(isLock);
		}
	}

	public override void Close()
	{
		base.Close();
		Vespa_System_Action_Extension_Method.fire(_onClose);
	}

	public void SetOnClose(Action closeAction)
	{
		_onClose = closeAction;
	}

	void PortalComponent2.Listener.SelectPortal(PortalComponent2 component)
	{
	}

	void PortalComponent2.Listener.SelectButton(PortalComponent2 component)
	{
	}

	void PortalComponent2.Listener.StartDrag()
	{
	}

	void PortalComponent2.Listener.OnDrag()
	{
	}

	void PortalComponent2.Listener.EndDrag()
	{
	}

	bool PortalComponent2.Listener.CanOpenShorCutBtn()
	{
		return false;
	}

	void PortalComponent2.Listener.SelectEnableContentsShortCut(PortalComponent2 component)
	{
		Vespa_System_Action_Extension_Method.fire(_onClickSelectable, component.ChapterIndex, component.DungeonIndex);
	}

	void PortalComponent2.Listener.SelectDisableContentsShortCut(string disableText)
	{
		CenterMessage.Open(LocalizationManager.GetStringDirect(disableText));
	}
}
