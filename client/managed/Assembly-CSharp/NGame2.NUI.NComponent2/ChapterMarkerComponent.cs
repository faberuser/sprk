using NGame2.NAccount;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class ChapterMarkerComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClickDirectMoveButton(ChapterMarkerComponent comp);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalRenewalComponents/ChapterMarkerComponent";

	public static PrefabAuto<ChapterMarkerComponent> PrefabAuto = new PrefabAuto<ChapterMarkerComponent>("Resources/Prefab2/Window/Portal/PortalRenewalComponents/ChapterMarkerComponent");

	public UISprite LockedSprite;

	public UISprite ActiveSprite;

	public UIButton MarkerButton;

	private Listener _listener;

	public int ChapterIndex { get; private set; }

	public bool IsLocked { get; private set; }

	public bool IsActive { get; private set; }

	public void Open(int chapterIndex, Listener listener)
	{
		_listener = listener;
		ChapterIndex = chapterIndex;
		Init();
	}

	private void Init()
	{
		IsLocked = !GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanGoToChapter(ChapterIndex);
		UIHelper.EnableGameObject(LockedSprite, IsLocked);
		UIHelper.EnableGameObject(ActiveSprite, enable: false);
		UIHelper.EnableButton(MarkerButton, !IsLocked);
	}

	public void ChangeActiveState(bool isActive)
	{
		IsActive = isActive;
		UIHelper.EnableGameObject(ActiveSprite, isActive);
	}

	public void OnClickButton()
	{
		if (!IsLocked && _listener != null)
		{
			_listener.OnClickDirectMoveButton(this);
		}
	}
}
