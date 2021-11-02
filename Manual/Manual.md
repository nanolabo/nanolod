# Nanolod Manual

## What is Nanolod

**Nanolod** is an Unity plugin to generate lods.

## Features

- Generate LODs in prefab mode
- Generate LODs at project level from the model inspector
- Generate LODs at scene level from the LODGroup component
- Target polycount is reached globally instead of per-mesh
- Manages mesh instances (does not create duplicates for a mesh instanced more than once)
- Preserves animations (bones weights)
- Preserves normals
- Preverves UVs (channel 0 for textures and 1 for lightmaps)
- Preserves borders
- Preserves vertex colors
- Also works at runtime !

## Current limitations / Known issues

- There is no parametrization for generated LODs. The polycount ratio for a given LOD is the LOD threshold (eg: LOD at 50% has 50% polycount)
- Tangents are not kept (but recomputed from normals)
- For now computation is single-threaded

## How to use

There are currently three ways to generate LODs

### Generate LODs for a prefab

This is the recommend way to use Nanolod, because it is often the easiest and most flexible way.  
The downside is that if the model asset is changed, you'll need to regenerate LODs (it's one click, but it won't be automatic). You can always generate LODs a project level instead from the model if this is an issue for you.  
In order to generate LODs on a prefab, first open the prefab in prefab mode (double click), then add an LODGroup component, and finally, click Auto Generate LODs from the menu, like it's shown on the picture below.

![from LODGroup](nanolod/Manual/lodgroup.png)

Generated meshes will be saved within the prefab itself. If you prefer to save the mesh assets in a separate folder, you can do that by changing the Nanolod preferences from the Preferences / Nanolod menu.

### Generate LODs at project level from the model inspector

![from model](nanolod/Manual/modelimporter.png)

### Generate LODs at scene level from the LODGroup component

![from LODGroup](nanolod/Manual/lodgroup.png)

Checkout the two samples to see the plugin in action !

Generated meshes will be saved within the scene itself. If you prefer to save the mesh assets in a separate folder (so that you can reuse them for instance), you can do that by changing the Nanolod preferences from the Preferences / Nanolod menu.
