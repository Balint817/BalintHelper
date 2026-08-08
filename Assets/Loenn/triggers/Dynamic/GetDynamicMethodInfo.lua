local trigger = {}

trigger.name = "BalintHelper/GetDynamicMethodInfoTrigger/LoadConstantInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
	{
		name = "main",
		data = {
			width = 16,
			height = 16,
			name = "",
			action = "Raw"
		}
	}
}

local actionValues = {
            "Raw",
            "Invoke"
}

trigger.fieldInformation = {
	name = {
		fieldType = "string",
		description = "The name of the dynamic method to fetch."
	},
	action = {
		options = actionValues,
		editable = false,
		description = "The action to perform on the constructor."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "name", "action"
}

trigger.triggerText = function(room, trigger)
	return "Dynamic Method (" .. trigger.name .. ")"
end

return trigger
