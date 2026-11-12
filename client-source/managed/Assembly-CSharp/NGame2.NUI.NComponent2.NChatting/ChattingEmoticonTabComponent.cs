using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NChatting;

public class ChattingEmoticonTabComponent : MonoBehaviour, InitOnCreate
{
	public interface Listener
	{
		void OnClickEmoticonTab(ChattingEmoticonTabComponent component);
	}

	public const string PrefabPath = "Prefab2/Component/Chatting/ChattingEmoticonTabComponent";

	public static PrefabAuto<ChattingEmoticonTabComponent> PrefabAuto = new PrefabAuto<ChattingEmoticonTabComponent>("Prefab2/Component/Chatting/ChattingEmoticonTabComponent");

	public UIBundleLoader EmoticonLoader;

	private Listener _listener;

	public int Index;

	public bool IsSelect;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open(Listener listener, int index, string assetPath)
	{
		_listener = listener;
		Index = index;
		UIHelper.SetBundleTexture(EmoticonLoader, assetPath);
	}

	public void SetSelect(bool isSelect)
	{
		IsSelect = isSelect;
		EmoticonLoader.UiTex.isGrayscale = !isSelect;
		UIHelper.EnableGameObject(EmoticonLoader, enable: false);
		UIHelper.EnableGameObject(EmoticonLoader, enable: true);
	}

	public void OnClickEmoticonTab()
	{
		if (_listener != null)
		{
			_listener.OnClickEmoticonTab(this);
		}
	}
}
