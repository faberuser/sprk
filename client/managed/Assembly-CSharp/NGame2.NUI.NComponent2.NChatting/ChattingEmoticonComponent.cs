using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NChatting;

public class ChattingEmoticonComponent : MonoBehaviour, InitOnCreate
{
	public interface Listener
	{
		void OnClickEmoticon(ChattingEmoticonComponent component);
	}

	public const string PrefabPath = "Prefab2/Component/Chatting/ChattingEmoticonComponent";

	public static PrefabAuto<ChattingEmoticonComponent> PrefabAuto = new PrefabAuto<ChattingEmoticonComponent>("Prefab2/Component/Chatting/ChattingEmoticonComponent");

	public UIBundleLoader EmoticonLoader;

	private Listener _listener;

	public EmoticonData EmoticonData { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open(Listener listener, EmoticonData emoticonData)
	{
		if (emoticonData != null)
		{
			_listener = listener;
			EmoticonData = emoticonData;
			string assetPath = emoticonData.AssetPath;
			UIHelper.SetBundleTexture(EmoticonLoader, assetPath);
		}
	}

	public void OnClickEmoticon()
	{
		if (_listener != null)
		{
			_listener.OnClickEmoticon(this);
		}
	}
}
