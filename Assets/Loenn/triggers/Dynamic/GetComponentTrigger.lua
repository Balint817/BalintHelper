local trigger = {}

trigger.name = "BalintHelper/GetComponentTrigger/NopInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
	{
		name = "Instruction (Get Component)",
		data = {
			width = 16,
			height = 16,
			componentType = "",
		}
	}
}

trigger.fieldInformation = {
	componentType = {
		fieldType = "string",
		description = "The fully qualified type name of the component to fetch from the entity currently on top of the stack."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "componentType"
}

return trigger
