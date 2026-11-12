using NGame2.NUI.NWindow.NPet;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class PetJukeboxBGMListComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnClick(JukeboxData jukeboxData);
	}

	public static string PrefabPath = "Prefab2/Component/Pet/PetJukeboxBGMListComponent";

	public static PrefabAuto<PetJukeboxBGMListComponent> PrefabAuto = new PrefabAuto<PetJukeboxBGMListComponent>(PrefabPath);

	private IListener _listener;

	private JukeboxData _jukeboxData;

	private bool _isNew;

	private bool _isNoEvent;

	public UIToggle Toggle_Jukebox;

	public UILabel Label_Title;

	public UILabel Label_SubTitle;

	public UISprite Sprite_New;

	public UIBundleLoader JukeboxIconTex;

	public string JukeboxCode => _jukeboxData.Code;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		SelectToggle(isSelect: false, noEvent: true);
	}

	public void Open(IListener listener, JukeboxData jukeboxData, bool isNew)
	{
		if (listener == null)
		{
			VespaDebug.LogError("listener is null");
			return;
		}
		_listener = listener;
		_jukeboxData = jukeboxData;
		_isNew = isNew;
		UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect(jukeboxData.Title));
		UIHelper.SetLabelText(Label_SubTitle, LocalizationManager.GetStringDirect(jukeboxData.SubTitle));
		UIHelper.EnableGameObject(Sprite_New, isNew);
		UIHelper.SetBundleTexture(JukeboxIconTex, PetJukebox.GetJukeboxPortraitPath(jukeboxData.IconPath));
	}

	public void OnClick()
	{
		if (_listener != null && !_isNoEvent)
		{
			if (_isNew)
			{
				_isNew = false;
				UIHelper.EnableGameObject(Sprite_New, enable: false);
				new LocalAccountBool(i_default: true, $"PetHouse_NewBGM_{_jukeboxData.Code}").Value = false;
			}
			_listener.OnClick(_jukeboxData);
		}
	}

	public void SelectToggle(bool isSelect, bool noEvent = false)
	{
		if (noEvent)
		{
			_isNoEvent = true;
		}
		if (Toggle_Jukebox != null)
		{
			if (!isSelect)
			{
				Toggle_Jukebox.optionCanBeNone = true;
				Toggle_Jukebox.value = isSelect;
				Toggle_Jukebox.optionCanBeNone = false;
			}
			else
			{
				Toggle_Jukebox.value = isSelect;
			}
		}
		if (noEvent)
		{
			_isNoEvent = false;
		}
	}

	public void FixPanelsDepth(int depth)
	{
		UIPanel[] componentsInChildren = GetComponentsInChildren<UIPanel>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].depth += depth;
		}
	}
}
