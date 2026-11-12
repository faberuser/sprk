using NGame2.NAccount;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class PortalComponent : UIScrollItemBehaviour
{
	public enum EShowMode
	{
		Default,
		TempBase
	}

	public interface Listener
	{
		void SelectPortal(PortalComponent Selected);
	}

	public const string PrefabPath = "Prefab2/Component/Portal/PortalComponent";

	public static PrefabAuto<PortalComponent> PrefabAuto = new PrefabAuto<PortalComponent>("Prefab2/Component/Portal/PortalComponent");

	public UITexture Texture_Portal;

	public UISprite Sprite_Selected;

	public UILabel Label_PortalName;

	public UILabel Label_NodeNum;

	public UIWidget WidgetLocked;

	private Listener _listener;

	public CampaignNodeData NodeData { get; private set; }

	public EShowMode ShowMode { get; private set; }

	public void Open(Listener listener, CampaignNodeData nodeData, EShowMode mode = EShowMode.Default)
	{
		_listener = listener;
		NodeData = nodeData;
		ShowMode = mode;
		if (ShowMode == EShowMode.Default)
		{
			SetupPortal();
		}
		else
		{
			SetupChapterBase();
		}
	}

	public void SetSelected(bool Select)
	{
		UIHelper.EnableGameObject(Sprite_Selected, Select);
	}

	public bool Selectable()
	{
		return !WidgetLocked.gameObject.activeSelf;
	}

	private void SetupPortal()
	{
		if (String_Extension_Method.IsNullOrEmpty(NodeData.PortalImageCode))
		{
			Texture_Portal.mainTexture = null;
		}
		else
		{
			AssetTexture portalBackground = UIHelper.GetPortalBackground(NodeData.PortalImageCode);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Portal, portalBackground);
		}
		SetSelected(Select: false);
		bool flag = true;
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(NodeData.ChapterIndex);
		if (data != null && data.Type != ChapterType.City)
		{
			flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanGoToChapter(NodeData.ChapterIndex) && GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(NodeData.ChapterIndex, NodeData.DungeonIndex);
		}
		UIHelper.EnableGameObject(WidgetLocked, !flag);
		UIHelper.SetLabelText(Label_PortalName, LocalizationManager.GetStringDirect(NodeData.PortalNameCode));
		UIHelper.EnableGameObject(Label_NodeNum, enable: false);
	}

	private void SetupChapterBase()
	{
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(NodeData.ChapterIndex);
		if (data != null && !String_Extension_Method.IsNullOrEmpty(data.BasePortalImageName))
		{
			AssetTexture portalBackground = UIHelper.GetPortalBackground(data.BasePortalImageName);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Portal, portalBackground);
		}
		else
		{
			Texture_Portal.mainTexture = null;
		}
		SetSelected(Select: false);
		UIHelper.EnableGameObject(WidgetLocked, enable: false);
		if (data != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect(data.Name);
			UIHelper.SetLabelText(Label_PortalName, LocalizationManager.GetString(WorldMap.CHAPTER_BASE, stringDirect));
		}
		else
		{
			UIHelper.SetLabelText(Label_PortalName, LocalizationManager.GetString(WorldMap.CHAPTER_BASE, "Error"));
		}
		UIHelper.EnableGameObject(Label_NodeNum, enable: true);
		UIHelper.SetLabelText(Label_NodeNum, $"{NodeData.ChapterIndex}-{NodeData.DungeonIndex}");
	}

	public void OnClickComponent()
	{
		if (_listener != null && Selectable())
		{
			_listener.SelectPortal(this);
		}
	}
}
