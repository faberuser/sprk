using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NDispatchBattle;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NPunish;
using NGame2.NUtil.NWrappedInfo;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

/// <summary>
/// Karma Dungeon (Divine Punishment) conveniences: dispatch, auto repeat and a
/// fixed party row. The server owns the economy: a dispatch's first run uses the
/// paid opening, every further run pays the opening cost, and runs that fill no
/// shard return theirs (karma.rs start_dispatch / settle_dispatch).
/// </summary>
public static class SprkKarmaRaid
{
	/// <summary>Remembers where the karma card's buttons were before the dispatch layout.</summary>
	private sealed class SelectLayout : MonoBehaviour
	{
		public UIButton Dispatch;

		public UILabel[] Captions;

		/// <summary>
		/// The karma group the window shows now. The window is reused for every karma
		/// dungeon while the button is made once, so its click reads this, not a capture.
		/// </summary>
		public PunishmentGroupData Karma;

		public readonly Dictionary<UIWidget, KeyValuePair<Vector3, int>> Originals = new Dictionary<UIWidget, KeyValuePair<Vector3, int>>();

		// The cloned Reward List button re-applies its own caption; keep ours.
		private void LateUpdate()
		{
			if (Dispatch == null || Captions == null || !Dispatch.gameObject.activeInHierarchy)
			{
				return;
			}
			string text = SprkText.Get("Dispatch");
			for (int i = 0; i < Captions.Length; i++)
			{
				if (Captions[i] != null && Captions[i].text != text)
				{
					Captions[i].text = text;
				}
			}
		}
	}

	/// <summary>
	/// Battle-top "Stop Auto Repeat" button, like the native Auto Repeat window of other raids:
	/// tapping it lets the current run finish and starts no further runs.
	/// </summary>
	private sealed class RepeatStopHost : MonoBehaviour
	{
		private GameObject button;

		private float nextTry;

		private void Update()
		{
			bool flag = autoRepeat && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Battle2" && IsKarmaBattle();
			if (!flag)
			{
				if (button != null)
				{
					UnityEngine.Object.Destroy(button);
					button = null;
				}
			}
			else if (button == null && Time.realtimeSinceStartup >= nextTry)
			{
				nextTry = Time.realtimeSinceStartup + 1f;
				try
				{
					button = CreateStopButton();
				}
				catch (Exception ex)
				{
					Debug.LogWarning("SPRK karma auto repeat stop button: " + ex);
				}
			}
		}
	}

	private static RepeatStopHost stopHost;

	private static bool IsKarmaBattle()
	{
		if (!SingletonAuto<BattleContext>.isValidInstance || !(SingletonAuto<BattleContext>.instance.DataProvider is PunishmentRaidContext punishmentRaidContext))
		{
			return false;
		}
		return punishmentRaidContext.PunishmentDungeonType == EPunishmentDungeonType.Karma;
	}

	private static NVespa.NPrefab2.PrefabAuto<NGame2.NUI.NWindow.NBattle.AutoRepeatBattle> stopPrefab;

	/// <summary>
	/// A private instance of the native Auto Repeat window (its own prefab, not the
	/// shared campaign instance), with Stop rewired to end the karma loop.
	/// </summary>
	private static GameObject CreateStopButton()
	{
		if (stopPrefab == null)
		{
			stopPrefab = new NVespa.NPrefab2.PrefabAuto<NGame2.NUI.NWindow.NBattle.AutoRepeatBattle>("Resources/Prefab2/Window/Battle/AutoRepeatBattle", isGlobal: true, NVespa.NPrefab2.EPrefabType.Ngui, pooling: false, touchMaterial: false);
		}
		if (stopPrefab.Factory == null || NGame2.GlobalUIRoot.instance == null)
		{
			return null;
		}
		NGame2.LayerInfo layerInfo = NGame2.GlobalUIRoot.instance.GetLayerInfo(NGame2.GlobalUIRoot.ELayerType.GlobalUI);
		NGame2.NUI.NWindow.NBattle.AutoRepeatBattle window = stopPrefab.Factory.Create(NGame2.GlobalUIRoot.instance.ContentsGroup.transform, layerInfo.Camera.gameObject.layer);
		if (window == null)
		{
			return null;
		}
		window.gameObject.name = "SprkKarmaStopAutoRepeat";
		window.Open();
		if (window.Label_Stop != null)
		{
			window.Label_Stop.text = LocalizationManager.GetStringDirect("REPEATBATTLE_STOP");
		}
		if (window.StopButton != null)
		{
			NGame2.NUI.UIHelper.EnableButton(window.StopButton, enable: true);
			window.StopButton.onClick = new List<EventDelegate>
			{
				new EventDelegate(delegate
				{
					autoRepeat = false;
					CenterMessage.Open(SprkText.Get("Auto Repeat will stop after this battle."));
				})
			};
		}
		return window.gameObject;
	}

