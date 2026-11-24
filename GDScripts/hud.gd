extends Control

@onready var timer_label: Label = $TimerLabel
@onready var match_timer: Timer = $MatchTimer

#var time_left : int = 60
var time_left : float = 60 # set timer UI to stage's MatchTimer

#func _ready() -> void:
	#pass
	#update_timer_display()
	#match_timer.timeout.connect(_on_timer_timeout)

func _process(_delta: float) -> void:
	time_left = match_timer.time_left # set timer UI to stage's MatchTimer
	update_timer_display()

# timeout code placed in PlayerSpawner.cs
#func _on_timer_timeout() -> void:
#	time_left -= 1
#	update_timer_display()
	# made the timer one shot so it stops automatically
	#if time_left <= 0:
	#	match_timer.stop()

func update_timer_display() -> void:
	var minutes : int = floor(time_left / 60.0)
	var seconds : int = int(time_left) % 60
	timer_label.text = "%d:%02d" % [minutes, seconds]
