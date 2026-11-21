"""Build a paper-cut-out head accessory (like Haphazardly Drawn Mask of Goblin) from a PNG.

The goblin mask (Accessory.Head.Head.Goblinmask) is a thin curved sheet cut along the
drawing's outline with a white paper border: a front face that shows the drawing, a white
back and dark side walls. This script builds the same kind of model from any image with a
transparent background, sized and curved like the goblin mask so it sits on heroes' faces
at the goblin's pivot (docs/accessory-masks.md).

Outputs (named for <name>, e.g. Xdd):
  <managed>/SprkAccessories/SprkMasks.<name>.png    texture (front drawing, back, sides)
  <managed>/SprkAccessories/SprkMasks.<name>.mesh   mesh (format below)
  <managed>/SprkImageRecovery/SprkIcons.Item.<item code>.png       124x124 item icon
  <managed>/SprkImageRecovery/SprkPortraits.<portrait>.png         330x234 Dressing Room image

Mesh format, little endian: b"SPKM", int32 version (1), int32 vertex count, int32 index
count, then per vertex 8 float32 (position xyz, normal xyz, uv), then int32 indices.
Unity's mesh space (left-handed): the face looks toward -Y, +Z is up.

Usage: python make_paper_mask.py <source.png> <name> <item code> <portrait name> [--goblin <Effect_Accessory_Head_Head_Goblinmask.unity3d>]
--goblin only verifies the geometry constants below against the original mesh (needs UnityPy).
"""
import os
import struct
import sys

import numpy as np
from PIL import Image, ImageFilter

# Goblin mask geometry (measured from Effect_Accessory_Head_Head_Goblinmask, all specs alike).
WIDTH = 0.3445          # x extent of the front face
MAX_HEIGHT = 0.30       # z extent of the whole mask
CENTER_Z = -0.0894      # middle of the z extent
CURVE_A, CURVE_C = 3.41, -0.1306   # front surface y = A*x^2 + C (wraps around the face)
THICKNESS = 0.0091      # back surface = front + THICKNESS in y

TEX = 512
FRONT_TOP = 0           # texture rows (PIL, top = 0) used by the front drawing
FRONT_BOTTOM = 400
BACK_UV = (0.15, 0.10)  # inside the white back patch
SIDE_UV = (0.85, 0.10)  # inside the dark side patch
PAPER = (238, 236, 230)
SIDE = (52, 48, 46)


