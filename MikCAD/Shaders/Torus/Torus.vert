#version 440 core

layout (location=0) in vec3 in_Position;
layout (location=1) in vec2 in_Tex;

out vec2 texCoord;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

void main()
{
	gl_Position = vec4(in_Position, 1.0) * modelMatrix * viewMatrix * projectionMatrix;
	//gl_Position = vec4(in_Position, 1.0);
	texCoord = in_Tex;
}