shader_type canvas_item;

uniform sampler2D data_texture : filter_nearest;
uniform sampler2D atlas_texture : filter_nearest;
uniform int atlas_columns = 4;
uniform float inner_growth = 0.0;
uniform float max_layer_width = 1024.0;
uniform float total_layers = 1024.0;

varying float v_integrity;
varying flat int v_mat_index;
varying vec2 v_local_uv;
varying vec4 v_corner_lights;

void vertex() {
	// pass data to fragment:
	vec4 data = texture(data_texture, UV);
	v_integrity = data.g * 2.0;
	v_mat_index = int(data.r * 255.0);

	// calc local UV for the tile:
	int v_type = int(round(COLOR.r * 255.0));
	v_local_uv = vec2(mod(float(v_type), 2.0), 1.0 - floor(float(v_type) / 2.0));
	
	// smooth lighting:
	float tileCount = round(COLOR.g * max_layer_width);
	vec2 tile_step = vec2(1.0 / tileCount, 1.0 / total_layers);

	float l00 = texture(data_texture, UV).b;
	float l10 = texture(data_texture, vec2(fract(UV.x + tile_step.x), UV.y)).b;
	float l01 = texture(data_texture, vec2(UV.x, clamp(UV.y + tile_step.y, 0.0, 1.0))).b;
	float l11 = texture(data_texture, vec2(fract(UV.x + tile_step.x), clamp(UV.y + tile_step.y, 0.0, 1.0))).b;
	v_corner_lights = vec4(l01, l11, l00, l10);
	
	// shrink:
	if (v_integrity > 1.0) {
		vec2 outward_dir = normalize(VERTEX);
		vec2 side_dir = vec2(-outward_dir.y, outward_dir.x);
		
		float damage = (1.0 - inner_growth) * 8.0;
		vec2 signs = ((v_local_uv * 2.0 - 1.0) - vec2(0.0, 1)) * damage;
		
		VERTEX += outward_dir * signs.y;
	}
}

void fragment() {
	// if destroyed, do not render
	if (v_integrity <= 0.0) discard;

	float light_bottom = mix(v_corner_lights.x, v_corner_lights.y, v_local_uv.x);
	float light_top = mix(v_corner_lights.z, v_corner_lights.w, v_local_uv.x);
	float smooth_light = mix(light_bottom, light_top, v_local_uv.y);

	// get atlas texture:
	float s = 1.0 / float(atlas_columns);
	vec2 atlas_offset = vec2(
		float(v_mat_index % atlas_columns) * s,
		float(v_mat_index / atlas_columns) * s
	);
	
	vec4 material_color = texture(atlas_texture, atlas_offset + (v_local_uv * s));
	material_color.rgb = material_color.rgb * smooth_light;
	
	if (v_integrity <= 1.0) {
		// get mine overlay texture:
		float end = (float(atlas_columns) - 1.0) * s;
		vec2 break_offset = vec2(end);
		
		vec4 breaking_color = texture(atlas_texture, break_offset + (v_local_uv * s));
		float top_dist = distance(v_local_uv, vec2(0.5, -0.25));
		top_dist *= top_dist * 1.5;
		float damage = 1.0 - v_integrity;
		float crack_percent = clamp(damage * 2.0 - top_dist, 0.0, 1.0) * breaking_color.a;

		// mix
		vec4 crack_albedo = mix(material_color, breaking_color, crack_percent);
		COLOR = vec4(crack_albedo.rgb * smooth_light, 1.0);
	}
	else {
		COLOR = material_color;
	}
}