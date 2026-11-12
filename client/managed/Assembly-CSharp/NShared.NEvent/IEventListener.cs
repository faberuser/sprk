namespace NShared.NEvent;

public interface IEventListener
{
	void OnEvent(IEvent triggerEvent);
}
