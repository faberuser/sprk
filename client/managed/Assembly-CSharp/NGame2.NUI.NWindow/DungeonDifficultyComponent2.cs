using System;
using System.Collections.Generic;
using NNguiExtension;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class DungeonDifficultyComponent2 : MonoBehaviour, IInspectorGuiDrawer
{
	[Serializable]
	public class PopupButtonUiData
	{
		public int DifficultyType;

		public UIButton Button;

		public UIWidget NewMarkWidget;

		public UIWidget LockedWidget;
	}

	public UISpriteSelector DifficultyBgSelector;

	public UILabel DifficultyLabel;

	public UIWidget DifficultyNewMark;

	public UIButton CurrentDifficultyButton;

	public UISprite Sprite_Dir;

	public List<PopupButtonUiData> PopupButtonList;

	private Func<int, string> _nameConverter;

	private Action<PopupButtonUiData> _onSelect;

	public int CurrentDifficulty { get; private set; }

	private int MaxClearedDifficulty { get; set; }

	private int MaxPlayableDifficulty { get; set; }

	public void Init(DungeonDifficultyType cur, DungeonDifficultyType max)
	{
		Init((int)cur, (int)max, (int d) => Enum.GetName(typeof(DungeonDifficultyType), d));
	}

	public void Init(UndergroundDifficultType cur, UndergroundDifficultType max)
	{
		Init((int)cur, (int)max, (int d) => Enum.GetName(typeof(UndergroundDifficultType), d));
	}

	private void Init(int diff, int max, Func<int, string> nameConverter)
	{
		MaxClearedDifficulty = Mathf.Min(diff, max);
		MaxPlayableDifficulty = max;
		CurrentDifficulty = MaxPlayableDifficulty;
		_nameConverter = nameConverter;
		_onSelect = null;
		RefreshButtons();
	}

	public void EnableSelect(bool enable)
	{
		CurrentDifficultyButton.enabled = enable;
	}

	public void SetCallback(Action<PopupButtonUiData> onSelect)
	{
		_onSelect = onSelect;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PopupButtonList); i++)
		{
			PopupButtonUiData data = PopupButtonList[i];
			UIButton button = data.Button;
			if (!(button == null))
			{
				button.onClick.Clear();
				button.onClick.Add(new EventDelegate(delegate
				{
					Vespa_System_Action_Extension_Method.fire(_onSelect, data);
				}));
			}
		}
	}

	public void SetAutoSelect()
	{
		SetCallback(delegate(PopupButtonUiData data)
		{
			SelectDifficuly(data.DifficultyType);
		});
	}

	public bool OnClickDifficultyButton(GameObject senderObject)
	{
		if (senderObject == null)
		{
			return false;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PopupButtonList); i++)
		{
			PopupButtonUiData popupButtonUiData = PopupButtonList[i];
			if (popupButtonUiData != null && !(popupButtonUiData.Button.gameObject != senderObject))
			{
				return SelectDifficuly(popupButtonUiData.DifficultyType);
			}
		}
		return false;
	}

	public bool SelectDifficuly(int difficulty)
	{
		if (CurrentDifficulty == difficulty)
		{
			return false;
		}
		if (difficulty > MaxPlayableDifficulty)
		{
			return false;
		}
		CurrentDifficulty = difficulty;
		RefreshButtons();
		return true;
	}

	private void RefreshButtons()
	{
		string strDungeonDiff = Vespa_System_Action_Extension_Method.fire(_nameConverter, CurrentDifficulty);
		if (DifficultyBgSelector != null)
		{
			DifficultyBgSelector.Select(strDungeonDiff);
		}
		DungeonDifficultyType dungeonDiffType = GetDungeonDiffType(strDungeonDiff);
		UIHelper.SetLabelText(DifficultyLabel, UIHelper.DungeonDifficultyLocale(dungeonDiffType));
		UIHelper.EnableGameObject(DifficultyNewMark, MaxPlayableDifficulty > MaxClearedDifficulty);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PopupButtonList); i++)
		{
			PopupButtonUiData popupButtonUiData = PopupButtonList[i];
			int difficultyType = popupButtonUiData.DifficultyType;
			bool flag = difficultyType <= MaxPlayableDifficulty;
			bool enable = flag && difficultyType > MaxClearedDifficulty;
			UIHelper.EnableButton(popupButtonUiData.Button, flag, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(popupButtonUiData.LockedWidget, !flag);
			UIHelper.EnableGameObject(popupButtonUiData.NewMarkWidget, enable);
		}
	}

	private DungeonDifficultyType GetDungeonDiffType(string strDungeonDiff)
	{
		if (strDungeonDiff == DungeonDifficultyType.Easy.ToString())
		{
			return DungeonDifficultyType.Easy;
		}
		if (strDungeonDiff == DungeonDifficultyType.Normal.ToString())
		{
			return DungeonDifficultyType.Normal;
		}
		if (strDungeonDiff == DungeonDifficultyType.Hard.ToString())
		{
			return DungeonDifficultyType.Hard;
		}
		if (strDungeonDiff == DungeonDifficultyType.Hell.ToString())
		{
			return DungeonDifficultyType.Hell;
		}
		return DungeonDifficultyType.Easy;
	}
}
