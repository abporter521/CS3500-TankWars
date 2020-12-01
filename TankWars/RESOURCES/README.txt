This is the README file for the final project of CS3500.  This project was created by team FlyBoys.  Team members are Adam Scott and Andrew Porter. 3 December 2020


MODEL
Our model consisted of various classes that are found in the game.  These objects are tanks, walls, projectiles, beams, powerups, world, Vector 2D and ControlCommand.  A brief
overview of each of these will be given.

Tanks - Tank objects store all the necessary fields received by the server per PS8 instructions. This includes a living state, ID, turret direction, body direction, hitpoints,powerUp number, fired status, and an individual frame counter, etc.
Inside of the tank class file is a contructor with these fields.  For the purposes of this particular assignment, we are content with the only methods being in this class
are getter and setter methods for these fields.

Walls - Walls are a simpler class containing only two points of the endpoints of a wall segment.  Walls also contain a unique ID.  Again, only getter methods in this class.

Projectiles - Contain the fields as per PS8 instructions and have getter/setter methods

Beams - Beams have an origin and direction.  Again only getter properties for this class

PowerUps- Same as other classes.

Vector2D - Class given to us by the teacher.  This class is not directly invoked, but rather a subclass of the other objects such as tanks and projectiles.

World - This is a class we specifically created as a parent object to hold all the other objects.  Contains dictionaries of tanks, walls, powerups, projectiles and beams.
Dictionary keys are the object's unique ID number, with the value being that specific object type. The World class has properties that fetch these dictionaries so that
they can be added or enumerated through where needed in the controller or Drawing Panel.  The world also has a size attribute that determines the width and height of the
actual world when seen by the player.

ControlCommand - This class is basically a struct that holds data for the movement of a player tank, turret direction, and weapon use.  These objects are created on the spot
in the game controller to be serialized and sent to the server.


CONTROL
Our control has 2 main components to it.  The network controller handles establishing a connection with, sending, and receiving data from a server.  This controller
is directly handled by the Game Controller class and is not a concern of this assignment.

The Game Controller is the main class that handles a lot of the logic to the game.  After connecting with the server, it receives the startup data of world
size and playerID. It will then receive walls and call a helper method that adds these walls to a method called UpdateWorld. More on that later.  After this inital contact,
the gamecontroller begins an event loop to be constantly receiving data from the server. Any errors during this time will fire an Error event that displays a message to the user.

Our UpdateWorld method is the core of the world building logic. This method receives a JSON string and deserializes it based on type. Then, in a thread-safe manner, will 
add and remove these objects to the world object.  After processing the message, it will fire an UpdateWorld event that tells the view to redraw. For beam objects that do not have an alive or 
dead state, there is a timer that uses a callback method to manage the lifespan of the beam.  It is important that this method function properly as the world object is the same that is used
by the Drawing Panel to display the world to the user.
The Game Controller also handles movement from the users end with a handful of methods.  The movement methods trigger boolean flags describing the direction of the tank. Firing method
sets boolean flags regarding if a weapon was fired. The turret method receives mouse input and converts it to a vector 2D that is uses to modify the tank turret direction. These
three methods all call the UpdateTankMovment method which serializes a ControlCommand object holding each of these actions.


VIEW 
The view for our game has two main components.  The Drawing Panel and the Form.  
Our form is very similar to layout to the solution provided.  We have spot for server name, player name and a connect button.  Hitting the enter key will trigger a connect button push.  
Hitting the escape key will trigger the form to close out.  We have in the form multiple event handlers that for mouse movement, button pressing, and mouse clicking.
All of these methods are small as most of them call methods in the controller.  There are a few flags however that are set so that these button clicks will not happen prematurely. 

Our drawing panel has the variables that contain all the images and sounds that are used in our game.  We have the OnPaint method which loops through all objects in the commonly shared 
world model.  This will draw each object in its respective drawer method.  We have some additional methods that draw the explosion sprite images at random at time of tank death, along with
a sound byte.  We have also added sound bytes for weapon firing. For example, since we only want the firing shot to be done once per click, we have a setter method that sets a flag and allows the 
sound byte to play.

--------------------------------------------------------------------------------------------------------------------------------------------------------------------
SERVER

This is the server portion of our Tank Wars game. This class handles the logic of the game and tracks and updates the locations of players, collisions of objects within the world, and sending these updates to all the connected players. These updates are sent out every frame and the frequency of these updates can be changed based on the settings file. 

SERVER SETTINGS 
The server settings are sent by the client and read by the server through an XML file. Listed below are the different settings that can be changed by the client and how they will affect the gameplay of Tank Wars.

MSPerFrame - This setting alters how often the server attempts to update the world for all of it's players. This number is in miliseconds and it's default is 17.

Universe Size - This settings alters how large the world is within the game. It specifically correlates to the number of units on each side of the square universe. If a tank reaches the edge of the world it wraps around to the other side. It's default is 2000

FramesPerShot - This setting alters how often a tank can fire a projectile. This number is in frames and it's default setting is 80.

RespawnRate - This settings alters the respawn time, in frames, of a tank once it has been killed. After the setting's value of frames the tank will respawn at a random location on the map. It's default setting is 300.

Walls - This setting communicates to the server where the walls should be placed. This is done using P1 containing an x and y value (P1(x,y)) and P2 containing an x and y value (P2(x,y)). The world can contain zero or more wall sections.

SERVER VALUES
The following are a series of values that the server tracks while the program is running. These values ensure that the game runs smoothly.

frameCounter - This value is created as a time using the amount of times that the world has been updated. This frameCounter is specifically used for the spawning of powerUps, once 1650 frames have passed within the world the server will spawn a powerUp. This frame number translates to a powerUp spawning every 27.5 seconds.

maxPowerUpNumber - This value is the number of powerUps that can be spawned within the world at the same time. It's value is 2 meaning that there can be a maximum of 2 powerUps within the world at any time. When a tank collects one of these powerUps the server will check the frameCounter for powerUps and see if it needs to spawn another that frame. 

EngineForce - This value is the distance a tank will travel every frame in the direction the player desires. This value (3) is in pixels and will move the tank 3 pixels in the desired direction of the player.

projectileForce - This value is the distance a standard projectile will travel every frame in the direction of the player's tank turret. This value (25) is in pixels and will move the projectile 25 pixels in the direction of the player's tank 

powerUpCount - This value is the number of powerUps currently spawned on the map. This value is used by the server in deciding whether it needs to spawn another powerUp within the world, based on the "maxPowerUpNumber" value. 

playerNumber - This value is used in tracking the number of clients that are connected to the server. This number is used in the construction of each player's tank object, allowing the server to track each player's tank individually.

