namespace NShared.NOperationValueReaderRuntime;

public class SkillLevel : IOperationValueReader
{
	long IOperationValueReader.Read(string[] arguments, OperationInfo operatinInfo)
	{
		return operatinInfo.FiredInfo.SkillLevel;
	}
}
