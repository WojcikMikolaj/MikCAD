#version 440 core

uniform float overrideEnabled;
uniform vec4 overrideColor;

out vec4 FragColor;

in vec4 localPos;
in vec4 vertexColor;// the input variable from the vertex shader (same name and same type)  

void main()
{
    if (localPos.x > 0)
        FragColor = (1-overrideEnabled) * vec4(1, 0, 0, 1) + overrideEnabled * overrideColor;
    if (localPos.y > 0)
        FragColor = (1-overrideEnabled) * vec4(0, 1, 0, 1) + overrideEnabled * overrideColor;
    if (localPos.z > 0)
        FragColor = (1-overrideEnabled) * vec4(0, 1, 1, 1) + overrideEnabled * overrideColor;
    FragColor = (1 - overrideEnabled) * FragColor + overrideEnabled * overrideColor* FragColor;
} 