	/// <summary>Keeps server-side KarmaDispatchMaxRepeat in step.</summary>
	public const int MaxDispatchRepeat = 10;

	private const float RepeatDelaySeconds = 5f;

	private static PunishmentGroupData pendingDispatch;

	private static bool autoRepeat;

	private static AccountManager Account()
	{
		return GlobalAutoBehaviour<AccountManager>.instance;
	}

	public static PunishmentGroupData GroupForRaid(RaidData raid)
	{
		if (raid == null || raid.Type != RaidType.KarmaRaid)
		{
			return null;
		}
		PunishmentGroupData karmaBossPunishmentData = GameRoot.Instance.PunishmentGroupDataContainer.GetKarmaBossPunishmentData(raid.Index);
		if (karmaBossPunishmentData != null)
		{
			return GameRoot.Instance.PunishmentGroupDataContainer.GetData(karmaBossPunishmentData.GroupIndex, EPunishmentDungeonType.Karma);
		}
		return null;
	}

	public static RaidData RaidFor(PunishmentGroupData group)
	{
		if (group != null)
		{
			return GameRoot.Instance.RaidDataContainer.GetData(group.KarmaRaidIndex, 1);
		}
		return null;
	}

	public static int ClampRepeat(PunishmentGroupData group, int value)
	{
		if (group != null)
		{
			return Mathf.Clamp(value, 1, MaxDispatchRepeat);
		}
		return Mathf.Clamp(value, 10, 200);
	}

	public static int PartySize(RaidData raid)
	{
		return System.Math.Max(1, raid?.MainPartyCount ?? 1);
	}

	public static int DispatchCost(PunishmentGroupData group, int repeat)
	{
		// Today's paid, unused opening covers the first run (karma.rs start_dispatch).
		return group.OpenStaminaCount * System.Math.Max(0, repeat - (IsOpen(group) ? 1 : 0));
	}

	public static bool IsOpen(PunishmentGroupData group)
	{
		return group != null && Account().OpenPunishmentRaidManager != null && Account().OpenPunishmentRaidManager.IsOpenPunishmentRaid(group.GroupIndex, EPunishmentDungeonType.Karma);
	}

	public static WCL_DispatchBattleInfo ActiveDispatch(PunishmentGroupData group)
	{
		UserDispatchBattleInfoManager dispatchBattleInfoManager = Account().DispatchBattleInfoManager;
		if (group == null || dispatchBattleInfoManager == null)
		{
			return null;
		}
		WCL_DispatchBattleInfo[] values = dispatchBattleInfoManager.GetValues();
		for (int i = 0; i < values.Length; i++)
		{
			if (values[i] != null && values[i].ChapterIndex == group.ChapterIndex && values[i].DungeonIndex == 1)
			{
				return values[i];
			}
		}
		return null;
	}

	/// <summary>The empty shard last chosen on the karma party screen, when still owned; 0 asks for the default shard.</summary>
	public static int LastShard(PunishmentGroupData group)
	{
		RaidData raidData = RaidFor(group);
		if (raidData == null)
		{
			return 0;
		}
		int value = new LocalAccountInt(0, $"ShardItemIndex_{raidData.Index}").Value;
		ItemInfo itemInfo = ((value == 0) ? null : Account().ItemManager.Get(value));
		if (itemInfo == null || itemInfo.Count <= 0)
		{
			return 0;
		}
		return value;
	}

	public static void PrepareDispatch(PunishmentGroupData group)
	{
		pendingDispatch = group;
	}

	public static void DispatchRequested(PunishmentGroupData group, bool started)
	{
		pendingDispatch = null;
	}

