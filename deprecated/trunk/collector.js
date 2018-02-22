function vectorToString(vector) {
    return `X: ${vector.X} Y: ${vector.Y} Z: ${vector.Z}`;
}

function drawBox(x, y, z, x2, y2, z2) {    
    API.callNative("DRAW_BOX", API.f(x), API.f(y), API.f(z), API.f(x2), API.f(y2), API.f(z2), 255, 255, 255, 170);
}

var currentVehcle = null;
var currentOffset = null;
var currentSize = null;

API.onServerEventTrigger.connect(function(ev, args) {
    if (ev === "START_COLLECTING") {
        currentVehcle = args[0];
        currentOffset = new Vector3();
        currentSize = new Vector3(1, 1, 1);
    } else if (ev === "STOP_COLLECTING") {
        currentVehcle = null;
    } else if (ev === "SAVE_COLLECT") {
        API.triggerServerEvent("COLLECT_RESULTS", currentVehcle, currentOffset, currentSize);
    }
});

API.onUpdate.connect(function() {
    if (currentVehcle != null) {
        API.setEntityRotation(currentVehcle, new Vector3(0, 0, 0));

        var vpos = API.getEntityPosition(currentVehcle);
        var center = new Vector3(vpos.X + currentOffset.X, vpos.Y + currentOffset.Y, vpos.Z + currentOffset.Z);
        drawBox(center.X - currentSize.X/2, center.Y - currentSize.Y/2, center.Z,
                center.X + currentSize.X/2, center.Y + currentSize.Y/2, center.Z + currentSize.Z);

        API.displaySubtitle("Offset: " + vectorToString(currentOffset) + "\nSize: " + vectorToString(currentSize));
    }
})

API.onKeyDown.connect(function(sender, e) {
    if (currentVehcle == null) return;
    var step = 0.05;

    if (e.KeyCode == Keys.I) {
        currentOffset = new Vector3(currentOffset.X, currentOffset.Y + step, currentOffset.Z);
    } else if (e.KeyCode == Keys.K) {
        currentOffset = new Vector3(currentOffset.X, currentOffset.Y - step, currentOffset.Z);
    } else if (e.KeyCode == Keys.J) {
        currentOffset = new Vector3(currentOffset.X - step, currentOffset.Y, currentOffset.Z);
    } else if (e.KeyCode == Keys.L) {
        currentOffset = new Vector3(currentOffset.X + step, currentOffset.Y, currentOffset.Z);
    } else if (e.KeyCode == Keys.U) {
        currentOffset = new Vector3(currentOffset.X, currentOffset.Y, currentOffset.Z + step);
    } else if (e.KeyCode == Keys.O) {
        currentOffset = new Vector3(currentOffset.X, currentOffset.Y, currentOffset.Z - step);
    }

    if (e.KeyCode == Keys.NumPad8) {
        currentSize = new Vector3(currentSize.X, currentSize.Y + step, currentSize.Z);
    } else if (e.KeyCode == Keys.NumPad5) {
        currentSize = new Vector3(currentSize.X, currentSize.Y - step, currentSize.Z);
    } else if (e.KeyCode == Keys.NumPad4) {
        currentSize = new Vector3(currentSize.X - step, currentSize.Y, currentSize.Z);
    } else if (e.KeyCode == Keys.NumPad6) {
        currentSize = new Vector3(currentSize.X + step, currentSize.Y, currentSize.Z);
    } else if (e.KeyCode == Keys.NumPad7) {
        currentSize = new Vector3(currentSize.X, currentSize.Y, currentSize.Z + step);
    } else if (e.KeyCode == Keys.NumPad1) {
        currentSize = new Vector3(currentSize.X, currentSize.Y, currentSize.Z - step);
    }

});