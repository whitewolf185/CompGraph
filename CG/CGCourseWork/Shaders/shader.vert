#version 330 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 inColor;
layout (location = 2) in vec3 inNormal;

out vec3 fragCoord;
out vec3 normal;
out vec3 color;

uniform mat4 transformation;

uniform bool animate;
uniform uint curTime;
uniform vec3 scale;

mat4 rotationMatrix(float angle)
{
    float s = sin(angle);
    float c = cos(angle);

    return mat4(c,    0.0,  s,  0.0,
                0.0,  1.0,  0.0, 0.0,
                -s,    0.0,  c,   0.0,
                0.0,  0.0,  0.0, 1.0);
}

void main()
{
    mat4 transMat = rotationMatrix(float(curTime)/ 3000000.0);
    if (animate){
        gl_Position = transformation * transMat * vec4(position.x, position.y, position.z, 1.0);
    } else {
        gl_Position = transformation * vec4(position.x, position.y, position.z, 1.0);
    }
    
    fragCoord = position;
    color = inColor;
    normal = inNormal;
}