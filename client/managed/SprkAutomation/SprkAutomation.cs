using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using JsonBin;
using MiniJSON;
using NGame2.IAP;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField;
using NGame2.NCutScene;
using NGame2.NGuild;
using NGame2.NLogin.NState;
using NGame2.NOrdeal;
using NGame2.NScene;
using NGame2.NTutorial;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NBattle;
using NGame2.NUI.NComponent2.NGuildAttendance;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NHelper.NPayShop;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NInventory;
using NGame2.NUI.NWindow.NPet;
using NGame2.NUI.NWindow.NSoulWeapon;
using NGame2.NUI.NWorldMap.NMapLayout;
using NGameAsset;
using NGameAsset.NBundleManager;
using NGameAsset.NCreature;
using NGameAsset.NEffect;
using NShader;
using NShared;
using NShared.EndOrdealArena;
using NShared.Login;
using NShared.NJit;
using NShared.ResurrectionHero;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public sealed class SprkAutomation : MonoBehaviour
{
	[Serializable]
	public class Command
	{
		public string id;

		public string action;

		public int target;

		public string text;
	}

	[Serializable]
	public class Failure
	{
		public string type;

		public string message;

		public string stack;
	}

	[Serializable]
	public class Element
	{
		public int id;

		public string path;

		public string kind;

		public string text;

		public bool enabled;

		public float x;

		public float y;
	}

	[Serializable]
	public class Reply
	{
		public string id;

		public bool ok;

		public string error;

		public string scene;

		public int pid;

		public int width;

		public int height;

		public int frame;

		public string screenshot;

		public List<Element> elements = new List<Element>();

		public List<string> behaviours = new List<string>();

		public List<Failure> errors = new List<Failure>();

		public List<object> materials = new List<object>();
	}

	private sealed class InnPresentationListener : HeroInnViewButtonGroup.Listener
	{
		public int GetCostValue(FriendlyActionType action)
		{
			return 0;
		}

		public string GetCostKey(FriendlyActionType action)
		{
			return "COMMON_FREE_BUY";
		}

		public bool CheckCoolTime(FriendlyActionType action)
		{
			return true;
		}

		public bool CheckCost(FriendlyActionType action)
		{
			return true;
		}

		public bool CheckClosenessPoint()
		{
			return false;
		}
	}

	private static SprkAutomation instance;

	private string root;

	private bool busy;

	private float nextPoll;

	private readonly List<Failure> errors = new List<Failure>();

	private AssetBundle janeProbeShaders;

	public static void Ensure()
	{
		SprkStorageMigration.StartReporter();
		string environmentVariable = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR");
		if (instance != null || string.IsNullOrEmpty(environmentVariable))
		{
			return;
		}
		try
		{
			environmentVariable = Path.GetFullPath(environmentVariable);
			Directory.CreateDirectory(environmentVariable);
			GameObject obj = new GameObject("SprkAutomation");
			UnityEngine.Object.DontDestroyOnLoad(obj);
			instance = obj.AddComponent<SprkAutomation>();
			instance.root = environmentVariable;
			Application.runInBackground = true;
			Application.logMessageReceived += instance.Log;
			File.WriteAllText(Path.Combine(environmentVariable, "ready.json"), "{\"pid\":" + Process.GetCurrentProcess().Id + "}");
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("SPRK automation init: " + ex);
		}
	}

	private void Log(string message, string stack, LogType type)
	{
		if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
		{
			if (string.IsNullOrEmpty(stack) && message != null && message.IndexOf("Duplicate passive", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				stack = Environment.StackTrace;
			}
			if (errors.Count == 100)
			{
				errors.RemoveAt(0);
			}
			errors.Add(new Failure
			{
				type = type.ToString(),
				message = message,
				stack = stack
			});
		}
	}

	private static string Hierarchy(Transform t)
	{
		string text = t.name;
		while (t.parent != null)
		{
			t = t.parent;
			text = t.name + "/" + text;
		}
		return text;
	}

	private static Element Describe(Component c, string kind, string text, bool enabled)
	{
		Camera camera = NGUITools.FindCameraForLayer(c.gameObject.layer);
		Vector3 vector = ((camera == null) ? Vector3.zero : camera.WorldToScreenPoint(c.transform.position));
		return new Element
		{
			id = c.gameObject.GetInstanceID(),
			path = Hierarchy(c.transform),
			kind = kind,
			text = text,
			enabled = enabled,
			x = vector.x,
			y = (float)Screen.height - vector.y
		};
	}

	private Reply Snapshot(string id)
	{
		Reply reply = new Reply
		{
			id = id,
			ok = true,
			scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
			pid = Process.GetCurrentProcess().Id,
			width = Screen.width,
			height = Screen.height,
			frame = Time.frameCount
		};
		UIButton[] array = UnityEngine.Object.FindObjectsOfType<UIButton>();
		UILabel[] array2;
		foreach (UIButton uIButton in array)
		{
			UILabel[] componentsInChildren = uIButton.GetComponentsInChildren<UILabel>();
			string text = "";
			array2 = componentsInChildren;
			foreach (UILabel uILabel in array2)
			{
				text = text + uILabel.text + " ";
			}
			reply.elements.Add(Describe(uIButton, "button", text.Trim(), uIButton.isEnabled && uIButton.enabled));
		}
		Collider[] array3 = UnityEngine.Object.FindObjectsOfType<Collider>();
		foreach (Collider collider in array3)
		{
			if (!(collider.GetComponent<UIButton>() != null) && !(NGUITools.FindCameraForLayer(collider.gameObject.layer) == null))
			{
				UIWidget component = collider.GetComponent<UIWidget>();
				if (!(component == null) || !(collider.GetComponent<UIEventListener>() == null))
				{
					reply.elements.Add(Describe(collider, "button", "", collider.enabled && (component == null || component.alpha > 0f)));
				}
			}
		}
		array2 = UnityEngine.Object.FindObjectsOfType<UILabel>();
		foreach (UILabel uILabel2 in array2)
		{
			reply.elements.Add(Describe(uILabel2, "label", uILabel2.text, uILabel2.enabled && uILabel2.alpha > 0f));
		}
		UIInput[] array4 = UnityEngine.Object.FindObjectsOfType<UIInput>();
		foreach (UIInput uIInput in array4)
		{
			reply.elements.Add(Describe(uIInput, "input", "", uIInput.enabled));
		}
		MapNode[] array5 = UnityEngine.Object.FindObjectsOfType<MapNode>();
		foreach (MapNode mapNode in array5)
		{
			Collider component2 = mapNode.GetComponent<Collider>();
			if (mapNode.NodeData != null && !mapNode.NodeData.Hidden)
			{
				reply.elements.Add(Describe(mapNode, "map-node", mapNode.ChapterIndex + ":" + mapNode.DungeonIndex + ":" + mapNode.NodeData.EntryNPCCode, mapNode.isActiveAndEnabled && component2 != null && component2.enabled));
			}
		}
		HashSet<string> hashSet = new HashSet<string>();
		MonoBehaviour[] array6 = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in array6)
		{
			if (!(monoBehaviour == null) && monoBehaviour.enabled)
			{
				string fullName = monoBehaviour.GetType().FullName;
				if ((fullName.Contains("Sprk") || fullName.Contains("Window") || fullName.Contains("Battle") || fullName.Contains("Lobby") || fullName.Contains("State")) && hashSet.Add(fullName))
				{
					reply.behaviours.Add(fullName);
				}
			}
		}
		reply.errors.AddRange(errors);
		return reply;
	}

	private GameObject Target(int id)
	{
		Transform[] array = UnityEngine.Object.FindObjectsOfType<Transform>();
		foreach (Transform transform in array)
		{
			if (transform.gameObject.GetInstanceID() == id)
			{
				return transform.gameObject;
			}
		}
		throw new InvalidOperationException("Target is no longer active: " + id);
	}

	private void Update()
	{
		if (root == null || busy || Time.realtimeSinceStartup < nextPoll)
		{
			return;
		}
		nextPoll = Time.realtimeSinceStartup + 0.1f;
		string path = Path.Combine(root, "command.json");
		if (!File.Exists(path))
		{
			return;
		}
		Command command = null;
		Reply reply = null;
		try
		{
			command = JsonUtility.FromJson<Command>(File.ReadAllText(path));
			File.Delete(path);
			if (command == null || !Guid.TryParseExact(command.id, "N", out var _))
			{
				throw new InvalidOperationException("Invalid command ID");
			}
			if (command.action == "dispatch-selection-probe")
			{
				string environmentVariable = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" || string.IsNullOrEmpty(environmentVariable) || !environmentVariable.StartsWith("dispatch-selection-"))
				{
					throw new InvalidOperationException("Dispatch selection fixture requires isolated Login and dispatch-selection profile");
				}
				string item = (string)Assembly.Load("SprkDispatchSelection").GetType("SprkDispatchSelectionProbe").GetMethod("Run")
					.Invoke(null, new object[1] { command.text });
				reply = Snapshot(command.id);
				reply.materials.Add(item);
				Write(reply);
				return;
			}
			if (command.action == "godking-background-probe")
			{
				reply = Snapshot(command.id);
				ContentUIGodkingTrialDungeonList[] array = UnityEngine.Object.FindObjectsOfType<ContentUIGodkingTrialDungeonList>();
				for (int i = 0; i < array.Length; i++)
				{
					UITexture component = array[i].transform.Find("Panel/Root/Bg").GetComponent<UITexture>();
					reply.materials.Add(new Dictionary<string, object>
					{
						{
							"texture",
							(component.mainTexture == null) ? null : component.mainTexture.name
						},
						{
							"shader",
							(component.shader == null) ? null : component.shader.name
						},
						{ "alpha", component.finalAlpha },
						{ "depth", component.depth },
						{ "enabled", component.enabled },
						{
							"active",
							component.gameObject.activeInHierarchy
						},
						{
							"uv",
							component.uvRect.ToString()
						},
						{
							"color",
							component.color.ToString()
						},
						{
							"region",
							component.drawRegion.ToString()
						},
						{
							"panels",
							string.Join(";", from p in component.GetComponentsInParent<UIPanel>()
								select p.name + ":" + p.alpha + ":" + p.depth)
						},
						{
							"loader",
							component.GetComponent<UIBundleLoader>().AssetPath
						},
						{
							"drawCall",
							(component.drawCall == null) ? null : component.drawCall.name
						},
						{ "visible", component.isVisible },
						{
							"layer",
							component.gameObject.layer
						}
					});
				}
				Write(reply);
				return;
			}
			if (command.action == "preview-shadow-probe")
			{
				string environmentVariable2 = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" || string.IsNullOrEmpty(environmentVariable2) || !environmentVariable2.StartsWith("preview-shadow-"))
				{
					throw new InvalidOperationException("Preview shadow fixture requires isolated Login and preview-shadow profile");
				}
				Type type = Assembly.Load("SprkFreeCamera").GetType("SprkFreeCamera");
				reply = Snapshot(command.id);
				reply.materials.Add((string)type.GetMethod("ProbePreviewShadows").Invoke(null, null));
				Write(reply);
				return;
			}
			if (command.action == "line-pet-index-probe" || command.action == "line-pet-currency-probe")
			{
				string environmentVariable3 = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" || string.IsNullOrEmpty(environmentVariable3) || !environmentVariable3.StartsWith("line-pets-"))
				{
					throw new InvalidOperationException("LINE pet fixture requires isolated Login and line-pets profile");
				}
				reply = Snapshot(command.id);
				reply.materials.Add((command.action == "line-pet-currency-probe") ? ProbeLinePetCurrency() : ProbeLinePetIndex());
				Write(reply);
				return;
			}
			if (command.action == "skill-formula-probe")
			{
				string environmentVariable4 = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" || string.IsNullOrEmpty(environmentVariable4) || !environmentVariable4.StartsWith("skill-formula-"))
				{
					throw new InvalidOperationException("Formula fixture requires isolated Login and skill-formula profile");
				}
				Type type2 = Assembly.Load("SprkSkillDescriptionFormula").GetType("SprkSkillDescriptionFormulaProbe");
				string text = ((command.text == "init") ? "Init" : ((command.text == "preference") ? "Preference" : ((command.text == "view") ? "View" : ((command.text == "index-view") ? "ViewArchive" : ((command.text == "toggle") ? "ToggleView" : "Run")))));
				string item2 = (string)type2.GetMethod(text).Invoke(null, null);
				reply = Snapshot(command.id);
				reply.materials.Add(item2);
				Write(reply);
				return;
			}
			if (command.action == "team-exp-refresh-probe")
			{
				string environmentVariable5 = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" || string.IsNullOrEmpty(environmentVariable5) || !environmentVariable5.StartsWith("team-exp-"))
				{
					throw new InvalidOperationException("Team EXP fixture requires isolated Login and team-exp profile");
				}
				string item3 = (string)Assembly.Load("SprkTeamExpRefresh").GetType("SprkTeamExpRefreshProbe").GetMethod("Run")
					.Invoke(null, null);
				reply = Snapshot(command.id);
				reply.materials.Add(item3);
				Write(reply);
				return;
			}
			if (command.action == "artifact-source-filter-probe")
			{
				string environmentVariable6 = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" || string.IsNullOrEmpty(environmentVariable6) || !environmentVariable6.StartsWith("artifact-filter-"))
				{
					throw new InvalidOperationException("Artifact source fixture requires isolated Login and artifact-filter profile");
				}
				string item4 = (string)Assembly.Load("SprkArtifactSourceFilter").GetType("SprkArtifactSourceFilterProbe").GetMethod("Run")
					.Invoke(null, null);
				reply = Snapshot(command.id);
				reply.materials.Add(item4);
				Write(reply);
				return;
			}
			if (command.action == "accessories-probe")
			{
				string environmentVariable7 = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" || string.IsNullOrEmpty(environmentVariable7) || !environmentVariable7.StartsWith("accessories-"))
				{
					throw new InvalidOperationException("Accessory fixture requires isolated Login and accessories profile");
				}
				Type type3 = Assembly.Load("SprkAccessories").GetType("SprkAccessoriesProbe");
				reply = Snapshot(command.id);
				reply.materials.Add((string)type3.GetMethod("Run").Invoke(null, null));
				Write(reply);
				return;
			}
			if (command.action == "inn-presentation-probe")
			{
				string environmentVariable8 = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" || string.IsNullOrEmpty(environmentVariable8) || !environmentVariable8.StartsWith("inn-presentation-"))
				{
					throw new InvalidOperationException("Inn presentation fixture requires isolated Login and inn-presentation profile");
				}
				reply = Snapshot(command.id);
				reply.materials.Add(ProbeInnPresentation());
				Write(reply);
				return;
			}
			if (command.action == "ticket-search-probe")
			{
				string environmentVariable9 = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" || string.IsNullOrEmpty(environmentVariable9) || !environmentVariable9.StartsWith("ticket-search-"))
				{
					throw new InvalidOperationException("Ticket search fixture requires isolated Login and ticket-search profile");
				}
				string item5 = (string)Assembly.Load("SprkTicketSearch").GetType("SprkTicketSearchProbe").GetMethod("Run")
					.Invoke(null, new object[1] { command.text });
				reply = Snapshot(command.id);
				reply.materials.Add(item5);
				Write(reply);
				return;
			}
			if (command.action == "hero-rune-filter-probe")
			{
				string environmentVariable10 = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" || string.IsNullOrEmpty(environmentVariable10) || !environmentVariable10.StartsWith("rune-filter-"))
				{
					throw new InvalidOperationException("Rune filter fixture requires isolated Login and rune-filter profile");
				}
				string item6 = (string)Assembly.Load("SprkHeroRuneFilter").GetType("SprkHeroRuneFilter").GetMethod("Probe")
					.Invoke(null, null);
				reply = Snapshot(command.id);
				reply.materials.Add(item6);
				Write(reply);
				return;
			}
			if (command.action == "battle-result-flask-probe")
			{
				string environmentVariable11 = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" || string.IsNullOrEmpty(environmentVariable11) || !environmentVariable11.StartsWith("flask-result-"))
				{
					throw new InvalidOperationException("Flask result fixture requires isolated Login and flask-result profile");
				}
				busy = true;
				StartCoroutine(ProbeBattleResultFlask(command));
				return;
			}
			if (command.action == "essence-craft-probe")
			{
				string environmentVariable12 = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" || string.IsNullOrEmpty(environmentVariable12) || !environmentVariable12.StartsWith("essence-craft-"))
				{
					throw new InvalidOperationException("Essence crafting fixture requires isolated Login and essence-craft profile");
				}
				List<object> collection = ProbeEssenceCraft();
				reply = Snapshot(command.id);
				reply.materials.AddRange(collection);
				Write(reply);
				return;
			}
			if (command.action == "arena-latency-probe")
			{
				Type type4 = Assembly.Load("SprkConquest").GetType("SprkArenaLatencyProbe");
				reply = Snapshot(command.id);
				reply.materials.Add(new Dictionary<string, object> { 
				{
					"result",
					(string)type4.GetMethod("Run").Invoke(null, null)
				} });
				Write(reply);
				return;
			}
			if (command.action == "arena-practice-probe" || command.action == "arena-practice-labels")
			{
				string environmentVariable13 = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" || string.IsNullOrEmpty(environmentVariable13) || !environmentVariable13.StartsWith("arena-practice-"))
				{
					throw new InvalidOperationException("Arena practice fixture requires isolated Login and arena-practice profile");
				}
				string item7 = (string)Assembly.Load("SprkArenaPractice").GetType("SprkArenaPracticeProbe").GetMethod((command.action == "arena-practice-probe") ? "Run" : "VerifyLabels")
					.Invoke(null, null);
				reply = Snapshot(command.id);
				reply.materials.Add(item7);
				Write(reply);
				return;
			}
			if (command.action == "inventory-rune-probe")
			{
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login")
				{
					throw new InvalidOperationException("Inventory rune probe requires isolated Login");
				}
				ProbeInventoryRunes();
				reply = Snapshot(command.id);
				reply.materials.Add("PASS: native inventory prefab exposes Normal Rune and Rune of Blasphemy subcategories, and both select correctly.");
				Write(reply);
				return;
			}
			if (command.action == "awakening-material-probe")
			{
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login")
				{
					throw new InvalidOperationException("Awakening material fixture requires isolated Login");
				}
				busy = true;
				StartCoroutine(ProbeAwakeningMaterials(command));
				return;
			}
			if (command.action == "forge-awaken-chance-probe")
			{
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login")
				{
					throw new InvalidOperationException("Forge chance fixture requires isolated Login");
				}
				string item8 = (string)Assembly.Load("SprkForgeAwakenChance").GetType("SprkForgeAwakenChanceProbe").GetMethod("Run")
					.Invoke(null, new object[1] { command.text });
				reply = Snapshot(command.id);
				reply.materials.Add(item8);
				Write(reply);
				return;
			}
			if (command.action == "arena-ranking-probe")
			{
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login")
				{
					throw new InvalidOperationException("Ranking fixture requires isolated Login");
				}
				TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
				GameObject gameObject = VespaResource.Load<GameObject>("Resources/Prefab2/Component/Match/MatchRankerComponent");
				if (gameObject == null)
				{
					throw new Exception("Native arena ranking row missing");
				}
				GameObject gameObject2 = new GameObject("ArenaRankingFixture");
				gameObject2.SetActive(value: false);
				try
				{
					MatchRankerComponent component2 = UnityEngine.Object.Instantiate(gameObject, gameObject2.transform, worldPositionStays: false).GetComponent<MatchRankerComponent>();
					MethodInfo method = component2.GetType().GetMethod("OnClickComponent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					method.Invoke(component2, null);
					int[][] array2 = new int[3][]
					{
						new int[4] { 1, 2, 3, 4 },
						new int[0],
						new int[2] { 4, 3 }
					};
					foreach (int[] array3 in array2)
					{
						MatchRankerInfo rankerInfo = new MatchRankerInfo
						{
							Rank = 0,
							MatchScore = 1000,
							CountryCode = "US",
							UserInfo = new CachedUserInfo
							{
								AccountId = 987654321L,
								Nick = "Ranking Fixture",
								TeamLevel = 100,
								AvatarHeroIndex = 15
							},
							HeroInfos = array3.Select((int id) => new DeckHeroInfo
							{
								Index = id,
								Star = 5,
								Level = 100
							}).ToArray()
						};
						if (!component2.Open(rankerInfo, MatchPvpType.Normal, null, isGlobal: true))
						{
							throw new Exception("Ranking row did not open");
						}
						IList list = (IList)component2.GetType().GetField("_HeroPortraitComponentList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(component2);
						if (list.Count != array3.Length)
						{
							throw new Exception("Ranking portrait count mismatch: " + list.Count);
						}
					}
					component2.Open(new MatchRankerInfo(), MatchPvpType.Normal, null, isGlobal: true);
					method.Invoke(component2, null);
				}
				finally
				{
					UnityEngine.Object.Destroy(gameObject2);
				}
				reply = Snapshot(command.id);
				reply.materials.Add("PASS: native ranking prefab renders four, zero and two squad portraits on reuse; empty-row click guards pass.");
				Write(reply);
				return;
			}
			if (command.action == "friend-duel-menu-probe")
			{
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login")
				{
					throw new InvalidOperationException("Friend menu fixture requires isolated Login");
				}
				GameObject obj = VespaResource.Load<GameObject>("Resources/Prefab2/Window/MessagePopup/InspectUserMenuPopup");
				if (obj == null)
				{
					throw new Exception("Friend menu prefab missing");
				}
				GameObject gameObject3 = new GameObject("FriendDuelFixture");
				gameObject3.SetActive(value: false);
				InspectUserMenuPopup component3 = UnityEngine.Object.Instantiate(obj, gameObject3.transform, worldPositionStays: false).GetComponent<InspectUserMenuPopup>();
				if (component3 == null || component3.DuelButton == null)
				{
					throw new Exception("Native friend menu has no Duel button binding");
				}
				MethodInfo method2 = component3.GetType().GetMethod("SprkSetDuelButtonVisible");
				if (method2 == null)
				{
					throw new Exception("Friend menu visibility patch missing");
				}
				method2.Invoke(component3, new object[1] { true });
				UIHelper.EnableButton(component3.DuelButton, enable: false, grayscaleIfDisable: true);
				if (!component3.DuelButton.gameObject.activeSelf || component3.DuelButton.isEnabled)
				{
					throw new Exception("Unavailable friend duel must remain visible and disabled");
				}
				UIHelper.EnableButton(component3.DuelButton, enable: true, grayscaleIfDisable: true);
				if (!component3.DuelButton.gameObject.activeSelf || !component3.DuelButton.isEnabled)
				{
					throw new Exception("Available friend duel must be visible and enabled");
				}
				method2.Invoke(component3, new object[1] { false });
				if (component3.DuelButton.gameObject.activeSelf)
				{
					throw new Exception("Nonfriend duel must remain hidden");
				}
				UnityEngine.Object.Destroy(gameObject3);
				reply = Snapshot(command.id);
				reply.materials.Add("PASS: actual friend menu prefab binding and visible-disabled, visible-enabled, nonfriend-hidden duel states.");
				Write(reply);
				return;
			}
			if (command.action == "purification-reuse-probe")
			{
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login")
				{
					throw new InvalidOperationException("Purification probe requires isolated Login");
				}
				busy = true;
				StartCoroutine(ProbePurificationReuse(command));
				return;
			}
			if (command.action == "boss-characteristics-probe")
			{
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login")
				{
					throw new InvalidOperationException("Boss scroll fixture requires isolated Login");
				}
				busy = true;
				StartCoroutine(ProbeBossCharacteristics(command));
				return;
			}
			if (command.action == "kasel-native-probe")
			{
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login")
				{
					throw new InvalidOperationException("Kasel fixture requires isolated Login");
				}
				busy = true;
				StartCoroutine(ProbeNativeKasel(command));
				return;
			}
			if (command.action == "maria-bird-probe")
			{
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login")
				{
					throw new InvalidOperationException("Maria fixture requires isolated Login");
				}
				TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
				string text2 = Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows");
				Dictionary<string, AssetInfo> dictionary = new Dictionary<string, AssetInfo>();
				MethodInfo setMethod = typeof(AssetInfo).GetProperty("EntryName").GetSetMethod(nonPublic: true);
				string[] files = Directory.GetFiles(Path.Combine(text2, "Assetbundle"), "*", SearchOption.AllDirectories);
				for (int i = 0; i < files.Length; i++)
				{
					string text3 = files[i].Substring(text2.Length + 1).Replace('\\', '/');
					AssetInfo assetInfo = new AssetInfo();
					setMethod.Invoke(assetInfo, new object[1] { text3 });
					dictionary.Add(text3, assetInfo);
				}
				SingletonAuto<AssetFileManager>.instance.Setup(dictionary);
				typeof(UnityCache).GetProperty("persistentDataPath").GetSetMethod(nonPublic: true).Invoke(null, new object[1] { Application.dataPath });
				typeof(NGameAsset.Define).GetProperty("PatchDownloaded").GetSetMethod(nonPublic: true).Invoke(null, new object[1] { true });
				LoaderPlaying loaderPlaying = SingletonUpdater<LoaderPlaying>.instance;
				if (!JsonRead.CreateRoot(File.ReadAllBytes(Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows/Assetbundle/BundlePack.bin")), out var jsonRead))
				{
					throw new Exception("Bundle pack unavailable");
				}
				loaderPlaying.InfoContainer.LoadFromJson(jsonRead["BundleFileInfo"]);
				SingletonAuto<NGameAsset.NCreature.BuilderContainer>.instance.Load(jsonRead["Creature"]);
				WeaponData weaponData = (SingletonAuto<NGameAsset.NCreature.BuilderContainer>.instance.Get("Hero.Maria") ?? throw new Exception("Maria builder unavailable")).WeaponList.Single((WeaponData w) => w.Name == "Weapon1");
				List<object> list2 = new List<object>();
				files = new string[2] { "Cos21Legend", "Cos21Legend1" };
				foreach (string text4 in files)
				{
					string[] array4 = new string[4] { "Unique01", "", "Trans", text4 };
					foreach (string text5 in array4)
					{
						foreach (CreatureStock.ELoadLevel value9 in Enum.GetValues(typeof(CreatureStock.ELoadLevel)))
						{
							CreatureStock stock = new CreatureStock
							{
								Code = "Hero.Maria",
								Star = 5,
								BodyPrefix = text4,
								HairPrefix = text4,
								LoadLevel = value9,
								WeaponStock = new WeaponStock(text5, 5, isEquipSoulWeapon: true, 2)
							};
							WeaponInstance.OpenData openData = weaponData.CreateOpenData(stock);
							if (openData == null)
							{
								throw new Exception("Maria weapon open failed");
							}
							try
							{
								NGameAsset.NCreature.ModelData modelData = (NGameAsset.NCreature.ModelData)((WeaponInstance.IOpenData)openData).GetModelData();
								if (modelData.Prefix != text4 || !modelData.AssetName.Contains("Cos21Legend_Weapon"))
								{
									throw new Exception("Maria " + text4 + " / " + text5 + " uses base bird " + modelData.AssetName);
								}
								foreach (AnimationLoadData animation in openData.AnimationLoadList)
								{
									AnimationData animationData = modelData.AnimationList.FirstOrDefault((AnimationData a) => AnimationKeyData.Comparer.Default.Equals(a.Key, animation.Data.Key));
									if (animationData != null && animation.Data.AssetName != animationData.AssetName)
									{
										throw new Exception("Maria bird lost Legend animation " + animationData.AssetName);
									}
								}
								list2.Add(new Dictionary<string, object>
								{
									{ "body", text4 },
									{ "incoming", text5 },
									{
										"level",
										value9.ToString()
									},
									{ "model", modelData.AssetName },
									{
										"animations",
										openData.AnimationLoadList.Count
									}
								});
							}
							finally
							{
								openData.Close();
							}
						}
					}
				}
				MethodInfo method3 = weaponData.GetType().GetMethod("SprkMariaLegendStock");
				CreatureStock[] array5 = new CreatureStock[3]
				{
					new CreatureStock
					{
						Code = "Hero.Jane",
						BodyPrefix = "Cos21Legend",
						WeaponStock = new WeaponStock("Unique01", 5, isEquipSoulWeapon: true, 2)
					},
					new CreatureStock
					{
						Code = "Hero.Maria",
						BodyPrefix = "Cos20Dessert",
						WeaponStock = new WeaponStock("Unique01", 5, isEquipSoulWeapon: true, 2)
					},
					new CreatureStock
					{
						Code = "Hero.Maria",
						BodyPrefix = "Cos21Legend",
						WeaponStock = new WeaponStock("Cos17Summer", 5, isEquipSoulWeapon: true, 2)
					}
				};
				for (int i = 0; i < array5.Length; i++)
				{
					CreatureStock creatureStock = array5[i];
					CreatureStock creatureStock2 = (CreatureStock)method3.Invoke(weaponData, new object[1] { creatureStock });
					if (creatureStock2.WeaponStock.WeaponPrefix != creatureStock.WeaponStock.WeaponPrefix || !creatureStock2.WeaponStock.IsEquipSoulWeapon || creatureStock2.WeaponStock.SoulWeaponGrade != 2 || creatureStock2.WeaponStock.WeaponStar != 5)
					{
						throw new Exception("Maria fix changed an unrelated appearance or soul weapon");
					}
				}
				reply = Snapshot(command.id);
				reply.materials.AddRange(list2);
				ProbeMariaBirdFrames(command, reply);
				reply.materials.Add("PASS: Maria Legend bird/animation selection across initial load, refresh and preview/combat load levels.");
				Write(reply);
				return;
			}
			if (command.action == "ordeal-revival-probe")
			{
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login")
				{
					throw new InvalidOperationException("Ordeals revival fixture requires isolated Login");
				}
				TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
				OrdealArenaManager ordealArenaManager = GlobalAutoBehaviour<OrdealArenaManager>.instance;
				BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				FieldInfo field = ordealArenaManager.GetType().GetField("_infoManager", flags);
				object value = field.GetValue(ordealArenaManager);
				UserOrdealArenaInfoManager userOrdealArenaInfoManager = new UserOrdealArenaInfoManager();
				field.SetValue(ordealArenaManager, userOrdealArenaInfoManager);
				AccountManager accountManager = GlobalAutoBehaviour<AccountManager>.instance;
				object value2 = accountManager.GetType().GetProperty("AccountManagers", flags).GetValue(accountManager, null);
				FieldInfo field2 = value2.GetType().GetField("UserManager");
				object value3 = field2.GetValue(value2);
				field2.SetValue(value2, new UserManager(accountManager, new UserInfo(), new PlayerMiscInfo(), new PlayerGuildPointInfo()));
				MethodInfo method4 = userOrdealArenaInfoManager.GetType().GetMethod("SetDeadHeroIndices", flags);
				GameObject obj2 = VespaResource.Load<GameObject>("Resources/" + HeroPortraitComponent.PrefabPath);
				if (obj2 == null)
				{
					throw new Exception("Hero portrait prefab missing");
				}
				GameObject gameObject4 = new GameObject("OrdealRevivalFixture");
				gameObject4.SetActive(value: false);
				HeroPortraitComponent component4 = UnityEngine.Object.Instantiate(obj2, gameObject4.transform, worldPositionStays: false).GetComponent<HeroPortraitComponent>();
				component4.GetType().GetField("<HeroInfo>k__BackingField", flags).SetValue(component4, new CachedHeroInfo
				{
					HeroIndex = 1
				});
				MethodInfo method5 = typeof(OrdealHelper).GetMethod("SprkSyncDeadPortrait");
				if (method5 == null)
				{
					throw new Exception("Ordeal death-state synchronization missing");
				}
				try
				{
					method4.Invoke(userOrdealArenaInfoManager, new object[1] { new int[1] { 1 } });
					if (component4.IsDead)
					{
						throw new Exception("Fixture must start with an unmarked portrait");
					}
					if (!(bool)method5.Invoke(null, new object[1] { component4 }) || !component4.IsDead)
					{
						throw new Exception("Unmarked dead hero bypasses revival");
					}
					if (OrdealHelper.GetResurrectionHeroGemCount() != 30)
					{
						throw new Exception("Revival price differs from archived 30 Rubies");
					}
					method4.Invoke(userOrdealArenaInfoManager, new object[1] { new int[0] });
					if ((bool)method5.Invoke(null, new object[1] { component4 }) || component4.IsDead)
					{
						throw new Exception("Revived hero remains marked dead");
					}
					Type type5 = ordealArenaManager.GetType().GetNestedTypes(flags).Single((Type t) => t.GetField("isWin", flags) != null);
					object obj3 = Activator.CreateInstance(type5, nonPublic: true);
					type5.GetField("<>4__this", flags).SetValue(obj3, ordealArenaManager);
					MethodInfo methodInfo = type5.GetMethods(flags).Single((MethodInfo m) => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(NShared.EndOrdealArena.Response));
					int[] array6 = new int[4] { 1, 2, 3, 4 };
					for (int num2 = 0; num2 < 2; num2++)
					{
						type5.GetField("isWin", flags).SetValue(obj3, false);
						methodInfo.Invoke(obj3, new object[1]
						{
							new NShared.EndOrdealArena.Response
							{
								BaseResult = BaseResultType.Success,
								Result = NShared.EndOrdealArena.ResultType.Success,
								DeadHeroIndices = array6,
								ClearNodeIndices = new int[0],
								SelectableBuffIndices = new int[0],
								ResultMatchScore = 5800
							}
						});
						if (!(from result5 in ordealArenaManager.GetDeadHeroInidices()
							orderby result5
							select result5).SequenceEqual(array6))
						{
							throw new Exception("Defeat callback failed to mark all four heroes dead");
						}
						if (!(bool)method5.Invoke(null, new object[1] { component4 }) || !component4.IsDead)
						{
							throw new Exception("Defeated-again hero bypasses revival");
						}
						for (int num3 = 1; num3 <= 4; num3++)
						{
							userOrdealArenaInfoManager.Apply(new NShared.ResurrectionHero.Response
							{
								DeadHeroIndices = array6.Skip(num3).ToArray(),
								CurrencyResults = new CurrencyResultInfo3[0]
							});
						}
						if (ordealArenaManager.GetDeadHeroInidices().Length != 0 || (bool)method5.Invoke(null, new object[1] { component4 }) || component4.IsDead)
						{
							throw new Exception("Repeated revival failed to clear death state");
						}
					}
					type5.GetField("isWin", flags).SetValue(obj3, true);
					methodInfo.Invoke(obj3, new object[1]
					{
						new NShared.EndOrdealArena.Response
						{
							BaseResult = BaseResultType.Success,
							Result = NShared.EndOrdealArena.ResultType.Success,
							DeadHeroIndices = new int[1] { 1 },
							ClearNodeIndices = new int[0],
							SelectableBuffIndices = new int[0],
							ResultMatchScore = 5900
						}
					});
					if (!ordealArenaManager.IsDeadHero(1) || ordealArenaManager.IsDeadHero(2))
					{
						throw new Exception("Victory callback lost partial death state");
					}
				}
				finally
				{
					field.SetValue(ordealArenaManager, value);
					field2.SetValue(value2, value3);
					UnityEngine.Object.Destroy(gameObject4);
				}
				reply = Snapshot(command.id);
				reply.materials.Add("PASS: native defeat callback marks four heroes dead through two defeat/revive cycles; portraits require 30-Ruby revival again; victory retains partial deaths.");
				Write(reply);
				return;
			}
			if (command.action == "tower-continuous-probe")
			{
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login")
				{
					throw new InvalidOperationException("Continuous Battle prefab probe requires isolated Login");
				}
				busy = true;
				StartCoroutine(ProbeTowerContinuous(command));
				return;
			}
			if (command.action == "native-stop-cycle")
			{
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" || command.target < 0 || command.target > 10)
				{
					throw new InvalidOperationException("Native overlay lifecycle probe requires Login and a 0..10 frame delay");
				}
				busy = true;
				StartCoroutine(ProbeNativeStop(command));
				return;
			}
			GameObject gameObject5;
			bool num4;
			if (command.action == "settings-probe")
			{
				if (!string.IsNullOrEmpty(command.text))
				{
					string environmentVariable14 = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
					if (string.IsNullOrEmpty(environmentVariable14) || !environmentVariable14.StartsWith("settings-persistence-"))
					{
						throw new InvalidOperationException("Settings writes require an isolated settings-persistence profile");
					}
					string[] array7 = command.text.Split(',');
					SystemOptionData.Graphic_FrameControl = (EFrameControl)int.Parse(array7[0]);
					SystemOptionData.Spec = (ESpecType)int.Parse(array7[1]);
					SystemOptionData.Sound_BgmVolume = float.Parse(array7[2], CultureInfo.InvariantCulture);
					SystemOptionData.Render_EnableOutline = bool.Parse(array7[3]);
					if (array7.Length > 4)
					{
						SprkTestPrefs.SetString("KingsRaid_GameOption_device_ConsumeBattery", array7[4]);
					}
				}
			}
			else
			{
				if (command.action == "click" || command.action == "click-and-stop-story" || command.action == "press" || command.action == "drag")
				{
					gameObject5 = Target(command.target);
					UIButton component5 = gameObject5.GetComponent<UIButton>();
					Collider component6 = gameObject5.GetComponent<Collider>();
					if (!(component5 != null))
					{
						if (!(component6 == null))
						{
							num4 = !component6.enabled;
							goto IL_237b;
						}
					}
					else if (component5.enabled)
					{
						num4 = !component5.isEnabled;
						goto IL_237b;
					}
					goto IL_2380;
				}
				if (command.action == "enter-raid-node")
				{
					PunishmentRaidComponent[] array8 = UnityEngine.Object.FindObjectsOfType<PunishmentRaidComponent>();
					PunishmentRaidComponent punishmentRaidComponent = null;
					PunishmentRaidComponent[] array9 = array8;
					foreach (PunishmentRaidComponent punishmentRaidComponent2 in array9)
					{
						if (punishmentRaidComponent2.gameObject.activeInHierarchy && punishmentRaidComponent2.PunishmentRaidData != null && punishmentRaidComponent2.PunishmentRaidData.RaidIndex == command.target)
						{
							if (punishmentRaidComponent != null)
							{
								throw new InvalidOperationException("Ambiguous raid node");
							}
							punishmentRaidComponent = punishmentRaidComponent2;
						}
					}
					if (punishmentRaidComponent == null || !punishmentRaidComponent.OpenRoot.gameObject.activeInHierarchy)
					{
						throw new InvalidOperationException("Raid node is not open");
					}
					punishmentRaidComponent.OnClickEnter();
				}
				else if (command.action == "video-probe")
				{
					if (!string.IsNullOrEmpty(command.text))
					{
						if (string.IsNullOrEmpty(GameRoot.Instance.VideoDataContainer.GetVideoPath(command.text)))
						{
							throw new InvalidOperationException("Unknown cinematic video code");
						}
						if (VideoManager.IsPlaying())
						{
							throw new InvalidOperationException("A cinematic is already running");
						}
						VideoManager.Open_VideoCode(command.text);
					}
					else if (command.target == 1 && VideoManager.IsPlaying())
					{
						VideoManager.SkipToClose(null);
					}
				}
				else if (command.action == "story-auto-probe")
				{
					if (!string.IsNullOrEmpty(command.text))
					{
						string[] array10 = command.text.Split(':');
						int num5 = int.Parse(array10[0]);
						int num6 = int.Parse(array10[1]);
						NGame2.NAccount.UserCampaignManager campaignManager = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager;
						if (!campaignManager.CanGoToChapter(num5) || !campaignManager.CanVisit(num5, num6))
						{
							throw new InvalidOperationException("Requested story test node is locked");
						}
						int linkedDungeonIndex = campaignManager.GetLinkedDungeonIndex(num5, num6);
						NGame2.NAccount.UserCampaignManager.SetChapterDungeonIndex(num5, num6, isForce: true);
						SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenPartySetting(new EnterDungeonData(num5, num6, linkedDungeonIndex, DungeonDifficultyType.Normal, campaignManager.CanPlayScenario(num5, num6) && !campaignManager.IsCompleted(num5, num6)));
					}
				}
				else if (command.action == "text" || command.action == "fill-input")
				{
					UIInput component7 = Target(command.target).GetComponent<UIInput>();
					if (component7 == null || !component7.enabled)
					{
						throw new InvalidOperationException("Input is disabled");
					}
					component7.value = command.text ?? "";
					if (command.action == "text")
					{
						component7.Submit();
					}
				}
				else if (command.action == "clear-errors")
				{
					errors.Clear();
				}
				else if (command.action == "focus-input")
				{
					if (command.target == 0)
					{
						UICamera.selectedObject = null;
					}
					else
					{
						UIInput component8 = Target(command.target).GetComponent<UIInput>();
						if (component8 == null || !component8.isActiveAndEnabled)
						{
							throw new InvalidOperationException("Input is disabled");
						}
						component8.isSelected = true;
					}
				}
				else if (command.action == "scroll")
				{
					UIScrollView uIScrollView = NGUITools.FindInParents<UIScrollView>(Target(command.target));
					if (uIScrollView == null || !float.TryParse(command.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2) || float.IsNaN(result2) || float.IsInfinity(result2) || Mathf.Abs(result2) > 10f)
					{
						throw new InvalidOperationException("Expected a scroll view and an amount between -10 and 10");
					}
					uIScrollView.Scroll(result2);
				}
				else if (!(command.action == "kasel-s2-probe") && !(command.action == "jane-legend-probe") && !(command.action == "maria-legend-probe") && !(command.action == "ui-navigation-probe"))
				{
					if (command.action == "preview-perk")
					{
						HeroTranscendSkillIconComponent component9 = Target(command.target).GetComponent<HeroTranscendSkillIconComponent>();
						if (component9 == null || component9.TranscendSkill == null)
						{
							throw new InvalidOperationException("Expected a text perk icon");
						}
						LearnTranscendSkillMessagePopup.Open((HeroInfo)component9.GetType().GetField("_heroInfo", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(component9), component9.TranscendSkill.TranscendSkillIndex, delegate
						{
						});
						if (command.text == "repeat-description")
						{
							LearnTranscendSkillMessagePopup learnTranscendSkillMessagePopup = UnityEngine.Object.FindObjectOfType<LearnTranscendSkillMessagePopup>();
							string text6 = learnTranscendSkillMessagePopup.Label_SkillDesc.text;
							learnTranscendSkillMessagePopup.Label_SkillDesc.text = text6 + "\n\n" + text6 + "\n\n" + text6;
							RestoredPortal.ReadablePerkDescription(learnTranscendSkillMessagePopup);
						}
					}
					else if (command.action == "open-raid-info")
					{
						List<object> list3 = new List<object>();
						RaidData raidData = null;
						RaidType[] array11 = new RaidType[7]
						{
							RaidType.DragonNormal,
							RaidType.DragonHard,
							RaidType.PunishmentRaid,
							RaidType.Challenge,
							RaidType.KarmaRaid,
							RaidType.GuildSuppress,
							RaidType.GuildSuppressSingle
						};
						foreach (RaidType raidType in array11)
						{
							IList<RaidData> list4 = null;
							try
							{
								list4 = GameRoot.Instance.RaidDataContainer.GetRaidDataListByType(raidType);
							}
							catch (Exception)
							{
							}
							if (list4 == null)
							{
								continue;
							}
							foreach (RaidData item9 in list4)
							{
								if (item9 == null)
								{
									continue;
								}
								string text7 = null;
								string text8 = null;
								string value4 = null;
								int num7 = 0;
								int num8 = 0;
								try
								{
									text8 = item9.Name;
									text7 = LocalizationManager.GetStringDirect(text8);
								}
								catch (Exception)
								{
								}
								try
								{
									num7 = item9.Index;
								}
								catch (Exception)
								{
								}
								try
								{
									num8 = item9.Level;
								}
								catch (Exception)
								{
								}
								try
								{
									value4 = item9.Type.ToString();
								}
								catch (Exception)
								{
								}
								list3.Add(new Dictionary<string, object>
								{
									{ "index", num7 },
									{ "level", num8 },
									{ "type", value4 },
									{ "name", text7 },
									{ "key", text8 }
								});
								if (command.target > 0)
								{
									if (num7 == command.target)
									{
										raidData = item9;
									}
								}
								else if (!string.IsNullOrEmpty(command.text) && text7 != null && text7.IndexOf(command.text, StringComparison.OrdinalIgnoreCase) >= 0 && (raidData == null || num8 < raidData.Level))
								{
									raidData = item9;
								}
							}
						}
						if (raidData == null)
						{
							PunishmentRaidComponent[] array9 = UnityEngine.Object.FindObjectsOfType<PunishmentRaidComponent>();
							foreach (PunishmentRaidComponent punishmentRaidComponent3 in array9)
							{
								PunishmentRaidData punishmentRaidData = punishmentRaidComponent3.PunishmentRaidData;
								if (punishmentRaidData == null)
								{
									continue;
								}
								RaidData data = GameRoot.Instance.RaidDataContainer.GetData(punishmentRaidData.RaidIndex, punishmentRaidData.RaidLevel);
								if (data == null)
								{
									continue;
								}
								list3.Add(new Dictionary<string, object>
								{
									{ "index", data.Index },
									{ "level", data.Level },
									{
										"type",
										data.Type.ToString()
									},
									{ "name", null },
									{ "key", data.Name },
									{ "node", punishmentRaidComponent3.name }
								});
								if (command.target > 0)
								{
									if (data.Index == command.target)
									{
										raidData = data;
									}
								}
								else if (!string.IsNullOrEmpty(command.text))
								{
									string text9 = null;
									try
									{
										text9 = LocalizationManager.GetStringDirect(data.Name);
									}
									catch (Exception)
									{
									}
									if (text9 != null && text9.IndexOf(command.text, StringComparison.OrdinalIgnoreCase) >= 0 && (raidData == null || data.Level < raidData.Level))
									{
										raidData = data;
									}
								}
							}
						}
						if (raidData == null)
						{
							throw new InvalidOperationException("Raid not found: " + (string.IsNullOrEmpty(command.text) ? command.target.ToString() : command.text) + " ; available=" + string.Join(",", list3.ConvertAll((object x) => ((Dictionary<string, object>)x)["type"]?.ToString() + ":" + ((Dictionary<string, object>)x)["index"]).ToArray()));
						}
						SceneAutoBehaviour<RaidManagement>.instance.OpenRaidInfo(raidData);
						reply.materials.Add(new Dictionary<string, object>
						{
							{ "kind", "openedRaid" },
							{ "index", raidData.Index },
							{ "level", raidData.Level },
							{ "raids", list3 }
						});
					}
					else if (command.action == "raid-info-probe")
					{
						RaidInfoBase[] array12 = UnityEngine.Object.FindObjectsOfType<RaidInfoBase>();
						foreach (RaidInfoBase raidInfoBase in array12)
						{
							UILabel raidTacticLabel = raidInfoBase.RaidTacticLabel;
							if (raidTacticLabel == null)
							{
								continue;
							}
							Dictionary<string, object> dictionary2 = new Dictionary<string, object>
							{
								{ "kind", "raidInfo" },
								{
									"path",
									Hierarchy(raidInfoBase.transform)
								},
								{
									"label",
									Hierarchy(raidTacticLabel.transform)
								},
								{
									"textLength",
									(raidTacticLabel.text != null) ? raidTacticLabel.text.Length : 0
								},
								{ "text", raidTacticLabel.text },
								{
									"labelPos",
									raidTacticLabel.transform.localPosition
								},
								{ "width", raidTacticLabel.width },
								{ "height", raidTacticLabel.height },
								{
									"pivot",
									raidTacticLabel.pivot.ToString()
								},
								{
									"overflow",
									raidTacticLabel.overflowMethod.ToString()
								},
								{
									"parent",
									Hierarchy(raidTacticLabel.transform.parent)
								}
							};
							Collider component10 = raidTacticLabel.GetComponent<Collider>();
							if (component10 != null)
							{
								dictionary2["collider"] = new Dictionary<string, object>
								{
									{
										"x",
										component10.bounds.size.x
									},
									{
										"y",
										component10.bounds.size.y
									}
								};
							}
							UIDragScrollView component11 = raidTacticLabel.GetComponent<UIDragScrollView>();
							dictionary2["labelDrag"] = ((component11 == null || component11.scrollView == null) ? null : Hierarchy(component11.scrollView.transform));
							List<object> list5 = new List<object>();
							UIScrollView[] componentsInChildren = raidInfoBase.GetComponentsInChildren<UIScrollView>(includeInactive: true);
							foreach (UIScrollView uIScrollView2 in componentsInChildren)
							{
								UIPanel panel = uIScrollView2.panel;
								Dictionary<string, object> dictionary3 = new Dictionary<string, object>
								{
									{
										"path",
										Hierarchy(uIScrollView2.transform)
									},
									{
										"pos",
										uIScrollView2.transform.localPosition
									},
									{
										"panelDepth",
										(panel == null) ? (-1) : panel.depth
									},
									{
										"clipping",
										(panel == null) ? "none" : panel.clipping.ToString()
									},
									{ "canMoveV", uIScrollView2.canMoveVertically },
									{ "disableDragIfFits", uIScrollView2.disableDragIfFits },
									{
										"movement",
										uIScrollView2.movement.ToString()
									},
									{
										"contentPivot",
										uIScrollView2.contentPivot.ToString()
									}
								};
								if (panel != null)
								{
									dictionary3["baseClip"] = new Dictionary<string, object>
									{
										{
											"x",
											panel.baseClipRegion.x
										},
										{
											"y",
											panel.baseClipRegion.y
										},
										{
											"z",
											panel.baseClipRegion.z
										},
										{
											"w",
											panel.baseClipRegion.w
										}
									};
									dictionary3["finalClip"] = new Dictionary<string, object>
									{
										{
											"x",
											panel.finalClipRegion.x
										},
										{
											"y",
											panel.finalClipRegion.y
										},
										{
											"z",
											panel.finalClipRegion.z
										},
										{
											"w",
											panel.finalClipRegion.w
										}
									};
									dictionary3["clipOffset"] = new Dictionary<string, object>
									{
										{
											"x",
											panel.clipOffset.x
										},
										{
											"y",
											panel.clipOffset.y
										}
									};
									dictionary3["panelSize"] = new Dictionary<string, object>
									{
										{ "x", panel.width },
										{ "y", panel.height }
									};
									try
									{
										dictionary3["shouldMoveV"] = uIScrollView2.shouldMoveVertically;
									}
									catch (Exception)
									{
									}
								}
								try
								{
									Bounds bounds = uIScrollView2.bounds;
									dictionary3["boundsMin"] = new Dictionary<string, object>
									{
										{
											"x",
											bounds.min.x
										},
										{
											"y",
											bounds.min.y
										}
									};
									dictionary3["boundsMax"] = new Dictionary<string, object>
									{
										{
											"x",
											bounds.max.x
										},
										{
											"y",
											bounds.max.y
										}
									};
								}
								catch (Exception)
								{
								}
								list5.Add(dictionary3);
							}
							dictionary2["scrolls"] = list5;
							List<object> list6 = new List<object>();
							UIDragScrollView[] componentsInChildren2 = raidInfoBase.GetComponentsInChildren<UIDragScrollView>(includeInactive: true);
							foreach (UIDragScrollView uIDragScrollView in componentsInChildren2)
							{
								list6.Add(new Dictionary<string, object>
								{
									{
										"path",
										Hierarchy(uIDragScrollView.transform)
									},
									{
										"target",
										(uIDragScrollView.scrollView == null) ? null : Hierarchy(uIDragScrollView.scrollView.transform)
									}
								});
							}
							dictionary2["drags"] = list6;
							reply.materials.Add(dictionary2);
						}
					}
					else if (command.action != "hero-growth-probe" && command.action != "account-form-probe" && command.action != "guild-attendance-probe" && command.action != "payment-probe" && command.action != "lobby-probe" && command.action != "snapshot" && command.action != "screenshot" && command.action != "quit" && command.action != "renderers" && command.action != "probe-renderer" && command.action != "soul-scene" && command.action != "dispatch-probe" && command.action != "party-ui" && command.action != "inn-scene" && command.action != "input-focus" && command.action != "perk-popup" && command.action != "skill-probe" && command.action != "combat-probe" && command.action != "combat-simulation" && command.action != "skill-table-probe" && command.action != "combat-table-probe")
					{
						throw new InvalidOperationException("Unknown action");
					}
				}
			}
			goto IL_3847;
			IL_3847:
			reply = Snapshot(command.id);
			if (command.action == "ui-navigation-probe")
			{
				GlobalAutoBehaviour<GlobalManagement>.instance.ForEachOpenedWindow(delegate(BaseWindow window)
				{
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "backStack" },
						{
							"type",
							(window == null) ? null : window.GetType().FullName
						},
						{
							"active",
							window != null && window.gameObject.activeInHierarchy
						},
						{
							"state",
							(window == null) ? null : window.WindowState.ToString()
						},
						{
							"opening",
							window != null && window.ReservedOpenTask
						}
					});
				});
				GlobalTopmostWindow globalTopmostWindow = GlobalTopmostWindow.instance;
				reply.materials.Add(new Dictionary<string, object>
				{
					{ "kind", "inputBlock" },
					{
						"blocked",
						globalTopmostWindow.ExecuteBack()
					}
				});
				BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
				foreach (object item10 in (IEnumerable)globalTopmostWindow.GetType().GetField("_touchDisablerList", bindingAttr).GetValue(globalTopmostWindow))
				{
					object value5 = item10.GetType().GetProperty("Disabler").GetValue(item10, null);
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "touchOwner" },
						{
							"type",
							value5.GetType().FullName
						},
						{
							"detail",
							value5.ToString()
						}
					});
				}
				AutoNextBattle[] array13 = Resources.FindObjectsOfTypeAll<AutoNextBattle>();
				foreach (AutoNextBattle autoNextBattle in array13)
				{
					if (autoNextBattle.gameObject.scene.IsValid())
					{
						reply.materials.Add(new Dictionary<string, object>
						{
							{ "kind", "autoNextWindow" },
							{
								"active",
								autoNextBattle.gameObject.activeInHierarchy
							},
							{
								"state",
								autoNextBattle.WindowState.ToString()
							},
							{ "opening", autoNextBattle.ReservedOpenTask },
							{
								"backType",
								autoNextBattle.BackButtonType.ToString()
							},
							{ "backRegister", autoNextBattle.BackRegister }
						});
					}
				}
			}
			if (command.action == "kasel-s2-probe")
			{
				ProbeKaselS2(command, reply);
			}
			if (command.action == "jane-legend-probe" || command.action == "maria-legend-probe")
			{
				ProbeJaneLegend(command, reply);
			}
			if (command.action == "settings-probe")
			{
				reply.materials.Add(new Dictionary<string, object>
				{
					{ "kind", "settings" },
					{
						"fps",
						(int)SystemOptionData.Graphic_FrameControl
					},
					{
						"targetFps",
						Application.targetFrameRate
					},
					{
						"vSync",
						QualitySettings.vSyncCount
					},
					{
						"texture",
						(int)SystemOptionData.Spec
					},
					{
						"bgm",
						SystemOptionData.Sound_BgmVolume
					},
					{
						"outline",
						SystemOptionData.Render_EnableOutline
					},
					{
						"prefsPath",
						Application.persistentDataPath
					}
				});
			}
			if (command.action == "video-probe")
			{
				VideoManager videoManager = SingletonUpdater<VideoManager>.instance;
				BindingFlags bindingAttr2 = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				Type type6 = videoManager.GetType();
				VideoPlayer videoPlayer = (VideoPlayer)type6.GetField("VideoPlayer", bindingAttr2).GetValue(videoManager);
				reply.materials.Add(new Dictionary<string, object>
				{
					{ "kind", "video" },
					{
						"state",
						type6.GetField("_state", bindingAttr2).GetValue(videoManager).ToString()
					},
					{
						"active",
						VideoManager.IsPlaying()
					},
					{
						"prepared",
						videoPlayer != null && videoPlayer.isPrepared
					},
					{
						"playing",
						videoPlayer != null && videoPlayer.isPlaying
					},
					{
						"frame",
						(videoPlayer == null) ? (-1) : videoPlayer.frame
					},
					{
						"time",
						(videoPlayer == null) ? 0.0 : videoPlayer.time
					},
					{
						"url",
						(videoPlayer == null) ? "" : videoPlayer.url
					},
					{
						"width",
						(!(videoPlayer == null)) ? videoPlayer.width : 0u
					},
					{
						"height",
						(!(videoPlayer == null)) ? videoPlayer.height : 0u
					}
				});
			}
			if (command.action == "story-auto-probe")
			{
				Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "SprkStoryAuto");
				bool flag = assembly != null && (bool)assembly.GetType("SprkStoryAuto").GetMethod("Active").Invoke(null, null);
				EnterDungeonData enterDungeon = SingletonAuto<CampaignContext>.instance.EnterDungeon;
				TutorialManager tutorialManager = SingletonUpdater<TutorialManager>.instance;
				reply.materials.Add(new Dictionary<string, object>
				{
					{ "kind", "storyAuto" },
					{ "active", flag },
					{
						"chapter",
						enterDungeon?.Chapter ?? 0
					},
					{
						"dungeon",
						enterDungeon?.OriginalDungeon ?? 0
					},
					{
						"difficulty",
						(enterDungeon == null) ? "" : enterDungeon.Difficulty.ToString()
					},
					{
						"scenario",
						enterDungeon?.IsScenario ?? false
					},
					{ "tutorial", tutorialManager.IsProgressing },
					{
						"tutorialCommand",
						(tutorialManager.CurrentTutorialData == null) ? "" : tutorialManager.CurrentTutorialData.Command
					}
				});
				if (GlobalAutoBehaviour<AccountManager>.isValidInstance && AccountManager.IsLogined())
				{
					AccountManager accountManager2 = GlobalAutoBehaviour<AccountManager>.instance;
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "storyRecruitment" },
						{
							"clauseOwned",
							accountManager2.GetHeroInfo(15) != null
						},
						{ "tutorialSkip", accountManager2.IsTutorialSkip },
						{
							"unlockComplete",
							accountManager2.TutorialManager.IsComplete(20001)
						},
						{
							"recruitmentComplete",
							accountManager2.TutorialManager.IsComplete(20002)
						}
					});
				}
				CutSceneManager[] array14 = UnityEngine.Object.FindObjectsOfType<CutSceneManager>();
				foreach (CutSceneManager cutSceneManager in array14)
				{
					ScriptGroup currentScriptGroup = cutSceneManager.CurrentScriptGroup;
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "storyCutscene" },
						{ "path", cutSceneManager.ScriptPath },
						{ "playing", cutSceneManager.IsPlaying },
						{ "loading", cutSceneManager.IsLoading },
						{ "script", cutSceneManager.CurrentScript },
						{
							"touch",
							(currentScriptGroup == null) ? "" : currentScriptGroup.TouchType.ToString()
						},
						{
							"skip",
							(currentScriptGroup == null) ? "" : currentScriptGroup.SkipType.ToString()
						}
					});
				}
				BattlePartySetting[] array15 = UnityEngine.Object.FindObjectsOfType<BattlePartySetting>();
				foreach (BattlePartySetting battlePartySetting in array15)
				{
					BindingFlags bindingAttr3 = BindingFlags.Instance | BindingFlags.NonPublic;
					BattlePartySetting.IDataProvider dataProvider = (BattlePartySetting.IDataProvider)typeof(BattlePartySetting).GetField("_dataProvider", bindingAttr3).GetValue(battlePartySetting);
					object value6 = typeof(BattlePartySetting).GetField("_listener", bindingAttr3).GetValue(battlePartySetting);
					EnterDungeonData enterDungeonData = dataProvider?.GetEnterDungeon();
					ChapterDungeonInfo chapterDungeonInfo = ((enterDungeonData == null) ? null : GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.Get(enterDungeonData.Chapter, enterDungeonData.OriginalDungeon));
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "storyParty" },
						{
							"type",
							battlePartySetting.GetType().FullName
						},
						{
							"open",
							battlePartySetting.IsOpen()
						},
						{
							"listener",
							(value6 == null) ? "" : value6.GetType().FullName
						},
						{
							"chapter",
							enterDungeonData?.Chapter ?? 0
						},
						{
							"dungeon",
							enterDungeonData?.OriginalDungeon ?? 0
						},
						{
							"linked",
							enterDungeonData?.LinkedDungeon ?? 0
						},
						{
							"difficulty",
							(enterDungeonData == null) ? "" : enterDungeonData.Difficulty.ToString()
						},
						{
							"replay",
							enterDungeonData?.IsScenarioReplay ?? false
						},
						{
							"battleType",
							(enterDungeonData == null || enterDungeonData.LinkedDungeonData == null) ? "" : enterDungeonData.LinkedDungeonData.BattleType.ToString()
						},
						{
							"maxStar",
							(int)(chapterDungeonInfo?.MaxStar ?? 0)
						},
						{
							"completedTime",
							(chapterDungeonInfo == null) ? "" : chapterDungeonInfo.CompletedTime
						},
						{
							"eligible",
							assembly != null && (bool)assembly.GetType("SprkStoryAuto").GetMethod("IsStory").Invoke(null, new object[1] { enterDungeonData })
						}
					});
				}
			}
			if (command.action == "hero-growth-probe")
			{
				if (command.text == "open-common")
				{
					Hero3DViewPanel[] array16 = UnityEngine.Object.FindObjectsOfType<Hero3DViewPanel>();
					for (int i = 0; i < array16.Length; i++)
					{
						array16[i].SetToggle(Hero3DViewPanel.EToggle.Info);
					}
				}
				for (int num9 = 1; num9 <= 5; num9++)
				{
					for (int num10 = 0; num10 <= ((num9 == 5) ? 5 : 0); num10++)
					{
						HeroInfo heroInfo = new HeroInfo
						{
							HeroIndex = 1,
							Star = (sbyte)num9,
							Transcended = (sbyte)num10,
							Level = 100,
							LimitBreakLevel = 10
						};
						reply.materials.Add(new Dictionary<string, object>
						{
							{ "kind", "heroCap" },
							{ "star", num9 },
							{ "transcended", num10 },
							{
								"maxLevel",
								heroInfo.GetMaxLevel()
							},
							{
								"canLimitBreak",
								heroInfo.CanIncLimitBreakLevel()
							},
							{
								"canLimitExp",
								heroInfo.CanIncLimitBreakExp()
							}
						});
					}
				}
				HeroStatPanel[] array17 = UnityEngine.Object.FindObjectsOfType<HeroStatPanel>();
				foreach (HeroStatPanel heroStatPanel in array17)
				{
					UIPanel[] componentsInChildren3 = heroStatPanel.GetComponentsInChildren<UIPanel>(includeInactive: true);
					foreach (UIPanel uIPanel in componentsInChildren3)
					{
						reply.materials.Add(new Dictionary<string, object>
						{
							{ "kind", "panel" },
							{
								"path",
								Hierarchy(uIPanel.transform)
							},
							{
								"active",
								uIPanel.gameObject.activeInHierarchy
							},
							{ "alpha", uIPanel.alpha },
							{
								"clip",
								uIPanel.baseClipRegion.ToString()
							},
							{
								"position",
								uIPanel.transform.localPosition.ToString()
							}
						});
					}
					UIWidget[] componentsInChildren4 = heroStatPanel.GetComponentsInChildren<UIWidget>(includeInactive: true);
					foreach (UIWidget uIWidget in componentsInChildren4)
					{
						if (!uIWidget.name.StartsWith("Label_Field") && !uIWidget.name.StartsWith("Label_Value"))
						{
							reply.materials.Add(new Dictionary<string, object>
							{
								{ "kind", "widget" },
								{
									"path",
									Hierarchy(uIWidget.transform)
								},
								{
									"active",
									uIWidget.gameObject.activeInHierarchy
								},
								{ "visible", uIWidget.isVisible },
								{ "alpha", uIWidget.finalAlpha },
								{ "width", uIWidget.width },
								{ "height", uIWidget.height },
								{
									"position",
									uIWidget.transform.localPosition.ToString()
								}
							});
						}
					}
				}
			}
			if (command.action == "account-form-probe")
			{
				UIInput[] array18 = UnityEngine.Object.FindObjectsOfType<UIInput>();
				foreach (UIInput uIInput in array18)
				{
					if (Hierarchy(uIInput.transform).Contains("AccountForm"))
					{
						reply.materials.Add(new Dictionary<string, object>
						{
							{ "kind", "accountInput" },
							{
								"path",
								Hierarchy(uIInput.transform)
							},
							{
								"length",
								uIInput.value.Length
							},
							{ "selected", uIInput.isSelected },
							{
								"type",
								uIInput.inputType.ToString()
							},
							{ "selectionStart", uIInput.selectionStart },
							{ "selectionEnd", uIInput.selectionEnd }
						});
					}
				}
			}
			if (command.action == "combat-table-probe")
			{
				IList obj4 = (IList)Json.Deserialize(command.text);
				if (obj4.Count > 1000)
				{
					throw new InvalidOperationException("Too many diagnostic rows");
				}
				foreach (Dictionary<string, object> item11 in obj4)
				{
					string text10 = (string)item11["table"];
					string text11 = (string)item11["key"];
					object value7 = typeof(GameRoot).GetProperty(text10 + "DataContainer").GetValue(GameRoot.Instance, null);
					MethodInfo methodInfo2 = value7.GetType().GetMethods().Single((MethodInfo m) => m.Name == "GetData" && m.GetParameters().Length == 1);
					Type parameterType = methodInfo2.GetParameters()[0].ParameterType;
					object obj5;
					if (text10 == "CreatureStarGradeStat")
					{
						string[] array19 = text11.Split(':');
						obj5 = new CreatureStarGradeStatKey(int.Parse(array19[0]), int.Parse(array19[1]), int.Parse(array19[2]));
					}
					else
					{
						obj5 = ((parameterType == typeof(string)) ? text11 : (parameterType.IsEnum ? Enum.ToObject(parameterType, int.Parse(text11)) : ((object)int.Parse(text11))));
					}
					object obj7;
					if (item11.ContainsKey("enumeration") && (bool)item11["enumeration"])
					{
						IEnumerable obj6 = (IEnumerable)value7.GetType().GetMethod("GetAllData").Invoke(value7, null);
						obj7 = null;
						foreach (object item12 in obj6)
						{
							PropertyInfo property = item12.GetType().GetProperty((text10 == "State") ? "StateType" : "Index");
							if (property == null)
							{
								throw new InvalidOperationException("Unsupported enumeration key: " + text10);
							}
							if (Convert.ToString(property.GetValue(item12, null)) == text11)
							{
								obj7 = item12;
								break;
							}
						}
					}
					else
					{
						obj7 = text10 switch
						{
							"SkillProjectile" => RestoredPortal.CombatProjectile(GameRoot.Instance.SkillProjectileDataContainer, (int)obj5), 
							"State" => RestoredPortal.CombatState(GameRoot.Instance.StateDataContainer, (string)obj5), 
							"Creature" => RestoredPortal.CombatCreature(GameRoot.Instance.CreatureDataContainer, (int)obj5), 
							_ => methodInfo2.Invoke(value7, new object[1] { obj5 }), 
						};
					}
					if (obj7 == null)
					{
						throw new InvalidOperationException("Missing diagnostic row: " + text10 + "/" + text11);
					}
					Dictionary<string, object> dictionary5 = new Dictionary<string, object>();
					foreach (string item13 in (IList)item11["fields"])
					{
						dictionary5[item13] = ((text10 == "CreatureStarGradeStat" && Enum.IsDefined(typeof(StatType), item13)) ? ((object)((CreatureStarGradeStatData)obj7).GetStat((StatType)Enum.Parse(typeof(StatType), item13))) : obj7.GetType().GetProperty(item13).GetValue(obj7, null));
					}
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "table", text10 },
						{ "key", text11 },
						{ "values", dictionary5 }
					});
				}
			}
			if (command.action == "skill-table-probe")
			{
				int[] array20 = (from s in (command.text ?? "").Split(',')
					select int.Parse(s)).ToArray();
				if (array20.Length > 1000)
				{
					throw new InvalidOperationException("Too many diagnostic rows");
				}
				int[] array21 = array20;
				foreach (int num11 in array21)
				{
					SkillData skillData = RestoredPortal.PunishmentSkill(GameRoot.Instance.SkillDataContainer, num11);
					if (skillData == null)
					{
						continue;
					}
					List<object> list7 = new List<object>();
					for (int num12 = 1; num12 <= 8; num12++)
					{
						OperationData operationData = skillData.GetOperationData(num12);
						if (operationData != null)
						{
							list7.Add(TableProperties(operationData));
						}
					}
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "index", num11 },
						{
							"skill",
							TableProperties(skillData)
						},
						{ "operations", list7 }
					});
				}
			}
			if (command.action == "combat-simulation")
			{
				int num13 = 70000;
				int num14 = command.target;
				int num15 = 0;
				string text13 = command.text ?? "";
				if (text13.Contains(";"))
				{
					string[] array22 = text13.Split(';');
					if (array22.Length != 4)
					{
						throw new InvalidOperationException("Expected chapter;dungeon;difficulty;heroes");
					}
					num13 = int.Parse(array22[0]);
					num14 = int.Parse(array22[1]);
					num15 = int.Parse(array22[2]);
					text13 = array22[3];
				}
				else if (command.target < 1 || command.target > 5)
				{
					throw new InvalidOperationException("Expected Apocalypsion dragon dungeon 1 to 5");
				}
				AccountManager accountManager3 = GlobalAutoBehaviour<AccountManager>.instance;
				int[] array23 = (from s in text13.Split(',')
					select int.Parse(s)).ToArray();
				if (array23.Length < 1 || array23.Length > 8 || array23.Distinct().Count() != array23.Length)
				{
					throw new InvalidOperationException("Expected 1 to 8 owned heroes");
				}
				Dictionary<int, CachedHeroInfo> dictionary6 = new Dictionary<int, CachedHeroInfo>();
				int[] array21 = array23;
				foreach (int num16 in array21)
				{
					CachedHeroInfo heroInfo2 = accountManager3.GetHeroInfo(num16);
					if (heroInfo2 == null)
					{
						throw new InvalidOperationException("Hero not owned");
					}
					dictionary6[num16] = heroInfo2;
				}
				BattleSimulationInstance battleSimulationInstance = new BattleSimulationInstance();
				battleSimulationInstance.Init("automation-diagnostic", new BattleSimulationInfo
				{
					ChapterIndex = num13,
					DungeonIndex = num14,
					DungeonDifficulty = num15,
					ExtraData = new BattleInstanceExtraData
					{
						InfinityLevel = 1
					},
					AccountInfos = new BattleSimulationAccountInfo[1]
					{
						new BattleSimulationAccountInfo
						{
							UserInfo = new CachedUserInfo
							{
								AccountId = accountManager3.UserInfo.AccountId
							},
							HeroInfos = dictionary6,
							AccountBuffs = accountManager3.GetActiveAccountBuffDataBaseList().ToArray()
						}
					}
				});
				try
				{
					battleSimulationInstance.BattleStart();
					long num17 = 0L;
					bool flag2 = false;
					List<object> list8 = new List<object>();
					for (int num18 = 0; num18 < 30000; num18++)
					{
						if (battleSimulationInstance.IsBattleEnd)
						{
							break;
						}
						battleSimulationInstance.Update();
						BattleInstance battleInstance = battleSimulationInstance.BattleInstance;
						long currentTimeMs = battleInstance.GlobalTimer.GetCurrentTimeMs();
						if (!flag2 && battleInstance.KarmaManager.Enabled && battleInstance.CreatureContainer.GetAllCreaturesOfTeam(0).Count > 0)
						{
							battleInstance.KarmaManager.InitFilterClassTag();
							battleInstance.KarmaManager.InitSelectableCardList();
							flag2 = true;
						}
						if (currentTimeMs >= num17)
						{
							num17 = currentTimeMs + 1000;
							List<object> list9 = new List<object>();
							foreach (BattleCreature allCreature in battleInstance.CreatureContainer.GetAllCreatures())
							{
								Dictionary<string, object> dictionary7 = new Dictionary<string, object>();
								StatType[] array24 = CombatStatTypes();
								for (int i = 0; i < array24.Length; i++)
								{
									StatType statType = array24[i];
									dictionary7[statType.ToString()] = allCreature.StatController.GetFinalStat(statType);
								}
								list9.Add(new Dictionary<string, object>
								{
									{ "hero", allCreature.CreatureIndex },
									{
										"level",
										allCreature.CreateInfo.HeroInfo.Level
									},
									{
										"hp",
										allCreature.StatController.Hp
									},
									{ "stats", dictionary7 },
									{
										"initialStats",
										CombatStats(allCreature.CreateInfo.StatData)
									},
									{
										"damage",
										battleInstance.Snapshot.StatisticsManager.TotalDamageMeter(allCreature.Uid)
									}
								});
							}
							list8.Add(new Dictionary<string, object>
							{
								{ "timeMs", currentTimeMs },
								{ "creatures", list9 }
							});
						}
						if (currentTimeMs >= 60000)
						{
							break;
						}
					}
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "combatSimulation" },
						{ "chapter", num13 },
						{ "dungeon", num14 },
						{ "difficulty", num15 },
						{ "ended", battleSimulationInstance.IsBattleEnd },
						{
							"win",
							battleSimulationInstance.IsWin()
						},
						{
							"timeMs",
							battleSimulationInstance.GetPurePlayTime()
						},
						{ "samples", list8 }
					});
				}
				finally
				{
					battleSimulationInstance.Stop();
					battleSimulationInstance.Clear();
				}
			}
			if (command.action == "combat-probe")
			{
				foreach (CachedHeroInfo heroInfo4 in GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true))
				{
					StatData equippedHeroStat = StatCalculator.GetEquippedHeroStat(heroInfo4);
					List<object> list10 = new List<object>();
					foreach (KeyValuePair<int, EquipItemInfo> item14 in heroInfo4.GetSlotIndexEquipItemPair())
					{
						list10.Add(new Dictionary<string, object>
						{
							{ "slot", item14.Key },
							{
								"item",
								item14.Value.ItemIndex
							},
							{
								"star",
								item14.Value.Star
							}
						});
					}
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "heroStats" },
						{ "hero", heroInfo4.HeroIndex },
						{ "level", heroInfo4.Level },
						{ "star", heroInfo4.Star },
						{ "transcended", heroInfo4.Transcended },
						{
							"stats",
							CombatStats(equippedHeroStat)
						},
						{ "equipment", list10 }
					});
				}
				if (SingletonAuto<BattleFacade>.isValidInstance)
				{
					string diagnosticFile = Path.Combine(root, "combat-thread.json");
					SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance running)
					{
						try
						{
							List<object> list12 = new List<object>();
							foreach (BattleCreature allCreature2 in running.CreatureContainer.GetAllCreatures())
							{
								Dictionary<string, object> dictionary11 = new Dictionary<string, object>();
								StatType[] array42 = CombatStatTypes();
								for (int j = 0; j < array42.Length; j++)
								{
									StatType statType2 = array42[j];
									dictionary11[statType2.ToString()] = allCreature2.StatController.GetFinalStat(statType2);
								}
								list12.Add(new Dictionary<string, object>
								{
									{ "hero", allCreature2.CreatureIndex },
									{
										"tier",
										allCreature2.CreateInfo.Tier
									},
									{
										"level",
										allCreature2.CreateInfo.HeroInfo.Level
									},
									{
										"hp",
										allCreature2.StatController.Hp
									},
									{
										"resting",
										allCreature2.CreateInfo.IsInitialResting
									},
									{
										"isAi",
										allCreature2.CreateInfo.IsAi
									},
									{
										"initialStats",
										CombatStats(allCreature2.CreateInfo.StatData)
									},
									{ "stats", dictionary11 }
								});
							}
							File.WriteAllText(diagnosticFile, Json.Serialize(new Dictionary<string, object>
							{
								{ "chapter", running.ChapterIndex },
								{ "dungeon", running.DungeonIndex },
								{
									"timeMs",
									running.GlobalTimer.GetCurrentTimeMs()
								},
								{ "creatures", list12 }
							}));
						}
						catch (Exception ex10)
						{
							File.WriteAllText(diagnosticFile, Json.Serialize(new Dictionary<string, object> { 
							{
								"error",
								ex10.ToString()
							} }));
						}
					});
					BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "replay" },
						{ "ended", battleCache.IsBattleEnded },
						{ "wave", battleCache.WaveIndex }
					});
					BindingFlags bindingAttr4 = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
					MonoBehaviour[] array25 = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
					foreach (MonoBehaviour monoBehaviour in array25)
					{
						if (monoBehaviour == null || (monoBehaviour.GetType().Name != "EndCampaign" && monoBehaviour.GetType().Name != "EndBattleWin"))
						{
							continue;
						}
						Dictionary<string, object> dictionary8 = new Dictionary<string, object>
						{
							{
								"kind",
								monoBehaviour.GetType().Name
							},
							{
								"active",
								monoBehaviour.gameObject.activeInHierarchy
							}
						};
						FieldInfo[] fields = monoBehaviour.GetType().GetFields(bindingAttr4);
						foreach (FieldInfo fieldInfo in fields)
						{
							if (fieldInfo.Name == "_endCampaignProxy")
							{
								dictionary8[fieldInfo.Name] = fieldInfo.GetValue(monoBehaviour) != null;
							}
							else if (fieldInfo.FieldType == typeof(bool) || fieldInfo.FieldType == typeof(float))
							{
								dictionary8[fieldInfo.Name] = fieldInfo.GetValue(monoBehaviour);
							}
						}
						reply.materials.Add(dictionary8);
					}
				}
				MethodInfo method6 = typeof(StatController).GetMethod("CalculateDamage", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				MethodInfo method7 = typeof(StatController).GetMethod("CalculateDamage_Season1", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				long[] array26 = new long[5] { 10000L, 100000L, 500000L, 1000000L, 5000000L };
				foreach (long num19 in array26)
				{
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "formula" },
						{ "defense", num19 },
						{
							"current",
							method6.Invoke(null, new object[4] { 1000000L, num19, 100, 100 })
						},
						{
							"legacy",
							method7.Invoke(null, new object[4] { 1000000L, num19, 100, 100 })
						}
					});
				}
			}
			if (command.action == "skill-probe")
			{
				foreach (CachedHeroInfo heroInfo5 in GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true))
				{
					if (heroInfo5 != null)
					{
						GameRoot.Instance.CreatureDataContainer.GetData(heroInfo5.HeroIndex);
						List<object> list11 = new List<object>();
						for (int num20 = 1; num20 <= 5; num20++)
						{
							Helper.SkillDetailInfo skillDetailInfo = Helper.GetSkillDetailInfo(heroInfo5, num20);
							list11.Add(new Dictionary<string, object>
							{
								{ "slot", num20 },
								{
									"index",
									skillDetailInfo.BasicInfo.TableIndex
								},
								{
									"extend",
									heroInfo5.GetSkillExtend(num20)
								},
								{
									"canEnhance",
									Helper.CanSkillExtend(skillDetailInfo)
								}
							});
						}
						HeroInfo heroInfo3 = JM_NShared_HeroInfo.Parse(JM_NShared_HeroInfo.EncodeJsonString(heroInfo5));
						heroInfo3.SkillExtend1 = 0;
						reply.materials.Add(new Dictionary<string, object>
						{
							{ "kind", "skill" },
							{ "hero", heroInfo5.HeroIndex },
							{
								"canEnhance",
								Helper.CanAnySkillExtend(heroInfo5)
							},
							{
								"unmaxedReminder",
								Helper.CanAnySkillExtend(heroInfo3)
							},
							{ "slots", list11 }
						});
					}
				}
			}
			if (command.action == "input-focus" || command.action == "focus-input")
			{
				UIInput selection = UIInput.selection;
				Type type7 = typeof(LobbyScene2).Assembly.GetType("RestoredPortal");
				MethodInfo methodInfo3 = ((type7 == null) ? null : type7.GetMethod("MenuShortcutKeyUp"));
				reply.materials.Add(new Dictionary<string, object>
				{
					{ "kind", "inputFocus" },
					{
						"selected",
						(selection == null) ? null : Hierarchy(selection.transform)
					},
					{
						"active",
						selection != null && selection.isActiveAndEnabled
					},
					{
						"cameraFocus",
						UICamera.inputHasFocus
					},
					{
						"heroesKey",
						(methodInfo3 == null) ? null : methodInfo3.Invoke(null, new object[1] { KeyCode.H })
					},
					{
						"inventoryKey",
						(methodInfo3 == null) ? null : methodInfo3.Invoke(null, new object[1] { KeyCode.B })
					},
					{
						"shopKey",
						(methodInfo3 == null) ? null : methodInfo3.Invoke(null, new object[1] { KeyCode.P })
					}
				});
			}
			if (command.action == "perk-popup")
			{
				LearnTranscendSkillMessagePopup[] array27 = UnityEngine.Object.FindObjectsOfType<LearnTranscendSkillMessagePopup>();
				foreach (LearnTranscendSkillMessagePopup learnTranscendSkillMessagePopup2 in array27)
				{
					UILabel label_SkillDesc = learnTranscendSkillMessagePopup2.Label_SkillDesc;
					UIScrollView componentInParent = label_SkillDesc.GetComponentInParent<UIScrollView>();
					FieldInfo field3 = typeof(UILabel).GetField("mPrintedSize", BindingFlags.Instance | BindingFlags.NonPublic);
					reply.materials.Add(new Dictionary<string, object>
					{
						{
							"title",
							learnTranscendSkillMessagePopup2.Label_SkillName.text
						},
						{ "text", label_SkillDesc.text },
						{ "fontSize", label_SkillDesc.fontSize },
						{
							"printedFontSize",
							field3.GetValue(label_SkillDesc)
						},
						{ "width", label_SkillDesc.width },
						{ "height", label_SkillDesc.height },
						{
							"overflow",
							label_SkillDesc.overflowMethod.ToString()
						},
						{
							"scroll",
							componentInParent != null
						},
						{
							"canScroll",
							componentInParent != null && componentInParent.shouldMoveVertically
						},
						{
							"labelY",
							label_SkillDesc.transform.localPosition.y
						},
						{
							"viewportY",
							(componentInParent == null) ? 0f : componentInParent.transform.localPosition.y
						},
						{
							"drag",
							label_SkillDesc.GetComponent<UIDragScrollView>() != null
						},
						{
							"warning",
							learnTranscendSkillMessagePopup2.Label_Warning.gameObject.activeInHierarchy
						}
					});
				}
			}
			if (command.action == "inn-scene")
			{
				BindingFlags bindingAttr5 = BindingFlags.Instance | BindingFlags.NonPublic;
				HeroInnView[] array28 = UnityEngine.Object.FindObjectsOfType<HeroInnView>();
				foreach (HeroInnView heroInnView in array28)
				{
					object value8 = typeof(HeroRecruitView).GetField("_renderFacade", bindingAttr5).GetValue(heroInnView);
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "inn" },
						{
							"path",
							Hierarchy(heroInnView.transform)
						},
						{ "hero", heroInnView.HeroIndex },
						{
							"hasInfo",
							heroInnView.HeroInfo != null
						},
						{
							"hasModel",
							value8 != null
						},
						{
							"state",
							heroInnView.WindowState.ToString()
						},
						{ "reserved", heroInnView.ReservedOpenTask }
					});
				}
				HeroInnManagement[] array29 = UnityEngine.Object.FindObjectsOfType<HeroInnManagement>();
				foreach (HeroInnManagement heroInnManagement in array29)
				{
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "manager" },
						{
							"refresh",
							heroInnManagement.GetType().GetField("_requestRefresh", bindingAttr5).GetValue(heroInnManagement)
						}
					});
				}
			}
			if (command.action == "guild-attendance-probe")
			{
				AccountManager accountManager4 = GlobalAutoBehaviour<AccountManager>.instance;
				BindingFlags bindingAttr6 = BindingFlags.Instance | BindingFlags.NonPublic;
				NShared.Login.Response response = typeof(AccountManager).GetField("loginResponse", bindingAttr6).GetValue(accountManager4) as NShared.Login.Response;
				reply.materials.Add(new Dictionary<string, object>
				{
					{ "kind", "loginAttendance" },
					{
						"wireDays",
						(response == null) ? null : ((object)response.MiscInfo.LoginDailyCount)
					},
					{
						"days",
						accountManager4.UserManager.MiscInfo.LoginDailyCount
					}
				});
				GuildManager guildManager = GlobalAutoBehaviour<GuildManager>.instance;
				GuildMemberAttendanceInfo guildMemberAttendanceInfo = guildManager.GuildMemberAttendanceInfo;
				reply.materials.Add(new Dictionary<string, object>
				{
					{ "kind", "guildAttendance" },
					{ "inGuild", guildManager.CheckIsGuild },
					{ "today", guildManager.GuildAttendanceToday },
					{ "days", guildManager.GuildAttendanceDayList },
					{
						"count",
						guildMemberAttendanceInfo?.SuccessiveCount ?? 0
					},
					{
						"rewardStep",
						guildMemberAttendanceInfo?.Successive ?? 0
					},
					{
						"updatedTime",
						guildMemberAttendanceInfo?.UpdatedTime
					},
					{
						"lastAttendance",
						guildMemberAttendanceInfo?.LastAttendanceTime
					}
				});
				GuildAttendanceComponent[] array30 = UnityEngine.Object.FindObjectsOfType<GuildAttendanceComponent>();
				foreach (GuildAttendanceComponent guildAttendanceComponent in array30)
				{
					BindingFlags bindingAttr7 = BindingFlags.Instance | BindingFlags.NonPublic;
					Component component12 = guildAttendanceComponent.GetType().GetField("_todayDailyAttendanceRewardComponent", bindingAttr7).GetValue(guildAttendanceComponent) as Component;
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "guildAttendanceCard" },
						{
							"today",
							guildAttendanceComponent.GetType().GetField("_today", bindingAttr7).GetValue(guildAttendanceComponent)
						},
						{
							"claimable",
							component12 != null
						},
						{
							"todayCardPath",
							(component12 == null) ? null : Hierarchy(component12.transform)
						},
						{
							"countLabel",
							(guildAttendanceComponent.PerfectAttendanceCountLabel == null) ? null : guildAttendanceComponent.PerfectAttendanceCountLabel.text
						}
					});
				}
			}
			if (command.action == "payment-probe")
			{
				AccountManager accountManager5 = GlobalAutoBehaviour<AccountManager>.instance;
				IAPWrapper iAPWrapper = SingletonAuto<IAPWrapper>.instance;
				reply.materials.Add(new Dictionary<string, object>
				{
					{ "kind", "billing" },
					{
						"initialized",
						iAPWrapper.IAPManager != null
					},
					{ "ready", iAPWrapper.IsReady },
					{
						"loginReady",
						StateBase_InAppBilling.IsReady
					}
				});
				string[] files = new string[5] { "None", "GoogleId", "AppleId", "OneStoreId", "UDPStoreId" };
				foreach (string text14 in files)
				{
					NewPayShopProductInfo newPayShopProductInfo = new NewPayShopProductInfo
					{
						Index = -1,
						ProductType = ProductType.Gem,
						PriceType = CurrencyType.Gem,
						Price = 10L
					};
					if (text14 != "None")
					{
						typeof(NewPayShopProductInfo).GetProperty(text14).SetValue(newPayShopProductInfo, "diagnostic.cash", null);
					}
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "purchasePolicy" },
						{ "provider", text14 },
						{
							"buyable",
							PayShopHelper.IsBuyable(newPayShopProductInfo)
						}
					});
				}
				NewPayShopProductInfo[] values = accountManager5.NewPayShopProductManager.GetValues();
				foreach (NewPayShopProductInfo newPayShopProductInfo2 in values)
				{
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "product" },
						{ "index", newPayShopProductInfo2.Index },
						{ "category", newPayShopProductInfo2.Category },
						{
							"cash",
							!string.IsNullOrEmpty(newPayShopProductInfo2.InAppPurchaseId)
						},
						{
							"currency",
							newPayShopProductInfo2.PriceType.ToString()
						},
						{ "price", newPayShopProductInfo2.Price },
						{
							"buyable",
							PayShopHelper.IsBuyable(newPayShopProductInfo2)
						}
					});
				}
				PayShopProductComponent[] array31 = UnityEngine.Object.FindObjectsOfType<PayShopProductComponent>();
				foreach (PayShopProductComponent payShopProductComponent in array31)
				{
					if (payShopProductComponent.ProductInfo != null)
					{
						reply.materials.Add(new Dictionary<string, object>
						{
							{ "kind", "shopCard" },
							{
								"index",
								payShopProductComponent.ProductInfo.Index
							},
							{
								"cash",
								!string.IsNullOrEmpty(payShopProductComponent.ProductInfo.InAppPurchaseId)
							},
							{
								"buyEnabled",
								payShopProductComponent.Button_Buy != null && payShopProductComponent.Button_Buy.isEnabled
							}
						});
					}
				}
			}
			if (command.action == "lobby-probe")
			{
				AccountManager accountManager6 = GlobalAutoBehaviour<AccountManager>.instance;
				BindingFlags bindingAttr8 = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				LobbyTopmostMenu[] array32 = UnityEngine.Object.FindObjectsOfType<LobbyTopmostMenu>();
				foreach (LobbyTopmostMenu lobbyTopmostMenu in array32)
				{
					try
					{
						lobbyTopmostMenu.UpdateDerived();
					}
					catch (Exception innerException)
					{
						throw new InvalidOperationException("Lobby HUD refresh failed", innerException);
					}
					Dictionary<string, object> dictionary9 = new Dictionary<string, object>
					{
						{ "kind", "lobbyHud" },
						{
							"path",
							Hierarchy(lobbyTopmostMenu.transform)
						}
					};
					FieldInfo[] fields = lobbyTopmostMenu.GetType().GetFields(bindingAttr8);
					foreach (FieldInfo fieldInfo2 in fields)
					{
						if (fieldInfo2.FieldType.IsEnum || fieldInfo2.FieldType == typeof(int) || fieldInfo2.FieldType == typeof(bool))
						{
							dictionary9[fieldInfo2.Name] = Convert.ToString(fieldInfo2.GetValue(lobbyTopmostMenu));
						}
					}
					reply.materials.Add(dictionary9);
				}
				NShared.Login.Response response2 = typeof(AccountManager).GetField("loginResponse", bindingAttr8).GetValue(accountManager6) as NShared.Login.Response;
				reply.materials.Add(new Dictionary<string, object>
				{
					{ "kind", "loginProgress" },
					{
						"dungeons",
						(response2 != null) ? response2.ChapterDungeons.Length : 0
					},
					{
						"isLogined",
						AccountManager.IsLogined()
					},
					{ "loginResponseComplete", accountManager6.LoginResponseComplete },
					{
						"currentStamina",
						accountManager6.UserManager.Stamina
					},
					{
						"teamLevel",
						accountManager6.UserManager.TeamLevel
					}
				});
				PortalContentsComponent[] array33 = UnityEngine.Object.FindObjectsOfType<PortalContentsComponent>();
				foreach (PortalContentsComponent portalContentsComponent in array33)
				{
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "portalContents" },
						{
							"path",
							Hierarchy(portalContentsComponent.transform)
						},
						{
							"category",
							portalContentsComponent.Categry.ToString()
						},
						{
							"lockedState",
							portalContentsComponent.LockedState.ToString()
						}
					});
				}
				ChapterMarkerComponent[] array34 = UnityEngine.Object.FindObjectsOfType<ChapterMarkerComponent>();
				foreach (ChapterMarkerComponent chapterMarkerComponent in array34)
				{
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "portalChapter" },
						{ "chapter", chapterMarkerComponent.ChapterIndex },
						{ "locked", chapterMarkerComponent.IsLocked }
					});
				}
				ContentsMenuComponent[] array35 = UnityEngine.Object.FindObjectsOfType<ContentsMenuComponent>();
				foreach (ContentsMenuComponent contentsMenuComponent in array35)
				{
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "portalMenu" },
						{
							"category",
							contentsMenuComponent.Category.ToString()
						},
						{
							"lockedState",
							contentsMenuComponent.LockedState.ToString()
						}
					});
				}
			}
			if (command.action == "dispatch-probe")
			{
				Type type8 = AppDomain.CurrentDomain.GetAssemblies().First((Assembly a) => a.GetName().Name == "SprkDispatch").GetType("SprkDispatch");
				BindingFlags bindingAttr9 = BindingFlags.Static | BindingFlags.NonPublic;
				reply.materials.Add(new Dictionary<string, object>
				{
					{
						"location",
						type8.Assembly.Location
					},
					{
						"ready",
						type8.GetMethod("Ready", bindingAttr9).Invoke(null, null)
					},
					{
						"time",
						Time.realtimeSinceStartup
					},
					{
						"frame",
						Time.frameCount
					},
					{
						"lastFrame",
						type8.GetField("lastFrame", bindingAttr9).GetValue(null)
					},
					{
						"lastStatus",
						type8.GetField("LastStatus").GetValue(type8.GetField("instance", bindingAttr9).GetValue(null))
					},
					{
						"lastError",
						type8.GetField("LastError").GetValue(type8.GetField("instance", bindingAttr9).GetValue(null))
					},
					{
						"host",
						SingletonUpdater<WebServiceManager>.instance.Requester.Host
					}
				});
			}
			if (command.action == "soul-scene")
			{
				SoulStoneRestoreWindow[] array36 = UnityEngine.Object.FindObjectsOfType<SoulStoneRestoreWindow>();
				foreach (SoulStoneRestoreWindow soulStoneRestoreWindow in array36)
				{
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "SoulJudgmentScene" },
						{
							"state",
							soulStoneRestoreWindow.GetType().GetField("_nodeTreeMainState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(soulStoneRestoreWindow)
								.ToString()
						},
						{
							"pack",
							soulStoneRestoreWindow.BgNodeTreeUIPlayer.NodeTreePackCode
						},
						{
							"tree",
							soulStoneRestoreWindow.BgNodeTreeUIPlayer.NodeTreeName
						}
					});
					SoulStoneRestoreWindow.NodeTreeStock[] nodeTreeStocks = soulStoneRestoreWindow.NodeTreeStocks;
					foreach (SoulStoneRestoreWindow.NodeTreeStock nodeTreeStock in nodeTreeStocks)
					{
						reply.materials.Add(new Dictionary<string, object>
						{
							{ "kind", "SoulJudgmentStock" },
							{
								"state",
								nodeTreeStock.State.ToString()
							},
							{ "pack", nodeTreeStock.PackCode },
							{ "tree", nodeTreeStock.TreeName },
							{ "once", nodeTreeStock.IsPlayOnce }
						});
					}
				}
			}
			if (command.action == "renderers")
			{
				LogoScene[] array37 = UnityEngine.Object.FindObjectsOfType<LogoScene>();
				foreach (LogoScene logoScene in array37)
				{
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "startupLogo" },
						{
							"path",
							Hierarchy(logoScene.transform)
						},
						{
							"played",
							LogoScene.IsLogoPlayed
						},
						{
							"alpha",
							(logoScene.LogoWidget == null) ? (-1f) : logoScene.LogoWidget.alpha
						}
					});
				}
				UIWidget[] componentsInChildren4 = UnityEngine.Object.FindObjectsOfType<UIWidget>();
				foreach (UIWidget uIWidget2 in componentsInChildren4)
				{
					if (Hierarchy(uIWidget2.transform).Contains("Login") || !(uIWidget2.color.r + uIWidget2.color.g + uIWidget2.color.b > 0.01f))
					{
						UITexture uITexture = uIWidget2 as UITexture;
						UISprite uISprite = uIWidget2 as UISprite;
						reply.materials.Add(new Dictionary<string, object>
						{
							{ "kind", "loginWidget" },
							{
								"path",
								Hierarchy(uIWidget2.transform)
							},
							{
								"color",
								uIWidget2.color.ToString()
							},
							{ "alpha", uIWidget2.finalAlpha },
							{ "depth", uIWidget2.depth },
							{
								"size",
								uIWidget2.width + "x" + uIWidget2.height
							},
							{
								"texture",
								(uITexture == null || uITexture.mainTexture == null) ? null : uITexture.mainTexture.name
							},
							{
								"sprite",
								(uISprite == null) ? null : uISprite.spriteName
							},
							{
								"panel",
								(uIWidget2.panel == null) ? null : Hierarchy(uIWidget2.panel.transform)
							}
						});
					}
				}
				UIPanel[] componentsInChildren3 = UnityEngine.Object.FindObjectsOfType<UIPanel>();
				foreach (UIPanel uIPanel2 in componentsInChildren3)
				{
					if (Hierarchy(uIPanel2.transform).Contains("LoginBackground"))
					{
						reply.materials.Add(new Dictionary<string, object>
						{
							{ "kind", "loginPanel" },
							{
								"path",
								Hierarchy(uIPanel2.transform)
							},
							{ "alpha", uIPanel2.alpha },
							{ "depth", uIPanel2.depth },
							{ "queue", uIPanel2.startingRenderQueue },
							{
								"mode",
								uIPanel2.renderQueue.ToString()
							}
						});
					}
				}
				reply.materials.Add(new Dictionary<string, object>
				{
					{ "kind", "shaderGlobals" },
					{
						"outline",
						Shader.GetGlobalFloat("_Outline")
					},
					{
						"outlineFactor",
						Shader.GetGlobalFloat("_OutlineFactor")
					},
					{
						"shadowDirection",
						Shader.GetGlobalVector("_ShadowDirection").ToString()
					},
					{
						"nativeUpdater",
						GlobalAutoBehaviour<ShaderVariableUpdater>.isValidInstance
					},
					{
						"shaderLOD",
						Shader.globalMaximumLOD
					},
					{
						"reversedZ",
						SystemInfo.usesReversedZBuffer
					},
					{
						"graphics",
						SystemInfo.graphicsDeviceType.ToString()
					}
				});
				Camera[] array38 = UnityEngine.Object.FindObjectsOfType<Camera>();
				foreach (Camera camera in array38)
				{
					RenderTexture targetTexture = camera.targetTexture;
					PostProcessLayer component13 = camera.GetComponent<PostProcessLayer>();
					PostProcessVolume component14 = camera.GetComponent<PostProcessVolume>();
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "camera" },
						{
							"path",
							Hierarchy(camera.transform)
						},
						{ "mask", camera.cullingMask },
						{ "near", camera.nearClipPlane },
						{ "far", camera.farClipPlane },
						{ "depth", camera.depth },
						{ "orthographic", camera.orthographic },
						{
							"position",
							camera.transform.position.ToString()
						},
						{ "enabled", camera.isActiveAndEnabled },
						{
							"clear",
							camera.clearFlags.ToString()
						},
						{
							"target",
							(targetTexture == null) ? "screen" : (targetTexture.name + " " + targetTexture.width + "x" + targetTexture.height + " aa" + targetTexture.antiAliasing)
						},
						{
							"postLayer",
							(component13 == null) ? "none" : (component13.enabled ? component13.antialiasingMode.ToString() : "disabled")
						},
						{
							"postVolume",
							(component14 == null) ? "none" : (component14.enabled ? ((component14.profile == null) ? "null" : component14.profile.name) : "disabled")
						},
						{
							"depthTexture",
							camera.depthTextureMode.ToString()
						}
					});
				}
				FrameTiming[] array39 = new FrameTiming[30];
				FrameTimingManager.CaptureFrameTimings();
				uint latestTimings = FrameTimingManager.GetLatestTimings((uint)array39.Length, array39);
				double num21 = 0.0;
				double num22 = 0.0;
				for (int num23 = 0; num23 < latestTimings; num23++)
				{
					num21 += array39[num23].gpuFrameTime;
					num22 += array39[num23].cpuFrameTime;
				}
				reply.materials.Add(new Dictionary<string, object>
				{
					{ "kind", "frame" },
					{
						"screen",
						Screen.width + "x" + Screen.height
					},
					{
						"targetFps",
						Application.targetFrameRate
					},
					{
						"vSync",
						QualitySettings.vSyncCount
					},
					{ "samples", latestTimings },
					{
						"gpuMs",
						(latestTimings == 0) ? 0.0 : (num21 / (double)latestTimings)
					},
					{
						"cpuMs",
						(latestTimings == 0) ? 0.0 : (num22 / (double)latestTimings)
					},
					{
						"deltaMs",
						Time.unscaledDeltaTime * 1000f
					}
				});
				Renderer[] array40 = UnityEngine.Object.FindObjectsOfType<Renderer>();
				foreach (Renderer renderer in array40)
				{
					if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
					{
						continue;
					}
					Material[] sharedMaterials = renderer.sharedMaterials;
					foreach (Material material in sharedMaterials)
					{
						Shader shader = ((material == null) ? null : material.shader);
						Dictionary<string, object> dictionary10 = new Dictionary<string, object>();
						MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
						renderer.GetPropertyBlock(materialPropertyBlock);
						dictionary10["shaderTransformEnabled"] = materialPropertyBlock.GetFloat("_ShaderTransform_IsEnable");
						dictionary10["shaderTransform"] = materialPropertyBlock.GetMatrix("_ShaderTransform_NormalTransform").ToString();
						SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
						MeshFilter component15 = renderer.GetComponent<MeshFilter>();
						Mesh mesh = ((skinnedMeshRenderer != null) ? skinnedMeshRenderer.sharedMesh : ((component15 == null) ? null : component15.sharedMesh));
						dictionary10["mesh"] = ((mesh == null) ? null : mesh.name);
						dictionary10["submeshes"] = ((!(mesh == null)) ? mesh.subMeshCount : 0);
						dictionary10["materials"] = renderer.sharedMaterials.Length;
						dictionary10["layer"] = renderer.gameObject.layer;
						if (material != null)
						{
							string[] files = new string[3] { "_MainTex", "_MainTex_Color", "_NoiseTex_Nor" };
							foreach (string key in files)
							{
								if (material.HasProperty(key))
								{
									Texture texture = material.GetTexture(key);
									dictionary10[key] = ((texture == null) ? null : (texture.name + " " + texture.width + "x" + texture.height));
								}
							}
							files = new string[2] { "_Color", "_TintColor" };
							foreach (string key2 in files)
							{
								if (material.HasProperty(key2))
								{
									dictionary10[key2] = material.GetColor(key2).ToString();
								}
							}
							files = new string[6] { "_ExtraAlpha", "_MainTex_Color_Str", "_MainTex_Color_Pow", "_MainTex_Alpha_Str", "_MainTex_Alpha_Pow", "_Fresnel" };
							foreach (string key3 in files)
							{
								if (material.HasProperty(key3))
								{
									dictionary10[key3] = material.GetFloat(key3);
								}
							}
							dictionary10["keywords"] = material.shaderKeywords;
							dictionary10["queue"] = material.renderQueue;
						}
						reply.materials.Add(new Dictionary<string, object>
						{
							{ "kind", "renderer" },
							{
								"id",
								renderer.GetInstanceID()
							},
							{
								"renderer",
								Hierarchy(renderer.transform)
							},
							{
								"material",
								(material == null) ? null : material.name
							},
							{
								"shader",
								(shader == null) ? null : shader.name
							},
							{
								"supported",
								shader != null && shader.isSupported
							},
							{ "visible", renderer.isVisible },
							{ "properties", dictionary10 }
						});
					}
				}
			}
			if (command.action == "probe-renderer")
			{
				Renderer renderer2 = null;
				Renderer[] array40 = UnityEngine.Object.FindObjectsOfType<Renderer>();
				foreach (Renderer renderer3 in array40)
				{
					if (renderer3.enabled && renderer3.gameObject.activeInHierarchy && Hierarchy(renderer3.transform) == command.text)
					{
						if (renderer2 != null)
						{
							throw new InvalidOperationException("Ambiguous renderer path");
						}
						renderer2 = renderer3;
					}
				}
				if (renderer2 == null)
				{
					throw new InvalidOperationException("Active renderer not found");
				}
				busy = true;
				StartCoroutine(Capture(reply, renderer2));
				return;
			}
			if (command.action == "party-ui")
			{
				BattlePartySetting[] array15 = UnityEngine.Object.FindObjectsOfType<BattlePartySetting>();
				foreach (BattlePartySetting battlePartySetting2 in array15)
				{
					if (!battlePartySetting2.gameObject.activeInHierarchy)
					{
						continue;
					}
					RaidPartySetting raidPartySetting = battlePartySetting2 as RaidPartySetting;
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "kind", "raid" },
						{
							"path",
							Hierarchy(battlePartySetting2.transform)
						},
						{
							"index",
							(!(raidPartySetting == null) && raidPartySetting.RaidData != null) ? raidPartySetting.RaidData.Index : 0
						},
						{
							"minimumLevel",
							(!(raidPartySetting == null) && raidPartySetting.RaidData != null) ? raidPartySetting.RaidData.ReqHeroLevel : 0
						}
					});
					HeroPortraitComponent[] componentsInChildren5 = battlePartySetting2.GetComponentsInChildren<HeroPortraitComponent>(includeInactive: true);
					foreach (HeroPortraitComponent heroPortraitComponent in componentsInChildren5)
					{
						if (heroPortraitComponent.HeroInfo != null)
						{
							reply.materials.Add(new Dictionary<string, object>
							{
								{ "kind", "hero" },
								{
									"id",
									heroPortraitComponent.gameObject.GetInstanceID()
								},
								{
									"path",
									Hierarchy(heroPortraitComponent.transform)
								},
								{
									"hero",
									heroPortraitComponent.HeroInfo.HeroIndex
								},
								{
									"level",
									heroPortraitComponent.HeroInfo.Level
								},
								{
									"active",
									heroPortraitComponent.gameObject.activeInHierarchy
								}
							});
						}
					}
					UISprite[] componentsInChildren6 = battlePartySetting2.GetComponentsInChildren<UISprite>(includeInactive: true);
					foreach (UISprite uISprite2 in componentsInChildren6)
					{
						if (uISprite2.name.Contains("Frame") || uISprite2.name == "Sprite_Top")
						{
							reply.materials.Add(new Dictionary<string, object>
							{
								{ "kind", "frame" },
								{
									"path",
									Hierarchy(uISprite2.transform)
								},
								{ "sprite", uISprite2.spriteName },
								{
									"active",
									uISprite2.gameObject.activeInHierarchy
								},
								{ "alpha", uISprite2.alpha }
							});
						}
					}
				}
			}
			if (command.action == "screenshot")
			{
				busy = true;
				StartCoroutine(Capture(reply));
				return;
			}
			goto end_IL_0064;
			IL_237b:
			if (num4)
			{
				goto IL_2380;
			}
			UICamera.MouseOrTouch currentTouch = UICamera.currentTouch;
			Camera currentCamera = UICamera.currentCamera;
			UICamera current8 = UICamera.current;
			int currentTouchID = UICamera.currentTouchID;
			Vector3 lastWorldPosition = UICamera.lastWorldPosition;
			try
			{
				Camera camera2 = (UICamera.currentCamera = NGUITools.FindCameraForLayer(gameObject5.layer));
				UICamera.current = ((camera2 == null) ? null : camera2.GetComponent<UICamera>());
				UICamera.currentTouchID = -1;
				UICamera.currentTouch = new UICamera.MouseOrTouch
				{
					current = gameObject5,
					pressed = gameObject5,
					last = gameObject5
				};
				if (command.action == "drag")
				{
					string[] array41 = (command.text ?? "").Split(',');
					if (array41.Length != 2 || !float.TryParse(array41[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result3) || !float.TryParse(array41[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var result4) || float.IsNaN(result3) || float.IsNaN(result4) || float.IsInfinity(result3) || float.IsInfinity(result4) || Mathf.Abs(result3) > 1000f || Mathf.Abs(result4) > 1000f || camera2 == null)
					{
						throw new InvalidOperationException("Expected a drag delta dx,dy within 1000 pixels");
					}
					Vector2 vector = camera2.WorldToScreenPoint(gameObject5.transform.position);
					UICamera.currentTouch.pos = vector;
					UICamera.lastWorldPosition = gameObject5.transform.position;
					UICamera.Notify(gameObject5, UICamera.ENotifyType.OnPress, true);
					try
					{
						for (int num25 = 1; num25 <= 12; num25++)
						{
							Vector2 vector2 = new Vector2(result3, result4) / 12f;
							UICamera.currentTouch.pos = vector + vector2 * num25;
							UICamera.currentTouch.delta = vector2;
							UICamera.currentTouch.totalDelta = vector2 * num25;
							UICamera.Notify(gameObject5, UICamera.ENotifyType.OnDrag, vector2);
						}
					}
					finally
					{
						UICamera.Notify(gameObject5, UICamera.ENotifyType.OnPress, false);
					}
				}
				else if (command.action == "press")
				{
					UICamera.Notify(gameObject5, UICamera.ENotifyType.OnPress, true);
					UICamera.Notify(gameObject5, UICamera.ENotifyType.OnPress, false);
				}
				else
				{
					UICamera.Notify(gameObject5, UICamera.ENotifyType.OnClick, null);
				}
				if (command.action == "click-and-stop-story" && AutoNextBattle.IsValidInstance)
				{
					AutoNextBattle.instance.OnClickStop();
				}
			}
			finally
			{
				UICamera.currentTouch = currentTouch;
				UICamera.currentCamera = currentCamera;
				UICamera.current = current8;
				UICamera.currentTouchID = currentTouchID;
				UICamera.lastWorldPosition = lastWorldPosition;
			}
			goto IL_3847;
			IL_2380:
			throw new InvalidOperationException("Button is disabled");
			end_IL_0064:;
		}
		catch (Exception ex9)
		{
			reply = new Reply
			{
				id = ((command == null) ? "invalid" : command.id),
				ok = false,
				error = ex9.ToString()
			};
		}
		Write(reply);
		if (command != null && command.action == "quit" && reply.ok)
		{
			Application.Quit();
		}
	}

	private IEnumerator Capture(Reply reply, Renderer hidden = null)
	{
		if (hidden != null)
		{
			hidden.enabled = false;
		}
		yield return new WaitForEndOfFrame();
		Texture2D texture2D = null;
		try
		{
			texture2D = ScreenCapture.CaptureScreenshotAsTexture();
			reply.screenshot = reply.id + ".png";
			File.WriteAllBytes(Path.Combine(root, reply.screenshot), texture2D.EncodeToPNG());
		}
		catch (Exception ex)
		{
			reply.ok = false;
			reply.error = ex.ToString();
		}
		finally
		{
			if (hidden != null)
			{
				hidden.enabled = true;
			}
			if (texture2D != null)
			{
				UnityEngine.Object.Destroy(texture2D);
			}
			busy = false;
		}
		Write(reply);
	}

	private IEnumerator ProbeNativeStop(Command cmd)
	{
		Reply failure = null;
		try
		{
			Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().First((Assembly a) => a.GetName().Name == "SprkStoryAuto");
			Type type = assembly.GetType("SprkStoryAuto");
			BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.NonPublic;
			type.GetField("continuousStory", bindingAttr).SetValue(null, true);
			type.GetField("lastProgress", bindingAttr).SetValue(null, Time.realtimeSinceStartup);
			if (type.GetField("runner", bindingAttr).GetValue(null) == null)
			{
				GameObject gameObject = new GameObject("SprkStoryAuto");
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				type.GetField("runner", bindingAttr).SetValue(null, gameObject.AddComponent(assembly.GetType("SprkStoryAutoRunner")));
			}
			int num = 0;
			AutoNextBattle.OpenGlobal(BattleType.Campaign, DungeonDifficultyType.Normal, num);
		}
		catch (Exception ex)
		{
			failure = new Reply
			{
				id = cmd.id,
				ok = false,
				error = ex.ToString()
			};
		}
		if (failure != null)
		{
			busy = false;
			Write(failure);
			yield break;
		}
		for (int frame = 0; frame < cmd.target; frame++)
		{
			yield return null;
		}
		try
		{
			AutoNextBattle.instance.OnClickStop();
		}
		catch (Exception ex2)
		{
			failure = new Reply
			{
				id = cmd.id,
				ok = false,
				error = ex2.ToString()
			};
		}
		yield return new WaitForSecondsRealtime(1f);
		if (failure == null)
		{
			try
			{
				failure = Snapshot(cmd.id);
			}
			catch (Exception ex3)
			{
				failure = new Reply
				{
					id = cmd.id,
					ok = false,
					error = ex3.ToString()
				};
			}
		}
		busy = false;
		Write(failure);
	}

	private List<object> ProbeEssenceCraft()
	{
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		AccountManager accountManager = GlobalAutoBehaviour<AccountManager>.instance;
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
		object value = accountManager.GetType().GetProperty("AccountManagers", bindingAttr).GetValue(accountManager, null);
		FieldInfo field = value.GetType().GetField("UserManager");
		FieldInfo field2 = value.GetType().GetField("ItemManager");
		object value2 = field.GetValue(value);
		object value3 = field2.GetValue(value);
		List<object> list = new List<object>();
		try
		{
			UserInfo userInfo = new UserInfo
			{
				Gold = 700000000L
			};
			field.SetValue(value, new UserManager(accountManager, userInfo, new PlayerMiscInfo(), new PlayerGuildPointInfo()));
			NGame2.NAccount.UserItemManager userItemManager = new NGame2.NAccount.UserItemManager();
			field2.SetValue(value, userItemManager);
			int[] array = new int[7] { 20101, 20102, 20103, 20104, 20105, 20107, 20106 };
			for (int i = 0; i < array.Length; i++)
			{
				int index = array[i];
				userItemManager.Reset(new ItemInfo[1]
				{
					new ItemInfo
					{
						ItemIndex = 20108,
						Count = 975
					}
				});
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(index);
				CraftItemSelectMessagePopup.Open(delegate
				{
					throw new Exception("Read-only fixture must not craft");
				}, data, 30);
				CraftItemSelectMessagePopup craftItemSelectMessagePopup = UnityEngine.Object.FindObjectsOfType<CraftItemSelectMessagePopup>().Single((CraftItemSelectMessagePopup p) => p.IsOpen());
				if (craftItemSelectMessagePopup.GetNumber() != 30 || !craftItemSelectMessagePopup.MaterialUi.Button_Craft.isEnabled)
				{
					throw new Exception("Essence " + index + ": native quantity=" + craftItemSelectMessagePopup.GetNumber() + ", enabled=" + craftItemSelectMessagePopup.MaterialUi.Button_Craft.isEnabled);
				}
				CraftItemMaterialComponent craftItemMaterialComponent = ((IEnumerable)craftItemSelectMessagePopup.GetType().GetField("_materialItemComponentList", bindingAttr).GetValue(craftItemSelectMessagePopup)).Cast<CraftItemMaterialComponent>().Single((CraftItemMaterialComponent c) => c.GetCurrentItemIndex() == index - 100);
				if (!craftItemMaterialComponent.Label_Count.text.Contains("975/300"))
				{
					throw new Exception("Native essence material balance incorrect: " + craftItemMaterialComponent.Label_Count.text);
				}
				HeroAwakeItemComponent comp = HeroAwakeItemComponent.PrefabAuto.Factory.Create(craftItemSelectMessagePopup.transform);
				MethodInfo method = Type.GetType("SprkHeroMaterialTickets, SprkHeroMaterialTickets", throwOnError: true).GetMethod("Open");
				HeroAwakeItemComponent comp2 = HeroAwakeItemComponent.PrefabAuto.Factory.Create(craftItemSelectMessagePopup.transform);
				ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(index - 100);
				method.Invoke(null, new object[7]
				{
					comp2,
					data2,
					30,
					0,
					false,
					HeroAwakeType.Awaken,
					null
				});
				if (!comp2.Sprite_New.gameObject.activeSelf)
				{
					throw new Exception("Awakening fragment ticket plus icon missing: " + data2.Index);
				}
				if (index == 20101)
				{
					((ItemIconComponent.Listener)comp2).OnClick(comp2.ItemIconComponent);
					SelectionItemUsePopup selectionItemUsePopup = UnityEngine.Object.FindObjectsOfType<SelectionItemUsePopup>().Single((SelectionItemUsePopup p) => p.IsOpen());
					if (selectionItemUsePopup.InputSelectedCount.value != "30" || (int)selectionItemUsePopup.GetType().GetField("_selectedItemIndex", bindingAttr).GetValue(selectionItemUsePopup) != data2.Index)
					{
						throw new Exception("Awakening fragment popup lost quantity or material defaults");
					}
					selectionItemUsePopup.Close();
				}
				userItemManager.Reset(new ItemInfo[1]
				{
					new ItemInfo
					{
						ItemIndex = 20108,
						Count = 1
					}
				});
				comp2.RefreshItemCount(30, 0);
				if (!comp2.Sprite_New.gameObject.activeSelf)
				{
					throw new Exception("One available fragment ticket should show plus");
				}
				userItemManager.Reset(new ItemInfo[1]
				{
					new ItemInfo
					{
						ItemIndex = 20108,
						Count = 975,
						Locked = 1
					}
				});
				comp2.RefreshItemCount(30, 0);
				if (comp2.Sprite_New.gameObject.activeSelf)
				{
					throw new Exception("Locked fragment tickets should hide plus");
				}
				userItemManager.Reset(new ItemInfo[0]);
				comp2.RefreshItemCount(30, 0);
				if (comp2.Sprite_New.gameObject.activeSelf)
				{
					throw new Exception("No fragment tickets should hide plus");
				}
				userItemManager.Reset(new ItemInfo[1]
				{
					new ItemInfo
					{
						ItemIndex = 20108,
						Count = 975
					}
				});
				comp2.RefreshItemCount(30, 30);
				if (comp2.Sprite_New.gameObject.activeSelf)
				{
					throw new Exception("Satisfied fragment requirement should hide plus");
				}
				FactoryBase.SafeReleaseComponent(ref comp2);
				method.Invoke(null, new object[7]
				{
					comp,
					data,
					30,
					0,
					false,
					HeroAwakeType.Transcend,
					null
				});
				if (!comp.Sprite_New.gameObject.activeSelf)
				{
					throw new Exception("Ticket-only essence plus icon missing: " + index);
				}
				userItemManager.Reset(new ItemInfo[1]
				{
					new ItemInfo
					{
						ItemIndex = 20108,
						Count = 9
					}
				});
				comp.RefreshItemCount(30, 0);
				if (comp.Sprite_New.gameObject.activeSelf)
				{
					throw new Exception("Plus visible with insufficient fragment tickets");
				}
				userItemManager.Reset(new ItemInfo[2]
				{
					new ItemInfo
					{
						ItemIndex = 20108,
						Count = 5
					},
					new ItemInfo
					{
						ItemIndex = index - 100,
						Count = 5
					}
				});
				comp.RefreshItemCount(30, 0);
				if (!comp.Sprite_New.gameObject.activeSelf)
				{
					throw new Exception("Mixed fragments/tickets plus icon missing");
				}
				userItemManager.Reset(new ItemInfo[1]
				{
					new ItemInfo
					{
						ItemIndex = 20108,
						Count = 975,
						Locked = 1
					}
				});
				comp.RefreshItemCount(30, 0);
				if (comp.Sprite_New.gameObject.activeSelf)
				{
					throw new Exception("Locked tickets incorrectly enable plus icon");
				}
				userItemManager.Reset(new ItemInfo[1]
				{
					new ItemInfo
					{
						ItemIndex = index - 100,
						Count = 10
					}
				});
				comp.RefreshItemCount(30, 0);
				if (!comp.Sprite_New.gameObject.activeSelf)
				{
					throw new Exception("Loose fragment plus icon regressed");
				}
				comp.RefreshItemCount(30, 30);
				if (comp.Sprite_New.gameObject.activeSelf)
				{
					throw new Exception("Plus visible after essence requirement met");
				}
				FactoryBase.SafeReleaseComponent(ref comp);
				userItemManager.Reset(new ItemInfo[1]
				{
					new ItemInfo
					{
						ItemIndex = 20108,
						Count = 975
					}
				});
				list.Add("PASS: native essence " + index + " quantity=30, fragments=975/300, Craft enabled; essence and fragment plus availability/refresh checks passed" + ((index == 20101) ? "; fragment ticket click/prefill/preselection passed" : ""));
				craftItemSelectMessagePopup.OnClickMax();
				if (craftItemSelectMessagePopup.GetNumber() != 97)
				{
					throw new Exception("Native Max does not count tickets");
				}
				craftItemSelectMessagePopup.Close();
			}
			userItemManager.Reset(new ItemInfo[1]
			{
				new ItemInfo
				{
					ItemIndex = 20108,
					Count = 25
				}
			});
			CraftItemSelectMessagePopup.Open(delegate
			{
			}, GameRoot.Instance.ItemDataContainer.GetData(20106), 30);
			CraftItemSelectMessagePopup craftItemSelectMessagePopup2 = UnityEngine.Object.FindObjectsOfType<CraftItemSelectMessagePopup>().Single((CraftItemSelectMessagePopup p) => p.IsOpen());
			if (craftItemSelectMessagePopup2.GetNumber() != 2)
			{
				throw new Exception("Native prefill did not cap to affordable material quantity");
			}
			list.Add("PASS: native shortage clamp: 25 tickets => quantity 2");
			craftItemSelectMessagePopup2.Close();
			return list;
		}
		finally
		{
			field.SetValue(value, value2);
			field2.SetValue(value, value3);
		}
	}

	private IEnumerator ProbeBattleResultFlask(Command cmd)
	{
		List<object> results = new List<object>();
		Reply failure = null;
		HeroInRewardComponent view = null;
		MethodInfo setup = null;
		CachedHeroInfo hero = null;
		FlaskType[] array = new FlaskType[2]
		{
			FlaskType.ExpFlask,
			FlaskType.CurseRuby
		};
		try
		{
			TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
			GameObject gameObject = new GameObject("FlaskResultFixture");
			view = HeroInRewardComponent.PrefabAuto.Factory.Create(gameObject.transform);
			view.flaskFilledEffects = new HeroInRewardComponent.FlaskFilledEffect[0];
			setup = view.GetType().GetMethod("SetupUI", BindingFlags.Instance | BindingFlags.NonPublic);
			hero = new CachedHeroInfo(new HeroInfo
			{
				HeroIndex = 1,
				Level = 60,
				Star = 5
			});
		}
		catch (Exception ex)
		{
			failure = new Reply
			{
				id = cmd.id,
				ok = false,
				error = ex.ToString()
			};
		}
		if (failure != null)
		{
			busy = false;
			Write(failure);
			yield break;
		}
		FlaskType[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			FlaskType type = array2[i];
			bool[] array3 = new bool[2] { false, true };
			foreach (bool rollover in array3)
			{
				float expected = 0f;
				string gain = null;
				try
				{
					FlaskItemData flaskItemData = GameRoot.Instance.FlaskItemDataContainer.GetAllData().First((FlaskItemData f) => f.Type == type && f.FlaskExp > 4);
					ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(flaskItemData.ItemCode);
					int num = flaskItemData.FlaskExp / 4;
					int num2 = (rollover ? num : (flaskItemData.FlaskExp / 2 + num));
					FlaskEndCampaignResultInfo flaskEndCampaignResultInfo = new FlaskEndCampaignResultInfo
					{
						FilledFlaskCount = (rollover ? 1 : 0),
						Info = new FlaskResultInfo
						{
							HeroIndex = 1,
							ItemIndex = dataByCode.Index,
							AddExp = num,
							NewExp = num2,
							AddExpRatio = 1000
						}
					};
					HeroExpResultInfo heroExpResultInfo = new HeroExpResultInfo
					{
						HeroIndex = 1,
						OldLevel = 60,
						NewLevel = 60,
						NewValue = 0,
						AddValue = 0
					};
					if (!(bool)setup.Invoke(view, new object[6] { hero, false, false, heroExpResultInfo, flaskEndCampaignResultInfo, false }))
					{
						throw new Exception("Native result card setup failed");
					}
					expected = (float)num2 / (float)flaskItemData.FlaskExp;
					gain = view.AddExpLabel.text;
				}
				catch (Exception ex2)
				{
					failure = new Reply
					{
						id = cmd.id,
						ok = false,
						error = ex2.ToString()
					};
				}
				if (failure != null)
				{
					break;
				}
				yield return new WaitForSecondsRealtime(1.6f);
				try
				{
					if (!view.FlaskExpGauge.gameObject.activeSelf || view.ExpGauge.gameObject.activeSelf)
					{
						throw new Exception(type.ToString() + " result switched from flask to hero EXP after animation");
					}
					if (Mathf.Abs(view.FlaskExpGauge.fillAmount - expected) > 0.02f || view.AddExpLabel.text != gain || !view.flaskSelector.gameObject.activeSelf)
					{
						throw new Exception(type.ToString() + " result lost its final flask progress, gain text or flask icon");
					}
					results.Add("PASS: native " + type.ToString() + " " + (rollover ? "rollover" : "partial") + " animation retains flask bar, fill=" + view.FlaskExpGauge.fillAmount + ", gain=" + gain);
				}
				catch (Exception ex3)
				{
					failure = new Reply
					{
						id = cmd.id,
						ok = false,
						error = ex3.ToString()
					};
				}
				if (failure != null)
				{
					break;
				}
			}
		}
		if (failure == null)
		{
			try
			{
				setup.Invoke(view, new object[6]
				{
					hero,
					false,
					false,
					new HeroExpResultInfo
					{
						HeroIndex = 1,
						OldLevel = 60,
						NewLevel = 60
					},
					null,
					false
				});
				if (!view.ExpGauge.gameObject.activeSelf || view.FlaskExpGauge.gameObject.activeSelf || view.flaskSelector.gameObject.activeSelf)
				{
					throw new Exception("Reused non-flask result card did not restore native hero EXP");
				}
				results.Add("PASS: reused card without a flask restores native hero EXP");
			}
			catch (Exception ex4)
			{
				failure = new Reply
				{
					id = cmd.id,
					ok = false,
					error = ex4.ToString()
				};
			}
		}
		if (view != null)
		{
			FactoryBase.SafeReleaseComponent(ref view);
		}
		busy = false;
		if (failure != null)
		{
			Write(failure);
			yield break;
		}
		Reply reply = Snapshot(cmd.id);
		reply.materials.AddRange(results);
		Write(reply);
	}

	private void ProbeInventoryRunes()
	{
		GameObject obj = VespaResource.Load<GameObject>("Resources/Prefab2/Window/Inventory/InvenList");
		if (obj == null)
		{
			throw new Exception("Native inventory prefab missing");
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(obj);
		InvenList component = gameObject.GetComponent<InvenList>();
		InvenList.Category category = Array.Find(component.Categories, (InvenList.Category c) => c.Type == InvenList.EToggle.Rune);
		if (category == null || category.SubCategorys.Length < 2 || category.SubCategorys[0].Toggle == null || category.SubCategorys[1].Toggle == null || !category.SubCategorys[0].ItemTypes.Contains(ItemType.Rune) || !category.SubCategorys[1].ItemTypes.Contains(ItemType.PunishmentRune))
		{
			throw new Exception("Native rune subcategories or item filters missing");
		}
		category.SubCategorys[0].Toggle.gameObject.SetActive(value: false);
		category.SubCategorys[1].Toggle.gameObject.SetActive(value: false);
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
		MethodInfo method = component.GetType().GetMethod("SelectCategory", bindingAttr);
		method.Invoke(component, new object[2]
		{
			InvenList.EToggle.Rune,
			0
		});
		if (!category.SubCategorys[0].Toggle.gameObject.activeSelf || !category.SubCategorys[1].Toggle.gameObject.activeSelf)
		{
			throw new Exception("Rune category button remains hidden");
		}
		method.Invoke(component, new object[2]
		{
			InvenList.EToggle.Rune,
			1
		});
		if ((int)component.GetType().GetField("_subCategoryIndex", bindingAttr).GetValue(component) != 1)
		{
			throw new Exception("Blasphemy rune selection failed");
		}
		gameObject.SetActive(value: false);
	}

	private void CheckAwakeningSortInitialization(HeroEquipAwaken view)
	{
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		Type type = view.GetType();
		string[] array = new string[5] { "ReqLevel", "Grade", "Star", "HeroLevel", "HeroGrade" };
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (Convert.ToInt32(type.GetField("_" + text + "SortType", bindingAttr).GetValue(view)) != 1)
			{
				throw new Exception("Cold awakening sort differs from native close/reset default: " + text);
			}
			FieldInfo field = type.GetField("Sort" + text + "Sprites", bindingAttr);
			object obj = Activator.CreateInstance(field.FieldType);
			string[] array3 = new string[2] { "Up", "Down" };
			foreach (string text2 in array3)
			{
				GameObject gameObject = new GameObject(text + text2);
				gameObject.transform.SetParent(view.transform, worldPositionStays: false);
				obj.GetType().GetField("Sprite" + text2).SetValue(obj, gameObject.AddComponent<UISprite>());
			}
			field.SetValue(view, obj);
		}
		MethodInfo method = type.GetMethod("SetupSortButton", bindingAttr);
		int[] array4 = new int[3] { 1, 0, 1 };
		foreach (int num in array4)
		{
			array2 = array;
			foreach (string text3 in array2)
			{
				FieldInfo field2 = type.GetField("_" + text3 + "SortType", bindingAttr);
				field2.SetValue(view, Enum.ToObject(field2.FieldType, num));
			}
			method.Invoke(view, null);
			array2 = array;
			foreach (string text4 in array2)
			{
				object value = type.GetField("Sort" + text4 + "Sprites", bindingAttr).GetValue(view);
				UISprite obj2 = (UISprite)value.GetType().GetField("SpriteUp").GetValue(value);
				UISprite uISprite = (UISprite)value.GetType().GetField("SpriteDown").GetValue(value);
				if (obj2.gameObject.activeSelf != (num == 0) || uISprite.gameObject.activeSelf != (num == 1))
				{
					throw new Exception("Awakening sort arrow disagrees with state: " + text4);
				}
			}
		}
	}

	private IEnumerator ProbeAwakeningMaterials(Command cmd)
	{
		GameObject host = new GameObject("AwakeningMaterialFixture");
		host.SetActive(value: false);
		GameObject gridRoot = new GameObject("AwakeningMaterialGridFixture");
		Reply failure = null;
		UIGrid grid = null;
		HeroEquipAwaken view = null;
		MethodInfo visibility = null;
		MethodInfo compact = null;
		List<ItemIconComponent> icons = new List<ItemIconComponent>();
		Action<int> check = delegate(int expected)
		{
			List<Transform> childList = grid.GetChildList();
			if (!grid.hideInactive || childList.Count != expected)
			{
				throw new Exception("Hidden awakening materials still occupy grid cells");
			}
			for (int i = 0; i < childList.Count; i++)
			{
				if (Mathf.Abs(childList[i].localPosition.x - (float)(i % 3 * 100)) > 0.1f || Mathf.Abs(childList[i].localPosition.y + (float)(i / 3 * 100)) > 0.1f)
				{
					throw new Exception("Awakening material grid has gaps or overlapping slots");
				}
				if (childList[i].GetComponent<ItemIconComponent>().RootWidget.isGrayscale)
				{
					throw new Exception("Visible awakening material is gray");
				}
			}
		};
		try
		{
			view = host.AddComponent<HeroEquipAwaken>();
			CheckAwakeningSortInitialization(view);
			GameObject gameObject = new GameObject("FreshAwakeningSortFixture");
			gameObject.SetActive(value: false);
			gameObject.transform.SetParent(host.transform, worldPositionStays: false);
			CheckAwakeningSortInitialization(gameObject.AddComponent<HeroEquipAwaken>());
			UIScrollGrid uIScrollGrid = gridRoot.AddComponent<UIScrollGrid>();
			GameObject gameObject2 = new GameObject("Grid");
			gameObject2.transform.SetParent(gridRoot.transform, worldPositionStays: false);
			grid = gameObject2.AddComponent<UIGrid>();
			grid.arrangement = UIGrid.Arrangement.Horizontal;
			grid.maxPerLine = 3;
			grid.cellWidth = 100f;
			grid.cellHeight = 100f;
			uIScrollGrid.Grid = grid;
			view.AwakeInvenItemListGrid = uIScrollGrid;
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
			visibility = view.GetType().GetMethod("SprkSetMaterialVisibility", bindingAttr);
			compact = view.GetType().GetMethod("SprkCompactMaterialGrid", bindingAttr);
			if (visibility == null || compact == null)
			{
				throw new Exception("Awakening material patch missing");
			}
			for (int num = 0; num < 8; num++)
			{
				GameObject gameObject3 = new GameObject("Material" + num);
				gameObject3.transform.SetParent(gameObject2.transform, worldPositionStays: false);
				ItemIconComponent itemIconComponent = gameObject3.AddComponent<ItemIconComponent>();
				itemIconComponent.RootWidget = gameObject3.AddComponent<UIWidget>();
				icons.Add(itemIconComponent);
			}
		}
		catch (Exception ex)
		{
			failure = new Reply
			{
				id = cmd.id,
				ok = false,
				error = ex.ToString()
			};
		}
		for (int cycle = 0; cycle < 3; cycle++)
		{
			if (failure != null)
			{
				break;
			}
			try
			{
				gridRoot.SetActive(value: false);
				grid.enabled = false;
				foreach (ItemIconComponent item in icons)
				{
					visibility.Invoke(null, new object[2] { item, false });
					item.transform.localPosition = Vector3.zero;
				}
				compact.Invoke(view, null);
				if (grid.GetChildList().Count != 0 || !grid.enabled)
				{
					throw new Exception("Hidden awakening grid did not retain a deferred layout request");
				}
				gridRoot.SetActive(value: true);
			}
			catch (Exception ex2)
			{
				failure = new Reply
				{
					id = cmd.id,
					ok = false,
					error = ex2.ToString()
				};
			}
			yield return null;
			yield return null;
			if (failure == null)
			{
				try
				{
					check(icons.Count);
				}
				catch (Exception ex3)
				{
					failure = new Reply
					{
						id = cmd.id,
						ok = false,
						error = ex3.ToString()
					};
				}
			}
		}
		if (failure == null)
		{
			try
			{
				for (int num2 = 0; num2 < 4; num2++)
				{
					for (int num3 = 0; num3 < icons.Count; num3++)
					{
						visibility.Invoke(null, new object[2]
						{
							icons[num3],
							(num2 == 0) ? (num3 != 0 && num3 != 2 && num3 != 5) : (num2 == 1)
						});
					}
					compact.Invoke(view, null);
					check(num2 switch
					{
						1 => 0, 
						0 => 3, 
						_ => icons.Count, 
					});
				}
			}
			catch (Exception ex4)
			{
				failure = new Reply
				{
					id = cmd.id,
					ok = false,
					error = ex4.ToString()
				};
			}
		}
		gridRoot.SetActive(value: false);
		UnityEngine.Object.Destroy(gridRoot);
		UnityEngine.Object.Destroy(host);
		busy = false;
		if (failure != null)
		{
			Write(failure);
			yield break;
		}
		Reply reply = Snapshot(cmd.id);
		reply.materials.Add("PASS: native awakening fresh-instance sort defaults and all five arrows in both directions; cold-open and pooled-reopen deferred layout, eight distinct slots without sorting, compact hide-all/restore-all and eligibility refresh round trips.");
		Write(reply);
	}

	private IEnumerator ProbeTowerContinuous(Command cmd)
	{
		Type typeFromHandle = typeof(AutoNextBattle);
		MethodInfo open = typeFromHandle.GetMethods().Single((MethodInfo m) => m.Name == "OpenGlobal" && m.GetParameters().Length == 3);
		FieldInfo count = typeFromHandle.GetField("ItemBoosterCount");
		FieldInfo delay = typeFromHandle.GetField("AutoCampaignDelay", BindingFlags.Instance | BindingFlags.NonPublic);
		long[] array = new long[3] { 0L, 2147483650L, 9007199254740993L };
		for (int num = 0; num < array.Length; num++)
		{
			long value = array[num];
			Reply failure = null;
			try
			{
				File.WriteAllText(Path.Combine(root, "tower-opening.txt"), "Loading native AutoNextBattle prefab; booster count=" + value);
				open.Invoke(null, new object[3]
				{
					BattleType.OrdealTower,
					DungeonDifficultyType.Easy,
					Convert.ChangeType(value, open.GetParameters()[2].ParameterType)
				});
			}
			catch (Exception ex)
			{
				failure = new Reply
				{
					id = cmd.id,
					ok = false,
					error = ex.ToString()
				};
			}
			if (failure != null)
			{
				busy = false;
				Write(failure);
				yield break;
			}
			yield return new WaitForSecondsRealtime(2f);
			try
			{
				AutoNextBattle autoNextBattle = AutoNextBattle.instance;
				if (!autoNextBattle.IsOpen() || Convert.ToInt64(count.GetValue(autoNextBattle)) != value || (float)delay.GetValue(autoNextBattle) != 2.5f || autoNextBattle.StopButton == null || autoNextBattle.Label_Stop == null)
				{
					throw new Exception("Native Continuous Battle popup lost its serialized defaults or runtime count");
				}
				autoNextBattle.OnClickStopWithoutMessage();
			}
			catch (Exception ex2)
			{
				failure = new Reply
				{
					id = cmd.id,
					ok = false,
					error = ex2.ToString()
				};
			}
			if (failure != null)
			{
				busy = false;
				Write(failure);
				yield break;
			}
			yield return new WaitForSecondsRealtime(1f);
		}
		Reply reply = Snapshot(cmd.id);
		reply.materials.Add("PASS: native Ordeals Continuous Battle popup opens/stops/reopens, default delay and UI references survive, and 64-bit counts above 2^31 and 2^53 remain exact.");
		busy = false;
		Write(reply);
	}

	private static StatType[] CombatStatTypes()
	{
		return new StatType[9]
		{
			StatType.MaxHp,
			StatType.PhysicalAttack,
			StatType.MagicalAttack,
			StatType.PhysicalDefense,
			StatType.MagicalDefense,
			StatType.PhysicalDodgeChance,
			StatType.MagicalDodgeChance,
			StatType.PhysicalToughness,
			StatType.MagicalToughness
		};
	}

	private static Dictionary<string, object> TableProperties(object row)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		PropertyInfo[] properties = row.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.CanRead && propertyInfo.GetIndexParameters().Length == 0)
			{
				Type propertyType = propertyInfo.PropertyType;
				if (propertyType.IsPrimitive || propertyType.IsEnum || propertyType == typeof(string) || propertyType.IsArray)
				{
					object value = propertyInfo.GetValue(row, null);
					dictionary[propertyInfo.Name] = (propertyType.IsEnum ? ((object)Convert.ToInt64(value)) : value);
				}
			}
		}
		return dictionary;
	}

	private static Dictionary<string, object> CombatStats(StatData data)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (data != null)
		{
			StatType[] array = CombatStatTypes();
			for (int i = 0; i < array.Length; i++)
			{
				StatType statType = array[i];
				dictionary[statType.ToString()] = data.GetStat(statType);
			}
		}
		return dictionary;
	}

	private IEnumerator ProbePurificationReuse(Command cmd)
	{
		GameObject host = null;
		Reply failure = null;
		MonoBehaviour window = null;
		UITweener[] tweens = null;
		List<object> report = new List<object>();
		BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		MethodInfo reset = null;
		MethodInfo animate = null;
		try
		{
			GameObject obj = VespaResource.Load<GameObject>("Resources/Prefab2/Window/PurifyHero/PurifyHeroRoom");
			if (obj == null)
			{
				throw new Exception("Purification prefab missing");
			}
			host = new GameObject("PurificationReuseFixture");
			host.SetActive(value: false);
			UIRoot uIRoot = UnityEngine.Object.FindObjectOfType<UIRoot>();
			if (uIRoot != null)
			{
				host.transform.SetParent(uIRoot.transform, worldPositionStays: false);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(obj, host.transform, worldPositionStays: false);
			window = gameObject.GetComponents<MonoBehaviour>().Single((MonoBehaviour c) => c.GetType().Name == "PurifyHeroRoom");
			window.enabled = false;
			Type type = window.GetType();
			reset = type.GetMethod("SprkResetPurifyTween", flags);
			animate = type.GetMethod("coAwakeHeroAnim", flags);
			tweens = new string[3] { "BottomTween", "TopTween", "HighlightTween" }.Select((string n) => (UITweener)type.GetField(n, flags).GetValue(window)).ToArray();
			FieldInfo field = type.GetField("_purifyScene", flags);
			object obj2 = Activator.CreateInstance(field.FieldType, nonPublic: true);
			FieldInfo field2 = field.FieldType.GetField("_purifyCamera", flags);
			field2.SetValue(obj2, Activator.CreateInstance(field2.FieldType, nonPublic: true));
			field.SetValue(window, obj2);
			Transform parent = gameObject.transform;
			while (parent != null && parent != host.transform)
			{
				parent.gameObject.SetActive(value: true);
				parent = parent.parent;
			}
			UITweener[] array = tweens;
			foreach (UITweener obj3 in array)
			{
				obj3.enabled = false;
				Transform parent2 = obj3.transform;
				while (parent2 != null && parent2 != host.transform)
				{
					parent2.gameObject.SetActive(value: true);
					parent2 = parent2.parent;
				}
			}
			host.SetActive(value: true);
		}
		catch (Exception ex)
		{
			failure = new Reply
			{
				id = cmd.id,
				ok = false,
				error = ex.ToString()
			};
		}
		if (failure != null)
		{
			if (host != null)
			{
				UnityEngine.Object.Destroy(host);
			}
			busy = false;
			Write(failure);
			yield break;
		}
		for (int hero = 0; hero < 3; hero++)
		{
			try
			{
				UITweener[] array = tweens;
				foreach (UITweener uITweener in array)
				{
					if (reset == null)
					{
						uITweener.ResetToBeginning();
						continue;
					}
					reset.Invoke(null, new object[1] { uITweener });
				}
				host.SetActive(value: true);
			}
			catch (Exception ex2)
			{
				failure = new Reply
				{
					id = cmd.id,
					ok = false,
					error = ex2.ToString()
				};
			}
			if (failure != null)
			{
				break;
			}
			yield return new WaitForSecondsRealtime(tweens.Max((UITweener t) => t.duration + t.delay) + 0.3f);
			try
			{
				report.Add(new Dictionary<string, object>
				{
					{
						"hero",
						hero + 1
					},
					{
						"tweens",
						((IEnumerable<UITweener>)tweens).Select((Func<UITweener, object>)((UITweener t) => new Dictionary<string, object>
						{
							{ "name", t.name },
							{ "enabled", t.enabled },
							{ "factor", t.tweenFactor },
							{ "duration", t.duration }
						})).ToArray()
					}
				});
				if (tweens.Any((UITweener t) => t.enabled || t.tweenFactor > 0.01f))
				{
					throw new Exception("Purification HUD faded after reopening hero " + (hero + 1));
				}
				if (!((IEnumerator)animate.Invoke(window, new object[4]
				{
					null,
					null,
					Enum.ToObject(animate.GetParameters()[2].ParameterType, 0),
					false
				})).MoveNext())
				{
					throw new Exception("Awakening coroutine did not yield");
				}
			}
			catch (Exception ex3)
			{
				failure = new Reply
				{
					id = cmd.id,
					ok = false,
					error = ex3.ToString()
				};
			}
			if (failure != null)
			{
				break;
			}
			yield return null;
			host.SetActive(value: false);
		}
		UnityEngine.Object.Destroy(host);
		busy = false;
		if (failure == null)
		{
			failure = new Reply
			{
				id = cmd.id,
				ok = true
			};
		}
		failure.materials.AddRange(report);
		Write(failure);
	}

	private IEnumerator ProbeBossCharacteristics(Command cmd)
	{
		List<object> report = new List<object>();
		string[] array = new string[5] { "Resources/Prefab2/Window/Raid/RaidInfo", "Resources/Prefab2/Window/GuildSuppress/GuildSuppressBossInfo", "Resources/Prefab2/Window/WorldBoss/WorldBossProfile", "Resources/Prefab2/Window/ShakmehDungeon/ShakmehBossInfoPopup", "Resources/Prefab2/Window/GodkingTrial/PopupUIGodkingTrialBossInfo" };
		foreach (string path in array)
		{
			GameObject host = null;
			UILabel label = null;
			UIScrollView scroll = null;
			Reply failure = null;
			string description = string.Join("\n", from num2 in Enumerable.Range(1, 45)
				select "Boss characteristic " + num2 + ": this description must remain scrollable after release.");
			Action<string> setDescription = null;
			Dictionary<string, object> values = new Dictionary<string, object> { { "prefab", path } };
			try
			{
				GameObject obj = VespaResource.Load<GameObject>(path);
				if (obj == null)
				{
					throw new Exception("Boss prefab missing: " + path);
				}
				host = new GameObject("BossScrollFixture");
				host.SetActive(value: false);
				UIRoot uIRoot = UnityEngine.Object.FindObjectOfType<UIRoot>();
				if (uIRoot != null)
				{
					host.transform.SetParent(uIRoot.transform, worldPositionStays: false);
				}
				GameObject gameObject = UnityEngine.Object.Instantiate(obj, host.transform, worldPositionStays: false);
				MonoBehaviour window = gameObject.GetComponents<MonoBehaviour>().First((MonoBehaviour c) => c.GetType().GetField("RaidTacticLabel") != null || c.GetType().GetField("TacticLabel") != null);
				FieldInfo fieldInfo = window.GetType().GetField("RaidTacticLabel") ?? window.GetType().GetField("TacticLabel");
				label = (UILabel)fieldInfo.GetValue(window);
				values["originalWidth"] = label.width;
				values["originalHeight"] = label.height;
				values["originalMaxLines"] = label.maxLineCount;
				values["originalParent"] = Hierarchy(label.transform.parent);
				values["originalScrolls"] = (from s in label.GetComponentsInParent<UIScrollView>(includeInactive: true)
					select s.name).ToArray();
				report.Add(values);
				window.enabled = false;
				Transform parent = label.transform;
				while (parent != null && parent != host.transform)
				{
					parent.gameObject.SetActive(value: true);
					parent = parent.parent;
				}
				host.SetActive(value: true);
				Type helper = Assembly.Load("SprkRaidInfoScroll").GetType("SprkRaidInfoScroll");
				MethodInfo setText = helper.GetMethod("SetText");
				MethodInfo refreshLabel = helper.GetMethod("RefreshLabel");
				setDescription = delegate(string text)
				{
					if (setText != null)
					{
						setText.Invoke(null, new object[3] { label, text, false });
					}
					else
					{
						UIHelper.SetLabelText(label, text);
					}
					if (refreshLabel != null)
					{
						refreshLabel.Invoke(null, new object[1] { label });
					}
					else if (window is RaidInfoBase)
					{
						helper.GetMethod("Refresh").Invoke(null, new object[1] { window });
					}
				};
				setDescription(description);
			}
			catch (Exception ex)
			{
				failure = new Reply
				{
					id = cmd.id,
					ok = false,
					error = ex.ToString()
				};
			}
			if (failure != null)
			{
				if (host != null)
				{
					UnityEngine.Object.Destroy(host);
				}
				failure.materials.AddRange(report);
				busy = false;
				Write(failure);
				yield break;
			}
			for (int frame = 0; frame < 6; frame++)
			{
				yield return null;
			}
			float before = 0f;
			float dragged = 0f;
			try
			{
				scroll = ((label.GetComponent<UIDragScrollView>() == null) ? null : label.GetComponent<UIDragScrollView>().scrollView);
				if (scroll == null)
				{
					throw new Exception("Characteristics has no scroll binding: " + path);
				}
				values["height"] = label.height;
				values["boundsHeight"] = scroll.bounds.size.y;
				values["viewportHeight"] = scroll.panel.height;
				if (scroll.bounds.size.y <= scroll.panel.height + 1f)
				{
					throw new Exception("Overflow text has no scrollable bounds: " + path);
				}
				before = scroll.transform.parent.InverseTransformPoint(label.transform.position).y;
				UICamera.currentScheme = UICamera.ControlScheme.Mouse;
				scroll.Press(pressed: true);
				scroll.MoveRelative(new Vector3(0f, 120f, 0f));
				scroll.Press(pressed: false);
				dragged = scroll.transform.parent.InverseTransformPoint(label.transform.position).y;
				values["before"] = before;
				values["dragged"] = dragged;
			}
			catch (Exception ex2)
			{
				failure = new Reply
				{
					id = cmd.id,
					ok = false,
					error = ex2.ToString()
				};
			}
			if (failure != null)
			{
				UnityEngine.Object.Destroy(host);
				failure.materials.AddRange(report);
				busy = false;
				Write(failure);
				yield break;
			}
			yield return new WaitForSecondsRealtime(1.2f);
			try
			{
				float y = scroll.transform.parent.InverseTransformPoint(label.transform.position).y;
				values["afterRelease"] = y;
				if (dragged - before < 80f || Mathf.Abs(y - dragged) > 2f)
				{
					throw new Exception("Characteristics jumps back after release: " + path + " " + before + " -> " + dragged + " -> " + y);
				}
				for (int num = 0; num < 10; num++)
				{
					setDescription(description);
				}
			}
			catch (Exception ex3)
			{
				failure = new Reply
				{
					id = cmd.id,
					ok = false,
					error = ex3.ToString()
				};
			}
			if (failure == null)
			{
				for (int frame = 0; frame < 8; frame++)
				{
					yield return null;
				}
				try
				{
					if (Mathf.Abs(scroll.transform.parent.InverseTransformPoint(label.transform.position).y - dragged) > 2f)
					{
						throw new Exception("Repeated UI refresh resets Characteristics scroll: " + path);
					}
					host.SetActive(value: false);
					host.SetActive(value: true);
				}
				catch (Exception ex4)
				{
					failure = new Reply
					{
						id = cmd.id,
						ok = false,
						error = ex4.ToString()
					};
				}
			}
			if (failure == null)
			{
				for (int frame = 0; frame < 8; frame++)
				{
					yield return null;
				}
				try
				{
					if (Mathf.Abs(scroll.transform.parent.InverseTransformPoint(label.transform.position).y - dragged) > 2f)
					{
						throw new Exception("Tab reopening resets unchanged Characteristics: " + path);
					}
					setDescription(description + "\nAnother boss description.");
					if (Mathf.Abs(scroll.transform.parent.InverseTransformPoint(label.transform.position).y - before) > 2f)
					{
						throw new Exception("Changed boss description did not return to top: " + path);
					}
					setDescription("Short characteristic.");
				}
				catch (Exception ex5)
				{
					failure = new Reply
					{
						id = cmd.id,
						ok = false,
						error = ex5.ToString()
					};
				}
			}
			if (failure == null)
			{
				for (int frame = 0; frame < 8; frame++)
				{
					yield return null;
				}
				try
				{
					if (scroll.bounds.size.y > scroll.panel.height + 5f)
					{
						throw new Exception("Short Characteristics retained stale overflow: " + path);
					}
					setDescription(description);
					scroll.InvalidateBounds();
					if (scroll.bounds.size.y <= scroll.panel.height + 1f)
					{
						throw new Exception("Long Characteristics did not become scrollable again: " + path);
					}
					values["refreshReopenAndTextChanges"] = true;
				}
				catch (Exception ex6)
				{
					failure = new Reply
					{
						id = cmd.id,
						ok = false,
						error = ex6.ToString()
					};
				}
			}
			UnityEngine.Object.Destroy(host);
			if (failure != null)
			{
				failure.materials.AddRange(report);
				busy = false;
				Write(failure);
				yield break;
			}
		}
		busy = false;
		Reply reply = Snapshot(cmd.id);
		reply.materials.AddRange(report);
		reply.materials.Add("PASS: native boss prefab overflow remains scrolled after release.");
		Write(reply);
	}

	private IEnumerator ProbeNativeKasel(Command cmd)
	{
		Reply failure = null;
		List<object> report = new List<object>();
		GameObject view = null;
		RenderTexture target = null;
		try
		{
			TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
			string text = Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows");
			Dictionary<string, AssetInfo> dictionary = new Dictionary<string, AssetInfo>();
			MethodInfo setMethod = typeof(AssetInfo).GetProperty("EntryName").GetSetMethod(nonPublic: true);
			string[] files = Directory.GetFiles(Path.Combine(text, "Assetbundle"), "*", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				string text2 = files[i].Substring(text.Length + 1).Replace('\\', '/');
				AssetInfo assetInfo = new AssetInfo();
				setMethod.Invoke(assetInfo, new object[1] { text2 });
				dictionary.Add(text2, assetInfo);
			}
			SingletonAuto<AssetFileManager>.instance.Setup(dictionary);
			typeof(UnityCache).GetProperty("persistentDataPath").GetSetMethod(nonPublic: true).Invoke(null, new object[1] { Application.dataPath });
			typeof(NGameAsset.Define).GetProperty("PatchDownloaded").GetSetMethod(nonPublic: true).Invoke(null, new object[1] { false });
			_ = SingletonAuto<BundleManager>.instance;
			typeof(NGameAsset.Define).GetProperty("PatchDownloaded").GetSetMethod(nonPublic: true).Invoke(null, new object[1] { true });
			view = new GameObject("NativeKaselS2Camera");
			Camera camera = view.AddComponent<Camera>();
			camera.cullingMask = 1073741824;
			camera.clearFlags = CameraClearFlags.Color;
			camera.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f);
			camera.transform.position = new Vector3(0f, 3f, -12f);
			camera.transform.LookAt(new Vector3(0f, 1f, 0f));
			target = (camera.targetTexture = new RenderTexture(768, 512, 24));
		}
		catch (Exception ex)
		{
			failure = new Reply
			{
				id = cmd.id,
				ok = false,
				error = ex.ToString()
			};
		}
		if (failure == null)
		{
			float deadline = Time.realtimeSinceStartup + 30f;
			while (!SingletonAuto<BundleManager>.instance.IsReady && Time.realtimeSinceStartup < deadline)
			{
				yield return null;
			}
			string[] array = new string[3] { "2-1.Casting", "2-2.Casting", "2-2.Projectile" };
			foreach (string stage in array)
			{
				EffectLoad load = new EffectLoad();
				try
				{
					load.Open("Skill.Hero." + ((cmd.target == 1) ? "Kasel.DarkLord" : "KaselDarkLordCos22GloryLeague") + ".Skill" + stage, 30);
				}
				catch (Exception ex2)
				{
					failure = new Reply
					{
						id = cmd.id,
						ok = false,
						error = ex2.ToString()
					};
				}
				deadline = Time.realtimeSinceStartup + 30f;
				while (failure == null && !load.IsDone && Time.realtimeSinceStartup < deadline)
				{
					yield return null;
				}
				if (failure == null && (!load.IsDone || !load.IsValid))
				{
					failure = new Reply
					{
						id = cmd.id,
						ok = false,
						error = stage + ": " + load.LoadMessage
					};
				}
				if (failure == null)
				{
					try
					{
						load.Value.SetDummyParent(Vector3.zero, Quaternion.identity, Vector3.one);
						load.Value.SetVisible(isVisible: true);
						RestoredPortal.EnsureNativeShaderVariables();
					}
					catch (Exception ex3)
					{
						failure = new Reply
						{
							id = cmd.id,
							ok = false,
							error = ex3.ToString()
						};
					}
					int frame = 0;
					while (failure == null && frame < 180)
					{
						try
						{
							load.Value.Update(1f / 60f);
							if (frame == 90)
							{
								foreach (Renderer item in from r in UnityEngine.Object.FindObjectsOfType<Renderer>()
									where r.name.IndexOf("Kasel", StringComparison.OrdinalIgnoreCase) >= 0
									select r)
								{
									report.Add(new Dictionary<string, object>
									{
										{ "stage", stage },
										{ "renderer", item.name },
										{ "enabled", item.enabled },
										{
											"layer",
											item.gameObject.layer
										},
										{
											"bounds",
											item.bounds.ToString()
										},
										{
											"shader",
											(item.sharedMaterial == null) ? "null" : item.sharedMaterial.shader.name
										}
									});
								}
							}
							if (frame % 15 == 0)
							{
								view.GetComponent<Camera>().Render();
								RenderTexture active = RenderTexture.active;
								RenderTexture.active = target;
								Texture2D texture2D = new Texture2D(768, 512, TextureFormat.RGB24, mipChain: false);
								texture2D.ReadPixels(new Rect(0f, 0f, 768f, 512f), 0, 0);
								texture2D.Apply();
								RenderTexture.active = active;
								int num = 0;
								int num2 = 0;
								int num3 = 0;
								Color32[] pixels = texture2D.GetPixels32();
								for (int i = 0; i < pixels.Length; i++)
								{
									Color32 color = pixels[i];
									if (Mathf.Abs(color.r - 64) > 15 || Mathf.Abs(color.g - 64) > 15 || Mathf.Abs(color.b - 64) > 15)
									{
										num++;
									}
									if (color.r > 200 && color.g < 50 && color.b > 200)
									{
										num2++;
									}
									if (color.r < 32 && color.g < 32 && color.b < 32)
									{
										num3++;
									}
								}
								string text3 = cmd.id + "-native-" + stage + "-" + frame + ".png";
								File.WriteAllBytes(Path.Combine(root, text3), texture2D.EncodeToPNG());
								UnityEngine.Object.DestroyImmediate(texture2D);
								report.Add(new Dictionary<string, object>
								{
									{ "stage", stage },
									{ "frame", frame },
									{ "visiblePixels", num },
									{ "magentaPixels", num2 },
									{ "darkPixels", num3 },
									{ "file", text3 }
								});
							}
						}
						catch (Exception ex4)
						{
							failure = new Reply
							{
								id = cmd.id,
								ok = false,
								error = ex4.ToString()
							};
						}
						yield return null;
						frame++;
					}
				}
				load.Close(detachParent: true);
				load.Release();
				if (failure != null)
				{
					break;
				}
			}
		}
		if (view != null)
		{
			UnityEngine.Object.DestroyImmediate(view);
		}
		if (target != null)
		{
			target.Release();
			UnityEngine.Object.DestroyImmediate(target);
		}
		busy = false;
		Reply reply = failure ?? Snapshot(cmd.id);
		reply.materials = report;
		Write(reply);
	}

	private void ProbeKaselS2(Command cmd, Reply reply)
	{
		string path = Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows/Assetbundle");
		if (janeProbeShaders == null)
		{
			janeProbeShaders = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault((AssetBundle b) => b.name.ToLowerInvariant().Contains("shader"));
		}
		if (janeProbeShaders == null)
		{
			janeProbeShaders = AssetBundle.LoadFromFile(Path.Combine(path, "Unity_2020/Shader.unity3d"));
		}
		string[] array = new string[3] { "2-1_Casting", "2-2_Casting", "2-2_Projectile" };
		foreach (string text in array)
		{
			AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(path, "Effect_Skill_Hero_KaselDarkLordCos22GloryLeague_Skill" + text + ".unity3d"));
			if (assetBundle == null)
			{
				throw new Exception("Missing Kasel S2 bundle: " + text);
			}
			GameObject gameObject = null;
			GameObject gameObject2 = null;
			RenderTexture renderTexture = null;
			try
			{
				gameObject = new GameObject("KaselS2Probe");
				foreach (string item in from n in assetBundle.GetAllAssetNames()
					where string.IsNullOrEmpty(cmd.text) || n.EndsWith(cmd.text, StringComparison.OrdinalIgnoreCase)
					select n)
				{
					UnityEngine.Object.Instantiate(assetBundle.LoadAsset<GameObject>(item), gameObject.transform, worldPositionStays: false);
				}
				Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
				for (int num2 = 0; num2 < componentsInChildren.Length; num2++)
				{
					componentsInChildren[num2].gameObject.layer = 30;
				}
				Renderer[] componentsInChildren2 = gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
				foreach (Renderer renderer in componentsInChildren2)
				{
					if (renderer.enabled && renderer.gameObject.activeInHierarchy)
					{
						Material sharedMaterial = renderer.sharedMaterial;
						Shader shader = ((sharedMaterial == null) ? null : sharedMaterial.shader);
						reply.materials.Add(new Dictionary<string, object>
						{
							{ "stage", text },
							{ "renderer", renderer.name },
							{
								"material",
								(sharedMaterial == null) ? null : sharedMaterial.name
							},
							{
								"shader",
								(shader == null) ? null : shader.name
							},
							{
								"supported",
								shader != null && shader.isSupported
							}
						});
					}
				}
				gameObject2 = new GameObject("KaselS2ProbeCamera");
				Camera camera = gameObject2.AddComponent<Camera>();
				camera.cullingMask = 1073741824;
				camera.clearFlags = CameraClearFlags.Color;
				camera.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 1f);
				camera.transform.position = new Vector3(0f, 3f, -12f);
				camera.transform.LookAt(new Vector3(0f, 1f, 0f));
				renderTexture = (camera.targetTexture = new RenderTexture(768, 512, 24));
				RestoredPortal.EnsureNativeShaderVariables();
				for (int num3 = 0; num3 < 4; num3++)
				{
					ParticleSystem[] componentsInChildren3 = gameObject.GetComponentsInChildren<ParticleSystem>();
					for (int num2 = 0; num2 < componentsInChildren3.Length; num2++)
					{
						componentsInChildren3[num2].Simulate(0.15f + (float)num3 * 0.3f, withChildren: false, restart: true, fixedTimeStep: true);
					}
					camera.Render();
					RenderTexture active = RenderTexture.active;
					RenderTexture.active = renderTexture;
					Texture2D texture2D = new Texture2D(768, 512, TextureFormat.RGB24, mipChain: false);
					texture2D.ReadPixels(new Rect(0f, 0f, 768f, 512f), 0, 0);
					texture2D.Apply();
					RenderTexture.active = active;
					string text2 = cmd.id + "-" + text + "-" + num3 + ".png";
					File.WriteAllBytes(Path.Combine(root, text2), texture2D.EncodeToPNG());
					reply.screenshot = text2;
					UnityEngine.Object.DestroyImmediate(texture2D);
				}
			}
			finally
			{
				if (gameObject != null)
				{
					UnityEngine.Object.DestroyImmediate(gameObject);
				}
				if (gameObject2 != null)
				{
					UnityEngine.Object.DestroyImmediate(gameObject2);
				}
				if (renderTexture != null)
				{
					renderTexture.Release();
					UnityEngine.Object.DestroyImmediate(renderTexture);
				}
				assetBundle.Unload(unloadAllLoadedObjects: true);
			}
		}
	}

	private void ProbeMariaBirdFrames(Command cmd, Reply reply)
	{
		string bundles = Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows/Assetbundle");
		List<AssetBundle> opened = new List<AssetBundle>();
		Func<string, AssetBundle> func = delegate(string relative)
		{
			AssetBundle assetBundle5 = AssetBundle.LoadFromFile(Path.Combine(bundles, relative));
			if (assetBundle5 == null)
			{
				throw new Exception("Maria visual bundle unavailable: " + relative);
			}
			opened.Add(assetBundle5);
			return assetBundle5;
		};
		GameObject gameObject = null;
		GameObject gameObject2 = null;
		GameObject gameObject3 = null;
		RenderTexture renderTexture = null;
		Texture2D texture2D = null;
		try
		{
			if (janeProbeShaders == null)
			{
				janeProbeShaders = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault((AssetBundle b) => b.name.ToLowerInvariant().Contains("shader"));
			}
			if (janeProbeShaders == null)
			{
				janeProbeShaders = AssetBundle.LoadFromFile(Path.Combine(bundles, "Unity_2020/Shader.unity3d"));
			}
			AssetBundle assetBundle = func("OriginalSpec/Creature_Hero_Maria_Cos21.unity3d");
			AssetBundle assetBundle2 = func("OriginalSpec/Creature_Hero_Maria_Star0.unity3d");
			AssetBundle assetBundle3 = func("Creature_Hero_Maria.unity3d");
			AssetBundle assetBundle4 = func("Creature_Hero_Maria_Animation.unity3d");
			gameObject = UnityEngine.Object.Instantiate(assetBundle.LoadAsset<GameObject>(assetBundle.GetAllAssetNames().Single((string n) => n.EndsWith("gameobject.hero_maria_cos21legend_body"))));
			gameObject2 = UnityEngine.Object.Instantiate(assetBundle2.LoadAsset<GameObject>(assetBundle2.GetAllAssetNames().Single((string n) => n.EndsWith("gameobject.hero_maria_cos21legend_weapon"))));
			gameObject.transform.position = Vector3.zero;
			gameObject.transform.rotation = Quaternion.identity;
			gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
			for (int num = 0; num < componentsInChildren.Length; num++)
			{
				componentsInChildren[num].gameObject.layer = 30;
			}
			Animation component = gameObject.GetComponent<Animation>();
			Animation component2 = gameObject2.GetComponent<Animation>();
			if (component == null || component2 == null)
			{
				throw new Exception("Maria native animation component missing");
			}
			component.playAutomatically = false;
			component2.playAutomatically = false;
			component.Stop();
			component2.Stop();
			SkinnedMeshRenderer[] componentsInChildren2 = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
			foreach (SkinnedMeshRenderer obj in componentsInChildren2)
			{
				obj.updateWhenOffscreen = true;
				Material[] sharedMaterials = obj.sharedMaterials;
				foreach (Material material in sharedMaterials)
				{
					if (material == null || material.shader == null || !material.shader.isSupported || material.mainTexture == null)
					{
						throw new Exception("Maria visual has a missing material/texture");
					}
				}
			}
			gameObject3 = new GameObject("MariaBirdProbeCamera");
			Camera camera = gameObject3.AddComponent<Camera>();
			camera.cullingMask = 1073741824;
			camera.clearFlags = CameraClearFlags.Color;
			camera.backgroundColor = new Color(0.2f, 0.25f, 0.3f, 1f);
			camera.orthographic = true;
			camera.orthographicSize = 1.5f;
			camera.transform.position = new Vector3(0f, 1f, 4f);
			camera.transform.LookAt(new Vector3(0f, 1f, 0f));
			RestoredPortal.EnsureNativeShaderVariables();
			renderTexture = (camera.targetTexture = new RenderTexture(768, 768, 24));
			string[] array = new string[6] { "Idle", "Astand", "Attack1", "Skill1", "Skill2", "Victory" };
			foreach (string motion in array)
			{
				AssetBundle obj2 = ((motion == "Idle" || motion == "Astand") ? assetBundle3 : assetBundle4);
				AnimationClip animationClip = obj2.LoadAllAssets<AnimationClip>().Single((AnimationClip c) => c.name.Equals("Hero_Maria_Legend@" + motion + "_" + motion, StringComparison.OrdinalIgnoreCase));
				AnimationClip animationClip2 = obj2.LoadAllAssets<AnimationClip>().Single((AnimationClip c) => c.name.Equals("Hero_Maria_Legend_Weapon@" + motion + "_" + motion, StringComparison.OrdinalIgnoreCase));
				component.AddClip(animationClip, "probe");
				component2.AddClip(animationClip2, "probe");
				component.Play("probe");
				component2.Play("probe");
				for (int num3 = 0; num3 < 25; num3++)
				{
					component["probe"].time = animationClip.length * (float)num3 / 25f;
					component2["probe"].time = animationClip2.length * (float)num3 / 25f;
					component.Sample();
					component2.Sample();
					camera.Render();
					if (num3 == 12)
					{
						RenderTexture active = RenderTexture.active;
						RenderTexture.active = renderTexture;
						texture2D = new Texture2D(768, 768, TextureFormat.RGB24, mipChain: false);
						texture2D.ReadPixels(new Rect(0f, 0f, 768f, 768f), 0, 0);
						texture2D.Apply();
						RenderTexture.active = active;
						string text = cmd.id + "-" + motion + ".png";
						File.WriteAllBytes(Path.Combine(root, text), texture2D.EncodeToPNG());
						UnityEngine.Object.Destroy(texture2D);
						texture2D = null;
						reply.screenshot = text;
					}
				}
			}
			reply.materials.Add("PASS: rendered 150 frames of archived Legend body/bird motions: idle, battle idle, attack, skills and victory.");
		}
		finally
		{
			if (gameObject != null)
			{
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
			if (gameObject3 != null)
			{
				UnityEngine.Object.DestroyImmediate(gameObject3);
			}
			if (renderTexture != null)
			{
				renderTexture.Release();
				UnityEngine.Object.DestroyImmediate(renderTexture);
			}
			if (texture2D != null)
			{
				UnityEngine.Object.DestroyImmediate(texture2D);
			}
			foreach (AssetBundle item in opened)
			{
				item.Unload(unloadAllLoadedObjects: false);
			}
		}
	}

	private void ProbeJaneLegend(Command cmd, Reply reply)
	{
		if (cmd.text != "OriginalSpec" && cmd.text != "MidSpec" && cmd.text != "LowSpec")
		{
			throw new InvalidOperationException("Expected a texture quality directory");
		}
		string hero = ((cmd.action == "maria-legend-probe") ? "Maria" : "Jane");
		string text = Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows/Assetbundle");
		AssetBundle[] source = AssetBundle.GetAllLoadedAssetBundles().ToArray();
		AssetBundle assetBundle = ((janeProbeShaders != null) ? janeProbeShaders : source.FirstOrDefault((AssetBundle b) => b.name.ToLowerInvariant().Contains("shader")));
		if (assetBundle == null)
		{
			assetBundle = AssetBundle.LoadFromFile(Path.Combine(text, "Unity_2020/Shader.unity3d"));
		}
		if (assetBundle == null)
		{
			throw new InvalidOperationException("Shader bundle unavailable");
		}
		janeProbeShaders = assetBundle;
		string text2 = Environment.GetEnvironmentVariable("SPRK_LEGEND_PROBE_BUNDLES");
		if (string.IsNullOrEmpty(text2))
		{
			text2 = text;
		}
		AssetBundle assetBundle2 = AssetBundle.LoadFromFile(Path.Combine(text2, cmd.text + "/Creature_Hero_" + hero + "_Cos21.unity3d"));
		if (assetBundle2 == null)
		{
			throw new InvalidOperationException("Costume bundle unavailable");
		}
		GameObject gameObject = null;
		GameObject gameObject2 = null;
		RenderTexture renderTexture = null;
		Texture2D texture2D = null;
		try
		{
			string text3 = assetBundle2.GetAllAssetNames().Single((string n) => n.EndsWith("gameobject.hero_" + hero.ToLowerInvariant() + "_cos21legend_body"));
			gameObject = UnityEngine.Object.Instantiate(assetBundle2.LoadAsset<GameObject>(text3));
			Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
			for (int num = 0; num < componentsInChildren.Length; num++)
			{
				componentsInChildren[num].gameObject.layer = 30;
			}
			SkinnedMeshRenderer[] componentsInChildren2 = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
			Bounds bounds = componentsInChildren2[0].bounds;
			SkinnedMeshRenderer[] array = componentsInChildren2;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
			{
				skinnedMeshRenderer.updateWhenOffscreen = true;
				bounds.Encapsulate(skinnedMeshRenderer.bounds);
				if (skinnedMeshRenderer.sharedMaterials.Length != skinnedMeshRenderer.sharedMesh.subMeshCount)
				{
					throw new InvalidOperationException("Missing materials: " + skinnedMeshRenderer.name);
				}
				Material[] sharedMaterials = skinnedMeshRenderer.sharedMaterials;
				foreach (Material material in sharedMaterials)
				{
					if (material == null || material.shader == null || !material.shader.isSupported)
					{
						throw new InvalidOperationException("Unsupported material: " + skinnedMeshRenderer.name);
					}
					if (hero == "Maria" && material.mainTexture == null)
					{
						throw new InvalidOperationException("Missing Maria material texture: " + skinnedMeshRenderer.name);
					}
					reply.materials.Add(new Dictionary<string, object>
					{
						{ "renderer", skinnedMeshRenderer.name },
						{ "material", material.name },
						{
							"shader",
							material.shader.name
						},
						{
							"supported",
							material.shader.isSupported
						}
					});
				}
			}
			gameObject2 = new GameObject("JaneLegendProbeCamera");
			Camera camera = gameObject2.AddComponent<Camera>();
			camera.cullingMask = 1073741824;
			camera.clearFlags = CameraClearFlags.Color;
			camera.backgroundColor = new Color(0.2f, 0.25f, 0.3f, 1f);
			camera.orthographic = true;
			camera.orthographicSize = Mathf.Max(bounds.extents.y, bounds.extents.x) * 1.25f;
			camera.transform.position = bounds.center + new Vector3(0f, 0f, 4f);
			camera.transform.LookAt(bounds.center);
			RestoredPortal.EnsureNativeShaderVariables();
			renderTexture = (camera.targetTexture = new RenderTexture(768, 768, 24));
			camera.Render();
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = renderTexture;
			texture2D = new Texture2D(768, 768, TextureFormat.RGB24, mipChain: false);
			texture2D.ReadPixels(new Rect(0f, 0f, 768f, 768f), 0, 0);
			texture2D.Apply();
			RenderTexture.active = active;
			reply.screenshot = cmd.id + ".png";
			File.WriteAllBytes(Path.Combine(root, reply.screenshot), texture2D.EncodeToPNG());
		}
		finally
		{
			if (gameObject2 != null)
			{
				UnityEngine.Object.Destroy(gameObject2);
			}
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
			if (texture2D != null)
			{
				UnityEngine.Object.Destroy(texture2D);
			}
			if (renderTexture != null)
			{
				renderTexture.Release();
				UnityEngine.Object.Destroy(renderTexture);
			}
			assetBundle2.Unload(unloadAllLoadedObjects: false);
		}
	}

	private void Write(Reply reply)
	{
		List<object> list = new List<object>();
		foreach (Element element in reply.elements)
		{
			list.Add(new Dictionary<string, object>
			{
				{ "id", element.id },
				{ "path", element.path },
				{ "kind", element.kind },
				{ "text", element.text },
				{ "enabled", element.enabled },
				{ "x", element.x },
				{ "y", element.y }
			});
		}
		List<object> list2 = new List<object>();
		foreach (Failure error in reply.errors)
		{
			list2.Add(new Dictionary<string, object>
			{
				{ "type", error.type },
				{ "message", error.message },
				{ "stack", error.stack }
			});
		}
		Dictionary<string, object> obj = new Dictionary<string, object>
		{
			{ "id", reply.id },
			{ "ok", reply.ok },
			{ "error", reply.error },
			{ "scene", reply.scene },
			{ "pid", reply.pid },
			{ "width", reply.width },
			{ "height", reply.height },
			{ "frame", reply.frame },
			{ "screenshot", reply.screenshot },
			{ "elements", list },
			{ "behaviours", reply.behaviours },
			{ "errors", list2 },
			{ "materials", reply.materials }
		};
		string text = Path.Combine(root, reply.id + ".json");
		File.WriteAllText(text + ".tmp", Json.Serialize(obj));
		File.Move(text + ".tmp", text);
	}

	private static string ProbeInnPresentation()
	{
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		GameObject gameObject = new GameObject("InnPresentationFixture");
		gameObject.SetActive(value: false);
		try
		{
			GameObject obj = VespaResource.Load<GameObject>("Resources/Prefab2/Window/FullScene/HeroInnView");
			if (obj == null)
			{
				throw new Exception("Native Inn prefab is missing");
			}
			HeroInnView component = UnityEngine.Object.Instantiate(obj, gameObject.transform, worldPositionStays: false).GetComponent<HeroInnView>();
			HeroInnViewButtonGroup[] obj2 = new HeroInnViewButtonGroup[3] { component.FriendlyBtnGroup, component.ClosenessBtnGroup, component.ConnectBtnGroup };
			InnPresentationListener listener = new InnPresentationListener();
			HeroInnViewButtonGroup[] array = obj2;
			foreach (HeroInnViewButtonGroup heroInnViewButtonGroup in array)
			{
				FriendlyActionType[] first = heroInnViewButtonGroup.Buttons.Select((HeroInnViewButton b) => b.ActionType).ToArray();
				bool[] first2 = heroInnViewButtonGroup.Buttons.Select((HeroInnViewButton b) => b.gameObject.activeSelf).ToArray();
				heroInnViewButtonGroup.SetListener(listener);
				heroInnViewButtonGroup.SetListener(listener);
				if (!first.SequenceEqual(heroInnViewButtonGroup.Buttons.Select((HeroInnViewButton b) => b.ActionType)) || !first2.SequenceEqual(heroInnViewButtonGroup.Buttons.Select((HeroInnViewButton b) => b.gameObject.activeSelf)))
				{
					throw new Exception("SetListener rewrote serialized Inn buttons");
				}
			}
			FriendlyActionType[] array2 = new FriendlyActionType[3]
			{
				FriendlyActionType.Closeness_Greeting,
				FriendlyActionType.Closeness_Conversation,
				FriendlyActionType.Closeness_Gift
			};
			for (int i = 0; i < array2.Length; i++)
			{
				FriendlyActionType action = array2[i];
				if (!component.ClosenessBtnGroup.Buttons.Any((HeroInnViewButton b) => b.ActionType == action))
				{
					throw new Exception("Missing native owned-hero action: " + action);
				}
			}
			int num = 0;
			foreach (HeroInnViewButton item in component.ClosenessBtnGroup.Buttons.Where((HeroInnViewButton b) => b.ActionType >= FriendlyActionType.Closeness_Greeting && b.ActionType <= FriendlyActionType.Closeness_Gift))
			{
				bool[] array3 = new bool[2] { false, true };
				foreach (bool flag in array3)
				{
					bool[] array4 = new bool[2] { false, true };
					foreach (bool isEnableCost in array4)
					{
						bool[] array5 = new bool[2] { false, true };
						foreach (bool flag2 in array5)
						{
							item.Set(flag, isEnableCost, flag2);
							bool activeSelf = item.Label_Disable.gameObject.activeSelf;
							bool activeSelf2 = item.Label_Cost.gameObject.activeSelf;
							bool activeSelf3 = item.Label_MaxCloseness.gameObject.activeSelf;
							if (activeSelf != (!flag && !flag2) || activeSelf2 != (flag && !flag2) || activeSelf3 != flag2 || new bool[3] { activeSelf, activeSelf2, activeSelf3 }.Count((bool active) => active) != 1)
							{
								throw new Exception("Overlapping/wrong Inn labels: " + item.ActionType);
							}
							num++;
						}
					}
				}
			}
			return "PASS: real Inn prefab preserves all three groups' actions and visibility; " + num + " native cooldown/cost/max label states are mutually exclusive. Affinity uses saved ClosenessPoint, not ownership.";
		}
		finally
		{
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	private static string ProbeLinePetCurrency()
	{
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		int[] array = new int[4] { 26, 27, 29, 30 };
		for (int i = 0; i < array.Length; i++)
		{
			int num = array[i];
			NewEquipGachaData data = GameRoot.Instance.NewEquipGachaDataContainer.GetData(num);
			NewEquipGachaData data2 = GameRoot.Instance.NewEquipGachaDataContainer.GetData((num == 26 || num == 27) ? 25 : 28);
			if (data.OpenGem != data2.OpenGem * 2 || data.OpenGem <= 0 || data.TicketOnly)
			{
				throw new Exception("Incorrect LINE ruby price/payment type: " + num);
			}
			if (data.Durational != 0 || !string.IsNullOrEmpty(data.BeginTime) || !string.IsNullOrEmpty(data.EndTime))
			{
				throw new Exception("LINE summon still has a period: " + num);
			}
		}
		return "PASS: native LINE summon tables load 500-ruby singles / 5,000-ruby 10+1 draws, with ruby payment and no event period.";
	}

	private static string ProbeLinePetIndex()
	{
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		AccountManager accountManager = GlobalAutoBehaviour<AccountManager>.instance;
		object value = accountManager.GetType().GetProperty("AccountManagers", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(accountManager, null);
		FieldInfo field = value.GetType().GetField("PetManager");
		object value2 = field.GetValue(value);
		NGame2.NAccount.UserPetManager userPetManager = new NGame2.NAccount.UserPetManager();
		userPetManager.Reset(new PetInfo[0]);
		field.SetValue(value, userPetManager);
		GameObject gameObject = new GameObject("LinePetIndexFixture");
		gameObject.SetActive(value: false);
		try
		{
			CreatureData[] source = (from d in GameRoot.Instance.CreatureDataContainer.GetAllData()
				where d.Race == CreatureRaceType.Pet
				select d).ToArray();
			CreatureData[] array = source.Where((CreatureData d) => d.IsLinePet).ToArray();
			if (array.Length != 16)
			{
				throw new Exception("Expected all 16 archived LINE pet definitions, found " + array.Length);
			}
			GameObject obj = VespaResource.Load<GameObject>("Resources/Prefab2/Window/Pet/PetListBook");
			if (obj == null)
			{
				throw new Exception("Pet index prefab missing");
			}
			PetListBook component = UnityEngine.Object.Instantiate(obj, gameObject.transform, worldPositionStays: false).GetComponent<PetListBook>();
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
			typeof(PetListBook).GetMethod("SetupPetList", bindingAttr).Invoke(component, null);
			MethodInfo method = typeof(PetListBook).GetMethod("SetupPetPortraitComponent", bindingAttr);
			FieldInfo field2 = typeof(PetListBook).GetField("_petPortraitComponentList", bindingAttr);
			int num = 0;
			foreach (CreatureGrade grade in new CreatureGrade[1].Concat(array.Select((CreatureData d) => d.CreatureGrade).Distinct()))
			{
				method.Invoke(component, new object[1] { grade });
				List<PetPortraitComponent> source2 = (List<PetPortraitComponent>)field2.GetValue(component);
				int[] array2 = (from d in source
					where grade == CreatureGrade.None || d.CreatureGrade == grade
					select d.Index into i
					orderby i
					select i).ToArray();
				int[] array3 = (from p in source2
					select p.CreatureData.Index into i
					orderby i
					select i).ToArray();
				if (!array2.SequenceEqual(array3))
				{
					throw new Exception("Pet index missing rows for " + grade.ToString() + ": expected " + array2.Length + ", found " + array3.Length);
				}
				num += array3.Length;
				foreach (CreatureData item in array.Where((CreatureData d) => grade == CreatureGrade.None || d.CreatureGrade == grade))
				{
					if (!array3.Contains(item.Index))
					{
						throw new Exception("LINE pet hidden: " + item.Index);
					}
				}
			}
			return "PASS: all 16 LINE pets appear in the real pet index and matching rarity tabs; " + num + " native portrait rows checked.";
		}
		finally
		{
			field.SetValue(value, value2);
			UnityEngine.Object.Destroy(gameObject);
		}
	}
}
