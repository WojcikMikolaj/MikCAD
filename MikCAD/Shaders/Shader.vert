#version 330 core

layout (location = 0) in vec3 in_Position;
//in vec3 in_Color;  
out vec4 vertexColor;
out vec4 a;
uniform mat4 projectionMatrix;
uniform mat4 sceneScaleMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

void main()
{
	//gl_Position = vec4(in_Position, 1.0);
	gl_Position = vec4(in_Position, 1.0) * modelMatrix * sceneScaleMatrix * viewMatrix * projectionMatrix;
	//gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(in_Position, 1.0);
	vertexColor = vec4(1.0,0.0,0.0,1.0);//in_Color;
}