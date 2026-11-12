using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using NGame2.NUI.NDescription;
using NGame2.NUtil;
using NShared;
using NVespa;
using UnityEngine;

namespace NGameAsset;

public class TableDataManager : SingletonAuto<TableDataManager>
{
	public enum ELoadStatus
	{
		None,
		Loading,
		FinalizeLoading,
		Finish
	}

	private DateTime _loadStartedTime;

	private List<IBaseDataContainer> _dataContainerList;

	private Action _onLoad;

	private JsonAssetAsyncLoader _jsonLoader;

	public ELoadStatus LoadStatus { get; private set; }

	public string LoadError { get; private set; }

	public TableDataManager()
	{
		LoadStatus = ELoadStatus.None;
	}

	public IEnumerator WaitForFinish()
	{
		if (LoadStatus != ELoadStatus.None)
		{
			yield return new WaitUntil(() => LoadStatus == ELoadStatus.Finish);
		}
	}

	public IEnumerator WaitAndReset()
	{
		if (LoadStatus != ELoadStatus.None)
		{
			yield return new WaitUntil(() => LoadStatus == ELoadStatus.Finish);
		}
		LoadStatus = ELoadStatus.None;
		JsonAssetAsyncLoader.AbandonAllLoad = false;
		LitJsonAssetAsyncLoader.AbandonAllLoad = false;
	}

	public void Reset()
	{
		if (LoadStatus != ELoadStatus.None)
		{
			LoadStatus = ELoadStatus.None;
		}
	}

	public void Load(bool autoRunPostProcess = true)
	{
		if (LoadStatus != ELoadStatus.None)
		{
			return;
		}
		if (SystemStatus.IsPlaying)
		{
			SingletonUpdater<AssetUpdateManager>.CreateInstance();
		}
		Action b = null;
		if (autoRunPostProcess)
		{
			b = delegate
			{
				GameRoot.Instance.OnDataContainerLoaded();
			};
		}
		_loadStartedTime = DateTime.Now;
		_onLoad = (Action)Delegate.Combine(_onLoad, b);
		RefreshDataContainer();
		_dataContainerList.AddRange(InitTableDataForClient());
		LoadStatus = ELoadStatus.Loading;
		LoadError = "";
	}

	private void RefreshDataContainer()
	{
		if (_dataContainerList != null)
		{
			foreach (IBaseDataContainer dataContainer in _dataContainerList)
			{
				_ = dataContainer;
			}
			_dataContainerList.Clear();
			_dataContainerList = null;
		}
		_dataContainerList = new List<IBaseDataContainer>();
	}

	public IEnumerator coAbandonAllLoad()
	{
		if (_jsonLoader != null)
		{
			JsonAssetAsyncLoader.AbandonAllLoad = true;
			LitJsonAssetAsyncLoader.AbandonAllLoad = true;
			yield return new WaitUntil(() => _jsonLoader.IsLoadQueueEmpty && SingletonAuto<LitJsonAssetAsyncLoader>.instance.IsLoadQueueEmpty);
		}
	}

	public bool IsLoadAbandoned()
	{
		return JsonAssetAsyncLoader.AbandonAllLoad;
	}

	private List<IBaseDataContainer> InitTableDataForClient()
	{
		if (_jsonLoader == null)
		{
			_jsonLoader = new JsonAssetAsyncLoader();
			GameRoot.Instance.SetJsonLoader(_jsonLoader);
		}
		GameRoot.Instance.SetAiScriptLoader(new TextAssetSyncLoader());
		return new List<IBaseDataContainer>
		{
			SingletonAuto<SkillDescContainer>.NewInstance.Load(SingletonAuto<LitJsonAssetAsyncLoader>.instance),
			SingletonAuto<SoundFxManager>.NewInstance.Load(SingletonAuto<LitJsonAssetAsyncLoader>.instance)
		};
	}

	public object GetJsonDataInResources(string fileName, int deserializeType = 0)
	{
		TextAsset textAsset = VespaResource.Load<TextAsset>("DefaultTableData/" + fileName);
		if (textAsset == null)
		{
			return null;
		}
		if (deserializeType == 0)
		{
			return JsonMapper.ToObject(textAsset.text);
		}
		return MiniJson.Deserialize(textAsset.text);
	}

	public bool IsLoaded()
	{
		return LoadStatus == ELoadStatus.Finish;
	}

	public void ChainUpdate()
	{
		if (LoadStatus == ELoadStatus.Loading)
		{
			UpdateLoading();
		}
	}

	private void EndOfLoading()
	{
		LoadStatus = ELoadStatus.Finish;
		_ = (DateTime.Now - _loadStartedTime).TotalMilliseconds;
		Vespa_System_Action_Extension_Method.fire(_onLoad);
		_onLoad = null;
	}

	private bool CheckAllDataContainerIsLoaded()
	{
		for (int i = 0; i < _dataContainerList.Count; i++)
		{
			if (!_dataContainerList[i].IsLoaded)
			{
				return false;
			}
		}
		return true;
	}

	private void EditorUpdateLoading()
	{
		if (!IsLoaded())
		{
			UpdateLoading();
		}
	}

	private void UpdateLoading()
	{
		if (!CheckAllDataContainerIsLoaded())
		{
			return;
		}
		LoadStatus = ELoadStatus.FinalizeLoading;
		foreach (IBaseDataContainer dataContainer in _dataContainerList)
		{
			string loadError = dataContainer.LoadError;
			if (!string.IsNullOrEmpty(loadError))
			{
				LoadError = loadError;
				break;
			}
		}
		EndOfLoading();
	}

	public ConstantDataContainer LoadConstantTableInResource()
	{
		return GameRoot.Instance.ConstantDataContainer;
	}
}
