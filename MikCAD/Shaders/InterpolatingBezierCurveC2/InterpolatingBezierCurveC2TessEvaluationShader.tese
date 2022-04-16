#version 440 core

layout (isolines, equal_spacing, ccw) in;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

vec3 Horner(vec4 p0, vec4 p1, vec4 p2, vec4 p3, float tau)
{
    vec4 p = p3;
    p = p * tau;
    p = p + p2;
    p = p * tau;
    p = p + p1;
    p = p * tau;
    p = p + p0;
    return p.xyz;
}

void main()
{
    float t = gl_TessCoord.x;
    vec4 p0 = vec4(gl_in[0].gl_Position);
    float chordLength = p0.w;
    vec4 p1 = vec4(gl_in[1].gl_Position);
    vec4 p2 = vec4(gl_in[2].gl_Position);
    vec4 p3 = vec4(gl_in[3].gl_Position);
    p0.w = 1;
    vec4 pos = vec4(Horner(p0, p1, p2, p3, t * chordLength), 1);
    gl_Position = pos * viewMatrix * projectionMatrix;
}