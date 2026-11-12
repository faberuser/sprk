using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBooster;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

[UIEvent(AllowDuringPatchDownloading = true)]
public class BoosterInfoMenuComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate, IBoosterListObserver
{
	public interface Listener
	{
		void OnClickBoosterInfoMenu();

		void RequestInitBoosterInfoList();
	}

	public const string PrefabPath = "Prefab2/Component/TopMostMenu/BoosterInfoMenuComponent";

	public static PrefabAuto<BoosterInfoMenuComponent> PrefabAuto = new PrefabAuto<BoosterInfoMenuComponent>("Prefab2/Component/TopMostMenu/BoosterInfoMenuComponent");

	private Listener _listener;

	public UIButton Button_Open;

	public UIGame3DWidget UiEffect_Booster;

	public UISprite Sprite_New;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		SetEffectPlay(play: false);
		if (GlobalAutoBehaviour<BoosterManager>.isValidInstance)
		{
			GlobalAutoBehaviour<BoosterManager>.instance.RemoveObserver(this);
		}
	}

	public void Open(Listener listener)
	{
		Reset();
		_listener = listener;
		GlobalAutoBehaviour<BoosterManager>.instance.AddObserver(this);
	}

	private void Reset()
	{
		_listener = null;
		SetNewSprite(visible: false);
		SetEffectPlay(play: false);
	}

	public void SetNewSprite(bool visible)
	{
		UIHelper.EnableGameObject(Sprite_New, visible);
	}

	public void SetEffectPlay(bool play)
	{
		if (UiEffect_Booster == null)
		{
			return;
		}
		if (play)
		{
			if (!UiEffect_Booster.IsPlaying)
			{
				ReAssignDepth();
				UiEffect_Booster.Play(null);
			}
		}
		else
		{
			UiEffect_Booster.Stop(immediatelyClear: true);
		}
	}

	private void ReAssignDepth()
	{
		int depth = UiEffect_Booster.depth;
		UiEffect_Booster.depth -= 1;
		UiEffect_Booster.depth = depth;
	}

	public void OnClickBoosterInfoMenu()
	{
		if (_listener != null)
		{
			_listener.OnClickBoosterInfoMenu();
		}
	}

	void IBoosterListObserver.OnChanged(BoosterInfo expiredBooster)
	{
		if (_listener != null)
		{
			_listener.RequestInitBoosterInfoList();
		}
	}
}
