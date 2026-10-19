# Light Volumes integration

`VRLTC_PoiLightVolumesInclude.poiTemplateCollection` expands three sections from two source files:

1. `Hooks` in `VRLTC_PoiLightVolumesHelpers.poiTemplateCollection` declares Poi's SH callback before the LV include.
2. `Source` in `VRLTC_LightVolumesSource.poiTemplateCollection` contains LV's embedded implementation.
3. `Helpers` contains Poi's iterator, lightmap shaping and baked specular adapters.

The generator writes these into Pro. Generated and locked shaders do not need the templates at runtime, and they include no LV files, so they compile wherever Poiyomi is installed. The LV package is optional. The wrapper retains GUID `c240050b0ea83c0439fceed9873024f1` and its `Template` local ID `-544282384758852595`. Its LV exclusion surrounds all three sections.

## Updating the include

Current upstream: [647f3b5e99ad2c5f3956134b2c955dd1b6fe38cb](https://github.com/REDSIM/VRCLightVolumes/commit/647f3b5e99ad2c5f3956134b2c955dd1b6fe38cb) (dev.21). It keeps braces balanced with every preprocessor branch visible, which d4rkAvatarOptimizer's parser requires.

Download `Packages/red.sim.lightvolumes/Shaders/LightVolumes.cginc` from the desired revision. From the shader checkout, run:

```sh
python3 ModularShader/Tools/update_light_volumes.py /path/to/LightVolumes.cginc --revision COMMIT
python3 ModularShader/Tools/update_light_volumes.py /path/to/LightVolumes.cginc --revision COMMIT --write
```

The first command validates without writing. The second replaces the source template and records the revision. The utility reapplies `LightVolumes.host.patch` using exact context. A mismatch stops the update for review, rather than silently applying an approximate patch. The tool does not download code, change the installed LV package, edit materials, or generate shaders.

For a manual update, retain `#T#Source`, replace the source beneath it, and reapply the host patch. To change a host adjustment, edit the source template and regenerate the patch as a full diff from the upstream file. Several hunks sit next to each other, so hand-edited context breaks easily. The seven remaining host adjustments are:

- Drop the `LightVolumesBuildConfig.cginc` include. A hardcoded `Assets/` path breaks VPM installs under `Packages/`, and LV's default config defines nothing. Every LV feature stays compiled in; the LV package's world scene stripping does not apply to Poiyomi.
- Group constant buffers to fit Pro's DirectX and Vulkan variants.
- Keep unshadowed point/spot irradiance even at zero visibility, so the artist's shadow reception controls can still use the available light.
- Alias LV's three texture samplers to Poi's `sampler_trilinear_clamp`, freeing up to three of DirectX's 16 sampler slots. LV creates all three textures with clamp wrap and no anisotropy and reads them only with `SampleLevel`, so the result is unchanged. Recheck this if upstream changes those texture settings.
- Evaluate each light's normal mask and baked shadow once, after the Point/Spot/Area branches. Upstream inlines the shadow path separately for each light type. In a locked material that receives cast shadows, the shared path saves about 160 DX11 instructions in every pass that samples LV. With Receive Casted Shadows at 0, the compiler removes the shadow path either way. Point lights use `LV_PointLightShadow` with forced cubemaps, so `LV_PointLightShadowPackedCube` is removed. A new `areaDepthRange` flag keeps Area's legacy reprojection depth range. The output is unchanged, apart from rounding of about 1e-8 in Point and Area shadows when `VRCLV_DISABLE_CUBEMAP_SHADOWS` is defined, which Poiyomi does not do.

  Mirror upstream shadow changes into the shared path by hand. A clean patch is not enough here: upstream's `forceCubemapShadow` inside `LV_PointLightShadow` means Area, but Poiyomi's Point lights pass it too. On every update, review upstream's `LV_PointLightShadow` diff. An Area-only use of `forceCubemapShadow` must become `areaDepthRange`, and a change that upstream did not also make to `LV_PointLightShadowPackedCube` must be checked against Point.
- Find regular volumes A and B in one scan in `LV_LightVolumeRegularSH`. Upstream searches for B in a second loop. The tested volumes, selection, fallback and output are the same; locked Base and Outline programs lose about 10 instructions and one dynamic loop.
- Jump straight to the next non-empty mask word in `LV_NextClusteredLight` instead of looping over empty words inside the light walk. The light order is unchanged; it removes the walk's only nested dynamic loop.

Poi's iterator calls LV's internal contribution and clustering functions and reads packed shadow IDs when recovering culled lights. Those signatures and data meanings still need review when upstream changes. The official SH hook removes the old public-function macro redirects; it does not remove those iterator dependencies.

Generate Pro after an update. Validate Base/Add on DirectX and Vulkan, baked/local specular, receive/ignore-shadow settings, Unity fallback, mixed lights, material locking, feature stripping and package-free compilation. Check that the generated shader includes only Unity's own shader files.

## Baked and individual specular

All Poiyomi LV paths use `PoiLightVolumesActive()`: the material's LV switch and the
official `LightVolumesEnabled()` availability check. Sampling, lightmap shaping and
highlights branch on it before doing LV work. Absent, disabled or unsupported LV uses
Poiyomi's existing Unity probe lighting, as described in the
[availability documentation](https://github.com/REDSIM/VRCLightVolumes/blob/lv-3/Documentation/ShaderFunctions.md#float-lightvolumesenabled).
An enabled LV setup retains upstream probe fallback, including when no volumes are registered.
LV runs in the built-in Base and Outline passes; Unity additive passes keep their own lighting.

Baked SH and individual lights stay separate. Volume SH is sampled once and cached without local lights. Material layers evaluate the cache directly; they do not resample the volumes or repeat the full shading stack. `LV_SpecularBRDFSH_Custom` is also registered with LV's combined regular/additive sampling functions for callers using the upstream API.

GGX's SH adapter uses Poi's BRDF with the material's normal/tangent basis, anisotropy, direction map and second specular layer. A weak SH direction fades out, and low directionality broadens the highlight. The existing bounded isotropic approximation remains available through the generic hook for PBR consumers.

Local lights reach GGX, Clear Coat and PBR the way LV's own `LightVolumeSHSpecular` handles them: each light's highlight is evaluated inside the light walk, with its own direction, visibility and emitter size, so no light data is stored for later. The walk runs before those layers set up their surface, so it prepares a copy of their normal and roughness first. Their f0 needs the final base color, so each lobe keeps the two parts of Schlick's Fresnel apart and the layer applies f0 afterwards. Stylized layers draw one light per pixel: the brightest after weighting by how close it is to the reflection direction, tinted by the other lights near that direction.

GGX, clearcoat and PBR use LV's finite-source horizon response as well as its roughness
widening. A large source can still produce a highlight while part of it is above the
surface, even when its center is below it. Unity lights and recovered SH retain their
existing response. This changes local LV highlights near grazing angles; it adds no
properties and needs no material remapping.

LV's optional Hi-Z shadow culling can omit fully occluded lights from a cluster.
Poiyomi still needs their available color for toon shadow palettes and reduced shadow
reception. The iterator merges shadow-casting candidates with the cluster mask in
ascending light-ID order. Range, projection and overdraw checks still use LV's
contribution function, and specular sees the same lights as diffuse. Outside the
clustering frustum, LV's ordinary light list is used.
The recovery adds candidate checks in clustered scenes with shadow maps, without
changing the world's culling settings or requiring a modified LV package.

lilToon retains its own highlight function and blend modes for both baked and local contributions. Its roughness, border and blur are not remapped from emitter size. Ambient-only SH contains no recoverable specular direction. Weak SH directions fade highlight coverage continuously; they do not change the artist’s roughness, border or blur. Unity Chan also evaluates its native two-layer shape and Replace/Add/Screen/Multiply blend for the baked SH and the walk's picked light. Its material maps are sampled once. Ignore Casted Shadows bypasses per-source visibility; the existing material ramp mask remains under Ignore Shadow, matching the native UTS controls.

GGX and lilToon local highlights now respect Light Data's base-pass Receive Casted Shadows. GGX and clearcoat also respect their Ignore Casted Shadows controls. GGX local and recovered SH highlights belong to Specular and use its tint, mask and strength. Reflections supplies cubemap reflections, so enabling both no longer doubles the SH lobe. Clearcoat keeps its separate Specular Tint/occlusion. Unity additive lights continue to use the additive-pass shadow controls.

No properties, defaults or enum values changed. Existing GGX materials with only Reflections enabled no longer receive an extra SH highlight; use Specular for LV highlights. Materials with both enabled lose the duplicate contribution. Multilayer deliberately keeps its three per-layer receive settings independent of Light Data’s main Receive Casted Shadows slider. LV follows that same design; the main slider does not gate its per-layer lightmaps.

The lilToon translator retains its established environment/main-shadow mapping and per-layer receive values. Unity Standard’s separate specular/reflection switches keep their existing mappings. Skin, Cloth and SDF diffuse paths remain unchanged.

## Shading modes

The first LV sample keeps available lighting separate from cast visibility. Ramped modes can use the light's shape without replacing the artist's shadow palette. Shading runs once after the light data is combined.

- Texture Ramp and ShadeMap consume the shared lightmap. Multilayer keeps its three normal strengths, ramps and per-layer shadow reception.
- Wrapped uses its existing Wrap and Normalization functions when collecting each light's form, then applies its Gradient Start/End and colors once. Its LV wrap helpers follow the Shading feature guard so shaders with Shading disabled and vertex-stage variants still compile.
- Flat combines cast visibility by light intensity without introducing normal shading.
- Realistic and PBR consume received diffuse SH. Only the occluded light's contribution is reduced; baked illumination and other lights remain. Lightmapped objects add only additive volumes and local lights.
- Skin, Cloth and SDF keep their existing paths.

Texture Ramp, ShadeMap, Wrapped, Flat, Realistic and PBR use Light Data's base-pass Receive Casted Shadows, including intermediate values. Multilayer uses its own per-layer reception independently of that control. PBR's individual LV highlights use that control too. Add-pass Receive Casted Shadows continues to control Unity additive passes; LV is sampled in the base pass. Casted Shadow Softness currently filters Unity directional shadow maps; this change does not add a separate LV blur filter.

No shader properties, defaults or enum values changed. See the material behavior notes above. Materials already receiving shadows gain the missing LV shadow response in Flat, Realistic and PBR. Wrapped gains per-light form shaping while retaining its authored gradient and color controls. Receive Casted Shadows at zero retains unoccluded light data.

## Regression tests

The regression suites are kept in the local development checkout.

Run `LightVolumeIntegrationTests` and `LightVolumeShadowTests` after updating the source or helpers. They cover material specular ownership, shadow reception, independent Multilayer layer controls, stylized blending, shader locking, Unity fallback and migration. Validate Base/Add on Vulkan and DirectX; generate Pro World when testing lightmapped variants.
