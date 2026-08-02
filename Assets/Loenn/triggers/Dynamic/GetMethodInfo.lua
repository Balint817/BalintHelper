local trigger = {}

trigger.name = "BalintHelper/GetMethodInfoTrigger/LoadConstantInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
	{
		name = "main",
		data = {
			width = 16,
			height = 16,
			className = "",
			methodName = "",
			genericTypes = "",
			argumentTypes = "",
			returnType = "",
		}
	}
}

trigger.fieldInformation = {
	className = {
		fieldType = "string",
		description = "The name of the class that declares the method."
	},
	methodName = {
		fieldType = "string",
		description = "The name of the method to fetch."
	},
	genericTypes = {
		fieldType = "string",
		description = "Optional. A comma separated list of the generic type arguments to use, if the method is generic."
	},
	argumentTypes = {
		fieldType = "string",
		description = "Optional. A comma separated list of the method's parameter types."
	},
	returnType = {
		fieldType = "string",
		description = "Optional. The method's return type, used to disambiguate between methods with the same name and parameters."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "className", "methodName", "genericTypes", "argumentTypes", "returnType"
}

-- TODO

return trigger
