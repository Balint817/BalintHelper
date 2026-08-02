local trigger = {}

trigger.name = "BalintHelper/BaseInstructionTrigger/StoreArrayElementInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
    {
        name = "Instruction (Store Array Element)",
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