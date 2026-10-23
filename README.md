<div align=center class=flex>
  <img height="125" alt="Poi Documentation" src="https://www.poiyomi.com/img/PoiyomiTextFull.svg">
  <br><br>
  <a href="https://discord.gg/poiyomi">
    <img alt="Discord" src="https://img.shields.io/discord/550477546958094348?color=%237289da&label=DISCORD&logo=Discord&style=for-the-badge">
  </a>
  <a href="https://www.patreon.com/c/poiyomi/">
    <img src="https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.vercel.app%2Fapi%3Fusername%3Dpoiyomi%26type%3Dpatrons&style=for-the-badge" alt="Support Poiyomi on Patreon">
  </a>
  <a href="https://www.poiyomi.com/">
    <img alt="Website" src="https://img.shields.io/website?down_color=ff2244&down_message=poiyomi.com&label=DOCUMENTATION&style=for-the-badge&up_color=E7BF2A&up_message=poiyomi.com&url=http%3A%2F%2Fpoiyomi.com%2F">
  </a>
  <a href="https://github.com/poiyomi/PoiyomiToonShader/blob/master/LICENSE.txt">
    <img alt="GitHub" src="https://img.shields.io/github/license/Poiyomi/PoiyomiToonShader?color=1BB7E4&style=for-the-badge">
  </a>
  <a href="https://github.com/poiyomi/PoiyomiToonShader/releases/latest">
    <img alt="GitHub tag (latest by date)" src="https://img.shields.io/github/v/tag/Poiyomi/PoiyomiToonShader?color=F05A7A&label=RELEASE&style=for-the-badge">
  </a>
  <img alt="GitHub all releases" src="https://img.shields.io/github/downloads/poiyomi/PoiyomiToonShader/total?style=for-the-badge">
</div>
<br>
<div align=center>

  **Poiyomi Shaders** are feature-rich shaders for Unity's Built-In Rendering Pipeline, intended for use with **VRChat.** They support multiple shading modes and robust light handling, and are designed to be easy-to-use and performant. They're also **free** and **open-source**!

</div>

---

<div align=center>

# [📦 Grab the VPM for VCC!](https://poiyomi.github.io/vpm/)
# [📦 Download the latest Unity Package!](https://github.com/poiyomi/PoiyomiToonShader/releases/latest)

</div>

After downloading, just import the package into your Unity project!

**Important:** When updating from a previous version, **make sure to delete the old `_PoiyomiShaders` folder** from your project's `Assets` folder before importing the new package! Otherwise, there may be interference.

If **distributing** an avatar or other asset that uses Poiyomi shaders, **do not include the `_PoiyomiShaders` folder in your asset's package.** Instead, direct users to download the correct version from the [releases page](https://github.com/poiyomi/PoiyomiToonShader/releases), or include the package alongside the asset, not as part of its Unity package.

---

<div align=center>
  <h3>
    <a href="https://discord.gg/poiyomi">
      🪧 Discord (Help and Info!)
    </a>
    •
    <a href="https://patreon.com/poiyomi">
      🪙 Patreon (exclusive features!)
    </a>
    •
    <a href="https://www.poiyomi.com/">
      📖 Documentation
    </a>
  </h3>
</div>

## Feature Highlights
- **Various Shading Styles**
  - Stylized Toon Shading presets with customizable options, such as Texture Ramp, Multilayer Math, and ShadeMap.
  - Realistic Shading presets, such as Wrapped, Skin, Realistic, and Cloth.
  - Sophisticated Light Data settings for changing how the lighting and shading is influenced on the Material.
  - Outline features for adding cartoon-style outer stencils to your Model, using the inverse-hull effect.
  - Grab Pass features, available as a separate shader variant.
- **Easy to Use**
  - User Interface with sophisticated organization and layout that sets it apart from other Toon shaders.
- **Fun, Decorative, and Powerful Features**
  - Up to 4 Decals, for fun and unique ways to decorate an Avatar with logos, shapes, stickers, and patterns.
  - Details, allowing you to apply (often tiled textures) to the Base Color and Normals.
  - Up to 4 Matcaps for creating fast and approximate looks from a spherical texture, can often add to a shading style.
  - UV Tile Discard support.
  - Cubemaps, for simulating reflections and other effects using an HDR cubemap texture.
  - Flipbooks, providing support for Texture Arrays that can be played as an animation on the Material.
  - Up to 4 Emission Slots, each with independent unique options for adding highlights or glow effects.
  - Dissolve effects with various transition styles. Also supports UV Tile Discarding using Dissolve.
  - Glitter, providing voronoi-based sparkles to the color of the mesh that can create glittery surfaces with specular lighting.
  - Various UV options such as support for UV0, UV1, UV2, UV3, Distortion, Panosphere, and Polar UV.
- **PBR Features**
  - Reflections & Specular, for metallics and reflective surfaces.
  - Clear Coat, creating glossy appearances that can be compared to automotive paint or carbon fiber composite.
  - Up to 2 Rim Lighting effects, allowing for fresnel-like highlights or glows around the edges of a Material.
  - Environmental Rim, simulating low-angle reflections from the surrounding environment.
