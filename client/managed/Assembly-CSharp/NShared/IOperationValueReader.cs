namespace NShared;

public interface IOperationValueReader
{
	long Read(string[] arguments, OperationInfo operatinInfo);
}
