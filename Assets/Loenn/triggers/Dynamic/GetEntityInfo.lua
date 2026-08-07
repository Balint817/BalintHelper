local trigger = {}

trigger.name = "BalintHelper/GetEntityInfoTrigger/LoadConstantInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
	{
		name = "main",
		data = {
			width = 16,
			height = 16,
			types = "",
			mode = "First",
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

local enumValues = {
	"First",
	"All"
}

trigger.fieldInformation = {
	types = {
		fieldType = "string",
		description = "A semicolon separated list of entity IDs and/or type names to filter for. Leave empty to match all entities."
	},
	mode = {
        options = enumValues,
        editable = false,
        description = "The operation to execute."
	},
	action = {
		options = actionValues,
		editable = false,
		description = "The action to perform on the constructor."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "types", "mode", "action"
}

trigger.triggerText = function(room, trigger)
	return "Entity (" .. trigger.mode .. ")"
end

-- TODO

return trigger
