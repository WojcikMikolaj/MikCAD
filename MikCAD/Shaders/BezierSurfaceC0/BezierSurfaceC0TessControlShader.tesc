#version 440 core

layout (vertices = 16) out;
uniform int UTessLevels;
uniform int VTessLevels;

void main()
{
    gl_TessLevelInner[0] = UTessLevels;
    gl_TessLevelOuter[1] = UTessLevels;
    gl_TessLevelOuter[3] = UTessLevels;

    gl_TessLevelInner[1] = VTessLevels;
    gl_TessLevelOuter[0] = VTessLevels;
    gl_TessLevelOuter[2] = VTessLevels;

    gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;
}