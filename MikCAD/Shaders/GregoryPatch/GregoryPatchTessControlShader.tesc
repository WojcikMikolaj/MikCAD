#version 440 core

layout (vertices = 20) out;
uniform int UTessLevels;
uniform int VTessLevels;

void main()
{
    gl_TessLevelInner[0] = UTessLevels-1;
    gl_TessLevelOuter[1] = UTessLevels-1;
    gl_TessLevelOuter[3] = UTessLevels-1;

    gl_TessLevelInner[1] = VTessLevels-1;
    gl_TessLevelOuter[0] = VTessLevels-1;
    gl_TessLevelOuter[2] = VTessLevels-1;

    gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;
}