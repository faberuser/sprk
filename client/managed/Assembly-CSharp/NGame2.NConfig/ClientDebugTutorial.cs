using NVespa;

namespace NGame2.NConfig;

public class ClientDebugTutorial : SingletonAuto<ClientDebugTutorial>
{
	private LocalStorageBool _tutorialEnabled = new LocalStorageBool(i_default: true, typeof(ClientDebugTutorial).FullName + "TutorialEnabled");

	private LocalStorageInt _testTutorialIndex = new LocalStorageInt(0, typeof(ClientDebugTutorial).FullName + "TestTutorialIndex");

	public bool TutorialEnabled
	{
		get
		{
			return _tutorialEnabled.Value;
		}
		set
		{
			_tutorialEnabled.Value = value;
		}
	}

	public int TestTutorialIndex
	{
		get
		{
			return _testTutorialIndex.Value;
		}
		set
		{
			_testTutorialIndex.Value = value;
		}
	}
}
