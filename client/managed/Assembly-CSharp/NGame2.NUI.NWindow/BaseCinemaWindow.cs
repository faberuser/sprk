using System;
using System.Collections;
using NGame2.NUI.NRender3D.NCinema;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NShared;
using NVespa;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class BaseCinemaWindow : FrameControlWindow
{
	public UITexture NodeTreeTexture;

	public ECinemaOwnerType CinemaOwnerType;

	[SerializeField]
	protected string _cinemaWindowDataName;

	protected CinemaWindowData _cinemaWindowData;

	protected CinemaOwner _cinemaOwner;

	private bool _isInit;

	protected bool _playEndNodeTree = true;

	private bool _useSceneChanger;

	private SceneChanger OpenSceneChanger;

	private SceneChanger CloseSceneChanger;

	protected bool _isOwnerWindow
	{
		get
		{
			if (_cinemaWindowData == null)
			{
				return false;
			}
			return _cinemaWindowData.IsOwnerWindow;
		}
	}

	protected bool _isOpenOncePlay
	{
		get
		{
			if (_cinemaWindowData == null)
			{
				return false;
			}
			return _cinemaWindowData.IsOpenPlayOnce;
		}
	}

	protected string _openNodeTreePack
	{
		get
		{
			if (_cinemaWindowData == null)
			{
				return "";
			}
			return _cinemaWindowData.OpenNodeTreePack;
		}
	}

	protected string _openNodeTreeName
	{
		get
		{
			if (_cinemaWindowData == null)
			{
				return "";
			}
			return _cinemaWindowData.OpenNodeTreeName;
		}
	}

	protected bool _isCloseOncePlay
	{
		get
		{
			if (_cinemaWindowData == null)
			{
				return false;
			}
			return _cinemaWindowData.IsClosePlayOnce;
		}
	}

	protected string _closeNodeTreePack
	{
		get
		{
			if (_cinemaWindowData == null)
			{
				return "";
			}
			return _cinemaWindowData.CloseNodeTreePack;
		}
	}

	protected string _closeNodeTreeName
	{
		get
		{
			if (_cinemaWindowData == null)
			{
				return "";
			}
			return _cinemaWindowData.CloseNodeTreeName;
		}
	}

	protected IDataProvider _provider
	{
		get
		{
			if (_cinemaOwner == null)
			{
				return null;
			}
			return _cinemaOwner.DataProvider;
		}
	}

	public override void Open()
	{
		OpenCinema();
		base.Open();
	}

	public override void Open(Action onComplete)
	{
		OpenCinema();
		base.Open(onComplete);
	}

	public virtual void Open(Action onOpen, Action onComplete)
	{
		OpenCinema();
		Open(onOpen, onComplete, null);
	}

	public virtual void OpenCustomCinema(string name, Action onOpen, Action onComplete)
	{
		OpenCinema(name);
		UIHelper.EnableGameObject(base.gameObject, enable: true);
		UIHelper.EnableGameObject(WindowRoot, enable: false);
		new SequenceTask().ADDTASK(new WaitTask(() => !_cinemaOwner.IsCompleted)).ADDTASK(new DelegateTask(delegate
		{
			Open(onOpen, onComplete, null);
		}).WrapperTouchControl().DisableTouch(DisableTouchWhileOpening)).Commit(GetType());
	}

	private void OpenCinema(string customName = "")
	{
		LoadCinemaWindowData(customName);
		LoadCinemaOwner();
		if (!_isOwnerWindow)
		{
			SetNextCienmaNodeTree(_isOpenOncePlay, _openNodeTreePack, _openNodeTreeName, NodeTreeOpenOnStart, NodeTreeOpenOnEnd);
		}
	}

	private void CloseCinema()
	{
		if (_provider != null)
		{
			_provider.RemoveTexture(NodeTreeTexture);
		}
		if (_isOwnerWindow)
		{
			if (_cinemaOwner != null)
			{
				_cinemaOwner.Close();
			}
		}
		else if (_provider != null && _playEndNodeTree)
		{
			SetNextCienmaNodeTree(_isCloseOncePlay, _closeNodeTreePack, _closeNodeTreeName, NodeTreeCloseOnStart, NodeTreeCloseOnEnd);
		}
		_cinemaOwner = null;
		_cinemaWindowData = null;
		_isInit = false;
		if (NodeTreeTexture != null)
		{
			NodeTreeTexture.mainTexture = null;
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		CloseCinema();
	}

	public override void Close()
	{
		if (_useSceneChanger)
		{
			VespaCoroutine.Start(this, coCloseWithSceneChanger(this));
		}
		else
		{
			base.Close();
		}
	}

	private void LoadCinemaWindowData(string customName)
	{
		if (!String_Extension_Method.IsNullOrEmpty(customName))
		{
			_cinemaWindowData = GameRoot.Instance.CinemaWindowDataContainer.GetData(customName);
			return;
		}
		if (String_Extension_Method.IsNullOrEmpty(_cinemaWindowDataName))
		{
			_cinemaWindowDataName = GetType().Name;
		}
		_cinemaWindowData = GameRoot.Instance.CinemaWindowDataContainer.GetData(_cinemaWindowDataName);
	}

	private void LoadCinemaOwner()
	{
		_cinemaOwner = SingletonAuto<CinemaOwnerContainer>.instance.GetCinemaOwner(CinemaOwnerType);
		if (_provider == null)
		{
			CinemaWindowDataProvider cinemaWindowDataProvider = new CinemaWindowDataProvider
			{
				NodeTreePack = _openNodeTreePack,
				NodeTreeName = _openNodeTreeName
			};
			cinemaWindowDataProvider.SetTexture(NodeTreeTexture);
			_cinemaOwner.Open(cinemaWindowDataProvider);
			_cinemaOwner.SetAction(NodeTreeOpenOnStart, NodeTreeOpenOnEnd);
		}
		else if (NodeTreeTexture != null)
		{
			if (!NodeTreeTexture.isAnchored)
			{
				int width = ((_provider.Texture != null) ? _provider.Texture.width : _provider.TextureWidth);
				int height = ((_provider.Texture != null) ? _provider.Texture.height : _provider.TextureHeight);
				NodeTreeTexture.width = width;
				NodeTreeTexture.height = height;
			}
			_provider.SetTexture(NodeTreeTexture);
			_cinemaOwner.SetRenderTexture();
		}
	}

	public void SetNextCienmaNodeTree(bool isOncePlay, string nodeTreePack, string nodeTreeName, Action onStart, Action onEnd)
	{
		if (_cinemaOwner != null && !String_Extension_Method.IsNullOrEmpty(nodeTreePack) && !String_Extension_Method.IsNullOrEmpty(nodeTreeName))
		{
			_cinemaOwner.SetNextNodeTree(isOncePlay, nodeTreePack, nodeTreeName, onStart, onEnd);
		}
	}

	public void PlayNodeTree(string cinemaName, bool isOpen, Action onStart, Action onEnd)
	{
		if (String_Extension_Method.IsNullOrEmpty(cinemaName))
		{
			return;
		}
		CinemaWindowData data = GameRoot.Instance.CinemaWindowDataContainer.GetData(cinemaName);
		if (data != null)
		{
			if (isOpen)
			{
				SetNextCienmaNodeTree(data.IsOpenPlayOnce, data.OpenNodeTreePack, data.OpenNodeTreeName, onStart, onEnd);
			}
			else
			{
				SetNextCienmaNodeTree(data.IsClosePlayOnce, data.CloseNodeTreePack, data.CloseNodeTreeName, onStart, onEnd);
			}
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_isOwnerWindow)
		{
			if (!_isInit && _cinemaOwner != null && _cinemaOwner.IsCompleted && (NodeTreeTexture.width != _provider.TextureWidth || NodeTreeTexture.height != _provider.TextureHeight))
			{
				_cinemaOwner.SetCinemaTexture();
				_isInit = true;
			}
			if (_cinemaOwner != null)
			{
				_cinemaOwner.ChainUpdate();
			}
		}
	}

	protected virtual void NodeTreeOpenOnStart()
	{
	}

	protected virtual void NodeTreeOpenOnEnd()
	{
	}

	protected virtual void NodeTreeCloseOnStart()
	{
	}

	protected virtual void NodeTreeCloseOnEnd()
	{
	}

	public virtual void OpenWithSceneChanger(Action onOpen, Action onComplete, GlobalTopmostWindow.ESceneChangeType eSceneChangeType = GlobalTopmostWindow.ESceneChangeType.BlackScreen)
	{
		_useSceneChanger = true;
		UIHelper.EnableGameObject(base.gameObject, enable: true);
		UIHelper.EnableGameObject(WindowRoot, enable: false);
		VespaCoroutine.Start(this, coOpenWithSceneChanger(onOpen, onComplete, eSceneChangeType));
	}

	private IEnumerator coOpenWithSceneChanger(Action onOpen, Action onComplete, GlobalTopmostWindow.ESceneChangeType eSceneChangeType)
	{
		OpenSceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, eSceneChangeType);
		while (!OpenSceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		OpenCinema();
		BaseOpen(onOpen, onComplete);
	}

	private void BaseOpen(Action onOpen, Action onComplete)
	{
		Open(onOpen, delegate
		{
			if (OpenSceneChanger != null)
			{
				OpenSceneChanger.Close();
			}
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}, null);
	}

	private IEnumerator coCloseWithSceneChanger(BaseCinemaWindow This)
	{
		CloseSceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!CloseSceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		_useSceneChanger = false;
		if (This != null && This.IsOpen())
		{
			This.Close();
		}
		CloseSceneChanger.Close();
	}
}
