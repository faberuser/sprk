using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NGame2.NLogin;
using NGame2.NScene;
using NGame2.NUI.NManager;
using NGameAsset;
using NGameAsset.NPatch;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PatchValidation : BaseWindow
{
	private enum PatchValidationResult
	{
		RemoteVersionDownloadFail,
		WindowClosed,
		LocalPatchFileNotExist,
		PatchDownloaderNotValid
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Login/PatchValidation";

	public UILabel LabelTitle;

	public UILabel LabelDesc;

	public UILabel LabelPercent;

	public Transform ButtonValidation;

	public Transform ButtonForPatchDownload;

	public Transform ButtonClose;

	public Transform ButtonConfirm;

	private PatchDownloader.PatchData _patchData;

	private PatchInfo _localPatchInfo;

	private Coroutine _processCoroutine;

	private LocalPatchInfoCollector _localPatchCollector;

	private const string LocaleReadyValidateDefault = "Do you want to start patch file validation?";

	private const string LocaleReadyValidate = "LOGIN_PATCH_CHECK_INFO_DESC";

	private const string LocaleValidatingDefalult = "Validating patch files...";

	private const string LocaleValidating = "LOGIN_PATCH_CHECKING_FILE_DESC";

	private const string LocaleValidateSuccessDefalult = "Valid patch file.";

	private const string LocaleValidateSuccess = "LOGIN_PATCH_PASS_CHECK_DESC";

	private const string LocalValidationFailDefault = "This is not a valid patch file.\nPress the OK button to enter the patch process.";

	private const string LocalValidationFail = "LOGIN_PATCH_FAIL_CHECK_DESC";

	public override void OnSystemBackButton()
	{
		if (!ButtonClose.gameObject.activeSelf)
		{
			if (!QuitGamePopup.IsOpenGlobal())
			{
				QuitGamePopup.OpenGlobal(delegate
				{
					GlobalAutoBehaviour<LoadingManager>.instance.Quit();
				});
			}
		}
		else
		{
			Close();
		}
	}

	public static void OpenPopup(bool openAtPatchDownload = true)
	{
		WindowPrefab<PatchValidation> windowPrefab = new WindowPrefab<PatchValidation>("Resources/Prefab2/Window/Login/PatchValidation", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.Open();
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private string GetStringByLocaleKey(string key, string defaultValue)
	{
		string value = string.Empty;
		if (!LocalizationManager.GetStringDirect(key, out value))
		{
			return defaultValue;
		}
		return value;
	}

	public override void Reset()
	{
		base.Reset();
		_localPatchInfo = null;
		_patchData = null;
		StopCoroutine();
		_processCoroutine = null;
		StopThreadJob();
		_localPatchCollector = null;
		UIHelper.EnableGameObject(ButtonForPatchDownload, enable: false);
		UIHelper.EnableGameObject(ButtonValidation, enable: true);
		UIHelper.EnableGameObject(ButtonClose, enable: true);
		UIHelper.EnableGameObject(ButtonConfirm, enable: false);
		UIHelper.SetLabelText(LabelPercent, string.Empty);
		UIHelper.SetLabelText(LabelDesc, GetStringByLocaleKey("LOGIN_PATCH_CHECK_INFO_DESC", "Do you want to start patch file validation?"));
	}

	public void OnClickPatchDownload()
	{
		UIHelper.EnableGameObject(ButtonClose, enable: false);
		UIHelper.EnableGameObject(ButtonForPatchDownload, enable: false);
		Close();
	}

	public void OnClickPatchValidation()
	{
		UIHelper.SetLabelText(LabelDesc, GetStringByLocaleKey("LOGIN_PATCH_CHECKING_FILE_DESC", "Validating patch files..."));
		UIHelper.EnableGameObject(ButtonClose, enable: false);
		UIHelper.EnableGameObject(ButtonValidation, enable: false);
		AppVersionInfo versionInfo = SingletonUpdater<LoginManager>.instance.VersionInfo;
		GlobalAutoBehaviour<PatchDownloader>.instance.GetPatchData(versionInfo.CdnProxyUrl, OnGetPatchData);
	}

	public void OnGetPatchData(PatchDownloader.PatchData patchData, PatchDownloader.EDownloadResponse response)
	{
		if (response == PatchDownloader.EDownloadResponse.OK)
		{
			_patchData = patchData;
			_processCoroutine = VespaCoroutine.Start(this, coProcessPatchValidation());
		}
		else
		{
			StopValidation(PatchValidationResult.RemoteVersionDownloadFail, "RemoteVersionDownloadFail");
		}
	}

	private IEnumerator coProcessPatchValidation()
	{
		if (_patchData == null)
		{
			StopValidation(PatchValidationResult.RemoteVersionDownloadFail, "PatchData is null.");
			yield break;
		}
		_localPatchCollector = new LocalPatchInfoCollector();
		_localPatchCollector.CollectLocalPatchInfoFromRealFile(OnEndCollectLocalPatchInfo, OnChangeProgress);
		while (_localPatchInfo == null)
		{
			yield return null;
		}
		UIHelper.EnableGameObject(ButtonClose, enable: true);
		UIHelper.SetLabelText(LabelPercent, string.Empty);
		ESpecType spec = SystemOptionData.Spec;
		PatchInfoData patchInfoData = (PatchInfoData)_patchData.PatchInfo.Clone();
		patchInfoData.FilterBySpec(spec);
		patchInfoData.FilterByLanguage(SingletonAuto<LocalizationManager>.instance.Language);
		patchInfoData.FilterByVideoSpec();
		patchInfoData.FilterCategory(LoginPatchDownloader.GetDownloadCategories());
		if (__Extension_Method_ICollection__.COUNT(patchInfoData.GetDownloadArchiveList(_localPatchInfo)) == 0)
		{
			UIHelper.EnableGameObject(ButtonConfirm, enable: true);
			UIHelper.SetLabelText(LabelDesc, GetStringByLocaleKey("LOGIN_PATCH_PASS_CHECK_DESC", "Valid patch file."));
			yield break;
		}
		Dictionary<string, AssetInfo> allAssetInfoDictionary = _localPatchInfo.GetAllAssetInfoDictionary();
		RemoveDuplicateNameFile(allAssetInfoDictionary, patchInfoData.GetAllAssetInfoDictionary());
		ThreadSafeDic<string, AssetInfo> assetInfoDic = new ThreadSafeDic<string, AssetInfo>(allAssetInfoDictionary);
		SceneAutoBehaviour<LocalPatchInfoWriter>.instance.BeginWrite(assetInfoDic);
		yield return null;
		SceneAutoBehaviour<LocalPatchInfoWriter>.instance.RequestEnd();
		while (SceneAutoBehaviour<LocalPatchInfoWriter>.instance.IsProcessing)
		{
			yield return null;
		}
		UIHelper.SetLabelText(LabelDesc, GetStringByLocaleKey("LOGIN_PATCH_FAIL_CHECK_DESC", "This is not a valid patch file.\nPress the OK button to enter the patch process."));
		UIHelper.EnableGameObject(ButtonForPatchDownload, enable: true);
	}

	private void RemoveDuplicateNameFile(Dictionary<string, AssetInfo> local, Dictionary<string, AssetInfo> remote)
	{
		Dictionary<string, string> upperStringFairDict = GetUpperStringFairDict(remote);
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, AssetInfo> item in local)
		{
			string key = item.Key.ToUpper();
			string key2 = item.Key;
			if (upperStringFairDict.ContainsKey(key) && key2 != upperStringFairDict[key])
			{
				DeleteFile(key2);
				list.Add(key2);
			}
		}
		foreach (string item2 in list)
		{
			local.Remove(item2);
		}
	}

	private void DeleteFile(string entryName)
	{
		string downloadedAssetPath = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, entryName);
		try
		{
			File.Delete(downloadedAssetPath);
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Remove Duplicate Entry [" + entryName + "]" + ex.StackTrace);
		}
	}

	private Dictionary<string, string> GetUpperStringFairDict(Dictionary<string, AssetInfo> assetInfoDict)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		Dictionary<string, AssetInfo>.Enumerator enumerator = assetInfoDict.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string key = enumerator.Current.Key.ToUpper();
			string key2 = enumerator.Current.Key;
			dictionary[key] = key2;
		}
		return dictionary;
	}

	private void StopValidation(PatchValidationResult result, string error)
	{
		VespaDebug.LogError(error);
		StopCoroutine();
		StopThreadJob();
		ShowMeesagePopup(result);
		Close();
	}

	private void StopCoroutine()
	{
		if (_processCoroutine != null)
		{
			VespaCoroutine.Stop(_processCoroutine);
		}
	}

	private void StopThreadJob()
	{
		if (_localPatchCollector != null)
		{
			_localPatchCollector.requestStop = true;
		}
	}

	private void ShowMeesagePopup(PatchValidationResult result)
	{
		if (result == PatchValidationResult.RemoteVersionDownloadFail)
		{
			string stringDirect = LocalizationManager.GetStringDirect("COMMON_ERROR_HTTP");
			MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), stringDirect);
		}
	}

	private void OnEndCollectLocalPatchInfo(PatchInfo localPatchInfo)
	{
		if (!IsOpen())
		{
			StopValidation(PatchValidationResult.WindowClosed, "Patch Validation Is Not Open");
		}
		else if (localPatchInfo == null)
		{
			StopValidation(PatchValidationResult.LocalPatchFileNotExist, "Local Patch Info Is Null");
		}
		else if (!GlobalAutoBehaviour<PatchDownloader>.isValidInstance)
		{
			StopValidation(PatchValidationResult.PatchDownloaderNotValid, "PatchDownloder Is Not Valid");
		}
		else
		{
			_localPatchInfo = localPatchInfo;
		}
	}

	private void OnChangeProgress(float progress)
	{
		string text = string.Format("{0} %", (progress * 100f).ToString("F1"));
		UIHelper.SetLabelText(LabelPercent, text);
	}
}
