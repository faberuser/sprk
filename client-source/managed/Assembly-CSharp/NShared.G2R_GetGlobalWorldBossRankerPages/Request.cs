namespace NShared.G2R_GetGlobalWorldBossRankerPages;

[Request]
public class Request
{
	public int WorldBossIndex { get; set; }

	public int Season { get; set; }

	public int StartPage { get; set; }

	public int EndPage { get; set; }
}
