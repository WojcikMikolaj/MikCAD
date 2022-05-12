#version 440 core

uniform float overrideEnabled;
uniform vec4 overrideColor;

out vec4 FragColor;

void main()
{
    FragColor = vec4(1, 1, 1, 1);
    FragColor = (1 - overrideEnabled) * FragColor + overrideEnabled * overrideColor * FragColor;
} 