#version 440 core

uniform float overrideEnabled;
uniform vec4 overrideColor;

out vec4 FragColor;

in vec4 a;
in vec4 vertexColor; // the input variable from the vertex shader (same name and same type)  

void main()
{
	FragColor = (1-overrideEnabled) * vertexColor + overrideEnabled * overrideColor;
	FragColor = (1 - overrideEnabled) * FragColor + overrideEnabled * overrideColor* FragColor;
} 