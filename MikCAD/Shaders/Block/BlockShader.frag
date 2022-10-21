#version 440 core

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
const vec3 LightColor = vec3(0.5,0.5,0.5);
const vec3 LightAmbient = vec3(0.2,0.2,0.2);

uniform float ka;
uniform float ks;
uniform float kd;
uniform float m;

const ivec3 off = ivec3(-1,0,1);

void main()
{
	FragColor = (1-overrideEnabled) * color + overrideEnabled * overrideColor;
	if (useTexture>0)
	{

		vec4 wave = texture(unit_wave, tex_coord);
		float s11 = wave.x;
		float s01 = textureOffset(unit_wave, tex_coord, off.xy).x;
		float s21 = textureOffset(unit_wave, tex_coord, off.zy).x;
		float s10 = textureOffset(unit_wave, tex_coord, off.yx).x;
		float s12 = textureOffset(unit_wave, tex_coord, off.yz).x;
		vec3 va = normalize(vec3(size.xy,s21-s01));
		vec3 vb = normalize(vec3(size.yx,s12-s10));
		vec4 bump = vec4( cross(va,vb), s11 );
		
		vec3 normal = normalize(vec3(dFdx(texture(texture1, texCoord).r), 1.0f, dFdy(texture(texture1, texCoord).r)));
		normal.x = dFdx(texture(texture1, texCoord).r);
		normal.y = dFdx(texture(texture1, texCoord).r);
		normal.z = dFdx(texture(texture1, texCoord).r);
		
		vec3 surfaceColor = texture(texture0, texCoord).rgb;
		
		vec4 lightPos = LightPos;
		vec3 viewVec = normalize(cameraPos).xyz;
		vec3 lightVec = normalize(lightPos.xyz - worldPos.xyz);
		vec3 halfVec = normalize(lightVec + viewVec);

		float spec = pow(max(dot(normal, halfVec), 0.0), m);
		vec3 specular = LightColor * spec;
		float diff = max(dot(normal, lightVec), 0.0);
		FragColor = vec4((LightAmbient * ka + kd * LightColor * diff) * surfaceColor + specular * ks,1);
		
		FragColor.w = FragColor.x;
		FragColor.x = abs(normal.x);
		FragColor.y = abs(normal.y);
		FragColor.z = abs(normal.z);
	}
	FragColor = (1 - overrideEnabled) * FragColor + overrideEnabled * overrideColor* FragColor;
}