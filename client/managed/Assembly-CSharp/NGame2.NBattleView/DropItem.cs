using NGame2.NBattleField;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NContents.NRoot;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView;

public class DropItem : MonoBehaviour
{
	public enum EDropType
	{
		Item,
		Gold
	}

	public static readonly StringKey Key_DropPosition = StringKey.Get("DropPosition");

	private RuntimeInstance _runtime;

	public EDropType DropType { get; private set; }

	public int DropValue { get; private set; }

	public void Open(int groupIndex, Vector3 position, EDropType type, int dropValue)
	{
		base.transform.position = SingletonAuto<BattleFacade>.instance.ValidateBattlePos(groupIndex, position);
		DropType = type;
		DropValue = dropValue;
	}

	public void Close()
	{
		if (_runtime != null)
		{
			Vespa_Common_Class_Extension_Method.Interface((IRuntime)_runtime).RequestStop(ERequestStop.Normal);
			Vespa_Common_Class_Extension_Method.Interface((IRuntime)_runtime).ReleaseSelf();
			_runtime = null;
		}
	}

	public void ExecuteNodeTree(TreeData tree, int creatureUid, int creatureLayer)
	{
		if (tree != null && tree.ContentsRoot is ChildRoot runtimeStart)
		{
			ChildRoot.RuntimeVariable runtimeVariable = SmallPooling<ChildRoot.RuntimeVariable>.CreateInstance();
			runtimeVariable.AddValue(VariableKeyword.Myself, (long)creatureUid);
			runtimeVariable.AddValue(Key_DropPosition, base.transform.position);
			_runtime = SmallPooling<RuntimeInstance>.CreateInstance();
			_runtime.Open(tree, runtimeVariable, null, null, creatureLayer, runtimeStart);
			SceneAutoBehaviour<RuntimeGlobalManager>.instance.AddRuntime(Vespa_SmallPooling_ExtensionMethod.CaptureSmallInstance(_runtime), immediatelyStart: false);
		}
	}
}
