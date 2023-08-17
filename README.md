> :warning: **If you are current student at the University of Utah**:
> 
> Plagirisim is against the School of Computings code of conduct.
> All work done here was from scract and/or skeleton code provided by the class.
>
> You will learn nothing if you cheat. You are cheating on yourself.
> This project made me into a great programmer. Do the same for yourself.
>
> The project is only to showcase my past work and show compentency in this subject




# game-los_coders_game
game-los_coders_game created by GitHub Classroom

Authors: Cristian Tapiero & Kevin Pineda
*********************************************************************************************************************************
Server:

Design Decisions:

- For our server  we took several design desicions that would make our game more enjoyable and dynamic.
first of all we added 2  power ups at the beginning of the game in a random location with the initial information 
that we send to the client so there is no a long delay for them to appear and players can compete for them. After being picked up
they appear in random locations and their delay is also random.

- the point system is based on the players killing other players. when a player kills others with a projectile one point is added
to its dashboard score. if the player uses the beam to kill others his score can be increased up to the amount of tanks killed by
the beam trajectory when its fired. so for example if 3 tanks are on the beam trajectory when its fired then the player that fired such beam 
will get 3 points.

- We used a Randomized method to respawn the tanks in the world and every time they die, they respawn in a random location 
  also, when you die the explosion animation stays until the player respawns in the new location. This helps the player
  indicate they are unable to move or shot and indicates the rest of the players. 

- A player is only able to pick up to 3 powerups and keep them until death.

- Players lose all their powerups if they die.

- Players are able to move and fire the beam at the same time without killing themselves.

- Players are able to stack points in one beam kill if they manage to get multiple targets in the same beam. 
 
- When players decide to disconnect from the server, we implemented a way for the server to handle this gracefully, which means
  that the server is not going to crash and the players left are able to play with out a problem.



  Our major reference was the Microsoft Documentation on C# classes.

Software Engineering Hurdles:

The initial week of starting the connections between the server and the model was the bulk of our work for the first the week. The following week was connecting the server, server settings, reading the xml, and connecting the world. We took the approach of taking little bites of work for each section (i.e collisions, respawning, controls). When impliments the differents parts we would work on other parts since some of them relied on the others. Finding the collisions and mathmatically adjusting them based on the vectors was one of the longest discussions on how to logically code it. 

Respawing relied on a stable and logical collisin checks. We also went from using global variables to fields in the objects classes (tank, projectile, powerup). This helped us access them for each individal object and prevent bugs that we happening. We also started in the server and made most fields public with public setters. This was not good design pratice. We were able to move all the game state code in to the world class. This helped us keep our fields and methods to be used internally without the need for the server and model to interlap.

Entry Dates: Commits on Dec 1 , 2021 - Dec 12, 2021
*********************************************************************************************************************************
Client

Design Decisions: When we started designing the application, we took into account the Model View Controller (MVC) breakdown and decided on starting with the Controller portion first. This
is due to having the most complex logic, where building the base of the code, would be the longest portion. We understood that it required constant adjustments that were determined by
the Model and View.

Once we completed the initial portion of the Controller, we then focused on finilazing the Model which was about 10% of the code that was written. In our View,
we decided on keeping the images in seperate lists that would be availible to us throughout the class. This allowed us to condense our code since we could take advantage of the
List data structure in any aspect that required images. The Last thing we added was a property in our Beam object to allow a Frame per Second counter in our View. 

In our View, we took most of our time at about 60% of the code we wrote. We added checks before the connect buttone was pressed to ensure that a server and a name were being passed. After the connect button is pressed, we also check if
the name is 16 characters or less. Our breakdown of the drawers was done to keep code condensed, accurate, and understandable if we wanted to added any future features or a new developer
takes control. Currently our projectiles are assigned a random color that is determined by one tank and all other tanks follow with the same color projectile. This allows consisitency
in every projectile being used and negates confusion. Drawing the walls was a difficult task to understand and try to implement. We finally came up with a solution by drawing two vectors
and translating it to code through forloops, addition, and understanding how vectors work.  

Using delegates was a major decesions in being able to pass information to the MVC components. This allowed us to have two seperate components link to each other. Our major reference was the Microsoft Documentation on C# classes. 

The key commands are detailed below:

UP              : W
DOWN            : S
LEFT            : A
RIGHT           : D
ATTACK          : Left Mouse Button
SPECIAL ATTACK  : Right Mouse Button

The Tank has 3 Hit Points. A projectile can deducte one hit point and a beam can deducte all hitpoints. Projectiles cannot go through walls, but beams can go through any wall. 

Entry Dates: Commits on Nov 20, 2021 - Dec 1, 2021