	/// <summary>Adds the karma shard choice to dispatch/start_dispatch.</summary>
	public static void EncodeStartDispatch(Dictionary<string, object> json, int chapterIndex, int dungeonIndex)
	{
		PunishmentGroupData punishmentGroupData = pendingDispatch;
		if (json != null && punishmentGroupData != null && punishmentGroupData.ChapterIndex == chapterIndex && dungeonIndex == 1)
		{
			json["FlaskItemIndex"] = LastShard(punishmentGroupData).ToString();
		}
	}

	public static void OpenDispatch(PunishmentGroupData group, int deckNumber = 1)
	{
		RaidData raidData = RaidFor(group);
		if (raidData != null)
		{
			ContentUIDispatchBattle.OpenGlobalRaid(raidData, deckNumber);
		}
	}

	/// <summary>
	/// The karma group a dispatch runs in (see <see cref="ActiveDispatch"/>), or null.
	/// Karma dungeons have no DispatchType in the dungeon table, so the lobby's
	/// on-going dispatch list cannot tell them apart by type.
	/// </summary>
	public static PunishmentGroupData GroupForDispatch(WCL_DispatchBattleInfo info)
	{
		if (info == null || info.DungeonIndex != 1)
		{
			return null;
		}
		foreach (PunishmentGroupData item in GameRoot.Instance.PunishmentGroupDataContainer.GetAllData())
		{
			if (item != null && item.PunishmentDungeonType == EPunishmentDungeonType.Karma && item.ChapterIndex == info.ChapterIndex)
			{
				return item;
			}
		}
		return null;
	}

	/// <summary>
	/// Gives the karma card the Boss card's two-button row: a Dispatch button where
	/// Boss shows Reward List, and Enter/Unlock narrowed to Boss's Enter.
	/// </summary>
	public static void LayoutSelectWindow(PunishDungeonTypeSelectWindow window, PunishmentGroupData karma)
	{
		try
		{
			LayoutSelectWindowUnsafe(window, karma);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SPRK karma dispatch button: " + ex);
		}
	}

