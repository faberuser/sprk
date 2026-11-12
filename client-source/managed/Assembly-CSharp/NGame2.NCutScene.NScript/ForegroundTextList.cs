using System.Collections.Generic;
using LitJson;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NCutScene;
using NVespa;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NCutScene.NScript;

public class ForegroundTextList : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson, IClickScript
{
	[SerializeField]
	public class Text : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public int TickLengthMs;

		public LocalizeLinkData TextLink = new LocalizeLinkData();

		public int TextSize = 50;

		public UIWidget.Pivot TextPivot = UIWidget.Pivot.Center;

		public Vector2 Offset = Vector2.zero;

		public Vector2 EndOffset = Vector2.zero;

		public bool BlendOffsetX;

		public AnimationCurve OffsetCurveX = new AnimationCurve();

		public bool BlendOffsetY;

		public AnimationCurve OffsetCurveY = new AnimationCurve();

		public Gradient TextColor = new Gradient();

		public Gradient OutlineColor = new Gradient();

		public bool IsDelete { get; private set; }

		public Text()
		{
			Vespa_Unity_Gradient_Extension_Method.Reset(TextColor, Color.white);
			Vespa_Unity_Gradient_Extension_Method.Reset(OutlineColor, Color.black);
			IsDelete = false;
		}

