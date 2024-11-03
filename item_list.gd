extends ItemList

var selected_indices : Array[int] = []


func _ready():
	multi_selected.connect(update_selection)


func update_selection(index : int, _selected : bool):
	deselect_all()
	
	if selected_indices.has(index):
		selected_indices.erase(index)
	else:
		selected_indices.append(index)
	
	for i in selected_indices:
		select(i, false)
