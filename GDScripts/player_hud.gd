extends Control


@onready var health_bar: ProgressBar = $HealthBar

# NOTE: Used to test out player 1 only
# @onready var player: Node = $"../P1"

#Used to test out displaying text in item box for planned items
@onready var item_slot = $ItemSlot

#Receive Player's number
@export_range(1, 4) var player_number: int = 1
@onready var player: Node = get_parent().get_node("P" + str(player_number))

@onready var player_label: Label = $PlayerLabel

var previous_hp: int = -1

func _ready() -> void:
	
	#Configured the Health Bar
	health_bar.min_value = 0
	health_bar.max_value = player.maxHP
	
	player_label.text = "Player " + str(player_number)
	
	#print("Player ", player_number, " HUD initialized!")
	
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
	if player_number == 1 and event is InputEventKey and event.pressed and not event.echo:
		match event.keycode:
			KEY_1:
				item_slot.set_item("speed_boost")
			KEY_0:
				item_slot.clear_item()
