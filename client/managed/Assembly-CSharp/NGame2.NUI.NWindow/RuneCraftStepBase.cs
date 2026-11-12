using NVespa.NNGUIExtension;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RuneCraftStepBase : MonoBehaviour
{
	[Header("[RuneCraftStepBase]")]
	public PunishmentRuneCraftWindow OwnerWindow;

	public AnimationClip AnimBeginLeft;

	public AnimationClip AnimBeginRight;

	public AnimationClip AnimEndLeft;

	public AnimationClip AnimEndRight;

	public UIPlayAssetSound NextStepSound;

	protected readonly string _gaugePropertyName = "_Gauge_Vaule";

	public virtual void Init()
	{
		UIHelper.EnableGameObject(this, enable: true);
	}

	public virtual void Close()
	{
		UIHelper.EnableGameObject(this, enable: false);
	}

	public void PlayNextStepSound()
	{
		NextStepSound.Play();
	}
}
