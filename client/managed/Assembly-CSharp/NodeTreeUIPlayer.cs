using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NNodeTree;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NRender3D.NRenderTargetCinema;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGameAsset;
using NShared;
using NVespa;
using UnityEngine;

[RequireComponent(typeof(UITexture))]
public class NodeTreeUIPlayer : MonoBehaviour, IInspectorGuiDrawer
{
	[Serializable]
	public class CameraStockData
	{
		public float ZoomOutRate;

		public float ZoomInDistance;

		public Vector3 ZoomInOffset;

		public float ZoomOutDistance;

		public Vector3 ZoomOutOffset;

		public float LookAngleX;
	}

	public class NodeTreeCreatureStock
	{
		public int CreatureIndex;

		public int Star;

		public Vector3 Position;

		public Vector3 Rotation;

		public float Scale;
	}

	public UITexture CinemaTexture;

	public string NodeTreeName = "";

	public string NodeTreePackCode = "";

	public float UVPosX;

	public float UVPosY;

	public float UVWidth = 1f;

	public float UVHeight = 1f;

	public bool UseCameraStock;

	public CameraStockData CameraStock;

	private RenderTargetCinema _cinema;

	private SceneChanger _sceneChanger;

	protected CameraController _cameraController;

	private NodeTreeCreatureStock _creatureStock;

	private Dictionary<StringKey, long> _dicAddKeywords = new Dictionary<StringKey, long>();

	private int _completedCount;

	private int _reservedCount;

	public void PlayOnce(string packCode, string treeName, System.Action onStart = null, System.Action onEnd = null, bool orthographic = false)
	{
		SetTexture();
		SetData(packCode, treeName);
		VespaCoroutine.Start(this, CoPlay(isOncePlay: true, onStart, onEnd, orthographic));
	}

	public void PlayOnce(System.Action onEnd = null, System.Action onStart = null, bool orthographic = false)
	{
		PlayOnce(NodeTreePackCode, NodeTreeName, onStart, onEnd, orthographic);
	}

	public void PlaySequence(string packCode, string treeName, System.Action onStart, System.Action onEnd, bool orthographic = false)
	{
		SetTexture();
		SetData(packCode, treeName);
		VespaCoroutine.Start(this, CoPlay(isOncePlay: false, onStart, onEnd, orthographic));
	}

	public void AddNodeTreeKeyWords(StringKey key, long value)
	{
		_dicAddKeywords.Remove(key);
		_dicAddKeywords.Add(key, value);
	}

	private void IncreaseReservedCount()
	{
		_reservedCount++;
	}

	private void IncreaseCompletedCount()
	{
		_completedCount++;
	}

	protected virtual void SetCameraController()
	{
		if (UseCameraStock)
		{
			_cameraController = new CameraController(_cinema, 1);
			_cameraController.ZoomOutRate = CameraStock.ZoomOutRate;
			_cameraController.ZoomIn_Distance = CameraStock.ZoomInDistance;
			_cameraController.ZoomIn_Offset = CameraStock.ZoomInOffset;
			_cameraController.ZoomOut_Distance = CameraStock.ZoomOutDistance;
			_cameraController.ZoomOut_Offset = CameraStock.ZoomOutOffset;
			_cameraController.LookAngleX = CameraStock.LookAngleX;
			RenderFacade rENDERFACADE = _cinema.GETRENDERFACADE(1);
			if (rENDERFACADE != null)
			{
				rENDERFACADE.AssetCreature.SetShaderTransform(new ShaderTransformData(CameraStock.LookAngleX, depthShift: true), null);
			}
		}
	}

	private void LateUpdate()
	{
		if (_cameraController != null)
		{
			_cameraController.ChainLateUpdate();
		}
	}

	private void SetTexture()
	{
		if (CinemaTexture == null)
		{
			CinemaTexture = GetComponent<UITexture>();
		}
	}

	public void SetData(string packCode, string treeName)
	{
		if (!String_Extension_Method.IsNullOrEmpty(treeName))
		{
			NodeTreeName = treeName;
		}
		if (!String_Extension_Method.IsNullOrEmpty(packCode))
		{
			NodeTreePackCode = packCode;
		}
	}

	private void AddNodeTreeKeyWords()
	{
		foreach (KeyValuePair<StringKey, long> dicAddKeyword in _dicAddKeywords)
		{
			_cinema.AddKeyword(dicAddKeyword.Key, dicAddKeyword.Value);
		}
		_dicAddKeywords.Clear();
	}

	public bool IsCompletedPlay()
	{
		return _reservedCount == _completedCount;
	}

	public bool IsCompletedLoad()
	{
		if (_cinema == null)
		{
			return false;
		}
		return _cinema.IsCompleteLoad();
	}

