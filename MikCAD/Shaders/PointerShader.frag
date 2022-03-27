#version 440 core
out vec4 FragColor;

in vec4 localPos;
in vec4 vertexColor;// the input variable from the vertex shader (same name and same type)  

void main()
{
    if (localPos.x > 0)
        FragColor = vec4(1, 0, 0, 1);
    if (localPos.y > 0)
        FragColor = vec4(0, 1, 0, 1);
    if (localPos.z > 0)
        FragColor = vec4(0, 1, 1, 1);
} 