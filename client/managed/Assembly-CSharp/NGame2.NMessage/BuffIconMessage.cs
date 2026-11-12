using NVespa;

namespace NGame2.NMessage;

public class BuffIconMessage : SmallPooling<BuffIconMessage>, ISmallPoolingItem, IMessage
{
	public enum EEventType
	{
		Show,
		Hide
	}

	public bool IsDummy { get; set; }

	public EEventType EventType { get; private set; }

	public long CreatureUid { get; private set; }

	public string IconCode { get; private set; }

	public string CustomCode { get; private set; }

	void ISmallPoolingItem.OnCreateNew()
	{
	}

	void ISmallPoolingItem.OnDestroy()
	{
	}

	void ISmallPoolingItem.OnCreate()
	{
	}

	void ISmallPoolingItem.OnRelease()
	{
	}

	void IMessage.ReleaseSelf()
	{
		SmallPooling<BuffIconMessage>.ReleaseInstance(this);
	}

	public void Open(EEventType eventType, long creatureUid, string iconCode, string customCode)
	{
		EventType = eventType;
		CreatureUid = creatureUid;
		IconCode = iconCode;
		CustomCode = customCode;
	}
}
