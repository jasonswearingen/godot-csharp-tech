//shader types detailed here: https://docs.godotengine.org/en/latest/tutorials/shading/shading_reference/shaders.html#shader-types
shader_type spatial;
//render modes detailed here: https://docs.godotengine.org/en/latest/tutorials/shading/shading_reference/spatial_shader.html
render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx; 

//default parameters
uniform vec4 albedo : hint_color;
uniform sampler2D texture_albedo : hint_albedo;
uniform float specular;
uniform float metallic;
uniform float roughness : hint_range(0,1);
uniform float point_size : hint_range(0,128);
uniform vec3 uv1_scale;
uniform vec3 uv1_offset;
uniform vec3 uv2_scale;
uniform vec3 uv2_offset;

//demo parameters
uniform float time_scale = 3.0;
uniform vec2 displacementMag = vec2(0.5,0.5);
uniform vec2 displacementSpeed = vec2(0.4,0.333);
uniform float pivot = 0.1;
uniform vec2 waveMagnitude = vec2(0.5,0.05);
uniform float twist = 0.1;
//I can't see anywhere to get the length of a model, so we need that to be a shader input
uniform float modelLength = 6;
//part of fish body (0 to 1) where the swim movements will start being applied
uniform float animationStartLoc = 0.0;
//part of fish body (0 to 1) where swim movements will be fully applied
uniform float animationFullLoc = 2.0;


//vec3 oscilate(vec3 input){
//	return vec3((cos(input.x), -sin(input.z)))
//}

//construct a 2d rotation matrix from the given angle
mat2 rotate2d(float angle){
	return mat2(vec2(cos(angle),-sin(angle)),vec2(sin(angle), cos(angle)));
}

void vertex() {
	
	
	float modelExtent = modelLength/2.0;	
	float extentLocation = (VERTEX.z + modelExtent)/modelLength;
	
//	float animationMask = smoothstep(animationStartLoc,animationFullLoc, - VERTEX.z);	
//
//
//	UV=UV*uv1_scale.xy+uv1_offset.xy;
//
//	float time = TIME * time_scale;
//
//
////	////////////make model move around over time
//	VERTEX.x += cos(time * displacementSpeed.x) * displacementMag.x;
//	VERTEX.y += sin(time* displacementSpeed.y) * displacementMag.y;
//
//	/////////////make pivot around center
//	float pivotAngle = cos(time) * pivot;
//	mat2 rotationMatrix = rotate2d(pivotAngle);
//	VERTEX.xz = rotationMatrix * VERTEX.xz;
//
//	/////////////////wave (horizontal+vertical)	
//	vec3 bodyPos = VERTEX;
//	VERTEX.xy += cos(time + bodyPos.zz) * waveMagnitude.xy;
//
//	///////////////////twist
//	float twistAngle = cos(time+VERTEX.z) * 0.3 * twist;
//	mat2 twistMatrix = rotate2d(twistAngle);
//	VERTEX.xy = twistMatrix * VERTEX.xy;
//
//	///////////////////  mask 
//	//smoothstep()
//	//mask = 1.0-VERTEX.z;
//	COLOR.rgb = vec3(animationMask);  //set vertex color, can be applied in fragment shader (ALBEDO = COLOR.rgb;)
//
	//
	
//
	
	//VERTEX = (INV_PROJECTION_MATRIX * vec4(VERTEX, 1.0)).xyz;
	
	COLOR.rgb = vec3(0);
	
	if(VERTEX.z <= 0.0){
		COLOR.b = VERTEX.z +5.0;
	}else if(VERTEX.z <= 1.0){
		COLOR.r = VERTEX.z;
	}else if(VERTEX.z <= 2.0){
		COLOR.g = VERTEX.z;
	}else if(VERTEX.z <= 3.0){
		COLOR.b = VERTEX.z;
	}
}




void fragment() {
	vec2 base_uv = UV;
	vec4 albedo_tex = texture(texture_albedo,base_uv);
	ALBEDO = albedo.rgb * albedo_tex.rgb;
	ALBEDO = COLOR.rgb;
	METALLIC = metallic;
	ROUGHNESS = roughness;
	SPECULAR = specular;
	//ALBEDO = NORMAL;
	//ALBEDO = VERTEX;
}
