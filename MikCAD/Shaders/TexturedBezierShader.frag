#version 440 core

uniform float overrideEnabled;
uniform vec4 overrideColor;

uniform float useTexture;
uniform sampler2D texture0;

uniform float ignoreBlack;

out vec4 FragColor;

in vec2 texCoord;

void main()
{
    FragColor = vec4(1, 1, 1, 1);
    if (useTexture>0)
    {
        FragColor = texture(texture0, texCoord);
        if(ignoreBlack>0)
        {
            if(FragColor==vec4(0,0,0,0))
            {
                discard;
            }
        }
        else
        {
            if(FragColor!=vec4(0,0,0,0))
            {
                discard;
            }
        }
        
    }
    FragColor = (1 - overrideEnabled) * FragColor + overrideEnabled * overrideColor * FragColor;
    //FragColor = vec4(texCoord.xy, 0,1);
} 