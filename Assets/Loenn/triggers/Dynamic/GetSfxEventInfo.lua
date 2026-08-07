local trigger = {}

trigger.name = "BalintHelper/GetSfxEventInfoTrigger/LoadConstantInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
	{
		name = "main",
		data = {
			width = 16,
			height = 16,
			eventPath = "",
			parameters = "",
			loop = false,
			action = "Raw"
		}
	}
}


local actionValues = {
            "Raw",
            "Read",
            "ReadIndexer",
            "Write",
            "WriteIndexer",
            "Invoke"
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
	},
	action = {
		options = actionValues,
		editable = false,
		description = "The action to perform on the constructor."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "eventPath", "parameters", "loop", "action"
}

local languageRegistry = require("language_registry")

trigger.triggerText = function(room, trigger)
    local language = languageRegistry.getLanguage()
    local result = language.triggers[trigger._name].placements.name.main

    if result._exists then
        return tostring(result)
    else
        return trigger._name
    end
end

-- TODO

return trigger
