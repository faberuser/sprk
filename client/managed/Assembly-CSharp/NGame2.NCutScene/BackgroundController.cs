using System;
using System.Collections;
using LitJson;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NCutScene;
using NGame2.NUtil;
using NShader;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NCutScene;

public class BackgroundController
{
	public enum EType
	{
		None,
		BlackScreen,
		SolidColorScreen,
		Filter,
		EntryMenuPopup
	}

	public class StockData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public EType Type = EType.BlackScreen;

		public Color Color = Color.white;

		public ChainFilter Filter = new ChainFilter(new BlurFilter());

		public string EntryMenuCode = string.Empty;

		public bool Reverse;

		bool ILoadJson.Load(JsonData json)
		{
			Type = json["Type"].GETENUM(Type);
			Color = json["Color"].GETCOLOR(Color);
			Filter.LOADFROMJSON(json["Filter"]);
			EntryMenuCode = json["EntryMenuCode"].GETSTRING(EntryMenuCode);
			Reverse = json["Reverse"].GETBOOLEAN(Reverse);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldEnum("Type", Type);
			jsonData.AddFieldColor("Color", Color);
			jsonData.AddFieldJsonMake("Filter", Filter);
			jsonData.AddFieldString("EntryMenuCode", EntryMenuCode);
			jsonData.AddFieldBool("Reverse", Reverse);
			return jsonData;
		}
	}

	public static readonly Color BlackScreenColor = new Color32(0, 0, 0, 192);

	public EntryMenuBackground EntryMenuBackground = new EntryMenuBackground();

	public Color BackgroundColor = BlackScreenColor;

	public CutSceneManager Owner { get; private set; }

	public CutSceneBackground Window => SceneAutoBehaviour<CommonManagement>.instance.GetInstance<CutSceneBackground>();

	public void Open(CutSceneManager owner, StockData stock, bool skipAni, Action onComplete)
	{
		Owner = owner;
		Owner.StartVespaCoroutine(coOpen(stock, skipAni, onComplete));
	}

	public void Open(CutSceneManager owner, string entryMenuCode, bool skipAni, bool reverse, Action onComplete)
	{
		Owner = owner;
		Owner.StartVespaCoroutine(coOpenEntryMenu(entryMenuCode, skipAni, reverse, onComplete));
	}

	public void ReleaseGameAsset()
	{
		EntryMenuBackground.Close();
	}

	public void Close()
	{
		if (Window.IsOpen())
		{
			Window.Close(isHide: false, skipAnimation: true, null);
		}
		ReleaseGameAsset();
	}

	public void CloseAsync(Action onComplete)
	{
		Owner.StartVespaCoroutine(coCloseAsync(onComplete));
	}

	private IEnumerator coCloseAsync(Action onComplete)
	{
		bool completeCloseEntryMenu = false;
		Owner.StartVespaCoroutine(coCloseEntryMenu(delegate
		{
			completeCloseEntryMenu = true;
		}));
		bool completeCloseWindow = false;
		Owner.StartVespaCoroutine(coCloseWindow(delegate
		{
			completeCloseWindow = true;
		}));
		while (!completeCloseEntryMenu || !completeCloseWindow)
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	private IEnumerator coCloseEntryMenu(Action onComplete)
	{
		bool entryMenuComplete = false;
		Owner.StartVespaCoroutine(EntryMenuBackground.coHideNgui(delegate
		{
			entryMenuComplete = true;
		}));
		while (!entryMenuComplete)
		{
			yield return null;
		}
		EntryMenuBackground.Close();
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	private IEnumerator coCloseWindow(Action onComplete)
	{
		if (Window == null || !Window.IsOpen())
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		bool isCompleteClose = false;
		Window.Close(isHide: false, skipAnimation: false, delegate
		{
			isCompleteClose = true;
		});
		while (!isCompleteClose)
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	public void Show(bool show)
	{
		if (Window != null && Window.IsOpen())
		{
			Window.Show(show);
		}
		EntryMenuBackground.SetEnable(show);
	}

	public void ChainUpdate()
	{
		if (Window != null && Window.IsOpen())
		{
			Window.SetColor(BackgroundColor);
		}
		EntryMenuBackground.SetColor(BackgroundColor);
		EntryMenuBackground.ChainUpdate();
	}

	private IEnumerator coOpen(StockData stock, bool skipAni, Action onComplete)
	{
		if (stock.Type != EType.None)
		{
			if (stock.Type == EType.BlackScreen)
			{
				BackgroundColor = BlackScreenColor;
				bool isBackgroundOpenComplete = false;
				Window.Open(null, skipAni, delegate
				{
					isBackgroundOpenComplete = true;
				});
				while (!isBackgroundOpenComplete)
				{
					yield return null;
				}
			}
			else if (stock.Type == EType.SolidColorScreen)
			{
				BackgroundColor = stock.Color;
				bool isBackgroundOpenComplete2 = false;
				Window.Open(null, skipAni, delegate
				{
					isBackgroundOpenComplete2 = true;
				});
				while (!isBackgroundOpenComplete2)
				{
					yield return null;
				}
			}
			else if (stock.Type == EType.Filter)
			{
				BackgroundColor = stock.Color;
				bool isBackgroundOpenComplete3 = false;
				Window.Open(stock.Filter, skipAni, delegate
				{
					isBackgroundOpenComplete3 = true;
				});
				while (!isBackgroundOpenComplete3)
				{
					yield return null;
				}
			}
			else if (stock.Type == EType.EntryMenuPopup)
			{
				BackgroundColor = stock.Color;
				yield return Owner.StartVespaCoroutine(coOpenEntryMenu(stock.EntryMenuCode, skipAni, stock.Reverse));
			}
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	private IEnumerator coOpenEntryMenu(string entryMenuCode, bool skipAni, bool reverse, Action onComplete = null)
	{
		if (String_Extension_Method.IsNullOrEmpty(entryMenuCode))
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		if (!EntryMenuBackground.Open(entryMenuCode, GameUIRoot.ELayerType.CutSceneBottomUI))
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		if (EntryMenuBackground.NguiPrefab == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		while (!EntryMenuBackground.NguiPrefab.ISDONE())
		{
			yield return null;
		}
		bool isComplete = false;
		Owner.StartVespaCoroutine(EntryMenuBackground.coShowNgui(skipAni, reverse, delegate
		{
			isComplete = true;
		}));
		while (!isComplete)
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}
}
