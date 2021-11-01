extends Panel

signal finished
signal high_score
signal no_high_score

var high_score = 0
var score_metadata = {}
var score_ldboard_name = "main"
var original_high_score_title = ""
var timeout = false
var p1_original_y = 0
var p2_original_y = 0

func _ready():
	visible = false
	$cnt_highscore.visible = false
	$cnt_checking.visible = false
	$cnt_highscore/lbl_status.visible = false
	original_high_score_title = $cnt_highscore/lbl_title.text
	p1_original_y = $cnt_highscore/lbl_name.rect_position.y
	p2_original_y = $cnt_highscore/lbl_name2.rect_position.y
	centralise_player1_nodes()

func start(score, max_scores, ldboard_name = "main", metadata = null):
	timeout = false
	high_score = score
	score_metadata = {}
	if metadata != null:
		score_metadata = metadata
	score_ldboard_name = ldboard_name
	visible = true
	$cnt_checking.visible = true
	$cnt_checking/anim.play("Loading")
	$PnlTimeout.start_timeout(self)
	yield(SilentWolf.Scores.get_score_position(score, ldboard_name), "sw_position_received")
	if timeout:
		return
	$PnlTimeout.stop_timeout()
	$cnt_checking.visible = false
	var position = SilentWolf.Scores.position
	if position <= max_scores and score > 0:
		$cnt_highscore.visible = true
		$cnt_highscore/lbl_title.text = original_high_score_title + str(high_score) + "!"
		if $cnt_highscore/lbl_name2.visible:
			centralise_player1_nodes()
		if metadata != null:
			if metadata.has("Player2Name"):
				spread_player_nodes()
		emit_signal("high_score")
	else:
		emit_signal("no_high_score")
		close_all()

func centralise_player1_nodes():
	$cnt_highscore/lbl_name2.visible = false
	$cnt_highscore/led_name2.visible = false
	$cnt_highscore/lbl_name.text = "Name:"
	$cnt_highscore/lbl_name.rect_position.y = ($cnt_highscore/lbl_name.rect_position.y + $cnt_highscore/lbl_name2.rect_position.y)/2
	$cnt_highscore/led_name.rect_position.y = ($cnt_highscore/led_name.rect_position.y + $cnt_highscore/led_name2.rect_position.y)/2

func spread_player_nodes():
	$cnt_highscore/lbl_name2.visible = true
	$cnt_highscore/led_name2.visible = true
	$cnt_highscore/lbl_name.text = "Player 1 Name:"
	$cnt_highscore/lbl_name.rect_position.y = p1_original_y
	$cnt_highscore/led_name.rect_position.y = p1_original_y
	$cnt_highscore/lbl_name2.rect_position.y = p2_original_y
	$cnt_highscore/led_name2.rect_position.y = p2_original_y
		
func is_valid_name(name):
	if name == "":
		return false
	return true

func close_all():
	visible = false
	$cnt_highscore.visible = false
	$cnt_checking.visible = false
	$cnt_highscore/lbl_status.visible = false
	$cnt_highscore/btn_submit.disabled = false
	$cnt_highscore/lbl_title.text = original_high_score_title
	if $cnt_checking/anim.is_playing():
		$cnt_checking/anim.seek(0, true)
	if $cnt_highscore/anim.is_playing():
		$cnt_highscore/anim.seek(0, true)
	$cnt_checking/anim.stop(true)
	$cnt_highscore/anim.stop(true)
	emit_signal("finished")
	

func on_timeout():
	timeout = true

func _on_BtnSubmit_pressed():
	timeout = false
	if !is_valid_name($cnt_highscore/led_name.text) or (score_metadata.has("Player2Name") and !is_valid_name($cnt_highscore/led_name2.text)):
		$cnt_highscore/lbl_status.visible = true
		$cnt_highscore/lbl_status.text = "Invalid player name"
		return
	if score_metadata.has("Player2Name"):
		score_metadata["Player2Name"] = $cnt_highscore/led_name2.text
	$cnt_highscore/lbl_status.visible = false
	$cnt_highscore/btn_submit.disabled = true
	$cnt_highscore/anim.play("Loading")
	$PnlTimeout.start_timeout(self)
	yield(SilentWolf.Scores.persist_score($cnt_highscore/led_name.text, high_score, score_ldboard_name, score_metadata), "sw_score_posted")
	if timeout:
		return
	$PnlTimeout.stop_timeout()
	close_all()

func _input(event):
	if Input.is_key_pressed(KEY_ENTER) and (event.is_pressed() and not event.is_echo()) and $cnt_highscore.visible:
		_on_BtnSubmit_pressed()
