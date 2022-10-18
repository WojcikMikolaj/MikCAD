#version 440 core

uniform float overrideEnabled;
uniform vec4 overrideColor;

out vec4 FragColor;

uniform vec4 color;

uniform float useTexture;
uniform sampler2D texture0;

uniform float ignoreBlack;
in vec2 texCoord;

void main()
{
	FragColor = (1-overrideEnabled) * color + overrideEnabled * overrideColor;
	if (useTexture>0)
	{
		FragColor = texture(texture0, texCoord);
	}
	FragColor = (1 - overrideEnabled) * FragColor + overrideEnabled * overrideColor* FragColor;
}