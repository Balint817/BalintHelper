local trigger = {}

trigger.name = "BalintHelper/IsBetweenInstructionTrigger"

trigger.nodeLimits = {1, -1}

trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
            bottomInclusive = true,
            topInclusive = true,
        }
    }
}


trigger.fieldOrder = {
    "x", "y", "width", "height", "bottomInclusive", "topInclusive"
}

trigger.triggerText = function(room, trigger)
    local text = "Between (a<"
    if trigger.bottomInclusive then
        text = text .. "="
    end
    text = text .. "x<"
    if trigger.topInclusive then
        text = text .. "="
    end
    text = text .. "b)"
    return text
end

return trigger