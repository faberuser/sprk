using System.Collections.Generic;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[UIEvent(AllowDuringPatchDownloading = true)]
public class MailListPopup : BaseWindow, MailListComponent.Listener
{
	public interface Listener
	{
		void OnReceiveAll(MailListPopup caller, int TotalCount);

		bool OnCloseRequest(MailListPopup caller);

		void OnClickMail(MailListComponent mailListComponent);

		void OpenMailListPopup(MailListPopup mailListPopup);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Mail/MailListPopup";

	private Listener _listener;

	public UILabel NumofMailLabel;

	public GameObject EmptyObject;

	public UIScrollGrid MailListGrid;

	public UIButton ReceiveAllButton;

	private List<MailListComponent> _componentList = new List<MailListComponent>();

	private int _totalCount;

	public void Open(Listener listener)
	{
		Open(delegate
		{
			Init(listener);
			if (_listener != null)
			{
				_listener.OpenMailListPopup(this);
			}
		}, null);
	}

	public void Init(Listener listener)
	{
		_listener = listener;
		SingletonAuto<MailInfoContainer>.instance.Open();
		UpdateButtonState();
		SprkMailClaimAll.Caption(this);
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	private void Clear()
	{
		_listener = null;
		ClearMailList();
	}

	public void ClearMailList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
		UpdateButtonState();
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public override void OnClosing()
	{
		SprkMailClaimAll.Cancel(this);
		base.OnClosing();
		Clear();
	}

	private void InsertMail(NShared.MailInfo mail)
	{
		long mailIndex = mail.MailIndex;
		if (!(_componentList.Find((MailListComponent e) => e.MailInfo.MailIndex == mailIndex) != null))
		{
			bool isNew = MailListComponent.IsNewMail(mailIndex);
			MailListComponent mailListComponent = MailListGrid.AddGridItem(MailListComponent.PrefabAuto);
			mailListComponent.Open(mail, isNew, this);
			_componentList.Add(mailListComponent);
		}
	}

	public void UpdateUIMailList(List<NShared.MailInfo> mailList, int totalCount, long lastIndex, bool isResetPosition = true)
	{
		if (lastIndex == 0L)
		{
			ClearMailList();
			foreach (NShared.MailInfo mail in mailList)
			{
				InsertMail(mail);
			}
		}
		else
		{
			bool flag = true;
			foreach (NShared.MailInfo mail2 in mailList)
			{
				if (flag)
				{
					if (mail2.MailIndex == lastIndex)
					{
						flag = false;
						InsertMail(mail2);
					}
				}
				else
				{
					InsertMail(mail2);
				}
			}
		}
		_totalCount = totalCount;
		UpdateUIGrid(isResetPosition);
	}

	private void UpdateUIGrid(bool isResetPosition = true)
	{
		UpdateButtonState();
		UIHelper.SetLabelText(NumofMailLabel, $"{_totalCount}");
		UIHelper.EnableGameObject(EmptyObject, _componentList.Count == 0);
		if (isResetPosition)
		{
			UIHelper.RepositionGrid(MailListGrid);
		}
		else if (MailListGrid != null && MailListGrid.Grid != null && MailListGrid.ScrollView != null)
		{
			MailListGrid.Grid.Reposition();
			MailListGrid.ScrollView.UpdateScrollbars();
		}
	}

	public void DeleteMail(long mailIndex, int totalCount)
	{
		_totalCount = totalCount;
		MailListComponent mailListComponent = _componentList.Find((MailListComponent e) => e.MailInfo.MailIndex == mailIndex);
		if (!(mailListComponent == null))
		{
			_componentList.Remove(mailListComponent);
			FactoryBase.ReleaseComponent(mailListComponent);
			UpdateUIGrid(isResetPosition: false);
		}
	}

	private void UpdateButtonState()
	{
		MailListComponent mailListComponent = _componentList.Find((MailListComponent e) => !e.isNormal());
		UIHelper.EnableButton(ReceiveAllButton, mailListComponent != null);
	}

	public void UpdateReadState()
	{
		_componentList.ForEach(delegate(MailListComponent e)
		{
			e.UpdateReadState();
		});
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	public void OnClickClose()
	{
		if (_listener == null || _listener.OnCloseRequest(this))
		{
			Close();
		}
	}

	void MailListComponent.Listener.OnClick(MailListComponent caller)
	{
		if (_listener != null)
		{
			_listener.OnClickMail(caller);
		}
	}

	public void OnClickReceiveAllMail()
	{
		if (_listener != null)
		{
			_listener.OnReceiveAll(this, _totalCount);
			_totalCount = 0;
		}
	}
}
