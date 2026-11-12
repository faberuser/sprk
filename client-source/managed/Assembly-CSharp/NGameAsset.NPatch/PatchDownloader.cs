using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniJSON;
using NGame2.NAccount;
using NGame2.NConfig;
using NGame2.NLogin;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NPatchGenerator;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;
using UnityEngine.Profiling;

namespace NGameAsset.NPatch;

public class PatchDownloader : GlobalAutoBehaviour<PatchDownloader>, DownloadHandlerBase.Listener, WebRequestFile.Listener
{
	public delegate void FuncStartPriorityDownload(string name, long length);

	public delegate void ProgressPriorityDownload(string name, float progress);

	public delegate void CompletePriorityDownload(string name);

	public enum EDownloadResponse
	{
		OK,
		None,
		PatchRootUrlIsNull,
		FullDisk,
		FailToDownloadUrl,
		CannotFindHost,
		CdnAddressEmpty,
		FailToGetRemoteVersionInfo,
		FailToGetRemotePatchInfo,
		FailToParseRemotePatchVersion,
		RemotePatchVersionInfoIsNull,
		RemotePatchVersionInfoIsEmpty,
		RemotePatchInfoIsEmpty,
		RemotePatchInfoIsNull,
		InvalidRemoteVersion,
		ErrorDuringExtract
	}

	public enum BackgroundDownloadLog
	{
		Begin,
		Resume,
		End
	}

	public enum DownloadState
	{
		Checking,
		Started,
		Pause,
		Done
	}

	public class PatchData
	{
		public string RootUrl { get; set; }

		public int Version { get; set; }

		public PatchInfoData PatchInfo { get; set; }

		public PatchInfoData LocalPatchInfo { get; set; }

		public void Clear()
		{
			if (PatchInfo != null && PatchInfo.Archives != null)
			{
				PatchInfo.Archives.Clear();
			}
			if (LocalPatchInfo != null && LocalPatchInfo.Archives != null)
			{
				LocalPatchInfo.Archives.Clear();
			}
			PatchInfo = null;
			LocalPatchInfo = null;
		}
	}

	public static List<string> DownloadedArchiveName = new List<string>();

	public static ArchiveInfo CurrentDownloadingArchive = null;

	public static bool TestBreakingPatch = false;

	public static string BreakingPatchArchiveName;

	public static long CurrentTotalAlloc = 0L;

	public static long BeginTotalAlloc = 0L;

	public static long EndTotalAlloc = 0L;

	public static long MaxTotalAlloc = 0L;

	public static long MaxGcTotal = 0L;

	public static long CurrentGcTotal = 0L;

	public static long CurrentUsedHeap = 0L;

	public static long MaxUsedHeap = 0L;

	public static long CurrentUsedMono = 0L;

	public static long MaxUsedMono = 0L;

	private static float BeginDownloadTime = 0f;

	public static float TotalElapsedTime = 0f;

	public int MaxParallelDownloadCount = 5;

	public int MaxRetryCount = 1;

	public int MaxDownloadTimeoutTime = 300;

	public DownloadState State;

	public static FuncStartPriorityDownload _startPriorityDownload;

	public static ProgressPriorityDownload _progressPriorityDownload;

	public static CompletePriorityDownload _completePriorityDownload;

	private static Queue<ArchiveInfo> _archiveQueue = new Queue<ArchiveInfo>();

	private static Queue<ArchiveInfo> _priorityArchiveQueue = new Queue<ArchiveInfo>();

	private static Dictionary<int, int> _coroutineIndexDownloadedCountPair = new Dictionary<int, int>();

	private static Dictionary<int, double> _coroutineIndexDownloadFractionPair = new Dictionary<int, double>();

	private static HashSet<EDownloadCategory> downloadedCategory = new HashSet<EDownloadCategory>();

	private static bool currentVersionPatchIsAllDownloaded = false;

	private HashSet<string> _createdDirectorySet = new HashSet<string>();

	private object _ioLocker = new object();

	private Action<EDownloadResponse, string> _onDownloaded;

	private Action<string> _onMessage;

	private Action<long, Action<bool>> _onCheckAgreeDownload;

	private int remotePatchVersion;

	public static HashSet<EDownloadCategory> DownloadedCategory => downloadedCategory;

	public static bool CurrentVersionPatchIsAllDownloaded
	{
		get
		{
			return currentVersionPatchIsAllDownloaded;
		}
		set
		{
			currentVersionPatchIsAllDownloaded = value;
		}
	}

	public int TotalArchiveCount { get; set; }

