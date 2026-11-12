using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class RewardPopup : BaseWindow
{
	public class RewardInfo
	{
		public int Gold;

		public int Gem;

		public int Stamina;

		public string CustomTitle;

		public string CustomContent;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/RewardPopup";

	public UIGrid MessageGrid;

	public UIWidget Widget_Gold;

	public UIWidget Widget_Gem;

	public UIWidget Widget_Stamina;

	public UILabel Label_Gold;

	public UILabel Label_Gem;

	public UILabel Label_Stamina;

	public UILabel DefaultTitleLabel;

	public UILabel CustomTitleLabel;

	public UILabel DefaultContentLabel;

	public UILabel CustomContentLabel;

	public static void OpenRewardPopup(RewardInfo info)
	{
		if (info.Gold != 0 || info.Gem != 0 || info.Stamina != 0)
		{
			WindowPrefab<RewardPopup> windowPrefab = new WindowPrefab<RewardPopup>("Resources/Prefab2/Window/MessagePopup/RewardPopup", GameUIRoot.ELayerType.UI);
			windowPrefab.instance.OpenInternal(info);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}
	}

	public override void Reset()
	{
		base.Reset();
		UIHelper.EnableGameObject(Widget_Gold, enable: false);
		UIHelper.EnableGameObject(Widget_Gem, enable: false);
		UIHelper.EnableGameObject(Widget_Stamina, enable: false);
		UIHelper.EnableGameObject(DefaultTitleLabel, enable: false);
		UIHelper.EnableGameObject(CustomTitleLabel, enable: false);
		UIHelper.EnableGameObject(DefaultContentLabel, enable: false);
		UIHelper.EnableGameObject(CustomContentLabel, enable: false);
	}

	protected void OpenInternal(RewardInfo info)
	{
		base.Open();
		SetupRewardMsg(info);
		SetupText(info.CustomTitle, info.CustomContent);
	}

	public void OnClickConfirm()
	{
		Close();
	}

	private void SetupRewardMsg(RewardInfo Info)
	{
		UIHelper.EnableGameObject(Widget_Gold, Info.Gold > 0);
		UIHelper.EnableGameObject(Widget_Gem, Info.Gem > 0);
		UIHelper.EnableGameObject(Widget_Stamina, Info.Stamina > 0);
		UILabel label_Gold = Label_Gold;
		int gold = Info.Gold;
		UIHelper.SetLabelText(label_Gold, Vespa_Int_Extension_Method.ToCommaString(gold));
		UILabel label_Gem = Label_Gem;
		int gem = Info.Gem;
		UIHelper.SetLabelText(label_Gem, Vespa_Int_Extension_Method.ToCommaString(gem));
		UILabel label_Stamina = Label_Stamina;
		int stamina = Info.Stamina;
		UIHelper.SetLabelText(label_Stamina, Vespa_Int_Extension_Method.ToCommaString(stamina));
		MessageGrid.Reposition();
	}

	private void SetupText(string title, string content)
	{
		UIHelper.EnableGameObject(DefaultTitleLabel, string.IsNullOrEmpty(title));
		UIHelper.EnableGameObject(CustomTitleLabel, !string.IsNullOrEmpty(title));
		UIHelper.SetLabelText(CustomTitleLabel, title);
		UIHelper.EnableGameObject(DefaultContentLabel, string.IsNullOrEmpty(content));
		UIHelper.EnableGameObject(CustomContentLabel, !string.IsNullOrEmpty(content));
		UIHelper.SetLabelText(CustomContentLabel, content);
	}
}
