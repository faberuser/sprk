using LitJson;
using NGame2.NCutScene.NPostProcess;
using NShader;
using NVespa;
using NVespa.NSingleton;
using UnityEngine.Rendering.PostProcessing;

namespace NGame2.NCutScene.NScript;

public class CameraPostProcess : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public NGame2.NCutScene.NPostProcess.Bloom Bloom = new NGame2.NCutScene.NPostProcess.Bloom();

	public NGame2.NCutScene.NPostProcess.ChromaticAberration ChromaticAberration = new NGame2.NCutScene.NPostProcess.ChromaticAberration();

	public NGame2.NCutScene.NPostProcess.Grayscale Grayscale = new NGame2.NCutScene.NPostProcess.Grayscale();

	public NGame2.NCutScene.NPostProcess.LensDistortion LensDistortion = new NGame2.NCutScene.NPostProcess.LensDistortion();

	public NGame2.NCutScene.NPostProcess.Sepia Sepia = new NGame2.NCutScene.NPostProcess.Sepia();

	public NGame2.NCutScene.NPostProcess.Vignette Vignette = new NGame2.NCutScene.NPostProcess.Vignette();

	private ILocalTimer _localTimer;

	private int _elapsedTick;

	bool IScript.IsEnd => Bloom.IsEnd & ChromaticAberration.IsEnd & Grayscale.IsEnd & LensDistortion.IsEnd & Sepia.IsEnd & Vignette.IsEnd;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		_localTimer = localTimer;
		_elapsedTick = 0;
		Bloom.Begin(() => GetCreator<UnityEngine.Rendering.PostProcessing.Bloom>());
		ChromaticAberration.Begin(() => GetCreator<UnityEngine.Rendering.PostProcessing.ChromaticAberration>());
		Grayscale.Begin(() => GetCreator<NShader.Grayscale>());
		LensDistortion.Begin(() => GetCreator<UnityEngine.Rendering.PostProcessing.LensDistortion>());
		Sepia.Begin(() => GetCreator<NShader.Sepia>());
		Vignette.Begin(() => GetCreator<UnityEngine.Rendering.PostProcessing.Vignette>());
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
			Bloom.Update(_elapsedTick);
			ChromaticAberration.Update(_elapsedTick);
			Grayscale.Update(_elapsedTick);
			LensDistortion.Update(_elapsedTick);
			Sepia.Update(_elapsedTick);
			Vignette.Update(_elapsedTick);
		}
	}

	void IScript.End()
	{
		Bloom.Skip();
		ChromaticAberration.Skip();
		Grayscale.Skip();
		LensDistortion.Skip();
		Sepia.Skip();
		Vignette.Skip();
	}

	void IScript.FinalizeScript(FinalizeType finalizeType)
	{
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		Bloom.LOADFROMJSON(json["Bloom"]);
		ChromaticAberration.LOADFROMJSON(json["ChromaticAberration"]);
		Grayscale.LOADFROMJSON(json["Grayscale"]);
		LensDistortion.LOADFROMJSON(json["LensDistortion"]);
		Sepia.LOADFROMJSON(json["Sepia"]);
		Vignette.LOADFROMJSON(json["Vignette"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("Bloom", Bloom);
		jsonData.AddFieldJsonMake("ChromaticAberration", ChromaticAberration);
		jsonData.AddFieldJsonMake("Grayscale", Grayscale);
		jsonData.AddFieldJsonMake("LensDistortion", LensDistortion);
		jsonData.AddFieldJsonMake("Sepia", Sepia);
		jsonData.AddFieldJsonMake("Vignette", Vignette);
		return jsonData;
	}
}
