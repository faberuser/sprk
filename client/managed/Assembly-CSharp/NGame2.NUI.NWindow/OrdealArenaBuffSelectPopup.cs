using System.Collections.Generic;
using NGame2.NNotify;
using NGame2.NOrdeal;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class OrdealArenaBuffSelectPopup : BaseWindow, OrdealArenaBuffSelectComponent.Listener
{
	public interface IListener
	{
		void OnClickConfimeBuffSelect(OrdealArenaBuffData data);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/OrdealArena/OrdealArenaBuffSelectPopup";

	[Header("[Basic UI]")]
	public UITexture Background;

	public UIGrid BuffCompGrid;

	public UISimpleGrid SelectedBuffGrid;

	public UIButton ConfimeButton;

	[Header("[Component Rot Tween Control]")]
	public float StartDelay;

	public float CompDelayIntervals;

	private List<OrdealArenaBuffSelectComponent> _listBuffSelectComponents = new List<OrdealArenaBuffSelectComponent>();

	private List<OrdealArenaBuffIconComponent> _listBuffIconComponents = new List<OrdealArenaBuffIconComponent>();

	private OrdealArenaBuffSelectComponent _selectComp;

	private IListener _listener;

	public static void Open(IListener listener, bool isLobbyType = false)
	{
		if (GlobalAutoBehaviour<OrdealArenaManager>.isValidInstance && GlobalAutoBehaviour<OrdealArenaManager>.instance.HasSelectableBuff())
		{
			WindowPrefab<OrdealArenaBuffSelectPopup> windowPrefab = new WindowPrefab<OrdealArenaBuffSelectPopup>("Resources/Prefab2/Window/OrdealArena/OrdealArenaBuffSelectPopup", GameUIRoot.ELayerType.UI);
			windowPrefab.instance.OpenInternal(listener, isLobbyType);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}
	}

	public void OpenInternal(IListener listener, bool isLobbyType)
	{
		base.Open();
		_listener = listener;
		List<int> selectableBuffList = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetSelectableBuffList();
		InitSelectableBuffList(selectableBuffList);
		InitSelectedBuffList();
		RefreshButton();
		EnableSelectedBuffGrid(!isLobbyType);
		InviteNotifyWindow currentWindow = GlobalAutoBehaviour<InviteNotifyManager>.instance.CurrentWindow;
		if (currentWindow != null)
		{
			currentWindow.Close();
		}
	}

	public override void Close()
	{
		_listener = null;
		_listBuffSelectComponents.ForEach(delegate(OrdealArenaBuffSelectComponent x)
		{
			x.StartFadeOut();
		});
		base.Close();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ReleaseAndClearDatas();
	}

	private void InitSelectableBuffList(List<int> buffList)
	{
		ReleaseAndClearDatas();
		int num = 0;
		foreach (int buff in buffList)
		{
			OrdealArenaBuffData data = GameRoot.Instance.OrdealArenaBuffDataContainer.GetData(buff);
			if (data != null)
			{
				OrdealArenaBuffSelectComponent ordealArenaBuffSelectComponent = OrdealArenaBuffSelectComponent.PrefabAuto.Factory.Create(BuffCompGrid.transform);
				if (!(ordealArenaBuffSelectComponent == null))
				{
					float rotTweenDelay = StartDelay + (float)num * CompDelayIntervals;
					ordealArenaBuffSelectComponent.Open(this, data, rotTweenDelay);
					_listBuffSelectComponents.Add(ordealArenaBuffSelectComponent);
					num++;
				}
			}
		}
		BuffCompGrid.Reposition();
	}

	private void InitSelectedBuffList()
	{
		FactoryBase.ReleaseAndClearList(_listBuffIconComponents);
		List<int> selectedBuffList = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetSelectedBuffList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(selectedBuffList))
		{
			return;
		}
		int num = 0;
		while (num < __Extension_Method_ICollection__.COUNT(selectedBuffList))
		{
			int num2 = selectedBuffList[num];
			OrdealArenaBuffData data = GameRoot.Instance.OrdealArenaBuffDataContainer.GetData(num2);
			if (data == null)
			{
				continue;
			}
			OrdealArenaBuffIconComponent ordealArenaBuffIconComponent = UIHelper.AddGridItem(SelectedBuffGrid, OrdealArenaBuffIconComponent.PrefabAuto);
			if (ordealArenaBuffIconComponent == null)
			{
				continue;
			}
			int num3 = 0;
			for (int i = num; i < __Extension_Method_ICollection__.COUNT(selectedBuffList); i++)
			{
				int num4 = selectedBuffList[i];
				if (num2 != num4)
				{
					break;
				}
				num3++;
			}
			ordealArenaBuffIconComponent.SetInfo(data, num3);
			_listBuffIconComponents.Add(ordealArenaBuffIconComponent);
			num += num3;
		}
		UIHelper.RepositionGrid(SelectedBuffGrid);
	}

	private void ReleaseAndClearDatas()
	{
		FactoryBase.ReleaseAndClearList(_listBuffSelectComponents);
		_selectComp = null;
	}

	private void RefreshButton()
	{
		UIHelper.EnableButton(ConfimeButton, _selectComp != null);
	}

	private void EnableSelectedBuffGrid(bool isEnable)
	{
		UIHelper.EnableGameObject(SelectedBuffGrid, isEnable);
	}

	public void OnClickConfimeBuffSelect()
	{
		if (_listener != null && !(_selectComp == null))
		{
			OrdealArenaBuffData buffData = _selectComp.GetBuffData();
			if (buffData != null)
			{
				_listener.OnClickConfimeBuffSelect(buffData);
			}
		}
	}

	public override void OnSystemBackButton()
	{
	}

	void OrdealArenaBuffSelectComponent.Listener.OnClickOrdealBuffComponent(OrdealArenaBuffSelectComponent comp)
	{
		OrdealArenaBuffSelectComponent selectComp = _selectComp;
		if (!(selectComp == comp))
		{
			if (selectComp != null)
			{
				selectComp.SetSelected(isSelect: false);
			}
			comp.SetSelected(isSelect: true);
			_selectComp = comp;
			RefreshButton();
		}
	}
}
