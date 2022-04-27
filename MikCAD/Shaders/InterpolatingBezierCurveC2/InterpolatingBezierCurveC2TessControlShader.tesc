#version 440 core

layout (vertices = 4) out;
uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

void main()
{
    gl_TessLevelOuter[0] = 1;
    vec4 posNDC;
    float minX = 1;
    float maxX = -1;
    float minY = 1;
    float maxY = -1;
    for(int i=0; i<4; i++)
    {
        vec4 pos =gl_in[gl_InvocationID+i].gl_Position;
        posNDC= pos * viewMatrix * projectionMatrix;
        posNDC = posNDC / posNDC.w;
        if (posNDC.x < minX)
        minX = posNDC.x;
        if (posNDC.y < minY)
        minY = posNDC.y;
        if (posNDC.x > maxX)
        maxX = posNDC.x;
        if (posNDC.y > maxY)
        maxY = posNDC.y;
    }
    gl_TessLevelOuter[1]= int(max(32, 256 * (maxX - minX) * (maxY - minY) / 4));
    
    gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;
}