namespace NGame2.NUI.NWorldMap.NMapLayout;

public class CrowdKey : Tuple<CrowdType, long>
{
	public CrowdKey(CrowdType type, long id)
		: base(type, id)
	{
	}
}
