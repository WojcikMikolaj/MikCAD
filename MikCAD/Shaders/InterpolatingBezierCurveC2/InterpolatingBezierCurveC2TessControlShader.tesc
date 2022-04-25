#version 440 core

layout (vertices = 4) out;
uniform int tessLevels;

void main()
{
    gl_TessLevelOuter[0] = 1;
    gl_TessLevelOuter[1] = int(gl_InvocationID%4!=0) * tessLevels + int(gl_InvocationID%4==0) * max(64, 1024 * int(gl_in[gl_InvocationID].gl_Position.w));
    gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;
}