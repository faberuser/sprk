using LitJson;
using NGame2.NCutScene.NModifier;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NCutScene;
using NVespa;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NCutScene.NScript;

public class DisplayText : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public LocalizeLinkData TitleLink = new LocalizeLinkData();

	public LocalizeLinkData TextLink = new LocalizeLinkData();

	public bool EnableTouchScale = true;

	public bool UseCustomTextSpeed;

	public float CustomTextSpeed = 10f;

	public bool UseColorFocus;

	public int ColorFocusSlot;

	public bool HideTitle;

	private ILocalTimer _localTimer;

	private string _title;

	private string _text;

	private float _elapsedTime;

	private int _displayCharCount;

	private bool _displayBegin;

	private bool _isEnd;

	private CutSceneWindow Window => SceneAutoBehaviour<CommonManagement>.instance.GetInstance<CutSceneWindow>();

	private float TextSpeed
	{
		get
		{
			if (UseCustomTextSpeed)
			{
				return CustomTextSpeed;
			}
			return GameSetting.data.CutScene.DisplayText_TextSpeed;
		}
	}

	bool IScript.IsEnd => _isEnd;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		if (Window == null)
		{
			return;
		}
		_localTimer = localTimer;
		_title = TitleLink.GetLocalize();
		_text = TextLink.GetLocalize();
		if (_text == null)
		{
			_text = string.Empty;
		}
		_elapsedTime = 0f;
		_displayCharCount = 0;
		_isEnd = false;
		_displayBegin = false;
		Window.SetText(_title, string.Empty);
		if (!UseColorFocus)
		{
			return;
		}
		CreatureSlotManager creatureSlotMgr = SceneAutoBehaviour<CutSceneManager>.instance.CreatureSlotMgr;
		for (int i = 0; i < creatureSlotMgr.Count; i++)
		{
			int slotIndexByIndex = creatureSlotMgr.GetSlotIndexByIndex(i);
			CreatureModifier creatureModByIndex = creatureSlotMgr.GetCreatureModByIndex(i);
			if (slotIndexByIndex == ColorFocusSlot)
			{
				creatureModByIndex.SetColorMix(Colors.zero, 0f);
			}
			else
			{
				creatureModByIndex.SetColorMix(GameSetting.data.CutScene.ColorFocus_OtherColorDefault, 0f);
			}
		}
	}

	void IScript.Update()
	{
		if (_isEnd)
		{
			return;
		}
		CutSceneWindow window = Window;
		if (window == null)
		{
			return;
		}
		if (!_displayBegin && EnableTouchScale)
		{
			_displayBegin = true;
			Window.BeginDisplay();
		}
		_elapsedTime += (float)_localTimer.deltaTick * 0.001f;
		int num = Mathf.CeilToInt(_elapsedTime * TextSpeed);
		if (_displayCharCount != num)
		{
			_displayCharCount = num;
			window.HideTitle(HideTitle);
			if (_displayCharCount < _text.Length)
			{
				Window.SetText(_title, _text.Substring(0, _displayCharCount), _text);
				return;
			}
			Window.SetText(_title, _text);
			_isEnd = true;
		}
	}

	void IScript.End()
	{
		Window.SetText(_title, _text);
		_isEnd = true;
	}

	void IScript.FinalizeScript(FinalizeType finalizeType)
	{
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		TitleLink.LOADFROMJSON(json["TitleLink"]);
		TextLink.LOADFROMJSON(json["TextLink"]);
		EnableTouchScale = json["EnableTouchScale"].GETBOOLEAN(EnableTouchScale);
		UseColorFocus = json["UseColorFocus"].GETBOOLEAN(UseColorFocus);
		ColorFocusSlot = json["ColorFocusSlot"].GETINT(ColorFocusSlot);
		HideTitle = json["HideTitle"].GETBOOLEAN(HideTitle);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("TitleLink", TitleLink);
		jsonData.AddFieldJsonMake("TextLink", TextLink);
		jsonData.AddFieldBool("EnableTouchScale", EnableTouchScale);
		jsonData.AddFieldBool("UseColorFocus", UseColorFocus);
		jsonData.AddFieldLong("ColorFocusSlot", ColorFocusSlot);
		jsonData.AddFieldBool("HideTitle", HideTitle);
		return jsonData;
	}
}
