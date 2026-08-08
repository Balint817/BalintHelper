local trigger = {}

trigger.name = "BalintHelper/NewArrayInstructionTrigger/NewArrayInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
			type = "",
			dimensions = 1
        }
    }
}

trigger.fieldInformation = {
    type = {
        fieldType = "string",
        description = "The element type of the array."
    },
	dimensions = {
	    fieldType = "number",
		description = "The number of dimensions the array should have (or the array's rank)"
	}
}

trigger.fieldOrder = {
    "x", "y", "width", "height", "type"
}

local languageRegistry = require("language_registry")

trigger.triggerText = function(room, trigger)
    local language = languageRegistry.getLanguage()
    local result = language.triggers[trigger._name].placements.name.main

    if result._exists then
        return tostring(result)
    else
        return trigger._name
    end
end

return trigger