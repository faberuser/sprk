using NGame2.NBattleView;
using NGame2.NNodeTree.NObject;
using NGame2.NNodeTree.NRuntime;
using NVespa;

namespace NGame2.NNodeTree;

public class RuntimeInstance : SmallPooling<RuntimeInstance>, IRuntime, ISmallPoolingItem
{
	public interface IRuntimeStart
	{
		bool Start(RuntimeInstance runtime, IVariable variable);
	}

	private IRuntime _parent;

	private ISceneContainer _sceneContainer;

	private TreeData _tree;

	private IRuntimeStart _runtimeStart;

	private int _frameCount;

	private ERequestStop _requestStopFromInternal;

	private VespaList<long> _reservedUidList = new VespaList<long>();

	bool IRuntime.IsPlaying => m_runtimeState != ERuntimeState.End;

	ERuntimeState IRuntime.RuntimeState => m_runtimeState;

	IRuntime IRuntime.Parent
	{
		get
		{
			return _parent;
		}
		set
		{
			_parent = value;
		}
	}

	TreeData IRuntime.Tree => _tree;

	public int Layer { get; private set; }

	public IObserver Observer { get; private set; }

	public LocalTimer LocalTimer { get; private set; }

	public bool IsStoped { get; private set; }

	public ERuntimeState m_runtimeState { get; private set; }

	public LocalVariable Variable { get; private set; }

	public ISceneContainer SceneContainer
	{
		get
		{
			if (_sceneContainer != null)
			{
				return _sceneContainer;
			}
			if (BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
			{
				return BattleViewGenericSingleton<CreatureManager>.instance;
			}
			return NullSceneContainer.Instance;
		}
	}

	public ExecuteContainer ExecuteContainer { get; private set; }

	public ItemContainer ItemContainer { get; private set; }

	void ISmallPoolingItem.OnCreateNew()
	{
		ExecuteContainer = new ExecuteContainer(this);
		ItemContainer = new ItemContainer(this);
	}

	void ISmallPoolingItem.OnDestroy()
	{
	}

	void ISmallPoolingItem.OnCreate()
	{
	}

	void ISmallPoolingItem.OnRelease()
	{
		Layer = 0;
		Observer = null;
		LocalTimer = null;
		IsStoped = false;
		m_runtimeState = ERuntimeState.Init;
		if (Variable != null)
		{
			Vespa_Common_Class_Extension_Method.Interface((IVariable)Variable).ReleaseSelf();
			Variable = null;
		}
		ExecuteContainer.ResetAnyway();
		ItemContainer.ResetAnyway();
		_parent = null;
		_tree = null;
		_runtimeStart = null;
		_frameCount = -1;
		_requestStopFromInternal = ERequestStop.None;
		_reservedUidList.Clear();
	}

	public void Open(TreeData tree, IVariable variable, ILocalTimer parentTimer, IObserver observer, int layer, IRuntimeStart runtimeStart)
	{
		Open(null, tree, variable, parentTimer, observer, layer, runtimeStart);
	}

	public void Open(ISceneContainer creatureContainer, TreeData tree, IVariable variable, ILocalTimer parentTimer, IObserver observer, int layer, IRuntimeStart runtimeStart)
	{
		if (observer == null)
		{
			Observer = NullObserver.Default;
		}
		else
		{
			Observer = observer;
		}
		Layer = layer;
		LocalTimer = new LocalTimer(parentTimer);
		m_runtimeState = ERuntimeState.Init;
		_runtimeStart = runtimeStart;
		_sceneContainer = creatureContainer;
		_tree = tree;
		variable?.Open(this);
		Variable = CreateLocalVariable(variable);
	}

	public void ReserveObjectUid(long uid)
	{
		ObjectBase objectBase = Container.instance.GetObjectBase(uid);
		if (!(objectBase == null))
		{
			Container.instance.CaptureObject(objectBase);
			_reservedUidList.Add(uid);
		}
	}

	private LocalVariable CreateLocalVariable(IVariable variable)
	{
		if (variable is LocalVariable result)
		{
			return result;
		}
		LocalVariable localVariable = SmallPooling<LocalVariable>.CreateInstance();
		if (variable != null)
		{
			localVariable.SetParent(variable);
			variable.ReleaseSelf();
		}
		Vespa_Common_Class_Extension_Method.Interface((IVariable)localVariable).Open(this);
		return localVariable;
	}

	bool IRuntime.Start()
	{
		if (IsStoped)
		{
			return false;
		}
		if (m_runtimeState != ERuntimeState.Init)
		{
			return false;
		}
		RegisterRuntimeVariable();
		if (_runtimeStart != null && !_runtimeStart.Start(this, Variable))
		{
			return false;
		}
		m_runtimeState = ERuntimeState.Playing;
		return true;
	}

	void IRuntime.Update()
	{
		if (m_runtimeState == ERuntimeState.Playing && _frameCount != GlobalTimer.frameCount)
		{
			_frameCount = GlobalTimer.frameCount;
			if (_parent != null && _parent.RuntimeState == ERuntimeState.Playing)
			{
				_parent.Update();
			}
			ExecuteContainer.Update(this);
			if (_requestStopFromInternal != ERequestStop.None)
			{
				Vespa_Common_Class_Extension_Method.Interface((IRuntime)this).RequestStop(_requestStopFromInternal);
				_requestStopFromInternal = ERequestStop.None;
			}
		}
	}

	bool IRuntime.IsEnd()
	{
		if (m_runtimeState == ERuntimeState.End)
		{
			return true;
		}
		if (ExecuteContainer.IsEnd())
		{
			m_runtimeState = ERuntimeState.End;
			return true;
		}
		return false;
	}

	void IRuntime.RequestStop(ERequestStop request)
	{
		if (!IsStoped)
		{
			IsStoped = true;
			ExecuteContainer.RequestStop(this, request);
			if (request == ERequestStop.Force)
			{
				Vespa_Common_Class_Extension_Method.Interface((IRuntime)this).Close();
			}
		}
	}

	void IRuntime.Close()
	{
		IsStoped = true;
		m_runtimeState = ERuntimeState.End;
		ItemContainer.Clear();
		if (Variable != null)
		{
			Vespa_Common_Class_Extension_Method.Interface((IVariable)Variable).ReleaseSelf();
			Variable = null;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_reservedUidList); i++)
		{
			Container.instance.ReleaseObjectUid(_reservedUidList[i]);
		}
		_reservedUidList.ClearAndRelease();
	}

	IRuntime IRuntime.CaptureSelf()
	{
		return SmallPooling<RuntimeInstance>.CaptureInstance(this);
	}

	void IRuntime.ReleaseSelf()
	{
		SmallPooling<RuntimeInstance>.ReleaseInstance(this);
	}

	public void RequestStopFromInternal(ERequestStop request)
	{
		if (_requestStopFromInternal < request)
		{
			_requestStopFromInternal = request;
		}
	}

	private void RegisterRuntimeVariable()
	{
		if (LocalTimer == null)
		{
			LocalTimer = new LocalTimer(MainTimer.DefaultLocal);
		}
		float beginTime = LocalTimer.time;
		Variable.AddValue(VariableKeyword.BeginTime, LocalTimer.time);
		Variable.AddFunc(VariableKeyword.CurrentTime, () => LocalTimer.time);
		Variable.AddFunc(VariableKeyword.ElapsedTime, () => LocalTimer.time - beginTime);
	}
}
