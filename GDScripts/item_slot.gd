extends Panel

@onready var item_label: Label = $ItemLabel

#When there is no item in the item slot
var current_item: String = "none"


func _ready() -> void: 
	update_item_display()
	
	
#Assigned an item to the item slot
func set_item(item_tag: String) -> void:
	current_item = item_tag
	update_item_display()
	print("item obtained: ", current_item)
	
	
	#Executed after item has been used or deactivated 
func clear_item() -> void:
	current_item = "none"
	update_item_display()
	print("Item slot cleared")
	
	#NOTE: Used for implmenting items later
func update_item_display() -> void: 
	match current_item:
		"speed_boost": 
			item_label.text = "SDP"
		"none":
			item_label.text = ""
		_:
			item_label.text = "?"
			push_warning("Unknown item tag: " + current_item)
