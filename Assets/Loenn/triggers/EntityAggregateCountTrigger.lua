local trigger = {}

trigger.name = "BalintHelper/EntityAggregateCountTrigger"

trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,

            counterId = "entityCount",
            aggregateMode = "Maximum",
            entityTypes = "TheoCrystal;ExtendedVariantMode/TheoCrystal",
        }
    }
}

local aggregateModes = { "Minimum", "Maximum", "Sum" }

trigger.fieldInformation = {
    counterId = {
        fieldType = "string",
        description = "Session counter updated by this trigger group"
    },
    aggregateMode = {
        options = aggregateModes,
        editable = false,
        description = "How all triggers with the same counterId are combined"
    },
    entityTypes = {
        fieldType = "string",
        description = "Comma-separated type names or entity IDs to count inside the trigger"
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height",
    "counterId",
    "aggregateMode",
    "entityTypes"
}

return trigger