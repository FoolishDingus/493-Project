extends Panel

@onready var item_label: Label = $ItemLabel
@onready var player: CharacterBody3D = get_parent().get_parent()

#When there is no item in the item slot
var current_item: String = "none"

func _ready() -> void: 
	update_item_display()
	
	
#Assigned an item to the item slot
func set_item(item_tag: String) -> void:
	current_item = item_tag
	update_item_display()
	print("Gave P1: ", current_item)
	
	
	#Executed after item has been used or deactivated 
func clear_item() -> void:
	current_item = "none"
	update_item_display()
	print("Item slot cleared")
	
	#NOTE: Used for implmenting items later
func update_item_display() -> void:
	match current_item:
		"speed": 
			item_label.text = "speed"
			player.itemSlot = current_item
		"rocket": 
			item_label.text = "rocket"
			player.itemSlot = current_item
		"bomb": 
			item_label.text = "bomb"
			player.itemSlot = current_item
		"shield": 
			item_label.text = "shield"
			player.itemSlot = current_item
		"speed": 
			item_label.text = "speed Boost"
			player.itemSlot = current_item
		"none":
			item_label.text = ""
		_:
			item_label.text = "?"
			push_warning("Unknown item tag: " + current_item)
