#version 440 core

layout (location = 0) in vec3 in_Position;
layout (location=1) in vec2 in_Tex;

out vec2 texCoord;

uniform sampler2D texture1;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

void main()
{
    vec4 interPos = vec4(in_Position, 1.0);
    if (interPos.z>2)
    	interPos.z = texture(texture1, in_Tex).r;
    gl_Position = interPos* modelMatrix * viewMatrix * projectionMatrix;
    gl_PointSize = 5;

    texCoord = in_Tex;
}