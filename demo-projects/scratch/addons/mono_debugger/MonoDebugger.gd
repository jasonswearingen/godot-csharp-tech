tool
extends EditorPlugin

var btn: CheckBox = null
var on: bool

const setting = 'mono/debugger_agent/wait_for_debugger'

func _enter_tree() -> void:
	on = ProjectSettings.get_setting(setting)
	btn = CheckBox.new()
	btn.text = 'Wait For Debugger'
	btn.pressed = on
	btn.connect('pressed', self, '_on_button_pressed')
	add_control_to_container(CONTAINER_TOOLBAR, btn)

func _exit_tree() -> void:
	btn.queue_free()
	remove_control_from_container(CONTAINER_TOOLBAR, btn)
	btn = null
	
func _on_button_pressed() -> void:
	on = !on
	ProjectSettings.set_setting(setting, on)
	ProjectSettings.save()
