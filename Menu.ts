/// <reference path="types-gt-mp/index.d.ts" />

var greet_menu = API.createMenu("Greetings", "Choose greeting:", 0, 0, 3, false);

greet_menu.AddItem(API.createMenuItem("Handshake", "Handshake"));
greet_menu.AddItem(API.createMenuItem("Kiss", "Kiss"));
greet_menu.AddItem(API.createMenuItem("High Five", "High Five"));

greet_menu.OnItemSelect.connect(function (sender, item, index) {
    API.triggerServerEvent("GREET", index);

    API.showCursor(false);
    greet_menu.Visible = false;
});

greet_menu.OnMenuClose.connect(function (sender) {
    API.triggerServerEvent("CANCEL_GREET");

    API.showCursor(false);
    greet_menu.Visible = false;
});

API.onServerEventTrigger.connect(function (name, args) {
    if (name == "GREET_MENU") {
        API.showCursor(true);
        greet_menu.Visible = true;
    }
});