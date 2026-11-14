using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NLogin;
using NGame2.NScene;
using NGame2.NScene.NPreloader;
using NGameAsset;
using NGameAsset.NPatch;
using NShader;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class BackgroundDownloader : MonoBehaviour, IGlobalWindow, IUpdater, DownloadWarningPopup.Listener
{
	public enum DownloadResultType
	{
		Success,
		DownloadFail,
		ConfigNotFound
	}

	public enum ProgressState
	{
		Normal,
		Complete
	}

	[Serializable]
	public class ProgressColor
	{
		public ProgressState State;

		public Color BgColor;

		public Color BarColor;
	}

	public class PriorityDownloadInfo
	{
		public string name;

		public long bytes;

		public float progress;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/BackgroundDownloader";

	public static LocalStorageBool IsOnlyPriorityDownload = new LocalStorageBool(i_default: false, typeof(BackgroundDownloader).FullName + "IsOnlyPriorityDownload");

	public static LocalStorageBool IsBackgroundDownload = new LocalStorageBool(i_default: false, typeof(BackgroundDownloader).FullName + "IsBackgroundDownload");

	public static LocalStorageString DownloadFileWriteSleepTime = new LocalStorageString(string.Empty, typeof(BackgroundDownloader).FullName + "DownloadFileWriteSleepTime");

	private static GlobalWindowPrefab<BackgroundDownloader> _windowPrefab = new GlobalWindowPrefab<BackgroundDownloader>("Resources/Prefab2/Window/BackgroundDownloader", GlobalUIRoot.ELayerType.GlobalUI);

	private static int DEFAULT_BACK_GROUND_DOWNLOAD_RESELECT_TIME = 30;

	private static int DEFAULT_BACK_GROUND_DOWNLOAD_RESELECT_FULLTIME = 60;

	private static float MIN_BACK_GROUND_DOWNLOAD_RESELECT_TIME = 1f;

	private static float MIN_BACK_GROUND_DOWNLOAD_RESELECT_FULLTIME = 1f;

	[Header("[User Input]")]
	public ProgressColor[] LoadingBarColor;

	[Header("[GameObject Link]")]
	public UIWidget ProgressRoot;

	public UILabel ProgressLabel;

	public UISprite Sprite_LoadingBg;

	public UISprite Sprite_LoadingBar;

	public Vector3 StandardButtonPos;

	public UIWidget PriorityProgressRoot;

	public UILabel PriorityProgressLabel;

	public UISprite Sprite_PriorityLoadingBar;

	[Header("[WaitPopup]")]
	public UIWidget WaitRoot;

	public UILabel WaitTitleLabel;

	public UILabel WaitContentLabel;

	public UILabel WaitOKButtonLabel;

	[Header("[Block]")]
	public UIPanel BlockRoot;

	[Header("[BlurTexture]")]
	public UIWidget BlurRoot;

	public UITexture BlurTexture;

	private bool _backgroundFilter;

	private string _cdnUrl;

	private PatchDownloader.PatchData _patchData;

	private bool _isDownloadAgreement;

	private bool _isPriorityDownloading;

	private float _priorityDownloadingTime;

	private float _priorityDownloadingFullTime;

	private Dictionary<string, PriorityDownloadInfo> _priorityDownloadProgress = new Dictionary<string, PriorityDownloadInfo>();

	private float _downloadReselectTime = 30f;

	private float _downloadReselectFullTime = 60f;

	private EDownloadCategory[] _categories;

	public Action<string> OnMessage { get; set; }

	public Action<float> OnProgressChanged { get; set; }

	public Action OnAgreement { get; set; }

	private string _CdnUrl
	{
		get
		{
			if (string.IsNullOrEmpty(_cdnUrl))
			{
				return null;
			}
			return Vespa_String_Extension_Method.CombineUrl(_cdnUrl, "LastBuildVersion.txt");
		}
	}

	public int MaxTotalArchiveCountInRuntime { get; set; }

	public virtual void Awake()
	{
		GlobalUpdateBehaviour.AddUpdater(this);
		UnityEngine.Object.DontDestroyOnLoad(base.transform.root.gameObject);
		PatchDownloader._startPriorityDownload = (PatchDownloader.FuncStartPriorityDownload)Delegate.Combine(PatchDownloader._startPriorityDownload, new PatchDownloader.FuncStartPriorityDownload(StartPriorityDownload));
		PatchDownloader._progressPriorityDownload = (PatchDownloader.ProgressPriorityDownload)Delegate.Combine(PatchDownloader._progressPriorityDownload, new PatchDownloader.ProgressPriorityDownload(ProgressPriorityDownload));
		PatchDownloader._completePriorityDownload = (PatchDownloader.CompletePriorityDownload)Delegate.Combine(PatchDownloader._completePriorityDownload, new PatchDownloader.CompletePriorityDownload(CompletePriorityDownload));
		GlobalAutoBehaviour<GuiManager>.instance.Add("PatchDownloader2", this, delegate
		{
			if (!String_Extension_Method.IsNullOrEmpty(GlobalAutoBehaviour<AccountManager>.instance.CreatedTime))
			{
				DateTime dateTime = DateTime.Parse(GlobalAutoBehaviour<AccountManager>.instance.CreatedTime);
				GUILayout.Label($"CreatedTime: {dateTime.ToLongDateString()} {dateTime.ToLongTimeString()}");
				DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
				GUILayout.Label($"ServerTime: {currentServerUTCTime.ToLongDateString()} {currentServerUTCTime.ToLongTimeString()}");
			}
			else
			{
				GUILayout.Label("CreatedTime is Null");
			}
			IsOnlyPriorityDownload.Value = GUILayout.Toggle(IsOnlyPriorityDownload.Value, "Stop Background Download");
			if (GUILayout.Button("SessionExpired", GUILayout.Width(150f)))
			{
				SingletonUpdater<LoginManager>.instance.OnSessionExpired();
			}
			if (GUILayout.Button("Open Wait Popup", GUILayout.Width(150f)))
			{
				OpenWaitPopup(LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_CONFIRM_TITLE), LocalizationManager.GetStringDirect("BACK_GROUND_DOWNLOAD_RESELECT_DESC", _downloadReselectTime));
			}
			if (GUILayout.Button("FilteredWindow", GUILayout.Width(150f)))
			{
				if (_backgroundFilter)
				{
					ReleaseBackgroundFilter();
				}
				else
				{
					SetBackgroundFilter(new BlurFilter());
				}
			}
		});
	}

	public void OnDestroy()
	{
		PatchDownloader._startPriorityDownload = (PatchDownloader.FuncStartPriorityDownload)Delegate.Remove(PatchDownloader._startPriorityDownload, new PatchDownloader.FuncStartPriorityDownload(StartPriorityDownload));
		PatchDownloader._progressPriorityDownload = (PatchDownloader.ProgressPriorityDownload)Delegate.Remove(PatchDownloader._progressPriorityDownload, new PatchDownloader.ProgressPriorityDownload(ProgressPriorityDownload));
		PatchDownloader._completePriorityDownload = (PatchDownloader.CompletePriorityDownload)Delegate.Remove(PatchDownloader._completePriorityDownload, new PatchDownloader.CompletePriorityDownload(CompletePriorityDownload));
	}

	public static void Open()
	{
		GetInstance().OpenInternal();
	}

	public static void Close()
	{
		GetInstance().CloseInternal();
	}

	public static GlobalWindowPrefab<BackgroundDownloader> GetPrefab()
	{
		return _windowPrefab;
	}

	public static BackgroundDownloader GetInstance()
	{
		return _windowPrefab.instance;
	}

	internal void OpenInternal()
	{
		IsBackgroundDownload.Value = true;
		base.gameObject.SetActive(value: true);
	}

	internal void CloseInternal()
	{
		IsBackgroundDownload.Value = false;
		UIHelper.EnableGameObject(ProgressRoot, enable: false);
		UIHelper.EnableGameObject(PriorityProgressRoot, enable: false);
		UIHelper.EnableGameObject(WaitRoot, enable: false);
		UIHelper.EnableGameObject(BlockRoot, enable: false);
		ReleaseBackgroundFilter();
		_isPriorityDownloading = false;
		_priorityDownloadProgress.Clear();
		_priorityDownloadingTime = 0f;
		_priorityDownloadingFullTime = 0f;
		Vespa_Common_Class_Extension_Method.Interface((IGlobalWindow)this).Close();
	}

	void IGlobalWindow.ReserveCache(UiPreloader preloader)
	{
	}

	bool IGlobalWindow.IsOpen()
	{
		return base.gameObject.activeSelf;
	}

	void IGlobalWindow.Close()
	{
		base.gameObject.SetActive(value: false);
		_patchData = null;
	}

	public void Init(string cdnUrl, EDownloadCategory[] categories)
	{
		_categories = categories;
		_cdnUrl = cdnUrl;
		_downloadReselectTime = GameRoot.Instance.ConstantDataContainer.GetValueInt("BsckGroundDownLoadReselectTime", DEFAULT_BACK_GROUND_DOWNLOAD_RESELECT_TIME);
		if (_downloadReselectTime < MIN_BACK_GROUND_DOWNLOAD_RESELECT_TIME)
		{
			_downloadReselectTime = MIN_BACK_GROUND_DOWNLOAD_RESELECT_TIME;
		}
		_downloadReselectFullTime = GameRoot.Instance.ConstantDataContainer.GetValueInt("BsckGroundDownLoadReselectFullTime", DEFAULT_BACK_GROUND_DOWNLOAD_RESELECT_FULLTIME);
		if (_downloadReselectFullTime < MIN_BACK_GROUND_DOWNLOAD_RESELECT_FULLTIME)
		{
			_downloadReselectFullTime = MIN_BACK_GROUND_DOWNLOAD_RESELECT_FULLTIME;
		}
		int length = Enum.GetValues(typeof(EDownloadCategory)).Length;
		if (PatchDownloader.DownloadedCategory.Count == length)
		{
			PatchDownloader.CurrentVersionPatchIsAllDownloaded = true;
		}
		if (DefinePlatform.IsEditor && NGameAsset.Define.LocalAssetLoading.Value)
		{
			OnDownloaded(PatchDownloader.EDownloadResponse.None, string.Empty);
		}
		else if (string.IsNullOrEmpty(_CdnUrl))
		{
			OnDownloaded(PatchDownloader.EDownloadResponse.CdnAddressEmpty, "CDN address is empty.");
		}
		else if (SystemOptionData.Spec == ESpecType.Undefine)
		{
			MessagePopup.OpenError("Undefine Resource Spec.", delegate
			{
				Reset();
				SingletonUpdater<LoginManager>.instance.ReturnToLogin();
			});
		}
		else
		{
			GlobalAutoBehaviour<PatchDownloader>.instance.GetPatchData(cdnUrl, OnGetPatchData);
		}
	}

	public void OnGetPatchData(PatchDownloader.PatchData patchData, PatchDownloader.EDownloadResponse response)
	{
		if (response == PatchDownloader.EDownloadResponse.OK)
		{
			_patchData = patchData;
			StartDownload();
		}
	}

	public void SetAlphaProgress(float alpha, float delay = 0f)
	{
		if (delay <= 0f)
		{
			UIHelper.SetWidgetAlpha(ProgressRoot, alpha);
		}
		else
		{
			VespaCoroutine.Start(coAlphaProgress(alpha, delay));
		}
	}

	private IEnumerator coAlphaProgress(float alpha, float delay)
	{
		yield return new WaitForSeconds(delay);
		UIHelper.SetWidgetAlpha(ProgressRoot, alpha);
	}

	private void OnDownloaded(PatchDownloader.EDownloadResponse downloadErrorCase, string error)
	{
		GlobalAutoBehaviour<PatchDownloader>.instance.State = PatchDownloader.DownloadState.Done;
		if (!string.IsNullOrEmpty(error))
		{
			MessagePopup.OpenError(error, delegate
			{
				CloseWithResult(DownloadResultType.DownloadFail);
			});
		}
		else
		{
			CloseWithResult(DownloadResultType.Success);
		}
	}

	private void CloseWithResult(DownloadResultType result)
	{
		if (result != DownloadResultType.Success)
		{
			GlobalAutoBehaviour<LoadingManager>.instance.Quit();
			return;
		}
		Vespa_System_Action_Extension_Method.fire(OnAgreement);
		if (SystemOptionData.DownloadType == EDownloadType.LoginPreDownload)
		{
			_ = (object)null;
			_ = 54;
			_ = 1;
			_ = 0;
			Array.Empty<object>();
		}
		else if (SystemOptionData.DownloadType == EDownloadType.LoginFullDownload)
		{
			_ = (object)null;
			_ = 53;
			_ = 1;
			_ = 0;
			Array.Empty<object>();
		}
		else
		{
			_ = (object)null;
			_ = 55;
			_ = 1;
			_ = 0;
			Array.Empty<object>();
		}
		Close();
	}

	public bool IsPriorityDownloading()
	{
		return _isPriorityDownloading;
	}

	private void SetPriorityDownloading(bool downloading)
	{
		if (_isPriorityDownloading != downloading)
		{
			_isPriorityDownloading = downloading;
			if (downloading)
			{
				SetBackgroundFilter(new BlurFilter());
				_priorityDownloadingTime = Time.time;
				_priorityDownloadingFullTime = Time.time;
			}
			else
			{
				ReleaseBackgroundFilter();
				_priorityDownloadingTime = 0f;
				_priorityDownloadingFullTime = 0f;
			}
		}
		UIHelper.EnableGameObject(BlockRoot, downloading);
	}

	private void UpdatePriorityDownloadingTime()
	{
		if (!_isPriorityDownloading || UIHelper.IsEnabledGameObjet(WaitRoot))
		{
			return;
		}
		if (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			_priorityDownloadingTime = Time.time;
			_priorityDownloadingFullTime = Time.time;
		}
		else if (_priorityDownloadingTime > 0f || _priorityDownloadingFullTime > 0f)
		{
			float num = Time.time - _priorityDownloadingTime;
			float num2 = Time.time - _priorityDownloadingFullTime;
			if (num > _downloadReselectTime)
			{
				OpenWaitPopup(LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_CONFIRM_TITLE), LocalizationManager.GetStringDirect("BACK_GROUND_DOWNLOAD_RESELECT_DESC", _downloadReselectTime));
			}
			else if (num2 > _downloadReselectFullTime)
			{
				OpenWaitPopup(LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_CONFIRM_TITLE), LocalizationManager.GetStringDirect("BACK_GROUND_DOWNLOAD_RESELECT_DESC", _downloadReselectFullTime));
			}
		}
	}

	private void OpenWaitPopup(string title, string content)
	{
		UIHelper.SetLabelText(WaitTitleLabel, title);
		UIHelper.SetLabelText(WaitContentLabel, content);
		UIHelper.EnableGameObject(WaitRoot, enable: true);
	}

	public void OnClickWaitOK()
	{
		UIHelper.EnableGameObject(WaitRoot, enable: false);
		if (_isPriorityDownloading)
		{
			_isPriorityDownloading = false;
		}
		SingletonUpdater<LoginManager>.instance.ReturnToLogin();
	}

	public void OnClickWaitCancel()
	{
		UIHelper.EnableGameObject(WaitRoot, enable: false);
		_priorityDownloadingTime = (_isPriorityDownloading ? Time.time : 0f);
		_priorityDownloadingFullTime = (_isPriorityDownloading ? Time.time : 0f);
	}

	private ESpecType GetSuitableSpec()
	{
		if (SystemInfo.systemMemorySize >= 2200)
		{
			return ESpecType.Mid;
		}
		return ESpecType.Low;
	}

	private void StartDownload(ESpecType specType)
	{
		SystemOptionData.Spec = specType;
		StartDownload();
	}

	private void StartDownload()
	{
		Vespa_System_Action_Extension_Method.fire(OnMessage, "Start Downloading Assets");
		if (_patchData == null)
		{
			VespaDebug.LogError("_remotePatchVersionInfo is null.");
			Vespa_System_Action_Extension_Method.fire(OnAgreement);
			return;
		}
		UIHelper.EnableGameObject(ProgressRoot, enable: true);
		SystemOptionData.DownloadType = EDownloadType.BackgroundDownload;
		GlobalAutoBehaviour<PatchDownloader>.instance.SetMaxParallelDownloadCount(2);
		_patchData.PatchInfo.FilterCategory(_categories);
		GlobalAutoBehaviour<PatchDownloader>.instance.StartDownload(_patchData, OnDownloaded, OnMessage, CheckAgreeDownload, abandonLoad: false);
	}

	private void CheckAgreeDownload(long totalDownloadBytes, Action<bool> onAgree)
	{
		_ = LoginData.LastLoginMethod;
		_ = LoginData.LastServerName;
		if (totalDownloadBytes == 0L)
		{
			Vespa_System_Action_Extension_Method.fire(onAgree, i_argA: true);
			Vespa_System_Action_Extension_Method.fire(OnAgreement);
			return;
		}
		string title = LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_CONFIRM_TITLE);
		string text = LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_CONFIRM_MESSAGE_BACK_GROUND);
		text = text + "\n" + LocalizationManager.GetString(Patch.DOWNLOAD_FILE_SIZE, Vespa_Long_Extension_Method.LongToByteFormat(totalDownloadBytes));
		_isDownloadAgreement = true;
		if (_isDownloadAgreement)
		{
			Vespa_System_Action_Extension_Method.fire(onAgree, i_argA: true);
			Vespa_System_Action_Extension_Method.fire(OnAgreement);
			return;
		}
		GlobalMessagePopup.OpenYesOnly(title, text, delegate
		{
			_isDownloadAgreement = true;
			Vespa_System_Action_Extension_Method.fire(onAgree, i_argA: true);
			Vespa_System_Action_Extension_Method.fire(OnAgreement);
		}, ignoreOnClickClose: true);
	}

	public Color GetLoadingBarColor(ProgressState state)
	{
		for (int i = 0; i < LoadingBarColor.Length; i++)
		{
			if (LoadingBarColor[i].State == state)
			{
				return LoadingBarColor[i].BarColor;
			}
		}
		return Color.white;
	}

	public Color GetLoadingBgColor(ProgressState state)
	{
		for (int i = 0; i < LoadingBarColor.Length; i++)
		{
			if (LoadingBarColor[i].State == state)
			{
				return LoadingBarColor[i].BgColor;
			}
		}
		return Color.black;
	}

	private void SetProgressView(ProgressState state, float progress)
	{
		Color loadingBgColor = GetLoadingBgColor(state);
		UIHelper.SetWidgetColor(Sprite_LoadingBar, GetLoadingBarColor(state));
		UIHelper.SetWidgetColor(Sprite_LoadingBg, loadingBgColor);
		UIHelper.SetLabelColor(ProgressLabel, GetLoadingBarColor(state));
		UIHelper.SetSpriteFillAmount(Sprite_LoadingBar, progress);
	}

	public void Reset()
	{
		if (_patchData != null)
		{
			_patchData.Clear();
		}
		_patchData = null;
	}

	public void UpdateDerived()
	{
		if (ProgressRoot == null || !ProgressRoot.gameObject.activeInHierarchy)
		{
			return;
		}
		if (DefinePlatform.IsEditor && NGameAsset.Define.LocalAssetLoading.Value)
		{
			SetProgressView(ProgressState.Complete, 1f);
			return;
		}
		switch (GlobalAutoBehaviour<PatchDownloader>.instance.State)
		{
		case PatchDownloader.DownloadState.Checking:
			UIHelper.SetLabelText(ProgressLabel, string.Empty);
			SetProgressView(ProgressState.Normal, 0f);
			break;
		case PatchDownloader.DownloadState.Started:
		case PatchDownloader.DownloadState.Pause:
		{
			int totalArchiveCount = GlobalAutoBehaviour<PatchDownloader>.instance.TotalArchiveCount;
			if (MaxTotalArchiveCountInRuntime < totalArchiveCount)
			{
				MaxTotalArchiveCountInRuntime = totalArchiveCount;
			}
			int maxTotalArchiveCountInRuntime = MaxTotalArchiveCountInRuntime;
			int num = maxTotalArchiveCountInRuntime - totalArchiveCount;
			double downloadedFraction = GlobalAutoBehaviour<PatchDownloader>.instance.GetDownloadedFraction();
			if (GlobalAutoBehaviour<PatchDownloader>.instance.IsDownloadClear())
			{
				break;
			}
			if (totalArchiveCount == 0)
			{
				SetProgressView(ProgressState.Complete, 1f);
				break;
			}
			float num2 = (float)downloadedFraction / (float)totalArchiveCount;
			if (num2 > 0.9999f)
			{
				Vespa_System_Action_Extension_Method.fire(OnProgressChanged, 1f);
				UIHelper.EnableGameObject(ProgressRoot, enable: false);
				break;
			}
			if (num > 0)
			{
				num2 = ((float)downloadedFraction + (float)num) / (float)maxTotalArchiveCountInRuntime;
			}
			Vespa_System_Action_Extension_Method.fire(OnProgressChanged, num2);
			float num3 = num2 * 100f;
			if (num3 <= 0f)
			{
				num3 = 0f;
			}
			if (num3 >= 100f)
			{
				num3 = 100f;
			}
			string text = string.Format("{0}%", num3.ToString("F1"));
			UIHelper.SetLabelText(ProgressLabel, text);
			SetProgressView(ProgressState.Normal, num2);
			break;
		}
		case PatchDownloader.DownloadState.Done:
			SetProgressView(ProgressState.Complete, 1f);
			break;
		default:
			SetProgressView(ProgressState.Normal, 0f);
			break;
		}
	}

	public void StartPriorityDownload(string name, long bytes)
	{
		SetPriorityDownloading(downloading: true);
		UIHelper.EnableGameObject(PriorityProgressRoot, enable: true);
		lock (_priorityDownloadProgress)
		{
			PriorityDownloadInfo priorityDownloadInfo = new PriorityDownloadInfo();
			priorityDownloadInfo.name = name;
			priorityDownloadInfo.bytes = bytes;
			priorityDownloadInfo.progress = 0f;
			if (_priorityDownloadProgress.ContainsKey(name))
			{
				_priorityDownloadProgress[name] = priorityDownloadInfo;
			}
			else
			{
				_priorityDownloadProgress.Add(name, priorityDownloadInfo);
			}
			SetPriorityProgressBar(name);
		}
	}

	public void ProgressPriorityDownload(string name, float progress)
	{
		lock (_priorityDownloadProgress)
		{
			if (_priorityDownloadProgress.ContainsKey(name))
			{
				UIHelper.EnableGameObject(PriorityProgressRoot, enable: true);
				_priorityDownloadProgress[name].progress = progress;
				SetPriorityProgressBar(name);
			}
		}
	}

	public void CompletePriorityDownload(string name)
	{
		lock (_priorityDownloadProgress)
		{
			bool flag = false;
			if (_priorityDownloadProgress.ContainsKey(name))
			{
				_priorityDownloadProgress.Remove(name);
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			if (__Extension_Method_ICollection__.COUNT(_priorityDownloadProgress) <= 0)
			{
				UIHelper.SetSpriteFillAmount(Sprite_PriorityLoadingBar, 1f);
				UIHelper.SetLabelText(PriorityProgressLabel, SprkText.Get("Download Complete"));
				UIHelper.EnableGameObject(PriorityProgressRoot, enable: false);
				SetPriorityDownloading(downloading: false);
			}
			else
			{
				if (_priorityDownloadingTime != 0f)
				{
					_priorityDownloadingTime = Time.time;
				}
				SetPriorityProgressBar(name);
			}
		}
	}

	private void SetPriorityProgressBar(string name)
	{
		lock (_priorityDownloadProgress)
		{
			if (__Extension_Method_ICollection__.COUNT(_priorityDownloadProgress) == 0 || !_priorityDownloadProgress.ContainsKey(name))
			{
				return;
			}
			long num = 0L;
			long num2 = 0L;
			foreach (KeyValuePair<string, PriorityDownloadInfo> item in _priorityDownloadProgress)
			{
				num += item.Value.bytes;
				num2 += item.Value.bytes * (long)item.Value.progress;
			}
			if (num != 0L)
			{
				double num3 = (double)num2 / (double)num;
				if (num3 >= 1.0)
				{
					num3 = 1.0;
				}
				UIHelper.SetSpriteFillAmount(Sprite_PriorityLoadingBar, (float)num3);
				long num4 = num2 / 1048576;
				long num5 = num / 1048576;
				string stringDirect = LocalizationManager.GetStringDirect("BACK_GROUND_DOWNLOAD_BOTTOM_DESC", num4, num5);
				if (!String_Extension_Method.IsNullOrEmpty(stringDirect))
				{
					UIHelper.SetLabelText(PriorityProgressLabel, stringDirect);
				}
				else
				{
					UIHelper.SetLabelText(PriorityProgressLabel, SprkText.Format("Downloading... ({0}/{1}MB)", num4, num5));
				}
			}
		}
	}

	public void OnConfirmAndStartDownload(ESpecType specType)
	{
		UIHelper.EnableGameObject(this, enable: true);
		StartDownload(specType);
	}

	public void OnShowDownloadSelectPopup()
	{
		UIHelper.EnableGameObject(this, enable: true);
	}

	void IUpdater.ChainUpdate()
	{
		if (base.isActiveAndEnabled)
		{
			UpdateDerived();
			UpdatePriorityDownloadingTime();
		}
	}

	private void SetBackgroundFilter(IFilter filter)
	{
		if (!_backgroundFilter)
		{
			float f = 100f;
			RenderTexture renderTexture = GlobalAutoBehaviour<GlobalCameraManager>.instance.CaptureCamera(Mathf.RoundToInt(f), filter);
			if (!(renderTexture == null))
			{
				UIHelper.EnableGameObject(BlurRoot, enable: true);
				BlurTexture.gameObject.SetActive(value: true);
				BlurTexture.mainTexture = renderTexture;
				_backgroundFilter = true;
			}
		}
	}

	private void ReleaseBackgroundFilter()
	{
		if (_backgroundFilter)
		{
			_backgroundFilter = false;
			RenderTexture renderTexture = BlurTexture.mainTexture as RenderTexture;
			if (renderTexture != null)
			{
				VespaObject.DestroyObject(renderTexture);
				BlurTexture.mainTexture = null;
				BlurTexture.gameObject.SetActive(value: false);
			}
			UIHelper.EnableGameObject(BlurRoot, enable: false);
		}
	}
}
