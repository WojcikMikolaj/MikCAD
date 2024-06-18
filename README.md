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
 

 |![UI](https://user-images.githubusercontent.com/52234302/237031281-fd6f2fd8-a71e-4c40-a6ce-885781d7b2a4.png)|
 |:--:|
 |Application window with C0 surface.|
