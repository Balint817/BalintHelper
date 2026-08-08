local trigger = {}

trigger.name = "BalintHelper/GetConstructorInfoTrigger/LoadConstantInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
	{
		name = "main",
		data = {
			width = 16,
			height = 16,
			className = "",
			argumentTypes = "",
			action = "Raw"
		}
	}
}


local actionValues = {
            "Raw",
            "Invoke"
}

trigger.fieldInformation = {
	className = {
		fieldType = "string",
		description = "The name of the class that declares the constructor."
	},
	argumentTypes = {
		fieldType = "string",
		description = "Optional. A semicolon separated list of the constructor's parameter types."
	},
	action = {
		options = actionValues,
		editable = false,
		description = "The action to perform on the constructor."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "className", "argumentTypes", "action"
}

trigger.triggerText = function(room, trigger)
	return "Constructor (" .. trigger.className .. ")"
end

return trigger
