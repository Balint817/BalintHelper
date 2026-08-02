local trigger = {}

trigger.name = "BalintHelper/GetEntityInfoTrigger/LoadConstantInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
	{
		name = "Instruction (Get Entity Info)",
		data = {
			width = 16,
			height = 16,
			types = "",
		}
	}
}

trigger.fieldInformation = {
	types = {
		fieldType = "string",
		description = "A semicolon separated list of entity IDs and/or type names to filter for. Leave empty to match all entities."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "types"
}

return trigger
