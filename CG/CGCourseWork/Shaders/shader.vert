#version 330 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 inColor;
layout (location = 2) in vec3 inNormal;

out vec3 fragCoord;
out vec3 normal;
out vec3 color;

uniform mat4 transformation;

void main()
{
    gl_Position = transformation * vec4(position.x, position.y, position.z, 1.0);

    fragCoord = position;
    color = inColor;
    normal = inNormal;
}