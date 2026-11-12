using LitJson;
using NGame2.NCutScene.NPostProcess;
using NTEC.PPU;
using NVespa;
using NVespa.NSingleton;
using UnityEngine.Rendering.PostProcessing;

namespace NGame2.NCutScene.NScript;

public class CameraPostProcess2 : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public NGame2.NCutScene.NPostProcess.Noctovision Noctovision = new NGame2.NCutScene.NPostProcess.Noctovision();

	public NGame2.NCutScene.NPostProcess.Noise Noise = new NGame2.NCutScene.NPostProcess.Noise();

	public NGame2.NCutScene.NPostProcess.OldFilm OldFilm = new NGame2.NCutScene.NPostProcess.OldFilm();

	public NGame2.NCutScene.NPostProcess.Pixelize Pixelize = new NGame2.NCutScene.NPostProcess.Pixelize();

	public NGame2.NCutScene.NPostProcess.RadialBlur RadialBlur = new NGame2.NCutScene.NPostProcess.RadialBlur();

	public NGame2.NCutScene.NPostProcess.SelectColor SelectColor = new NGame2.NCutScene.NPostProcess.SelectColor();

	public NGame2.NCutScene.NPostProcess.Threshold Threshold = new NGame2.NCutScene.NPostProcess.Threshold();

	private ILocalTimer _localTimer;

	private int _elapsedTick;

	bool IScript.IsEnd => Noctovision.IsEnd & Noise.IsEnd & OldFilm.IsEnd & Pixelize.IsEnd & RadialBlur.IsEnd & SelectColor.IsEnd & Threshold.IsEnd;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		_localTimer = localTimer;
		_elapsedTick = 0;
		Noctovision.Begin(() => GetCreator<NTEC.PPU.Noctovision>());
		Noise.Begin(() => GetCreator<NTEC.PPU.Noise>());
		OldFilm.Begin(() => GetCreator<NTEC.PPU.OldFilm>());
		Pixelize.Begin(() => GetCreator<NTEC.PPU.Pixelize>());
		RadialBlur.Begin(() => GetCreator<RadialBlur2>());
		SelectColor.Begin(() => GetCreator<NTEC.PPU.SelectColor>());
		Threshold.Begin(() => GetCreator<NTEC.PPU.Threshold>());
	}

	private T GetCreator<T>() where T : PostProcessEffectSettings
	{
		return SceneAutoBehaviour<CutSceneManager>.instance.PostProcessManager.GetCreator<T>();
	}

	void IScript.Update()
	{
		if (_localTimer != null)
		{
			_elapsedTick += _localTimer.deltaTick;
			Noctovision.Update(_elapsedTick);
			Noise.Update(_elapsedTick);
			OldFilm.Update(_elapsedTick);
			Pixelize.Update(_elapsedTick);
			RadialBlur.Update(_elapsedTick);
			SelectColor.Update(_elapsedTick);
			Threshold.Update(_elapsedTick);
		}
	}

	void IScript.End()
	{
		Noctovision.Skip();
		Noise.Skip();
		OldFilm.Skip();
		Pixelize.Skip();
		RadialBlur.Skip();
		SelectColor.Skip();
		Threshold.Skip();
	}

	void IScript.FinalizeScript(FinalizeType finalizeType)
	{
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		Noctovision.LOADFROMJSON(json["Noctovision"]);
		Noise.LOADFROMJSON(json["Noise"]);
		OldFilm.LOADFROMJSON(json["OldFilm"]);
		Pixelize.LOADFROMJSON(json["Pixelize"]);
		RadialBlur.LOADFROMJSON(json["RadialBlur"]);
		SelectColor.LOADFROMJSON(json["SelectColor"]);
		Threshold.LOADFROMJSON(json["Threshold"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("Noctovision", Noctovision);
		jsonData.AddFieldJsonMake("Noise", Noise);
		jsonData.AddFieldJsonMake("OldFilm", OldFilm);
		jsonData.AddFieldJsonMake("Pixelize", Pixelize);
		jsonData.AddFieldJsonMake("RadialBlur", RadialBlur);
		jsonData.AddFieldJsonMake("SelectColor", SelectColor);
		jsonData.AddFieldJsonMake("Threshold", Threshold);
		return jsonData;
	}
}
