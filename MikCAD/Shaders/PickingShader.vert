#version 330 core

layout (location = 0) in vec3 in_Position;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

void main(){
    gl_PointSize = 10;
    gl_Position = vec4(in_Position, 1.0) * modelMatrix * viewMatrix * projectionMatrix;
}