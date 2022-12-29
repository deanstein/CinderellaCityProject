![Main Preview](README_intro.png)

# The Cinderella City Project
The Cinderella City Project is a digital reconstruction of a historic shopping center for a virtual reality history experience. 

The real Cinderella City Mall existed in Englewood, Colorado from 1968 to 1998.

## Project Updates

- [Watch the latest presentation on Cinderella City's history and enjoy a walkthrough of the newest simulation version](https://youtu.be/va9xoslXjQo) (2022-04-26)

## Quick Links

- [Sign the petition to save the last remaining Cinderella City building from demolition](https://www.change.org/p/save-the-englewood-civic-center-from-demolition-by-adapting-it-into-the-new-heart-of-the-neighborhood)
- [Read about the project and donate on Ko-Fi](https://www.ko-fi.com/cinderellacityproject)
- [See historic photos and the latest simulation progress on Instagram](https://instagram.com/cinderellacityproject) or [Facebook](https://www.facebook.com/cinderellacityproject)
- [See videos on YouTube](https://www.youtube.com/c/TheCinderellaCityProject)
- [Try the Cinderella City Simulation (work in progress)](https://github.com/deanstein/CinderellaCityProject#try-the-cinderella-city-simulation-work-in-progress)

## Press Coverage
**Recent** 

- [Once Upon a Time at Cinderella City: Project Update](https://www.westword.com/news/cinderella-city-famous-colorado-mall-virtual-online-project-update-13955248) (*Westword*, 2022-04-26)

- [Fight to Save the Final Remains of Cinderella City](https://www.westword.com/news/englewood-citycenter-development-and-fight-to-save-cinderella-citys-final-remains-11853344) (*Westword*, 2021-02-19)

- [One Denverite is Creating a Virtual tour of the Iconic Cinderella City Mall](https://www.5280.com/2020/04/one-denverite-is-creating-a-virtual-tour-of-the-iconic-cinderella-city-mall/) (*5280*, 2020-04-23)

- [Cinderella City Virtual Video Game Project Update](https://www.westword.com/news/cinderella-city-virtual-video-game-project-update-2020-11516741) (*Westword*, 2020-01-28)

**Archive**

- [A Giant Fairytale Mall in Colorado Is Being Brought Back to Virtual Life](http://veryfamousmagazine.com/fairytale-mall-colorado-virtual-life/) (*Very Famous Magazine*, 2018-11-01)

- [Cinderella City Vintage Photos Helping to Turn famed Mall Into video Game](https://www.westword.com/news/cinderella-city-vintage-photos-helping-to-turn-famed-mall-into-video-game-10303551) (*Westword*, 2018-06-28)

- [Bringing Cinderella City Back to virtual Life for its 50th Anniversary](https://www.westword.com/news/bringing-cinderella-city-back-to-virtual-life-for-its-50th-anniversary-10069280) (*Westword*, 2018-03-16)

- [Cinderella City Project Video Preview new Effects, Music by Dead Mall](https://www.westword.com/news/cinderella-city-project-video-preview-new-effects-music-by-dead-mall-10983679) (*Westword*, 2018-01-16)
 

# Cinderella City Simulation

The Cinderella City Simulation is at the heart of the Cinderella City Project. 

Modeled in [Autodesk FormIt](https://formit.autodesk.com/), and brought to life in [Unity](https://unity.com/), the simulation illustrates distinct time periods of Cinderella City's three-decade life, including the colorful 1960s-1970s, and the staid 1980s-1990s. Eventually, the simulation will also offer an "Alternate Future" interactive exhibit showing how the shopping center could have been adaptively reused in 1998 in lieu of demolition.

While experiencing the mall in each era, the player can "time travel" to other time periods while maintaining the current position in space to see how the world of retail and design changed over 30 years.

The simulation includes an accurate representation of the architectural character and signage of the shopping mall and other immersive elements like period-correct music, historical photographs, animated shoppers, and eventually, recorded verbal memories.

When released, the Cinderella City Simulation will support a variety of platforms including virtual reality headsets. The simulation may also be made into a permanent or semi-permanent Englewood, Colorado exhibit in the future.


# Try the Cinderella City Simulation (work in progress)

The Cinderella City Simulation is under development, but in-progress builds are available for intrepid mall fans to try out.

The simulation is currently limited to machines running Microsoft Windows, but other platforms will be supported in the future.

The latest ready-to-play builds from the development branch can be found at the top of the **[Releases page](https://github.com/deanstein/CinderellaCityProject/releases)**.

- Click "Assets" below the release notes at the link above
- Download the .7z file ending in "_Win" 
- [Install 7-Zip first](https://www.7-zip.org/a/7z2201-x64.exe) if you don't have it
- Extract the .7z file
	- Right-click the .7z file, 7-Zip -> Extract To "CinderellaCityProject..."
- Run "Cinderella City Simulation.exe"
- Optionally choose your desired resolution
	- Recommended starting resolution: 1920x1080 or similar
	- Higher resolutions may result in slower performance

This is a work-in-progress, so these builds may exhibit bugs and unexpected behavior. 

### Simulation Controls

After the simulation loads, you'll find yourself at the Main Menu, where you can pick a time and place to start. Once you're in the simulation, here's how to navigate:

- Keyboard (Windows)
	- WASD to walk forward, left, right, and backward respectively
		- Hold Shift to run
	- Mouse to look around
	- Space to jump
	- Q and E to time-travel
	- P to toggle the people on/off
	- O to reset the people to their original positions
	- G to enter/exit anti-gravity mode. While in anti-gravity mode:
		- Hold R to rise/ascend
		- Hold F to fall/descend
		- Hit G again to toggle gravity on
	- X to take a screenshot (stored at users/AppData/The Cinderella City Project/Cinderella City Simulation/Screenshots)
	- 1, 2, 3, 4 to cycle between special post-processing camera modes. Tap the number again to return to the default.
	- ESC to go to the Pause Menu
		- You can also time-travel from here, with a visual showing what the other eras look like at this point in space
	- M to go to the Main Menu
	- V to go to the Visualization Menu
	- U to go to the Audio Menu


# Known Issues

The simulation is still under development, so some issues are known and will be fixed in future updates:

- Large areas of both time periods are unfinished.


# Unity Project Structure + Development Guide

The Cinderella City Simulation is a Unity project, requiring a specific folder structure to enable automation of some elements, or to minimize effort when manual (one-time setup) steps are required.

In broad strokes, the Cinderella City Simulation requires the following:
- The Unity project file
- 1st-party assets
	- FBX files comprising different versions of the mall, in strategic pieces
	- UI sprites and other graphics
	- Scripts and behaviors
- 3rd-party assets (not included in the GitHub repo)
	- Music
	- People
	- Vegetation
	- Packages and code

## FormIt Model + FBX Assets

The Cinderella City Mall model is built in Autodesk FormIt, and exported in pieces as FBX files.

Eventually, this project will feature three versions of Cinderella City Mall, built in FormIt and experienced in Unity:
- 1960s/1970s
- 1980s/1990s
- Alternate Future

Each version requires FBX files exported to the Assets folder in Unity, with a subfolder indicating the time period or era: **Assets/FBX/60s70s/**

Each FBX file needs to be stored in a folder with a matching name, which allows the AssetImportPipeline to manage textures and materials separately for each file. A few examples:
- Assets/FBX/60s70s/mall-doors-windows-interior/mall-doors-windows-interior.fbx
- Assets/FBX/60s70s/mall-floor-ceiling-vertical/mall-floor-ceiling-vertical.fbx
- Assets/FBX/6070s/proxy-people/proxy-people.fbx

## AssetImportPipeline

To automate the import process of importing various file types, and to clean up stale data on import, the AssetImportPipeline code looks for 3D models and other files that are imported into the Unity project, and automatically executes crucial steps in the current scene. 

**AssetImportPipeline assumes that the current scene open in the Editor is the scene intended as a destination for files updated in the Assets folder.**

- Any files intended for processing by the pipeline need to be allowlisted, so only the ones the project is set to configure will be sent through the AssetImportPipeline
- Allowlisted FBX files will be automatically placed in the game scene, if they aren't there already, using a global scale defined by us, and global positioning as defined in the FBX file
- Allowlisted FBX files will extract all textures and materials to subfolders inside the current folder, and will delete existing textures and materials inside the current scene
- Whitelisted FBX files with "proxy" in their name will automatically get their proxy objects from FormIt replaced with real objects from Unity (for example, trees, people, and cameras)
- Allowlisted FBX files with "speaker" in their name will automatically get audio emitters, doppler effects, and custom behavior script components to simulate the effect of mall speakers (also used for global sounds like background noise and chatter)
- Allowlisted audio files get imported with certain settings, so they sound like they are coming from mall speakers
- All images in the "UI" folder get imported as sprites

## Scene Hierarchy
Each scene needs to have one "Container" object that contains all objects in the Scene. This is crucial to be able to toggle all Scene objects on/off.

Scene structure example:
- **60s70s** (Scene)
	- **60s70sContainer** (GameObject) // used for toggling scenes on or off by disabling all children
		- Environment
			- SkyDome (3rd-party GameObject)
			- Test Camera (used to validate the SkyDome settings with a camera, since the Editor view is inaccurate)
		- FPSController (GameObject)
		- UILauncher (GameObject)
		- Geometry group 1 (GameObject)
		- Geometry group 2 (GameObject)
		- Geometry group ... (GameObject)

## Scene Configurations
All Scenes require a bit of manual setup to enable certain behaviors and communication, in addition to the automatic import that AssetImportPipeline provides.

The following Scenes are required, and need to be organized as follows:

### Asynchronous Scene Loading (Includes scenes: LoadingScreen)
The LoadingScreen is responsible for asynchronously loading all required Scenes in the game, including the 3D geometric and 2D UI scenes, so that switching between Scenes is seamless.

 - **LoadingScreen** (Scene)
 	- **LoadingScreenContainer** (GameObject)
		- **LoadingScreenLauncher** (GameObject)
			- Holds scripts for generating UI (as children of the launcher), and for toggling between scenes
			- Requires Scripts:
				- *CreateScreenSpaceUILayoutByName* (Script Component)
					- Responsible for identifying which UI components to build based on the Scene name
				- *LoadAllScenesAsync* (Script Component)
					- Responsible for asynchronously loading all specified scenes
	- **Occlusion Area** (GameObject)
		- Used for occlusion culling in all first-person scenes

### UI + Menu Scenes (Includes scenes: MainMenu, PauseMenu)
In scenes that exclusively generate and display UI elements, we need to add custom script components to some GameObjects to control behaviors related to UI:

 - **MainMenu** (Scene)
 	- **MainMenuContainer** (GameObject)
	 	- **Sun** (GameObject) (PauseMenu only)
		 	- Used for matching the Sun settings of other scenes, for the purposes of accurate inactive scene screenshots
			 - Requires Scripts:
			 	- *ManageSunSettings* (ScriptComponent)
				 	- Responsible for collecting Sun settings for FPSController scenes, and applying them to PauseMenu for accurate inactive screenshots
		- **MainMenuLauncher** (GameObject)
			- Holds scripts for generating UI (as children of the launcher), and for toggling between scenes
			- Requires Scripts:
				- *CreateScreenSpaceUILayoutByName* (Script Component)
					- Responsible for identifying which UI components to build based on the Scene name
				- *ToggleSceneAndUIByInputEvent* (Script Component)
					- Responsible for responding to input events and displaying scenes and UI

### First-Person Scenes (Includes scenes: 60s70s, 80s90s, AltFuture)
In scenes with an FPSController and FirstPersonCharacter (60s70s, 80s90s, AltFuture), we need to add custom script components to some GameObjects to control behaviors related to UI and the FPSController. Note that the FPSController needs to be renamed with a prefix of the era it's in.

 - **60s70s** (Scene)
 	- **60s70sContainer** (GameObject)
		- **Environment** (GameObject)
			- **Sky Dome** (TimeOfDay object)
				- 3rd-party sky dome, responsible for the sun, clouds, stars, and moon
				- Note that a light has been added to the Sun object in the Sky Dome, which is set to the parameters of the old sun
				- Requires settings:
					- Ambient color gradient under "Day" section should be set to RGB 130, 130, 130
				- Requires Scripts:
					- *ManageSunSettings* (Unity Component)
		- **60s70sFPSController** (GameObject)
			- Responsible for the player's movement in space, derived from the Unity standard asset, but modified
			- Requires Specific Name: '(EraName)FPSController'
			- Requires Tags: Player
			- Requires Scripts:
				- *CharacterController* (Unity Component)
				- *ManageFPSControllers* (Script Component)
					- Tracks and controls FPSControllers across Scenes
			- **FirstPersonCharacter** (GameObject)
				- *AudioListener* (Unity Component)
				- *Post Process Layer + Post Process Volume* (Unity Component)
					- Overlay the camera with screen-based color and brightness effects
				- *Camera* (Camera Component)
					- The player's eye height, view angle, and head orientation
				- Requires Scripts:
					- *ToggleCameraEffectsByInputEvent.cs* (Script Component)
						- Responsible for watching for keyboard events and toggling scene effects
			- **FirstPersonAgent** (GameOBject)
				- *Agent* (Nav Mesh Agent)
					- Allows the NPCs in the game to avoid colliding with the player, and enables the player to follow a path for "guided tours"
				- Requires a Navigation Mesh to be present in the scene
				- Requires Scripts:
					- *FollowPlayer.cs* (Script Component)
						- Responsible for making the FirstPersonAgent follow the player
					- *UpdateFPSAgentByState.cs* (Script Component)
						- Responsible for toggling the FPSAgent on/off to facilitate relocation when required
		- **UILauncher** (GameObject)
			- Holds scripts for generating UI (as children of the launcher), and for toggling between scenes
				- Requires Scripts:
					- *CreateScreenSpaceUILayoutByName* (Script Component)
						- Responsible for creating the Heads Up Display layout when in-game
					- **ToggleSceneAndUIByInputEvent* (Script Component)
						- Responsible for watching for keyboard events and toggling between Scenes (including menus)
		- **CubeMapRenderPosition** (GameObject)
			- Represents a position in space from which to execute CubeMap updates, for use in glassy reflections

**Navigation Mesh**

First-person scenes with NPCs require the setup and baking of a Navigation Mesh for each scene, to allow the NPCs to find destinations and follow paths.

Each scene requires its own navigation mesh to be baked, and these meshes should be re-baked when scene geometry changes in a way that would affect navigation abilities.

- Run the "Update Nav Meshes for All Scenes" item in the CCP menu
- Or run the "Post Process Scene Update" from the CCP > Batch Operations menu

**Occlusion Culling**

First-person scenes require the setup and baking of occlusion culling data to maintain high performance while navigating around the large, detailed mall.

Because the Cinderella City Project uses multiple scenes opened additively, the occlusion culling data must be baked with all scenes open. To bake occlusion properly for all scenes:

- Run the "Update Occlusion Culling for All Scenes" item in the CCP menu
- Or run the "Post Process Scene Update" from the CCP > Batch Operations menu




