/// <reference path="types-gt-mp/index.d.ts" />

var g_menu = API.createMenu("Greetings", "Choose greeting:", 0, 0, 3, false);

g_menu.AddItem(API.createMenuItem("Handshake", "Handshake"));
g_menu.AddItem(API.createMenuItem("Fist Bump", "Fist Bump"));
g_menu.AddItem(API.createMenuItem("High Five", "High Five"));

g_menu.OnItemSelect.connect(function (sender, item, index) {
    API.triggerServerEvent("GREET", index);

    API.showCursor(false);
    g_menu.Visible = false;
});

g_menu.OnMenuClose.connect(function (sender) {
    API.triggerServerEvent("CANCEL_GREET");

    API.showCursor(false);
    g_menu.Visible = false;
});

API.onServerEventTrigger.connect(function (name, args) {
    if (name == "GREET_MENU") {
        API.showCursor(true);
        g_menu.Visible = true;
    }
});