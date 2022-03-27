#version 440 core

out vec4 color;
uniform vec3 PickingColor;

void main(){
    color = vec4(PickingColor,1.0);
}