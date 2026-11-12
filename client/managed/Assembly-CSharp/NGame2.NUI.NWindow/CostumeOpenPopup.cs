using NGame2.NCutScene;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class CostumeOpenPopup : BaseWindow
{
	public interface Listener
	{
		void OnClick();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/SubStory/CostumeOpenPopup";

	public UILabel Label_Desc;

	public UITexture Texture_CostumePortrait;

	public float OpenDuration;

	private float _openedTime;

	private Listener _listener;

	private CostumeData _costumeData;

	public void Open(CostumeData costumeData, Listener listener)
	{
		if (costumeData != null)
		{
			Open(delegate
			{
				Init(costumeData, listener);
			}, null);
		}
	}

	public void Init(CostumeData costumeData, Listener listener)
	{
		_listener = listener;
		_openedTime = Time.time;
		_costumeData = costumeData;
		if (_costumeData == null)
		{
			VespaDebug.LogError("costumeData is null");
			return;
		}
		InitPortrait();
		SetLabel();
	}

	private void InitPortrait()
	{
		if (!String_Extension_Method.IsNullOrEmpty(_costumeData.IconPortraitPath))
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(_costumeData.IconPortraitPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_CostumePortrait, plainTexture);
		}
	}

	private void SetLabel()
	{
		UIHelper.SetLabelText(Label_Desc, _costumeData.CostumeName);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (IsOpen() && Time.time - _openedTime > OpenDuration)
		{
			Close();
		}
	}

	public void OnClickWindow()
	{
		if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Costume.ToString());
		}
		Close();
	}

	public static bool CanOpen()
	{
		if (SceneAutoBehaviour<CutSceneManager>.isValidInstance)
		{
			if (SceneAutoBehaviour<CutSceneManager>.instance.IsLoading)
			{
				return false;
			}
			if (SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying)
			{
				return false;
			}
		}
		return true;
	}
}
