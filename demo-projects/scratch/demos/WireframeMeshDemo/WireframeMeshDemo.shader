shader_type spatial;
render_mode 
blend_mix
//,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx
,depth_draw_alpha_prepass //needed if drawing alpha
,cull_disabled
,unshaded

;
uniform vec4 albedo : hint_color = vec4(0.5,0.5,0.5,1);
uniform sampler2D texture_albedo : hint_albedo;
//uniform float specular;
//uniform float metallic;
//uniform float roughness : hint_range(0,1);
//uniform float point_size : hint_range(0,128);
uniform vec3 uv1_scale;
uniform vec3 uv1_offset;
uniform vec3 uv2_scale;
uniform vec3 uv2_offset;

uniform vec4 lineAlbedo : hint_color = vec4(0,0,0,1);
//set to false if you want to keep interior edge wireframes
uniform bool removeInteriorEdges = true;
uniform bool cullBackfaces = true;




vec3 getScreenCoord(vec3 vertex, mat4 modelView_Matrix, mat4 proj_Matrix){	
	//get screen coords of vertex.   Z is depth (1 = far, 0 = close)
//can manually set the screenspace coords by setting POSITION = getScreenCoord(VERTEX,MODELVIEW_MATRIX,PROJECTION_MATRIX);
	vec4 vert = vec4(vertex, 1.0);
	vert = modelView_Matrix * vert;
	vert = proj_Matrix * vert;
	return vert.xyz;
}


float get_edge(vec3 color) {
	//code from cybereality
    vec3 deriv = fwidth(color);
    float width = 1.0;
    vec3 threshold = step(deriv * width, color);
    return 1.0 - min(min(threshold.x, threshold.y), threshold.z);
}
vec3 makeBC(vec2 xy){	
	//construct a barrycentric z coord from two of it's components.
	//with barycentric coords we can reconstruct the 3rd axis if we know two.
	return vec3(xy.xy,1.0  - (xy.x+xy.y));
}

float compAdd4(vec4 vec){
	return vec.x+vec.y+vec.z+vec.w;
}
float compAdd3(vec3 vec){
	return vec.x+vec.y+vec.z;
}
float compAdd2(vec2 vec){
	return vec.x+vec.y;
}
float compSub4(vec4 vec){
	return vec.x-vec.y-vec.z-vec.w;
}
float compSub3(vec3 vec){
	return vec.x-vec.y-vec.z;
}
float compSub2(vec2 vec){
	return vec.x-vec.y;
}

bool eq(float val1, float val2, float threshhold){			
	return abs(val1 - val2)<threshhold;	
//	//don't use the following because that causes pixelation
//	vec2 temp = round(vec * (1.0/threshhold));
//	return temp.x==temp.y;
}
bool eq2(vec2 val1, vec2 val2, float threshhold){
	return eq(val1.x,val2.y,threshhold) && eq(val1.y,val2.y,threshhold);
}
bool eq3(vec3 val1, vec3 val2, float threshhold){
	return eq(val1.x,val2.y,threshhold) && eq(val1.y,val2.y,threshhold) && eq(val1.z,val2.z,threshhold);
}

bool compEq2(vec2 vec, float threshhold){			
	return abs(compSub2(vec))<threshhold;	
//	//don't use the following because that causes pixelation
//	vec2 temp = round(vec * (1.0/threshhold));
//	return temp.x==temp.y;
}
bool compEq3(vec3 vec, float threshhold){			
	return (abs(compSub2(vec.xy))<threshhold) && (abs(compSub2(vec.xz))<threshhold)&& (abs(compSub2(vec.yz))<threshhold);		
}
float compMin3(vec3 vec){
	return min(min(vec.x,vec.y),vec.z);
}
float compMin2(vec2 vec){
	return min(vec.x,vec.y);
}



