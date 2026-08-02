local trigger = {}

trigger.name = "BalintHelper/InvokeMethodTrigger"

trigger.placements = {
	{
		name = "main",
		data = {
			width = 16,
			height = 16,
			methodName = "",
			onlyOnce = true,
			argumentMode = "None",
		}
	}
}

local argumentModes = { "None", "Position", "Bounds" }

trigger.fieldInformation = {
	methodName = {
		fieldType = "string",
		description = "The name of the dynamic method (defined via a DefineMethodTrigger) to invoke when the player enters this trigger."
	},
	onlyOnce = {
		fieldType = "boolean",
		description = "Whether the method should only be invoked the first time the player enters this trigger."
	},
	argumentMode = {
		options = argumentModes,
		editable = false,
		description = "What (if anything) to pass to the dynamic method as its single argument: None, the trigger's Position (Vector2), or its Bounds (Rectangle)."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "methodName", "onlyOnce", "argumentMode"
}

return trigger
