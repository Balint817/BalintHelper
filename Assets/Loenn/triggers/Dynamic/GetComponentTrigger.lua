local trigger = {}

trigger.name = "BalintHelper/GetComponentTrigger/NopInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
	{
		name = "main",
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

trigger.triggerText = function(room, trigger)
	return "Component (" .. trigger.componentType .. ")"
end

return trigger
