extends Control


@onready var health_bar: ProgressBar = $HealthBar

# NOTE: Used to test out player 1 only
# @onready var player: Node = $"../P1"

#Used to test out displaying text in item box for planned items
@onready var item_slot = $ItemSlot

#Receive Player's number
#@export_range(1, 4) var player_number: int = 1
#@onready var player: Node = get_parent().get_node("P" + str(player_number))
@onready var player: CharacterBody3D = get_parent()
@onready var player_label: Label = $PlayerLabel

var previous_hp: int = -1

func _ready() -> void:
	
	#Configured the Health Bar
	health_bar.min_value = 0
	health_bar.max_value = player.maxHP
	
	player_label.text = "Player " + str(player.playerID)
	
	#print("Player ", player_number, " HUD initialized!")
	# changed health bars to have their colors set by player number
	match player.playerID:
		1:
			health_bar.self_modulate = Color(0.86, 0.48, 0.26, 1) # R,G,B, alpha
			#health_bar.border_color = Color(0.99, 0.75, 0.24, 1)
		2:
			health_bar.self_modulate = Color(0, 0.63, 0.93, 1)
			#health_bar.border_color = Color(0.45, 0.86, 0.99, 1)
		3:
			health_bar.self_modulate = Color(0.14, 0.7, 0.5, 1)
			#health_bar.border_color = Color(0.26, 0.95, 0.08, 1)
		4:
			health_bar.self_modulate = Color(0.85, 0.87, 0, 1)
			#health_bar.border_color = Color(0.54, 0.56, 0, 1)
	update_health_bar()
	
#Keeps the health bar updated
func _process(_delta: float) -> void:
	if is_instance_valid(player) and not player.is_queued_for_deletion():
		update_health_bar()
	else:
		health_bar.value = 0

func update_health_bar() -> void:
	
	#Used a function from player.cs where HP goes down due to damage 
	var current_hp: int = clampi(int(player.get("hp")), 0, player.maxHP)
	
	health_bar.value = current_hp
	
	#Keep track on the amount of HP the player has
	if current_hp != previous_hp:
		#print("Player: ", player_number, " HP: ", current_hp, "/", health_bar.max_value)
		previous_hp = current_hp
		
		
#Used to test out the display in the item box
func _input(event: InputEvent) -> void:
	if player.playerID == 1 and event is InputEventKey and event.pressed and not event.echo:
		match event.keycode:
			KEY_0:
				item_slot.clear_item()
			KEY_1:
				item_slot.set_item("rocket")
			KEY_2:
				item_slot.set_item("bomb")
			KEY_3:
				item_slot.set_item("shield")
			KEY_4:
				item_slot.set_item("speed")