vec3 getBaryCoords(vec4 chan1To4, float chan5){
	//get the barycentric coordinates in the face of the current pixel 
	
	
	
	//works with 5 color channels written to by the vertex shader
	
	
	if(chan5 > 0.0 ){		
		//5th channel used, this usually won't happen, but on perhaps 1% of verts it will be set.		
		
		if(compAdd2(chan1To4.rg) ==0.0){			
			return vec3(chan1To4.ba, chan5);
		}else if(compAdd2(chan1To4.gb) ==0.0){			
			return vec3(chan1To4.ar, chan5);
		}else if(compAdd2(chan1To4.ba) ==0.0){			
			return vec3(chan1To4.rg, chan5);
		}else if(compAdd2(chan1To4.ar) ==0.0){			
			return vec3(chan1To4.gb, chan5);
		}else if(compAdd2(chan1To4.rb) ==0.0){			
			return vec3(chan1To4.ga, chan5);
		}else if(compAdd2(chan1To4.ga) ==0.0){			
			return vec3(chan1To4.br, chan5);
		}
		//unknown (should never get here)
		return vec3(1);
	}
	
	//three of the 4 "normal" color chanels is used.  R,G,B,A.  need to find which 3 of the 4
	if(chan1To4.a==0.0){
		return chan1To4.rgb;
	}else if(chan1To4.b==0.0){
		return chan1To4.rga;
	}else if(chan1To4.g==0.0){
		return chan1To4.rba;
	}else if(chan1To4.r==0.0){
		return chan1To4.gba;
	}
	
	//should never get here	
	return vec3(1);
}



float aaStep(float a, float b){
	//smoothly interpolates step.
	float afwidth = fwidth(b) * 0.5;
	return smoothstep(a - afwidth, a + afwidth, b);
}
vec3 aaStep3(vec3  a, vec3 b){
	//smoothly interpolates step.
	vec3 afwidth = fwidth(b) * 0.5;
	return smoothstep(a - afwidth, a + afwidth, b);
}

//our normal COLOR channel has various data encoded into it from the MeshDataTool.  
//the vertex() function parses that and creates and encoded BC coords in 5 channels to be interpolated by the fragment() shader.
//Those 5 channels are RED,GREEN,BLUE,ALPHA,ALPHABLACK(TRANSPARENT).  see vertex() shader for details on encoding.
varying vec4 _bcChannels1To4;
varying float _bcChannel5;

//provides the location in screen space
varying vec3 _screenCoord;

//exposed for debugging purposes:  the vertex priority in regards to inner edge removal
varying float _vertPriority;


void vertex() {
	UV=UV*uv1_scale.xy+uv1_offset.xy;
	_screenCoord = getScreenCoord(VERTEX,MODELVIEW_MATRIX, PROJECTION_MATRIX);
	
	
	/////////////////////////////////////////////////////////////
	////////////////  READ DATA ENCODED INTO VERTEX COLOR (written by Godot's MeshDataTool)
	/////////////////////////////////////////////////////////////
	_bcChannels1To4 = vec4(0,0,0,0);			
	_bcChannel5 = 0.0;
	
	
	//get encoded channel
	vec4 encColor = roundEven(COLOR * 10.0);
	int colorChannel = int(encColor.r);
	switch(colorChannel){
		case 2:
			_bcChannels1To4 = vec4(1,0,0,0);
			break;
		case 4:
			_bcChannels1To4 = vec4(0,1,0,0);
			break;
		case 6:
			_bcChannels1To4 = vec4(0,0,1,0);
			break;
		case 8:
			_bcChannels1To4 = vec4(0,0,0,1);
			break;
		case 10:
			_bcChannel5 =1.0;
			break;
		default:
			_bcChannels1To4 = vec4(1);
			_bcChannel5 =1.0;
			break;
	}
	//_edgePriority = vec3(0);
	
	//handle removal of interrior edges.  does so by overwhelming the color channel for that vertex, making opposite line very faint.
	if(removeInteriorEdges==true){
		int greenChan = int(encColor.g);
		//bool isInterriorEdge = (greenChan& 1) !=0;  //for some reason this bitmask doesn't work on 1% of edges... so using next line instead
		bool overwhelmOppositeEdge = greenChan==1;
		if(overwhelmOppositeEdge){
			//_bcChannels *= 1000000.0;
			//_edgePriority = vec3(10);// 100000.0f;
			
			//edges adjacent to this vertex will be shown
			_vertPriority = 100000.0;
		}else{
			_vertPriority =1.0;
		}
	}else{
		//all edges are important
		_vertPriority = 1.0;
	}
	_bcChannels1To4 *=_vertPriority;
	_bcChannel5 *=_vertPriority;
	
	
}

uniform float innerMargin  : hint_range(0,1) = 0.0;
uniform float outerMargin  : hint_range(0,1) = 0.0;
uniform float lineWidth  : hint_range(0,1) = 0.05;
uniform bool aaLines = false;
uniform float banding: hint_range(0,1) =0.0;

