using NGame2.NUI.NManager;
using NGameAsset;
using NShared.NWorldMap;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class NodeIndicator<TData> : INodeIndicator where TData : ITimerEvent
{
	public class StockData
	{
		public MapComponent MapComp;

		public TData Data;

		public string AssetEffectCode;

		public Transform ParentTm;

		public Vector3 WorldPosition;

		public bool RenderModeWorldMap;

		public float RenderModeAngle;

		public bool EatEffectOnClose;

		public ILocalTimer LocalTimer;
	}

	private GameObject _effectGo;

	private IAsyncLoad<IEffect> _effectAsync;

	public StockData Stock { get; protected set; }

	public TData Data => Stock.Data;

	public int ChapterIndex { get; protected set; }

	public int DungeonIndex { get; protected set; }

	public GameObject EffectGo => _effectGo;

	public NodeIndicator()
	{
	}

	public NodeIndicator(int chapterIndex, int dungeonIndex)
	{
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
	}

	private void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			CloseImmediately();
		}
	}

	public void Open(StockData stock)
	{
		Stock = stock;
		_effectGo = new GameObject();
		_effectGo.layer = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.Bottom3D).Layer;
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(_effectGo.transform, Stock.ParentTm);
		_effectGo.transform.position = Stock.WorldPosition;
		SetScale(100);
		_effectGo.AddComponent<DelegateUpdater>().Open(this, ChainUpdate);
		CreateAssetEffect();
	}

	public void Close(bool eatEffect)
	{
		CloseInternal(eatEffect, releaseImmediately: false);
	}

	public void CloseImmediately()
	{
		CloseInternal(eatEffect: false, releaseImmediately: true);
	}

	private void CloseInternal(bool eatEffect, bool releaseImmediately)
	{
		if (eatEffect && Stock.EatEffectOnClose)
		{
			CreateEatEffect();
		}
		if (_effectAsync != null)
		{
			EffectContainer.ReleaseEffect(_effectAsync, detachParent: true, releaseImmediately);
			_effectAsync = null;
		}
		if (_effectGo != null)
		{
			VespaObject.DestroyGo(_effectGo);
			_effectGo = null;
		}
	}

	public void Show(bool show)
	{
		UIHelper.EnableGameObject(_effectGo, show);
	}

	public void Rotate(int angle)
	{
		Vector3 forward = Quaternion.AngleAxis(angle, Vector3.up) * _effectGo.transform.forward;
		_effectGo.transform.forward = forward;
	}

	public void SetScale(int scalePercentage)
	{
		_effectGo.transform.localScale = Vector3.one * Stock.MapComp.ScaleData.MapIndicatorScale * scalePercentage / 100f;
	}

	private void CreateAssetEffect()
	{
		_effectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync(Stock.AssetEffectCode, _effectGo.layer);
		_effectAsync.Value.SetParent(_effectGo.transform);
		if (Stock.RenderModeWorldMap)
		{
			_effectAsync.Value.SetShaderTransform(new ShaderTransformData(Stock.RenderModeAngle, depthShift: true), _effectGo.transform);
		}
		if (Stock.LocalTimer != null)
		{
			_effectAsync.Value.SetTimer(Stock.LocalTimer);
		}
	}

	private void ChainUpdate()
	{
		if (_effectAsync != null)
		{
			MapLayout instance = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
			float num = Mathf.Clamp01(1f - instance.MapViewer.GetNodeMenuWeight() * 2f);
			if (num < Mathf.Epsilon)
			{
				_effectAsync.Value.SetVisible(isVisible: false);
			}
			else
			{
				_effectAsync.Value.SetVisible(isVisible: true);
				_effectAsync.Value.GetRenderGroupModifier().SetExtraAlpha(num);
			}
			_effectAsync.Value.Update();
		}
	}

	private void CreateEatEffect()
	{
		if (!(_effectGo == null))
		{
			IAsyncLoad<IEffect> asyncLoad = SingletonAuto<EffectContainer>.instance.CreateAsync("UI.WorldMap.Indicator.Eat", _effectGo.layer);
			asyncLoad.Value.SetDummyParent(_effectGo.transform.position, _effectGo.transform.rotation, _effectGo.transform.lossyScale);
			if (Stock.RenderModeWorldMap)
			{
				asyncLoad.Value.SetShaderTransform(new ShaderTransformData(Stock.RenderModeAngle, depthShift: true), _effectGo.transform);
			}
			SingletonUpdater<AssetUpdateManager>.instance.Add(asyncLoad, isGlobal: false);
			SoundStock stock = new SoundStock
			{
				Code = "UI.WorldMap.Coin",
				LocalTimer = GlobalTimer.DefaultLocal
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
		}
	}
}
