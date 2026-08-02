local trigger = {}

trigger.name = "BalintHelper/BaseInstructionTrigger/NopInstruction"

trigger.placements = {
    {
        name = "Instruction (Noop)",
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