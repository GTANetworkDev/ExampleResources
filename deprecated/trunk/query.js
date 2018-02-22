var currentModel = null;

API.onServerEventTrigger.connect(function(ev, args) {
    if (ev === "QUERY_MODEL_SIZE") {
    	currentModel = args[0];
        API.startCoroutine(modelCoroutine);
    }
});

function *modelCoroutine() {
    if (currentModel == null) return;

	API.loadModel(currentModel);

    while (!API.returnNative("HAS_MODEL_LOADED", 8, currentModel)) {
        yield 500;
    }

    var dims = API.getModelDimensions(currentModel);

    API.triggerServerEvent("QUERY_MODEL_RESPONSE", currentModel, dims.Maximum, dims.Minimum);

    currentModel = null;
}