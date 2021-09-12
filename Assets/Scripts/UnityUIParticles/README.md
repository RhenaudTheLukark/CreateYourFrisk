# UnityUiParticles
Unity ParticleSystem for built-in UI

[![](https://img.shields.io/badge/requirement-Unity%202018.2%2B-green.svg)](https://unity3d.com)

Full compatibility with Unity UI Canvas: sorting order, masking, UI shaders etc...
Size and speed of particles are in canvas-based coordinate system.

## Usage
Just add ParticleSystemMeshGenerator to gameobject with ParticleSystem.

## Restrictions
Texture sheet animation 'Sprites' mode is unsupported due to availability of using the
'Trails' module. It requires second material in one canvas renderer.
On the other hand, 'Sprites' mode requires all the sprites were inside the same texture, which makes it redundant.
Just use the 'Grid' mode instead.  
Camera for baking mesh - Orthographic

## Requirements
Unity **2018.2+** due to new interface for ParticleSystemRender:
* BakeMesh
* BakeTrailsMesh

## License
* MIT

## Credits
Made by [Kirill Belonogov](https://github.com/ken48).