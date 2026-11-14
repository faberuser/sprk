"""Compose Maya's perk icons in the released Light/Dark perk style.

Frames: pixels shared by three of Selene's Light (or Dark) perk icons (60001010/20/50, 60001011/21/51).
Tint: per-channel linear fit from Selene's skill icon 1010 to her perk 60001010 / 60001011 inside the frame.
Usage: python make_perk_icons.py <dir with extracted sprites> <out dir>
"""
import os, sys
import numpy as np
from PIL import Image

src, out = sys.argv[1], sys.argv[2]
os.makedirs(out, exist_ok=True)
load = lambda n: np.asarray(Image.open(os.path.join(src, n + ".png")).convert("RGBA")).astype(np.float32)


def frame(names):
    imgs = [load(n) for n in names]
    stack = np.stack([i[..., :3] for i in imgs])
    spread = stack.max(0) - stack.min(0)
    mask = (spread.max(-1) < 30).astype(np.float32)            # identical across perks -> frame
    # Keep only the border band and corner ornaments; ignore accidental matches in the middle.
    h, w = mask.shape
    yy, xx = np.mgrid[0:h, 0:w]
    edge = np.minimum(np.minimum(xx, w - 1 - xx), np.minimum(yy, h - 1 - yy))
    corner = (np.minimum(xx, w - 1 - xx) < 34) & (np.minimum(yy, h - 1 - yy) < 34)
    mask *= ((edge < 9) | corner).astype(np.float32)
    # Close small holes, then drop isolated specks (3x3 max/min filters).
    def shift_stack(m):
        p = np.pad(m, 1)
        return np.stack([p[1 + dy:1 + dy + h, 1 + dx:1 + dx + w] for dy in (-1, 0, 1) for dx in (-1, 0, 1)])
    dil = lambda m: shift_stack(m).max(0)
    ero = lambda m: shift_stack(m).min(0)
    mask = ero(ero(dil(dil(mask))))
    mask = dil(ero(mask))
    # Soften by one pixel so the frame blends into the art.
    pad = np.pad(mask, 1)
    soft = (pad[1:-1, 1:-1] * 4 + pad[:-2, 1:-1] + pad[2:, 1:-1] + pad[1:-1, :-2] + pad[1:-1, 2:]) / 8
    return np.median(stack, 0), np.clip(np.maximum(mask, soft), 0, 1)


def tint_fit(base, perk, mask):
    inside = mask < 0.05
    coeffs = []
    for c in range(3):
        x, y = base[..., c][inside], perk[..., c][inside]
        a, b = np.polyfit(x, y, 1)
        coeffs.append((a, b))
    return coeffs


def compose(art, frame_rgb, mask, coeffs):
    rgb = art[..., :3].copy()
    for c, (a, b) in enumerate(coeffs):
        rgb[..., c] = np.clip(rgb[..., c] * a + b, 0, 255)
    m = mask[..., None]
    rgb = rgb * (1 - m) + frame_rgb * m
    return Image.fromarray(np.dstack([rgb, np.full(rgb.shape[:2], 255.0)]).astype(np.uint8), "RGBA")


light_rgb, light_mask = frame(["60001010", "60001020", "60001050"])
dark_rgb, dark_mask = frame(["60001011", "60001021", "60001051"])
base = load("1010")[..., :3]
light_tint = tint_fit(base, load("60001010")[..., :3], light_mask)
dark_tint = tint_fit(base, load("60001011")[..., :3], dark_mask)
print("light tint", [(round(a, 2), round(b, 1)) for a, b in light_tint])
print("dark tint", [(round(a, 2), round(b, 1)) for a, b in dark_tint])

jobs = {f"6000{s}": (str(s), "light") for s in (1410, 1420, 1430, 1440)}
jobs.update({f"6000{s + 1}": (str(s), "dark") for s in (1410, 1420, 1430, 1440)})
jobs["60001450"] = ("Maya_5", "light")
jobs["60001451"] = ("DARKNESS_MAYA_5", "dark")
for name, (art, kind) in jobs.items():
    a = load(art)
    if a.shape[:2] != light_mask.shape:
        a = np.asarray(Image.fromarray(a.astype(np.uint8)).resize(light_mask.shape[::-1], Image.LANCZOS)).astype(np.float32)
    img = compose(a, light_rgb if kind == "light" else dark_rgb, light_mask if kind == "light" else dark_mask,
                  light_tint if kind == "light" else dark_tint)
    img.save(os.path.join(out, f"SprkIcons.Skill.{name}.png"))
names = sorted(jobs)
sheet = Image.new("RGBA", (130 * len(names), 130), (40, 40, 40, 255))
for i, n in enumerate(names):
    sheet.paste(Image.open(os.path.join(out, f"SprkIcons.Skill.{n}.png")), (i * 130 + 3, 3))
sheet.resize((sheet.width * 2, sheet.height * 2)).save(os.path.join(src, "maya_perks.png"))
print("wrote", len(jobs))
