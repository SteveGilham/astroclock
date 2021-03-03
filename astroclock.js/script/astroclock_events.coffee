"use strict";
lat = document.getElementById("lat")
long = document.getElementById("long")
canvas = document.getElementById("display")

# Add load event
window.addEventListener('load', this.canvasApp, false)
lat.addEventListener('change', this.updateLabels, false)
long.addEventListener('change', this.updateLabels, false)
canvas.addEventListener('mousemove', this.mousehandler, false)
