using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SweepResultPopup : BaseRewardPopup
{
	public new static string PrefabPath = "Resources/Prefab2/Window/MessagePopup/SweepResultPopup";

	public UIPanel RootPanel;

	public UITable SweepTable;

	public GameObject StartRoot;

	public GameObject EndRoot;

	public GameObject FloorRoot;

	public GameObject CountRoot;

	public UILabel LabelStart;

	public UILabel LabelEnd;

	public UILabel LabelFloor;

	public UILabel LabelCount;

	private BattleType _battleType;

	private Action _onClose;

	private List<ItemIconComponent> _compList = new List<ItemIconComponent>();

	public string Title { get; set; }

	public string Message { get; set; }

	public static SweepResultPopup Open(PopupOpenData popupOpen, BattleType battleType, Action onClose = null)
	{
		WindowPrefab<SweepResultPopup> windowPrefab = new WindowPrefab<SweepResultPopup>(PrefabPath, GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(popupOpen, battleType, onClose);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	protected void OpenInternal(PopupOpenData popupOpen, BattleType battleType, Action onClose = null)
	{
		_onClose = onClose;
		_battleType = battleType;
		_popupOpen = popupOpen;
		OpenWithData(_popupOpen, isGlobal: true, _onClose, isOpenWithNoReward: true);
	}

	public void SetText(SweepDungeonType dungeonType, int start, int end, long count)
	{
		bool flag = start == end;
		UIHelper.EnableGameObject(FloorRoot, flag);
		UIHelper.EnableGameObject(StartRoot, !flag);
		UIHelper.EnableGameObject(EndRoot, !flag);
		if (flag)
		{
			string stepLocalText = SweepHelper.GetStepLocalText(dungeonType, start);
			UIHelper.SetLabelText(LabelFloor, stepLocalText);
		}
		else
		{
			string stepLocalText2 = SweepHelper.GetStepLocalText(dungeonType, start);
			string stepLocalText3 = SweepHelper.GetStepLocalText(dungeonType, end);
			UIHelper.SetLabelText(LabelStart, stepLocalText2);
			UIHelper.SetLabelText(LabelEnd, stepLocalText3);
		}
		string stringDirect = LocalizationManager.GetStringDirect("SWEEP_RESULT_COUNT_DESC", count);
		UIHelper.SetLabelText(LabelCount, stringDirect);
		if ((bool)SweepTable)
		{
			SweepTable.Reposition();
		}
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_compList);
	}

	protected override void SetBattleType(out BattleType battleType)
	{
		battleType = _battleType;
	}

	public void OnClickConfirm()
	{
		Close();
	}

	public void OnClickClose()
	{
		Close();
	}
}
