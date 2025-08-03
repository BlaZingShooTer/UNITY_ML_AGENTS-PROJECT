# UNITY_ML_AGENTS-PROJECT
# 🐦 Unity ML-Agents – Hummingbird Environment

This project explores reinforcement learning using **Unity ML-Agents Toolkit** through the **Hummingbird environment**, where an agent learns to fly and collect nectar efficiently. The project was designed to understand the basics of training agents using Unity simulations and reward-based learning.

---

## 🧠 Objective

Train a hummingbird agent in a simulated 3D environment to fly, hover, and collect nectar using reinforcement learning techniques.





---

## 🚀 Features

- 🛠️ **Unity ML-Agents Integration**: Connected Unity with Python ML-Agents toolkit to run training sessions
- 🎯 **Reward-Based Learning**: The agent learns nectar collection behavior based on positive/negative rewards
- 🔧 **Hyperparameter Tuning**: Adjusted basic training parameters to improve agent learning
- 📈 **Progress Tracking**: Monitored learning behavior using TensorBoard
- 🤖 Built using **Unity 202X.X** and **ML-Agents Toolkit vX.X.X**

---

## 🛠️ Tech Stack

| Tool | Description |
|------|-------------|
| **Unity** | 3D simulation and environment logic (C#) |
| **ML-Agents Toolkit** | Reinforcement learning framework for Unity |
| **Python** | Running training sessions and managing rewards/logs |
| **TensorBoard** | Visualizing training progress and agent performance |

---

## 🧩 How It Works

1. The Unity environment simulates a 3D world with flowers containing nectar
2. The hummingbird agent is controlled using a neural network trained via reinforcement learning
3. A reward system is defined to encourage correct behaviors like reaching nectar, hovering, or avoiding walls
4. Training is run using ML-Agents Python APIs, and results are visualized through TensorBoard

---

## 📷 Screenshots

TensorBoard result
<img width="1920" height="916" alt="Screenshot (37)" src="https://github.com/user-attachments/assets/724a3ad0-4a8c-48cc-9b2c-9819f7196c99" />
<img width="1920" height="915" alt="Screenshot (38)" src="https://github.com/user-attachments/assets/e2ed9746-ff84-49b7-8b1f-095437f53806" />
<img width="859" height="499" alt="Screenshot (39)" src="https://github.com/user-attachments/assets/eb356f1c-6ccc-4260-b233-985b9397ea34" />


https://github.com/user-attachments/assets/6f6b23d7-55e6-4590-8dbc-7ffc56414cc9

---

## 📚 Learnings & Highlights

- Learned how to set up and configure Unity ML-Agents for a prebuilt environment  
- Explored reward shaping, training step configuration, and curriculum learning basics  
- Understood how to bridge Unity and Python for simulation-driven ML training  

---

## 🔧 Getting Started

> Prerequisites: Unity 202X.X, Python 3.8+, ML-Agents Toolkit installed

```bash
# Navigate to ML-Agents directory
mlagents-learn config/trainer_config.yaml --run-id hummingbird-test --env=Hummingbird.exe
