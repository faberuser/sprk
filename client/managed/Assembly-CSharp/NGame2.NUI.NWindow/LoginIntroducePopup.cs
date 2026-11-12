using System;
using System.Collections;
using NGameAsset;
using NGameAsset.NBundleManager;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LoginIntroducePopup : BaseWindow
{
	protected Coroutine _coLoadAtlas;

	protected Action _onClose;

	[Header("LoginIntroducePopup")]
	public UITexture LoadingTexture;

	protected GameTitleSceneIntroduceDataContainer _gameIntroduceContainer => GameRoot.Instance.GameTitleSceneIntroduceDataContainer;

	public void Open(Action onComplete, Action onClose)
	{
		_onClose = onClose;
		base.Open(onComplete);
	}

	public virtual void OnClose()
	{
		UIHelper.EnableGameObject(LoadingTexture, enable: false);
		base.Close();
	}

	public void StartLoadAtlas(string atlasName, Action<UIAtlas> complete)
	{
		if (_coLoadAtlas != null)
		{
			StopCoroutine(_coLoadAtlas);
			_coLoadAtlas = null;
		}
		UIHelper.EnableGameObject(LoadingTexture, enable: true);
		_coLoadAtlas = StartCoroutine(coAtlasLoading(atlasName, complete));
	}

	protected IEnumerator coAtlasLoading(string atlasName, Action<UIAtlas> complete)
	{
		UIAtlas uIAtlas = null;
		while (uIAtlas == null)
		{
			yield return null;
			uIAtlas = GetAtlas(atlasName);
		}
		UIHelper.EnableGameObject(LoadingTexture, enable: false);
		Vespa_System_Action_Extension_Method.fire(complete, uIAtlas);
	}

	protected UIAtlas GetAtlas(string atlasName)
	{
		if (String_Extension_Method.IsNullOrEmpty(atlasName))
		{
			return null;
		}
		string fileName = $"{atlasName}.unity3d";
		BundleLoadFromFile bundleLoadFromFile = new BundleLoadFromFile();
		bundleLoadFromFile.Open(fileName);
		bundleLoadFromFile.SetSpecFolder();
		IBundleLoad bundleLoad = Vespa_Common_Class_Extension_Method.Interface((IBundleLoad)bundleLoadFromFile);
		if (!bundleLoad.Exists())
		{
			return null;
		}
		bundleLoad.StartLoad();
		AssetBundle assetBundle = bundleLoadFromFile.AssetBundle;
		if (assetBundle == null)
		{
			return null;
		}
		return assetBundle.mainAsset as UIAtlas;
	}

	protected int GetDataCount(GameIntroduceType eType)
	{
		int num = 0;
		if (_gameIntroduceContainer != null)
		{
			foreach (GameTitleSceneIntroduceData allDatum in _gameIntroduceContainer.GetAllData())
			{
				if (allDatum.Type == eType)
				{
					num++;
				}
			}
		}
		return num;
	}

	protected GameTitleSceneIntroduceData GetTableData(int order, GameIntroduceType eType)
	{
		if (_gameIntroduceContainer != null)
		{
			int num = 0;
			foreach (GameTitleSceneIntroduceData allDatum in _gameIntroduceContainer.GetAllData())
			{
				if (allDatum.Type == eType)
				{
					if (order == num)
					{
						return allDatum;
					}
					num++;
				}
			}
		}
		return null;
	}
}
