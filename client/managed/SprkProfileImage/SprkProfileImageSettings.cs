using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NShared;
using UnityEngine;

/// <summary>
/// Settings > Game > "Profile Pictures": Show (default) or Hide. Hidden, every
/// portrait shows the leader hero icon as before, and other players' pictures
/// are not downloaded. The My Info Profile tab preview keeps showing the
/// player's own picture so it can still be managed. A local preference only:
/// it never changes what other players see.
/// </summary>
public static partial class SprkProfileImage
{
	private const string ShownKey = "Sprk.ProfilePictures.Show";

	private static bool shownLoaded;

	private static bool shown = true;

	/// <summary>Loaders that show the picture even while pictures are hidden (the Profile tab preview).</summary>
	private static readonly HashSet<UIBundleLoader> previews = new HashSet<UIBundleLoader>();

	public static bool Shown
	{
		get
		{
			if (!shownLoaded)
			{
				shownLoaded = true;
				shown = SprkTestPrefs.GetInt(ShownKey, 1) != 0;
			}
			return shown;
		}
		set
		{
			bool changed = Shown != value;
			shown = value;
			SprkTestPrefs.SetInt(ShownKey, value ? 1 : 0);
			SprkTestPrefs.Save();
			if (!changed)
			{
				return;
			}
			if (!value)
			{
				// Nothing more to look up; cached pictures are dropped, not shown.
				ClearRemote();
			}
			foreach (UIBundleLoader item in new List<UIBundleLoader>(watched))
			{
				if (item != null && !previews.Contains(item))
				{
					item.SprkReload();
				}
			}
			RaiseChanged();
		}
	}

	/// <summary>Like <see cref="Watch"/>, but shown regardless of <see cref="Shown"/>.</summary>
	public static void WatchPreview(UIBundleLoader loader)
	{
		if (loader != null)
		{
			previews.RemoveWhere((UIBundleLoader l) => l == null);
			previews.Add(loader);
			Watch(loader);
		}
	}

	private static bool ShowsOn(UIBundleLoader loader)
	{
		if (!Shown)
		{
			return previews.Contains(loader);
		}
		return true;
	}

	public const string SettingsRowName = "SPRK Profile Pictures";

	private static readonly string[] settingsChoices = new string[2] { "Show", "Hide" };

	/// <summary>Adds (once) the Show/Hide row at the end of the Game tab.</summary>
	public static GameOptionToggle4Component SetupSettings(GameOptionWindow window)
	{
		if (window == null || window.MainTabs == null)
		{
			return null;
		}
		GameOptionWindow.TabUIInfo tab = window.MainTabs.FirstOrDefault((GameOptionWindow.TabUIInfo t) => t.GameOptionTab == GameOptionTab.Game && t.GameOptionGrid != null);
		if (tab == null)
		{
			return null;
		}
		GameOptionToggle4Component row = tab.GameOptionGrid.GetComponentsInChildren<GameOptionToggle4Component>(includeInactive: true).FirstOrDefault((GameOptionToggle4Component r) => r.name == SettingsRowName);
		bool created = row == null;
		if (created)
		{
			row = UIHelper.AddGridItem(tab.GameOptionGrid, GameOptionToggle4Component.PrefabAuto);
			row.name = SettingsRowName;
			// The row is driven here, not by the options table.
			row.enabled = false;
			row.ParentRoot.Root.SetActive(value: true);
			if (row.ChildRoot != null && row.ChildRoot.Root != null && row.ChildRoot.Root != row.ParentRoot.Root)
			{
				row.ChildRoot.Root.SetActive(value: false);
			}
			if (row.ParentRoot.ToolTipImage != null)
			{
				row.ParentRoot.ToolTipImage.SetActive(value: false);
			}
		}
		row.ParentRoot.Label_OptionName.text = SprkText.Get("Profile Pictures");
		UIToggle[] array = new UIToggle[4]
		{
			row.toggleInfo.Toggle1,
			row.toggleInfo.Toggle2,
			row.toggleInfo.Toggle3,
			row.toggleInfo.Toggle4
		};
		UIToggle[] toggles = array.Take(settingsChoices.Length).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].onChange.Clear();
			array[i].group = 0;
			if (i >= toggles.Length)
			{
				array[i].Set(state: false);
				array[i].gameObject.SetActive(value: false);
			}
		}
		UIGrid grid = toggles[0].transform.parent.GetComponent<UIGrid>();
		if (grid != null)
		{
			// Positions are set below; the grid must not rearrange them.
			grid.arrangement = UIGrid.Arrangement.CellSnap;
			grid.cellWidth = 0f;
			grid.cellHeight = 0f;
			grid.pivot = UIWidget.Pivot.TopLeft;
			grid.animateSmoothly = false;
			grid.hideInactive = true;
		}
		bool syncing = false;
		Action refresh = delegate
		{
			syncing = true;
			int num = (Shown ? 0 : 1);
			for (int k = 0; k < toggles.Length; k++)
			{
				toggles[k].startsActive = k == num;
				toggles[k].Set(k == num);
			}
			syncing = false;
		};
		for (int j = 0; j < toggles.Length; j++)
		{
			UIToggle toggle = toggles[j];
			int choice = j;
			toggle.gameObject.SetActive(value: true);
			UILabel label = toggle.GetComponentInChildren<UILabel>(includeInactive: true);
			if (label != null)
			{
				label.text = SprkText.Get(settingsChoices[j]);
				label.width = 180;
				label.overflowMethod = UILabel.Overflow.ShrinkContent;
			}
			// Same placement as the other two- and three-choice SPRK rows.
			Vector3 localPosition = toggle.transform.localPosition;
			localPosition.x = 420f + ((float)j - (float)(toggles.Length - 1) * 0.5f) * 280f;
			toggle.transform.localPosition = localPosition;
			EventDelegate.Add(toggle.onChange, delegate
			{
				if (!syncing)
				{
					if (toggle.value)
					{
						Shown = choice == 0;
					}
					refresh();
				}
			});
		}
		refresh();
		if (created)
		{
			tab.GameOptionGrid.RepositionGrid();
		}
		return row;
	}
}
