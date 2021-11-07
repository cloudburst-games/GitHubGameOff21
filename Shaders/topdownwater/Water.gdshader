shader_type canvas_item;

uniform vec2 scale;
uniform float y_zoom;

uniform sampler2D noise;
uniform vec2 distortion_scale;
uniform float intensity;
uniform float speed;


uniform vec4 water_color : hint_color;

void fragment(){
	vec4 curr_color = texture(TEXTURE,UV); // Get current color of pixel
	if (curr_color.b > curr_color.r && curr_color.b > curr_color.g) {
    //if (curr_color == vec4(0.18,0.56,0.83,1)){ //Color(0.184314, 0.564706, 0.831373)
       // COLOR = vec4(1,0,0,1);
    //COLOR = vec4(0,0,0,1);
	float distortion = texture(noise, UV*scale*distortion_scale + TIME*speed).x;
	distortion -= 0.5;
	COLOR = texture(TEXTURE, UV);
	COLOR.rgb = mix (vec4(distortion * intensity).rgb, water_color.rgb, water_color.a);
	}
	else{
        COLOR = curr_color;
    }
}