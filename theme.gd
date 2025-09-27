@tool
extends ProgrammaticTheme

func setup():
	set_save_path("res://main_theme.tres")

func define_theme():
	define_style("PanelContainer", {
		panel = stylebox_flat({
			bg_color = Color.GRAY,
			border_color = Color.DARK_GRAY,
			border_width_bottom = 12,
			border_width_left = 12,
			border_width_right = 12,
			border_width_top = 12,
		})
	})
	define_style("MarginContainer", {
		margin_left = 8,
		margin_right = 8,
		margin_top = 8,
		margin_bottom = 8,
	})
	define_style("VBoxContainer", {
		separation = 8,
	})
	pass
