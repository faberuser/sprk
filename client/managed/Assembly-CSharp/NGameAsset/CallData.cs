namespace NGameAsset;

public class CallData
{
	public string Dump;

	public int CountResult;

	public int LoadSceneCount;

	public int Tick;

	public int SerialNo;

	public string ToString(string prefix)
	{
		return $"{prefix} (Result={CountResult}, LoadScene={LoadSceneCount}, Tick={Tick}, SerialNo={SerialNo}";
	}
}
