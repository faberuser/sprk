using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JsonBin;
using MiniJSON;
using NClientData;
using NClientData.NLocalization;
using NGame2;
using NGame2.NAccount;
using NGame2.NUI.NAtlasAssignComponent;
using NGame2.NUI.NDescription;
using NGame2.NUI.NWindow;
using NGameAsset;
using NGameAsset.NBundleManager;
using NGameAsset.NNgui;
using NGameAsset.NNguiAtlas;
using NShared;
using NShared.NJit;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

public static class SprkSkillDescriptionFormulaProbe
{
	private static HeroSkillDesc displayedCard;

	private static ArchiveSkillDesc displayedArchive;

	private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static object Member(object owner, string name)
	{
		Type type = owner.GetType();
		while (type != null)
		{
			PropertyInfo property = type.GetProperty(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				return property.GetValue(owner, null);
			}
			FieldInfo field = type.GetField(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				return field.GetValue(owner);
			}
			type = type.BaseType;
		}
		throw new Exception("Missing member " + name);
	}

	public static string Init()
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
		typeof(Define).GetProperty("PatchDownloaded").GetSetMethod(nonPublic: true).Invoke(null, new object[1] { true });
		Check(JsonRead.CreateRoot(File.ReadAllBytes(Path.Combine(text, "Assetbundle/BundlePack.bin")), out var root), "native bundle pack");
		SingletonUpdater<LoaderPlaying>.instance.InfoContainer.LoadFromJson(root["BundleFileInfo"]);
		SingletonAuto<Container>.instance.Load(root["Ngui"]);
		SingletonAuto<AtlasDataContainer>.instance.Load(root["NguiAtlas"], root["NguiAsset"]);
		SingletonAuto<LocalizationLoader>.instance.SetEnableLoadFromPatch(enableLoad: true);
		GameRoot.Instance.SetJsonLoader(new JsonAssetAsyncLoader());
		GameRoot.Instance.SetAiScriptLoader(new TextAssetSyncLoader());
		GameRoot.Instance.InitForClient();
		GameRoot.Instance.OnDataContainerLoaded(editorVerify: false);
		SingletonAuto<SkillDescContainer>.instance.Load(SingletonAuto<LitJsonAssetAsyncLoader>.instance);
		return "Loading native descriptions";
	}

	private static void Check(bool ok, string reason)
	{
		if (!ok)
		{
			throw new Exception("Skill formula fixture: " + reason);
		}
	}

	private static CachedHeroInfo MysticHero()
	{
		return new CachedHeroInfo(new HeroInfo
		{
			HeroIndex = 26,
			Level = 100,
			Star = 5,
			Transcended = 5,
			SkillLevel1 = 100,
			SkillLevel2 = 100,
			SkillLevel3 = 100,
			SkillLevel4 = 100
		});
	}

	private static void CheckDescriptionLayout(MonoBehaviour owner, UILabel label, bool shortText)
	{
		UIScrollView componentInParent = label.GetComponentInParent<UIScrollView>();
		UIPanel component = componentInParent.GetComponent<UIPanel>();
		UISprite uISprite = owner.GetComponentsInChildren<UISprite>(includeInactive: true).Single((UISprite s) => s.name == "Sprite_DescBack");
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(componentInParent.transform, uISprite.transform, considerInactive: true);
		Vector3 vector = SprkSkillDescriptionFormula.TextStart(label, componentInParent.transform);
		Check(Mathf.Abs(vector.x - bounds.min.x - (bounds.max.y - vector.y)) < 0.6f, "equal visible top/left padding: " + owner.GetType().Name);
		Check((bool)Member(componentInParent, "shouldMove"), "drag enabled for fitting descriptions");
		Check(componentInParent.dragEffect == UIScrollView.DragEffect.MomentumAndSpring && componentInParent.restrictWithinPanel, "native rubber-band configuration");
		if (shortText)
		{
			Check(componentInParent.bounds.size.y <= component.finalClipRegion.w + 1f, "short text fits viewport");
			Vector3 localPosition = componentInParent.transform.localPosition;
			componentInParent.MoveRelative(new Vector3(0f, 45f, 0f));
			Check(componentInParent.RestrictWithinBounds(instant: false, horizontal: false, vertical: true), "fitting text springs back after drag");
			SpringPanel component2 = componentInParent.GetComponent<SpringPanel>();
			Check(component2 != null && component2.enabled && Vector3.Distance(component2.target, localPosition) < 1f, "spring target preserves padded resting position");
			componentInParent.DisableSpring();
			componentInParent.RestrictWithinBounds(instant: true, horizontal: false, vertical: true);
			Check(Vector3.Distance(componentInParent.transform.localPosition, localPosition) < 1f, "fitting text returns to rest");
		}
		else
		{
			float num = componentInParent.bounds.size.y - component.finalClipRegion.w;
			if (num > 2f)
			{
				Vector3 localPosition2 = componentInParent.transform.localPosition;
				float num2 = Mathf.Min(40f, num * 0.5f);
				componentInParent.MoveRelative(new Vector3(0f, num2, 0f));
				componentInParent.RestrictWithinBounds(instant: true, horizontal: false, vertical: true);
				Check(Mathf.Abs(componentInParent.transform.localPosition.y - localPosition2.y - num2) < 1f, "overflowing text remains scrollable");
				componentInParent.ResetPosition();
			}
		}
	}

	public static string Run()
	{
		SprkSkillDescriptionFormula.VerifyRepeatedTermGrouping();
		Check(SingletonAuto<SkillDescContainer>.instance.IsLoaded, "description load complete");
		string environmentVariable = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR");
		List<object> list = new List<object>();
		int num = 0;
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		CreatureData[] array = (from object id in (IList)Json.Deserialize(File.ReadAllText(Path.Combine(environmentVariable, "fixture-roster.json")))
			select GameRoot.Instance.CreatureDataContainer.GetData(Convert.ToInt32(id))).ToArray();
		Check(array.Length == 102 && array.All((CreatureData h) => h != null && h.Race == CreatureRaceType.Hero), "tracked playable hero roster");
		string[][] obj = new string[8][]
		{
			new string[2] { "en", "English" },
			new string[2] { "de", "German" },
			new string[2] { "fr", "French" },
			new string[2] { "ja", "Japanese" },
			new string[2] { "ko", "Korean" },
			new string[2] { "ru", "Russian" },
			new string[2] { "zh-Hans", "ChineseSimplified" },
			new string[2] { "zh-Hant", "ChineseTraditional" }
		};
		Dictionary<string, IDictionary> dictionary2 = new Dictionary<string, IDictionary>();
		string path = Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows");
		string[][] array2 = obj;
		foreach (string[] array3 in array2)
		{
			string path2 = Path.Combine(path, "LocalizationJit/LocalizationAll_" + array3[1] + ".jit");
			if (File.Exists(path2))
			{
				object obj2 = Member(BuildTools_for_Localize.Deserialize(File.ReadAllBytes(path2)), "_dic");
				obj2.GetType().GetMethod("DevOnly_ForceLoadAllRawData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(obj2, null);
				dictionary2[array3[0]] = (IDictionary)obj2.GetType().GetMethod("JsonLoad_Get", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(obj2, null);
			}
		}
		CreatureData[] array4 = array;
		foreach (CreatureData creature in array4)
		{
			CachedHeroInfo cachedHeroInfo = new CachedHeroInfo(new HeroInfo
			{
				HeroIndex = creature.Index,
				Level = 100,
				Star = 5,
				Transcended = 5,
				SkillLevel1 = 100,
				SkillLevel2 = 100,
				SkillLevel3 = 100,
				SkillLevel4 = 100
			});
			Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
			for (int num3 = 1; num3 <= 9; num3++)
			{
				List<int> list2 = new List<int>();
				if (num3 <= 4)
				{
					list2.Add(creature.GetSkillIndexBySlot(num3));
					int[] linkedSkillIndexBySlot = creature.GetLinkedSkillIndexBySlot(num3);
					if (linkedSkillIndexBySlot != null)
					{
						list2.AddRange(linkedSkillIndexBySlot);
					}
				}
				else if (num3 == 5)
				{
					list2.Add(creature.SoulWeaponBasicSkillIndex);
				}
				else if (num3 <= 7)
				{
					int[] transcendSkill = creature.GetTranscendSkill((num3 == 6) ? 3 : 5);
					if (transcendSkill != null)
					{
						list2.AddRange(transcendSkill);
					}
				}
				else
				{
					SoulWeaponSkillData soulWeaponSkillData = GameRoot.Instance.SoulWeaponSkillDataContainer.GetAllData().FirstOrDefault((SoulWeaponSkillData s) => s.BasicSkillIndex == creature.SoulWeaponBasicSkillIndex);
					if (soulWeaponSkillData != null)
					{
						list2.Add(soulWeaponSkillData.GetAddSkillIndex(num3 - 7));
					}
				}
				List<object> list3 = new List<object>();
				foreach (int item in list2.Where((int i) => i > 0).Distinct())
				{
					Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
					foreach (KeyValuePair<string, IDictionary> lang in dictionary2)
					{
						Func<string, string> localize = delegate(string key)
						{
							int num17 = JitOption.StringToHashCode(key);
							return (!lang.Value.Contains(num17)) ? ((!lang.Value.Contains(key)) ? key : ((string)lang.Value[key])) : ((string)lang.Value[num17]);
						};
						try
						{
							SprkSkillDescriptionFormula.WordProvider = (string text4) => SprkSkillFormulaWords.Get(text4, lang.Key);
							SprkSkillDescriptionFormula.Result rendered = SprkSkillDescriptionFormula.Render(item, cachedHeroInfo, localize);
							Check(new string[3] { "trunc(", "round(", "ceiling(" }.All((string token) => !rendered.Text.Contains(token)) && !rendered.Text.Contains(" = "), "clean formula presentation " + item + "/" + lang.Key);
							dictionary4[lang.Key] = rendered.Text;
							if (!(lang.Key == "en"))
							{
								continue;
							}
							num += rendered.Formulas;
							foreach (string item2 in rendered.Unsupported)
							{
								if (!dictionary.ContainsKey(item2))
								{
									dictionary[item2] = 0;
								}
								dictionary[item2]++;
							}
						}
						catch (Exception ex)
						{
							dictionary4[lang.Key] = "ERROR: " + ex.ToString();
						}
						finally
						{
							SprkSkillDescriptionFormula.WordProvider = null;
						}
					}
					list3.Add(new Dictionary<string, object>
					{
						{ "index", item },
						{
							"name",
							SkillDescContainer.GetSkillName(cachedHeroInfo, GameRoot.Instance.SkillDataContainer.GetData(item))
						},
						{ "descriptions", dictionary4 }
					});
				}
				dictionary3[(num3 == 5) ? "sw" : ((num3 == 6) ? "t3" : ((num3 == 7) ? "t5" : ((num3 > 7) ? ("sw" + (num3 - 7)) : num3.ToString())))] = list3;
			}
			list.Add(new Dictionary<string, object>
			{
				{ "index", creature.Index },
				{ "code", creature.CodeName },
				{ "name", creature.Name },
				{ "skills", dictionary3 }
			});
		}
		Dictionary<string, object> dictionary5 = new Dictionary<string, object>();
		CachedHeroInfo hero = new CachedHeroInfo(new HeroInfo
		{
			HeroIndex = 1,
			Level = 100,
			Star = 5,
			Transcended = 5,
			SkillLevel1 = 100,
			SkillLevel2 = 100,
			SkillLevel3 = 100,
			SkillLevel4 = 100
		});
		string[] array5 = new string[2] { "Lost Self", "Wrath of the Matriarch" };
		foreach (string name in array5)
		{
			List<object> list4 = new List<object>();
			foreach (SkillData item3 in from s in GameRoot.Instance.SkillDataContainer.GetAllData()
				where SkillDescContainer.GetSkillName(s) == name
				select s)
			{
				Dictionary<string, object> dictionary6 = new Dictionary<string, object>();
				foreach (KeyValuePair<string, IDictionary> language in dictionary2)
				{
					Func<string, string> localize2 = delegate(string key)
					{
						int num17 = JitOption.StringToHashCode(key);
						return (!language.Value.Contains(num17)) ? ((!language.Value.Contains(key)) ? key : ((string)language.Value[key])) : ((string)language.Value[num17]);
					};
					SprkSkillDescriptionFormula.Result result = SprkSkillDescriptionFormula.Render(item3.Index, hero, localize2);
					Check(result.Formulas == 0 && result.Unsupported.Count == 0, "boss quantity must be context-independent: " + item3.Index);
					dictionary6[language.Key] = result.Text;
				}
				list4.Add(new Dictionary<string, object>
				{
					{ "index", item3.Index },
					{ "descriptions", dictionary6 }
				});
			}
			dictionary5[name] = list4;
		}
		Dictionary<string, object> dictionary7 = new Dictionary<string, object>();
		for (int num4 = 1; num4 <= 10; num4++)
		{
			Dictionary<string, object> dictionary8 = new Dictionary<string, object>();
			foreach (SkillLevelFactorData item4 in from r in GameRoot.Instance.SkillLevelFactorDataContainer.GetAllData()
				orderby r.Level
				select r)
			{
				dictionary8[item4.Level.ToString()] = (decimal)item4.GetFactor(num4) / 1000m;
			}
			dictionary7[num4.ToString()] = dictionary8;
		}
		Dictionary<string, object> dictionary9 = new Dictionary<string, object>();
		array4 = array;
		foreach (CreatureData creatureData in array4)
		{
			Dictionary<string, object> dictionary10 = new Dictionary<string, object>();
			for (int num5 = 1; num5 <= 5; num5++)
			{
				Dictionary<string, object> dictionary11 = new Dictionary<string, object>();
				for (int num6 = 0; num6 <= 5; num6++)
				{
					dictionary11[num6.ToString()] = (decimal)GameRoot.Instance.CreatureStatDataContainer.GetLevelStatFactor(creatureData.Index, num5, num6) / 1000m;
				}
				dictionary10[num5.ToString()] = dictionary11;
			}
			dictionary9[creatureData.CodeName] = dictionary10;
		}
		File.WriteAllText(Path.Combine(environmentVariable, "formula-export.json"), Json.Serialize(new Dictionary<string, object>
		{
			{ "heroes", list },
			{ "bosses", dictionary5 },
			{ "formulas", num },
			{ "unsupported", dictionary },
			{ "level_scaling", dictionary7 },
			{ "awakening_coefficients", dictionary9 }
		}));
		Check(dictionary.Count == 0, "complete native expression coverage");
		int num7 = 0;
		int[] array6 = new int[8] { 1, 25, 26, 29, 34, 72, 85, 111 };
		for (int num2 = 0; num2 < array6.Length; num2++)
		{
			int num8 = array6[num2];
			short[] array7 = new short[3] { 1, 40, 100 };
			for (int num9 = 0; num9 < array7.Length; num9++)
			{
				short num10 = array7[num9];
				sbyte[] array8 = new sbyte[2] { 1, 5 };
				for (int num11 = 0; num11 < array8.Length; num11++)
				{
					sbyte b = array8[num11];
					CachedHeroInfo hero2 = new CachedHeroInfo(new HeroInfo
					{
						HeroIndex = num8,
						Level = 100,
						Star = b,
						Transcended = (sbyte)((b == 5) ? 5 : 0),
						SkillLevel1 = num10,
						SkillLevel2 = num10,
						SkillLevel3 = num10,
						SkillLevel4 = num10
					});
					CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(num8);
					for (int num12 = 1; num12 <= 4; num12++)
					{
						SprkSkillDescriptionFormula.Result result2 = SprkSkillDescriptionFormula.Render(data.GetSkillIndexBySlot(num12), hero2, null);
						Check(result2.Unsupported.Count == 0, "context parity " + num8 + "/" + num12 + "/" + num10 + "/" + b + ": " + string.Join(";", result2.Unsupported.ToArray()));
						num7++;
						if (num8 == 26 && num12 == 1)
						{
							Check(result2.Text.Contains("4 * (248 +"), "Mystic Barrage groups four identical components");
							Check(result2.Text.IndexOf("103.5%", StringComparison.Ordinal) == result2.Text.LastIndexOf("103.5%", StringComparison.Ordinal), "Mystic Barrage ATK coefficient shown once");
							Check(result2.Text.IndexOf("Level Scaling", StringComparison.Ordinal) == result2.Text.LastIndexOf("Level Scaling", StringComparison.Ordinal), "Mystic Barrage scaling shown once");
						}
					}
				}
			}
		}
		UIRoot uIRoot = UnityEngine.Object.FindObjectOfType<UIRoot>();
		GameObject gameObject = VespaResource.Load<GameObject>("Resources/Prefab2/Window/Hero/HeroSkillDesc");
		Check(gameObject != null, "native card prefab");
		GameObject gameObject2 = new GameObject("SkillFormulaFixture");
		if (uIRoot != null)
		{
			gameObject2.transform.SetParent(uIRoot.transform, worldPositionStays: false);
		}
		HeroSkillDesc component = UnityEngine.Object.Instantiate(gameObject, gameObject2.transform, worldPositionStays: false).GetComponent<HeroSkillDesc>();
		AtlasAssignerBase[] componentsInChildren = component.GetComponentsInChildren<AtlasAssignerBase>(includeInactive: true);
		for (int num2 = 0; num2 < componentsInChildren.Length; num2++)
		{
			componentsInChildren[num2].Assign();
		}
		component.transform.SetParent(GlobalUIRoot.instance.ContentsGroup.transform, worldPositionStays: false);
		component.UseTaskWhenOpen = false;
		File.WriteAllText(Path.Combine(environmentVariable, "card-layout.json"), Json.Serialize(((IEnumerable<UIWidget>)component.GetComponentsInChildren<UIWidget>(includeInactive: true)).Select((Func<UIWidget, object>)((UIWidget w) => new Dictionary<string, object>
		{
			{ "name", w.name },
			{
				"parent",
				w.transform.parent.name
			},
			{
				"x",
				w.transform.localPosition.x
			},
			{
				"y",
				w.transform.localPosition.y
			},
			{ "width", w.width },
			{ "height", w.height },
			{ "depth", w.depth },
			{
				"pivot",
				w.pivot.ToString()
			}
		})).ToList()));
		CreatureData creatureData2 = array.First((CreatureData h) => h.CodeName.IndexOf("Annette", StringComparison.OrdinalIgnoreCase) >= 0);
		CachedHeroInfo heroInfo = new CachedHeroInfo(new HeroInfo
		{
			HeroIndex = creatureData2.Index,
			Level = 100,
			Star = 5,
			Transcended = 5,
			SkillLevel1 = 100,
			SkillLevel2 = 100,
			SkillLevel3 = 100,
			SkillLevel4 = 100
		});
		SprkSkillDescriptionFormula.Formula = false;
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		object value = instance.GetType().GetProperty("AccountManagers", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance, null);
		value.GetType().GetField("ItemManager").SetValue(value, new NGame2.NAccount.UserItemManager());
		component.Open(heroInfo, 1, 0, null);
		File.WriteAllText(Path.Combine(environmentVariable, "card-sprites.json"), Json.Serialize(((IEnumerable<UISprite>)component.GetComponentsInChildren<UISprite>(includeInactive: true)).Select((Func<UISprite, object>)((UISprite w) => new Dictionary<string, object>
		{
			{ "name", w.name },
			{ "sprite", w.spriteName },
			{
				"atlas",
				(w.atlas == null) ? "" : w.atlas.name
			},
			{
				"type",
				w.type.ToString()
			}
		})).ToList()));
		string text = component.Label_SkillDesc.text;
		SprkSkillDescriptionFormula component2 = component.GetComponent<SprkSkillDescriptionFormula>();
		Check(component2 != null, "card hook");
		component2.Toggle();
		Check(component.Label_SkillDesc.text != text, "formula description differs");
		component2.Toggle();
		Check(component.Label_SkillDesc.text == text, "native calculated text restored exactly");
		for (int num13 = 0; num13 < 10; num13++)
		{
			component.Refresh();
		}
		Check(component.GetComponentsInChildren<UIButton>(includeInactive: true).Count((UIButton uIButton4) => uIButton4.name == "SPRK Formula Toggle") == 1, "one button after refresh");
		UIButton uIButton = component.GetComponentsInChildren<UIButton>(includeInactive: true).Single((UIButton uIButton4) => uIButton4.name == "SPRK Formula Toggle");
		UIButton button_LinkedSkill = component.Button_LinkedSkill;
		float x = button_LinkedSkill.transform.parent.InverseTransformPoint(component.Label_SkillLevel.transform.parent.position).x;
		Check(Mathf.Abs(uIButton.transform.localPosition.x + button_LinkedSkill.transform.localPosition.x - 2f * x) < 0.1f, "button mirrored around description title");
		Check(uIButton.onClick.Count == 1, "one toggle callback");
		component.Open(MysticHero(), 1, 0, null);
		CheckDescriptionLayout(component, component.Label_SkillDesc, shortText: true);
		component2.Toggle();
		CheckDescriptionLayout(component, component.Label_SkillDesc, shortText: true);
		for (int num14 = 0; num14 < 10; num14++)
		{
			component.Refresh();
		}
		CheckDescriptionLayout(component, component.Label_SkillDesc, shortText: true);
		component2.Toggle();
		CachedHeroInfo cachedHeroInfo2 = new CachedHeroInfo(new HeroInfo
		{
			HeroIndex = 72,
			Level = 100,
			Star = 5,
			Transcended = 5,
			SkillLevel1 = 100,
			SkillLevel2 = 100,
			SkillLevel3 = 100,
			SkillLevel4 = 100
		});
		component.Open(cachedHeroInfo2, 1, 0, null);
		component2.Toggle();
		Check(SprkSkillDescriptionFormula.Formula, "formula selected");
		string text2 = component.Label_SkillDesc.text;
		component.OnClickChangeLinkedSkill();
		Check(SprkSkillDescriptionFormula.Formula && component.Label_SkillDesc.text != text2, "linked variant retains formula mode");
		CheckDescriptionLayout(component, component.Label_SkillDesc, shortText: false);
		displayedCard = component;
		GameObject gameObject3 = VespaResource.Load<GameObject>("Resources/Prefab2/Window/IllustratedBook/ArchiveSkillDesc");
		Check(gameObject3 != null, "native Index card prefab");
		ArchiveSkillDesc archive = UnityEngine.Object.Instantiate(gameObject3, GlobalUIRoot.instance.ContentsGroup.transform, worldPositionStays: false).GetComponent<ArchiveSkillDesc>();
		componentsInChildren = archive.GetComponentsInChildren<AtlasAssignerBase>(includeInactive: true);
		for (int num2 = 0; num2 < componentsInChildren.Length; num2++)
		{
			componentsInChildren[num2].Assign();
		}
		archive.UseTaskWhenOpen = false;
		SprkSkillDescriptionFormula.Formula = false;
		archive.Open(null, cachedHeroInfo2, 1, 0);
		string text3 = archive.LabelDesc.text;
		SprkSkillDescriptionFormula component3 = archive.GetComponent<SprkSkillDescriptionFormula>();
		Check(component3 != null, "Index refresh hook");
		component3.Toggle();
		string indexFormula = archive.LabelDesc.text;
		Check(indexFormula != text3 && indexFormula == SprkSkillDescriptionFormula.Render(GameRoot.Instance.CreatureDataContainer.GetData(72).SkillIndex1, cachedHeroInfo2, null).Text, "Index formula uses native reference hero");
		Check(new string[3] { "trunc(", "round(", "ceiling(" }.All((string token) => !indexFormula.Contains(token)) && !indexFormula.Contains(" = "), "Index simplified text");
		CheckDescriptionLayout(archive, archive.LabelDesc, shortText: false);
		component3.Toggle();
		Check(archive.LabelDesc.text == text3, "Index calculated description restored");
		for (int num15 = 0; num15 < 10; num15++)
		{
			archive.SetSkillInfo();
		}
		UIButton uIButton2 = archive.GetComponentsInChildren<UIButton>(includeInactive: true).Single((UIButton uIButton4) => uIButton4.name == "SPRK Formula Toggle");
		Check(uIButton2.isEnabled && uIButton2.onClick.Count == 1, "one enabled Index toggle callback after refreshes");
		Transform parent = archive.Button_LinkedSkill.transform.parent;
		float x2 = parent.InverseTransformPoint(archive.LabelName.transform.parent.position).x;
		UIButton uIButton3 = archive.GetComponentsInChildren<UIButton>(includeInactive: true).Single((UIButton uIButton4) => uIButton4.name == "Button_Close");
		Vector3 vector = parent.InverseTransformPoint(uIButton3.transform.position);
		Check(Mathf.Abs(uIButton2.transform.localPosition.x + vector.x - 2f * x2) < 0.1f && Mathf.Abs(uIButton2.transform.localPosition.y - vector.y) < 0.1f, "Index toggle mirrors Close horizontally at the same height");
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(parent, uIButton3.transform, considerInactive: true);
		Check(NGUIMath.CalculateRelativeWidgetBounds(parent, archive.Button_LinkedSkill.transform, considerInactive: true).max.x < bounds.min.x, "Index linked switch clears Close");
		Check(archive.LabelDesc.GetComponentInParent<UIScrollView>() != null, "Index retains native description scrolling");
		CheckDescriptionLayout(archive, archive.LabelDesc, shortText: false);
		archive.Open(null, MysticHero(), 1, 0);
		CheckDescriptionLayout(archive, archive.LabelDesc, shortText: true);
		component3.Toggle();
		CheckDescriptionLayout(archive, archive.LabelDesc, shortText: true);
		for (int num16 = 0; num16 < 10; num16++)
		{
			archive.SetSkillInfo();
		}
		CheckDescriptionLayout(archive, archive.LabelDesc, shortText: true);
		component3.Toggle();
		archive.Open(null, cachedHeroInfo2, 1, 0);
		component3.Toggle();
		archive.OnClickSwitchLinkedSkill();
		Check(SprkSkillDescriptionFormula.Formula && archive.LabelDesc.text != indexFormula, "Index linked skill retains formula mode");
		archive.OnClickShowTalentSkill();
		bool activeSelf = archive.WidgetTalentSkill.gameObject.activeSelf;
		component3.Toggle();
		Check(archive.WidgetTalentSkill.gameObject.activeSelf == activeSelf, "Index attributes unaffected by formula toggle");
		archive.OnClickShowTalentSkill();
		component3.Toggle();
		displayedArchive = archive;
		File.WriteAllText(Path.Combine(environmentVariable, "index-layout.json"), Json.Serialize(((IEnumerable<UIWidget>)archive.GetComponentsInChildren<UIWidget>(includeInactive: true)).Select((Func<UIWidget, object>)((UIWidget w) => new Dictionary<string, object>
		{
			{ "name", w.name },
			{
				"parent",
				w.transform.parent.name
			},
			{
				"x",
				w.transform.localPosition.x
			},
			{
				"y",
				w.transform.localPosition.y
			},
			{ "width", w.width },
			{ "height", w.height },
			{ "depth", w.depth },
			{
				"text",
				(w is UILabel) ? ((UILabel)w).text : ""
			},
			{
				"overflow",
				(w is UILabel) ? ((UILabel)w).overflowMethod.ToString() : ""
			}
		})).ToList()));
		File.WriteAllText(Path.Combine(environmentVariable, "scroll-layout.json"), Json.Serialize(((IEnumerable<UILabel>)new UILabel[2] { component.Label_SkillDesc, archive.LabelDesc }).Select((Func<UILabel, object>)delegate(UILabel label)
		{
			UIScrollView componentInParent = label.GetComponentInParent<UIScrollView>();
			UIPanel component4 = componentInParent.GetComponent<UIPanel>();
			return new Dictionary<string, object>
			{
				{
					"owner",
					(label == archive.LabelDesc) ? "Index" : "Heroes"
				},
				{
					"clip",
					component4.baseClipRegion.ToString()
				},
				{
					"offset",
					component4.clipOffset.ToString()
				},
				{
					"bounds",
					componentInParent.bounds.ToString()
				},
				{ "disableDragIfFits", componentInParent.disableDragIfFits },
				{
					"effect",
					componentInParent.dragEffect.ToString()
				},
				{
					"pivot",
					label.pivot.ToString()
				},
				{
					"widgets",
					((IEnumerable<UIWidget>)componentInParent.GetComponentsInChildren<UIWidget>(includeInactive: true)).Select((Func<UIWidget, object>)((UIWidget w) => new Dictionary<string, object>
					{
						{ "name", w.name },
						{
							"pos",
							w.transform.localPosition.ToString()
						},
						{ "width", w.width },
						{ "height", w.height },
						{
							"colliders",
							w.GetComponents<Collider>().Length
						}
					})).ToList()
				}
			};
		}).ToList()));
		File.WriteAllText(Path.Combine(environmentVariable, "formula-report.txt"), "PASS: " + array.Length + " heroes; " + num + " symbolic nodes; " + num7 + " level/awakening cases; grouped Mystic Barrage, mixed/nested sums and rounding-boundary isolation; hero and Index toggles/native restoration/pooling, mirrored placement, clear Close button, equal visible top/left padding, fitting-text spring-back and overflow scrolling, linked variants and skill-attribute isolation. No trunc notation or explanatory footer. Unsupported categories: " + dictionary.Count + ".");
		return "PASS: formula export and native card fixture";
	}

	public static string Preference()
	{
		Check(SprkSkillDescriptionFormula.Formula, "preference survived process restart");
		return "PASS: formula preference survived process restart";
	}

	public static string View()
	{
		Check(displayedCard != null, "card fixture exists");
		displayedCard.Open(MysticHero(), 1, 0, null);
		GameObject gameObject = new GameObject("SkillFormulaVisualFixture");
		gameObject.layer = 30;
		gameObject.AddComponent<UIPanel>().depth = 0;
		Transform transform = displayedCard.Label_SkillDesc.transform;
		while (transform.parent != null && transform.name != "WidgetCurSkillLevelDesc")
		{
			transform = transform.parent;
		}
		UIButton uIButton = displayedCard.GetComponentsInChildren<UIButton>(includeInactive: true).Single((UIButton b) => b.name == "SPRK Formula Toggle");
		Transform[] array = new Transform[4]
		{
			transform,
			displayedCard.Button_LinkedSkill.transform,
			uIButton.transform,
			displayedCard.SkillIconGrid.transform
		};
		foreach (Transform obj in array)
		{
			Vector3 localPosition = obj.localPosition;
			Vector3 localScale = obj.localScale;
			obj.SetParent(gameObject.transform, worldPositionStays: false);
			obj.localPosition = localPosition;
			obj.localScale = localScale;
			obj.gameObject.SetActive(value: true);
		}
		array = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int num = 0; num < array.Length; num++)
		{
			array[num].gameObject.layer = 30;
		}
		UIRect[] componentsInChildren = gameObject.GetComponentsInChildren<UIRect>(includeInactive: true);
		foreach (UIRect uIRect in componentsInChildren)
		{
			UIRect.AnchorPoint[] array2 = new UIRect.AnchorPoint[4] { uIRect.leftAnchor, uIRect.rightAnchor, uIRect.topAnchor, uIRect.bottomAnchor };
			foreach (UIRect.AnchorPoint anchorPoint in array2)
			{
				if (anchorPoint.target != null && !anchorPoint.target.IsChildOf(gameObject.transform))
				{
					anchorPoint.target = null;
				}
			}
		}
		UIPanel[] componentsInChildren2 = gameObject.GetComponentsInChildren<UIPanel>(includeInactive: true);
		for (int num = 0; num < componentsInChildren2.Length; num++)
		{
			componentsInChildren2[num].alpha = 1f;
		}
		UIWidget[] componentsInChildren3 = gameObject.GetComponentsInChildren<UIWidget>(includeInactive: true);
		foreach (UIWidget obj2 in componentsInChildren3)
		{
			obj2.RemoveFromPanel();
			obj2.CreatePanel();
			obj2.MarkAsChanged();
		}
		GameObject gameObject2 = new GameObject("SkillFormulaVisualCamera");
		gameObject2.transform.position = new Vector3(18f, -42f, -1000f);
		Camera camera = gameObject2.AddComponent<Camera>();
		camera.orthographic = true;
		camera.orthographicSize = 450f;
		camera.nearClipPlane = 0.1f;
		camera.farClipPlane = 2000f;
		camera.cullingMask = 1073741824;
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = new Color(0.08f, 0.07f, 0.05f);
		camera.depth = 3000f;
		camera.rect = new Rect(0.275f, 0f, 0.45f, 1f);
		return "PASS: native skill card visual fixture";
	}

	public static string ToggleView()
	{
		displayedCard.GetComponent<SprkSkillDescriptionFormula>().Toggle();
		return "PASS: visual mode toggled";
	}

	public static string ViewArchive()
	{
		Check(displayedArchive != null, "Index card fixture exists");
		displayedArchive.Open(null, MysticHero(), 1, 0);
		GameObject gameObject = new GameObject("IndexFormulaVisualFixture");
		gameObject.layer = 29;
		gameObject.AddComponent<UIPanel>().depth = 0;
		Transform windowRoot = displayedArchive.WindowRoot;
		windowRoot.SetParent(gameObject.transform, worldPositionStays: false);
		windowRoot.localPosition = Vector3.zero;
		windowRoot.localScale = Vector3.one;
		windowRoot.gameObject.SetActive(value: true);
		Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 29;
		}
		UIPanel[] componentsInChildren2 = gameObject.GetComponentsInChildren<UIPanel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].alpha = 1f;
		}
		UIWidget[] componentsInChildren3 = gameObject.GetComponentsInChildren<UIWidget>(includeInactive: true);
		foreach (UIWidget obj in componentsInChildren3)
		{
			obj.RemoveFromPanel();
			obj.CreatePanel();
			obj.MarkAsChanged();
		}
		GameObject gameObject2 = new GameObject("IndexFormulaVisualCamera");
		gameObject2.transform.position = new Vector3(0f, 0f, -1000f);
		Camera camera = gameObject2.AddComponent<Camera>();
		camera.orthographic = true;
		camera.orthographicSize = 550f;
		camera.nearClipPlane = 0.1f;
		camera.farClipPlane = 2000f;
		camera.cullingMask = 536870912;
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = new Color(0.08f, 0.07f, 0.05f);
		camera.depth = 3001f;
		camera.rect = new Rect(0.225f, 0f, 0.55f, 1f);
		return "PASS: native Index card visual fixture";
	}
}
