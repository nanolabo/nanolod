# Nanolod Manual

## What is Nanolod

**Nanolod** is an Unity plugin to generate lods.

## Features

* Generate LODs at project level from the model inspector
* Generate LODs at scene level from the LODGroup component
* Target polycount is reached globally instead of per-mesh
* Manages mesh instances (does not create duplicates for a mesh instanced more than once)
* Preserves animations (bones weights)
* Preserves normals
* Preverves UVs (channel 0 for textures and 1 for lightmaps)
* Preserves borders
* Preserves vertex colors
* Also works at runtime !

## Current limitations / Known issues

* This is the first version (it might not be very stable, updates will come)
* There is no parametrization for generated LODs. The polycount ratio for a given LOD is the LOD threshold (eg: LOD at 50% has 50% polycount)
* Tangents are not kept (but recomputed from normals)
* For now computation is single-threaded

## How to use

There are currently two ways to generate LODs

### Generate LODs at project level from the model inspector

![from model](nanolod/Manual/modelimporter.png)

### Generate LODs at scene level from the LODGroup component

![from LODGroup](nanolod/Manual/lodgroup.png)

Checkout the two samples to see the plugin in action !