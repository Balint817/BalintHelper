local trigger = {}

trigger.name = "BalintHelper/GetSessionInfoTrigger/LoadConstantInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
			type = "Flag",
			name = "",
			action = "Raw"
        }
    }
}


local actionValues = {
            "Raw",
            "Read",
            "Write",
}

local enumValues = {
"Flag",
"Counter",
"Slider"
}

trigger.fieldInformation = {
    type = {
        options = enumValues,
        editable = false,
        description = "The type of value to fetch"
    },
	name = {
	    fieldType = "string",
		description = "The actual name of the value"
	},
	action = {
		options = actionValues,
		editable = false,
		description = "The action to perform on the constructor."
	}
}

trigger.fieldOrder = {
    "x", "y", "width", "height", "type", "name", "action"
}

trigger.triggerText = function(room, trigger)
    return "Session (" .. trigger.type .. " " .. trigger.name .. ")"
end

-- TODO

return trigger