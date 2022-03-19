//#version 330 core
//in vec3 pass_Color;
//out vec4 out_Color;
//
//void main(void) {
//	out_Color = vec4(pass_Color, 1.0);
//}

#version 330 core
out vec4 FragColor;

in vec4 a;
in vec4 vertexColor; // the input variable from the vertex shader (same name and same type)  

void main()
{
	FragColor = vertexColor;
} 