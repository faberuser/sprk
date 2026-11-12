namespace NGame2.NChatting;

public interface IChannelListener
{
	void OnChannelEnter(int channelNo);

	void OnChannelLeave();
}