- **Advanced Masking Features**
  - Vertex Colors support and various masking support for advanced customization.
  - RGBA Color Masking for applying different color tints, textures, and normal maps with the main color.
  - Signature Global Masking support, allowing for up to 16 Color Channels to use from up to 4 Global Masks across the Material.
- **The most advanced Material User Interface**
  - Integrated Texture Packers for merging Masks and Color Channels in various features.
  - Use Presets to easily apply features from a Material onto another Material.
  - Search functionality, allowing ease of access to common features.
  - Cross-Shader Editor for editing multiple Materials using Poiyomi Shaders, all at once.
  - All features of ThryEditor.
- **Full AudioLink Support**
  - Over 10+ modules in Poiyomi Shaders supporting various AudioLink effects!
  - AL Spectrum, projecting a waveform-like effect that react to the World's music onto the UV.
- **LTCGI Support**
  - Supports receiving LTCGI diffuse and specular lighting from Worlds actively producing LTCGI.
- **Light Volumes Support**
  - Supports receiving volumetric and balanced lighting from Worlds baked with VRC Light Volumes.

## Versions

Poiyomi Shaders include multiple released versions for backwards compatibility. The latest version is always the most up-to-date and is recommended for use in new projects.

Currently, the latest publicly-accessible version contains versions 9.3 thru 10.0.

## Pro Shader

[Poiyomi Pro](https://www.patreon.com/c/poiyomi/) offers additional features and functionality, and always has the latest developments. It contains everything in the free version, plus features like:
- Fur Shader, supporting all of the main shader's feature plus fluffy fur.
- Tessellation and Geometry Shader effects, like a Geometric Dissolve.
- TPS
- SSAO
- Contact Shadows
- Squish
- Early, unreleased features
- and more!

To get access to Poiyomi Pro, [support Poiyomi on Patreon](https://www.patreon.com/c/poiyomi/) at the $10 tier or higher, then use the VCC package `Poiyomi Pro` to authenticate with your Patreon account to download and install Poiyomi Pro into your project. Optionally, you can install Poiyomi Pro manually by accessing the downloads on our [Poiyomi Pro Website](https://pro.poiyomi.com/) by logging in with your Patreon account. For more information, refer to our Documentation [here](https://www.poiyomi.com/download#poiyomi-pro).

---

# 💻 For Developers

## ThryEditor

Poiyomi relies on [ThryEditor](https://github.com/poiyomi/ThryEditor) to display its inspector properly and lock the shader to ensure optimal performance. ThryEditor is provided as a separate VCC Package available in the same repository notated above and will be auto-installed when adding Poiyomi Toon to your project via VCC.

## Compatibility

### VRChat / Built-in Render Pipeline

Poiyomi Shaders are designed for use on Windows 10/11 with the `Built-In Render Pipeline (BIRP)` on `Direct3D 11`. We target VRChat's [currently supported Unity version](https://docs.vrchat.com/docs/current-unity-version), `2022.3.22f1` at time of writing. Higher Unity versions above 2022.3 are tested and are known to work, but we primarily target the latest Unity version that VRChat supports.

Linux users are advised to use Vulkan, as OpenGL is unsupported.

### BasisVR / Universal Render Pipeline (URP)

We support the Universal Rendering Pipeline (URP) on `Unity 6000.0 and newer` for BasisVR and other games running on URP. Packages can be acquired in the [Releases](https://github.com/poiyomi/PoiyomiToonShader/releases/latest) page. **If using the URP version, you must use the package labeled `Poi.Toon.URP.<version>.unitypackage`.**

### Incompatibility

**WE DO NOT SUPPORT the High Definition Render Pipeline (HDRP) and Scriptable Render Pipeline (SRP)!**

Non-DirectX 11 platforms (e.g. OpenGL, Metal, etc.) are not guaranteed to work as most of our code requires DirectX 11. However the shader has been tested on Linux using Vulkan and does work, but keep in mind it is highly experimental.

## Contributing

Active development happens on the Pro shader in a private repository, with free releases happening periodically. If you'd like to contribute code to the shader, join the [Discord](https://discord.gg/poiyomi) and inquire. For the Editor code side, you can contribute to [ThryEditor](https://github.com/poiyomi/ThryEditor), as it's MIT-licensed and accepts pull requests.

We're always looking for help with [documentation](https://poiyomi.com/). If you'd like to contribute, you can do so by [in the docs repository](https://github.com/poiyomi/PoiyomiDocs) - we accept pull requests! Docs are written in Markdown, and are built using [Docusaurus](https://docusaurus.io/).

To report a bug or request a feature, you can do so either by [opening an issue](https://github.com/poiyomi/PoiyomiToonShader/issues) or by joining the [Discord](https://discord.gg/poiyomi) and asking. Please make sure to include as much information as possible, including screenshots and/or videos if applicable. Ensure you're using the latest version of the shader, and that the issue is not already reported.
