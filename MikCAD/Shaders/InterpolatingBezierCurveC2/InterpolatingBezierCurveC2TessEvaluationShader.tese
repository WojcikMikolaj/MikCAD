#version 440 core

layout (isolines, equal_spacing, ccw) in;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

vec3 Casteljau(float u, vec4 p0, vec4 p1, vec4 p2, vec4 p3, int size)
{
    if(size>4)
    size=4;
    int i=0;
    int j=0;
    vec4 arr[4] = vec4[](p0,p1,p2,p3);
    for(i=1; i<size; i++)
    {
        for(j=0; j<size-i; j++)
        {
            arr[j] = arr[j] * (1.-u) + arr[j+1] * u;
        }
    }

    vec4 p = arr[0];
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
    vec4 pos = vec4(Casteljau(t, p0, p1, p2, p3, 4), 1);
    gl_Position = pos * viewMatrix * projectionMatrix;
}