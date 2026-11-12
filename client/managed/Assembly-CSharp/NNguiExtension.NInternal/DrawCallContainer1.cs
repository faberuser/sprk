using System;
using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NNguiExtension.NInternal;

public class DrawCallContainer1
{
	private class DrawCallFiller : IDisposable
	{
		public class DcInfo
		{
			public UIDrawCall.OnRenderCallback onRender;

			public int applyWidgetCount;
		}

		private Dictionary<UIDrawCall, DcInfo> _dic = new Dictionary<UIDrawCall, DcInfo>();

		void IDisposable.Dispose()
		{
			_dic.Clear();
		}

		public void AddDrawCall(UIDrawCall dc)
		{
			_dic.Add(dc, new DcInfo());
		}

		public void Fill(UIPanel.WidgetConatiner widgets, bool generateNormals)
		{
			using UIPanel.WidgetConatiner.WidgetIterator widgetIterator = widgets.CreateIterator();
			foreach (UIWidget item in (IEnumerable<UIWidget>)widgetIterator)
			{
				if (item == null || item.drawCall == null)
				{
					continue;
				}
				if (!item.isVisible || !item.hasVertices)
				{
					item.drawCall = null;
				}
				else
				{
					if (!_dic.TryGetValue(item.drawCall, out var value))
					{
						continue;
					}
					value.applyWidgetCount++;
					item.WriteToBuffers(item.drawCall.dcData, generateNormals);
					if (item.mOnRender != null)
					{
						if (value.onRender == null)
						{
							value.onRender = item.mOnRender;
							continue;
						}
						DcInfo dcInfo = value;
						dcInfo.onRender = (UIDrawCall.OnRenderCallback)Delegate.Combine(dcInfo.onRender, item.mOnRender);
					}
				}
			}
		}

