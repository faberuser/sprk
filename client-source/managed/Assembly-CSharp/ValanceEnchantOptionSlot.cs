using NGame2.NUI;
using NVespa.NPrefab2;
using UnityEngine;

public class ValanceEnchantOptionSlot : MonoBehaviour
{
	public enum ValanceEnchantSlotState
	{
		Fix,
		Preview,
		Active,
		None
	}

	public UILabel labelOptionDesc;

	public UILabel labelOptionStat;

	public Animator anim;

	private ValanceEnchantSlotState curState = ValanceEnchantSlotState.None;

	public const string PrefabPath = "Resources/Prefab2/Window/Valance/ValanceEnchantOptionSlot";

	public static PrefabAuto<ValanceEnchantOptionSlot> PrefabAuto = new PrefabAuto<ValanceEnchantOptionSlot>("Resources/Prefab2/Window/Valance/ValanceEnchantOptionSlot");

	public void Init(string str, ValanceEnchantSlotState animState)
	{
		Init(str, string.Empty, animState);
	}

	public void Init(string optionDesc, string step, ValanceEnchantSlotState animState)
	{
		UIHelper.SetLabelText(labelOptionDesc, optionDesc);
		UIHelper.SetLabelText(labelOptionStat, step);
		curState = animState;
		PlayAnimation(curState);
	}

	private void OnEnable()
	{
		if (curState != ValanceEnchantSlotState.None)
		{
			PlayAnimation(curState);
		}
	}

	public void PlayAnimation(ValanceEnchantSlotState state)
	{
		switch (state)
		{
		case ValanceEnchantSlotState.Fix:
			anim.SetInteger("state", 1);
			break;
		case ValanceEnchantSlotState.Preview:
			anim.SetInteger("state", 2);
			break;
		case ValanceEnchantSlotState.Active:
			anim.SetInteger("state", 3);
			break;
		}
	}
}
