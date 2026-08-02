local trigger = {}

trigger.name = "BalintHelper/DefineMethodTrigger"

trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
			methodName = "MyMethod",
			argCount = 0
        }
    }
}

trigger.fieldInformation = {
    methodName = {
        fieldType = "string",
        description = "The name of the dynamic method."
    }
    argCount = {
        fieldType = "number",
        description = "Number of arguments the method expects."
    },
}

trigger.fieldOrder = {
    "x", "y", "width", "height", "methodName", "argCount"
}

return trigger