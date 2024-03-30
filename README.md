# Neurotechnologies Lab Project Template

Template for the Neurotechnologies lab. It aims to give the student the resources necessary for developing a BCI application in Unity.

# Table of Contents
  * [Prerequisites](#prerequisites)
  * [Installation](#installation)
  * [Usage](#usage)

## Prerequisites

- A CLI or GUI with support for Git. For example, i use [git-bash](https://gitforwindows.org/) as a CLI, and you can use [Github Desktop](https://desktop.github.com/) for GUI
- [Unity Editor](https://unity.com/download) version 2022.3.17f1 or later
- [Unicorn Suite Hybrid Black](https://euvtro.sharepoint.com/:u:/s/Introducereinneurotehnologii/EWzoUaj5M0VFlK1hu9HFJA8BVQedBUdeT-7SEqQqrkMjGQ?e=EGtcgs)

## Installation

To get this Unity project template up and running, you need to follow these steps:

1. Clone the repository in a folder of your liking ([This](https://youtu.be/bQrtezWlphU?si=N-UqagFMgqONtPYw&t=42) is how to clone a repository wtih CLI, and [This](https://youtu.be/PoZNIbs_wx8?si=zmT12G9UFDycPjFg) is for GUI)
2. Open Unity Hub and select the Open button to select a project (The button may not say exactly "Open", depending on your Unity Hub version, but it should be similar)

![Unity Hub](https://github.com/Relu12345/BCI-Lab-Project/assets/94746838/ad9dc154-1d5d-42d8-98b9-2eb3d2f00f63)


3. Select the project folder that you just cloned
 
![Unity Hub Select Folder](https://github.com/Relu12345/BCI-Lab-Project/assets/94746838/db8bc609-6c7c-4930-bf76-2698294826c8)

4. Now your project should be openning, give it a minute or two.

- ### If you do not have exactly version 2022.3.17f1 of Unity, you will get a warning saying your version does not match. Just make sure you have a version newer than 2022.3.17f1 and you can proceed with opening the project while ignoring the warning

![Warning Unity Hub Project](https://github.com/Relu12345/BCI-Lab-Project/assets/94746838/86505df6-cdc7-406a-a5bd-d760e66146d5)

You are good to go! We will now take a look at what is available for us to use when developing our BCI Game.

## Usage

1. After your project has opened, navigate inside the Prefabs folder.

![Unity Interface](https://github.com/Relu12345/BCI-Lab-Project/assets/94746838/e81ee394-f791-446e-804f-3c015f1f98af)

2. Here, you are presented with the main components of the Unicorn ERP Unity Interface (The UI elements are optional, but recommended for debugging!):

![Unity Prefabs Folder](https://github.com/Relu12345/BCI-Lab-Project/assets/94746838/1cd9de28-f67c-4f24-832c-f7a1be91ab99)

- **bciManager2D/3D**: These are the main components of the ERP Interface, you can use whichever fits your game idea, 2D or 3D. Each manager contains a respective 2D or 3D Flash Controller Script, which will handle the object's flashing.

### UI Elements
  
- **battery**: A UI element that shows the current battery level of the headset.
- **classifierAccuracy**: This is a UI element that displays the accuracy result of your last training. Make sure this atleast in the yellow if you want a good experience!
- **dataLost**: Another UI element that shows up whenever there are packets lost from the headset.
- **sqBar and sqBrain**: Two UI elements that are used for the same thing: Displaying what electrodes are working correctly. You only need to use one of them, as they do essentially the same thing.

3. Drag and drop a bciManager in your hierarchy / in your scene, coresponding to your game's perspective (2D or 3D). I will use 3D.

![BCI Manager](https://github.com/Relu12345/BCI-Lab-Project/assets/94746838/44bdb9c9-c1db-4159-b099-66d2936c58ff)

4. Let's take a look at the Unity Inspector for a moment:

![BCI Manager Inspector](https://github.com/Relu12345/BCI-Lab-Project/assets/94746838/a941d204-e288-449c-812f-2e6b4ea10708)

The BCI Manager has 2 scripts attached to it:

- ERP Flash Controller 2D/3D
- BCI Manager 2D/3D

We are especially interested about the Flash Controller. This component enables the developer to create an object, which the user will use to train themselves in order to use the application. After the training, the user can then play the game as intended. Without the training section, the user will almost certainly be unable score an accuracy high enough to be able to have a pleasant experience with your game =)

Before we continue, let's take a deeper look at every component of the Flash Controller:

### BCI Settings

- **Number Of Training Trials**: How many times should the training object flash. The higher the number, the more it will take to train, but also it the more chance of having a high accuracy. It takes values between 30-150, with a default of 60
- **Flashtime Ms**: The frequency at which your BCI objects (including the training object) will flash at. It can take values between 50-300, with a default of 100. Even though a lower frequency might make the game more engaging and playable, making the objects stand out more and generaly be faster to select, this comes at the cost of eye tiredness and possibly even migraines. You have been warned!
- **Number Of Classes**: Each BCI object will have a Class Id, which is required so that the API can flash each object at a different frequency from other object, thus making them individually selectable. It takes values between 4-15, with a default of 6. Even though you can create more objects than the limit, they will not work at the same time, they will work only if you destroy / disable other BCI objects, to make room for the new ones.

### Flash Objects

- **Training Object**: This represents the object you will use to train the user to be able to select the other objects in the game. The training object has to be the same as (or atleast extremely similar to the) objects in the game, if you want the game to be any playable.
  - **Class Id**: This is the classification id of the object, used so that you can programmatically select objects. This can be left as default "1", as it does not interfere with the objects and is counted separately.
  - **Game Object**: The object used to represent the BCI object, usually in a visual way, but this being a normal Unity Game Object, it can hold other scripts too or whatever your creative mind comes up with =)
  - **Flash and Dark Material / Sprite**: These are used to represent the training and BCI object's visual states, with the dark material / sprite (depending if you are making a 3D, respectively 2D game) representing the base material / texture of your object, and the flash material / sprite representing the object's visual appearance when flashing.

### Application Objects
They are the objects used in the game itself, with a setup exactly the same as the training object, but with the Class Id being separate from it, meaning if you used "1" for the training object, you can set your first BCI object as "1" too. Also, to keep in mind, there have to be at least 2 objects present in the game, unless you want an error:

![Objects Error](https://github.com/Relu12345/BCI-Lab-Project/assets/94746838/8e588205-e088-43f8-aea4-6c17f0a96dde)

You are done with the setup instructions! You can go wild now =)
