local trigger = {}

trigger.name = "BalintHelper/GetSceneTrigger/NopInstruction"

trigger.placements = {
    {
        name = "Instruction (Get Scene)",
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