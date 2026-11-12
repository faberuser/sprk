namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class GachaSelectShopEvent
{
	public SelectShopNoticeInfo[] SelectShopNoticeInfos { get; set; }
}
