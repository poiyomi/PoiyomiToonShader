#!/usr/bin/env python3
"""Apply Poiyomi's small host patch to an upstream LightVolumes.cginc.

Run with a downloaded upstream file and --revision COMMIT. By default only
validate; add --write to update the source template. Does not fetch files or
modify artist materials, the installed LV package, or generated shaders.
"""
import argparse
import re
from pathlib import Path


def apply_patch(source, patch):
    lines = patch.splitlines(keepends=True)
    hunks = []
    current = None
    for line in lines:
        if line.startswith('@@ '):
            current = [[], []]
            hunks.append(current)
            continue
        if current is not None and line.strip('\r\n') == '':
            # Editors may strip the space off blank context lines
            line = ' ' + line
        if current is not None and line[:1] in (' ', '-', '+'):
            if line[0] != '+':
                current[0].append(line[1:])
            if line[0] != '-':
                current[1].append(line[1:])
    if not hunks:
        raise ValueError('The host patch is empty.')
    for index, (before, after) in enumerate(hunks, 1):
        before, after = ''.join(before), ''.join(after)
        if source.count(before) != 1:
            raise ValueError(f'Host patch hunk {index} no longer matches uniquely. Review the upstream change before updating.')
        source = source.replace(before, after, 1)
    return source


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('upstream', type=Path)
    parser.add_argument('--revision', required=True)
    parser.add_argument('--write', action='store_true')
    args = parser.parse_args()
    if not re.fullmatch(r'[a-zA-Z0-9_.-]+', args.revision):
        parser.error('Revision must be a commit or tag, without whitespace.')
    folder = Path(__file__).resolve().parents[1] / 'Editor/Poi_FeatureModules/Toon/Poi_PerPassIncludes'
    source = args.upstream.read_text()
    if '#T#' in source or 'LV_CUSTOM_SH_SPECULAR_BRDF' not in source:
        parser.error('Expected a plain upstream include with the custom SH BRDF hook.')
    try:
        source = apply_patch(source, (folder / 'LightVolumes.host.patch').read_text())
    except ValueError as error:
        parser.error(str(error))
    result = '#T#Source\n// Upstream lv-3: ' + args.revision + '\n// Host adjustments are recorded in LightVolumes.host.patch; see LightVolumes.md.\n' + source
    if args.write:
        (folder / 'VRLTC_LightVolumesSource.poiTemplateCollection').write_text(result)
    print(('Updated source template.' if args.write else 'Host patch applies cleanly; no files changed.') + ' Generate and validate Pro before shipping.')


if __name__ == '__main__':
    main()
