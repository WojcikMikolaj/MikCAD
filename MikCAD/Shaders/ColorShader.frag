#version 440 core

uniform float overrideEnabled;
uniform vec4 overrideColor;

out vec4 FragColor;

uniform vec4 color;

void main()
{
	FragColor = color;
	FragColor = (1 - overrideEnabled) * FragColor + overrideEnabled * overrideColor;
} 