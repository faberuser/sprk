using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NDescription;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class SkillAllUpgradeMessagePopup : BaseWindow
{
	[Serializable]
	public class SkillMaxLevelUpComponent
	{
		public UIWidget skillWidget;

		public UILabel skillLevelLabel1;

		public UILabel skillLevelLabel2;

		public UILabel skillNameLabel;

		public UITexture skillIconTexture1;

		public UITexture skillIconTexture2;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/SkillAllUpgradeMessagePopup";

	private Action<int[], int[]> _onConfirm;

	public UIButton ConfirmButton;

	public UILabel priceLabel;

	public SkillMaxLevelUpComponent[] skillComponents;

	public const int numSkill = 4;

	private List<int> skillIndices = new List<int>();

	private List<int> skillTargets = new List<int>();

	public static void Open(HeroInfo heroInfo, Action<int[], int[]> onConfirm)
	{
		WindowPrefab<SkillAllUpgradeMessagePopup> windowPrefab = new WindowPrefab<SkillAllUpgradeMessagePopup>("Resources/Prefab2/Window/MessagePopup/SkillAllUpgradeMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(heroInfo, onConfirm);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(HeroInfo heroInfo, Action<int[], int[]> onConfirm)
	{
		base.Open();
		_onConfirm = onConfirm;
		Helper.SkillBasicInfo[] array = new Helper.SkillBasicInfo[4];
		Helper.SkillDetailInfo[] array2 = new Helper.SkillDetailInfo[4];
		int[] array3 = new int[4];
		int[] array4 = new int[4];
		for (int i = 0; i < 4; i++)
		{
			array[i] = Helper.GetSkillBasicInfo(heroInfo, i + 1);
			array2[i] = Helper.GetSkillDetailInfo(heroInfo, i + 1);
			if (array2[i].BasicInfo.TableData == null)
			{
				return;
			}
			AssetTexture skillIcon = UIHelper.GetSkillIcon2(array2[i].BasicInfo.TableIndex, heroInfo);
			UIHelper.ResetPlaneAssetTextureAsync(this, skillComponents[i].skillIconTexture1, skillIcon);
			AssetTexture skillIcon2 = UIHelper.GetSkillIcon2(array2[i].BasicInfo.TableIndex, heroInfo);
			UIHelper.ResetPlaneAssetTextureAsync(this, skillComponents[i].skillIconTexture2, skillIcon2);
			array3[i] = array[i].Level;
			array4[i] = array3[i];
		}
		int num = 0;
		while (true)
		{
			int num2 = 0;
			for (int j = 1; j < 4; j++)
			{
				if (!array2[j].BasicInfo.IsLocked && array4[j] < array4[num2])
				{
					num2 = j;
				}
			}
			int num3 = array4[num2] + 1;
			if (num3 > heroInfo.Level)
			{
				break;
			}
			SkillPriceData data = GameRoot.Instance.SkillPriceDataContainer.GetData(new SkillPriceKey(num2 + 1, num3 - 1));
			if (data == null || num + data.ReqGold > GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold)
			{
				break;
			}
			num += data.ReqGold;
			array4[num2] = num3;
		}
		bool enable = false;
		for (int k = 0; k < 4; k++)
		{
			if (array3[k] < array4[k])
			{
				enable = true;
				break;
			}
		}
		UIHelper.EnableButton(ConfirmButton, enable);
		UIHelper.SetLabelText(priceLabel, num);
		for (int l = 0; l < 4; l++)
		{
			if (array2[l].BasicInfo.IsLocked)
			{
				skillComponents[l].skillWidget.isGrayscale = true;
				UIHelper.EnableGameObject(skillComponents[l].skillLevelLabel1, enable: false);
				UIHelper.EnableGameObject(skillComponents[l].skillLevelLabel2, enable: false);
			}
			else
			{
				skillComponents[l].skillWidget.isGrayscale = false;
				UIHelper.EnableGameObject(skillComponents[l].skillLevelLabel1, enable: true);
				UIHelper.EnableGameObject(skillComponents[l].skillLevelLabel2, enable: true);
				UIHelper.SetLabelText(skillComponents[l].skillLevelLabel1, "Lv " + array3[l]);
				UIHelper.SetLabelText(skillComponents[l].skillLevelLabel2, "Lv " + array4[l]);
			}
			string skillName = SkillDescContainer.GetSkillName(heroInfo, array[l].TableData);
			UIHelper.SetLabelText(skillComponents[l].skillNameLabel, skillName);
		}
		skillIndices.Clear();
		skillTargets.Clear();
		for (int m = 0; m < 4; m++)
		{
			if (!array2[m].BasicInfo.IsLocked && array3[m] < array4[m])
			{
				skillIndices.Add(m + 1);
				skillTargets.Add(array4[m]);
			}
		}
	}

	public void ClickCancel()
	{
		Close();
	}

	public override void Reset()
	{
		skillIndices.Clear();
		skillTargets.Clear();
		base.Reset();
	}

	public void ClickConfirm()
	{
		Action<int[], int[]> onConfirm = _onConfirm;
		Close();
		Vespa_System_Action_Extension_Method.fire(onConfirm, skillIndices.ToArray(), skillTargets.ToArray());
	}
}