	protected IEnumerator CoPlay(bool isOncePlay, System.Action onStart, System.Action onEnd, bool orthographic = false)
	{
		IncreaseReservedCount();
		if (!IsValidData())
		{
			Vespa_System_Action_Extension_Method.fire(onStart);
			Vespa_System_Action_Extension_Method.fire(onEnd);
			IncreaseCompletedCount();
			yield break;
		}
		yield return new WaitUntil(() => _reservedCount - 1 == _completedCount);
		if (_cinema != null)
		{
			if (isOncePlay)
			{
				ReleaseCinema();
			}
			else if (!isOncePlay)
			{
				_cinema.LoadNodeTree(NodeTreePackCode, NodeTreeName);
			}
		}
		if (_cinema == null)
		{
			RenderTargetCinema.StockData stock = default(RenderTargetCinema.StockData);
			if (CinemaTexture != null)
			{
				stock.RenderTargetWidth = CinemaTexture.width;
				stock.RenderTargetHeight = CinemaTexture.height;
			}
			else
			{
				SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out stock.RenderTargetWidth, out stock.RenderTargetHeight);
			}
			stock.NodeTreePackCode = NodeTreePackCode;
			stock.NodeTreeName = NodeTreeName;
			_cinema = RenderTargetCinema.CreateInstance(stock);
		}
		if (_cinema == null)
		{
			Vespa_System_Action_Extension_Method.fire(onStart);
			Vespa_System_Action_Extension_Method.fire(onEnd);
			IncreaseCompletedCount();
			yield break;
		}
		Vespa_System_Action_Extension_Method.fire(onStart);
		CreatueCreature();
		AddNodeTreeKeyWords();
		do
		{
			yield return null;
			if (_cinema == null)
			{
				Vespa_System_Action_Extension_Method.fire(onEnd);
				IncreaseCompletedCount();
				yield break;
			}
		}
		while (!_cinema.IsCompleteLoad());
		OrthographicMode(orthographic);
		if (!_cinema.StartNodeTree(onEnd))
		{
			Vespa_System_Action_Extension_Method.fire(onEnd);
			IncreaseCompletedCount();
			yield break;
		}
		if (CinemaTexture != null && _cinema != null)
		{
			CinemaTexture.mainTexture = _cinema.RenderTarget;
			CinemaTexture.alpha = 1f;
			CinemaTexture.uvRect = new Rect(UVPosX, UVPosY, UVWidth, UVHeight);
		}
		IncreaseCompletedCount();
	}

	public RenderFacade GetRuntimeSoulStoneRenderfacade(int soulStoneIndex)
	{
		if (_cinema == null)
		{
			return null;
		}
		if (!_cinema.IsRuntime())
		{
			return null;
		}
		SoulWeaponNodeTreeData dataFromSoulStoneIndex = GameRoot.Instance.SoulWeaponNodeTreeDataContainer.GetDataFromSoulStoneIndex(soulStoneIndex);
		if (dataFromSoulStoneIndex == null)
		{
			return null;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(dataFromSoulStoneIndex.SoulStoneCreatureIndex);
		if (data == null)
		{
			return null;
		}
		string modelPrefix = data.ModelPrefix;
		foreach (RenderFacade renderfacade in _cinema.GetRenderfacades())
		{
			if (renderfacade.AssetCreatureAsync != null && renderfacade.AssetCreatureAsync.Stock.BodyPrefix.CompareTo(modelPrefix) == 0)
			{
				return renderfacade;
			}
		}
		return null;
	}

	public void AddCreatureStock(int creatureIndex, int star, Vector3 position, Vector3 rotation, float scale)
	{
		_creatureStock = new NodeTreeCreatureStock
		{
			CreatureIndex = creatureIndex,
			Star = star,
			Position = position,
			Rotation = rotation,
			Scale = scale
		};
	}

	private void CreatueCreature()
	{
		if (_creatureStock != null)
		{
			SpawnCreature(_creatureStock.CreatureIndex, _creatureStock.Star, _creatureStock.Position, _creatureStock.Rotation, _creatureStock.Scale);
			_creatureStock = null;
		}
	}

	public void Stop()
	{
		ReleaseCinema();
	}

	public void Close()
	{
		VespaCoroutine.StopAll(this);
		ReleaseCinema();
		_dicAddKeywords.Clear();
		_completedCount = 0;
		_reservedCount = 0;
	}

	public void ReleaseCinema()
	{
		RenderTargetCinema.ReleaseInstance(_cinema);
		_cinema = null;
	}

	public void CreautureRelease()
	{
		if (!(_cinema == null) && _cinema.GETRENDERFACADE(1) != null)
		{
			_cinema.DestroyCreature(1);
		}
	}

	public void SpawnCreature(int creatureIndex, int star, Vector3 position, Vector3 rotation, float size)
	{
		if (_cinema == null)
		{
			return;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(creatureIndex);
		if (data == null)
		{
			return;
		}
		AnimationData.RuntimeData runtimeData = null;
		RenderFacade rENDERFACADE = _cinema.GETRENDERFACADE(1);
		if (rENDERFACADE != null)
		{
			runtimeData = rENDERFACADE.AnimationData.Runtime;
		}
		CreautureRelease();
		string codeName = data.CodeName;
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, codeName, star);
		_cinema.SpawnCreature(null, 1, renderFacadeBuilder);
		RenderFacade rENDERFACADE2 = _cinema.GETRENDERFACADE(1);
		if (rENDERFACADE2 != null)
		{
			if (runtimeData != null)
			{
				rENDERFACADE2.AnimationData.Play(runtimeData.Stock, runtimeData.Frame, null);
				rENDERFACADE2.AnimationData.ChainUpdate();
			}
			Debug.Log("Create");
			rENDERFACADE2.transform.localScale = Vector3.one * size;
			rENDERFACADE2.transform.position = position;
			rENDERFACADE2.transform.eulerAngles = rotation;
			SetCameraController();
		}
	}

	public void OrthographicMode(bool orthographic)
	{
		if (_cinema != null)
		{
			_cinema.OrthographicMode(orthographic);
		}
	}

	private bool IsValidData()
	{
		if (String_Extension_Method.IsNullOrEmpty(NodeTreePackCode))
		{
			return false;
		}
		if (String_Extension_Method.IsNullOrEmpty(NodeTreeName))
		{
			return false;
		}
		return true;
	}
}
