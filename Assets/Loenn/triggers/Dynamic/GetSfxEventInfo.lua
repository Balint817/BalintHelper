local trigger = {}

trigger.name = "BalintHelper/GetSfxEventInfoTrigger/LoadConstantInstruction"

trigger.placements = {
	{
		name = "Instruction (Get Sfx Event Info)",
		data = {
			width = 16,
			height = 16,
			eventPath = "",
			parameters = "",
			loop = false,
		}
	}
}

trigger.fieldInformation = {
	eventPath = {
		fieldType = "string",
		description = "The FMOD event path to play, e.g. \"event:/game/general/thing_booped\"."
	},
	parameters = {
		fieldType = "string",
		description = "A semicolon separated list of \"name=value\" FMOD parameter assignments to set on the event instance before playing, e.g. \"intensity=0.5;pitch=1.2\". Leave empty for no parameters."
	},
	loop = {
		fieldType = "boolean",
		description = "Whether the event instance should keep playing/looping instead of being released immediately after starting."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "eventPath", "parameters", "loop"
}

return trigger
