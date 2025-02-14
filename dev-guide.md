# Development Guide

I've created this document as a reference point for changes made as well as challenges and obstacles faced, in case this might ever become a more full fledged simulation. The reason I reduced this down to a single anchor is that my prseent computer is too underpowered for a larger version. Before I could even import all of the assets for one complete version of the mall, let alone add lighting or other features, the amount of time it took to save and load the levels became unwieldy. Even with the smaller version, I'm prone to seeing errors on screen about running out of GPU memory, as well as prolific screen tearing, but these aren't completely prohibitive.

In theory, if I can get one store in good shape, then the same concepts could be applied to building out the rest of the mall in the future, so I have captured those steps.

## Basic Steps Taken

This is a high level overview of the process, and then each step will be broken down in more detail in its own section.

1. Import section into the level
    - TBD: Correct collisions if necessary
    - TBD: Correct materials if necessary
2. TBD: Create lighting
3. TBD: Music, NPCs

## Import Scenes

At a high level, importing the graphics is fortunately fairly easy since they already exist. You can see in the `Content` directory that I sort of maintained the same folder structure as the original Unity project. Unfortunately, since it is such a different project, the git link is tenuous, so I would recommend manually re-downloading the FBX files in the case they ever update. I also deleted all the `*.meta` files and the `Materials` and `Textures` folder since they are specific to the Unity project. The `<asset_name>.fbm` folders can stay or go, I think Unreal will extract the same files anyway.

In Unreal, there is an option "File" > "Import Into Level" which I used for each FBX file. Then it asks for a location to extract the files to; I chose the same directory where the FBX was located. Finally, it asks for some options to choose when importing; I mostly used all the defaults except for the collisions on a case-by-case basis.

The nice thing here is that the positions of everything came out correct in my experience. The not-so-nice thing is all the issues I experienced with collisions and materials. There are also a lot of warnings that pop up, far too many to analyze, but I think most of them are trivial (i.e. renaming materials with underscores instead of spaces). It will create a bunch of `*.uasset` files for the meshes, materials, textures, etc.

## Collisions

During FBX scene import, on the static meshes tab there is an option for "Generate Missing Collision". 

![](./Screenshots/Guide-01.PNG)

Initially, I would always keep this checked, and then import all of the scenes. Now, I still recommend checking it the first time importing a scene, but then it's necessary to check the collisions on that scene before moving forward:

![](./Screenshots/Guide-02.PNG)

As we can see from these purplish lines, it generated some collisions outside of the range of the meshes. This is no good as it will create an "invisible wall" effect when we play the game.

In cases like these, I will undo my work, re-import and leave "Generate Missing Collision" unchecked. In tutorials I found, they would say to find the mesh in the Content Drawer and edit from there, but for some reason that didn't work for me. It would only show one mesh out of the multiple that make up each scene. Instead I would edit the scene, then click on the mesh in the viewport, and then double click the mesh:

![](./Screenshots/Guide-03.PNG)  
![](./Screenshots/Guide-04.PNG)

The prefered "quick and easy" option is the "Auto Convex Collision" but I'd find it produces the same kind of issues of filling in unwanted gaps:

![](./Screenshots/Guide-05.png)

Instead, I used "Box Simplified Collision" more often. This is what that looks like on a finished result:

![](./Screenshots/Guide-06.png)

This is not really a sustainable method to apply to the full mall, as it's rather tedious, so I still hope to find something better. When creating a box simplified collision, it will create it around the entire space. I had to shrink down the box, then make more and follow the same process of resizing, moving and rotating. One of the difficulties with this is that it's hard to find that happy medium between too narrow and too wide when resizing, even with snapping to grid turned off. Collisions that are too narrow are what create that "clipping" effect where, e.g., you can't walk through a wall, but if you get really close you'll be "inside" it and seeing what's behind it.

## Material Fixes

Another issue I experienced is materials not importing correctly. For example, it'll be the wrong color, or glass won't be transparent. Here's an example of that - the mall is overall extremely white, even down to the solid white glass. The first screenshot was taken after I had already fixed some of the glass; the second is after fixing the remainder of the glass, so you can clearly compare and tell how the glass initially looked like a white wall.

![](./Screenshots/Guide-07.PNG)  
![](./Screenshots/Guide-08.PNG)

The glass was a specific one-off fix, so I'm not sure if it's worth documenting yet. Ideally, I would find a way to correct these things on import, but I'm not sure if that'll be possible. Most likely what will end up happening is, like with the glass, I'll implement a fix that's specific to the Unreal material that's a best approximation of what was originally intended, on a case-by-case basis.

## Lighting Implementation

WIP - Stuff I've tried so far:

### Emissive Material

- Give the material a const 1 (1 + click on blueprint) to adjust intensity
- Give the material a constant vector 3 (3 + click on blueprint) to adjust color
- Multiply above 2 (M + click on blueprint) and connect to Emissive Color
- Unlit shading model maybe makes a difference?

#### Downsides
- Projects onto the floor in a weird circular pattern that isn't bright enough, even if intensity is cranked way up
- Only projects light when it's on camera. So the room gets dim spots as you move/look around

### Actor Lights
- Rectangular light can be shaped to fit into fixture. Maybe copyable, not tried yet

#### Downsides

- Placing in each fixture when there's about ~500 of them
- Likewise, editing all at once if needed, i.e. to make brighter, might not be possible
- The light just projects out of nowhere, doesn't make the fixture look like a light, so may need emissive anyway

### Settings Attempted

- Global Settings: Enabled virtual textures, virtual lightmaps. No idea what these did, just saw in a video. Also enabled static lighting in order to allow me to bake it, maybe other two do this as well
- Baked Lighting: First needed to add the GPU Lightmass plugin, then it can be accessed in the Build menu. Used all default settings except Realtime Viewport, noticed no difference in resulting lighting
- Post Process Volume
    - Enable Infinite Extent
    - Exposure > Metering Mode Manual and Exposure Compensation 10. This seems to act like a global brightness or gamma setting, not sure if needed
    - Global Illumination > Lumen (was already set to this)
    - Lumen Global Illumination > Lumen Scene Detail: Suggested in a video but I didn't notice any difference altering the value. There are some other light settings here
    - LGI > Max Trace Distance: Suggested in forum that turning this up would reduce the "look away" disappearing light issue. Instead I noticed that turning it _down_ seemed to make interiors appear brighter
    - Bloom can be adjusted. This is potentially useful with a bright emissive to make the fixture look cleaner


## Other Changes

TBD