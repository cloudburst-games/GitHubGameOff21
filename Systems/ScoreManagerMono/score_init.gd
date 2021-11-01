extends Node

var config = {
	"api_key": "0X8VUf5W3x9CfP7ddbJpyzZVQ0E4D9f9tpGAIR5i",
	"game_id": "Brackeys212", #MiloTheMenaceOfCandyland
	"game_version": "1.0.2",
	"log_level": 1
}

func _ready():
	if SilentWolf.config != config:
		SilentWolf.configure(config)
