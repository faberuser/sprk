using System.Collections;
using NShared.NJit;

namespace NShared;

public class TutorialDataContainer : JitDataContainerDefault<TutorialKey, TutorialData>
{
	public const string TABLENAME = "TutorialTable";

	private int _lastTutorialIndex_in_JsonLoad;

	private int _lastTutorialSequence_in_JsonLoad;

	public override TablePathDefine PathDefine => new TablePathDefine("TutorialTable");

	protected override void OnLoadingValue_from_JsonData(IDictionary jsonDic, TutorialData value)
	{
		if (value.Index == _lastTutorialIndex_in_JsonLoad)
		{
			_lastTutorialSequence_in_JsonLoad++;
		}
		else
		{
			_lastTutorialIndex_in_JsonLoad = value.Index;
			_lastTutorialSequence_in_JsonLoad = 0;
		}
		value.Sequence = _lastTutorialSequence_in_JsonLoad;
	}

	public TutorialData GetData(int tutorialIndex, int sequence)
	{
		return GetData(new TutorialKey(tutorialIndex, sequence));
	}
}
