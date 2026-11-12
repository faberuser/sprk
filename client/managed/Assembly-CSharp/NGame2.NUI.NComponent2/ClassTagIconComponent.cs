using System;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ClassTagIconComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClick(ClassTagIconComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/Icon/ClassTagIconComponent";

	public static PrefabAuto<ClassTagIconComponent> PrefabAuto = new PrefabAuto<ClassTagIconComponent>("Prefab2/Component/Icon/ClassTagIconComponent");

	public UISprite IconSprite;

	public UILabel Label_Desc;

	public CreatureTagType TagType;

	private Listener _listener;

	private bool _AllClassTag;

	public void Open(CreatureTagType tagType, Listener listener, bool ShowDesc = false)
	{
		_AllClassTag = false;
		TagType = tagType;
		_listener = listener;
		Refresh();
		UIHelper.EnableGameObject(Label_Desc, ShowDesc);
		if (ShowDesc)
		{
			SetupDesc();
		}
	}

	public void OpenAllClassTag(Listener listener, bool ShowDesc = false)
	{
		_AllClassTag = true;
		_listener = listener;
		Refresh();
		UIHelper.EnableGameObject(Label_Desc, ShowDesc);
		if (ShowDesc)
		{
			SetupDesc();
		}
	}

	public void Refresh()
	{
		InitIconSprite();
	}

	private void InitIconSprite()
	{
		if ((bool)IconSprite)
		{
			if (!_AllClassTag)
			{
				IconSprite.spriteName = TagType.ToString();
			}
			else
			{
				IconSprite.spriteName = "All";
			}
		}
	}

	private void SetupDesc()
	{
		UIHelper.SetLabelText(text: LocalizationManager.GetString((Enum)(_AllClassTag ? ((object)HeroClass.CLASS_TAG_ALL) : ((object)TagType))), label: Label_Desc);
	}

	public void OnClickIcon()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}
}
