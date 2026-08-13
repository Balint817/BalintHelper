local smokeAura = {}

smokeAura.name = "BalintHelper/SmokeAura"
smokeAura.depth = -1000000
smokeAura.placements = {
	name = "main",
	data = {
		width = 16,
		height = 32,
		sessionSlider = "",
		colorA = "463759",
		colorB = "8f7aa8",
	}
}

smokeAura.fieldInformation = {
	sessionSlider = { fieldType = "string" },
	colorA = { fieldType = "color" },
	colorB = { fieldType = "color" },
}

smokeAura.minimumSize = {8, 8}
smokeAura.resizable = {true, true}

return smokeAura