def load_mask(path, size):
    """Source image scaled into a size x size canvas with padding, plus its alpha mask."""
    src = Image.open(path).convert("RGBA")
    w, h = src.size
    pad = 0.14   # room for the paper border
    scale = size * (1 - 2 * pad) / max(w, h)
    big = src.resize((max(1, round(w * scale)), max(1, round(h * scale))), Image.LANCZOS)
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    canvas.paste(big, ((size - big.width) // 2, (size - big.height) // 2))
    return canvas


def paper(canvas, border):
    """Drawing on a white paper cut-out: alpha dilated by `border` pixels, then smoothed."""
    alpha = canvas.getchannel("A")
    solid = alpha.point(lambda v: 255 if v >= 96 else 0)
    cut = solid.filter(ImageFilter.MaxFilter(border * 2 + 1))
    cut = cut.filter(ImageFilter.GaussianBlur(border * 0.6)).point(lambda v: 255 if v >= 128 else 0)
    sheet = Image.new("RGBA", canvas.size, PAPER + (255,))
    sheet.alpha_composite(canvas)
    return sheet, np.asarray(cut) > 0


def trace(mask):
    """Outer boundary of the largest blob as a list of (x, y) pixel-corner points (CCW in image space)."""
    # Keep only the largest connected blob, with holes filled (the cut-out is solid paper).
    h, w = mask.shape
    seen = np.zeros_like(mask, bool)
    best = None
    for sy, sx in zip(*np.nonzero(mask)):
        if seen[sy, sx]:
            continue
        stack, blob = [(sy, sx)], []
        seen[sy, sx] = True
        while stack:
            y, x = stack.pop()
            blob.append((y, x))
            for ny, nx in ((y + 1, x), (y - 1, x), (y, x + 1), (y, x - 1)):
                if 0 <= ny < h and 0 <= nx < w and mask[ny, nx] and not seen[ny, nx]:
                    seen[ny, nx] = True
                    stack.append((ny, nx))
        if best is None or len(blob) > len(best):
            best = blob
    m = np.zeros((h + 2, w + 2), bool)
    for y, x in best:
        m[y + 1, x + 1] = True
    # Directed boundary edges between pixel corners, inside pixel on the right (image space).
    nxt = {}
    for y, x in zip(*np.nonzero(m)):
        if not m[y - 1, x]:
            nxt.setdefault((x, y), []).append((x + 1, y))          # top edge, walking right
        if not m[y, x + 1]:
            nxt.setdefault((x + 1, y), []).append((x + 1, y + 1))  # right edge, walking down
        if not m[y + 1, x]:
            nxt.setdefault((x + 1, y + 1), []).append((x, y + 1))  # bottom edge, walking left
        if not m[y, x - 1]:
            nxt.setdefault((x, y + 1), []).append((x, y))          # left edge, walking up
    # The outer boundary passes the top-most, left-most corner.
    start = min(nxt, key=lambda p: (p[1], p[0]))
    pts, cur = [], start
    while True:
        pts.append(cur)
        cur = nxt[cur].pop(0)
        if cur == start:
            break
    return np.array(pts, dtype=np.float64) - 1.0


def simplify(pts, tol):
    """Douglas-Peucker on a closed polygon."""
    def dp(p):
        if len(p) < 3:
            return p
        a, b = p[0], p[-1]
        ab = b - a
        n = np.hypot(*ab)
        if n == 0:
            dist = np.hypot(*(p - a).T)
        else:
            dist = np.abs(ab[0] * (p[:, 1] - a[1]) - ab[1] * (p[:, 0] - a[0])) / n
        i = int(np.argmax(dist))
        if dist[i] > tol:
            return np.vstack([dp(p[: i + 1])[:-1], dp(p[i:])])
        return np.array([a, b])
    far = int(np.argmax(np.hypot(*(pts - pts[0]).T)))
    first = dp(np.vstack([pts[: far + 1]]))
    second = dp(np.vstack([pts[far:], pts[:1]]))
    return np.vstack([first[:-1], second[:-1]])


def resample(poly, step):
    """Evenly spaced points along a closed polygon (keeps corners)."""
    out = []
    for i in range(len(poly)):
        a, b = poly[i], poly[(i + 1) % len(poly)]
        n = max(1, int(np.ceil(np.hypot(*(b - a)) / step)))
        for k in range(n):
            out.append(a + (b - a) * k / n)
    return np.array(out)


def area(poly):
    x, y = poly[:, 0], poly[:, 1]
    return 0.5 * np.sum(x * np.roll(y, -1) - np.roll(x, -1) * y)


def earclip(poly):
    """Triangulate a simple polygon (counter-clockwise in a y-up frame)."""
    idx = list(range(len(poly)))
    tris = []

    def inside(p, a, b, c):
        def s(p1, p2, p3):
            return (p1[0] - p3[0]) * (p2[1] - p3[1]) - (p2[0] - p3[0]) * (p1[1] - p3[1])
        d1, d2, d3 = s(p, a, b), s(p, b, c), s(p, c, a)
        return not ((d1 < 0 or d2 < 0 or d3 < 0) and (d1 > 0 or d2 > 0 or d3 > 0))

    guard = 0
    while len(idx) > 3:
        guard += 1
        if guard > 100000:
            raise RuntimeError("ear clipping failed")
        best = None
        for k in range(len(idx)):
            i0, i1, i2 = idx[k - 1], idx[k], idx[(k + 1) % len(idx)]
            a, b, c = poly[i0], poly[i1], poly[i2]
            cross = (b[0] - a[0]) * (c[1] - a[1]) - (b[1] - a[1]) * (c[0] - a[0])
            if cross <= 1e-12:
                continue
            if any(inside(poly[j], a, b, c) for j in idx if j not in (i0, i1, i2)):
                continue
            # prefer well-shaped ears
            e = [np.hypot(*(b - a)), np.hypot(*(c - b)), np.hypot(*(a - c))]
            quality = cross / max(e) ** 2
            if best is None or quality > best[0]:
                best = (quality, k)
        if best is None:
            raise RuntimeError("polygon is not simple")
        k = best[1]
        tris.append((idx[k - 1], idx[k], idx[(k + 1) % len(idx)]))
        del idx[k]
    tris.append(tuple(idx))
    return tris


def refine(poly, tris, spacing):
    """Insert interior grid points and make the mesh Delaunay (boundary edges kept).

    The toon shader lights and outlines per vertex, so long slivers from ear clipping show as
    streaks; evenly sized triangles shade like the original mask. The boundary is not split, so
    the side walls meet the faces without T-junctions.
    """
    pts = [tuple(p) for p in poly]
    n = len(pts)
    boundary = {tuple(sorted((i, (i + 1) % n))) for i in range(n)}

    def orient(a, b, c):
        return (pts[b][0] - pts[a][0]) * (pts[c][1] - pts[a][1]) - (pts[b][1] - pts[a][1]) * (pts[c][0] - pts[a][0])

    tris = [t if orient(*t) > 0 else (t[0], t[2], t[1]) for t in tris]

    def in_circle(a, b, c, d):
        ax, ay = pts[a][0] - pts[d][0], pts[a][1] - pts[d][1]
        bx, by = pts[b][0] - pts[d][0], pts[b][1] - pts[d][1]
        cx, cy = pts[c][0] - pts[d][0], pts[c][1] - pts[d][1]
        return (ax * ax + ay * ay) * (bx * cy - cx * by) - (bx * bx + by * by) * (ax * cy - cx * ay) \
            + (cx * cx + cy * cy) * (ax * by - bx * ay) > 1e-9

    def legalize(tris):
        changed = True
        while changed:
            changed = False
            owner = {}
            for k, t in enumerate(tris):
                for e in ((t[0], t[1]), (t[1], t[2]), (t[2], t[0])):
                    owner.setdefault(tuple(sorted(e)), []).append(k)
            done = set()
            for e, ks in owner.items():
                if len(ks) != 2 or e in boundary or ks[0] in done or ks[1] in done:
                    continue
                t1, t2 = tris[ks[0]], tris[ks[1]]
                a = next(v for v in t1 if v not in e)
                b = next(v for v in t2 if v not in e)
                # t1 = (e0, e1, a) counter-clockwise; flip when b lies in t1's circumcircle
                i = t1.index(a)
                p, q = t1[(i + 1) % 3], t1[(i + 2) % 3]
                if not in_circle(p, q, a, b):
                    continue
                n1, n2 = (a, p, b), (b, q, a)
                if orient(*n1) <= 1e-9 or orient(*n2) <= 1e-9:
                    continue
                tris[ks[0]], tris[ks[1]] = n1, n2
                done.update(ks)
                changed = True
        return tris

    tris = legalize(tris)
    P = np.array(pts)
    x0, y0 = P.min(0)
    x1, y1 = P.max(0)
    seg_a, seg_b = P, np.roll(P, -1, 0)

    def dist_to_boundary(p):
        d = seg_b - seg_a
        t = np.clip(((p - seg_a) * d).sum(1) / np.maximum((d * d).sum(1), 1e-12), 0, 1)
        return np.min(np.hypot(*(seg_a + d * t[:, None] - p).T))

    for row, y in enumerate(np.arange(y0 + spacing / 2, y1, spacing * 0.866)):
        offset = spacing / 2 if row % 2 else 0.0
        for x in np.arange(x0 + offset, x1, spacing):
            p = np.array([x, y])
            if dist_to_boundary(p) < spacing * 0.55:
                continue
            pts.append((x, y))
            v = len(pts) - 1
            for k, t in enumerate(tris):
                if orient(t[0], t[1], v) > 1e-9 and orient(t[1], t[2], v) > 1e-9 and orient(t[2], t[0], v) > 1e-9:
                    a, b, c = t
                    tris[k] = (a, b, v)
                    tris += [(b, c, v), (c, a, v)]
                    break
            else:
                pts.pop()     # outside the polygon (concave part)
        tris = legalize(tris)
    return np.array(pts), tris


def build(source, name, item_code, portrait, managed):
    # --- texture --------------------------------------------------------------------
    front_h = FRONT_BOTTOM - FRONT_TOP
    canvas = load_mask(source, front_h)
    border = max(3, round(front_h * 0.028))
    sheet, cut = paper(canvas, border)
    # Dark rim outside the paper, like the goblin mask's drawn outline (the mesh includes it).
    rim_px = max(2, round(border * 0.7))
    rim = np.asarray(Image.fromarray((cut * 255).astype(np.uint8)).filter(ImageFilter.MaxFilter(rim_px * 2 + 1))) > 0
    front = np.array(sheet.convert("RGB"))
    front[~cut] = SIDE    # the rim, plus bleed so edge sampling never picks up white
    tex = Image.new("RGBA", (TEX, TEX), PAPER + (255,))
    ox = (TEX - front_h) // 2
    tex.paste(Image.fromarray(front).convert("RGBA"), (ox, FRONT_TOP))
    draw_side = Image.new("RGBA", (TEX // 2 - 16, TEX - FRONT_BOTTOM - 8), SIDE + (255,))
    tex.paste(draw_side, (TEX // 2 + 16, FRONT_BOTTOM + 8))
    tex = tex.convert("RGB")

    # --- outline in front-canvas pixels -> mesh space --------------------------------
    outline = trace(rim)
    outline = simplify(outline, 1.2)
    outline = resample(outline, front_h / 40)
    # mesh frame: x to the hero's right... the goblin maps texture u = 0.5585 - 2.42 x,
    # so image left is +x. z up = image up.
    px_w = outline[:, 0].max() - outline[:, 0].min()
    px_h = outline[:, 1].max() - outline[:, 1].min()
    scale = min(WIDTH / px_w, MAX_HEIGHT / px_h)
    cx = (outline[:, 0].max() + outline[:, 0].min()) / 2
    cy = (outline[:, 1].max() + outline[:, 1].min()) / 2

    def to_mesh(p):
        return np.stack([-(p[:, 0] - cx) * scale, -(p[:, 1] - cy) * scale + CENTER_Z], 1)

    def to_uv(p):
        u = (p[:, 0] + ox) / TEX
        v = 1.0 - (p[:, 1] + FRONT_TOP) / TEX
        return np.stack([u, v], 1)

    xz = to_mesh(outline)
    if area(xz) < 0:          # ear clipping wants counter-clockwise in (x, z)
        xz, outline = xz[::-1].copy(), outline[::-1].copy()
    tris = earclip(xz)
    flat_px, tris = refine(outline, tris, front_h / 40)
    flat = to_mesh(flat_px)
    uv = to_uv(flat_px)

    def surface(x):
        return CURVE_A * x * x + CURVE_C

    verts, norms, uvs, index = [], [], [], []

    def emit(p, n, t):
        verts.append(p)
        norms.append(n / np.linalg.norm(n))
        uvs.append(t)
        return len(verts) - 1

    def face(ids, want):
        a, b, c = (np.array(verts[i]) for i in ids)
        # Unity: front faces are clockwise as seen by the viewer -> Cross(b-a, c-a) points at the viewer.
        if np.dot(np.cross(b - a, c - a), want) < 0:
            ids = (ids[0], ids[2], ids[1])
        index.extend(ids)

    n_front = len(flat)
    front_ids, back_ids = [], []
    for (x, z), t in zip(flat, uv):
        y = surface(x)
        slope = 2 * CURVE_A * x
        front_ids.append(emit((x, y, z), np.array([slope, -1.0, 0.0]), t))
    for (x, z) in flat:
        y = surface(x) + THICKNESS
        slope = 2 * CURVE_A * x
        back_ids.append(emit((x, y, z), np.array([-slope, 1.0, 0.0]), BACK_UV))
    for a, b, c in tris:
        fa, fb, fc = front_ids[a], front_ids[b], front_ids[c]
        face((fa, fb, fc), np.array(norms[fa]) + np.array(norms[fb]) + np.array(norms[fc]))
        ba, bb, bc = back_ids[a], back_ids[b], back_ids[c]
        face((ba, bb, bc), np.array(norms[ba]) + np.array(norms[bb]) + np.array(norms[bc]))
    # Side walls along the outline (the first len(outline) points are the boundary).
    m = len(outline)
    centroid = flat[:m].mean(0)
    for i in range(m):
        j = (i + 1) % m
        p0, p1 = flat[i], flat[j]
        edge = p1 - p0
        out = np.array([edge[1], -edge[0]])
        if np.dot(out, (p0 + p1) / 2 - centroid) < 0:
            out = -out
        n = np.array([out[0], 0.0, out[1]])
        q = []
        for (x, z) in (p0, p1):
            y = surface(x)
            q.append(emit((x, y, z), n, SIDE_UV))
            q.append(emit((x, y + THICKNESS, z), n, SIDE_UV))
        face((q[0], q[2], q[1]), n)
        face((q[1], q[2], q[3]), n)

    verts = np.array(verts, np.float32)
    norms = np.array(norms, np.float32)
    uvs = np.array(uvs, np.float32)

    acc = os.path.join(managed, "SprkAccessories")
    tex.save(os.path.join(acc, f"SprkMasks.{name}.png"), optimize=True)
    with open(os.path.join(acc, f"SprkMasks.{name}.mesh"), "wb") as f:
        f.write(b"SPKM")
        f.write(struct.pack("<iii", 1, len(verts), len(index)))
        f.write(np.concatenate([verts, norms, uvs], 1).astype("<f4").tobytes())
        f.write(np.array(index, "<i4").tobytes())

    # --- UI images ---------------------------------------------------------------------
    cutout = sheet.copy()
    cutout.putalpha(Image.fromarray((cut * 255).astype(np.uint8)))
    # dark outline like the goblin's Dressing Room image
    edge = Image.fromarray((cut * 255).astype(np.uint8)).filter(ImageFilter.MaxFilter(border * 2 + 1))
    framed = Image.new("RGBA", cutout.size, SIDE + (0,))
    framed.putalpha(edge)
    framed.alpha_composite(cutout)
    bbox = framed.getbbox()
    framed = framed.crop(bbox)

    def fit(img, w, h, margin):
        s = min((w - 2 * margin) / img.width, (h - 2 * margin) / img.height)
        r = img.resize((max(1, round(img.width * s)), max(1, round(img.height * s))), Image.LANCZOS)
        out = Image.new("RGBA", (w, h), (0, 0, 0, 0))
        out.alpha_composite(r, ((w - r.width) // 2, (h - r.height) // 2))
        return out

    rec = os.path.join(managed, "SprkImageRecovery")
    fit(framed, 330, 234, 8).save(os.path.join(rec, f"SprkPortraits.{portrait}.png"), optimize=True)

    # Item icon: the grade 5 (orange) glow used by accessory item icons, with the mask on top.
    yy, xx = np.mgrid[0:124, 0:124].astype(np.float32)
    r = np.hypot(xx - 61.5, yy - 61.5) / 87.0
    t = np.clip(r, 0, 1)[..., None]
    inner, outer = np.array([255, 214, 96]), np.array([150, 70, 10])
    bg = (inner * (1 - t) ** 1.4 + outer * (1 - (1 - t) ** 1.4)).clip(0, 255).astype(np.uint8)
    icon = Image.fromarray(np.dstack([bg, np.full((124, 124), 255, np.uint8)]), "RGBA")
    icon.alpha_composite(fit(framed, 124, 124, 10))
    icon.save(os.path.join(rec, f"SprkIcons.Item.{item_code}.png"), optimize=True)
    return verts, index


def check_goblin(bundle):
    """Print the original mesh's extents so the constants above can be re-checked."""
    import UnityPy
    from UnityPy.helpers.MeshHelper import MeshHandler
    for o in UnityPy.load(bundle).objects:
        if o.type.name == "Mesh":
            h = MeshHandler(o.read())
            h.process()
            v = np.array(h.m_Vertices)
            n = np.array(h.m_Normals)
            front = v[n[:, 1] < -0.5]
            print("goblin x", v[:, 0].min(), v[:, 0].max(), "z", v[:, 2].min(), v[:, 2].max())
            print("goblin front y(x) fit", np.polyfit(front[:, 0], front[:, 1], 2))
            tri = h.get_triangles()[0]
            a, b, c = (v[i] for i in tri[0])
            print("goblin winding check (>0 = Cross(b-a,c-a) along normal):",
                  float(np.dot(np.cross(b - a, c - a), n[tri[0][0]])))


if __name__ == "__main__":
    args = sys.argv[1:]
    goblin = None
    if "--goblin" in args:
        i = args.index("--goblin")
        goblin = args[i + 1]
        del args[i:i + 2]
    if len(args) != 4:
        sys.exit(__doc__)
    here = os.path.dirname(os.path.abspath(__file__))
    managed = os.path.normpath(os.path.join(here, "..", "..", "managed"))
    verts, index = build(args[0], args[1], args[2], args[3], managed)
    print(f"{args[1]}: {len(verts)} vertices, {len(index) // 3} triangles,"
          f" x {verts[:, 0].min():.3f}..{verts[:, 0].max():.3f}, z {verts[:, 2].min():.3f}..{verts[:, 2].max():.3f}")
    if goblin:
        check_goblin(goblin)
