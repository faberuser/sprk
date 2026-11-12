using NShared.NJit;

namespace NShared;

public class StateToolTipDataContainer : JitDataContainerDefault<int, StateToolTipData>
{
	public const string TABLENAME = "StateToolTipTable";

	public override TablePathDefine PathDefine => new TablePathDefine("StateToolTipTable");

	public StateToolTipData GetStateToolTipData(StateData stateData)
	{
		foreach (StateToolTipData allDatum in GetAllData())
		{
			if (allDatum.StateType.Contains(stateData.StateType))
			{
				return allDatum;
			}
		}
		return null;
	}
}
