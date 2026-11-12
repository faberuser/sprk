using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GuideMenuComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClick(GuideMenuComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/Guide/GuideMenuComponent";

	public static PrefabAuto<GuideMenuComponent> PrefabAuto = new PrefabAuto<GuideMenuComponent>("Prefab2/Component/Guide/GuideMenuComponent");

	private Listener _listener;

	public UILabel Lbl_Title;

	public UIToggle Toggle_Base;

	public GameGuideData GameGuideData { get; private set; }

	public void Open(Listener listner, GameGuideData gameGuideData)
	{
		if (listner == null || gameGuideData == null)
		{
			VespaDebug.LogError("listner == null || gameGuideData == null");
			return;
		}
		_listener = listner;
		GameGuideData = gameGuideData;
		Init();
	}

	public void SetSelect(bool select)
	{
		Toggle_Base.Set(select);
	}

	private void Init()
	{
		SetTitle();
	}

	private void SetTitle()
	{
		string stringDirect = LocalizationManager.GetStringDirect(GameGuideData.Name);
		UIHelper.SetLabelText(Lbl_Title, stringDirect);
	}

	public void OnClick()
	{
		if (_listener != null && Toggle_Base != null && Toggle_Base.value)
		{
			_listener.OnClick(this);
		}
	}
}
