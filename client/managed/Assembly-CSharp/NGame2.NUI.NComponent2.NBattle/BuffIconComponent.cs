using NGame2.NBattleField;
using NGame2.NBattleView;
using NGame2.NMessage;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle;

public class BuffIconComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Battle/BuffIconComponent";

	public static PrefabAuto<BuffIconComponent> PrefabAuto = new PrefabAuto<BuffIconComponent>("Prefab2/Component/Battle/BuffIconComponent");

	public UIBundleLoader IconBundleTex;

	public UISprite CooltimeSprite;

	public UILabel CountLabel;

	public string IconCode { get; private set; }

	public string StateType { get; private set; }

	public int CreatureUid { get; private set; }

	public int StackCount { get; private set; }

	public long OperationUid { get; private set; }

	public int ElapsedTick { get; private set; }

	public int DurationMs { get; private set; }

	public StateStackDurationRuleType StackDurationRuleType { get; private set; }

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		UIHelper.EnableGameObject(CountLabel, enable: false);
		UIHelper.EnableGameObject(CooltimeSprite, enable: false);
	}

	public void Open(ChangeState callback)
	{
		StateType = callback.ChangedStateType;
		CreatureUid = callback.CreatureUid;
		StackCount = callback.StackCount;
		OperationUid = callback.OperationUid;
		StateData data = GameRoot.Instance.StateDataContainer.GetData(callback.ChangedStateType);
		OperationRuntime operationRuntime = SceneAutoBehaviour<BattleCallbackManager>.instance.GetOperationRuntime(OperationUid);
		if (data != null && operationRuntime != null)
		{
			string iconPath = data.IconPath;
			HeroInfo heroInfo = GetHeroInfo(operationRuntime);
			UIHelper.SetBundleTexture(assetPath: (!String_Extension_Method.IsNullOrEmpty(iconPath)) ? UIHelper.GetBuffIconPath(iconPath, heroInfo) : UIHelper.GetSkillIconPath(operationRuntime.FiredSkillData.Index, heroInfo), bundleTex: IconBundleTex);
			UIHelper.EnableGameObject(CountLabel, StackCount > 1);
			if (StackCount > 1)
			{
				UIHelper.SetLabelText(CountLabel, StackCount);
			}
			SetCooltime(0f);
			ElapsedTick = 0;
			StackDurationRuleType = data.StackDurationRuleType;
			BeginOperation beginOperation = operationRuntime.GetBeginOperation(CreatureUid);
			if (beginOperation != null)
			{
				DurationMs = beginOperation.DurationMs;
			}
		}
	}

	public void ChainUpdate()
	{
		if (CooltimeSprite == null)
		{
			return;
		}
		OperationRuntime operationRuntime = SceneAutoBehaviour<BattleCallbackManager>.instance.GetOperationRuntime(OperationUid);
		if (operationRuntime == null)
		{
			SetCooltime(1f);
			return;
		}
		BeginOperation beginOperation = operationRuntime.GetBeginOperation(CreatureUid);
		if (beginOperation == null)
		{
			SetCooltime(0f);
			return;
		}
		bool num = SingletonAuto<BattleFacade>.instance.IsBattleEnabled();
		int durationMs = beginOperation.DurationMs;
		ILocalTimer operationTimer = operationRuntime.OperationTimer;
		int num2 = (num ? (operationTimer?.deltaTick ?? 0) : 0);
		ElapsedTick += num2;
		DurationMs = durationMs;
		float num3 = Mathf.Clamp01((float)ElapsedTick / (float)durationMs);
		if (Mathf.Abs(CooltimeSprite.fillAmount - num3) > 0.001f)
		{
			SetCooltime(num3);
		}
	}

	private void SetCooltime(float fillAmount)
	{
		UIHelper.EnableGameObject(CooltimeSprite, fillAmount > 0.001f);
		CooltimeSprite.fillAmount = fillAmount;
	}

	private HeroInfo GetHeroInfo(OperationRuntime opRuntime)
	{
		if (opRuntime.FireSkillCallback == null)
		{
			return null;
		}
		return CreatureManager.GetHeroInfoFromCreatureUid(opRuntime.FireSkillCallback.CreatureUid);
	}
}
