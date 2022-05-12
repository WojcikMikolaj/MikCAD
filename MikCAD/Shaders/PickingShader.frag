#version 440 core

uniform float overrideEnabled;
uniform vec4 overrideColor;

out vec4 FragColor;
uniform vec3 PickingColor;

void main(){
    FragColor = vec4(PickingColor,1.0);
    FragColor = (1 - overrideEnabled) * FragColor + overrideEnabled * overrideColor* FragColor;
}