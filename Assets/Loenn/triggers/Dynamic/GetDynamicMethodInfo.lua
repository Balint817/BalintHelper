local trigger = {}

trigger.name = "BalintHelper/GetDynamicMethodInfoTrigger/LoadConstantInstruction"

trigger.placements = {
	{
		name = "Instruction (Get Dynamic Method Info)",
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
