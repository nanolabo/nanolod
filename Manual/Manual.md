# Nanolod Manual

## What is Nanolod

**Nanolod** is an Unity plugin to generate lods.

## Features

* Generate LODs at project level from the model inspector
* Generate LODs at scene level from the LODGroup component
* Target polycount is reached globally instead of per-mesh
* Preserves animations (bones weights)
* Preserves normals
* Preverves UVs (channel 0)
* Preserves borders

## Current limitations / Known issues

* This is a PREVIEW version (it might not be very stable)
* There is no parametrization for generated LODs. The polycount ratio for a given LOD is the LOD threshold (eg: LOD at 50% has 50 polycount)
* Vertex colors are not supported
* Tangents are not kept (but recomputed from normals)
* UV channels other than 0 are not supported

## How to install

* In Unity, open package manager (Window / Package Manager)
* Click "+", and "Add package from disk..."
* Select "package.json" from Nanolod
* Enjoy :)

## How to use

There are currently two ways to generate LODs

### Generate LODs at project level from the model inspector

![from model](nanolod/Manual/modelimporter.png)

### Generate LODs at scene level from the LODGroup component

![from LODGroup](nanolod/Manual/lodgroup.png)