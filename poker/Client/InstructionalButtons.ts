interface IButton {
    text: string;
    button;
}

class InstructionalButtons {
    scaleform: GTA.Scaleform;
    buttons: IButton[];

    constructor() {
        this.scaleform = API.requestScaleform("instructional_buttons");
        this.buttons = [];
    }

    init() {
        this.scaleform.CallFunction("CLEAR_ALL");
        this.scaleform.CallFunction("TOGGLE_MOUSE_BUTTONS", 0);
        this.scaleform.CallFunction("CREATE_CONTAINER");

        for (let i = 0; i < this.buttons.length; i++) {
            this.scaleform.CallFunction("SET_DATA_SLOT", i, this.buttons[i].button, this.buttons[i].text);
        }

        this.scaleform.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
    }

    draw() {
        this.scaleform.Render2D();
    }

    addButtonStr(text: string, button: string): IButton {
        let newBtn: IButton = {
            text: text,
            button: button
        };

        this.buttons.push(newBtn);

        this.init();
        return newBtn;
    }

    addButtonInt(text: string, button: number): IButton {
        let newBtn: IButton = {
            text: text,
            button: button
        };

        this.buttons.push(newBtn);
        this.init();
        return newBtn;
    }

    removeButton(btn: IButton): void {
        let indx: number = this.buttons.indexOf(btn);
        if (indx > -1)
            this.buttons.splice(indx, 1);
    }

    clear() {
        this.buttons = [];
    }
}

function createInstructionalButtons(): InstructionalButtons {
    return new InstructionalButtons();
}

var menuOpen: boolean = false;
var instance: InstructionalButtons = new InstructionalButtons();

API.onResourceStart.connect(() => {
    instance.init();
});

API.onUpdate.connect(() => {
    if (API.isControlJustPressed(200) && !menuOpen) {
        menuOpen = true;
    } else if (API.isControlJustReleased(200) && menuOpen) {
        menuOpen = false;
        if (instance.buttons.length > 0) instance.init();
    }
    
    if (instance.buttons.length > 0)
        instance.draw();
});
