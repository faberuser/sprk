using System;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGameAsset;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class PortalMoveToPopup : BaseWindow
{
	public interface Listener
	{
		void MoveNode(int ChapterIndex, int DungeonIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/PortalMoveToPopup";

	public UILabel Label_NodeName;

	public UILabel Label_Confirm;

	public UITexture Texture_Chapter;

	private Listener _listener;

	private Action _actionClick;

	public int ChapterIndex { get; private set; }

	public int DungeonIndex { get; private set; }

	public static void Open(Listener listener, CampaignNodeData nodeData)
	{
		WindowPrefab<PortalMoveToPopup> windowPrefab = new WindowPrefab<PortalMoveToPopup>("Resources/Prefab2/Window/MessagePopup/PortalMoveToPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(listener, nodeData);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void Open(Listener listener, PortalData nodeData)
	{
		WindowPrefab<PortalMoveToPopup> windowPrefab = new WindowPrefab<PortalMoveToPopup>("Resources/Prefab2/Window/MessagePopup/PortalMoveToPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(listener, nodeData);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void Open(PortalData nodeData, Action actionClick)
	{
		WindowPrefab<PortalMoveToPopup> windowPrefab = new WindowPrefab<PortalMoveToPopup>("Resources/Prefab2/Window/MessagePopup/PortalMoveToPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(nodeData, actionClick);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(Listener listener, CampaignNodeData nodeData)
	{
		base.Open();
		ChapterIndex = nodeData.ChapterIndex;
		DungeonIndex = nodeData.DungeonIndex;
		_listener = listener;
		if (nodeData == null)
		{
			return;
		}
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(ChapterIndex);
		if (data == null || SceneAutoBehaviour<PortalManagement>.instance.GetInstance<PortalSelect3>() == null)
		{
			return;
		}
		PortalData data2 = GameRoot.Instance.PortalDataContainer.GetData(ChapterIndex, -1);
		if (data2 == null)
		{
			return;
		}
		string popupImagePath = data2.PopupImagePath;
		if (String_Extension_Method.IsNullOrEmpty(popupImagePath))
		{
			return;
		}
		AssetTexture assetTexture = new AssetTexture();
		assetTexture.Open(popupImagePath);
		UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Chapter, assetTexture);
		string stringDirect = LocalizationManager.GetStringDirect(data.Name);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(nodeData.Code) && !String_Extension_Method.IsNullOrEmpty(nodeData.Code[0]))
		{
			string empty = string.Empty;
			if (nodeData.Code[0].Contains("-"))
			{
				int num = nodeData.Code[0].IndexOf('-');
				empty = nodeData.Code[0].Substring(num + 1);
			}
			else
			{
				empty = nodeData.Code[0];
			}
			UIHelper.SetLabelText(Label_NodeName, LocalizationManager.GetStringDirect("PORTAL_LAST_AREA_NAME", stringDirect, empty));
		}
		else
		{
			UIHelper.SetLabelText(Label_NodeName, LocalizationManager.GetStringDirect("PORTAL_LAST_AREA_NAME", stringDirect, DungeonIndex));
		}
		UIHelper.SetLabelText(Label_Confirm, LocalizationManager.GetStringDirect("PORTAL_LAST_AREA_MOVE"));
	}

	private void OpenInternal(Listener listener, PortalData nodeData)
	{
		base.Open();
		ChapterIndex = nodeData.ChapterIndex;
		DungeonIndex = nodeData.DungeonIndex;
		_listener = listener;
		if (nodeData != null)
		{
			if (!String_Extension_Method.IsNullOrEmpty(nodeData.PopupImagePath))
			{
				AssetTexture assetTexture = new AssetTexture();
				assetTexture.Open(nodeData.PopupImagePath);
				UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Chapter, assetTexture);
			}
			UIHelper.SetLabelText(Label_NodeName, LocalizationManager.GetStringDirect(nodeData.Name));
			UIHelper.SetLabelText(Label_Confirm, LocalizationManager.GetStringDirect("PORTAL_MOVE_NODE"));
		}
	}

	private void OpenInternal(PortalData nodeData, Action actionClick = null)
	{
		base.Open();
		ChapterIndex = nodeData.ChapterIndex;
		DungeonIndex = nodeData.DungeonIndex;
		_actionClick = actionClick;
		if (nodeData != null)
		{
			if (!String_Extension_Method.IsNullOrEmpty(nodeData.PopupImagePath))
			{
				AssetTexture assetTexture = new AssetTexture();
				assetTexture.Open(nodeData.PopupImagePath);
				UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Chapter, assetTexture);
			}
			UIHelper.SetLabelText(Label_NodeName, LocalizationManager.GetStringDirect(nodeData.Name));
			UIHelper.SetLabelText(Label_Confirm, LocalizationManager.GetStringDirect("PORTAL_MOVE_NODE"));
		}
	}

	public void OnClickMove()
	{
		if (_listener != null)
		{
			_listener.MoveNode(ChapterIndex, DungeonIndex);
			GlobalAutoBehaviour<PopupManegement>.instance.CloseAll();
			PortalRenewal instance = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<PortalRenewal>(openedOnly: true);
			if (instance != null)
			{
				instance.Close();
			}
		}
		Vespa_System_Action_Extension_Method.fire(_actionClick);
	}

	public override void Reset()
	{
		_listener = null;
		_actionClick = null;
		ChapterIndex = 0;
		DungeonIndex = 0;
		base.Reset();
	}
}
