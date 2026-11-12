using LitJson;
using NGame2.NCutScene.NPostProcess;
using NTEC.PPU;
using NVespa;
using NVespa.NSingleton;
using UnityEngine.Rendering.PostProcessing;

namespace NGame2.NCutScene.NScript;

public class CameraPostProcess1 : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public NGame2.NCutScene.NPostProcess.CrossedEyes CrossedEyes = new NGame2.NCutScene.NPostProcess.CrossedEyes();

	public NGame2.NCutScene.NPostProcess.Drunk Drunk = new NGame2.NCutScene.NPostProcess.Drunk();

	public NGame2.NCutScene.NPostProcess.Frame Frame = new NGame2.NCutScene.NPostProcess.Frame();

	public NGame2.NCutScene.NPostProcess.IndustrialCamera IndustrialCamera = new NGame2.NCutScene.NPostProcess.IndustrialCamera();

	public NGame2.NCutScene.NPostProcess.Infrared Infrared = new NGame2.NCutScene.NPostProcess.Infrared();

	public NGame2.NCutScene.NPostProcess.Invert Invert = new NGame2.NCutScene.NPostProcess.Invert();

	private ILocalTimer _localTimer;

	private int _elapsedTick;

	bool IScript.IsEnd => CrossedEyes.IsEnd & Drunk.IsEnd & Frame.IsEnd & IndustrialCamera.IsEnd & Infrared.IsEnd & Invert.IsEnd;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		_localTimer = localTimer;
		_elapsedTick = 0;
		CrossedEyes.Begin(() => GetCreator<NTEC.PPU.CrossedEyes>());
		Drunk.Begin(() => GetCreator<NTEC.PPU.Drunk>());
		Frame.Begin(() => GetCreator<NTEC.PPU.Frame>());
		IndustrialCamera.Begin(() => GetCreator<NTEC.PPU.IndustrialCamera>());
		Infrared.Begin(() => GetCreator<NTEC.PPU.Infrared>());
		Invert.Begin(() => GetCreator<NTEC.PPU.Invert>());
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
			CrossedEyes.Update(_elapsedTick);
			Drunk.Update(_elapsedTick);
			Frame.Update(_elapsedTick);
			IndustrialCamera.Update(_elapsedTick);
			Infrared.Update(_elapsedTick);
			Invert.Update(_elapsedTick);
		}
	}

	void IScript.End()
	{
		CrossedEyes.Skip();
		Drunk.Skip();
		Frame.Skip();
		IndustrialCamera.Skip();
		Infrared.Skip();
		Invert.Skip();
	}

	void IScript.FinalizeScript(FinalizeType finalizeType)
	{
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		CrossedEyes.LOADFROMJSON(json["CrossedEyes"]);
		Drunk.LOADFROMJSON(json["Drunk"]);
		Frame.LOADFROMJSON(json["Frame"]);
		IndustrialCamera.LOADFROMJSON(json["IndustrialCamera"]);
		Infrared.LOADFROMJSON(json["Infrared"]);
		Invert.LOADFROMJSON(json["Invert"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("CrossedEyes", CrossedEyes);
		jsonData.AddFieldJsonMake("Drunk", Drunk);
		jsonData.AddFieldJsonMake("Frame", Frame);
		jsonData.AddFieldJsonMake("IndustrialCamera", IndustrialCamera);
		jsonData.AddFieldJsonMake("Infrared", Infrared);
		jsonData.AddFieldJsonMake("Invert", Invert);
		return jsonData;
	}
}
