/* SPRK Android account adapter. Bundled locally; no remote scripts or listener. */
"use strict";

let session = null;
let loginView = null;
let choices = [];
let form = null;
let queued = null;
let pendingTicket = null;
let busy = false;
const androidLog = new NativeFunction(Module.getExportByName("liblog.so", "__android_log_write"),
    "int", ["int", "pointer", "pointer"]);
const logTag = Memory.allocUtf8String("SPRK-Accounts");
const log = message => androidLog(4, logTag, Memory.allocUtf8String(message));
const text = value => value && !value.isNull() ? value.content : "";

function characters(value) {
    if (typeof value === "string") return value;
    value = Java.cast(value, Java.use("java.lang.CharSequence"));
    let result = "";
    // Editable is exposed through an interface on this Android runtime; its
    // JavaScript toString is Object.toString, not CharSequence.toString.
    for (let i = 0; i < value.length(); i++) {
        const character = value.charAt(i);
        result += typeof character === "number" ? String.fromCharCode(character) : character;
    }
    return result;
}

function ui(action) {
    Java.perform(() => Java.scheduleOnMainThread(() => {
        try { action(); } catch (error) { log("UI error: " + error.message); }
    }));
}

function post(url, body, bearer) {
    const connection = Java.use("java.net.URL").$new(url).openConnection();
    const http = Java.cast(connection, Java.use("java.net.HttpURLConnection"));
    http.setConnectTimeout(15000);
    http.setReadTimeout(25000);
    http.setRequestMethod("POST");
    http.setRequestProperty("Content-Type", "application/json");
    if (bearer) http.setRequestProperty("Authorization", "Bearer " + bearer);
    http.setDoOutput(true);
    try {
        const bytes = Java.use("java.lang.String").$new(JSON.stringify(body)).getBytes("UTF-8");
        const stream = http.getOutputStream();
        stream.write(bytes); stream.close();
        const status = http.getResponseCode();
        const response = status >= 400 ? http.getErrorStream() : http.getInputStream();
        if (response === null) throw new Error("Server returned HTTP " + status);
        const reader = Java.use("java.io.BufferedReader").$new(
            Java.use("java.io.InputStreamReader").$new(response, "UTF-8"));
        let result = "", line;
        while ((line = reader.readLine()) !== null) result += characters(line);
        reader.close();
        const json = JSON.parse(result);
        if (status >= 400 || json.status !== "success") {
            throw new Error(json.InternalErrorMessage || json.message || "Sign-in failed (HTTP " + status + ").");
        }
        return json.data;
    } finally { http.disconnect(); }
}

function fail(message) {
    busy = false;
    ui(() => {
        if (!form) return;
        form.message.setText(Java.use("java.lang.String").$new(message));
        form.submit.setEnabled(true);
        form.register.setEnabled(true);
        form.password.setText(Java.use("java.lang.String").$new(""),
            Java.use("android.widget.TextView$BufferType").EDITABLE.value);
    });
}

function authenticate(register) {
    if (busy || !form) return;
    const username = characters(form.username.getText()).trim().toLowerCase();
    const password = characters(form.password.getText());
    const confirm = characters(form.confirm.getText());
    if (!/^[a-z0-9_-]{3,32}$/.test(username)) return fail("Enter a username with 3–32 letters, numbers, underscores or hyphens.");
    if (password.length < 8 || password.length > 128) return fail("Enter a password with 8–128 characters.");
    if (register && password !== confirm) return fail("The passwords do not match.");
    const selected = form.server.getSelectedItemPosition();
    const server = choices[selected];
    if (!server) return fail("Select a server.");
    busy = true;
    form.submit.setEnabled(false); form.register.setEnabled(false);
    form.message.setText(Java.use("java.lang.String").$new(register ? "Creating account…" : "Signing in…"));
    const worker = Java.registerClass({
        name: "net.sprk.AccountRequest" + Date.now(),
        implements: [Java.use("java.lang.Runnable")],
        methods: { run() {
            try {
                const root = server.endpoint + "/api/auth/";
                const tokens = post(root + (register ? "register" : "login"), { username, password });
                const verified = post(root + "token/verify", {}, tokens.access_token);
                if (!verified || !verified.id || !verified.session || verified.id !== tokens.id) {
                    throw new Error("Account authenticated, but the game ticket could not be verified.");
                }
                // Native state changes must run on Unity's thread, never on this worker.
                queued = { id: verified.id, ticket: verified.session, server: server.name };
                log("Password authentication and game-ticket verification succeeded");
                ui(() => {
                    if (form) {
                        const empty = Java.use("java.lang.String").$new("");
                        const editable = Java.use("android.widget.TextView$BufferType").EDITABLE.value;
                        form.password.setText(empty, editable);
                        form.confirm.setText(empty, editable);
                        form.dialog.dismiss(); form = null;
                    }
                });
            } catch (error) { fail(error.message || "Unable to reach the server. Please try again."); }
        } }
    });
    Java.use("java.lang.Thread").$new(worker.$new()).start();
}

