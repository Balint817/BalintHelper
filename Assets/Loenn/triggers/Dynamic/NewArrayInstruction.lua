local trigger = {}

trigger.name = "BalintHelper/NewArrayInstructionTrigger/NewArrayInstruction"

trigger.placements = {
    {
        name = "Instruction (New Array)",
        data = {
            width = 16,
            height = 16,
			type = "",
			dimensions = 1
        }
    }
}

trigger.fieldInformation = {
    type = {
        fieldType = "string",
        description = "The element type of the array."
    },
	dimensions = {
	    fieldType = "number",
		description = "The number of dimensions the array should have (or the array's rank)"
	}
	
}

trigger.fieldOrder = {
    "x", "y", "width", "height", "type"
}

return trigger