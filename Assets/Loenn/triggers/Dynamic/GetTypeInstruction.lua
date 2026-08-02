local trigger = {}

trigger.name = "BalintHelper/BaseInstructionTrigger/GetTypeInstruction"

trigger.placements = {
    {
        name = "Instruction (Get Object Type)",
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