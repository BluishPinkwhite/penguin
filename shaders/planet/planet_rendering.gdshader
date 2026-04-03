shader_type canvas_item;

uniform sampler2D data_texture : filter_nearest;
uniform sampler2D atlas_texture : filter_nearest;
uniform int atlas_columns = 4;
uniform float inner_growth = 0.0;

varying float v_integrity;
varying flat int v_mat_index;
varying vec2 v_local_uv;

void vertex() {
	vec4 data = texture(data_texture, UV);
	v_integrity = data.g;
	v_mat_index = int(data.r * 255.0);

	vec2 dir = normalize(VERTEX);
	if (length(VERTEX) > 0.1) {
		VERTEX += dir * inner_growth;
	}

	int v_type = int(round(COLOR.r * 255.0));
	v_local_uv = vec2(mod(float(v_type), 2.0), 1.0 - floor(float(v_type) / 2.0));
}

void fragment() {
	if (v_integrity <= 0.0) discard;

	float s = 1.0 / float(atlas_columns);
	vec2 atlas_offset = vec2(
		float(v_mat_index % atlas_columns) * s,
		float(v_mat_index / atlas_columns) * s
	);

	COLOR = texture(atlas_texture, atlas_offset + (v_local_uv * s));
}