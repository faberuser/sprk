using LitJson;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NCutScene;
using NVespa;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NCutScene.NScript;

public class ForegroundText : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public int TickLengthMs;

	public string DecorateName = string.Empty;

	public float DecorateBlendTime;

	public Gradient BaseColor = new Gradient();

	public bool DontClose;

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

	private ILocalTimer _localTimer;

	private int _elapsedTick;

	private bool _isEnd;

	private CutSceneForeground Foreground => SceneAutoBehaviour<CommonManagement>.instance.GetInstance<CutSceneForeground>();

	bool IScript.IsEnd => _isEnd;

	public ForegroundText()
	{
		Vespa_Unity_Gradient_Extension_Method.Reset(BaseColor, Color.black);
		Vespa_Unity_Gradient_Extension_Method.Reset(TextColor, Color.white);
		Vespa_Unity_Gradient_Extension_Method.Reset(OutlineColor, Color.black);
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
		CutSceneForegroundTextComponent cutSceneForegroundTextComponent = Foreground.AddTextComponent();
		cutSceneForegroundTextComponent.Text = TextLink.GetLocalize();
		if (!String_Extension_Method.IsNullOrEmpty(cutSceneForegroundTextComponent.Text))
		{
			cutSceneForegroundTextComponent.TextSize = TextSize;
			cutSceneForegroundTextComponent.TextOffset = Offset;
			cutSceneForegroundTextComponent.TextPivot = TextPivot;
			cutSceneForegroundTextComponent.TextColor = Vespa_Unity_Gradient_Extension_Method.EVALUATE(TextColor, 0f, Color.white);
			cutSceneForegroundTextComponent.OutlineColor = Vespa_Unity_Gradient_Extension_Method.EVALUATE(OutlineColor, 0f, Color.black);
		}
		_elapsedTick = 0;
		_isEnd = false;
	}

	void IScript.Update()
	{
		if (!(Foreground == null) && _localTimer != null && !_isEnd)
		{
			_elapsedTick += _localTimer.deltaTick;
			float frame = Mathf.Clamp01((float)_elapsedTick / (float)Mathf.Max(1, TickLengthMs));
			UpdateForeground(frame);
			if (_elapsedTick >= TickLengthMs)
			{
				_isEnd = true;
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
		_isEnd = false;
	}

	private void UpdateForeground(float frame)
	{
		CutSceneForeground foreground = Foreground;
		if (foreground == null || _localTimer == null)
		{
			return;
		}
		foreground.BaseColor = Vespa_Unity_Gradient_Extension_Method.EVALUATE(BaseColor, frame, foreground.BaseColor);
		CutSceneForegroundTextComponent textComponent = foreground.GetTextComponent(0);
		if (textComponent != null && !String_Extension_Method.IsNullOrEmpty(textComponent.Text))
		{
			Vector2 offset = Offset;
			if (BlendOffsetX)
			{
				offset.x = Mathf.Lerp(Offset.x, EndOffset.x, Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(OffsetCurveX, frame, frame));
			}
			if (BlendOffsetY)
			{
				offset.y = Mathf.Lerp(Offset.y, EndOffset.y, Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(OffsetCurveY, frame, frame));
			}
			textComponent.TextOffset = offset;
			textComponent.TextColor = Vespa_Unity_Gradient_Extension_Method.EVALUATE(TextColor, frame, textComponent.TextColor);
			textComponent.OutlineColor = Vespa_Unity_Gradient_Extension_Method.EVALUATE(OutlineColor, frame, textComponent.OutlineColor);
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		TickLengthMs = json["TickLengthMs"].GETINT(TickLengthMs);
		DecorateName = json["DecorateName"].GETSTRING(DecorateName);
		DecorateBlendTime = json["DecorateBlendTime"].GETFLOAT(DecorateBlendTime);
		BaseColor = json["BaseColor"].GetGradient(BaseColor);
		DontClose = json["DontClose"].GETBOOLEAN(DontClose);
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
		jsonData.AddFieldString("DecorateName", DecorateName);
		jsonData.AddFieldDouble("DecorateBlendTime", DecorateBlendTime);
		jsonData.AddFieldGradient("BaseColor", BaseColor);
		jsonData.AddFieldBool("DontClose", DontClose);
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
