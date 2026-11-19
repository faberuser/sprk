using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

/// <summary>
/// Auto-battle skill rules. In Hero Info → Skills every active skill gets a button that opens a
/// chooser: ★1 / ★2 / ★3 (priority rank), Normal, or Never use. Automatic skill selection never
/// uses "Never use" skills and casts the best-ranked ready skill before any other. Stored per
/// account and hero, locally and (for the server's battle worker) on the server.
///
/// Where the rules reach the skill choice:
/// - heroes with on-screen skill buttons on Auto: BattleManagement reads them directly;
/// - AI-driven heroes in offline battles: frozen in <see cref="BattleCreated"/>;
/// - background simulations: marked on the create info (<see cref="Mark"/>);
/// - online battles and dispatch: the server's worker marks the create infos from the stored
///   rules, and the battle logs carry them to every client in the battle.
/// </summary>
public sealed class SprkSkillPriority : MonoBehaviour
{
	/// <summary>Rule value of a skill that auto battle must never use.</summary>
	public const int Off = -1;

	private const int MaxSlot = 5;

	private const int OffBits = 7;

	private const string KeyPrefix = "Sprk.SkillAuto.";

	private static readonly int[] Choices = new int[5] { 1, 2, 3, 0, Off };

	private static readonly Color RankedButton = Color.white;

	private static readonly Color NormalButton = new Color(0.42f, 0.42f, 0.42f, 1f);

	private static readonly Color OffButton = new Color(0.55f, 0.22f, 0.22f, 1f);

	private static readonly Color RankedText = new Color(1f, 0.92f, 0.45f, 1f);

	private static readonly Color NormalText = new Color(0.82f, 0.82f, 0.82f, 1f);

	private static readonly Color OffText = new Color(1f, 0.62f, 0.58f, 1f);

	private static readonly Color Backdrop = new Color(0.1f, 0.1f, 0.13f, 0.94f);

	private static readonly Color ChoiceIdle = new Color(0.5f, 0.5f, 0.5f, 1f);

	// Main thread only: PlayerPrefs may not be read from the battle worker threads.
	private static readonly Dictionary<string, int> Cache = new Dictionary<string, int>();

	// Native battle worker only (see MarkFromPayload); keyed "accountId:heroIndex".
	private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, int> WorkerRules = new System.Collections.Concurrent.ConcurrentDictionary<string, int>();

	private sealed class LiveSnapshot
	{
		public long AccountId;

		public Dictionary<int, int> Rules;
	}

	// Offline battles on this client (see BattleCreated); never modified after it is published.
	private static volatile LiveSnapshot live;

	private static readonly string TracePath = TraceFile();

	private static readonly object TraceLock = new object();

	private static SprkSkillPriority openChooser;

	private HeroSkillSubComponent owner;

	private Transform template;

	private int baseDepth;

	private GameObject root;

	private UIButton button;

	private UILabel label;

	private GameObject chooser;

	private readonly List<UIButton> choiceButtons = new List<UIButton>();

	private int heroIndex;

	private int slot;

	// ---- Rules ---------------------------------------------------------------------------------

	/// <summary>0 = Normal, 1-3 = priority rank, <see cref="Off"/> = never used by auto battle.</summary>
	public static int Rule(int rules, int slot)
	{
		if (rules == 0 || slot < 1 || slot > MaxSlot)
		{
			return 0;
		}
		int num = (rules >> (slot - 1) * 3) & 7;
		if (num == OffBits)
		{
			return Off;
		}
		if (num < 1 || num > 3)
		{
			return 0;
		}
		return num;
	}

	private static int WithRule(int rules, int slot, int rule)
	{
		int num = (slot - 1) * 3;
		int num2 = ((rule == Off) ? OffBits : ((rule >= 1 && rule <= 3) ? rule : 0));
		return (rules & ~(7 << num)) | (num2 << num);
	}

	public static bool IsOff(int rules, int slot)
	{
		return Rule(rules, slot) == Off;
	}

	/// <summary>Lower is better; skills without a rank (Normal, soul weapon) sort last.</summary>
	public static int Rank(int rules, int slot)
	{
		int num = Rule(rules, slot);
		if (num <= 0)
		{
			return int.MaxValue;
		}
		return num;
	}

