using System;
using System.Collections.Generic;
using NGame2.NBattleView.NUIOn3D;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWorldMap;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class MarketingUIOffManager : GlobalAutoBehaviour<MarketingUIOffManager>, InitOnCreate
{
	public class MarketingUIValue : ClientValueManager.ClientValueBool<MarketingUIOffManager>
	{
		public MarketingUIValue(string name, bool defalutValue = false)
			: base(name, defalutValue)
		{
		}
	}

	public class MarketingUIListValue : MarketingUIValue
	{
		public List<Type> UINameList = new List<Type>();

		public MarketingUIListValue(string name, bool defalutValue = false)
			: base(name, defalutValue)
		{
		}

		public void Add(Type type)
		{
			UINameList.Add(type);
		}

		public bool IsContain(Type type)
		{
			return UINameList.Contains(type);
		}

		public void Clear()
		{
			UINameList.Clear();
		}
	}

	private List<ClientValueManager.IClientValue> MarketingUIDrawList = new List<ClientValueManager.IClientValue>();

	private MarketingUIValue IsActiveMarketingUIOff = new MarketingUIValue("IsActiveMarketingUIOff");

	public MarketingUIListValue MarketingUILobby1 = new MarketingUIListValue("MarketingUILobby1");

	private MarketingUIListValue MarketingUILobby2 = new MarketingUIListValue("MarketingUILobby2");

	private MarketingUIListValue MarketingUIBattle1 = new MarketingUIListValue("MarketingUIBattle1");

	private MarketingUIListValue MarketingUIBattle2 = new MarketingUIListValue("MarketingUIBattle2");

	private MarketingUIListValue MarketingUIBattle3 = new MarketingUIListValue("MarketingUIBattle3");

	private MarketingUIListValue MarketingUIBattleTitle = new MarketingUIListValue("MarketingUIBattleTitle");

	private MarketingUIValue MapIndicator = new MarketingUIValue("MapIndicator");

	private MarketingUIValue MapPrefabProp = new MarketingUIValue("MapPrefabProp");

	private MarketingUIValue MapAvatar = new MarketingUIValue("MapAvatar");

	private MarketingUIValue MapNode = new MarketingUIValue("MapNode");

	public static bool IsOffMarketingUI => GlobalAutoBehaviour<MarketingUIOffManager>.instance.IsActiveMarketingUIOff.Value;

	void InitOnCreate.InitializeSingleton()
	{
		InitList();
		GlobalAutoBehaviour<GuiManager>.instance.Add("MarketingUIOffManager", this, delegate
		{
			IsActiveMarketingUIOff.Draw();
			if (IsActiveMarketingUIOff.Value)
			{
				using (new BlockManager().VerticalBox(Color.red))
				{
					GUILayout.Label("Marketing UI On/Off");
					GUI.color = Color.red;
					GUILayout.Label("마을이나, 전투로 이동후에 적용");
					GUI.color = Color.white;
					foreach (ClientValueManager.IClientValue marketingUIDraw in MarketingUIDrawList)
					{
						marketingUIDraw.Draw();
					}
				}
				using (new BlockManager().VerticalBox(Color.green))
				{
					GUILayout.Label("Marketing UI On/Off - Map");
					GUI.color = Color.red;
					GUILayout.Label("실시간으로 업데이트");
					GUI.color = Color.white;
					MapIndicator.Draw();
					MapPrefabProp.Draw();
					MapAvatar.Draw();
					MapNode.Draw();
				}
			}
		});
	}

	private void InitList()
	{
		MarketingUIDrawList.Clear();
		MarketingUILobby1.Clear();
		MarketingUILobby2.Clear();
		MarketingUIBattle1.Clear();
		MarketingUIBattle2.Clear();
		MarketingUIBattle3.Clear();
		MarketingUIBattleTitle.Clear();
		MarketingUILobby1.Add(typeof(LobbyTopmostMenu));
		MarketingUILobby1.Add(typeof(LeftMenu));
		MarketingUILobby2.Add(typeof(LobbyRightMenu));
		MarketingUILobby2.Add(typeof(LobbyNodeFunctionMenu));
		MarketingUIBattle1.Add(typeof(BossStatus));
		MarketingUIBattle1.Add(typeof(WorldBossStatus));
		MarketingUIBattle1.Add(typeof(PvpStatus));
		MarketingUIBattle1.Add(typeof(TacticStatus));
		MarketingUIBattle2.Add(typeof(SystemButtonAuto));
		MarketingUIBattle2.Add(typeof(SystemButtonAutoSimple));
		MarketingUIBattle2.Add(typeof(SystemButtonExit));
		MarketingUIBattle2.Add(typeof(SystemButtonExitSmall));
		MarketingUIBattle2.Add(typeof(SystemButtonPartyExit));
		MarketingUIBattle2.Add(typeof(SystemButtonPause));
		MarketingUIBattle2.Add(typeof(SystemButtonPvpGiveUp));
		MarketingUIBattle2.Add(typeof(BattleChat));
		MarketingUIBattle2.Add(typeof(TimeStatus));
		MarketingUIBattle2.Add(typeof(WaveStatus));
		MarketingUIBattle2.Add(typeof(DamageMeter));
		MarketingUIBattle2.Add(typeof(BossBuffStatus));
		MarketingUIBattle2.Add(typeof(BattleEmoti));
		MarketingUIBattle2.Add(typeof(BoosterStatus));
		MarketingUIBattle2.Add(typeof(BattleCameraSwitch));
		MarketingUIBattle3.Add(typeof(HeroParty));
		MarketingUIBattleTitle.Add(typeof(HUD));
		MarketingUIBattleTitle.Add(typeof(PatternAlarm));
		MarketingUIBattleTitle.Add(typeof(BossPatternAlarm));
		MarketingUIBattleTitle.Add(typeof(BossNameTag));
		MarketingUIBattleTitle.Add(typeof(ScreenEffect));
		MarketingUIBattle2.Add(typeof(KarmaCardSelectWindow));
		MarketingUIDrawList.Add(MarketingUILobby1);
		MarketingUIDrawList.Add(MarketingUILobby2);
		MarketingUIDrawList.Add(MarketingUIBattle1);
		MarketingUIDrawList.Add(MarketingUIBattle2);
		MarketingUIDrawList.Add(MarketingUIBattle3);
		MarketingUIDrawList.Add(MarketingUIBattleTitle);
	}

	public static bool IsContainType(Type type)
	{
		if (!IsOffMarketingUI)
		{
			return false;
		}
		return GlobalAutoBehaviour<MarketingUIOffManager>.instance.IsContain(type);
	}

	private bool IsContain(Type type)
	{
		foreach (ClientValueManager.IClientValue marketingUIDraw in GlobalAutoBehaviour<MarketingUIOffManager>.instance.MarketingUIDrawList)
		{
			if (marketingUIDraw is MarketingUIListValue && IsContain(marketingUIDraw as MarketingUIListValue, type))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsContain(MarketingUIListValue value, Type type)
	{
		if (value == null)
		{
			return false;
		}
		if (!value.Value)
		{
			return false;
		}
		return value.IsContain(type);
	}

	public static void HideWindow(GameObject go)
	{
		if (!(go == null))
		{
			DisableComponent<UIPanel>(go);
			DisableComponent<UIGame3DWidget>(go);
			go.SetActive(value: false);
		}
	}

	public static void DisableComponent<T>(GameObject go) where T : MonoBehaviour
	{
		T[] componentsInChildren = go.GetComponentsInChildren<T>(includeInactive: true);
		if (__Extension_Method_ICollection__.COUNT(componentsInChildren) != 0)
		{
			T[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
		}
	}

	public static void UpdateMap()
	{
		MapLayout mapLayout = UnityEngine.Object.FindObjectOfType<MapLayout>();
		if (mapLayout == null)
		{
			return;
		}
		foreach (KeyValuePair<string, bool> item in new Dictionary<string, bool>
		{
			{
				"IndicatorRoot",
				GlobalAutoBehaviour<MarketingUIOffManager>.instance.MapIndicator.Value
			},
			{
				"AvatarRoot",
				GlobalAutoBehaviour<MarketingUIOffManager>.instance.MapAvatar.Value
			},
			{
				"PrefabPropRoot",
				GlobalAutoBehaviour<MarketingUIOffManager>.instance.MapPrefabProp.Value
			},
			{
				"NodeRoot",
				GlobalAutoBehaviour<MarketingUIOffManager>.instance.MapNode.Value
			}
		})
		{
			string key = item.Key;
			bool value = item.Value;
			Transform transform = Vespa_Unity_Transform_Extension_Method.FindChildRecursively(mapLayout.transform, key);
			if (transform != null)
			{
				transform.gameObject.SetActive(!value);
			}
		}
	}
}
