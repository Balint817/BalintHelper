local trigger = {}

trigger.name = "BalintHelper/BaseInstructionTrigger/IsDefinedInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
    {
        name = "Instruction (Is Defined)",
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