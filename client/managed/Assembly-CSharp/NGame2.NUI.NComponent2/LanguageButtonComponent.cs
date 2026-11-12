using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class LanguageButtonComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClick(LanguageButtonComponent component);
	}

	public const string PrefabPath = "Prefab2/Component/GameOption/LanguageButtonComponent";

	public static PrefabAuto<LanguageButtonComponent> PrefabAuto = new PrefabAuto<LanguageButtonComponent>("Prefab2/Component/GameOption/LanguageButtonComponent");

	public UISprite ImageTexture;

	private Listener _listener;

	public SystemLanguage Language { get; private set; }

	public void Open(SystemLanguage language, Listener listener)
	{
		Language = language;
		_listener = listener;
		UIHelper.EnableGameObject(ImageTexture, enable: true);
		ImageTexture.spriteName = Vespa_String_Extension_Method.GetEnumString(language);
	}

	public void OnClickButton()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}
}