void fragment() {
	
	if(cullBackfaces && FRONT_FACING==false){
		discard;
	}
	
	vec2 base_uv = UV;
	vec4 albedo_tex = texture(texture_albedo,base_uv);
	vec4 _textureColor = albedo * albedo_tex;
	vec3 _lineColor =  mix(ALBEDO,lineAlbedo.rgb, lineAlbedo.a);
	vec4 _finalColor = _textureColor;
	
	
	//dist of pixel from camera
	float dist = 1.0/_screenCoord.z;
		
	//get barycentric coordinates of the current pixel
	vec3 bc = getBaryCoords(_bcChannels1To4, _bcChannel5);	
	
	/////////////////////////////////////////////////////////////
	////////////////  WIREFRAME RENDERING TECHNIQUE
	/////////////////////////////////////////////////////////////
	{
		float bcDistFromEdge = compMin3(bc);
		
		if(bcDistFromEdge<=(1.0 -innerMargin)){
			if(banding != 0.0){
				bcDistFromEdge =bcDistFromEdge - (round(bcDistFromEdge * (banding * 100.0)) / (banding * 100.0) );
			}			
			//"Anti-Aliased Edges" :  smooth edge lines and inner edge removal, but does not scale thickness by triangle size
			float edge = 1.0 - (aaLines? aaStep(lineWidth, bcDistFromEdge):step(lineWidth, bcDistFromEdge));
			if(edge == 1.0 && bcDistFromEdge > outerMargin ){
				//ALBEDO = vec3(0);							
				//ALBEDO *= lineAlbedo.rgb * lineAlbedo.a;
				_finalColor = vec4(_lineColor,1);
				
			}else{
				//ALBEDO = bc;
				//ALBEDO = vec3(0.5);
			}
		}else{
		}
		
		ALBEDO = _finalColor.rgb;
		ALPHA = _finalColor.a;
		//ALBEDO = bc;
		//ALBEDO = _bcChannels.rgb;
		//ALBEDO = COLOR.rgb;
		//ALBEDO = vec3(tempEdgePriority/10.0);
		//ALBEDO = 1.0/_screenCoord;
	}
	
	//////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////
	//		//VARIOUS TECHNIQUES / NOTES HERE
	//////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////
	{
//		
//	//basic wireframe, width determined by triangle size
//	{
//		if( (bc.r < threshhold || bc.g < threshhold || bc.b < threshhold)){
//			ALBEDO = vec3(0);
//		}else{
//			ALBEDO = bc;
//		}		
//	}
//		
//	//wireframe with fixed width, based on distance from camera.  from: https://tchayen.github.io/wireframes-with-barycentric-coordinates/
//	{	
////////////////  THIS WAY DOES NOT REMOVE INTERRIOR, BUT LINE WIDTHS SAME	
//		float adjustedLineWidth = 5.0 * dist;
//		vec3 lineColor = vec3(0);
//		vec3 d = fwidth(bc);
//		vec3 f = aaStep3(d*adjustedLineWidth,bc);
//
//		//commenting this part out in favor of if/else in case we want to do extra logic on wire condition
//		{
//			//float result = min(min(f.x,f.y),f.z);
//			//ALBEDO = min(vec3(result),bc);
//		}
//		{
//			if(f == vec3(1)){
//				ALBEDO = bc;
//			}else{
//				ALBEDO = vec3(0);
//			}
//		}
//	}
//
//	//experiments from https://www.pressreader.com/australia/net-magazine/20171005/282853666142801
//	{
//		//calculate our pixels distance from any edge
//		float bcDistFromEdge = compMin3(bc);
//
		
//		//"Anti-Aliased Edges" :  smooth edge lines and inner edge removal, but does not scale thickness by triangle size
//		lineWidth = 0.05;
//		float edge = 1.0 - aaStep(lineWidth, bcDistFromEdge);
//		if(edge == 1.0 ){
//			ALBEDO = vec3(0);							
//		}else{
//			ALBEDO = bc;
//			//ALBEDO = vec3(0.5);
//		}
		
	
//		//"See-Through Materials"
//		//need to set render_modes:  ,depth_draw_alpha_prepass,cull_disabled
//		ALBEDO = vec3(0);		
//		if(edge == 1.0 ){
//			ALBEDO = vec3(0);					
//			if(FRONT_FACING==false){
//				//ALBEDO = vec3(0.25);
//				ALPHA = 0.25;
//			}
//		}else{
//			ALPHA = edge;	
//		}
//
//		//"Inner Edge Removal"
//		ALPHA = 1.0;
//		ALBEDO = vec3(0.75);
//		if(edge == 1.0 ){
//			ALBEDO = vec3(0);
//		}
//
//
//	}
		
	}
}
