"use strict";
function setHandCards(firstCard, secondCard) {
    let firstCardNode = document.getElementById("firstHandCard");
    let secondCardNode = document.getElementById("secondHandCard");
    let newFirst = firstCard.split(':', 2);
    let newSecond = secondCard.split(':', 2);
    // k:diams
    firstCardNode.setAttribute("class", "card rank-" + newFirst[0].toLowerCase() + " " + newFirst[1]);
    secondCardNode.setAttribute("class", "card rank-" + newSecond[0].toLowerCase() + " " + newSecond[1]);
    firstCardNode.firstElementChild.innerHTML = newFirst[0].toUpperCase();
    secondCardNode.firstElementChild.innerHTML = newSecond[0].toUpperCase();
    firstCardNode.lastElementChild.innerHTML = "&" + newFirst[1] + ";";
    secondCardNode.lastElementChild.innerHTML = "&" + newSecond[1] + ";";
}
function clearTableCards() {
    let tableul = document.getElementById("mainTableList");
    while (tableul.firstChild) {
        tableul.removeChild(tableul.firstChild);
    }
}
function addTableCard(card, selected = false) {
    let newCard = card.split(':', 2);
    let newli = document.createElement("li");
    let tableul = document.getElementById("mainTableList");
    let newdiv = document.createElement("div");
    newdiv.setAttribute("class", "card rank-" + newCard[0].toLowerCase() + " " + newCard[1]);
    let newspan = document.createElement("span");
    newspan.setAttribute("class", "rank");
    newspan.innerHTML = newCard[0].toUpperCase();
    let suitspan = document.createElement("span");
    suitspan.setAttribute("class", "suit");
    suitspan.innerHTML = "&" + newCard[1] + ";";
    newdiv.appendChild(newspan);
    newdiv.appendChild(suitspan);
    if (selected) {
        let strong = document.createElement("strong");
        strong.appendChild(newdiv);
        newli.appendChild(strong);
    }
    else {
        newli.appendChild(newdiv);
    }
    tableul.appendChild(newli);
}
function clearTimers() {
    let tableul = document.getElementById("timerbarsList");
    while (tableul.firstChild) {
        tableul.removeChild(tableul.firstChild);
    }
}
function addTimerText(label, text, selected = false) {
    let newli = document.createElement("li");
    let tableul = document.getElementById("timerbarsList");
    let newdiv = document.createElement("div");
    newdiv.setAttribute("class", "timerbar");
    let newimg = document.createElement("img");
    if (!selected)
        newimg.setAttribute("src", "images/all_black_bg.png");
    else
        newimg.setAttribute("src", "images/all_grey_bg.png");
    let labelp = document.createElement("p");
    labelp.setAttribute("class", "timerbar_label");
    labelp.innerHTML = label;
    let textp = document.createElement("p");
    textp.setAttribute("class", "timerbar_text");
    textp.innerHTML = text;
    newdiv.appendChild(newimg);
    newdiv.appendChild(labelp);
    newdiv.appendChild(textp);
    newli.appendChild(newdiv);
    tableul.appendChild(newli);
}
function addTimerCards(label, cardLeft, cardRight) {
    let newLeft = cardLeft.split(':', 2);
    let newRight = cardRight.split(':', 2);
    let newli = document.createElement("li");
    let tableul = document.getElementById("timerbarsList");
    let newdiv = document.createElement("div");
    newdiv.setAttribute("class", "timerbar");
    let newimg = document.createElement("img");
    newimg.setAttribute("src", "images/all_black_bg.png");
    newdiv.appendChild(newimg);
    let labelp = document.createElement("p");
    labelp.setAttribute("class", "timerbar_label");
    labelp.innerHTML = label;
    newdiv.appendChild(labelp);
    {
        let card1div = document.createElement("p");
        card1div.setAttribute("class", "card rank-" + newLeft[0].toLowerCase() + " " + newLeft[1] + " timerbar_text_card timber_card_left");
        let newspan = document.createElement("span");
        newspan.setAttribute("class", "rank");
        newspan.innerHTML = newLeft[0].toUpperCase();
        let suitspan = document.createElement("span");
        suitspan.setAttribute("class", "suit");
        suitspan.innerHTML = "&" + newLeft[1] + ";";
        card1div.appendChild(newspan);
        card1div.appendChild(suitspan);
        newdiv.appendChild(card1div);
    }
    {
        let card1div = document.createElement("p");
        card1div.setAttribute("class", "card rank-" + newRight[0].toLowerCase() + " " + newRight[1] + " timerbar_text_card timber_card_right");
        let newspan = document.createElement("span");
        newspan.setAttribute("class", "rank");
        newspan.innerHTML = newRight[0].toUpperCase();
        let suitspan = document.createElement("span");
        suitspan.setAttribute("class", "suit");
        suitspan.innerHTML = "&" + newRight[1] + ";";
        card1div.appendChild(newspan);
        card1div.appendChild(suitspan);
        newdiv.appendChild(card1div);
    }
    newli.appendChild(newdiv);
    tableul.appendChild(newli);
}
