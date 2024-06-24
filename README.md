# MikCAD:
CAD software written in C# using WPF for GUI and OpenTK for visualization.\
Supported features and objects:
  - Creation and modification of:
    - Points,
    - Toruses,
    - Bézier curves:
      - C0,
      - C2,
      - Interpolating with C2 continuity,
    - Bézier surfaces:
      - C0,
      - Wrapped C0 (cylinder without base),
      - C2,
      - Wrapped C2 (cylinder without base),
  - Hole patching using Gregory patch,
  - Finding and displaying (in world and parameter space) intersection between following objects:
    - Bézier surfaces,
    - Toruses,  
  - Stereoscopy using colors,
  - Machine milling simulation using simplified G-code files:
    - flat and ball cutters with configurable radius,
    - error detection:
      - milling too deep,
      - milling down with flat cutter.
  - G-code generation for one provided model designed using this software.

# Screenshots

# From a 3D model to a real one (milled on CNC)

![model](https://github.com/WojcikMikolaj/MikCAD/assets/52234302/af174759-d306-4353-9287-86f146d82d61)
|:--:|
|My teapot model composed of C2 surfaces. Low tessellation level.|

![model2](https://github.com/WojcikMikolaj/MikCAD/assets/52234302/63ddb117-b06a-4a87-89d9-340ae502dbd1)
|:--:|
|My teapot model composed of C2 surfaces. High tessellation level.|

![sc1](https://github.com/WojcikMikolaj/MikCAD/assets/52234302/ac79b7d4-c69d-4e82-8288-f55a641600e6)
|:--:|
|Paths for first milling.|

![sc2](https://github.com/WojcikMikolaj/MikCAD/assets/52234302/91b8d46d-5254-4529-b3a0-9f7c3726047f)
|:--:|
|Paths for second milling.|

![sc3](https://github.com/WojcikMikolaj/MikCAD/assets/52234302/5c7cf026-c6dd-4705-96ac-7b4527315c5d)
|:--:|
|Paths for fourth milling.|

![sc8](https://github.com/WojcikMikolaj/MikCAD/assets/52234302/6bd5fe0c-e391-447b-9aa2-268c6e58797f)
|:--:|
|Paths for detailed milling.|

![frez](https://github.com/WojcikMikolaj/MikCAD/assets/52234302/996058de-d6e4-4528-ac0f-ca5782c33ca2)
|:--:|
|Simulated milling result.|

![czajnik](https://github.com/WojcikMikolaj/MikCAD/assets/52234302/81e4c77c-8fc7-48a3-823a-58d3045a2d6e)
|:--:|
|My teapot model in real life - milled on a industrial CNC machine|

# Intersecting objects

![int1](https://github.com/WojcikMikolaj/MikCAD/assets/52234302/584156f3-c9ca-41dd-b445-6e37b6269a2b)
|:--:|
|Before.|

![int3](https://github.com/WojcikMikolaj/MikCAD/assets/52234302/5501c254-bc24-49a5-b3da-2d78e71f9f91)
|:--:|
|Callculated intersection.|

![int4](https://github.com/WojcikMikolaj/MikCAD/assets/52234302/e10df2c8-cc04-40ad-8953-6f8b4af0d748)
|:--:|
|After, view 1.|

![int5](https://github.com/WojcikMikolaj/MikCAD/assets/52234302/163093fa-433f-47dc-affa-569cd61e4f4a)
|:--:|
|After, view 2.|

![in6](https://github.com/WojcikMikolaj/MikCAD/assets/52234302/fa3f91f5-19bc-4f79-8144-b958d6184841)
|:--:|
|After, view 2. Displayed intersection curve.|

![int2](https://github.com/WojcikMikolaj/MikCAD/assets/52234302/141d7114-2f63-432e-979c-78b2c72bb02c)
|:--:|
|After, view3.|

# Other

|![UI](https://user-images.githubusercontent.com/52234302/237031281-fd6f2fd8-a71e-4c40-a6ce-885781d7b2a4.png)|
|:--:|
|Application window with C0 surface.|

![walec2](https://github.com/WojcikMikolaj/MikCAD/assets/52234302/a6d1d678-4f2f-4d79-a360-44c13bff8d7a)
|:--:|
|Cylinder.|

![3tori](https://github.com/WojcikMikolaj/MikCAD/assets/52234302/cff60963-4cdb-4553-89e4-e63d2b20135d)
|:--:|
|Same object, different tessellation levels.|


