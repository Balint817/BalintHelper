local trigger = {}

trigger.name = "BalintHelper/BaseInstructionTrigger/AllocInstruction"

trigger.placements = {
    {
        name = "Instruction (Allocate Memory)",
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