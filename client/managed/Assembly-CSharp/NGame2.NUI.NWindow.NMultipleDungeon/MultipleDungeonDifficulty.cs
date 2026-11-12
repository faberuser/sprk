using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NMultipleDungeon;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NMultipleDungeon;

public class MultipleDungeonDifficulty : MonoBehaviour, MultipleDungeonDiffButtonComponent.IListener, MultipleDungeonDiffButtonComponent.IDataProvider
{
	public interface IListener
	{
		void SelectDiff(int diff);
	}

	public UISimpleGrid DiffSimpleGrid;

	public Transform NewTransform;

	public UILabel DiffLabel;

	public UISpriteSelector DiffSpriteSeletor;

	public UIPopupRoot DiffRoot;

	private IListener _iListener;

	private int _chapterIndex;

	private List<MultipleDungeonDiffButtonComponent> _diffButtonComponentList = new List<MultipleDungeonDiffButtonComponent>();

	public void Init(IListener iListener, int diff)
	{
		_iListener = iListener;
		SetName(diff);
		SetDiff(diff);
		EnableIsNew(enable: false);
	}

	public void Close()
	{
		ClearList();
	}

	private void ClearList()
	{
		FactoryBase.ReleaseAndClearList(_diffButtonComponentList);
	}

	private void SetDiff(int diff)
	{
		if (!(DiffSpriteSeletor == null))
		{
			DiffSpriteSeletor.Select(diff.ToString());
		}
	}

	private void SetName(int diff)
	{
		UIHelper.SetLabelText(DiffLabel, GetButtonNameLocalize(diff));
	}

	private void SetIsNew()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_diffButtonComponentList))
		{
			return;
		}
		MultipleDungeonDiffButtonComponent multipleDungeonDiffButtonComponent = null;
		bool enable = false;
		for (int i = 0; i < _diffButtonComponentList.Count; i++)
		{
			multipleDungeonDiffButtonComponent = _diffButtonComponentList[i];
			if (!(multipleDungeonDiffButtonComponent == null) && multipleDungeonDiffButtonComponent.IsNew)
			{
				enable = true;
				break;
			}
		}
		EnableIsNew(enable);
	}

	private void EnableIsNew(bool enable)
	{
		UIHelper.EnableGameObject(NewTransform, enable);
	}

	public void SetDifficulty(int chapterIndex, List<UnderPrisonDungeonData> list)
	{
		ClearList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list) || DiffSimpleGrid == null || DiffSimpleGrid.Grid == null)
		{
			return;
		}
		_chapterIndex = chapterIndex;
		UnderPrisonData underPrisonData = GetUnderPrisonData();
		if (underPrisonData == null)
		{
			return;
		}
		int openDiff = GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager.GetOpenDiff(chapterIndex);
		UnderPrisonDungeonData underPrisonDungeonData = null;
		MultipleDungeonDiffButtonComponent multipleDungeonDiffButtonComponent = null;
		for (int i = 0; i < list.Count; i++)
		{
			underPrisonDungeonData = list[i];
			if (underPrisonDungeonData != null)
			{
				multipleDungeonDiffButtonComponent = UIHelper.AddGridItem(DiffSimpleGrid, MultipleDungeonDiffButtonComponent.PrefabAuto);
				if (!(multipleDungeonDiffButtonComponent == null))
				{
					multipleDungeonDiffButtonComponent.Open(this, new MultipleDungeonDiffButtonComponent.MultipleDungeonDiffButtonInfo
					{
						IListener = this,
						Diff = underPrisonDungeonData.Diff,
						IsLock = (openDiff < underPrisonDungeonData.Diff),
						IsNew = GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager.CheckIsNewDiff(underPrisonData.Type, underPrisonDungeonData.Diff),
						ButtonNameLocalize = GetButtonNameLocalize(underPrisonDungeonData.Diff)
					});
					_diffButtonComponentList.Add(multipleDungeonDiffButtonComponent);
				}
			}
		}
		UIHelper.ResetGridPosition(DiffSimpleGrid.Grid);
		SetIsNew();
	}

	private string GetButtonNameLocalize(int diff)
	{
		return LocalizationManager.GetStringDirect("UNDERPRISON_DUNGEON_LEVEL_0" + diff);
	}

	private UnderPrisonData GetUnderPrisonData()
	{
		return GameRoot.Instance.UnderPrisonDataContainer.GetData(_chapterIndex);
	}

	void MultipleDungeonDiffButtonComponent.IListener.SelectDiff(int diff)
	{
		SetName(diff);
		SetDiff(diff);
		SetIsNew();
		if (_iListener != null)
		{
			_iListener.SelectDiff(diff);
		}
		if (DiffRoot != null)
		{
			DiffRoot.HidePopup(immediate: true);
		}
	}

	UnderPrisonType MultipleDungeonDiffButtonComponent.IDataProvider.GetUnderPrisonType()
	{
		return GetUnderPrisonData()?.Type ?? UnderPrisonType.None;
	}
}
