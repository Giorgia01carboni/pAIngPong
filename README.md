# Ping pAIng: Using ML Agents to Play Ping Pong

A Unity project where an AI agent learns to play Ping Pong using reinforcement learning.

---

## Overview

This project demonstrates how to use Unity's ML-Agents toolkit to develop an AI agent capable of playing a 2D Ping Pong game. The agent is trained using the PPO algorithm, focusing on optimizing its paddle movement to keep the disk in play. Key features include training configurations, disk movement logic, and a custom reward structure that guides the agent's learning process.

---

## Features

- **Reinforcement Learning**: The agent uses PPO for stable and efficient learning.
- **Custom Training Environment**: A 2D Pong field with dynamic collision handling and edge case management.
- **Reward System**: Designed to encourage optimal paddle movement and penalize out-of-bounds misses.
- **Performance Visualization**: Training progress visualized using TensorBoard.

---

## Technical Details

- **Unity Version**: Compatible with Unity ML-Agents version 3.0.0.
- **Training Frameworks**: Utilizes PyTorch 1.13.1 for reinforcement learning.
- **Training Setup**: Parallel environment training with 15 instances of the game field for faster sample collection.

---

## Results

The training achieved the following:

- **Improved Cumulative Rewards**: Demonstrating better disk control over time.
- **Stable Policy Loss**: Indicating effective learning with minimal instability.
- **Reduced Entropy**: Transition from exploration to exploitation as the agent refined its strategy.
