local trigger = {}

trigger.name = "BalintHelper/InputBlockTrigger"

trigger.placements = {
    {
        name = "Input Block Trigger",
        data = {
            width = 16,
            height = 16,

        }
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height"
}

return trigger