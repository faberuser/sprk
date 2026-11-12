using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class MemoryFragmentManagement : BaseManagement<MemoryFragmentManagement>, MemoryFragment.Listener, IUserObserver<EntryMenuManagement.OpenType>, IUserObserver
{
	private SceneChanger _sceneChanger;

	private static int _currentMemoryFragmentIndex = -1;

	public int CurrentMemoryFragmentIndex
	{
		get
		{
			return _currentMemoryFragmentIndex;
		}
		set
		{
			_currentMemoryFragmentIndex = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<MemoryFragment>("Resources/Prefab2/Window/MemoryFragment/MemoryFragment");
		SceneAutoBehaviour<EntryMenuManagement>.instance.ObserverManager.AddObserver(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void OpenMemoryFragment()
	{
		GetInstance<MemoryFragment>().Open(this);
	}

	public void OpenMemoryFragment(MemoryFragmentData memoryFragmentData, bool isWindowActive = true)
	{
		GetInstance<MemoryFragment>().Open(this, memoryFragmentData);
		GetInstance<MemoryFragment>().gameObject.SetActive(isWindowActive);
	}

	void MemoryFragment.Listener.OnEnterMemoryFragmentEntryMenu(string enterNPCCode)
	{
		VespaCoroutine.Start(coEnterMemoryFragmentEntryMenu(enterNPCCode));
	}

	private IEnumerator coEnterMemoryFragmentEntryMenu(string enterNPCCode)
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		GetInstance<MemoryFragment>().Close();
		SceneAutoBehaviour<EntryMenuManagement>.instance.CloseEntryMenu(null);
		MemoryFragment memoryFragment = GetInstance<MemoryFragment>();
		memoryFragment.gameObject.SetActive(value: false);
		SceneAutoBehaviour<EntryMenuManagement>.instance.OpenEntryMenuUnNode(enterNPCCode, skipAni: false, playNPCSound: true, isIgnoreTrigger: false, delegate
		{
		});
		GlobalAutoBehaviour<GlobalManagement>.instance.RegisterOpenedWindow(memoryFragment);
		while (!SceneAutoBehaviour<EntryMenuManagement>.instance.IsEntryMenuInit)
		{
			yield return null;
		}
		sceneChanger.Close();
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
	}

	public MemoryFragmentData GetOpenedWindowCurrentMemoryFragmentData()
	{
		MemoryFragment memoryFragment = GetInstance<MemoryFragment>();
		if (memoryFragment == null)
		{
			return null;
		}
		return memoryFragment.CurrentMemoryFragmentData;
	}

	public void StartSceneChanger(float timeout = 5f, GlobalTopmostWindow.ESceneChangeType type = GlobalTopmostWindow.ESceneChangeType.BlackScreen)
	{
		_sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(timeout, type);
	}

	public void EndSceneChanger()
	{
		if (_sceneChanger != null && !_sceneChanger.IsClosed())
		{
			_sceneChanger.Close();
		}
	}

	void IUserObserver<EntryMenuManagement.OpenType>.OnChanged(EntryMenuManagement.OpenType changedInfo)
	{
		if (changedInfo == EntryMenuManagement.OpenType.UnNodeData)
		{
			StartSceneChanger();
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
