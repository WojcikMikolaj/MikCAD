#version 440 core

uniform float showNormals;

uniform float overrideEnabled;
uniform vec4 overrideColor;

out vec4 FragColor;

uniform vec4 color;

uniform float useTexture;
uniform sampler2D texture0;
uniform sampler2D texture1;

uniform float ignoreBlack;


in vec2 texCoord;
in vec4 worldPos;

uniform vec4 cameraPos;

uniform vec4 LightPos;
uniform vec3 LightColor;
uniform vec3 LightAmbient;

uniform float ka;
uniform float ks;
uniform float kd;
uniform float m;

const ivec3 off = ivec3(-1, 0, 1);

vec3 NormalMapping(vec3 normal)
{
    return mul(transpose(mat3(vec3(1,0,0), vec3(0,0,-1), vec3(0,1,0))), normal);
}

void main()
{
    FragColor = (1-overrideEnabled) * color + overrideEnabled * overrideColor;
    if (useTexture>0)
    {
        vec2 h = 1.0/textureSize(texture1, 0);
        float L = texture(texture1, texCoord + vec2(-h.x, 0)).x;
        float R = texture(texture1, texCoord + vec2(h.x, 0)) .x;
        float T = texture(texture1, texCoord + vec2(0, -h.y)).x;
        float B = texture(texture1, texCoord + vec2(0, h.y)).x;
        
        vec3 tangential = vec3(2*h.x, 0, R-L);
        vec3 bitangential = vec3(0, 2*h.y, T-B);
        
        vec3 normal = NormalMapping(normalize(cross(bitangential,tangential)));
        normal.xz = -normal.xz;

        vec3 surfaceColor = texture(texture0, texCoord).rgb;

        vec4 lightPos = LightPos;
        vec3 viewVec = normalize(cameraPos).xyz;
        vec3 lightVec = normalize(lightPos.xyz - worldPos.xyz);
        vec3 halfVec = normalize(lightVec + viewVec);

        float spec = pow(max(dot(normal, halfVec), 0.0), m);
        vec3 specular = LightColor * spec;
        float diff = max(dot(normal, lightVec), 0.0);
        FragColor = vec4((LightAmbient * ka + kd * LightColor * diff) * surfaceColor + specular * ks, 1);

        if(showNormals > 0)
        {
            FragColor.xyz = normal.xyz;
        }
    }
    FragColor = (1 - overrideEnabled) * FragColor + overrideEnabled * overrideColor* FragColor;
}