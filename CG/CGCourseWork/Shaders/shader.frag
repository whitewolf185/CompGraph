#version 330 core

const int DarkBlue = 0;
const int Green = 1;
const int Cyan = 2;
const int LightPaint = 3;
const int None = 4;

struct Material {
	vec3 k_a;
	vec3 k_d;
	vec3 k_s;
	float p;
};

struct Light {
	vec3 intensity;
	vec3 position;
	float attenuation;
};

uniform Material material;
uniform Light light;
uniform vec3 cameraPosition;
uniform int colorMode;

uniform bool animate;
uniform uint curTime;
in vec3 fragCoord;
in vec3 normal;
in vec3 color;

out vec4 fragColor;

mat3 rotationMatrix(float angle)
{
	float s = sin(angle);
	float c = cos(angle);

	return mat3(c,    0.0,  -s,
				0.0,  1.0,  0.0,
				s,    0.0,  c);
}

void main()
{
	if (colorMode == DarkBlue)
	fragColor = vec4(0.0f, 0.2f, 0.4f, 1.0f);
	else if (colorMode == Green)
	fragColor = vec4(0.5f, 1.0f, 0.5f, 1.0f);
	else if (colorMode == Cyan)
	fragColor = vec4(0.0f, 1.0f, 1.0f, 1.0f);
	else if (colorMode == LightPaint)
	fragColor = vec4(light.intensity, 1);
	else {
		vec3 actulLightPos = light.position;
		if (animate){
			actulLightPos = rotationMatrix(float(curTime) / 3000000.0) * light.position;
		}
		//фоновая составляющая
		vec3 I_a = material.k_a;
		//рассеяная составляющая
		vec3 L = actulLightPos - fragCoord;
		vec3 N = normal;
		float cosLN = float(dot(L, N) / (length(L) * length(N)));
		vec3 I_d = material.k_d * light.intensity * max(0, cosLN);
		I_d /= float(pow(length(L), 2) * light.attenuation);
		//отраженная составляющая
		vec3 I_s;
		if (cosLN > 0){
			vec3 cameraDirection = normalize(cameraPosition - fragCoord);
			vec3 normalizedL = normalize(L);
			vec3 R = ((cosLN * N) - normalizedL) + N * cosLN;
			vec3 S = normalize(cameraDirection);
			float cosRSp = float(pow(max(0.0, float(dot(R, S)) / (float(length(R)) * float(length(S)))), float(material.p)));
			I_s = material.k_s * light.intensity * cosRSp;
			I_s /= float(pow(length(L), 2) * light.attenuation);
		} else {
			I_s = vec3(0, 0, 0);
		}
		fragColor = vec4((I_a + I_d + I_s) * color, 1);
	}
}