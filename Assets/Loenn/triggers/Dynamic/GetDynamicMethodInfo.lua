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
		}
	}
}

trigger.fieldInformation = {
	name = {
		fieldType = "string",
		description = "The name of the dynamic method to fetch."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "name"
}

return trigger
