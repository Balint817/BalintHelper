local trigger = {}

trigger.name = "BalintHelper/GetFieldInfoTrigger/LoadConstantInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
	{
		name = "main",
		data = {
			width = 16,
			height = 16,
			className = "",
			fieldName = "",
			fieldType = "",
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
	className = {
		fieldType = "string",
		description = "The name of the class that declares the field."
	},
	fieldName = {
		fieldType = "string",
		description = "The name of the field to fetch."
	},
	fieldType = {
		fieldType = "string",
		description = "Optional. The field's type, used to disambiguate between fields with the same name."
	},
	action = {
		options = actionValues,
		editable = false,
		description = "The action to perform on the constructor."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "className", "fieldName", "fieldType", "action"
}

trigger.triggerText = function(room, trigger)
	return "Field (" .. trigger.fieldName .. ")"
end

-- TODO

return trigger