	protected override void Awake()
	{
		base.Awake();
		MaxParallelDownloadCount = GetMaxParallelDownloadCount();
		GlobalAutoBehaviour<GuiManager>.instance.Add("PatchDownloader", this, delegate
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
			TestBreakingPatch = GUILayout.Toggle(TestBreakingPatch, "Test DownloadingBroken");
			LoginPatchDownloader.IsOverwriteRemotePatchVersion.Value = GUILayout.Toggle(LoginPatchDownloader.IsOverwriteRemotePatchVersion.Value, "Is Overwrite Patch Version");
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("   Version Number");
				LoginPatchDownloader.OverwriteRemotePatchVersionNumber.Value = GUILayout.TextField(LoginPatchDownloader.OverwriteRemotePatchVersionNumber.Value);
			}
			LoginPatchDownloader.IsStopDownload.Value = GUILayout.Toggle(LoginPatchDownloader.IsStopDownload.Value, "Stop Download");
			if (GUILayout.Button("Hero Introduce Popup", GUILayout.Width(150f)))
			{
				LoginIntroduceHero loginIntroduceHero = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginIntroduceHero>();
				if (loginIntroduceHero != null)
				{
					loginIntroduceHero.Open(null);
				}
			}
		});
	}

	public static bool IsBackgorundDownloading()
	{
		if (GlobalAutoBehaviour<PatchDownloader>.isValidInstance)
		{
			return GlobalAutoBehaviour<PatchDownloader>.instance.State != DownloadState.Done;
		}
		return false;
	}

	private int GetMaxParallelDownloadCount()
	{
		return 30;
	}

	private void Clear()
	{
		_archiveQueue.Clear();
		_priorityArchiveQueue.Clear();
		_coroutineIndexDownloadedCountPair.Clear();
		_coroutineIndexDownloadFractionPair.Clear();
	}

	public void CreateAssetDirectory(string dir)
	{
		lock (_ioLocker)
		{
			if (!_createdDirectorySet.Contains(dir))
			{
				_createdDirectorySet.Add(dir);
				Directory.CreateDirectory(dir);
			}
		}
	}

	public bool IsDownloadClear()
	{
		return __Extension_Method_ICollection__.COUNT(_coroutineIndexDownloadFractionPair) == 0;
	}

	public double GetDownloadedFraction()
	{
		double num = 0.0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_coroutineIndexDownloadFractionPair); i++)
		{
			num += _coroutineIndexDownloadFractionPair[i];
		}
		return num;
	}

	public void StopDownload()
	{
		_archiveQueue.Clear();
		_priorityArchiveQueue.Clear();
	}

	public void SetMaxParallelDownloadCount(int count)
	{
		MaxParallelDownloadCount = count;
		if (MaxParallelDownloadCount <= 1)
		{
			MaxParallelDownloadCount = 2;
		}
	}

	public int GetPatchVersion()
	{
		return remotePatchVersion;
	}

	public void DetermineIsFullDownLoadUser(PatchData patchData)
	{
		PatchInfoData localPatchInfo = patchData.LocalPatchInfo;
		if (!SingletonUpdater<LoginManager>.isValidInstance || !SingletonUpdater<LoginManager>.instance.IsNewUser || localPatchInfo == null || patchData == null || localPatchInfo.Archives == null)
		{
			return;
		}
		int num = 0;
		foreach (ArchiveInfo archive in localPatchInfo.Archives)
		{
			if (num < archive.SrcVersion)
			{
				num = archive.SrcVersion;
			}
		}
		int version = patchData.Version;
		if (num > 0 && num != version)
		{
			SingletonUpdater<LoginManager>.instance.IsNewUser = false;
			Define.FullDownloadUser.Value = true;
		}
	}

	public void StartDownload(PatchData patchData, Action<EDownloadResponse, string> onDownloaded, Action<string> onMessage, Action<long, Action<bool>> checkAgreeDownload, bool abandonLoad = true)
	{
		_onDownloaded = onDownloaded;
		_onMessage = onMessage;
		_onCheckAgreeDownload = checkAgreeDownload;
		patchData.PatchInfo.FilterBySpec(SystemOptionData.Spec);
		patchData.PatchInfo.FilterByLanguage(SingletonAuto<LocalizationManager>.instance.Language);
		patchData.PatchInfo.FilterByVideoSpec();
		State = DownloadState.Checking;
		if (!SystemOptionData.TryBackgroundDownload)
		{
			RequestBackgroundLog(BackgroundDownloadLog.Begin);
			SystemOptionData.TryBackgroundDownload = true;
		}
		else
		{
			RequestBackgroundLog(BackgroundDownloadLog.Resume);
		}
		VespaCoroutine.Start(this, coStartDownload(patchData, onDownloaded, onMessage, checkAgreeDownload, abandonLoad));
	}

	private IEnumerator coStartDownload(PatchData patchData, Action<EDownloadResponse, string> onDownloaded, Action<string> onMessage, Action<long, Action<bool>> checkAgreeDownload, bool abandonLoad = true)
	{
		if (patchData == null)
		{
			State = DownloadState.Done;
			Vespa_System_Action_Extension_Method.fire(_onDownloaded, EDownloadResponse.RemotePatchVersionInfoIsNull, "patchData is null");
			yield break;
		}
		if (patchData.PatchInfo == null)
		{
			State = DownloadState.Done;
			Vespa_System_Action_Extension_Method.fire(_onDownloaded, EDownloadResponse.RemotePatchInfoIsNull, "patchData.PatchInfo is null");
			yield break;
		}
		List<ArchiveInfo> downloadList = patchData.PatchInfo.GetDownloadArchiveList(patchData.LocalPatchInfo);
		patchData.PatchInfo.GetDownloadSize(patchData.LocalPatchInfo);
		if (abandonLoad && downloadList.Count > 0)
		{
			yield return SingletonAuto<TableDataManager>.instance.coAbandonAllLoad();
		}
		CreateArchiveQueue(downloadList);
		if (TestBreakingPatch && downloadList.Count > 0)
		{
			for (int i = 0; i < 10; i++)
			{
				ArchiveInfo archiveInfo = Vespa_Generic_IList_Extension_Method.RandomGet(downloadList);
				if (archiveInfo.Name.EndsWith(".cb"))
				{
					BreakingPatchArchiveName = archiveInfo.Name;
					break;
				}
			}
		}
		TotalArchiveCount = GetTotalArchiveQueue();
		State = DownloadState.Started;
		if (String_Extension_Method.IsNullOrEmpty(patchData.RootUrl))
		{
			State = DownloadState.Done;
			Vespa_System_Action_Extension_Method.fire(_onDownloaded, EDownloadResponse.PatchRootUrlIsNull, "Patch root url is null");
			yield break;
		}
		Vespa_System_Action_Extension_Method.fire(_onMessage, "Start Parallel Download");
		CaptureBegin();
		ThreadSafeDic<string, AssetInfo> assetInfoDic = new ThreadSafeDic<string, AssetInfo>((patchData.LocalPatchInfo == null) ? new Dictionary<string, AssetInfo>() : patchData.LocalPatchInfo.GetAllAssetInfoDictionary());
		Dictionary<string, AssetInfo> allAssetInfoDictionary = patchData.PatchInfo.GetAllAssetInfoDictionary();
		SingletonAuto<AssetFileManager>.instance.Setup(allAssetInfoDictionary);
		EDownloadResponse downloadError = EDownloadResponse.None;
		string errorMsg = null;
		List<Coroutine> coroutineList = new List<Coroutine>();
		int endedCoroutineCount = 0;
		int maxParallelDownloadCount = ((MaxParallelDownloadCount <= 0) ? 1 : MaxParallelDownloadCount);
		for (int j = 0; j < maxParallelDownloadCount; j++)
		{
			endedCoroutineCount++;
		}
		SceneAutoBehaviour<LocalPatchInfoWriter>.instance.BeginWrite(assetInfoDic);
		while (endedCoroutineCount < maxParallelDownloadCount)
		{
			yield return null;
		}
		SceneAutoBehaviour<LocalPatchInfoWriter>.instance.RequestEnd();
		while (SceneAutoBehaviour<LocalPatchInfoWriter>.instance.IsProcessing)
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(_onMessage, "Downloading Asset Completed");
		CaptureEnd();
		if (downloadError != EDownloadResponse.None)
		{
			for (int k = 0; k < coroutineList.Count; k++)
			{
				VespaCoroutine.Stop(coroutineList[k]);
			}
			coroutineList.Clear();
		}
		Clear();
		CurrentDownloadingArchive = null;
		State = DownloadState.Done;
		RequestBackgroundLog(BackgroundDownloadLog.End);
		Vespa_System_Action_Extension_Method.fire(_onDownloaded, downloadError, errorMsg);
	}

	public void StartBackgroundDownload(EDownloadCategory[] downloadCategories)
	{
		VespaCoroutine.Start(coStartBackgroundDownload(downloadCategories));
	}

	private IEnumerator coStartBackgroundDownload(EDownloadCategory[] downloadCategories)
	{
		State = DownloadState.Checking;
		BackgroundDownloader.Open();
		while (!BackgroundDownloader.GetPrefab().IsOpen())
		{
			yield return null;
		}
		AppVersionInfo versionInfo = SingletonUpdater<LoginManager>.instance.VersionInfo;
		SetAppVersionInfo(versionInfo);
		bool isAgreement = false;
		BackgroundDownloader.GetInstance().OnAgreement = delegate
		{
			isAgreement = true;
		};
		BackgroundDownloader.GetInstance().Init(versionInfo.CdnProxyUrl, downloadCategories);
		BackgroundDownloader.GetInstance().SetAlphaProgress(0f);
		while (!isAgreement && State != DownloadState.Done)
		{
			yield return null;
		}
		BackgroundDownloader.GetInstance().SetAlphaProgress(1f);
		while (GlobalMessagePopup.IsOpenGlobal())
		{
			GlobalMessagePopup.CloseGlobal();
			yield return null;
		}
	}

	private void CreateArchiveQueue(List<ArchiveInfo> necessaryArchiveList)
	{
		_archiveQueue.Clear();
		_priorityArchiveQueue.Clear();
		necessaryArchiveList.Sort(ArchiveInfo.PriorityAscendingSort);
		lock (_archiveQueue)
		{
			for (int i = 0; i < necessaryArchiveList.Count; i++)
			{
				_archiveQueue.Enqueue(necessaryArchiveList[i]);
			}
		}
	}

	private IEnumerator CheckAgreementDownloadFiles(long totalDonwloadBytes)
	{
		bool isAgree = false;
		Vespa_System_Action_Extension_Method.fire(_onCheckAgreeDownload, totalDonwloadBytes, delegate(bool agreeResult)
		{
			isAgree = agreeResult;
		});
		while (!isAgree)
		{
			yield return null;
		}
	}

	public void ReturnToLogin()
	{
		VespaCoroutine.StopAll(this);
		Clear();
	}

	private ArchiveInfo DequeueArchive()
	{
		ArchiveInfo archiveInfo = null;
		lock (_priorityArchiveQueue)
		{
			if (_priorityArchiveQueue.Count > 0)
			{
				archiveInfo = _priorityArchiveQueue.Dequeue();
				if (_startPriorityDownload != null)
				{
					_startPriorityDownload(archiveInfo.Name, archiveInfo.Length);
				}
			}
			else
			{
				lock (_archiveQueue)
				{
					if (_archiveQueue.Count > 0)
					{
						archiveInfo = _archiveQueue.Dequeue();
					}
				}
			}
		}
		if (archiveInfo != null)
		{
			CurrentDownloadingArchive = archiveInfo;
			return archiveInfo;
		}
		CurrentDownloadingArchive = null;
		return null;
	}

	public bool IsPatchDownloading()
	{
		if (IsEmptyArchiveQueue())
		{
			return CurrentDownloadingArchive != null;
		}
		return true;
	}

	public bool IsEmptyArchiveQueue()
	{
		if (_archiveQueue.Count == 0)
		{
			return _priorityArchiveQueue.Count == 0;
		}
		return false;
	}

	public int GetTotalArchiveQueue()
	{
		return _archiveQueue.Count + _priorityArchiveQueue.Count;
	}

	private ArchiveInfo GetArchiveInfoInQueue(string assetName)
	{
		lock (_archiveQueue)
		{
			Queue<ArchiveInfo>.Enumerator enumerator = _archiveQueue.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.AssetInfos.Find((AssetInfo x) => x.EntryName.Contains(assetName)) != null)
				{
					return enumerator.Current;
				}
			}
			return null;
		}
	}

	private bool IsBundleInArchive(ArchiveInfo archive, string assetName)
	{
		if (archive == null || String_Extension_Method.IsNullOrEmpty(assetName))
		{
			return false;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(archive.AssetInfos); i++)
		{
			if (archive.AssetInfos[i].EntryName.Contains(assetName))
			{
				return true;
			}
		}
		return false;
	}

	public void PriorityBundle(string assetName, Action complete)
	{
		if (IsBundleInArchive(CurrentDownloadingArchive, assetName))
		{
			VespaCoroutine.Start(this, coWaitPriorityDownloadBundle(CurrentDownloadingArchive.Name, complete));
			return;
		}
		if (IsEmptyArchiveQueue())
		{
			Vespa_System_Action_Extension_Method.fire(complete);
			return;
		}
		ArchiveInfo archive = GetArchiveInfoInQueue(assetName);
		if (archive == null)
		{
			Vespa_System_Action_Extension_Method.fire(complete);
			return;
		}
		lock (_archiveQueue)
		{
			_archiveQueue = new Queue<ArchiveInfo>(_archiveQueue.Where((ArchiveInfo a) => a.Md5 != archive.Md5));
			lock (_priorityArchiveQueue)
			{
				_priorityArchiveQueue.Enqueue(archive);
			}
		}
		VespaCoroutine.Start(this, coWaitPriorityDownloadBundle(archive.Name, complete));
	}

	private IEnumerator coWaitPriorityDownloadBundle(string archiveName, Action complete)
	{
		while (IsDownloadingArchive(archiveName))
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(complete);
	}

	private bool IsDownloadingArchive(string archiveName)
	{
		if (CurrentDownloadingArchive != null && CurrentDownloadingArchive.Name == archiveName)
		{
			return true;
		}
		if (DownloadedArchiveName.Contains(archiveName))
		{
			return false;
		}
		if (IsEmptyArchiveQueue())
		{
			return false;
		}
		if (_priorityArchiveQueue.Count() > 0)
		{
			return true;
		}
		return false;
	}

	public bool IsPriorityQueue()
	{
		return _priorityArchiveQueue.Count() > 0;
	}

	private IEnumerator coDownloadArchive(int coroutineIndex, int remotePatchVersion, string remotePatchRootUrl, ThreadSafeDic<string, AssetInfo> localAssetInfoDictionary, Action<EDownloadResponse, string> onDownloaded)
	{
		_coroutineIndexDownloadedCountPair[coroutineIndex] = 0;
		_coroutineIndexDownloadFractionPair[coroutineIndex] = 0.0;
		bool isProcessing;
		bool onSuccess;
		while (true)
		{
			yield return null;
			if (IsEmptyArchiveQueue())
			{
				break;
			}
			yield return new WaitPatchDownloadPause();
			ArchiveInfo archiveInfo = DequeueArchive();
			if (archiveInfo == null)
			{
				continue;
			}
			string archiveName = archiveInfo.Name;
			int num = remotePatchVersion;
			string text = num + "/" + archiveName;
			string remoteArchiveUrl = remotePatchRootUrl + text;
			int retryCount = 0;
			while (true)
			{
				int curDownloadCount = _coroutineIndexDownloadedCountPair[coroutineIndex];
				float downloadStartTime = Time.time;
				string timeOutLog = null;
				byte[] buffer = new byte[102400];
				isProcessing = true;
				onSuccess = false;
				DownloadContextData downloadContextData = new DownloadContextData
				{
					CoroutineIndex = coroutineIndex,
					RemotePatchVersion = remotePatchVersion,
					ArchiveInfo = archiveInfo,
					AssetInfoDictionary = localAssetInfoDictionary
				};
				bool flag = archiveInfo.Name.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase) || archiveInfo.Name.EndsWith(".webm", StringComparison.InvariantCultureIgnoreCase);
				WebRequest req;
				try
				{
					req = (flag ? ((WebRequest)new WebRequestFile(this, remoteArchiveUrl, downloadContextData, delegate(bool success)
					{
						onSuccess = success;
						isProcessing = false;
					})) : ((WebRequest)new WebRequestScript(this, remoteArchiveUrl, buffer, downloadContextData, delegate(bool success)
					{
						onSuccess = success;
						isProcessing = false;
					})));
				}
				catch (IOException)
				{
					Vespa_System_Action_Extension_Method.fire(onDownloaded, EDownloadResponse.FullDisk, EDownloadResponse.FullDisk.ToString());
					yield break;
				}
				while (!req.isDone && string.IsNullOrEmpty(req.error))
				{
					if (Time.time - downloadStartTime > (float)MaxDownloadTimeoutTime)
					{
						timeOutLog = "Downloading timeout. url=" + remoteArchiveUrl;
						break;
					}
					yield return null;
					double val = _coroutineIndexDownloadFractionPair[coroutineIndex];
					_coroutineIndexDownloadFractionPair[coroutineIndex] = System.Math.Max(val, (double)curDownloadCount + (double)(req.progress * 0.5f));
					if (_progressPriorityDownload != null)
					{
						_progressPriorityDownload(archiveName, req.progress);
					}
				}
				string text2 = timeOutLog ?? req.error;
				if (string.IsNullOrEmpty(text2))
				{
					break;
				}
				EDownloadResponse i_argA = EDownloadResponse.FailToDownloadUrl;
				string text3 = "Failed to download. url=" + remoteArchiveUrl + " error=" + text2;
				if (text2.IndexOf("UnknownHostException", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					i_argA = EDownloadResponse.CannotFindHost;
					text3 = "Cannot find Host due to bad internet connection.";
				}
				VespaDebug.LogError(text3);
				if (retryCount++ > MaxRetryCount)
				{
					VespaDebug.LogError("Stop downloading patch!");
					Vespa_System_Action_Extension_Method.fire(onDownloaded, i_argA, text3);
					yield break;
				}
				yield return new WaitForSeconds(1f);
			}
			while (isProcessing)
			{
				yield return null;
			}
			if (!onSuccess)
			{
				string text4 = "Error On Extracting";
				VespaDebug.LogError(text4);
				Vespa_System_Action_Extension_Method.fire(onDownloaded, EDownloadResponse.ErrorDuringExtract, text4);
				yield break;
			}
			DownloadedArchiveName.Add(archiveName);
			if (_completePriorityDownload != null)
			{
				_completePriorityDownload(archiveName);
			}
			SingletonUpdater<GCManager>.instance.PendingCollect();
		}
		Vespa_System_Action_Extension_Method.fire(onDownloaded, EDownloadResponse.None, null);
	}

	private void RegisterAssetInfoDictionary(ArchiveInfo archiveInfo, string assetFilePath, string entryName, ThreadSafeDic<string, AssetInfo> assetInfoDictionary)
	{
		AssetInfo assetInfo = archiveInfo.AssetInfos.FirstOrDefault((AssetInfo e) => e.EntryName == entryName);
		_ = SingletonUpdater<Executer>.instance;
		AssetInfo assetInfo2 = new AssetInfo();
		assetInfo2.FullPath = assetFilePath;
		assetInfo2.EntryName = entryName;
		FileUtility.TryGetFileInfos(assetFilePath, out var Md, out var WriteTime, out var Length);
		assetInfo2.Md5 = Md;
		assetInfo2.Length = Length;
		if (assetInfo != null)
		{
			assetInfo2.Time = assetInfo.Time;
			assetInfo2.Cat = assetInfo.Cat;
			assetInfo2.AssetName = assetInfo.AssetName;
		}
		else
		{
			assetInfo2.Cat = archiveInfo.Cat;
			assetInfo2.AssetName = string.Empty;
		}
		FileUtility.SetFileWriteTime(assetFilePath, WriteTime);
		assetInfoDictionary.ADD(entryName, assetInfo2);
		if (assetInfo != null)
		{
			if (!string.Equals(assetInfo2.Md5, assetInfo.Md5, StringComparison.InvariantCultureIgnoreCase))
			{
				VespaDebug.LogError("Asset MD5 mismatch. archiveInfo=" + archiveInfo.Name + "/ assetName=" + entryName + "/ local=" + assetInfo2.Md5 + "/ remote=" + assetInfo.Md5);
			}
			if (assetInfo2.Length != assetInfo.Length)
			{
				VespaDebug.LogError("Asset Length mismatch. local=" + assetInfo2.Length + " remote=" + assetInfo.Length);
			}
		}
	}

	private void SaveLocalVersionInfo(int patchVersion, ThreadSafeDic<string, AssetInfo> assetInfoDictionary)
	{
		SingletonUpdater<Executer>.instance.NextRun(delegate
		{
			SceneAutoBehaviour<LocalPatchInfoWriter>.instance.SetDirty();
		});
	}

	public void Pause()
	{
		if (State == DownloadState.Started)
		{
			State = DownloadState.Pause;
			Debug.Log("#### PatchDownload Pause");
		}
	}

	public void Resume()
	{
		if (State == DownloadState.Pause)
		{
			State = DownloadState.Started;
			Debug.Log("#### PatchDownload Resume");
		}
	}

	public static string GetDownloadedAssetPath(string persistentDataPath, string subPath)
	{
		string text = String_Extension_Method.CombinePath(persistentDataPath, "Documents");
		return String_Extension_Method.CombinePath(String_Extension_Method.CombinePath(rhs: DefinePlatform.PlatformName, This: String_Extension_Method.CombinePath(text, "Patch")), subPath).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
	}

	public static string GetAssetEntryName(string downloadAssetPath)
	{
		int startIndex = downloadAssetPath.IndexOf("Assetbundle");
		return downloadAssetPath.Substring(startIndex).Replace('\\', '/');
	}

	public static void CaptureBegin()
	{
		BeginTotalAlloc = Profiler.GetTotalAllocatedMemoryLong();
		MaxTotalAlloc = BeginTotalAlloc;
		BeginDownloadTime = Time.realtimeSinceStartup;
	}

	public static void CaptureEnd()
	{
		EndTotalAlloc = Profiler.GetTotalAllocatedMemoryLong();
		TotalElapsedTime = Time.realtimeSinceStartup - BeginDownloadTime;
	}

	public static void CaptureMemory()
	{
		if (BeginTotalAlloc != 0L && EndTotalAlloc == 0L)
		{
			TotalElapsedTime = Time.realtimeSinceStartup - BeginDownloadTime;
			CurrentTotalAlloc = Profiler.GetTotalAllocatedMemoryLong();
			CurrentGcTotal = GC.GetTotalMemory(forceFullCollection: false);
			CurrentUsedHeap = Profiler.usedHeapSizeLong;
			CurrentUsedMono = Profiler.GetMonoUsedSizeLong();
			MaxTotalAlloc = System.Math.Max(MaxTotalAlloc, CurrentTotalAlloc);
			MaxGcTotal = System.Math.Max(MaxGcTotal, CurrentGcTotal);
			MaxUsedHeap = System.Math.Max(MaxUsedHeap, CurrentUsedHeap);
			MaxUsedMono = System.Math.Max(MaxUsedMono, CurrentUsedMono);
		}
	}

	private void UpdateDownloadedFraction(int coroutineIndex, double ratio)
	{
		if (_coroutineIndexDownloadFractionPair.ContainsKey(coroutineIndex))
		{
			double num = System.Math.Min(1.0, 0.5 + ratio / 2.0);
			_coroutineIndexDownloadFractionPair[coroutineIndex] = (double)_coroutineIndexDownloadedCountPair[coroutineIndex] + num;
		}
	}

	private void CompleteDownloadedFraction(int coroutineIndex)
	{
		if (_coroutineIndexDownloadedCountPair.ContainsKey(coroutineIndex))
		{
			_coroutineIndexDownloadedCountPair[coroutineIndex]++;
		}
		else
		{
			_coroutineIndexDownloadedCountPair[coroutineIndex] = 1;
		}
		_coroutineIndexDownloadFractionPair[coroutineIndex] = _coroutineIndexDownloadedCountPair[coroutineIndex];
	}

	void DownloadHandlerBase.Listener.OnCreateDicrectory(string assetFilePath)
	{
		string directoryName = Path.GetDirectoryName(assetFilePath);
		CreateAssetDirectory(directoryName);
	}

	void DownloadHandlerBase.Listener.OnUpateExtractAsset(DownloadContextData data, double ratio)
	{
		UpdateDownloadedFraction(data.CoroutineIndex, ratio);
	}

	void DownloadHandlerBase.Listener.OnCompleteExtractAsset(DownloadContextData data, string entryName)
	{
		string downloadedAssetPath = GetDownloadedAssetPath(UnityCache.persistentDataPath, entryName);
		RegisterAssetInfoDictionary(data.ArchiveInfo, downloadedAssetPath, entryName, data.AssetInfoDictionary);
	}

	void DownloadHandlerBase.Listener.OnCompleteExtractArchive(DownloadContextData data)
	{
		CompleteDownloadedFraction(data.CoroutineIndex);
		SaveLocalVersionInfo(data.RemotePatchVersion, data.AssetInfoDictionary);
	}

	void WebRequestFile.Listener.OnUpateExtractAsset(DownloadContextData data, double ratio)
	{
		UpdateDownloadedFraction(data.CoroutineIndex, ratio);
	}

	void WebRequestFile.Listener.OnCompleteExtractArchive(DownloadContextData data)
	{
		if (data != null)
		{
			ArchiveInfo archiveInfo = data.ArchiveInfo;
			if (archiveInfo != null && !__Extension_Method_ICollection__.IsNullOrEmpty(archiveInfo.AssetInfos))
			{
				string entryName = archiveInfo.AssetInfos[0].EntryName;
				string downloadedAssetPath = GetDownloadedAssetPath(UnityCache.persistentDataPath, entryName);
				RegisterAssetInfoDictionary(data.ArchiveInfo, downloadedAssetPath, entryName, data.AssetInfoDictionary);
				CompleteDownloadedFraction(data.CoroutineIndex);
				SaveLocalVersionInfo(data.RemotePatchVersion, data.AssetInfoDictionary);
			}
		}
	}

	public void RequestBackgroundLog(BackgroundDownloadLog type)
	{
	}

	public void GetPatchData(string cdnUrl, Action<PatchData, EDownloadResponse> onComplete)
	{
		if (String_Extension_Method.IsNullOrEmpty(cdnUrl))
		{
			VespaDebug.LogError("cdnUrl is Null Or Empty");
		}
		else
		{
			VespaCoroutine.Start(GlobalAutoBehaviour<GlobalManagement>.instance, coGetPatchData(cdnUrl, onComplete));
		}
	}

	private IEnumerator coGetPatchData(string cdnUrl, Action<PatchData, EDownloadResponse> onComplete)
	{
		string localPatchJsonPath = GetDownloadedAssetPath(UnityCache.persistentDataPath, "/PatchInfo/patch.json");
		string localLastBuildVersionPath = GetDownloadedAssetPath(UnityCache.persistentDataPath, "/PatchInfo/LastBuildVersion.txt");
		int localLastBuildVersion = -1;
		if (File.Exists(localPatchJsonPath) || File.Exists(localLastBuildVersionPath))
		{
			localLastBuildVersion = Vespa_String_Extension_Method.TOINT(File.ReadAllText(localLastBuildVersionPath), 0);
		}
		string lastBuildVersionUrl = Vespa_String_Extension_Method.CombineUrl(cdnUrl, "LastBuildVersion.txt");
		WwwText lastBuildVersionWWW = new WwwText(Vespa_String_Extension_Method.AppendRandomNumber(lastBuildVersionUrl, "?"));
		while (!lastBuildVersionWWW.isDone)
		{
			yield return null;
		}
		if (!string.IsNullOrEmpty(lastBuildVersionWWW.error))
		{
			Vespa_System_Action_Extension_Method.fire(onComplete, null, EDownloadResponse.FailToGetRemoteVersionInfo);
			yield break;
		}
		if (string.IsNullOrEmpty(lastBuildVersionWWW.text))
		{
			Vespa_System_Action_Extension_Method.fire(onComplete, null, EDownloadResponse.RemotePatchVersionInfoIsEmpty);
			yield break;
		}
		long normalizedVersion = SingletonAuto<Config>.instance.ClientConfig.GetNormalizedVersion();
		int remoteLastBuildVersion = 0;
		string[] array = lastBuildVersionWWW.text.Split('.');
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(array))
		{
			remoteLastBuildVersion = Vespa_String_Extension_Method.TOINT(array[0], 0);
		}
		if (LoginPatchDownloader.IsOverwriteRemotePatchVersion.Value)
		{
			int output = 0;
			if (String_Extension_Method.TryToInt(LoginPatchDownloader.OverwriteRemotePatchVersionNumber.Value, out output))
			{
				Debug.LogWarning("Overwrite RemotePatchVersion : " + remotePatchVersion);
				remoteLastBuildVersion = output;
			}
		}
		if (remoteLastBuildVersion == 0)
		{
			try
			{
				Dictionary<string, object> obj = (Json.Deserialize(lastBuildVersionWWW.text) as Dictionary<string, object>) ?? throw new Exception("Failed to parse remote patch version.");
				long num = 0L;
				object obj2 = null;
				foreach (KeyValuePair<string, object> item in obj)
				{
					string key = item.Key;
					object value = item.Value;
					if (value == null)
					{
						continue;
					}
					long normalizedVersion2 = ClientConfig.GetNormalizedVersion(key);
					if (normalizedVersion2 <= normalizedVersion)
					{
						if (normalizedVersion2 == normalizedVersion)
						{
							remoteLastBuildVersion = Vespa_String_Extension_Method.TOINT(value.ToString(), 0);
							break;
						}
						if (normalizedVersion2 < normalizedVersion && normalizedVersion2 > num && value != null)
						{
							num = normalizedVersion2;
							obj2 = value;
						}
					}
				}
				if (remoteLastBuildVersion == 0 && num != 0L)
				{
					remoteLastBuildVersion = Vespa_String_Extension_Method.TOINT(obj2.ToString(), 0);
				}
			}
			catch (Exception)
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, null, EDownloadResponse.FailToParseRemotePatchVersion);
				yield break;
			}
			if (remoteLastBuildVersion <= 0)
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, null, EDownloadResponse.InvalidRemoteVersion);
				yield break;
			}
		}
		if (localLastBuildVersion != remoteLastBuildVersion)
		{
			string url = Vespa_String_Extension_Method.AppendRandomNumber(cdnUrl + remoteLastBuildVersion + "/" + PatchGenerator.GetPatchInfoFileName(), "?");
			WwwText patchJsonWww = new WwwText(url);
			while (!patchJsonWww.isDone)
			{
				yield return null;
			}
			if (!string.IsNullOrEmpty(patchJsonWww.error))
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, null, EDownloadResponse.FailToGetRemotePatchInfo);
				yield break;
			}
			if (string.IsNullOrEmpty(patchJsonWww.text))
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, null, EDownloadResponse.RemotePatchInfoIsEmpty);
				yield break;
			}
			int num2 = remoteLastBuildVersion;
			string directoryName = Path.GetDirectoryName(localLastBuildVersionPath);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			File.WriteAllText(localLastBuildVersionPath, num2.ToString());
			File.WriteAllText(localPatchJsonPath, patchJsonWww.text);
		}
		PatchInfo parsedPatchInfo = null;
		bool parsed = false;
		SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
		{
			string s = File.ReadAllText(localPatchJsonPath);
			parsedPatchInfo = JsonMarshaler.Parse<PatchInfo>(s);
		}, (Action)delegate
		{
			parsed = true;
		});
		while (!parsed)
		{
			yield return null;
		}
		remotePatchVersion = remoteLastBuildVersion;
		PatchData patchData = new PatchData();
		patchData.Version = remoteLastBuildVersion;
		patchData.PatchInfo = new PatchInfoData(parsedPatchInfo);
		patchData.RootUrl = lastBuildVersionUrl.Remove(lastBuildVersionUrl.LastIndexOf('/') + 1);
		if (!patchData.RootUrl.EndsWith("/"))
		{
			patchData.RootUrl += "/";
		}
		PatchInfo localPatchInfo = SceneAutoBehaviour<LocalPatchInfoWriter>.instance.GetLocalPatchInfo();
		patchData.LocalPatchInfo = new PatchInfoData(localPatchInfo);
		Vespa_System_Action_Extension_Method.fire(onComplete, patchData, EDownloadResponse.OK);
	}

	public static void SetAppVersionInfo(AppVersionInfo versionInfo)
	{
	}
}
