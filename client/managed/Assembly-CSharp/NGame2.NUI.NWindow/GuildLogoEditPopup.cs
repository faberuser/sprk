using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class GuildLogoEditPopup : BaseWindow, GuildLogoComponent.Listener, GuildBGComponent.Listener
{
	public interface Listener
	{
		void OnClickReset();

		void OnClickApply(int iconIndex, int bgIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Guild/GuildLogoEditPopup";

	private int _flagIndex;

	private int _iconIndex;

	private int _bgIndex;

	public UIScrollGrid GridGuildLogoIcon;

	private List<GuildLogoComponent> _guildLogoIconList = new List<GuildLogoComponent>();

	public UIScrollGrid GridGuildLogoBG;

	private List<GuildBGComponent> _guildLogoBGList = new List<GuildBGComponent>();

	public UIFlagWidget UIFlagWidget;

	private Listener _listener;

	public float guildLogoWidth;

	public float guildBGWidth;

	public void Open(Listener listener, int flagIndex, int iconIndex, int bgIndex)
	{
		base.Open();
		Init(listener, flagIndex, iconIndex, bgIndex);
	}

	public void Init(Listener listener, int flagIndex, int iconIndex, int bgIndex)
	{
		_listener = listener;
		_flagIndex = flagIndex;
		_iconIndex = iconIndex;
		_bgIndex = bgIndex;
		InitGuildIcon();
		InitGuildBG();
		InitUIFlagWidget();
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_guildLogoIconList);
		FactoryBase.ReleaseAndClearList(_guildLogoBGList);
	}

	private void InitGuildIcon()
	{
		for (int i = 1; i <= GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildIcon"); i++)
		{
			GuildLogoComponent guildLogoComponent = UIHelper.AddGridItem(GridGuildLogoIcon, GuildLogoComponent.PrefabAuto);
			guildLogoComponent.Open(i, this);
			_guildLogoIconList.Add(guildLogoComponent);
		}
		UIHelper.RepositionGrid(GridGuildLogoIcon);
		UpdateSelectedIcon();
		GuildLogoComponent comp = _guildLogoIconList.Find((GuildLogoComponent e) => e.IsSelected);
		GridGuildLogoIcon.ScrollTo(comp, guildLogoWidth, 0f);
	}

	private void UpdateSelectedIcon()
	{
		foreach (GuildLogoComponent guildLogoIcon in _guildLogoIconList)
		{
			guildLogoIcon.SetSelected(_iconIndex);
		}
		if (UIFlagWidget != null)
		{
			UIFlagWidget.ChangeLogo((byte)_iconIndex);
		}
	}

	private void InitUIFlagWidget()
	{
		if (UIFlagWidget != null)
		{
			UIFlagWidget.Open((byte)_flagIndex, (byte)_iconIndex, (byte)_bgIndex);
			UIFlagWidget.Play("Loop");
		}
	}

	private void InitGuildBG()
	{
		for (int i = 1; i <= GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"); i++)
		{
			GuildBGComponent guildBGComponent = UIHelper.AddGridItem(GridGuildLogoBG, GuildBGComponent.PrefabAuto);
			guildBGComponent.Open(i, this);
			_guildLogoBGList.Add(guildBGComponent);
		}
		UIHelper.RepositionGrid(GridGuildLogoBG);
		UpdateSelectedBG();
		GuildBGComponent comp = _guildLogoBGList.Find((GuildBGComponent e) => e.IsSelected);
		GridGuildLogoBG.ScrollTo(comp, guildBGWidth, 0f);
	}

	private void UpdateSelectedBG()
	{
		foreach (GuildBGComponent guildLogoBG in _guildLogoBGList)
		{
			guildLogoBG.SetSelected(_bgIndex);
		}
		if (UIFlagWidget != null)
		{
			UIFlagWidget.ChangeBG((byte)_bgIndex);
		}
	}

	public void OnClickExitButton()
	{
		Close();
	}

	public void OnClickApply()
	{
		_listener.OnClickApply(_iconIndex, _bgIndex);
		Close();
	}

	public void OnClickReset()
	{
		_iconIndex = 1;
		_bgIndex = 1;
		InitGuildIcon();
		InitGuildBG();
	}

	void GuildLogoComponent.Listener.OnClickGuildLogoComponent(int LogoIndex)
	{
		_iconIndex = LogoIndex;
		UpdateSelectedIcon();
	}

	void GuildBGComponent.Listener.OnClickGuildBGComponent(int index)
	{
		_bgIndex = index;
		UpdateSelectedBG();
	}
}
