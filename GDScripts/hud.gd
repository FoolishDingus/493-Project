extends Control

@onready var timer_label: Label = $TimerLabel
@onready var match_timer: Timer = $"../../MatchTimer"

var time_left: int = 60

func _ready() -> void: 
	update_timer_display()
	match_timer.timeout.connect(_on_timer_timeout)

func _on_timer_timeout() -> void:
	time_left -= 1
	update_timer_display()	
	if time_left <= 0:
		match_timer.stop()

func update_timer_display() -> void:
	var minutes = time_left / 60
	var seconds = time_left % 60
	
	timer_label.text = "%d:%02d" % [minutes, seconds]
