#version 330 core
layout (location = 0) in vec2 coord2f;

//uniform mat4 proj4f;

void main(void) {
    gl_Position = vec4(coord2f, 0, 1);
} 