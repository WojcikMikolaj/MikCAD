#version 440 core

layout (isolines, equal_spacing, ccw) in;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

vec3 BSplineEval(float u, vec4 p0, vec4 p1, vec4 p2, vec4 p3)
{
    int i=0;
    int j=0;
    
    vec4 points[4] = vec4[](p0,p1,p2,p3);
    vec4 d[4] = vec4[](p0,p1,p2,p3);
    float alpha[4] = float[](u,u,u,u);
    for(i=1; i<4; i++)
    {
        for(j=i; j<4;j++)
        {
            d[j] = (1-u) * points[j-1] + u*d[j];
        }
        for(j=0; j<4; j++)
            points[j] = d[j];
    }
    return d[3].xyz;
}

void main()
{
    float u = gl_TessCoord.x;
    vec4 p0 = vec4( gl_in[0].gl_Position );
    vec4 p1 = vec4( gl_in[1].gl_Position );
    vec4 p2 = vec4( gl_in[2].gl_Position );
    vec4 p3 = vec4( gl_in[3].gl_Position );
    vec4 pos = vec4( BSplineEval( u, p0, p1, p2, p3), 1.);
    gl_Position = pos * viewMatrix * projectionMatrix;
}