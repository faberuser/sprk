namespace NGame2.NMessage.NDirect;

public static class OperationHashCode
{
	public static int GetHashCode(int skillIndex, int operationNo)
	{
		return skillIndex * 100 + operationNo;
	}
}