		public DcInfo GetDcInfo(UIDrawCall dc)
		{
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, dc, null);
		}
	}

	private bool _rebuild;

	private int _rebuildDepthMin;

	private int _rebuildDepthMax;

	private List<IDrawCallWrapper> mDrawCalls = new List<IDrawCallWrapper>();

	private DrawCallFiller _dcFiller = new DrawCallFiller();

	public int Count => mDrawCalls.Count;

	public void RebuildAll()
	{
		_rebuild = true;
		_rebuildDepthMin = int.MinValue;
		_rebuildDepthMax = int.MaxValue;
	}

	private void RebuildDepth(int depthMin, int depthMax)
	{
		if (!_rebuild)
		{
			_rebuild = true;
			_rebuildDepthMin = depthMin - 1;
			_rebuildDepthMax = depthMax + 1;
		}
		else
		{
			_rebuildDepthMin = Mathf.Min(_rebuildDepthMin, depthMin - 1);
			_rebuildDepthMax = Mathf.Max(_rebuildDepthMax, depthMax + 1);
		}
	}

	private bool IsRebuildDepth(int depth)
	{
		if (!_rebuild)
		{
			return false;
		}
		if (_rebuildDepthMin <= depth)
		{
			return depth <= _rebuildDepthMax;
		}
		return false;
	}

	public void SetDirty()
	{
		int count = mDrawCalls.Count;
		for (int i = 0; i < count; i++)
		{
			mDrawCalls[i].isDirty = true;
		}
	}

	public void DestroyAll()
	{
		int count = mDrawCalls.Count;
		for (int i = 0; i < count; i++)
		{
			UIDrawCall native = mDrawCalls[i].Native;
			if (native != null)
			{
				UIDrawCall.Destroy(native);
			}
		}
		mDrawCalls.Clear();
	}

	public int UpdateRenderQueue(UIPanel panel, int startingRenderQueue, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		int curRenderQueue = startingRenderQueue;
		for (int i = 0; i < mDrawCalls.Count; i++)
		{
			mDrawCalls[i].UpdateRenderQueue(panel, pos, rot, scale, ref curRenderQueue);
		}
		return curRenderQueue;
	}

	public void SetLayer(int layer)
	{
		for (int i = 0; i < mDrawCalls.Count; i++)
		{
			mDrawCalls[i].SetLayer(layer);
		}
	}

	public void OnRemoveWidget(UIWidget w)
	{
		if (!(w.drawCall == null))
		{
			int depth = w.depth;
			if (depth == w.drawCall.depthStart || depth == w.drawCall.depthEnd)
			{
				RebuildDepth(w.drawCall.depthStart, w.drawCall.depthEnd);
			}
			w.drawCall.isDirty = true;
			w.drawCall = null;
		}
	}

	public void OnGeometryChanged(UIWidget w)
	{
		if (!IsRebuildDepth(w.depth))
		{
			if (w.drawCall != null)
			{
				w.drawCall.isDirty = true;
			}
			else
			{
				AssignDrawCall(w);
			}
		}
	}

	public void AssignDrawCall(UIWidget w)
	{
		if (!w.isVisible)
		{
			return;
		}
		UIDrawCall uIDrawCall = FindDrawCall(w);
		if (uIDrawCall != null)
		{
			w.drawCall = uIDrawCall;
			uIDrawCall.UpdateDepthRange(w.depth);
			if (w.hasVertices)
			{
				uIDrawCall.isDirty = true;
			}
		}
		else
		{
			RebuildDepth(w.depth, w.depth);
		}
	}

	private UIDrawCall FindDrawCall(UIWidget w)
	{
		Material material = w.material;
		Texture mainTexture = w.mainTexture;
		Shader shader = w.shader;
		UIRect.ERenderKeywords finalRenderKeywords = w.finalRenderKeywords;
		int depth = w.depth;
		for (int i = 0; i < mDrawCalls.Count; i++)
		{
			UIDrawCall native = mDrawCalls[i].Native;
			if (native == null)
			{
				continue;
			}
			int num = int.MinValue;
			if (i > 0)
			{
				num = mDrawCalls[i - 1].depthEnd;
			}
			int num2 = int.MaxValue;
			if (i < mDrawCalls.Count - 1)
			{
				num2 = mDrawCalls[i + 1].depthStart;
			}
			if (num <= depth && depth <= num2)
			{
				if (native.baseMaterial == material && native.mainTexture == mainTexture && native.shader == shader && native.renderKeywords == finalRenderKeywords)
				{
					return native;
				}
			}
			else if (num > depth)
			{
				break;
			}
		}
		return null;
	}

	public void UpdateDrawCalls(UIPanel panel, UIPanel.WidgetConatiner widgets, bool generateNormals)
	{
		if (_rebuild)
		{
			RebuildDrawCalls(panel, widgets, generateNormals);
		}
		using (_dcFiller)
		{
			for (int i = 0; i < mDrawCalls.Count; i++)
			{
				IDrawCallWrapper drawCallWrapper = mDrawCalls[i];
				if (drawCallWrapper.isDirty)
				{
					UIDrawCall native = drawCallWrapper.Native;
					_dcFiller.AddDrawCall(native);
				}
			}
			_dcFiller.Fill(widgets, generateNormals);
			for (int j = 0; j < mDrawCalls.Count; j++)
			{
				IDrawCallWrapper drawCallWrapper2 = mDrawCalls[j];
				if (!drawCallWrapper2.isDirty)
				{
					continue;
				}
				UIDrawCall native2 = drawCallWrapper2.Native;
				if (native2.dcData.IsEmpty)
				{
					UIDrawCall.Destroy(native2);
					mDrawCalls.RemoveAt(j);
					j--;
					continue;
				}
				native2.isDirty = false;
				DrawCallFiller.DcInfo dcInfo = _dcFiller.GetDcInfo(native2);
				if (dcInfo != null)
				{
					native2.UpdateGeometry(dcInfo.applyWidgetCount);
					native2.onRender = dcInfo.onRender;
				}
			}
		}
	}

	private void RebuildDrawCalls(UIPanel panel, UIPanel.WidgetConatiner widgets, bool generateNormals)
	{
		if (!_rebuild)
		{
			return;
		}
		HashSet<UIDrawCall> hashSet = new HashSet<UIDrawCall>();
		List<IDrawCallWrapper> list = new List<IDrawCallWrapper>();
		UIDrawCall.OnRenderCallback onRender = null;
		int i;
		for (i = 0; i < mDrawCalls.Count && mDrawCalls[i].depthEnd < _rebuildDepthMin; i++)
		{
		}
		int num = i;
		for (int j = i; j < mDrawCalls.Count; j++)
		{
			IDrawCallWrapper drawCallWrapper = mDrawCalls[j];
			if (drawCallWrapper.depthStart <= _rebuildDepthMax)
			{
				if (drawCallWrapper.Native != null)
				{
					hashSet.Add(drawCallWrapper.Native);
				}
			}
			else
			{
				list.Add(drawCallWrapper);
			}
		}
		mDrawCalls.RemoveRange(num, mDrawCalls.Count - num);
		Material material = null;
		Texture texture = null;
		Shader shader = null;
		UIRect.ERenderKeywords eRenderKeywords = UIRect.ERenderKeywords.None;
		UIDrawCall uIDrawCall = null;
		int num2 = 0;
		using (UIPanel.WidgetConatiner.WidgetIterator widgetIterator = widgets.CreateIterator())
		{
			foreach (UIWidget item in (IEnumerable<UIWidget>)widgetIterator)
			{
				if (item is UIWidgetExtraRenderQ)
				{
					if (item.depth >= _rebuildDepthMin && item.depth <= _rebuildDepthMax)
					{
						material = null;
						texture = null;
						shader = null;
						eRenderKeywords = UIRect.ERenderKeywords.None;
						AddDrawCall(uIDrawCall, num2, ref onRender);
						num2 = 0;
						uIDrawCall = null;
						mDrawCalls.Add(new UIWidgetExtraRenderQ.DrawCallWrapper(item as UIWidgetExtraRenderQ));
					}
					continue;
				}
				if (!item.isVisible || !item.hasVertices)
				{
					item.drawCall = null;
					continue;
				}
				if (item.drawCall == null)
				{
					if (_rebuildDepthMin > item.depth || item.depth > _rebuildDepthMax)
					{
						continue;
					}
				}
				else
				{
					if (!hashSet.Contains(item.drawCall))
					{
						continue;
					}
					item.drawCall = null;
				}
				Material material2 = item.material;
				Texture mainTexture = item.mainTexture;
				Shader shader2 = item.shader;
				UIRect.ERenderKeywords finalRenderKeywords = item.finalRenderKeywords;
				if (material != material2 || texture != mainTexture || shader != shader2 || eRenderKeywords != finalRenderKeywords)
				{
					AddDrawCall(uIDrawCall, num2, ref onRender);
					num2 = 0;
					uIDrawCall = null;
					material = material2;
					texture = mainTexture;
					shader = shader2;
					eRenderKeywords = finalRenderKeywords;
				}
				if (material == null && shader == null && texture == null)
				{
					continue;
				}
				if (uIDrawCall == null)
				{
					if (true)
					{
						uIDrawCall = UIDrawCall.Create(panel, material, texture, shader, eRenderKeywords);
						uIDrawCall.SetDepthRange(item.depth);
						uIDrawCall.panel = panel;
					}
				}
				else
				{
					if (item.depth < uIDrawCall.depthStart)
					{
						VespaDebug.LogError("--- 이게 발생해? ---");
					}
					uIDrawCall.UpdateDepthRange(item.depth);
				}
				item.drawCall = uIDrawCall;
				if (uIDrawCall != null)
				{
					num2++;
					item.WriteToBuffers(uIDrawCall.dcData, generateNormals);
					if (item.mOnRender != null)
					{
						onRender = ((onRender != null) ? ((UIDrawCall.OnRenderCallback)Delegate.Combine(onRender, item.mOnRender)) : item.mOnRender);
					}
				}
			}
		}
		AddDrawCall(uIDrawCall, num2, ref onRender);
		mDrawCalls.AddRange(list);
		foreach (UIDrawCall item2 in hashSet)
		{
			UIDrawCall.Destroy(item2);
		}
		_rebuild = false;
	}

	private void AddDrawCall(UIDrawCall dc, int widgetCount, ref UIDrawCall.OnRenderCallback onRender)
	{
		if (dc != null && !dc.dcData.IsEmpty)
		{
			dc.dcData.FlushPending();
			mDrawCalls.Add(new UIDrawCallWrapper(dc));
			dc.UpdateGeometry(widgetCount);
			dc.onRender = onRender;
			dc.isDirty = false;
			onRender = null;
		}
	}
}
