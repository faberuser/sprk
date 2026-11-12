using UnityEngine;

namespace NVespa.NNGUIExtension;

public class TooltipPrintInfo : MonoBehaviour
{
	public UITooltipTrigger.ETooltipTriggerMode Mode;

	public UIWidget CustomAnchor;

	public UIWidget.Pivot OwnerPivot = UIWidget.Pivot.TopRight;

	public UIWidget.Pivot TooltipPivot;

	public Vector3 Offset;
}
