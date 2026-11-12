using NGame2.NConfig;
using NGame2.NUI.NManager;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NScene.NPreloader;

public class LoginScenePreloader : RootPreloader
{
	public override void Begin()
	{
		base.Begin();
		CreaturePreloadType = EAsyncPreloadType.Normal;
		EffectPreloadType = EAsyncPreloadType.Delay;
		SoundPreloadType = EAsyncPreloadType.Delay;
		AddUiPreload(SceneAutoBehaviour<LoginManagement>.instance.ReserveCache, "LoginManagement.instance.ReserveCache");
		base.Preloader.AddAction(delegate
		{
			SingletonAuto<Config>.CreateInstance();
		}, "Config.CreateInstance");
	}
}
