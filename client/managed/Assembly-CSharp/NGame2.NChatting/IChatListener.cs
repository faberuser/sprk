using NGame2.NUI.NWindow.NChatting;
using NShared;

namespace NGame2.NChatting;

public interface IChatListener
{
	void OnChatAdded(ChatInfo chatInfo, bool showPreview, EmotiType emoticon);

	void OnWhisperUserAdded(long accountId, string userName);

	void OnWhisperUserRemoved(long accountId);
}