function showForm() {
    ui(() => {
        if (form) return;
        const activity = Java.use("com.unity3d.player.UnityPlayer").currentActivity.value;
        const JString = Java.use("java.lang.String");
        const Linear = Java.use("android.widget.LinearLayout");
        const layout = Linear.$new(activity); layout.setOrientation(1); layout.setPadding(32, 16, 32, 16);
        function label(value) {
            const view = Java.use("android.widget.TextView").$new(activity);
            view.setText(JString.$new(value)); layout.addView(view); return view;
        }
        const server = Java.use("android.widget.Spinner").$new(activity);
        const names = Java.use("java.util.ArrayList").$new();
        choices.forEach(choice => names.add(JString.$new(choice.name)));
        const adapter = Java.use("android.widget.ArrayAdapter").$new(activity, 17367048, names);
        adapter.setDropDownViewResource(17367049); server.setAdapter(adapter); layout.addView(server);
        function input(hint, type) {
            const view = Java.use("android.widget.EditText").$new(activity);
            view.setHint(JString.$new(hint)); view.setSingleLine(true); view.setInputType(type);
            layout.addView(view); return view;
        }
        const username = input("Username", 0x80001);
        const password = input("Password", 0x81);
        const confirm = input("Confirm password (new accounts only)", 0x81);
        const message = label("Sign in with your SPRK account, or create a new account.");
        const submit = Java.use("android.widget.Button").$new(activity);
        const register = Java.use("android.widget.Button").$new(activity);
        submit.setText(JString.$new("Sign in")); register.setText(JString.$new("Create account"));
        const Click = Java.registerClass({
            name: "net.sprk.AccountClick" + Date.now(),
            implements: [Java.use("android.view.View$OnClickListener")],
            methods: { onClick(view) {
                try { authenticate(view.equals(register)); }
                catch (error) { log("Account form error: " + error.stack); fail("Unable to read the account form. Please try again."); }
            } }
        });
        submit.setOnClickListener(Click.$new()); register.setOnClickListener(Click.$new());
        layout.addView(submit); layout.addView(register);
        const dialog = Java.use("android.app.AlertDialog$Builder").$new(activity)
            .setTitle(JString.$new("SPRK Account")).setView(layout).setCancelable(false).create();
        form = { dialog, server, username, password, confirm, message, submit, register };
        dialog.show();
        dialog.getWindow().setSoftInputMode(0x20);
        log("Username/password form opened");
    });
}

setImmediate(() => {
    log("Adapter loading");
    Il2Cpp.perform(() => {
        const game = Il2Cpp.domain.assembly("Assembly-CSharp").image;
        // The original AppGuard bootstrap is removed from classes.dex as well.
        // Keep its old game-facing facade inert; SPRK authenticates on the server.
        const guard = game.class("AppGuardUnityManager");
        const guardFacade = guard.alloc();
        guard.field("_instance").value = guardFacade;
        guard.field("AppGuardAuthId").value = Il2Cpp.string("");
        guard.field("IsInitialized").value = true;
        guard.field("IsEmulateDetected").value = false;
        guard.method("get_Instance").implementation = function () { return guardFacade; };
        for (const name of ["start", "onS2AuthTryCallback", "onViolationCallback", "setUserId",
            "setUniqueClientId", "RegisterAppGuardGui", "DrawGui"]) {
            guard.method(name).implementation = function () {};
        }
        guard.method("getAppGuardUdid").implementation = function () { return Il2Cpp.string(""); };
        const select = game.class("NGame2.NUI.NWindow.LoginSelect");
        const manager = game.class("NGame2.NLogin.LoginManager");
        const data = game.class("NGame2.NLogin.LoginData");
        const gameState = game.class("NGame2.NLogin.NState.StateBase_LoginGameServer");

        // Never reuse a native guest/provider login saved before this adapter.
        data.method("get_LastLoginMethod").implementation = function () { return 0; };
        const open = select.method("Open", 4);
        open.implementation = function (listener, previous, servers, lastServer) {
            this.method("Open", 4).invoke(listener, 0, servers, lastServer);
            loginView = this;
            session = listener;
            choices = [];
            for (let i = 0; i < servers.length; i++) {
                const server = servers.get(i);
                if (!server.method("get_Opened").invoke()) continue;
                const host = text(server.method("get_Host").invoke());
                const secure = server.method("get_Https").invoke();
                choices.push({ name: text(server.method("get_Name").invoke()),
                    endpoint: (secure ? "https://" : "http://") + host.replace(/\/$/, "") });
            }
            const table = this.field("LoginButtonTable").value;
            if (!table.isNull()) table.method("get_gameObject").invoke().method("SetActive").invoke(false);
            busy = false; pendingTicket = null;
            showForm();
        };
        for (const name of ["OnClickGuest", "OnClickFacebook", "OnClickGooglePlus", "OnClickGameCenter", "OnClickApple", "OnClickLoginByAccountId", "LoginByAccountId"]) {
            select.method(name).implementation = function () { showForm(); };
        }
        // Preserve the native state lifecycle and server checks, replacing only
        // its identity and verification input with a server-issued game ticket.
        const constructor = gameState.method(".ctor", 4);
        constructor.implementation = function (id, method, verification, popup) {
            if (!pendingTicket) throw new Error("SPRK game login requires password authentication");
            this.method(".ctor", 4).invoke(Il2Cpp.string(pendingTicket.id), 1,
                Il2Cpp.string(pendingTicket.ticket), false);
            log("Authenticated identity passed to native game login");
        };
        const update = manager.method("ChainUpdate");
        update.implementation = function () {
            if (queued) {
                const result = queued; queued = null; pendingTicket = result;
                try {
                    this.method("SetServerName").invoke(Il2Cpp.string(result.server));
                    data.method("set_LastLoginId").invoke(Il2Cpp.string(result.id));
                    this.field("LastLoginVerificationInfo").value.method("set_Value").invoke(Il2Cpp.string(result.ticket));
                    session.method("NGame2.NUI.NWindow.LoginSelect.Listener.OnSelect", 4)
                        .invoke(loginView, 1, Il2Cpp.string(result.server), true);
                } catch (error) { log("Native handoff failed: " + error.message); busy = false; showForm(); }
            }
            this.method("ChainUpdate").invoke();
        };
        log("Native login hooks installed (Unity " + Il2Cpp.unityVersion + ")");
    }).catch(error => log("Initialization failed: " + error.stack));
});