	public static int SlotOfSkill(CreatureData creatureData, int skillIndex)
	{
		if (creatureData == null || skillIndex == 0)
		{
			return 0;
		}
		for (int i = 1; i <= MaxSlot; i++)
		{
			if (creatureData.GetSkillIndexBySlot(i) == skillIndex)
			{
				return i;
			}
		}
		return 0;
	}

	public static int Rules(HeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return 0;
		}
		return Rules(heroInfo.HeroIndex);
	}

	public static int Rules(BattleCreatureCreateInfo createInfo)
	{
		return createInfo?.SprkSkillRules ?? 0;
	}

	public static void Mark(BattleCreatureCreateInfo createInfo, int rules)
	{
		if (createInfo != null && rules != 0)
		{
			createInfo.SprkSkillRules = rules;
		}
	}

	/// <summary>Reads the rules of the given heroes on the main thread for a background simulation.</summary>
	public static Dictionary<int, int> Capture(int[] heroIndices)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		if (heroIndices == null)
		{
			return dictionary;
		}
		try
		{
			foreach (int num in heroIndices)
			{
				int num2 = Rules(num);
				if (num2 != 0)
				{
					dictionary[num] = num2;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SPRK skill priority: " + ex.Message);
		}
		return dictionary;
	}

	/// <summary>
	/// Adds the party's rules to dispatch/start_dispatch as "SkillRules" ({"heroIndex": rules}).
	/// The server freezes them on the job and hands them to the native dispatch worker.
	/// </summary>
	public static void EncodeDispatch(Dictionary<string, object> json, int[] heroIndices)
	{
		if (json == null || heroIndices == null)
		{
			return;
		}
		Dictionary<int, int> dictionary = Capture(heroIndices);
		if (dictionary.Count == 0)
		{
			return;
		}
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		foreach (KeyValuePair<int, int> item in dictionary)
		{
			dictionary2[item.Key.ToString()] = item.Value;
		}
		json["SkillRules"] = MiniJSON.Json.Serialize(dictionary2);
	}

	/// <summary>
	/// The rules the server stored for one account of a worker run: the account info's
	/// "SkillRules" in the run's "Simulation", or for dispatch the job's frozen top-level "SkillRules".
	/// </summary>
	public static object PayloadRules(System.Collections.IDictionary data, long accountId, bool dispatch)
	{
		if (data == null)
		{
			return null;
		}
		if (dispatch && data.Contains("SkillRules"))
		{
			return data["SkillRules"];
		}
		System.Collections.IList list = ((data["Simulation"] as System.Collections.IDictionary)?["AccountInfos"]) as System.Collections.IList;
		if (list == null)
		{
			return null;
		}
		foreach (object item in list)
		{
			System.Collections.IDictionary dictionary = item as System.Collections.IDictionary;
			System.Collections.IDictionary dictionary2 = dictionary?["UserInfo"] as System.Collections.IDictionary;
			if (dictionary2 != null && dictionary.Contains("SkillRules") && dictionary2.Contains("AccountId") && Convert.ToInt64(dictionary2["AccountId"]) == accountId)
			{
				return dictionary["SkillRules"];
			}
		}
		return null;
	}

	/// <summary>
	/// Marks a worker-side hero with the rules the server forwarded (see <see cref="PayloadRules"/>).
	/// The mark is serialized with the battle logs, so every client simulates the same choices. The
	/// rules are also kept per account and hero, in case the battle rebuilds the create info.
	/// </summary>
	public static void MarkFromPayload(object skillRules, long accountId, BattleCreatureCreateInfo createInfo)
	{
		if (createInfo == null || createInfo.HeroInfo == null)
		{
			return;
		}
		int heroIndex = createInfo.HeroInfo.HeroIndex;
		string text = WorkerKey(accountId, heroIndex);
		try
		{
			System.Collections.IDictionary dictionary = (skillRules == null) ? null : ((skillRules as System.Collections.IDictionary) ?? (MiniJSON.Json.Deserialize(skillRules.ToString()) as System.Collections.IDictionary));
			string key = heroIndex.ToString();
			int num = ((dictionary != null && dictionary.Contains(key)) ? Convert.ToInt32(dictionary[key]) : 0);
			if (num == 0)
			{
				// The worker is long-lived: forget rules the player has since cleared.
				WorkerRules.TryRemove(text, out var _);
				return;
			}
			Mark(createInfo, num);
			WorkerRules[text] = num;
			Trace("worker-mark", heroIndex, num, "", 0);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SPRK skill priority rules: " + ex.Message);
		}
	}

	/// <summary>
	/// Called on the main thread by BattleFacade.CreateBattle. Offline battles simulate on the battle
	/// thread, where PlayerPrefs cannot be read, and AI-driven heroes (sub team, hero sets, any hero
	/// without on-screen skill buttons) pick skills inside the simulation. So the logged-in account's
	/// rules are frozen here for the whole battle, including the integrity re-run of its logs.
	/// Online battles take their rules only from the worker's logs (create infos marked by the
	/// server's worker): local rules there could differ from what the worker used, and the
	/// client's simulation would diverge.
	/// </summary>
	public static void BattleCreated(bool isOnlineMode)
	{
		live = null;
		if (isOnlineMode)
		{
			return;
		}
		try
		{
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				return;
			}
			AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
			UserInfo userInfo = instance.UserInfo;
			if (userInfo == null || userInfo.AccountId <= 0 || instance.HeroManager == null)
			{
				return;
			}
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			foreach (CachedHeroInfo value in instance.HeroManager.GetValues())
			{
				if (value != null)
				{
					int num = Rules(value.HeroIndex);
					if (num != 0)
					{
						dictionary[value.HeroIndex] = num;
					}
				}
			}
			if (dictionary.Count > 0)
			{
				live = new LiveSnapshot
				{
					AccountId = userInfo.AccountId,
					Rules = dictionary
				};
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SPRK skill priority: " + ex.Message);
		}
	}

	/// <summary>
	/// Rules of a battle creature: its marked create info (background simulations, and online
	/// battles via the worker's logs), else the native worker's account/hero rules, else the
	/// current offline battle's rules for the logged-in account's own heroes.
	/// </summary>
	public static int Rules(BattleCreature creature)
	{
		if (creature == null)
		{
			return 0;
		}
		int num = Rules(creature.CreateInfo);
		if (num != 0 || creature.CreateInfo == null || creature.CreateInfo.HeroInfo == null)
		{
			return num;
		}
		int heroIndex = creature.CreateInfo.HeroInfo.HeroIndex;
		if (!WorkerRules.IsEmpty && WorkerRules.TryGetValue(WorkerKey(creature.PlayerId, heroIndex), out var value))
		{
			return value;
		}
		LiveSnapshot liveSnapshot = live;
		if (liveSnapshot != null && creature.PlayerId == liveSnapshot.AccountId && liveSnapshot.Rules.TryGetValue(heroIndex, out value))
		{
			return value;
		}
		return 0;
	}

	private static string WorkerKey(long accountId, int heroIndex)
	{
		return accountId + ":" + heroIndex;
	}

	public static int Rules(int heroIndex)
	{
		string text = Key(heroIndex);
		if (text == null)
		{
			return 0;
		}
		if (!Cache.TryGetValue(text, out var value))
		{
			value = SprkTestPrefs.GetInt(text, 0);
			Cache[text] = value;
		}
		return value;
	}

	private static void Save(int heroIndex, int rules)
	{
		string text = Key(heroIndex);
		if (text != null)
		{
			Cache[text] = rules;
			SprkTestPrefs.SetInt(text, rules);
			PlayerPrefs.Save();
			SyncHero(heroIndex, rules);
		}
	}

	// ---- Server copy (online battles) -----------------------------------------------------------

	// Main thread only. The account whose complete set the server has (0 = not yet, or a send failed).
	private static long syncedAccount;

	private static bool fullSyncInFlight;

	private static bool resyncAfterFullSync;

	private static bool syncWaiting;

	private static long CurrentAccount()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return 0L;
		}
		UserInfo userInfo = GlobalAutoBehaviour<AccountManager>.instance.UserInfo;
		return (userInfo == null) ? 0 : userInfo.AccountId;
	}

	/// <summary>
	/// Called when the lobby starts: once per account and session, sends the complete local set so
	/// the server's battle worker uses the same rules in online battles. Waits for the heroes to load.
	/// </summary>
	public static void SyncSoon()
	{
		if (syncWaiting)
		{
			return;
		}
		try
		{
			syncWaiting = true;
			VespaCoroutine.Start(SyncWhenReady());
		}
		catch (Exception ex)
		{
			syncWaiting = false;
			Debug.LogWarning("SPRK skill priority sync: " + ex.Message);
		}
	}

	private static System.Collections.IEnumerator SyncWhenReady()
	{
		float until = Time.realtimeSinceStartup + 60f;
		while (!TrySyncAll() && Time.realtimeSinceStartup < until)
		{
			yield return null;
		}
		syncWaiting = false;
	}

	/// <summary>False while the account or its heroes are not loaded yet.</summary>
	private static bool TrySyncAll()
	{
		try
		{
			long num = CurrentAccount();
			if (num <= 0)
			{
				return false;
			}
			if (syncedAccount == num)
			{
				return true;
			}
			if (fullSyncInFlight)
			{
				resyncAfterFullSync = true;
				return true;
			}
			AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
			CachedHeroInfo[] array = instance.HeroManager?.GetValues();
			if (array == null || array.Length == 0)
			{
				return false;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			foreach (CachedHeroInfo cachedHeroInfo in array)
			{
				if (cachedHeroInfo != null)
				{
					int num2 = Rules(cachedHeroInfo.HeroIndex);
					if (num2 != 0)
					{
						dictionary[cachedHeroInfo.HeroIndex.ToString()] = num2;
					}
				}
			}
			fullSyncInFlight = true;
			resyncAfterFullSync = false;
			Send(dictionary, replace: true, delegate(bool ok)
			{
				fullSyncInFlight = false;
				if (ok && CurrentAccount() == num)
				{
					syncedAccount = num;
				}
				// A rule changed while the full set was on its way; send the set again.
				if (resyncAfterFullSync)
				{
					resyncAfterFullSync = false;
					syncedAccount = 0L;
					TrySyncAll();
				}
			});
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SPRK skill priority sync: " + ex.Message);
			return true;
		}
	}

	private static void SyncHero(int heroIndex, int rules)
	{
		long num = CurrentAccount();
		if (num <= 0)
		{
			return;
		}
		if (syncedAccount != num || fullSyncInFlight)
		{
			// The full set read from the local cache already includes this change.
			TrySyncAll();
			return;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object> { [heroIndex.ToString()] = rules };
		Send(dictionary, replace: false, delegate(bool ok)
		{
			if (!ok && syncedAccount == num)
			{
				// Resend the complete set on the next change or lobby visit.
				syncedAccount = 0L;
			}
		});
	}

	private static void Send(Dictionary<string, object> rules, bool replace, Action<bool> done)
	{
		NShared.SprkSetSkillRules.Request request = new NShared.SprkSetSkillRules.Request
		{
			SkillRules = MiniJSON.Json.Serialize(rules),
			Replace = replace
		};
		NVespa.SingletonUpdater<NVespa.NWeb.WebServiceRequester>.instance.Request(request, NVespa.NWeb.WebRequestFlag.DisableRetry | NVespa.NWeb.WebRequestFlag.SilenceError, delegate(NShared.SprkSetSkillRules.Response response)
		{
			bool flag = response != null && response.BaseResult == BaseResultType.Success && response.Result == NShared.SprkSetSkillRules.ResultType.Success;
			if (!flag)
			{
				Debug.LogWarning("SPRK skill priority sync failed: " + ((response == null) ? "no response" : (response.BaseResult + "/" + response.Result)));
			}
			done?.Invoke(flag);
		});
	}

	private static string Key(int heroIndex)
	{
		if (heroIndex <= 0 || !GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return null;
		}
		UserInfo userInfo = GlobalAutoBehaviour<AccountManager>.instance.UserInfo;
		if (userInfo == null || userInfo.AccountId <= 0)
		{
			return null;
		}
		return KeyPrefix + userInfo.AccountId + "." + heroIndex;
	}

	/// <summary>Sets one skill's rule; a rank already held by another skill moves to this skill's old state.</summary>
	private static int Assign(int rules, int slot, int rule)
	{
		int num = Rule(rules, slot);
		if (rule >= 1 && rule <= 3)
		{
			for (int i = 1; i <= MaxSlot; i++)
			{
				if (i != slot && Rule(rules, i) == rule)
				{
					rules = WithRule(rules, i, (num >= 1 && num <= 3) ? num : 0);
				}
			}
		}
		return WithRule(rules, slot, rule);
	}

	private static string TraceFile()
	{
		try
		{
			// The native dispatch worker does not inherit SPRK_AUTOMATION_DIR, so tests can name the file directly.
			string environmentVariable = Environment.GetEnvironmentVariable("SPRK_SKILL_PRIORITY_TRACE");
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				return environmentVariable;
			}
			environmentVariable = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR");
			return string.IsNullOrEmpty(environmentVariable) ? null : System.IO.Path.Combine(environmentVariable, "skill-priority.log");
		}
		catch
		{
			return null;
		}
	}

	/// <summary>True only in automation/diagnostic runs; callers skip building trace text otherwise.</summary>
	public static bool Tracing => TracePath != null;

	/// <summary>Automation-only decision log (no-op in normal play); safe from worker threads.</summary>
	public static void Trace(string source, int heroIndex, int rules, string ready, int chosen)
	{
		if (TracePath == null || rules == 0)
		{
			return;
		}
		try
		{
			string text = "";
			for (int i = 1; i <= MaxSlot; i++)
			{
				int num = Rule(rules, i);
				if (num != 0)
				{
					text = text + ((text.Length == 0) ? "" : ",") + i + ":" + ((num == Off) ? "off" : ("#" + num));
				}
			}
			lock (TraceLock)
			{
				System.IO.File.AppendAllText(TracePath, DateTime.Now.ToString("HH:mm:ss.fff") + " " + source + " hero=" + heroIndex + " rules=[" + text + "] ready=[" + ready + "] chosen=" + chosen + Environment.NewLine);
			}
		}
		catch
		{
		}
	}

	// ---- Hero Info UI --------------------------------------------------------------------------

	/// <summary>Called at the end of HeroSkillSubComponent.Open (also on every refresh).</summary>
	public static void Attach(HeroSkillSubComponent component, bool isMine, Helper.SkillDetailInfo info)
	{
		if (component == null)
		{
			return;
		}
		try
		{
			SprkSkillPriority sprkSkillPriority = component.GetComponent<SprkSkillPriority>();
			if (!(isMine && component.HeroInfo != null && info.BasicInfo.TableData != null && info.BasicInfo.TableData.TriggerType == SkillTriggerType.None && component.SkillSlotIndex > 0 && component.SkillSlotIndex <= MaxSlot && FindChild(component.transform, "Label_SkillLevel") != null))
			{
				if (sprkSkillPriority != null)
				{
					sprkSkillPriority.CloseChooser();
					sprkSkillPriority.Show(show: false);
				}
				return;
			}
			if (sprkSkillPriority == null)
			{
				sprkSkillPriority = component.gameObject.AddComponent<SprkSkillPriority>();
				sprkSkillPriority.owner = component;
			}
			sprkSkillPriority.heroIndex = component.HeroInfo.HeroIndex;
			sprkSkillPriority.slot = component.SkillSlotIndex;
			sprkSkillPriority.CloseChooser();
			if (sprkSkillPriority.EnsureButton())
			{
				// The linked-skill switch (e.g. Isaiah 2-1/2-2) sits where the rule button goes; the
				// Skill Info popup keeps its own switch for reading the other form.
				if (component.SwitchLinkedSkillButton != null)
				{
					component.SwitchLinkedSkillButton.gameObject.SetActive(false);
				}
				sprkSkillPriority.Layout();
				sprkSkillPriority.Show(show: true);
				sprkSkillPriority.Apply();
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SPRK skill priority UI: " + ex);
		}
	}

	private bool EnsureButton()
	{
		if (button != null)
		{
			return true;
		}
		Transform parent = owner.transform;
		while (parent != null && !parent.name.StartsWith("HeroList"))
		{
			parent = parent.parent;
		}
		template = ((parent == null) ? null : FindChild(parent, "Button_AllHero"));
		if (template == null)
		{
			return false;
		}
		baseDepth = 0;
		UIWidget[] componentsInChildren = owner.GetComponentsInChildren<UIWidget>(includeInactive: true);
		foreach (UIWidget uIWidget in componentsInChildren)
		{
			baseDepth = Math.Max(baseDepth, uIWidget.depth);
		}
		root = Clone("SprkSkillPriority", baseDepth + 1, out button, out label);
		if (root == null)
		{
			return false;
		}
		EventDelegate.Add(button.onClick, OnClick);
		return true;
	}

	private GameObject Clone(string name, int depth, out UIButton cloneButton, out UILabel cloneLabel)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(template.gameObject, owner.transform, worldPositionStays: false);
		gameObject.name = name;
		MonoBehaviour[] componentsInChildren = gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
		foreach (MonoBehaviour monoBehaviour in componentsInChildren)
		{
			if (monoBehaviour.GetType().Name == "UILocalizeEx")
			{
				UnityEngine.Object.DestroyImmediate(monoBehaviour);
			}
		}
		cloneButton = gameObject.GetComponentInChildren<UIButton>(includeInactive: true);
		cloneLabel = gameObject.GetComponentInChildren<UILabel>(includeInactive: true);
		if (cloneButton == null || cloneLabel == null)
		{
			UnityEngine.Object.Destroy(gameObject);
			cloneButton = null;
			cloneLabel = null;
			return null;
		}
		cloneButton.onClick.Clear();
		UIWidget[] componentsInChildren2 = gameObject.GetComponentsInChildren<UIWidget>(includeInactive: true);
		int num = int.MaxValue;
		UIWidget[] array = componentsInChildren2;
		foreach (UIWidget uIWidget in array)
		{
			num = Math.Min(num, uIWidget.depth);
		}
		array = componentsInChildren2;
		foreach (UIWidget obj in array)
		{
			obj.leftAnchor.target = null;
			obj.rightAnchor.target = null;
			obj.topAnchor.target = null;
			obj.bottomAnchor.target = null;
			obj.depth = obj.depth - num + depth;
		}
		NGUITools.SetLayer(gameObject, owner.gameObject.layer);
		NGUITools.MarkParentAsChanged(gameObject);
		return gameObject;
	}

	private static void Place(GameObject cloneRoot, UIButton cloneButton, UILabel cloneLabel, Vector3 center, float width, float height)
	{
		UIWidget[] array = new UIWidget[2]
		{
			cloneRoot.GetComponent<UIWidget>(),
			cloneButton.GetComponent<UIWidget>()
		};
		foreach (UIWidget uIWidget in array)
		{
			if (uIWidget != null)
			{
				uIWidget.pivot = UIWidget.Pivot.Center;
				uIWidget.width = Mathf.Max(2, Mathf.RoundToInt(width));
				uIWidget.height = Mathf.Max(2, Mathf.RoundToInt(height));
			}
		}
		if (cloneButton.gameObject != cloneRoot)
		{
			cloneButton.transform.localPosition = Vector3.zero;
			cloneButton.transform.localScale = Vector3.one;
		}
		cloneLabel.overflowMethod = UILabel.Overflow.ShrinkContent;
		cloneLabel.pivot = UIWidget.Pivot.Center;
		cloneLabel.width = Mathf.Max(2, Mathf.RoundToInt(width * 0.86f));
		cloneLabel.height = Mathf.Max(2, Mathf.RoundToInt(height * 0.8f));
		cloneLabel.fontSize = Mathf.Max(8, Mathf.RoundToInt(height * 0.5f));
		cloneLabel.transform.localScale = Vector3.one;
		cloneLabel.transform.localPosition = new Vector3(0f, height * 0.04f, 0f);
		cloneRoot.transform.localScale = Vector3.one;
		cloneRoot.transform.localPosition = center;
		NGUITools.UpdateWidgetCollider(cloneButton.gameObject);
	}

	private bool Region(out float left, out float right, out float centerY, out float iconHeight, out float buttonLeft)
	{
		left = (right = (centerY = (iconHeight = (buttonLeft = 0f))));
		Transform transform = FindChild(owner.transform, "Label_SkillLevel");
		Transform transform2 = ExtendIcon(last: true);
		Transform transform3 = ExtendIcon(last: false);
		if (transform == null || transform2 == null || transform3 == null)
		{
			return false;
		}
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(owner.transform, transform, considerInactive: true);
		Bounds bounds2 = NGUIMath.CalculateRelativeWidgetBounds(owner.transform, transform2, considerInactive: true);
		Bounds bounds3 = NGUIMath.CalculateRelativeWidgetBounds(owner.transform, transform3, considerInactive: true);
		iconHeight = bounds2.size.y;
		centerY = bounds2.center.y;
		right = bounds.max.x;
		left = bounds3.min.x - bounds3.size.x * 0.2f;
		buttonLeft = bounds2.max.x + bounds2.size.x * 0.35f;
		return iconHeight > 0f && right > left;
	}

	private void Layout()
	{
		if (!Region(out var _, out var right, out var centerY, out var iconHeight, out var buttonLeft))
		{
			Show(show: false);
			return;
		}
		float num = iconHeight * 0.78f;
		float num2 = Mathf.Max((right - buttonLeft) * 0.8f, num);
		Place(root, button, label, new Vector3(right - num2 * 0.5f, centerY, 0f), num2, num);
	}

	private Transform ExtendIcon(bool last)
	{
		if (owner.ExtendSkills == null)
		{
			return null;
		}
		int num = owner.ExtendSkills.Length;
		for (int i = 0; i < num; i++)
		{
			HeroSkillSubComponent.ExtendSkillGroup extendSkillGroup = owner.ExtendSkills[last ? (num - 1 - i) : i];
			if (extendSkillGroup != null && extendSkillGroup.Texture_SkillIcon != null)
			{
				return extendSkillGroup.Texture_SkillIcon.transform;
			}
		}
		return null;
	}

	private void Show(bool show)
	{
		if (root != null && root.activeSelf != show)
		{
			root.SetActive(show);
		}
	}

	private static string Caption(int rule)
	{
		if (rule == Off)
		{
			return SprkText.Get("Off");
		}
		if (rule >= 1 && rule <= 3)
		{
			return "★" + rule;
		}
		return SprkText.Get("Normal");
	}

	private void Apply()
	{
		if (button == null)
		{
			return;
		}
		int num = Rule(Rules(heroIndex), slot);
		label.text = Caption(num);
		label.color = ((num == Off) ? OffText : ((num > 0) ? RankedText : NormalText));
		SetButtonColor(button, (num == Off) ? OffButton : ((num > 0) ? RankedButton : NormalButton));
	}

	private static void SetButtonColor(UIButton target, Color color)
	{
		target.defaultColor = color;
		target.hover = color;
		target.UpdateColor(instant: true);
	}

	public void OnClick()
	{
		if (owner == null || owner.HeroInfo == null || button == null)
		{
			return;
		}
		heroIndex = owner.HeroInfo.HeroIndex;
		slot = owner.SkillSlotIndex;
		if (chooser != null && chooser.activeSelf)
		{
			CloseChooser();
		}
		else
		{
			OpenChooser();
		}
	}

	private void OpenChooser()
	{
		if (openChooser != null && openChooser != this)
		{
			openChooser.CloseChooser();
		}
		if (!Region(out var left, out var right, out var centerY, out var iconHeight, out var _))
		{
			return;
		}
		if (chooser == null && !BuildChooser())
		{
			return;
		}
		float num = right - left;
		float num2 = iconHeight * 1.34f;
		float num3 = iconHeight * 0.96f;
		float num4 = num * 0.012f;
		float num5 = (num - num4 * (Choices.Length + 1)) / (float)Choices.Length;
		Transform transform = chooser.transform;
		UIButton component = transform.GetChild(0).GetComponentInChildren<UIButton>(includeInactive: true);
		Place(transform.GetChild(0).gameObject, component, transform.GetChild(0).GetComponentInChildren<UILabel>(includeInactive: true), new Vector3(left + num * 0.5f, centerY, 0f), num + num4 * 2f, num2);
		int num6 = Rule(Rules(heroIndex), slot);
		for (int i = 0; i < Choices.Length; i++)
		{
			Transform child = transform.GetChild(i + 1);
			UIButton uIButton = choiceButtons[i];
			UILabel componentInChildren = child.GetComponentInChildren<UILabel>(includeInactive: true);
			Place(child.gameObject, uIButton, componentInChildren, new Vector3(left + num4 + num5 * 0.5f + (float)i * (num5 + num4), centerY, 0f), num5, num3);
			componentInChildren.text = Caption(Choices[i]);
			componentInChildren.fontSize = Mathf.Max(8, Mathf.RoundToInt(num3 * 0.5f));
			bool flag = Choices[i] == num6;
			componentInChildren.color = ((Choices[i] == Off) ? OffText : (flag ? RankedText : NormalText));
			SetButtonColor(uIButton, flag ? ((Choices[i] == Off) ? OffButton : RankedButton) : ChoiceIdle);
		}
		chooser.SetActive(value: true);
		Show(show: false);
		openChooser = this;
	}

	private bool BuildChooser()
	{
		chooser = new GameObject("SprkSkillPriorityChooser");
		chooser.transform.SetParent(owner.transform, worldPositionStays: false);
		chooser.layer = owner.gameObject.layer;
		UIButton cloneButton;
		UILabel cloneLabel;
		GameObject gameObject = Clone("Backdrop", baseDepth + 20, out cloneButton, out cloneLabel);
		if (gameObject == null)
		{
			UnityEngine.Object.Destroy(chooser);
			chooser = null;
			return false;
		}
		gameObject.transform.SetParent(chooser.transform, worldPositionStays: false);
		cloneLabel.text = "";
		SetButtonColor(cloneButton, Backdrop);
		EventDelegate.Add(cloneButton.onClick, CloseChooser);
		choiceButtons.Clear();
		for (int i = 0; i < Choices.Length; i++)
		{
			int choice = Choices[i];
			UIButton cloneButton2;
			UILabel cloneLabel2;
			GameObject gameObject2 = Clone("Choice" + i, baseDepth + 30, out cloneButton2, out cloneLabel2);
			gameObject2.transform.SetParent(chooser.transform, worldPositionStays: false);
			EventDelegate.Add(cloneButton2.onClick, delegate
			{
				Choose(choice);
			});
			choiceButtons.Add(cloneButton2);
		}
		NGUITools.SetLayer(chooser, owner.gameObject.layer);
		NGUITools.MarkParentAsChanged(chooser);
		return true;
	}

	private void Choose(int rule)
	{
		if (owner == null || owner.HeroInfo == null)
		{
			CloseChooser();
			return;
		}
		heroIndex = owner.HeroInfo.HeroIndex;
		slot = owner.SkillSlotIndex;
		Save(heroIndex, Assign(Rules(heroIndex), slot, rule));
		CloseChooser();
		Transform parent = owner.transform.parent;
		SprkSkillPriority[] array = ((parent == null) ? new SprkSkillPriority[1] { this } : parent.GetComponentsInChildren<SprkSkillPriority>(includeInactive: false));
		foreach (SprkSkillPriority sprkSkillPriority in array)
		{
			if (sprkSkillPriority.heroIndex == heroIndex)
			{
				sprkSkillPriority.Apply();
			}
		}
	}

	public void CloseChooser()
	{
		if (chooser != null && chooser.activeSelf)
		{
			chooser.SetActive(value: false);
			Show(show: true);
		}
		if (openChooser == this)
		{
			openChooser = null;
		}
	}

	private void OnDisable()
	{
		CloseChooser();
	}

	private static Transform FindChild(Transform root, string name)
	{
		Transform[] componentsInChildren = root.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == name)
			{
				return transform;
			}
		}
		return null;
	}
}
