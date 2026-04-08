shader_type canvas_item;

uniform sampler2D data_texture : filter_nearest;
uniform sampler2D atlas_texture : filter_nearest;
uniform int atlas_columns = 4;
uniform float inner_growth = 0.0;

varying float v_integrity;
varying float v_light;
varying flat int v_mat_index;
varying vec2 v_local_uv;

void vertex() {
	// pass data to fragment:
	vec4 data = texture(data_texture, UV);
	v_integrity = data.g * 2.0;
	v_light = data.b;
	v_mat_index = int(data.r * 255.0);

	// calc local UV for the tile:
	int v_type = int(round(COLOR.r * 255.0));
	v_local_uv = vec2(mod(float(v_type), 2.0), 1.0 - floor(float(v_type) / 2.0));
	
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

	// get atlas texture:
	float s = 1.0 / float(atlas_columns);
	vec2 atlas_offset = vec2(
		float(v_mat_index % atlas_columns) * s,
		float(v_mat_index / atlas_columns) * s
	);
	vec4 material_color = texture(atlas_texture, atlas_offset + (v_local_uv * s));
	material_color.rgb = material_color.rgb * v_light;

	
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
		COLOR = vec4(crack_albedo.rgb * v_light, 1.0);
	}
	else {
		COLOR = material_color;
	}
}