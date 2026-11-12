using System;
using NGameAsset;
using NGameAsset.NWorldMap;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class MapImageLoader
{
	private UITexture[] _mapTex;

	private IWorldMapLoad _worldMapLoad;

	private AsyncManager.SingleEntry _asyncEntry;

	public void Open(UITexture[] uiTex)
	{
		_mapTex = uiTex;
	}

	public void Load(CampaignChapterData chapterData, Action onComplete)
	{
		if (chapterData == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		ReleaseGameAsset();
		_worldMapLoad = SingletonAuto<Container>.instance.CreateLoad(chapterData.Map);
		if (_worldMapLoad == null)
		{
			VespaDebug.LogError("Map image not found, code = " + chapterData.Map);
			OnCompleteWorldMapLoad(chapterData, onComplete);
			return;
		}
		if (_worldMapLoad.IsDone)
		{
			OnCompleteWorldMapLoad(chapterData, onComplete);
			return;
		}
		_asyncEntry = new AsyncManager.SingleEntry();
		_asyncEntry.AsyncLoad = _worldMapLoad;
		_asyncEntry.OnComplete = delegate
		{
			OnCompleteWorldMapLoad(chapterData, onComplete);
			_asyncEntry = null;
		};
		SingletonUpdater<AsyncManager>.instance.Add(_asyncEntry);
	}

	public void Unload()
	{
		ReleaseGameAsset();
	}

	private void OnCompleteWorldMapLoad(CampaignChapterData chapterData, Action onComplete)
	{
		if (_worldMapLoad == null || !_worldMapLoad.IsValid)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		if (_worldMapLoad.MapImageCount == 0)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		EMapArrangement mapArrangement = chapterData.MapArrangement;
		int num = 0;
		for (int i = 0; i < _worldMapLoad.MapImageCount; i++)
		{
			MapImageData imageData = _worldMapLoad.GetImageData(0);
			num += ((mapArrangement == EMapArrangement.Horizontal) ? imageData.DisplayWidth : imageData.DisplayHeight);
		}
		int num2 = ((mapArrangement == EMapArrangement.Horizontal) ? (-chapterData.MapSizeX / 2) : (chapterData.MapSizeY / 2));
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_mapTex); j++)
		{
			if (_mapTex[j] == null)
			{
				continue;
			}
			UIHelper.EnableGameObject(_mapTex[j], j < _worldMapLoad.MapImageCount);
			if (j < _worldMapLoad.MapImageCount)
			{
				MapImageData imageData2 = _worldMapLoad.GetImageData(j);
				UIHelper.SetTextureMaterial(_mapTex[j], _worldMapLoad.GetImageAsset(j));
				if (mapArrangement == EMapArrangement.Horizontal)
				{
					int num3 = Mathf.RoundToInt((float)chapterData.MapSizeX * (float)imageData2.DisplayWidth / (float)num);
					_mapTex[j].transform.localPosition = Vector3.right * (num2 + num3 / 2);
					_mapTex[j].width = num3 + 1;
					_mapTex[j].height = chapterData.MapSizeY;
					float num4 = 1f / (float)num3;
					_mapTex[j].uvRect = new Rect((0f - num4) * 0.5f, 0f, 1f + num4, 1f);
					num2 += num3;
				}
				else
				{
					int num5 = Mathf.RoundToInt((float)chapterData.MapSizeY * (float)imageData2.DisplayHeight / (float)num);
					_mapTex[j].transform.localPosition = Vector3.up * (num2 - num5 / 2);
					_mapTex[j].width = chapterData.MapSizeX;
					_mapTex[j].height = num5 + 1;
					float num6 = 1f / (float)num5;
					_mapTex[j].uvRect = new Rect(0f, (0f - num6) * 0.5f, 1f, 1f + num6);
					num2 -= num5;
				}
			}
			else
			{
				UIHelper.SetTextureMaterial(_mapTex[j], null);
			}
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	public void ReleaseGameAsset()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_mapTex); i++)
		{
			UIHelper.SetTextureMaterial(_mapTex[i], null);
		}
		if (_asyncEntry != null)
		{
			_asyncEntry.OnComplete = null;
			_asyncEntry = null;
		}
		if (_worldMapLoad != null)
		{
			_worldMapLoad.Close();
			_worldMapLoad = null;
		}
	}
}
