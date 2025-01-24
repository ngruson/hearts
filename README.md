# Hearts Card Game

Welcome to the Hearts card game! This project is a web-based implementation of the classic Hearts card game using Blazor WebAssembly and the actor pattern for game logic.

## Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Setup](#setup)
- [Project Structure](#project-structure)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license

## Overview
The Hearts card game is a trick-taking game where the objective is to avoid taking certain cards that carry penalty points. This web-based version allows players to join or create games, play with others in real-time, and enjoy the classic gameplay in a modern format.

## Features
- Multiplayer game lobby for joining and creating games
- Real-time gameplay with SignalR
- Actor-based game logic for robust and scalable game management
- Intuitive user interface with interactive card selection and trick-taking

## Setup
### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (for managing front-end dependencies)

### Getting Started
1. **Clone the Repository**
   ```bash
   git clone https://github.com/ngruson/hearts.git
   cd hearts

## Running the Application
Open your web browser and navigate to https://localhost:5001.
Create a new game or join an existing game from the lobby.
Follow the on-screen instructions to play the game.

## Playing the Game
- Each player is dealt 13 cards.
- Select three cards to pass to an opponent at the beginning of each round.
- Play cards to avoid taking hearts and the queen of spades.
- The player with the lowest score at the end of the game wins.

## Contributing
Contributions are welcome! Please follow these steps to contribute:
- Fork the repository.
- Create a new branch (git checkout -b feature/your-feature).
- Commit your changes (git commit -m 'Add your feature').
- Push to the branch (git push origin feature/your-feature).
- Create a pull request.