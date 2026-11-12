using NGame2.NUI.NManager.NBooster;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class BoosterDescComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Icon/BoosterDescComponent";

	public static PrefabAuto<BoosterDescComponent> PrefabAuto = new PrefabAuto<BoosterDescComponent>("Prefab2/Component/Icon/BoosterDescComponent");

	public UIBundleLoader BundleTex_Booster;

	public UIGame3DWidget UiEffect_Border;

	public UILabel Label_Desc;

	private BoosterInfo _info;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		UiEffect_Border.Stop(immediatelyClear: true);
		_info = null;
	}

	public void Open(BoosterInfo info)
	{
		if (info != null)
		{
			_info = info;
			SetupIcon();
			SetupDesc();
		}
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
		if (_info != null)
		{
			string boosterIconPath = UIHelper.GetBoosterIconPath(_info.ItemCode);
			UIHelper.SetBundleTexture(BundleTex_Booster, boosterIconPath);
		}
		UiEffect_Border.Play(null);
	}

	private void SetupDesc()
	{
		if (_info != null)
		{
			UIHelper.SetLabelText(Label_Desc, _info.Desc);
		}
	}
}
