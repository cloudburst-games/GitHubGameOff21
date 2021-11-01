extends Panel

var pnl_scoreunit_scn = preload("res://Systems/ScoreManagerMono/pnl_scoreunit.tscn")

var max_scores_to_show = 10
var timeout = false
var current_ldboard_name = "main"

func _ready():
	visible = false
	$loading_sprite.visible = false

func show_and_refresh_board(max_scores, ldboard_name = "main"):
	show_board()
	refresh_board(max_scores, ldboard_name)

func refresh_board(max_scores, ldboard_name = "main"):
	timeout = false
	max_scores_to_show = max_scores
	current_ldboard_name = ldboard_name
	clear_board()
	$loading_sprite.visible = true
	$anim.play("Loading")
	$PnlTimeout.start_timeout(self)
	yield(SilentWolf.Scores.get_high_scores(max_scores, ldboard_name), "sw_scores_received")
	if timeout:
		return
	$PnlTimeout.stop_timeout()
	$loading_sprite.visible = false
	var pnl_size = $pnl_scores.rect_size.y
	var increment = round(pnl_size/max_scores)
	for n in range(max_scores):
		if SilentWolf.Scores.scores.size() < n+1:
			return
		var item = SilentWolf.Scores.scores[n]
		var pnl_scoreunit = pnl_scoreunit_scn.instance()
		$pnl_scores.add_child(pnl_scoreunit)
		pnl_scoreunit.rect_position.y = increment*n
		pnl_scoreunit.rect_size = Vector2($pnl_scores.rect_size.x,increment)
		pnl_scoreunit.get_node("HBoxContainer").rect_size = Vector2(pnl_scoreunit.rect_size.x - 30, pnl_scoreunit.rect_size.y)
		pnl_scoreunit.get_node("HBoxContainer").rect_position.x = 15
		pnl_scoreunit.get_node("HBoxContainer/lbl_name").text = item["player_name"]
		pnl_scoreunit.get_node("HBoxContainer/lbl_pos").text = str(n+1)+"."
		pnl_scoreunit.get_node("HBoxContainer/lbl_score").text = str(item["score"])
		if item.has("metadata"):
			for meta_item in item["metadata"]:
				var meta_label = pnl_scoreunit.get_node("HBoxContainer/lbl_name").duplicate()
				pnl_scoreunit.get_node("HBoxContainer").add_child(meta_label)
				meta_label.text = str(item["metadata"][meta_item])
				if meta_item == "Player2Name":
					meta_label.name = "lbl_p2name"

			var p1_pos = pnl_scoreunit.get_node("HBoxContainer/lbl_name").get_index()
			if pnl_scoreunit.get_node("HBoxContainer").has_node("lbl_p2name"):
				pnl_scoreunit.get_node("HBoxContainer").move_child(pnl_scoreunit.get_node("HBoxContainer/lbl_p2name"), p1_pos+1)

		# move the score to the end of the list
		pnl_scoreunit.get_node("HBoxContainer").move_child(pnl_scoreunit.get_node("HBoxContainer/lbl_score"),pnl_scoreunit.get_node("HBoxContainer").get_child_count()-1)

func show_board():
	visible = true

func _on_btn_close_pressed():
	close_all()
	
func on_timeout():
	timeout = true
	for node in get_children():
		if node is Button:
			node.disabled = true

func close_all():
	visible = false
	$loading_sprite.visible = false
	if $anim.is_playing():
		$anim.seek(0, true)
	$anim.stop(true)
	for node in get_children():
		if node is Button:
			node.disabled = false

func clear_board():
	for node in $pnl_scores.get_children():
		node.queue_free()
	
func _on_btn_refresh_pressed():
	refresh_board(max_scores_to_show, current_ldboard_name)
