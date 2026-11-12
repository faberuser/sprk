using System;
using NGame2.NCutScene.NScript;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NCutScene;

public class ScriptFactory : SceneAutoBehaviour<ScriptFactory>
{
	public IncreaseDic<string, Func<IScript>> Dic = new IncreaseDic<string, Func<IScript>>();

	public ScriptFactory()
	{
		AddCreateFunc<CameraPostProcess>();
		AddCreateFunc<CameraPostProcess1>();
		AddCreateFunc<CameraPostProcess2>();
		AddCreateFunc<ClearCameraPostProcess>();
		AddCreateFunc<CreatureEnter>();
		AddCreateFunc<CreatureLeave>();
		AddCreateFunc<CreatureAction>();
		AddCreateFunc<CreatureAlpha>();
		AddCreateFunc<CreatureColorFocus>();
		AddCreateFunc<CreatureColorFocus2>();
		AddCreateFunc<DisplayText>();
		AddCreateFunc<ForegroundText>();
		AddCreateFunc<ForegroundTextList>();
		AddCreateFunc<TextWindowAlpha>();
		AddCreateFunc<PlayBgm>();
		AddCreateFunc<BgmVolume>();
		AddCreateFunc<PlayAssetSound>();
		AddCreateFunc<PlayAssetSoundInCutScene>();
		AddCreateFunc<PlayScreenEffect>();
		AddCreateFunc<PlayCreatureEffect>();
		AddCreateFunc<PlayCameraShake>();
		AddCreateFunc<PlayCreatureShake>();
		AddCreateFunc<SceneChange>();
		AddCreateFunc<BackgroundChange>();
		AddCreateFunc<BackgroundColor>();
		AddCreateFunc<TimerChange>();
		AddCreateFunc<NodeTreeControl>();
		AddCreateFunc<EmptyScript>();
		AddCreateFunc<PauseMainScene>();
		AddCreateFunc<EndPlayCreatureEffect>();
	}

	public IScript CreateScriptSafe(string typeName)
	{
		IScript script = CreateScript(typeName);
		if (script != null)
		{
			return script;
		}
		return new Dump();
	}

	public IScript CreateScript(string typeName)
	{
		Func<IScript> vALUE = Dic.GETVALUE(typeName, null);
		if (vALUE == null)
		{
			return null;
		}
		IScript script = Vespa_System_Action_Extension_Method.fire(vALUE);
		if (script != null)
		{
			return script;
		}
		return null;
	}

	private void AddCreateFunc<T>() where T : IScript, new()
	{
		Dic.Add(typeof(T).Name, () => new T());
	}
}
