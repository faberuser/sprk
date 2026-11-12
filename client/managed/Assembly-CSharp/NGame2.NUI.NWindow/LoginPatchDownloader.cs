using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NLogin;
using NGame2.NPatch;
using NGameAsset;
using NGameAsset.NPatch;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LoginPatchDownloader : BaseWindow, DownloadWarningPopup.Listener
{
	[Serializable]
	public class SpecButtonInfo
	{
		public ESpecType SpecType;

		public UILabel SizeLabel;

		public GameObject buttonObject;

		public UILabel LabelInfo;

		public UILabel LabelInfoUnable;
	}

	public enum DownloadResultType
	{
		Success,
		DownloadFail,
		ConfigNotFound
	}

	public enum LastOpenPopup
	{
		None,
		FirstDownload,
		SelectDownload
	}

	private enum DefaultSpecType
	{
		LocalStorage,
		NewUserDefault,
		Suitable
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginPatchDownloader";

	public static LocalStorageBool IsOverwriteRemotePatchVersion = new LocalStorageBool(i_default: false, typeof(LoginPatchDownloader).FullName + "IsOverwriteRemotePatchVersion");

	public static LocalStorageBool IsStopDownload = new LocalStorageBool(i_default: false, typeof(LoginPatchDownloader).FullName + "IsStopDownload");

	public static LocalStorageString OverwriteRemotePatchVersionNumber = new LocalStorageString(string.Empty, typeof(LoginPatchDownloader).FullName + "OverwriteRemotePatchVersionNumber");

	public UIWidget SelectSpecRoot;

	public UIGrid SelectSpecGrid;

	public UIWidget SelectSpecButtonRoot;

	public UIWidget ProgressRoot;

	public UILabel ProgressLabel;

	public UISprite Sprite_LoadingBar;

	public UISprite Sprite_MainFrameSmall;

	public Vector3 StandardButtonPos;

	public SpecButtonInfo[] SpecButtons;

	public UIWidget FirstDownloadRoot;

	public UIButton ButtonSelectSpec;

	public UILabel DownloadMessageLabel;

	public UILabel DownloadTitleLabel;

	public UILabel DownloadQualityLabel;

	[Header("[SelectDownloadPopup]")]
	public UIWidget SelctDownloadRoot;

	public UILabel SelectDownloadTitleLabel;

	public UIButton SelectDownloadSpecButton;

	public UIButton SelectDownloadTypeButton;

	public UILabel SelectBackgroundMessageLabel;

	public UILabel SelectBackgroundPredownloadSizeLabel;

	public UILabel SelectBackgroundDownloadSizeLabel;

	public UILabel SelectBackgroundWIFILabel;

	public UILabel SelectBackgroundQualityLabel;

	public UILabel SelectBackgroundDownloadTypeLabel;

	public UILabel SelectBackgroundStartButtonLabel;

	[Header("[SelectDownloadType]")]
	public UIWidget SelectDownloadTypeRoot;

	private string _cdnProxyUrl;

	private PatchDownloader.PatchData _patchData;

	private int _originHeight;

	private ESpecType _currentSelectedSpec;

	private List<Vector3> listSpectButtonPos = new List<Vector3>();

	private bool _isOpenedFirstDownload;

	private LastOpenPopup _lastOpenPopup;

	private Action<DownloadResultType> _onDownloaded;

	private Action<int> _onVersionChecked;

	private Action<int> _onRemoteVersion;

	private static readonly long MAX_AUTO_FULL_DOWNLOAD_IN_MB = 100L;

	public Action<string> OnMessage { get; set; }

	public Action<float> OnProgressChanged { get; set; }

	public void Init(string cdnProxyUrl, Action<int> onVersionChecked, Action<DownloadResultType> onDownloaded)
	{
		_cdnProxyUrl = cdnProxyUrl;
		_onDownloaded = onDownloaded;
		_onVersionChecked = onVersionChecked;
		_onRemoteVersion = OnRemoteVersion;
		if (DefinePlatform.IsEditor && Define.LocalAssetLoading.Value)
		{
			Vespa_System_Action_Extension_Method.fire(_onVersionChecked, 0);
			OnDownloaded(PatchDownloader.EDownloadResponse.None, null);
			return;
		}
		if (string.IsNullOrEmpty(_cdnProxyUrl))
		{
			Vespa_System_Action_Extension_Method.fire(_onVersionChecked, 0);
			OnDownloaded(PatchDownloader.EDownloadResponse.CdnAddressEmpty, "CDN address is empty.");
			return;
		}
		UIHelper.EnableButton(SelectDownloadSpecButton, IsATSCSupported(), !IsATSCSupported());
		UIHelper.EnableButton(ButtonSelectSpec, IsATSCSupported(), !IsATSCSupported());
		if (SystemOptionData.Spec == ESpecType.Undefine)
		{
			UIHelper.EnableGameObject(SelectSpecRoot, enable: false);
			UIHelper.EnableGameObject(SelectDownloadTypeRoot, enable: false);
			UIHelper.EnableGameObject(ProgressRoot, enable: false);
			QueryRemotePatchVersionInfo();
		}
		else
		{
			UIHelper.EnableGameObject(SelectSpecRoot, enable: false);
			UIHelper.EnableGameObject(SelectDownloadTypeRoot, enable: false);
			QueryRemotePatchVersionInfo();
		}
		if (!GameRoot.isValidInstance)
		{
			return;
		}
		PatchPlatform platform = PatchDownloadHelper.GetCurrentPlatform();
		GameRoot.Instance.PatchDownloadSpecDataContainer.ForEachData(delegate(PatchDownloadSpecData data)
		{
			if (platform == data.Platform)
			{
				switch (data.Spec)
				{
				case PatchSpecType.Low:
					SpecButtons[2].LabelInfo.text = LocalizationManager.GetString(Patch.DOWNLOAD_QUALITY_DEVICE_MEMORY, data.RamMin / 1024);
					break;
				case PatchSpecType.Mid:
					SpecButtons[1].LabelInfo.text = LocalizationManager.GetString(Patch.DOWNLOAD_QUALITY_DEVICE_MEMORY, data.RamMin / 1024);
					break;
				case PatchSpecType.High:
					SpecButtons[0].LabelInfo.text = LocalizationManager.GetString(Patch.DOWNLOAD_QUALITY_DEVICE_MEMORY, data.RamMin / 1024);
					break;
				}
			}
		});
	}

	private bool IsATSCSupported()
	{
		return UIHelper.IsATSCSupported();
	}

	private PatchDownloadSpecData GetRecommandedSpecData()
	{
		if (!GameRoot.isValidInstance)
		{
			return null;
		}
		return PatchDownloadHelper.GetRecommandSpecData();
	}

	private ESpecType ConvertNGameAssetSpecType(PatchSpecType spec)
	{
		return spec switch
		{
			PatchSpecType.Low => ESpecType.Low, 
			PatchSpecType.Mid => ESpecType.Mid, 
			PatchSpecType.High => ESpecType.Original, 
			_ => ESpecType.Undefine, 
		};
	}

	private ESpecType GetDefaultSpec()
	{
		ESpecType eSpecType = SystemOptionData.Spec;
		if (eSpecType == ESpecType.Undefine)
		{
			PatchDownloadSpecData recommandedSpecData = GetRecommandedSpecData();
			if (recommandedSpecData != null)
			{
				eSpecType = ConvertNGameAssetSpecType(recommandedSpecData.StandardSpec);
			}
		}
		return eSpecType;
	}

	public void OnRemoteVersion(int remoteVersion)
	{
		_ = SystemOptionData.Spec;
		IsNewUser();
		Vespa_System_Action_Extension_Method.fire(_onVersionChecked, remoteVersion);
		UpdateDownloadSize();
		_currentSelectedSpec = GetDefaultSpec();
		GetRecommandedSpecData();
		SystemOptionData.DownloadType = EDownloadType.LoginFullDownload;
		OnClickStartDownload();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		if (_patchData != null)
		{
			_patchData.Clear();
		}
		_patchData = null;
	}

	public void StopDownload()
	{
		GlobalAutoBehaviour<PatchDownloader>.instance.StopDownload();
	}

	public int GetRemoteVersion()
	{
		if (_patchData == null)
		{
			return 0;
		}
		return _patchData.Version;
	}

	public bool IsNewUser()
	{
		bool isValidInstance = SingletonUpdater<LoginManager>.isValidInstance;
		bool isNewUser = SingletonUpdater<LoginManager>.instance.IsNewUser;
		bool value = SingletonUpdater<LoginManager>.instance.ForceBackgroundDownload.Value;
		if (isValidInstance)
		{
			return isNewUser || value;
		}
		return false;
	}

	private void QueryRemotePatchVersionInfo()
	{
		Vespa_System_Action_Extension_Method.fire(OnMessage, "Download RemoteVersionInfo");
		GlobalAutoBehaviour<PatchDownloader>.instance.GetPatchData(_cdnProxyUrl, OnGetPatchData);
	}

	public void OnGetPatchData(PatchDownloader.PatchData patchData, PatchDownloader.EDownloadResponse response)
	{
		if (response == PatchDownloader.EDownloadResponse.OK)
		{
			_patchData = patchData;
			Vespa_System_Action_Extension_Method.fire(_onRemoteVersion, (_patchData != null) ? _patchData.Version : 0);
		}
		else
		{
			OnDownloaded(response, response.ToString());
		}
	}

	private ESpecType GetSuitableSpec()
	{
		if (SystemInfo.systemMemorySize >= 2200)
		{
			return ESpecType.Mid;
		}
		return ESpecType.Low;
	}

	public void OpenFirstDownloadPopup(ESpecType spec)
	{
		_lastOpenPopup = LastOpenPopup.FirstDownload;
		_currentSelectedSpec = spec;
		UIHelper.EnableGameObject(FirstDownloadRoot, enable: true);
		SpecButtonInfo specButtonInfo = GetSpecButtonInfo(spec);
		string text = LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_CONFIRM_TITLE);
		string text2 = LocalizationManager.GetString(Patch.DOWNLOAD_QUALITY_SELECT_MESSAGE);
		text2 = text2 + "\n" + LocalizationManager.GetString(Patch.DOWNLOAD_FILE_SIZE, specButtonInfo.SizeLabel.text);
		text2 += $"\n [sub][c][ff0000]{LocalizationManager.GetString(Patch.DOWNLOAD_RECOMMEND_USE_WIFI_MESSAGE)}[-][/c][/sub]";
		UIHelper.SetLabelText(DownloadTitleLabel, text);
		UIHelper.SetLabelText(DownloadMessageLabel, text2);
		string text3 = string.Format("{0} : ", LocalizationManager.GetStringDirect("GAME_QUALITY"));
		switch (spec)
		{
		case ESpecType.Original:
			text3 += LocalizationManager.GetStringDirect("GAME_QUALITY_HIGH_TITLE");
			break;
		case ESpecType.Mid:
			text3 += LocalizationManager.GetStringDirect("GAME_QUALITY_RECOMMENDED_TITLE");
			break;
		case ESpecType.Low:
			text3 += LocalizationManager.GetStringDirect("GAME_QUALITY_LOW_TITLE");
			break;
		}
		UIHelper.SetLabelText(DownloadQualityLabel, text3);
		_isOpenedFirstDownload = true;
	}

	public void OpenSelectDownloadPopup(ESpecType spec, bool showSpecSelect = true)
	{
		_lastOpenPopup = LastOpenPopup.SelectDownload;
		_currentSelectedSpec = spec;
		bool flag = IsATSCSupported() && showSpecSelect;
		UIHelper.EnableButton(SelectDownloadSpecButton, flag, !flag);
		GlobalAutoBehaviour<PatchDownloader>.instance.DetermineIsFullDownLoadUser(_patchData);
		OpenSelectDownloadPopup(spec);
	}

	private void OpenSelectDownloadPopup(ESpecType spec)
	{
		_currentSelectedSpec = spec;
		PatchInfoData patchInfoData = (PatchInfoData)_patchData.PatchInfo.Clone();
		patchInfoData.FilterBySpec(spec);
		patchInfoData.FilterByLanguage(SingletonAuto<LocalizationManager>.instance.Language);
		patchInfoData.FilterCategory(new EDownloadCategory[1]);
		patchInfoData.FilterByVideoSpec();
		PatchInfoData patchInfoData2 = (PatchInfoData)_patchData.PatchInfo.Clone();
		patchInfoData2.FilterBySpec(spec);
		patchInfoData2.FilterByLanguage(SingletonAuto<LocalizationManager>.instance.Language);
		patchInfoData2.FilterCategory(new EDownloadCategory[2]
		{
			EDownloadCategory.PreDownload,
			EDownloadCategory.EnterLobbyScene
		});
		patchInfoData2.FilterByVideoSpec();
		List<long> list = new List<long>();
		list.Add(patchInfoData.GetDownloadSize(_patchData.LocalPatchInfo));
		list.Add(patchInfoData2.GetDownloadSize(_patchData.LocalPatchInfo));
		if (list == null || __Extension_Method_ICollection__.COUNT(list) <= 0)
		{
			SystemOptionData.DownloadType = EDownloadType.LoginFullDownload;
			OnClickStartDownload();
			return;
		}
		if (list[list.Count - 1] <= MAX_AUTO_FULL_DOWNLOAD_IN_MB * 1024 * 1024)
		{
			SystemOptionData.DownloadType = EDownloadType.LoginFullDownload;
			OnClickStartDownload();
			return;
		}
		string text = "??";
		switch (spec)
		{
		case ESpecType.Original:
			text = LocalizationManager.GetStringDirect("GAME_QUALITY_HIGH_TITLE");
			break;
		case ESpecType.Mid:
			text = LocalizationManager.GetStringDirect("GAME_QUALITY_RECOMMENDED_TITLE");
			break;
		case ESpecType.Low:
			text = LocalizationManager.GetStringDirect("GAME_QUALITY_LOW_TITLE");
			break;
		}
		string text2 = LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_CHOICE);
		string text3 = $"{Vespa_Long_Extension_Method.ToCommaString(list[0] / 1048576)} MB";
		string text4 = $"{LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_CONFIRM_MESSAGE)}\n{LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_CHOICE_DESC)}";
		string text5 = LocalizationManager.GetString(Patch.DOWNLOAD_FILE_MIN_SIZE, text3);
		string text6 = "?? MB";
		if (__Extension_Method_ICollection__.COUNT(list) > 1)
		{
			text6 = $"{Vespa_Long_Extension_Method.ToCommaString((list[1] - list[0]) / 1048576)} MB";
		}
		string text7 = LocalizationManager.GetString(Patch.DOWNLOAD_FILE_BACKGROUND_SIZE, text6);
		string text8 = LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_WIFI);
		LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_ALL_NAME);
		LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_ALL);
		string text9 = "?? MB";
		if (__Extension_Method_ICollection__.COUNT(list) > 1)
		{
			text9 = $"{Vespa_Long_Extension_Method.ToCommaString(list[1] / 1048576)} MB";
		}
		string text10 = LocalizationManager.GetString(Patch.DOWNLOAD_FILE_SIZE, text9);
		UIHelper.SetLabelText(SelectDownloadTitleLabel, text2);
		UIHelper.SetLabelText(SelectBackgroundMessageLabel, text4);
		UIHelper.SetLabelText(SelectBackgroundWIFILabel, text8);
		UIHelper.SetLabelText(SelectBackgroundQualityLabel, text);
		if (SystemOptionData.DownloadType == EDownloadType.Undefine)
		{
			SystemOptionData.DownloadType = EDownloadType.LoginPreDownload;
		}
		bool flag = IsNewUser();
		if (!flag)
		{
			SystemOptionData.DownloadType = EDownloadType.LoginFullDownload;
		}
		UIHelper.EnableButton(SelectDownloadTypeButton, flag, !flag);
		if (SystemOptionData.DownloadType == EDownloadType.LoginFullDownload)
		{
			UIHelper.SetLabelText(SelectBackgroundDownloadTypeLabel, LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_ALL_NAME));
			UIHelper.SetLabelText(SelectBackgroundPredownloadSizeLabel, text10);
			UIHelper.SetLabelText(SelectBackgroundDownloadSizeLabel, LocalizationManager.GetString(Patch.DOWNLOAD_FILE_BACKGROUND_SIZE, "0MB"));
			UIHelper.SetLabelText(SelectBackgroundStartButtonLabel, LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_ALL_NAME));
		}
		else if (SystemOptionData.DownloadType == EDownloadType.LoginPreDownload)
		{
			UIHelper.SetLabelText(SelectBackgroundDownloadTypeLabel, LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_BACKGROUND_NAME));
			UIHelper.SetLabelText(SelectBackgroundPredownloadSizeLabel, text5);
			UIHelper.SetLabelText(SelectBackgroundDownloadSizeLabel, text7);
			UIHelper.SetLabelText(SelectBackgroundStartButtonLabel, LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_BACKGROUND_NAME));
		}
		else
		{
			SystemOptionData.DownloadType = EDownloadType.LoginFullDownload;
			UIHelper.SetLabelText(SelectBackgroundDownloadTypeLabel, LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_ALL_NAME));
			UIHelper.SetLabelText(SelectBackgroundPredownloadSizeLabel, text10);
			UIHelper.SetLabelText(SelectBackgroundDownloadSizeLabel, LocalizationManager.GetString(Patch.DOWNLOAD_FILE_BACKGROUND_SIZE, "0MB"));
			UIHelper.SetLabelText(SelectBackgroundStartButtonLabel, LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_ALL_NAME));
		}
		UIHelper.EnableGameObject(SelctDownloadRoot, enable: true);
		_isOpenedFirstDownload = true;
	}

	public void OpenDownloadTypeSelectPopup()
	{
		UIHelper.EnableGameObject(SelectDownloadTypeRoot, enable: true);
		_ = (object)null;
		_ = 50;
		_ = 1;
		_ = 0;
		Array.Empty<object>();
	}

	public void OpenSpecSelectPopup()
	{
		UIHelper.EnableGameObject(SelectSpecRoot, enable: true);
		ShowSuitableSpecButton();
	}

	private void ShowSuitableSpecButton()
	{
		Dictionary<PatchSpecType, SpecButtonInfo> obj = new Dictionary<PatchSpecType, SpecButtonInfo>
		{
			[PatchSpecType.High] = SpecButtons[0],
			[PatchSpecType.Mid] = SpecButtons[1],
			[PatchSpecType.Low] = SpecButtons[2]
		};
		PatchDownloadSpecData recommandedSpecData = GetRecommandedSpecData();
		foreach (KeyValuePair<PatchSpecType, SpecButtonInfo> item in obj)
		{
			PatchSpecType key = item.Key;
			SpecButtonInfo value = item.Value;
			if (value != null)
			{
				if (recommandedSpecData.SelectSpecInPatchDownload.Contains(key))
				{
					bool flag = !CheckLossQualitySpec(key);
					UIHelper.EnableGameObject(value.buttonObject, enable: true);
					UIHelper.EnableGameObject(value.LabelInfo, flag);
					UIHelper.EnableGameObject(value.LabelInfoUnable, !flag);
					SelectSpecGrid.AddChild(value.buttonObject.transform);
				}
				else
				{
					UIHelper.EnableGameObject(value.buttonObject, enable: false);
					UIHelper.EnableGameObject(value.LabelInfo, enable: false);
					UIHelper.EnableGameObject(value.LabelInfoUnable, enable: false);
					value.buttonObject.transform.SetParent(SelectSpecButtonRoot.transform);
				}
			}
		}
	}

	private bool CheckLossQualitySpec(PatchSpecType type)
	{
		PatchDownloadSpecData recommandedSpecData = GetRecommandedSpecData();
		bool result = false;
		switch (recommandedSpecData.StandardSpec)
		{
		case PatchSpecType.Low:
			result = false;
			break;
		case PatchSpecType.Mid:
			result = type == PatchSpecType.Low;
			break;
		case PatchSpecType.High:
			result = type == PatchSpecType.Low || type == PatchSpecType.Mid;
			break;
		}
		return result;
	}

	public SpecButtonInfo GetSpecButtonInfo(ESpecType spec)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(SpecButtons); i++)
		{
			if (SpecButtons[i].SpecType == spec)
			{
				return SpecButtons[i];
			}
		}
		return null;
	}

	private long DownloadSize(PatchDownloader.PatchData patchData, ESpecType spec)
	{
		if (patchData == null || patchData.PatchInfo == null)
		{
			return 0L;
		}
		PatchInfoData obj = (PatchInfoData)patchData.PatchInfo.Clone();
		obj.FilterBySpec(spec);
		obj.FilterByLanguage(SingletonAuto<LocalizationManager>.instance.Language);
		obj.FilterByVideoSpec();
		obj.FilterCategory(GetDownloadCategories());
		return obj.GetDownloadSize(patchData.LocalPatchInfo) / 1048576;
	}

	private void UpdateDownloadSize()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(SpecButtons); i++)
		{
			SpecButtonInfo specButtonInfo = SpecButtons[i];
			if (specButtonInfo != null)
			{
				ESpecType specType = specButtonInfo.SpecType;
				UILabel sizeLabel = specButtonInfo.SizeLabel;
				UIHelper.SetLabelText(sizeLabel, "??");
				if (_patchData != null && _patchData.PatchInfo != null && (PatchInfoData)_patchData.PatchInfo.Clone() != null)
				{
					long num = DownloadSize(_patchData, specType);
					UIHelper.SetLabelText(sizeLabel, Vespa_Long_Extension_Method.ToCommaString(num) + " MB");
				}
			}
		}
	}

	public void OnClickSelectSpec(ESpecType eSpec)
	{
		OnClickCloseSelectSpec();
		if (_lastOpenPopup == LastOpenPopup.FirstDownload)
		{
			OpenFirstDownloadPopup(eSpec);
		}
		else if (_lastOpenPopup == LastOpenPopup.SelectDownload)
		{
			OpenSelectDownloadPopup(eSpec);
		}
		else
		{
			OpenFirstDownloadPopup(eSpec);
		}
	}

	public void OnSelectSpec(GameObject buttonInfoRoot)
	{
		ESpecType eSpec = ESpecType.Undefine;
		if (buttonInfoRoot == null)
		{
			return;
		}
		SpecButtonInfo[] specButtons = SpecButtons;
		foreach (SpecButtonInfo specButtonInfo in specButtons)
		{
			if (specButtonInfo.buttonObject.Equals(buttonInfoRoot))
			{
				eSpec = specButtonInfo.SpecType;
				break;
			}
		}
		OnClickSelectSpec(eSpec);
	}

	public void OnClickSelectDownloadType(EDownloadType eType)
	{
		UIHelper.EnableGameObject(SelectDownloadTypeRoot, enable: false);
		switch (eType)
		{
		case EDownloadType.LoginFullDownload:
			SystemOptionData.DownloadType = EDownloadType.LoginFullDownload;
			break;
		case EDownloadType.LoginPreDownload:
			SystemOptionData.DownloadType = EDownloadType.LoginPreDownload;
			break;
		default:
			SystemOptionData.DownloadType = EDownloadType.LoginFullDownload;
			break;
		}
		OpenSelectDownloadPopup(_currentSelectedSpec, SystemOptionData.Spec == ESpecType.Undefine);
	}

	public void OnSelect_FullDownload()
	{
		OnClickSelectDownloadType(EDownloadType.LoginFullDownload);
	}

	public void OnSelect_BackgroundDownload()
	{
		OnClickSelectDownloadType(EDownloadType.LoginPreDownload);
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
			return;
		}
		UIHelper.EnableGameObject(ProgressRoot, enable: true);
		UIHelper.EnableGameObject(SelectSpecRoot, enable: false);
		UIHelper.EnableGameObject(FirstDownloadRoot, enable: false);
		UIHelper.EnableGameObject(SelctDownloadRoot, enable: false);
		UIHelper.EnableGameObject(SelectDownloadTypeRoot, enable: false);
		if (SystemOptionData.DownloadType == EDownloadType.LoginPreDownload)
		{
			_ = (object)null;
			_ = 51;
			_ = 1;
			_ = 0;
			Array.Empty<object>();
		}
		else
		{
			_ = (object)null;
			_ = 52;
			_ = 1;
			_ = 0;
			Array.Empty<object>();
		}
		if (SystemOptionData.DownloadType == EDownloadType.LoginPreDownload)
		{
			_patchData.PatchInfo.FilterCategory(new EDownloadCategory[1]);
		}
		else
		{
			EDownloadCategory[] downloadCategories = GetDownloadCategories();
			_patchData.PatchInfo.FilterCategory(downloadCategories);
			EDownloadCategory[] array = downloadCategories;
			foreach (EDownloadCategory item in array)
			{
				PatchDownloader.DownloadedCategory.Add(item);
			}
			int length = Enum.GetValues(typeof(EDownloadCategory)).Length;
			if (PatchDownloader.DownloadedCategory.Count == length)
			{
				PatchDownloader.CurrentVersionPatchIsAllDownloaded = true;
			}
		}
		GlobalAutoBehaviour<PatchDownloader>.instance.StartDownload(_patchData, OnDownloaded, OnMessage, CheckAgreeDownload);
	}

	private void CheckAgreeDownload(long totalDownloadBytes, Action<bool> onAgree)
	{
		_ = LoginData.LastLoginMethod;
		_ = LoginData.LastServerName;
		if (totalDownloadBytes == 0L)
		{
			Vespa_System_Action_Extension_Method.fire(onAgree, i_argA: true);
			return;
		}
		if (_isOpenedFirstDownload)
		{
			Vespa_System_Action_Extension_Method.fire(onAgree, i_argA: true);
			return;
		}
		string text = LocalizationManager.GetString(Patch.DOWNLOAD_PATCH_CONFIRM_TITLE);
		string text2 = LocalizationManager.GetString(Patch.DOWNLOAD_EXTRA_RESOURCE_MESSAGE);
		text2 = text2 + "\n" + LocalizationManager.GetString(Patch.DOWNLOAD_FILE_SIZE, Vespa_Long_Extension_Method.LongToByteFormat(totalDownloadBytes));
		text2 += $"\n\n [sub][c][ff0000]{LocalizationManager.GetString(Patch.DOWNLOAD_RECOMMEND_USE_WIFI_MESSAGE)}[-][/c][/sub]";
		MessagePopup.OpenYesOnly(text, text2, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onAgree, i_argA: true);
		}, ignoreOnClickClose: true);
	}

	public override void Reset()
	{
	}

	private void TestAbortDonwnload()
	{
		if (Input.GetKeyDown(KeyCode.F12))
		{
			StopDownload();
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		TestAbortDonwnload();
		if (ProgressRoot == null || !ProgressRoot.gameObject.activeInHierarchy)
		{
			return;
		}
		if (DefinePlatform.IsEditor && Define.LocalAssetLoading.Value)
		{
			UIHelper.SetSpriteFillAmount(Sprite_LoadingBar, 1f);
			return;
		}
		switch (GlobalAutoBehaviour<PatchDownloader>.instance.State)
		{
		case PatchDownloader.DownloadState.Checking:
			UIHelper.SetSpriteFillAmount(Sprite_LoadingBar, 0f);
			UIHelper.SetLabelText(ProgressLabel, LocalizationManager.GetString(Patch.LOGIN_VERSION_CHECKING));
			break;
		case PatchDownloader.DownloadState.Started:
		{
			int totalArchiveCount = GlobalAutoBehaviour<PatchDownloader>.instance.TotalArchiveCount;
			double downloadedFraction = GlobalAutoBehaviour<PatchDownloader>.instance.GetDownloadedFraction();
			if (totalArchiveCount == 0)
			{
				UIHelper.SetSpriteFillAmount(Sprite_LoadingBar, 1f);
				break;
			}
			float num = (float)downloadedFraction / (float)totalArchiveCount;
			Vespa_System_Action_Extension_Method.fire(OnProgressChanged, num);
			UIHelper.SetSpriteFillAmount(Sprite_LoadingBar, num);
			string text = LocalizationManager.GetString(Patch.LOGIN_FILE_DOWNLOADING, downloadedFraction.ToString("F2"), totalArchiveCount);
			UIHelper.SetLabelText(ProgressLabel, text);
			break;
		}
		case PatchDownloader.DownloadState.Done:
			UIHelper.SetSpriteFillAmount(Sprite_LoadingBar, 1f);
			break;
		default:
			UIHelper.SetSpriteFillAmount(Sprite_LoadingBar, 0f);
			break;
		}
	}

	private void OnDownloaded(PatchDownloader.EDownloadResponse downloadErrorCase, string error)
	{
		if (downloadErrorCase == PatchDownloader.EDownloadResponse.None || downloadErrorCase == PatchDownloader.EDownloadResponse.OK)
		{
			Define.PatchDownloaded = true;
			Vespa_System_Action_Extension_Method.fire(_onDownloaded, DownloadResultType.Success);
			Close();
		}
		else
		{
			MessagePopup.OpenError(error, delegate
			{
				Vespa_System_Action_Extension_Method.fire(_onDownloaded, DownloadResultType.DownloadFail);
			});
		}
	}

	public static EDownloadCategory[] GetDownloadCategories()
	{
		HashSet<EDownloadCategory> hashSet = new HashSet<EDownloadCategory>();
		hashSet.Add(EDownloadCategory.PreDownload);
		if (SystemOptionData.DownloadType == EDownloadType.LoginPreDownload)
		{
			return hashSet.ToArray();
		}
		hashSet.Add(EDownloadCategory.EnterLobbyScene);
		if (!SingletonUpdater<LoginManager>.isValidInstance || SingletonUpdater<LoginManager>.instance == null)
		{
			return hashSet.ToArray();
		}
		bool isNewUser = SingletonUpdater<LoginManager>.instance.IsNewUser;
		DownloadControlDataContainer downloadControlDataContainer = GameRoot.Instance.DownloadControlDataContainer;
		if (!isNewUser && GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance != null && downloadControlDataContainer != null)
		{
			UserInfo userInfo = GlobalAutoBehaviour<AccountManager>.instance.GetUserInfo();
			if (userInfo != null)
			{
				int lastChapterIndex = userInfo.LastChapterIndex;
				for (int i = 9; i <= lastChapterIndex; i++)
				{
					EDownloadCategory[] downloadCategories = downloadControlDataContainer.GetDownloadCategories(i);
					if (downloadCategories != null && downloadCategories.Length != 0)
					{
						EDownloadCategory[] array = downloadCategories;
						foreach (EDownloadCategory item in array)
						{
							hashSet.Add(item);
						}
					}
				}
			}
		}
		return hashSet.ToArray();
	}

	public void OnConfirmAndStartDownload(ESpecType specType)
	{
		UIHelper.EnableGameObject(this, enable: true);
		StartDownload(specType);
	}

	public void OnShowDownloadSelectPopup()
	{
		UIHelper.EnableGameObject(this, enable: true);
		OpenSpecSelectPopup();
	}

	public void OnClickCloseSelectSpec()
	{
		UIHelper.EnableGameObject(SelectSpecRoot, enable: false);
	}

	public void OnClickCloseSelectDownload()
	{
		UIHelper.EnableGameObject(SelectDownloadTypeRoot, enable: false);
	}

	public void OnClickStartDownload()
	{
		switch (_currentSelectedSpec)
		{
		case ESpecType.Undefine:
			if (SystemOptionData.Spec != ESpecType.Undefine)
			{
				StartDownload(SystemOptionData.Spec);
			}
			break;
		case ESpecType.Low:
			StartDownload(ESpecType.Low);
			break;
		case ESpecType.Mid:
			StartDownload(ESpecType.Mid);
			break;
		case ESpecType.Original:
			StartDownload(ESpecType.Original);
			break;
		case ESpecType.High:
			break;
		}
	}

	public void OnClickSelectSpecConfirm()
	{
		UIHelper.EnableGameObject(SelectSpecRoot, enable: false);
		UIHelper.EnableGameObject(FirstDownloadRoot, enable: false);
		UIHelper.EnableGameObject(SelectDownloadTypeRoot, enable: false);
		if (IsNewUser())
		{
			OpenSelectDownloadPopup(_currentSelectedSpec);
			return;
		}
		SystemOptionData.DownloadType = EDownloadType.LoginFullDownload;
		OnClickStartDownload();
	}
}
