local trigger = {}

trigger.name = "BalintHelper/GetConstructorInfoTrigger/LoadConstantInstruction"

trigger.placements = {
	{
		name = "Instruction (Get Constructor Info)",
		data = {
			width = 16,
			height = 16,
			className = "",
			argumentTypes = "",
		}
	}
}

trigger.fieldInformation = {
	className = {
		fieldType = "string",
		description = "The name of the class that declares the constructor."
	},
	argumentTypes = {
		fieldType = "string",
		description = "Optional. A comma separated list of the constructor's parameter types."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "className", "argumentTypes"
}

return trigger
