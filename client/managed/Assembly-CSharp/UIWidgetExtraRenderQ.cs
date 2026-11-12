using NNguiExtension.NInternal;
using UnityEngine;

public class UIWidgetExtraRenderQ : UIWidget
{
	public class DrawCallWrapper : IDrawCallWrapper
	{
		private UIWidgetExtraRenderQ _widget;

		bool IDrawCallWrapper.isDirty
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		UIDrawCall IDrawCallWrapper.Native => null;

		int IDrawCallWrapper.depthStart => _widget.depth;

		int IDrawCallWrapper.depthEnd => _widget.depth;

		public DrawCallWrapper(UIWidgetExtraRenderQ widget)
		{
			_widget = widget;
		}

		void IDrawCallWrapper.UpdateRenderQueue(UIPanel panel, Vector3 pos, Quaternion rot, Vector3 scale, ref int curRenderQueue)
		{
			curRenderQueue = _widget.UpdateExtraRenderQueue(panel, curRenderQueue, panel.renderQueue == UIPanel.RenderQueue.Explicit);
		}

		void IDrawCallWrapper.SetLayer(int layer)
		{
		}
	}

	public virtual int UpdateExtraRenderQueue(UIPanel panel, int renderQueue, bool isExplicit)
	{
		return renderQueue;
	}
}
