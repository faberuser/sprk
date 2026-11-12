using NGame2.NAccount;
using NGame2.NNotify;
using NGame2.NUI.NManager.NLobby;
using NGameAsset;
using NShared;
using NShared.SetReviewPopup;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class AppReviewPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/AppReviewPopup";

	private static WindowPrefab<AppReviewPopup> _windowPrefab;

	public static LocalStorageString ErrorReviewPopup = new LocalStorageString(string.Empty, typeof(AppReviewPopup).FullName + "ErrorReviewPopup");

	public UILabel TitleLabel;

	public UILabel DescLabel;

	public UITexture PopupImage;

	private float _lastRefreshInfoTime;

	private bool OpenNotifyPopup;

	public static AppReviewPopup instance => _windowPrefab.instance;

	public static bool IsValidInstance => _windowPrefab.IsOpen();

	public new static bool Open()
	{
		return false;
	}

	public static void ForceOpen()
	{
	}

	private void OpenInternal()
	{
		base.Open();
		Init();
	}

	private void Init()
	{
		UIHelper.SetLabelText(TitleLabel, LocalizationManager.GetStringDirect("REVIEW_POPUP_TITLE"));
		UIHelper.SetLabelText(DescLabel, LocalizationManager.GetStringDirect("REVIEW_POPUP_DESC"));
		string text = "BMPopup/Img/Review_Popup";
		if (!String_Extension_Method.IsNullOrEmpty(text))
		{
			AssetTexture assetTexture = new AssetTexture();
			assetTexture.Open(text);
			UIHelper.ResetPlaneAssetTextureAsync(this, PopupImage, assetTexture);
		}
		else
		{
			VespaDebug.LogError("PopupImage is null");
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (!(Time.time - _lastRefreshInfoTime < 0.1f))
		{
			_lastRefreshInfoTime = Time.time;
			if (GlobalAutoBehaviour<InviteNotifyManager>.isValidInstance && GlobalAutoBehaviour<InviteNotifyManager>.instance.CurrentWindow != null)
			{
				base.Close();
			}
		}
	}

	public void RequestOpenAppReviewPopup(AppReviewPopupType appReviewPopupType)
	{
		NShared.SetReviewPopup.Request request = new NShared.SetReviewPopup.Request
		{
			ReviewType = appReviewPopupType
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetReviewPopup.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == ResultType.Success)
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.AppReviewPopup = (int)appReviewPopupType;
			}
		});
	}

	public void OnClickClose()
	{
		RequestOpenAppReviewPopup(AppReviewPopupType.PopupOff);
		Close();
	}

	public void OnClickReview()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			Application.OpenURL("https://play.google.com/store/apps/details?id=com.masang.kingsraid");
		}
		else
		{
			_ = Application.platform;
			_ = 8;
		}
		RequestOpenAppReviewPopup(AppReviewPopupType.AppReview);
		Close();
	}

	public void OnClickCS()
	{
		SceneAutoBehaviour<LobbyManagement>.instance.OpenCsMailPopup();
		OnClickClose();
	}
}
