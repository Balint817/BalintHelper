local trigger = {}

trigger.name = "BalintHelper/CastInstructionTrigger/CastInstruction"

trigger.placements = {
    {
        name = "Instruction (Cast)",
        data = {
            width = 16,
            height = 16,
			type = "",
			sourceType = "",
        }
    }
}

trigger.fieldInformation = {
    type = {
        fieldType = "string",
        description = "The target type of the operation."
    },
	sourceType = {
        fieldType = "string",
        description = "Optional. The type to use as the source type. (useful in case of custom conversions, and required when casting null to a value type)"
	}
}

trigger.fieldOrder = {
    "x", "y", "width", "height", "type"
}

return trigger