local trigger = {}

trigger.name = "BalintHelper/BaseInstructionTrigger/ArrayVectorLengthInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
    {
        name = "Instruction (Get Vector Length)",
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