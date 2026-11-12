using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class LegendBoosterDescComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Icon/LegendBoosterDescComponent";

	public static PrefabAuto<LegendBoosterDescComponent> PrefabAuto = new PrefabAuto<LegendBoosterDescComponent>("Prefab2/Component/Icon/LegendBoosterDescComponent");

	public UIBundleLoader BundleTex_Booster;

	public UIGame3DWidget UiEffect_Border;

	public string BoosterTexName;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		UiEffect_Border.Stop(immediatelyClear: true);
	}

	public void Open()
	{
		SetupIcon();
	}

	private void OnEnable()
	{
		if (UiEffect_Border != null && !UiEffect_Border.IsPlaying)
		{
			UiEffect_Border.Play(null);
		}
	}

	private void SetupIcon()
	{
		string boosterIconPath = UIHelper.GetBoosterIconPath(BoosterTexName);
		UIHelper.SetBundleTexture(BundleTex_Booster, boosterIconPath);
		UiEffect_Border.Play(null);
	}
}
