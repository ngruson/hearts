window.moveCardAnimation = (cardId) => {
    const card = document.getElementById(cardId);
    if (card) {
        card.style.animation = 'moveToNextPlayer 1s forwards';
        setTimeout(() => {
            card.style.animation = ''; // Reset animation
        }, 1000);
    }
};

window.setTextContent = (element, text) => {
    if (!element) {
        return null;
    }
    element.textContent = text;
};

function getElementPosition(id) {
    const element = document.getElementById(id);
    if (!element) {
        return null;
    }
    const rect = element.getBoundingClientRect();
    return { top: rect.top, left: rect.left };
}

function moveCardsToWinner() {
    const trickCards = getAllImagesInCenterArea();
    const winner = document.getElementById("tricks");

    if (trickCards.length === 0 || !winner) {
        return;
    }

    // Get the winner's position
    const winnerRect = winner.getBoundingClientRect();

    const offsetX = winnerRect.left;
    const offsetY = winnerRect.top;

    // Move cards to winner
    for (let card of trickCards) {
        card.style.position = 'absolute';
        card.style.top = `${offsetY}px`;
        card.style.left = `${offsetX}px`;
    }
}

function getAllImagesInCenterArea() {
    const centerArea = document.getElementById('center-area');
    if (!centerArea) {
        return [];
    }
    return centerArea.getElementsByTagName('img');
}

function toggleDarkMode(isDarkMode) {
    if (isDarkMode) {
        document.documentElement.classList.add('dark');
    } else {
        document.documentElement.classList.remove('dark');
    }
}
