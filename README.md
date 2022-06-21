# DOUBLE PRECISION COORDINATE SYSTEM
A plug-and-play, bolt-on 64-bit floating point coordinate retrofit for the Unity game engine.

## Current Features
- Singleton coordinate system object that handles broadphase calculations for determining interactions in the much larger world created by 64-bit coordinates.
- Bolt-on components that convert gameobjects with 32-bit coordinates to ones with 64-bit coordinates.
- A single bolt-on component that accomplishes floating origin viewing of the DCPS scene.

## How Does it Work?
The DCPS works via a sort of recursive multi-floating origin system for physical interactions, and a single floating origin for viewing per camera. The MFO for physics is ultimately a pre-broadphase broadphase that positions objects relative to each other, eliminating useless distances between them. (See diagrams, when they're added).

The DCPS does not eliminate Unity's native physics engine, merely bolts onto the top of it. While classes that depend on the 64-bit nature of the DCPS will not function without a DCPS component attached, anything that functions using native Unity rigidbody and physics engine behaviors will work normally if the DCPS is suddenly removed.
