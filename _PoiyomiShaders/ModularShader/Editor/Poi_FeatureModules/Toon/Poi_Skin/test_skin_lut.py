"""
Test script to verify the Poi Skin module's LUT sampling matches the shader logic.
Run: python test_skin_lut.py
"""
from PIL import Image
import os

LUT_PATH = os.path.join(os.path.dirname(__file__), '..', '..', '..', '..', '..', 'Textures', 'Ramps', 'T_skinColorLUT.png')
LUT_PATH = os.path.normpath(LUT_PATH)

img = Image.open(LUT_PATH)
w, h = img.size
pixels = img.load()
print(f"LUT: {LUT_PATH}")
print(f"Size: {w}x{h}")
print()

def bilinear_sample(u, v):
    """Simulate sampler_linear_clamp: u,v in [0,1], v=1 is top of image (y=0)"""
    u = max(0.0, min(1.0, u))
    v = max(0.0, min(1.0, v))
    # Unity v=1 -> image y=0 (top), v=0 -> image y=h-1 (bottom)
    px = u * (w - 1)
    py = (1.0 - v) * (h - 1)
    x0 = max(0, min(w - 2, int(px)))
    y0 = max(0, min(h - 2, int(py)))
    fx = px - x0
    fy = py - y0
    def g(x, y): return pixels[x, y]
    def lerp3(a, b, t): return tuple(a[i] + (b[i] - a[i]) * t for i in range(3))
    top = lerp3(g(x0, y0), g(x0 + 1, y0), fx)
    bot = lerp3(g(x0, y0 + 1), g(x0 + 1, y0 + 1), fx)
    return lerp3(top, bot, fy)

def shader_sample_lut(melanin, melaninRedness, hemoglobin):
    """Exact replica of PoiSampleSkinLUT from the shader"""
    blend = (1.0 - melaninRedness) * 2.0
    panelA = min(int(blend), 1)
    panelB = panelA + 1
    panelLerp = blend - panelA

    margin = 1.0 / 384.0
    pw = 1.0 / 3.0

    uA = panelA * pw + margin + (pw - 2.0 * margin) * melanin
    uB = panelB * pw + margin + (pw - 2.0 * margin) * melanin
    v = 1.0 - hemoglobin

    colorA = bilinear_sample(uA, v)
    colorB = bilinear_sample(uB, v)

    def lerp3(a, b, t): return tuple(a[i] + (b[i] - a[i]) * t for i in range(3))
    result = lerp3(colorA, colorB, panelLerp)
    return tuple(int(round(c)) for c in result)

# ---- TEST 1: Corner verification ----
print("=== TEST 1: Corner pixel verification ===")
tests = [
    (0, 1, 0, "Lightest pheomelanin"),
    (0, 0, 0, "Lightest eumelanin"),
    (1, 1, 0, "Darkest pheomelanin"),
    (1, 0, 0, "Darkest eumelanin"),
    (0, 1, 1, "Lightest pheo, max hemoglobin"),
]
for mel, red, hemo, desc in tests:
    r = shader_sample_lut(mel, red, hemo)
    print(f"  {desc}: ({r[0]:3d},{r[1]:3d},{r[2]:3d})")
print()

# ---- TEST 2: Monotonic melanin per panel ----
print("=== TEST 2: Melanin monotonicity (should darken) ===")
mono_pass = True
for red_label, red in [("pheo", 1.0), ("mixed", 0.5), ("eu", 0.0)]:
    prev_lum = 999
    for mel_step in range(21):
        mel = mel_step / 20.0
        r = shader_sample_lut(mel, red, 0)
        lum = r[0] * 0.299 + r[1] * 0.587 + r[2] * 0.114
        if lum > prev_lum + 3:
            print(f"  [FAIL] {red_label}: mel={mel:.2f} lum={lum:.0f} > prev {prev_lum:.0f}")
            mono_pass = False
        prev_lum = lum
if mono_pass:
    print("  [PASS] All melanin ramps monotonically darken")
print()

# ---- TEST 3: Redness slider smoothness ----
print("=== TEST 3: Redness slider smoothness ===")
smooth_pass = True
for mel_test in [0.0, 0.3, 0.6, 1.0]:
    prev_lum = None
    for red_step in range(21):
        red = red_step / 20.0
        r = shader_sample_lut(mel_test, red, 0)
        lum = r[0] * 0.299 + r[1] * 0.587 + r[2] * 0.114
        if prev_lum is not None and abs(lum - prev_lum) > 15:
            print(f"  [FAIL] mel={mel_test:.1f}: jump at red={red:.2f} lum={lum:.0f} prev={prev_lum:.0f}")
            smooth_pass = False
        prev_lum = lum
if smooth_pass:
    print("  [PASS] Redness transitions smooth at all melanin levels")
print()

# ---- TEST 4: Hemoglobin adds redness ----
print("=== TEST 4: Hemoglobin effect ===")
for hemo_step in range(5):
    hemo = hemo_step / 4.0
    r = shader_sample_lut(0, 1, hemo)
    print(f"  hemo={hemo:.2f}: ({r[0]:3d},{r[1]:3d},{r[2]:3d})")
print()

# ---- TEST 5: Preset accuracy ----
print("=== TEST 5: Presets vs Fitzpatrick reference ===")
presets = [
    ("Pale",       0.0,  0.5,  0.05, 254, 222, 203),
    ("Fair",       0.1,  0.5,  0.05, 248, 214, 193),
    ("Light",      0.2,  0.5,  0.05, 227, 189, 161),
    ("Medium",     0.3,  0.5,  0.0,  203, 160, 122),
    ("Olive",      0.35, 0.25, 0.0,  181, 137, 101),
    ("Tan",        0.4,  0.75, 0.0,  182, 132,  88),
    ("Brown",      0.5,  0.5,  0.0,  142,  92,  57),
    ("Dark Brown", 0.65, 0.5,  0.0,   99,  56,  36),
    ("Deep",       0.85, 0.5,  0.0,   49,  27,  23),
]
for name, mel, red, hemo, tr, tg, tb in presets:
    result = shader_sample_lut(mel, red, hemo)
    err = sum((result[i] - t) ** 2 for i, t in enumerate((tr, tg, tb)))
    status = "PASS" if err < 600 else "WARN"
    print(f"  [{status}] {name:12s}: got ({result[0]:3d},{result[1]:3d},{result[2]:3d}) target=({tr:3d},{tg:3d},{tb:3d}) err={err}")
print()

print("=== DONE ===")
