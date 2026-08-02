local trigger = {}

trigger.name = "BalintHelper/GetPropertyInfoTrigger/LoadConstantInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
	{
		name = "Instruction (Get Property Info)",
		data = {
			width = 16,
			height = 16,
			className = "",
			propertyName = "",
			returnType = "",
			indexerTypes = "",
		}
	}
}

trigger.fieldInformation = {
	className = {
		fieldType = "string",
		description = "The name of the class that declares the property."
	},
	propertyName = {
		fieldType = "string",
		description = "The name of the property to fetch."
	},
	returnType = {
		fieldType = "string",
		description = "Optional. The property's type, used to disambiguate between properties with the same name."
	},
	indexerTypes = {
		fieldType = "string",
		description = "Optional. A comma separated list of the property's indexer parameter types, if it is an indexer."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "className", "propertyName", "returnType", "indexerTypes"
}

return trigger
