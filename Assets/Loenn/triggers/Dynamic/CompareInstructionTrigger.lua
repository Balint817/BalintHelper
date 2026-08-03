local trigger = {}

trigger.name = "BalintHelper/CompareInstructionTrigger"

trigger.nodeLimits = {1, -1}

trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
			type = "Equals",
        }
    }
}

local enumValues = {

            "Equals",
            "NotEquals",
            "GreaterThan",
            "GreaterThanOrEquals",
            "LessThan",
            "LessThanOrEquals"
}

trigger.fieldInformation = {
    type = {
        options = enumValues,
        editable = false,
        description = "The operation to execute."
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

-- TODO

return trigger