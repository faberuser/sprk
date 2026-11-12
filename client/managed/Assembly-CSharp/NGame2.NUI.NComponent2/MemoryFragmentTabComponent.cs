using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class MemoryFragmentTabComponent : LinkFactoryComponent
{
	public interface IListener
	{
		void OnClickTab(MemoryFragmentTabComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/MemoryFragment/MemoryFragmentTabComponent";

	public static PrefabAuto<MemoryFragmentTabComponent> PrefabAuto = new PrefabAuto<MemoryFragmentTabComponent>("Prefab2/Component/MemoryFragment/MemoryFragmentTabComponent");

	private IListener _listener;

	public MemoryFragmentData _memoryFragmentData;

	public UIToggle MemoryFragmentTab;

	public UIBundleLoader HeroIconTex;

	public UILabel DungeonTitleLabel;

	public UISprite ClearSprite;

	public void Open(IListener listener, MemoryFragmentData memoryFragmentData, bool isCleared)
	{
		if (listener == null)
		{
			VespaDebug.LogError("listener == null");
			return;
		}
		_listener = listener;
		_memoryFragmentData = memoryFragmentData;
		UIHelper.SetBundleTexture(HeroIconTex, memoryFragmentData.PortalPath);
		UIHelper.SetLabelText(DungeonTitleLabel, LocalizationManager.GetStringDirect(memoryFragmentData.SelectDungeonName));
		UIHelper.EnableGameObject(ClearSprite, isCleared);
	}

	public void OnClick()
	{
		if (_listener != null)
		{
			_listener.OnClickTab(this);
		}
	}

	public void SelectToggle(bool isSelect)
	{
		MemoryFragmentTab.Set(isSelect);
	}
}
