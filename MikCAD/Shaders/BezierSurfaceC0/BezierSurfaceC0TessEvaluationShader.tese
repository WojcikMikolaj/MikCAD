#version 440 core

layout (quads, equal_spacing, ccw) in;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

uniform int HorizontalPatchesCount;
uniform int VerticalPatchesCount;

out vec2 texCoord;

vec4 Casteljau(float u, vec4 p0, vec4 p1, vec4 p2, vec4 p3, int size)
{
    if (size>4)
    size=4;
    int i=0;
    int j=0;
    vec4 arr[4] = vec4[](p0, p1, p2, p3);
    for (i=1; i<size; i++)
    {
        for (j=0; j<size-i; j++)
        {
            arr[j] = arr[j] * (1.-u) + arr[j+1] * u;
        }
    }

    vec4 p = arr[0];
    return vec4(p.xyz, 1);
}

void main()
{
    float u = gl_TessCoord.x;
    float v = gl_TessCoord.y;
    //first row
    vec4 p0 = vec4(gl_in[0].gl_Position);
    vec4 p1 = vec4(gl_in[1].gl_Position);
    vec4 p2 = vec4(gl_in[2].gl_Position);
    vec4 p3 = vec4(gl_in[3].gl_Position);
    //second row
    vec4 p4 = vec4(gl_in[4].gl_Position);
    vec4 p5 = vec4(gl_in[5].gl_Position);
    vec4 p6 = vec4(gl_in[6].gl_Position);
    vec4 p7 = vec4(gl_in[7].gl_Position);
    //third row
    vec4 p8 = vec4(gl_in[8].gl_Position);
    vec4 p9 = vec4(gl_in[9].gl_Position);
    vec4 p10 = vec4(gl_in[10].gl_Position);
    vec4 p11 = vec4(gl_in[11].gl_Position);
    //fourth row
    vec4 p12 = vec4(gl_in[12].gl_Position);
    vec4 p13 = vec4(gl_in[13].gl_Position);
    vec4 p14 = vec4(gl_in[14].gl_Position);
    vec4 p15 = vec4(gl_in[15].gl_Position);

    vec4 inter1 = Casteljau(v, p0, p4, p8, p12, 4);
    vec4 inter2 = Casteljau(v, p1, p5, p9, p13, 4);
    vec4 inter3 = Casteljau(v, p2, p6, p10, p14, 4);
    vec4 inter4 = Casteljau(v, p3, p7, p11, p15, 4);
    
    vec4 pos = Casteljau(u, inter1, inter2, inter3, inter4, 4);
    gl_Position = pos * viewMatrix * projectionMatrix;
    
    float unum = gl_PrimitiveID / VerticalPatchesCount;
    float vnum = gl_PrimitiveID % VerticalPatchesCount;
    float patchUSize = 1.0f/HorizontalPatchesCount;
    float patchVSize = 1.0f/VerticalPatchesCount;

    float startU = unum/HorizontalPatchesCount;
    float startV = vnum/VerticalPatchesCount;
    
    texCoord = vec2(startU + v*patchUSize, startV +u*patchVSize);  
}