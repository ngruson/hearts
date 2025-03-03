window.moveCardAnimation = (cardId) => {
    const card = document.getElementById(cardId);
    if (card) {
        card.style.animation = 'moveToNextPlayer 1s forwards';
        setTimeout(() => {
            card.style.animation = ''; // Reset animation
        }, 1000);
    }
};
