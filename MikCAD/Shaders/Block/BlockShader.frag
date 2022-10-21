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

void main()
{
	FragColor = (1-overrideEnabled) * color + overrideEnabled * overrideColor;
	if (useTexture>0)
	{
		FragColor = texture(texture0, texCoord);
		//FragColor.rgb = texture(texture1, texCoord).rgb/5;
//		FragColor.r = 0;
//		FragColor.g = 1;
//		FragColor.b = 0;
//		FragColor.w =FragColor.w  + texture(texture0, texCoord).w;
	}
	FragColor = (1 - overrideEnabled) * FragColor + overrideEnabled * overrideColor* FragColor;
}