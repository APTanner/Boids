# Boids
![Boids](https://github.com/lunchbox-boy/Boids/assets/53225660/fc220b3c-d581-42f4-bc2b-ec448bf32f35)
Unity boids project

Built to play around with vector maths and simulations / emergent behavior
## Features
* Standard boid characteristics 
    * Alignment
    * Cohesion
    * Separation
* Staying within bounds
* Running away from position
* Circling around single position (bait ball)

## Design
* Not the most efficient
    * Reallocates memory every fixed update
    * O(n^2) nested for loop
* Compute Shader
    * Uses GPU to speed up computation
    * Probably needs some thread optimization; currently just one-dimensional batches of 1024 `BoidData`; \**learning for the future*\*
