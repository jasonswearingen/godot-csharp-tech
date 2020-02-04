//shader types detailed here: https://docs.godotengine.org/en/latest/tutorials/shading/shading_reference/shaders.html#shader-types
shader_type spatial;
//render modes detailed here: https://docs.godotengine.org/en/latest/tutorials/shading/shading_reference/spatial_shader.html
render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx; 

//default parameters
uniform vec4 albedo : hint_color = vec4(1);
uniform sampler2D texture_albedo : hint_albedo;
uniform float specular = 0.5;
uniform float metallic = 0.0;
uniform float roughness : hint_range(0,1) = 1;
//uniform float point_size : hint_range(0,128);
uniform vec3 uv1_scale = vec3(1);
uniform vec3 uv1_offset = vec3(0);
uniform vec3 uv2_scale = vec3(1);
uniform vec3 uv2_offset = vec3(0);

//////////////////////////////demo parameters
//how fast you want the fake swimming to move
uniform float speed = 3.0;
//modify how "powerful" the fake swim to be
uniform float power=0.5;
//offset time for each instance so they don't look exactly the same
uniform float offset = 0.0;


//let the model fake move (translate) in the x/y a bit
const vec2 displacementMag = vec2(0.1,0.1);
//how fast to let the model fake move
const vec2 displacementSpeed = vec2(0.4,0.333);
//pivot around center (y axis) by this many radians
const float pivot = 0.05;
const vec2 waveMagnitude = vec2(0.5,0.05);
//pivot around z axis this many radians
const float twist = 0.51;
//I can't see anywhere to get the length of a model, so we need that to be a shader input
const float modelLength = 6.0;
//part of fish body (0 to 1) where the swim movements will start being applied.  increase to make head stiffer
const float animationStartBias = 0.2;
//part of fish body (0 to 1) where swim movements will be fully applied.  increase to make body stiffer
const float animationFullBias = 1.5;//0.75;


//construct a 2d rotation matrix from the given angle
mat2 rotate2d(float angle){
	return mat2(vec2(cos(angle),-sin(angle)),vec2(sin(angle), cos(angle)));
}

void vertex() {	
	//vec3 worldPos = vec3(WORLD_MATRIX[3][0],WORLD_MATRIX[3][1],WORLD_MATRIX[3][2]);		
	UV=UV*uv1_scale.xy+uv1_offset.xy;
	float time = (TIME * speed) + offset;	
	float modelExtent = modelLength/2.0;	//half is negative
	float currentModelZLoc = (VERTEX.z + modelExtent)/modelLength;	


////	////////////make model move around over time
//	VERTEX.x += cos(time * displacementSpeed.x) * displacementMag.x;
//	VERTEX.y += sin(time* displacementSpeed.y) * displacementMag.y;

	/////////////make pivot around center
	float pivotAngle = cos(time) * pivot * power;
	mat2 rotationMatrix = rotate2d(pivotAngle);
	VERTEX.xz = rotationMatrix * VERTEX.xz;
	
	
	//weigh vertex swim animations based on where we are on the model	
	float animationMask = smoothstep(animationStartBias,animationFullBias, currentModelZLoc);
	//COLOR.rgb = vec3(animationMask);  //can enable this line to debug animationMask issues (set albedo to this color in the fragment() shader function)

	/////////////////wave (horizontal+vertical)	
	vec3 bodyPos = VERTEX;
	VERTEX.xy += cos(time - bodyPos.zz) * waveMagnitude.xy * animationMask * power; 

	///////////////////twist ("corkscrew" effect)
	float twistAngle = cos(time-VERTEX.z) * 0.3 * twist;
	mat2 twistMatrix = rotate2d(twistAngle);
	VERTEX.xy =mix(VERTEX.xy, twistMatrix * VERTEX.xy, animationMask); //can't multiplay animation mask with matrix, instead use "mix()" fcn

}




void fragment() {
	vec2 base_uv = UV;
	vec4 albedo_tex = texture(texture_albedo,base_uv);
	ALBEDO = albedo.rgb * albedo_tex.rgb;
	//ALBEDO = COLOR.rgb;  //can enable this line to debug animationMask issues (set vertex color in the vertex() shader function)
	METALLIC = metallic;
	ROUGHNESS = roughness;
	SPECULAR = specular;
	//ALBEDO = NORMAL;
	//ALBEDO = VERTEX;
}