		bool ILoadJson.Load(JsonData json)
		{
			TickLengthMs = json["TickLengthMs"].GETINT(TickLengthMs);
			TextLink.LOADFROMJSON(json["TextLink"]);
			TextSize = json["TextSize"].GETINT(TextSize);
			TextPivot = json["TextPivot"].GETENUM(TextPivot);
			Offset = json["Offset"].GETVECTOR2(Offset);
			EndOffset = json["EndOffset"].GETVECTOR2(EndOffset);
			BlendOffsetX = json["BlendOffsetX"].GETBOOLEAN(BlendOffsetX);
			OffsetCurveX = json["OffsetCurveX"].GetAnimationCurve(OffsetCurveX);
			BlendOffsetY = json["BlendOffsetY"].GETBOOLEAN(BlendOffsetY);
			OffsetCurveY = json["OffsetCurveY"].GetAnimationCurve(OffsetCurveY);
			TextColor = json["TextColor"].GetGradient(TextColor);
			OutlineColor = json["OutlineColor"].GetGradient(OutlineColor);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldLong("TickLengthMs", TickLengthMs);
			jsonData.AddFieldJsonMake("TextLink", TextLink);
			jsonData.AddFieldLong("TextSize", TextSize);
			jsonData.AddFieldEnum("TextPivot", TextPivot);
			jsonData.AddFieldVector2("Offset", Offset);
			jsonData.AddFieldVector2("EndOffset", EndOffset);
			jsonData.AddFieldBool("BlendOffsetX", BlendOffsetX);
			jsonData.AddFieldAnimationCurve("OffsetCurveX", OffsetCurveX);
			jsonData.AddFieldBool("BlendOffsetY", BlendOffsetY);
			jsonData.AddFieldAnimationCurve("OffsetCurveY", OffsetCurveY);
			jsonData.AddFieldGradient("TextColor", TextColor);
			jsonData.AddFieldGradient("OutlineColor", OutlineColor);
			return jsonData;
		}
	}

	public string DecorateName = string.Empty;

	public float DecorateBlendTime;

	public Gradient BaseColor = new Gradient();

	public bool DontClose;

	public List<Text> TextList = new List<Text>();

	private int CurrentIndex;

	private ILocalTimer _localTimer;

	private int _elapsedTick;

	private bool _isBegin;

	private bool _isEnd;

	private CutSceneForeground Foreground => SceneAutoBehaviour<CommonManagement>.instance.GetInstance<CutSceneForeground>();

	bool IScript.IsEnd
	{
		get
		{
			if (_isBegin)
			{
				return _isEnd;
			}
			return false;
		}
	}

	public ForegroundTextList()
	{
		Vespa_Unity_Gradient_Extension_Method.Reset(BaseColor, Color.black);
		TextList.Clear();
	}

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		if (Foreground == null)
		{
			_isEnd = true;
			return;
		}
		_localTimer = localTimer;
		Foreground.Open(Vespa_Unity_Gradient_Extension_Method.EVALUATE(BaseColor, 0f, Color.black), DecorateName, DecorateBlendTime);
		if (TextList.Count == 0)
		{
			_isEnd = true;
			return;
		}
		CurrentIndex = 0;
		AddComponent(CurrentIndex);
		_elapsedTick = 0;
		_isEnd = false;
		_isBegin = true;
	}

	private void AddComponent(int index)
	{
		Text text = TextList[index];
		CutSceneForegroundTextComponent cutSceneForegroundTextComponent = Foreground.AddTextComponent();
		cutSceneForegroundTextComponent.Text = text.TextLink.GetLocalize();
		if (!String_Extension_Method.IsNullOrEmpty(cutSceneForegroundTextComponent.Text))
		{
			cutSceneForegroundTextComponent.TextSize = text.TextSize;
			cutSceneForegroundTextComponent.TextOffset = text.Offset;
			cutSceneForegroundTextComponent.TextPivot = text.TextPivot;
			cutSceneForegroundTextComponent.TextColor = Vespa_Unity_Gradient_Extension_Method.EVALUATE(text.TextColor, 0f, Color.white);
			cutSceneForegroundTextComponent.OutlineColor = Vespa_Unity_Gradient_Extension_Method.EVALUATE(text.OutlineColor, 0f, Color.black);
		}
	}

	void IScript.Update()
	{
		if (!(Foreground == null) && _localTimer != null && _isBegin && !_isEnd)
		{
			_elapsedTick += _localTimer.deltaTick;
			Text text = TextList[CurrentIndex];
			float frame = Mathf.Clamp01((float)_elapsedTick / (float)Mathf.Max(1, text.TickLengthMs));
			UpdateForeground(frame);
			if (_elapsedTick >= text.TickLengthMs)
			{
				((IClickScript)this).OnClick();
			}
		}
	}

	void IScript.End()
	{
		if (Foreground == null || _localTimer == null)
		{
			_isEnd = true;
			return;
		}
		if (DontClose)
		{
			UpdateForeground(1f);
		}
		_isEnd = true;
	}

	void IScript.FinalizeScript(FinalizeType finalizeType)
	{
		CutSceneForeground foreground = Foreground;
		if (!(foreground == null) && !DontClose)
		{
			foreground.Close();
		}
	}

	void IScript.Reset()
	{
		_isBegin = false;
		CurrentIndex = 0;
	}

	private void UpdateForeground(float frame)
	{
		CutSceneForeground foreground = Foreground;
		if (foreground == null || _localTimer == null)
		{
			return;
		}
		Text text = null;
		CutSceneForegroundTextComponent cutSceneForegroundTextComponent = null;
		if (CurrentIndex < TextList.Count)
		{
			text = TextList[CurrentIndex];
			cutSceneForegroundTextComponent = foreground.GetTextComponent(CurrentIndex);
		}
		else
		{
			int num = TextList.Count - 1;
			if (num >= 0)
			{
				text = TextList[num];
				cutSceneForegroundTextComponent = foreground.GetTextComponent(num);
			}
		}
		foreground.BaseColor = Vespa_Unity_Gradient_Extension_Method.EVALUATE(BaseColor, frame, foreground.BaseColor);
		if (cutSceneForegroundTextComponent != null && !String_Extension_Method.IsNullOrEmpty(cutSceneForegroundTextComponent.Text))
		{
			Vector2 offset = text.Offset;
			if (text.BlendOffsetX)
			{
				offset.x = Mathf.Lerp(text.Offset.x, text.EndOffset.x, Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(text.OffsetCurveX, frame, frame));
			}
			if (text.BlendOffsetY)
			{
				offset.y = Mathf.Lerp(text.Offset.y, text.EndOffset.y, Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(text.OffsetCurveY, frame, frame));
			}
			cutSceneForegroundTextComponent.TextOffset = offset;
			cutSceneForegroundTextComponent.TextColor = Vespa_Unity_Gradient_Extension_Method.EVALUATE(text.TextColor, frame, cutSceneForegroundTextComponent.TextColor);
			cutSceneForegroundTextComponent.OutlineColor = Vespa_Unity_Gradient_Extension_Method.EVALUATE(text.OutlineColor, frame, cutSceneForegroundTextComponent.OutlineColor);
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		DecorateName = json["DecorateName"].GETSTRING(DecorateName);
		DecorateBlendTime = json["DecorateBlendTime"].GETFLOAT(DecorateBlendTime);
		BaseColor = json["BaseColor"].GetGradient(BaseColor);
		DontClose = json["DontClose"].GETBOOLEAN(DontClose);
		TextList.Clear();
		JsonData jsonData = json["TextList"];
		for (int i = 0; i < jsonData.Count; i++)
		{
			JsonData fromArray = jsonData.GetFromArray(i);
			Text text = new Text();
			((ILoadJson)text).Load(fromArray);
			TextList.Add(text);
		}
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("DecorateName", DecorateName);
		jsonData.AddFieldDouble("DecorateBlendTime", DecorateBlendTime);
		jsonData.AddFieldGradient("BaseColor", BaseColor);
		jsonData.AddFieldBool("DontClose", DontClose);
		jsonData.AddFieldList("TextList", TextList);
		return jsonData;
	}

	void IClickScript.OnClick()
	{
		if (_isBegin)
		{
			UpdateForeground(1f);
			CurrentIndex++;
			_elapsedTick = 0;
			if (TextList.Count > CurrentIndex)
			{
				AddComponent(CurrentIndex);
			}
			else
			{
				_isEnd = true;
			}
		}
	}
}
