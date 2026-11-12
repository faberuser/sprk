/* Local SPRK lobby compatibility for the archived Android content. */
"use strict";

setImmediate(() => Il2Cpp.perform(() => {
    const game = Il2Cpp.domain.assembly("Assembly-CSharp").image;
    const files = Il2Cpp.domain.assembly("mscorlib").image.class("System.IO.File");
    const exists = path => files.method("Exists", 1).invoke(Il2Cpp.string(path));

    // The archived patch index uses the older pack layout. Register an existing
    // local bundle through its actual downloaded path, including converted pack
    // metadata, even when the server's empty CDN index does not advertise it.
    const helper = game.class("NGameAsset.NBundleManager.BundleHelper");
    helper.method("GetBundlePathFromPatch", 3).implementation = function (name, folder, check) {
        const path = helper.method("GetBundlePathFromPatch", 3).invoke(name, folder, false);
        if (exists(text(path))) return path;
        return helper.method("GetBundlePathFromPatch", 3).invoke(name, folder, check);
    };

    // The Windows adapter understands the public WebSocket address. The old
    // Android connector still speaks the server's private native TCP protocol.
    // Only translate loopback URLs; do not downgrade remote secure connections.
    const connector = game.class("NVespa.NSocket.SocketConnector");
    connector.method("Connect", 3).implementation = function (address, port, callback) {
        const value = text(address);
        const match = /^wss?:\/\/(127\.0\.0\.1|localhost)(?=[:/]|$)/i.exec(value);
        if (match) {
            address = Il2Cpp.string(match[1]);
            log("Local native socket endpoint mapped to TCP port " + port);
        }
        this.method("Connect", 3).invoke(address, port, callback);
    };

    // Match the Windows PatchChatSession fix: this old LoginReq predates the
    // SessionKey field, so add the authenticated HTTP session when serializing.
    let gameSession = "";
    const requester = game.class("NVespa.NWeb.WebServiceRequester");
    requester.method("set_SessionKey", 1).implementation = function (value) {
        gameSession = text(value);
        this.method("set_SessionKey", 1).invoke(value);
    };
    const encoder = game.class("JM_NShared_NMessage_LoginReq");
    encoder.method("EncodeJsonObject", 1).implementation = function (request) {
        const result = encoder.method("EncodeJsonObject", 1).invoke(request);
        if (!gameSession) {
            const instance = requester.parent.method("get_instance", 0).invoke();
            gameSession = text(instance.method("get_SessionKey", 0).invoke());
        }
        result.method("set_Item", 2).invoke(Il2Cpp.string("SessionKey"), Il2Cpp.string(gameSession));
        log("Native chat login includes the authenticated game session");
        return result;
    };

    // Some old bundle metadata omits the quality directory. Resolve only real
    // existing Android variants; preserve native failure for genuinely absent data.
    for (const name of ["BundleLoadFromFile", "BundleLoadFromFileAsync"]) {
        const loader = game.class("NGameAsset.NBundleManager." + name);
        loader.method("Load", 2).implementation = function (path, callback) {
            const value = text(path);
            const match = /^(.*\/Assetbundle\/)([^/]+\.unity3d)$/.exec(value);
            if (match && !exists(value)) {
                for (const quality of ["HighAndMidSpec", "MidSpec", "OriginalSpec"]) {
                    const candidate = match[1] + quality + "/" + match[2];
                    if (exists(candidate)) {
                        path = Il2Cpp.string(candidate);
                        log("Resolved Android bundle variant: " + quality + "/" + match[2]);
                        break;
                    }
                }
            }
            this.method("Load", 2).invoke(path, callback);
        };
    }

    // SPRK has no external-store billing. Finish that optional startup substate
    // without invoking Google Play or presenting its unavailable-billing modal.
    const billing = game.class("NGame2.NLogin.NState.StateBase_InAppBilling");
    game.class("NGame2.NLogin.NHelper.CheckInAppBilling").method("Check").implementation = function () {};
    const success = billing.nested("InAppBillingState").field("Success").value;
    billing.nested("<coRun>c__Iterator0").method("MoveNext").implementation = function () {
        this.field("$this").value.field("_checkState").value = success;
        billing.method("set_IsReady").invoke(true);
        return false;
    };
    log("Local lobby compatibility hooks installed");
}).catch(error => log("Lobby compatibility initialization failed: " + error.stack)));
