local trigger = {}

trigger.name = "BalintHelper/BaseInstructionTrigger/ArrayVectorLengthInstruction"

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