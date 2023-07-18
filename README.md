<div align=center class=flex>
  <img height="125" alt="Poi Documentation" src="https://www.poiyomi.com/img/logo.svg">
  <br><br>
  <a href="https://discord.gg/poiyomi">
    <img alt="Discord" src="https://img.shields.io/discord/550477546958094348?color=%237289da&label=DISCORD&logo=Discord&style=for-the-badge">
  </a>
  <a href="https://patreon.com/poiyomi">
    <img src="https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.vercel.app%2Fapi%3Fusername%3Dpoiyomi%26type%3Dpatrons&style=for-the-badge" alt="Support Poiyomi on Patreon">
  </a>
  <a href="https://poiyomi.com/">
    <img alt="Website" src="https://img.shields.io/website?down_color=ff2244&down_message=poiyomi.com&label=DOCUMENTATION&style=for-the-badge&up_color=E7BF2A&up_message=poiyomi.com&url=http%3A%2F%2Fpoiyomi.com%2F">
  </a>
  <a href="https://github.com/poiyomi/PoiyomiToonShader/blob/master/LICENSE">
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
    <a href="https://poiyomi.com/">
      📖 Documentation (WIP!)
    </a>
  </h3>
</div>

## Features

- Multiple Shading models (Realistic, Toon, Flat, and more) with deep customization and robust handling of adverse lighting conditions
- Multiple rendering modes (Opaque, Cutout, Transparent, and more)
- Physically-based Reflections and Specular (metallic/smoothness workflow)
- Stylized rendering functionality (matcaps, outlines, rim lighting)
- Special effects (glitter/sparkle, emission, iridescence, etc)
- Powerful global masking system
- Extensive control over rendering options (Culling, ZWrite, ZTest, Stencils, etc)
- Much, much more!

## Versions

Poiyomi Shaders include multiple released versions for backwards compatibility. The latest version is always the most up-to-date, and is recommended for use in new projects. Older versions are provided for compatibility with older models - we include the last release for each minor version (e.g. 8.0.426 for v8.0, 7.3.050 for v7.3).

Currently, Poiyomi v7 is included for backwards compatibility with older models, and is no longer being updated. **It's not recommended to use v7 for new projects.**

When matching a version to a model, generally, the major and minor versions must match, but the patch version can be newer. For example, if a model specifies 8.0.295, one can use 8.0.426 (included in the latest release) without issue. For v7, generally, 7.3.050 is the last version, and should be used for all v7 models.

## Pro Shader

[Poiyomi Pro](https://patreon.com/poiyomi) offers additional features and functionality, and always has the latest developments. It contains everything in the free version, plus features like:
- Grabpass shading effects, like Refraction and Blur
- Fur shader, supporting all of the main shader's feature plus fluffy fur
- Tessellation and Geometry Shader effects, like a geometric dissolve
- Modular shader system support, for installing third-party shader modules
- TPS (8.1+)
- DPS Support (7.3, 8.2+)

To get access to Poiyomi Pro, [support Poiyomi on Patreon](https://patreon.com/poiyomi) at the $10 tier or higher, [link your Discord to your Patreon](https://support.patreon.com/hc/en-us/articles/212052266-Get-my-Discord-role), and join the [Poiyomi Discord](https://discord.gg/poiyomi) to get the Pro version of the shader.

---

# 💻 For Developers

## ThryEditor

Poiyomi relies on [ThryEditor](https://github.com/Thryrallo/ThryEditor) to display its inspector properly and lock the shader to ensure optimal performance. ThryEditor is included in the repository, and should not be updated separately.


## Compatibility

Poiyomi Shaders are designed for use with the **Built-In Render Pipeline (BIRP)** on **DirectX 11**. We target VRChat's [currently supported Unity version](https://docs.vrchat.com/docs/current-unity-version), **2019.4.31f1** at time of writing. Higher Unity versions have been tested and are known to work, but we primarily target the latest Unity version that VRChat supports.

Poiyomi shaders are currently **not compatible** with Unity's Scriptable Rendering Pipelines (URP, HDRP, SRP). Non-DirectX 11 platforms (e.g. OpenGL, Metal, Vulkan, etc.) are also not supported, and may not work at all.

The shaders can be used for games outside of VRChat, but may not be ideal due to materials being unable to share a common shader. For non-VRChat usage, make sure to use the [Locking](https://www.poiyomi.com/general/locking) functionality provided by ThryEditor to create optimized shaders for each material.

## Contributing

Active development happens on the Pro shader in a private repository, with free releases happening periodically. If you'd like to contribute code to the shader, join the [Discord](https://discord.gg/poiyomi) and inquire. For the Editor code side, you can contribute to [ThryEditor](https://github.com/Thryrallo/ThryEditor), as it's MIT-licensed and accepts pull requests.

We're always looking for help with [documentation](https://poiyomi.com/). If you'd like to contribute, you can do so by [in the docs repository](https://github.com/poiyomi/PoiyomiDocs) - we accept pull requests! Docs are written in Markdown, and are built using [Docusaurus](https://docusaurus.io/).

To report a bug or request a feature, you can do so either by [opening an issue](https://github.com/poiyomi/PoiyomiToonShader/issues) or by joining the [Discord](https://discord.gg/poiyomi) and asking. Please make sure to include as much information as possible, including screenshots and/or videos if applicable. Ensure you're using the latest version of the shader, and that the issue is not already reported.

