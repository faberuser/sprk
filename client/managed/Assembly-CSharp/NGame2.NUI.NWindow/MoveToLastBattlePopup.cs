using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class MoveToLastBattlePopup : BaseWindow
{
	public interface Listener
	{
		void MoveNode(int ChapterIndex, int DungeonIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/MoveToLastBattlePopup";

	public UILabel Label_NodeName;

	public UITexture Texture_Chapter;

	private Listener _listener;

	private CampaignNodeData _nodeData;

	public static void Open(Listener listener, CampaignNodeData nodeData)
	{
		WindowPrefab<MoveToLastBattlePopup> windowPrefab = new WindowPrefab<MoveToLastBattlePopup>("Resources/Prefab2/Window/MessagePopup/MoveToLastBattlePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(listener, nodeData);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(Listener listener, CampaignNodeData nodeData)
	{
		base.Open();
		_listener = listener;
		_nodeData = nodeData;
		Setup();
	}

	private void Setup()
	{
		if (_nodeData != null)
		{
			CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(_nodeData.ChapterIndex);
			if (data != null)
			{
				AssetTexture portalBackground = UIHelper.GetPortalBackground(data.BasePortalImageName);
				UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Chapter, portalBackground);
				string stringDirect = LocalizationManager.GetStringDirect(data.Name);
				UIHelper.SetLabelText(Label_NodeName, LocalizationManager.GetStringDirect("PORTAL_LAST_AREA_NAME", stringDirect, _nodeData.DungeonIndex));
			}
		}
	}

	public void OnClickMove()
	{
		if (_listener != null && _nodeData != null)
		{
			Close();
			_listener.MoveNode(_nodeData.ChapterIndex, _nodeData.DungeonIndex);
		}
	}
}
