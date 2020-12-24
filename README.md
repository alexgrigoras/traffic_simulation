# Traffic Simulation

## Description
Multi-agent system that manages traffic on a street layout with multiple intersections. The streets and intersections are configured as in the following image:

![Intersection image](images/intersection.png)

At each intersection, the car agents decide which way to go in a greedy manner, i.e they will choose a direction which:
- is either left, right, or forward;
- is selected such that the total distance from the entry point to exit is equal to the Manhattan distance between the entry point and the exit; for example, if a car wants to get from B to O, it should stay within the rectangle B-C-O-N;
- has no obstacles / the least amount of obstacles.

Possible obstacles are:
- **binary:** a red street light for that direction;
- **continuous:** the amount of traffic in the desired direction (the number of cars in the corresponding road segment).

The system should be controllable by multiple user-defined parameters, which should be read from a simple config file. The control parameters are:
- the intelligence of traffic lights:
    - non-intelligent traffic light: the switching time is constant, no matter the traffic;
    - intelligent: the switching time depends on the traffic; in this case, another parameter would be the level of intelligence:
        - level 1: the street light has knowledge of the amount of traffic in its adjacent street segments;
        - level 2: the street light has knowledge of the amount of traffic as far as two street segments away from its position;
        - level 3: the street light has full knowledge of the amount of traffic, on all street segments.
- the rate with which cars are generated at each entry point A, B, C, D. Rate = cars/second, cars/minute, etc;
- whether cars will prioritize street lights or the amount of traffic, meaning:
    - whether a car at an intersection will choose a street segment with a green light or
    - whether a car will wait at a red light if there is lower traffic on the following street segment.

## Implementation
The elements of the multi-agent system:
- Environment: turn-based
- Agents:
    - Intersection
    - Cars: turn-based
    - Traffic lights: turn-based
    
Environment:
- 7x7 grid

Car agent:
- Position (x, y)
- State:
    - Moving (front, left, right)
    - Waiting
- Starting position (A / B / C / D)
- Final destination (M / N / O / P)

Traffic light agent:
- Position (x, y)
- State:
    - Green - vertical, Red - horizontal
    - Red - vertical, Green - horizontal

Control parameters:
- The intelligence of traffic lights
- Cars rate
- Cars priority

## License
The software is licensed under the MIT license.