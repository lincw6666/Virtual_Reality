# VR Airplane Shooting Game

## Introduction
This is a VR airplane shooting game. Try to shoot as many targets as you can.

## Demo

[![VR Airplane Shooting Game](https://i.imgur.com/YpGHgl6.jpg)](https://drive.google.com/file/d/1VvrIE_kYWU9AMQR3MIl5HXBwEBmKrwDB/preview)

## What I've done:
- Use the laser pointer for the menu.
- Plane movements: roll, yaw and pitch.
- Menu and pause button while playing the game.
- A game over UI let player restart, go back to menu or exit the game.
- Sounds for engine and machine gun.
- Randomly appearing shooting targets.
- Tutorial (Not user friendly for 2D fallback XD).
 
## Control the plane by the control stick in the cockpit.
- The control stick is a part of the mesh of the cockpit. But I figure out
  which vertices construct the control stick. Then I did per vertex transformation
  to let the control stick movable.
- The details are in the script: StickController.cs

## I create a tutorial only for playing on the VR device. The following inputs mapping are for 2D fallback:

*** I know it's complicated. I recommand just play it on the VR device XD ***

    |-------------------------------------------|
    |                 Start Scene               |    
    |-------------------------------------------|
    |        Action        |       Mapping      | 
    |-------------------------------------------|
    | Pickup laser pointer | Mouse: left button |
    | Press UI button      | Keyboard: f        |
    |-------------------------------------------|

    |--------------------------------------------|
    |                  Game  Scene               |    
    |--------------------------------------------|
    |        Action         |       Mapping      | 
    |--------------------------------------------|
    | Hold control stick    | Mouse: left button |
    | Press UI button       | Keyboard: f        |
    | Fire (left/right)     | Keyboard: f/j      |
    | Accelerate/Decelerate | Keyboard: z        |
    | Yaw (left/right)      | Keyboard: 1/2      |
    |--------------------------------------------|

## Known bug
- It'll crash when we start the game after the tutorial. I'm trying to fix it.
  Because I think the stucture of my program likes a shit. I'll design a new 
  structure. I don't know how much time it'll take. Thus, I hand in the homework
  first.