	private static void LayoutSelectWindowUnsafe(PunishDungeonTypeSelectWindow window, PunishmentGroupData karma)
	{
		if (window == null || karma == null || window.KarmaEnterButton == null || window.BossEnterButton == null)
		{
			return;
		}
		UIWidget component = window.KarmaEnterButton.GetComponent<UIWidget>();
		UIWidget uIWidget = ((window.KarmaOpenButton != null) ? window.KarmaOpenButton.GetComponent<UIWidget>() : null);
		UIWidget component2 = window.BossEnterButton.GetComponent<UIWidget>();
		UIButton uIButton = FindButton(window.transform, "OnClickBossRewardPopup");
		UIWidget uIWidget2 = ((uIButton != null) ? uIButton.GetComponent<UIWidget>() : null);
		if (component == null || component2 == null || uIWidget2 == null)
		{
			return;
		}
		SelectLayout selectLayout = window.GetComponent<SelectLayout>();
		if (selectLayout == null)
		{
			selectLayout = window.gameObject.AddComponent<SelectLayout>();
			Remember(selectLayout, component);
			Remember(selectLayout, uIWidget);
		}
		selectLayout.Karma = karma;
		// Dispatch needs no opening: it pays per run (see DispatchCost).
		Vector3 key = selectLayout.Originals[component].Key;
		Vector3[] worldCorners = uIWidget2.worldCorners;
		Vector3[] worldCorners2 = component2.worldCorners;
		Vector3 vector = component.transform.parent.TransformPoint(key);
		Vector3 vector2 = new Vector3(vector.x - (worldCorners[0].x + worldCorners2[2].x) * 0.5f, vector.y - component2.transform.position.y, 0f);
		Place(selectLayout, component, component2.transform.position + vector2 - vector, component2.width);
		Place(selectLayout, uIWidget, component2.transform.position + vector2 - vector, component2.width);
		if (selectLayout.Dispatch == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(uIButton.gameObject, component.transform.parent, worldPositionStays: true);
			gameObject.name = "SprkKarmaDispatch";
			gameObject.transform.localScale = uIButton.transform.localScale;
			UIWidget component3 = gameObject.GetComponent<UIWidget>();
			ClearAnchors(component3);
			selectLayout.Dispatch = gameObject.GetComponent<UIButton>();
			SelectLayout layout = selectLayout;
			selectLayout.Dispatch.onClick = new List<EventDelegate>
			{
				new EventDelegate(delegate
				{
					OpenDispatch(layout.Karma);
				})
			};
			UILocalize[] componentsInChildren = gameObject.GetComponentsInChildren<UILocalize>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				UnityEngine.Object.DestroyImmediate(componentsInChildren[i]);
			}
			selectLayout.Captions = gameObject.GetComponentsInChildren<UILabel>(includeInactive: true);
		}
		selectLayout.Dispatch.transform.position = uIButton.transform.position + vector2;
		selectLayout.Dispatch.gameObject.SetActive(value: true);
	}

	private static UIButton FindButton(Transform root, string method)
	{
		UIButton[] componentsInChildren = root.GetComponentsInChildren<UIButton>(includeInactive: true);
		foreach (UIButton uIButton in componentsInChildren)
		{
			if (uIButton.onClick == null)
			{
				continue;
			}
			foreach (EventDelegate item in uIButton.onClick)
			{
				if (item != null && item.methodName == method)
				{
					return uIButton;
				}
			}
		}
		return null;
	}

	private static void Remember(SelectLayout layout, UIWidget widget)
	{
		if (widget != null && !layout.Originals.ContainsKey(widget))
		{
			layout.Originals[widget] = new KeyValuePair<Vector3, int>(widget.transform.localPosition, widget.width);
		}
	}

	/// <summary>Moves a widget by the same world offset as its remembered position moved.</summary>
	private static void Place(SelectLayout layout, UIWidget widget, Vector3 offset, int width)
	{
		if (!(widget == null) && layout.Originals.TryGetValue(widget, out var value))
		{
			ClearAnchors(widget);
			widget.transform.position = widget.transform.parent.TransformPoint(value.Key) + offset;
			widget.width = width;
		}
	}

	private static void ClearAnchors(UIRect rect)
	{
		if (!(rect == null))
		{
			rect.leftAnchor.target = null;
			rect.rightAnchor.target = null;
			rect.bottomAnchor.target = null;
			rect.topAnchor.target = null;
			rect.ResetAnchors();
		}
	}

	/// <summary>
	/// Karma party screen: freezes the party row and shows Auto Repeat between the
	/// shard selection and the Raid Info / Start Battle row.
	/// </summary>
	public static void SetupPartyScreen(PunishmentRaidPartySetting setting, bool karma)
	{
		try
		{
			FreezeParty(setting, karma);
			if (setting.Widget_Repeat == null && karma && !BorrowRepeatWidget(setting))
			{
				return;
			}
			UIWidget widget_Repeat = setting.Widget_Repeat;
			if (widget_Repeat == null)
			{
				return;
			}
			if (!karma)
			{
				widget_Repeat.gameObject.SetActive(value: false);
				return;
			}
			widget_Repeat.gameObject.SetActive(value: true);
			Transform transform = setting.transform.Find("Panel/Root/Widget_ShardInfo");
			Transform transform2 = setting.transform.Find("Panel/Root/Widget_BattleInfo/Button_RaidInfo");
			if (transform != null && transform2 != null)
			{
				// Centre the checkbox in the gap between the shard section and the battle row.
				ClearAnchors(widget_Repeat);
				// Measure the visible shard row (slot icon and button); the section's own
				// bounds include an invisible widget that reaches lower.
				Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(transform);
				Transform transform3 = transform.Find("ShardSlot");
				Transform transform4 = transform.Find("Button_SelectShard");
				if (transform3 != null && transform4 != null)
				{
					bounds = NGUIMath.CalculateAbsoluteWidgetBounds(transform3);
					bounds.Encapsulate(NGUIMath.CalculateAbsoluteWidgetBounds(transform4));
				}
				float num = NGUIMath.CalculateAbsoluteWidgetBounds(transform2).max.y;
				if (setting.StartButton != null)
				{
					num = Mathf.Max(num, NGUIMath.CalculateAbsoluteWidgetBounds(setting.StartButton.transform).max.y);
				}
				// No bar behind the checkbox: hide wide, flat sprites (keep box, tick and label).
				UISprite[] componentsInChildren = widget_Repeat.GetComponentsInChildren<UISprite>(includeInactive: true);
				foreach (UISprite uISprite in componentsInChildren)
				{
					if (uISprite != setting.SpriteBattleRepeatChecked && uISprite != setting.SpriteBattleRepeatUnusable && uISprite.width > uISprite.height * 3)
					{
						uISprite.enabled = false;
					}
				}
				Bounds bounds2 = VisibleBounds(widget_Repeat);
				float x = bounds.center.x;
				float num2 = (bounds.min.y + num) * 0.5f;
				if (transform4 != null && setting.StartButton != null)
				{
					// Between Select Empty Shard and Start Battle, centred on that column.
					Bounds bounds3 = SpriteBounds(transform4, square: false);
					Bounds bounds4 = SpriteBounds(setting.StartButton.transform.parent ?? setting.StartButton.transform, square: false);
					// Box's left edge on Select Empty Shard's left edge; centred in the gap
					// between that button and Start Battle's frame.
					// The button art has transparent margins inside its sprite: about a fifth
					// of the height on the left, 15% below Select and 9% above Start.
					x = bounds3.min.x + bounds3.size.y * 0.2f;
					num2 = (bounds3.min.y + bounds3.size.y * 0.15f + (bounds4.max.y - bounds4.size.y * 0.09f)) * 0.5f;
				}
				else
				{
					x = bounds.min.x;
				}
				Bounds bounds5 = SpriteBounds(widget_Repeat.transform, square: true, setting.SpriteBattleRepeatChecked, setting.SpriteBattleRepeatUnusable);
				if (bounds5.size.x <= 0f)
				{
					bounds5 = bounds2;
				}
				widget_Repeat.transform.position += new Vector3(x - bounds5.min.x, num2 - bounds5.center.y, 0f);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SPRK karma party screen: " + ex);
		}
	}

	// The squad row sits in a drag-scrolled panel; karma heroes keep their slots.
	private static void FreezeParty(PunishmentRaidPartySetting setting, bool karma)
	{
		Transform transform = setting.transform.Find("Panel/Root/Widget_SelectHeroList/Squard");
		if (transform == null)
		{
			return;
		}
		HashSet<UIScrollView> hashSet = new HashSet<UIScrollView>(transform.GetComponentsInChildren<UIScrollView>(includeInactive: true));
		UIDragScrollView[] componentsInChildren = transform.GetComponentsInChildren<UIDragScrollView>(includeInactive: true);
		foreach (UIDragScrollView uIDragScrollView in componentsInChildren)
		{
			uIDragScrollView.enabled = !karma;
			if (uIDragScrollView.scrollView != null)
			{
				hashSet.Add(uIDragScrollView.scrollView);
			}
		}
		foreach (UIScrollView item in hashSet)
		{
			if (karma)
			{
				item.ResetPosition();
			}
			item.enabled = !karma;
		}
	}

	// The largest drawn sprite under root: a button's body/frame, or (square) a checkbox's box.
	private static Bounds SpriteBounds(Transform root, bool square, params UIWidget[] skip)
	{
		UISprite uISprite = null;
		float num = 0f;
		UISprite[] componentsInChildren = root.GetComponentsInChildren<UISprite>(includeInactive: false);
		foreach (UISprite uISprite2 in componentsInChildren)
		{
			if (uISprite2.enabled && uISprite2.finalAlpha > 0.05f && uISprite2.width > 2 && uISprite2.height > 2 && Array.IndexOf(skip, uISprite2) < 0 && (!square || Mathf.Abs(uISprite2.width - uISprite2.height) <= uISprite2.height / 5))
			{
				float num2 = (float)uISprite2.width * (float)uISprite2.height;
				if (num2 > num)
				{
					num = num2;
					uISprite = uISprite2;
				}
			}
		}
		if (uISprite == null)
		{
			return default(Bounds);
		}
		// drawingDimensions excludes the atlas sprite's transparent padding.
		Vector4 drawingDimensions = uISprite.drawingDimensions;
		Transform transform = uISprite.transform;
		Bounds result = new Bounds(transform.TransformPoint(new Vector3(drawingDimensions.x, drawingDimensions.y, 0f)), Vector3.zero);
		result.Encapsulate(transform.TransformPoint(new Vector3(drawingDimensions.z, drawingDimensions.w, 0f)));
		return result;
	}

	// Bounds of the drawn parts only (disabled widgets still count in NGUIMath bounds).
	private static Bounds VisibleBounds(Component root)
	{
		bool flag = false;
		Bounds result = default(Bounds);
		UIWidget[] componentsInChildren = root.GetComponentsInChildren<UIWidget>(includeInactive: false);
		foreach (UIWidget uIWidget in componentsInChildren)
		{
			if (!uIWidget.enabled || uIWidget.finalAlpha <= 0.05f || uIWidget.width <= 2 || (!(uIWidget is UISprite) && !(uIWidget is UILabel)))
			{
				continue;
			}
			Vector3[] worldCorners = uIWidget.worldCorners;
			for (int j = 0; j < worldCorners.Length; j++)
			{
				if (!flag)
				{
					result = new Bounds(worldCorners[j], Vector3.zero);
					flag = true;
				}
				else
				{
					result.Encapsulate(worldCorners[j]);
				}
			}
		}
		if (!flag)
		{
			return NGUIMath.CalculateAbsoluteWidgetBounds(root.transform);
		}
		return result;
	}

	/// <summary>
	/// The karma prefab has no Auto Repeat checkbox; copy the dragon/technomagic one
	/// (RaidSinglePartySetting.Widget_Repeat) so it looks and behaves the same.
	/// </summary>
	private static bool BorrowRepeatWidget(PunishmentRaidPartySetting setting)
	{
		NGame2.NUI.NManager.NLobby.RaidManagement raidManagement = SceneAutoBehaviour<NGame2.NUI.NManager.NLobby.RaidManagement>.instance;
		if (raidManagement == null)
		{
			return false;
		}
		RaidSinglePartySetting raidSinglePartySetting = raidManagement.GetPrefab<RaidSinglePartySetting>();
		if (raidSinglePartySetting == null || raidSinglePartySetting.Widget_Repeat == null)
		{
			raidSinglePartySetting = raidManagement.GetInstance<RaidSinglePartySetting>();
		}
		if (raidSinglePartySetting == null || raidSinglePartySetting.Widget_Repeat == null)
		{
			return false;
		}
		Transform transform = raidSinglePartySetting.Widget_Repeat.transform;
		Transform transform2 = setting.transform.Find("Panel/Root/Widget_BattleInfo") ?? setting.transform;
		GameObject gameObject = UnityEngine.Object.Instantiate(transform.gameObject, transform2, worldPositionStays: false);
		gameObject.name = "SprkKarmaAutoRepeat";
		NGUITools.SetLayer(gameObject, transform2.gameObject.layer);
		// Anchors in the dragon prefab target objects outside this copy.
		UIRect[] componentsInChildren2 = gameObject.GetComponentsInChildren<UIRect>(includeInactive: true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			ClearAnchors(componentsInChildren2[j]);
		}
		setting.Widget_Repeat = gameObject.GetComponent<UIWidget>();
		setting.SpriteBattleRepeatChecked = Mirror<UISprite>(transform, gameObject.transform, raidSinglePartySetting.SpriteBattleRepeatChecked);
		setting.SpriteBattleRepeatUnusable = Mirror<UISprite>(transform, gameObject.transform, raidSinglePartySetting.SpriteBattleRepeatUnusable);
		UIButton[] componentsInChildren = gameObject.GetComponentsInChildren<UIButton>(includeInactive: true);
		foreach (UIButton uIButton in componentsInChildren)
		{
			uIButton.onClick = new List<EventDelegate>
			{
				new EventDelegate(setting.ToggleBattleRepeat)
			};
		}
		if (componentsInChildren.Length == 0)
		{
			if (gameObject.GetComponent<Collider>() == null)
			{
				NGUITools.AddWidgetCollider(gameObject);
			}
			UIEventListener.Get(gameObject).onClick = delegate
			{
				setting.ToggleBattleRepeat();
			};
		}
		return setting.Widget_Repeat != null;
	}

	/// <summary>The copy of <paramref name="target"/> (a child of <paramref name="source"/>) under <paramref name="copy"/>.</summary>
	private static T Mirror<T>(Transform source, Transform copy, Component target) where T : Component
	{
		if (target == null)
		{
			return null;
		}
		List<string> list = new List<string>();
		Transform transform = target.transform;
		while (transform != null && transform != source)
		{
			list.Insert(0, transform.name);
			transform = transform.parent;
		}
		if (transform != source)
		{
			return null;
		}
		Transform transform2 = ((list.Count == 0) ? copy : copy.Find(string.Join("/", list.ToArray())));
		if (!(transform2 != null))
		{
			return null;
		}
		return transform2.GetComponent<T>();
	}

	public static void ConfirmAutoRepeat(PunishmentGroupData group, Action onYes)
	{
		MessagePopup.Open(SprkText.Get("Auto Repeat"), SprkText.Format("Auto Repeat starts the next run automatically. When a run fills a shard, the Karma Dungeon closes and is re-opened for {0} Stamina. Continue?", (group == null) ? 0 : group.OpenStaminaCount), onYes, null);
	}

	public static void ArmAutoRepeat(bool enabled)
	{
		autoRepeat = enabled;
		if (enabled && stopHost == null)
		{
			GameObject gameObject = new GameObject("SprkKarmaAutoRepeat");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			stopHost = gameObject.AddComponent<RepeatStopHost>();
		}
	}

	public static void StopAutoRepeat()
	{
		autoRepeat = false;
	}

	/// <summary>Called after a karma end_content response was applied.</summary>
	public static void ContentEnded(NShared.EndContent.Request request, bool success)
	{
		if (!autoRepeat || request == null || request.DungeonType != EPunishmentDungeonType.Karma)
		{
			return;
		}
		if (!success)
		{
			autoRepeat = false;
			return;
		}
		CenterMessage.Open(SprkText.Format("Auto Repeat: next run in {0}s. Return to the lobby to stop.", (int)RepeatDelaySeconds));
		VespaCoroutine.Start(ContinueRepeat());
	}

	private static IEnumerator ContinueRepeat()
	{
		float until = Time.realtimeSinceStartup + RepeatDelaySeconds;
		while (Time.realtimeSinceStartup < until)
		{
			if (!autoRepeat)
			{
				yield break;
			}
			yield return null;
		}
		if (!autoRepeat)
		{
			yield break;
		}
		PunishmentRaidContext context = SingletonAuto<PunishmentRaidContext>.instance;
		PunishmentGroupData group = GameRoot.Instance.PunishmentGroupDataContainer.GetData(context.GroupIndex, EPunishmentDungeonType.Karma);
		if (group == null)
		{
			autoRepeat = false;
			yield break;
		}
		int defaultFlaskIndex = GameRoot.Instance.ShardFlaskItemDataContainer.GetDefaultFlaskIndex(group.GroupIndex);
		int flaskItemIndex = context.ExtraData.FlaskItemIndex;
		ItemInfo itemInfo = ((flaskItemIndex == 0) ? null : Account().ItemManager.Get(flaskItemIndex));
		if (flaskItemIndex != defaultFlaskIndex && (itemInfo == null || itemInfo.Count <= 0))
		{
			context.ExtraData.FlaskItemIndex = defaultFlaskIndex;
		}
		if (IsOpen(group))
		{
			Repeat();
			yield break;
		}
		int openStamina = group.OpenStaminaCount;
		if (Account().UserManager.StaminaManager.GetCurrentStamina(StaminaType.Chicken) < openStamina)
		{
			autoRepeat = false;
			MessagePopup.Open(SprkText.Get("Auto Repeat"), SprkText.Get("Not enough Stamina to re-open the Karma Dungeon. Auto Repeat has stopped."));
			yield break;
		}
		NShared.OpenPunishmentRaid.Request request = new NShared.OpenPunishmentRaid.Request
		{
			GroupIndex = group.GroupIndex,
			DungeonType = 1
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.OpenPunishmentRaid.Response response)
		{
			if (response == null || response.BaseResult != BaseResultType.Success || response.Result != NShared.OpenPunishmentRaid.ResultType.Success)
			{
				autoRepeat = false;
				if (response != null && response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response != null)
				{
					MessagePopup.OpenError(response.Result);
				}
			}
			else
			{
				Account().UserManager.Apply(response.StaminaResult);
				Account().OpenPunishmentRaidManager.Set(response.PunishmentRaidInfos);
				Account().OpenPunishmentRaidManager.Apply(response.OpenPunishmentRaidInfo);
				if (autoRepeat)
				{
					Repeat();
				}
			}
		});
	}

	private static void Repeat()
	{
		if (SceneAutoBehaviour<PunishEndContentManagement>.isValidInstance)
		{
			SceneAutoBehaviour<PunishEndContentManagement>.instance.BattleRepeat(new BattlePartySetting.ExtraOptionData
			{
				AutoRepeat = true,
				IsEndCampaign = true
			});
		}
		else
		{
			autoRepeat = false;
		}
	}
}
