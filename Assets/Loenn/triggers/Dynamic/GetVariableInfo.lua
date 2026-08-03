local trigger = {}

trigger.name = "BalintHelper/GetVariableInfoTrigger/LoadConstantInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
			type = "Local",
			name = "",
        }
    }
}

local enumValues = {
"Local",
"Global",
"Argument"
}

trigger.fieldInformation = {
    type = {
        options = enumValues,
        editable = false,
        description = "The type of variable to fetch"
    },
	name = {
	    fieldType = "string",
		description = "The actual name of the variable"
	}
}

trigger.fieldOrder = {
    "x", "y", "width", "height", "type", "name"
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

-- TODO

return trigger