local trigger = {}

trigger.name = "BalintHelper/GetSessionInfoTrigger/LoadConstantInstruction"

trigger.placements = {
    {
        name = "Instruction (Get Session Info)",
        data = {
            width = 16,
            height = 16,
			type = "Flag",
			name = "",
        }
    }
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
	}
}

trigger.fieldOrder = {
    "x", "y", "width", "height", "type", "name"
}

return trigger