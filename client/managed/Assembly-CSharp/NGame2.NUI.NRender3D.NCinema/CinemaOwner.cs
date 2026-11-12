using System;
using NVespa;

namespace NGame2.NUI.NRender3D.NCinema;

public class CinemaOwner : ICinemaOwner
{
	private IDataProvider _dataProvider;

	private Cinema _cinema;

	private bool _hasNextNodeTree;

	private bool _isNextOncePlay;

	private ICinemaState _cinemaState;

	private Action _onStart;

	private Action _onEnd;

	public ECinemaOwnerType OwnerType { get; set; }

	public bool IsCompleted
	{
		get
		{
			if (_cinemaState == null)
			{
				return true;
			}
			if (_cinemaState.State != ECinemaState.Create)
			{
				return _cinemaState.State != ECinemaState.Loading;
			}
			return false;
		}
	}

	public ECinemaState CinemaState
	{
		get
		{
			if (_cinemaState == null)
			{
				return ECinemaState.Invalid;
			}
			return _cinemaState.State;
		}
	}

	public IDataProvider DataProvider => _dataProvider;

	public Cinema Cinema
	{
		get
		{
			return _cinema;
		}
		set
		{
			_cinema = value;
		}
	}

	public bool HasNextNodeTree
	{
		get
		{
			return _hasNextNodeTree;
		}
		set
		{
			_hasNextNodeTree = value;
		}
	}

	public bool IsNextOncePlay => _isNextOncePlay;

	public Action OnStart => _onStart;

	public Action OnEnd => _onEnd;

	public CinemaOwner(ECinemaOwnerType ownerType)
	{
		OwnerType = ownerType;
	}

	public void Open(IDataProvider dataProvider)
	{
		if (dataProvider != null && !String_Extension_Method.IsNullOrEmpty(dataProvider.NodeTreeName) && !String_Extension_Method.IsNullOrEmpty(dataProvider.NodeTreePack))
		{
			_dataProvider = dataProvider;
			SetTextureSize();
			SetData(dataProvider.NodeTreePack, dataProvider.NodeTreeName);
			SetState(new CreateCinemaState());
		}
	}

	public void Close()
	{
		_onStart = null;
		_onEnd = null;
		if (_dataProvider != null)
		{
			_dataProvider.Close();
		}
		_dataProvider = null;
		_cinemaState = null;
		_hasNextNodeTree = false;
		Cinema.ReleaseInstance(_cinema);
		_cinema = null;
	}

	private void SetTextureSize()
	{
		if (_dataProvider.Texture != null)
		{
			_dataProvider.TextureHeight = _dataProvider.Texture.height;
			_dataProvider.TextureWidth = _dataProvider.Texture.width;
		}
		else
		{
			SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out var width, out var height);
			_dataProvider.TextureWidth = width;
			_dataProvider.TextureHeight = height;
		}
	}

	private void SetData(string nodeTreePack, string nodeTreeName)
	{
		if (_dataProvider != null && !String_Extension_Method.IsNullOrEmpty(nodeTreePack) && !String_Extension_Method.IsNullOrEmpty(nodeTreeName))
		{
			if (_dataProvider.NodeTreePack != nodeTreePack)
			{
				_dataProvider.NodeTreePack = nodeTreePack;
			}
			if (_dataProvider.NodeTreeName != nodeTreeName)
			{
				_dataProvider.NodeTreeName = nodeTreeName;
			}
		}
	}

	public void SetNextNodeTree(bool isOncePlay, string nodeTreePack, string nodeTreeName, Action onStart, Action onEnd)
	{
		SetData(nodeTreePack, nodeTreeName);
		_hasNextNodeTree = true;
		_isNextOncePlay = isOncePlay;
		SetAction(onStart, onEnd);
	}

	public void SetAction(Action onStart, Action onEnd)
	{
		_onStart = onStart;
		_onEnd = onEnd;
	}

	public void SetState(ICinemaState renderTargetCinemaState)
	{
		_cinemaState = renderTargetCinemaState;
		if (_cinemaState != null && !_cinemaState.OnStart(this))
		{
			Vespa_System_Action_Extension_Method.fire(OnEnd);
		}
	}

	public void SetRenderTexture()
	{
		if (_dataProvider != null && !(_dataProvider.Texture == null))
		{
			_dataProvider.SetRenderTexture(_cinema.RenderTarget);
		}
	}

	public void SetCinemaTexture()
	{
		if (_cinema != null && _dataProvider != null)
		{
			_cinema.ChangeCinemaTexture(_dataProvider.Texture);
		}
	}

	public void NoticeError(string errorMessage)
	{
		if (!String_Extension_Method.IsNullOrEmpty(errorMessage))
		{
			VespaDebug.LogError(errorMessage);
		}
	}

	public void ChainUpdate()
	{
		if (_cinemaState != null)
		{
			_cinemaState.ChainUpdate();
		}
	}

	public void DrawGui()
	{
		if (Cinema != null)
		{
			Cinema.DrawGUI();
		}
	}
}
