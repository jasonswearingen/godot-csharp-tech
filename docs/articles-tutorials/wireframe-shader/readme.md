![Writing a wireframe shader in Godot](./img/teapot-editor.png "Blender for non-artists")




- [A "How to Use" guide to adding this wireframe shader to Godot.](#a-%22how-to-use%22-guide-to-adding-this-wireframe-shader-to-godot)
- [Blender for Non-Artists:  Using Blender to get gamedev tasks done.](#blender-for-non-artists-using-blender-to-get-gamedev-tasks-done)
  - [How this file is organized](#how-this-file-is-organized)
  - [The Problems](#the-problems)
- [How to UV Unwrap](#how-to-uv-unwrap)
  - [First steps](#first-steps)
  - [The wrong way, part 1:  (unwrap but not contiguous)](#the-wrong-way-part-1-unwrap-but-not-contiguous)
  - [The wrong way, part 2:  (unwrap continuous but not split by material)](#the-wrong-way-part-2-unwrap-continuous-but-not-split-by-material)
  - [The right way to unwrap (for this model at least)](#the-right-way-to-unwrap-for-this-model-at-least)
  - [pack uv map nicely](#pack-uv-map-nicely)
  - [For more tutorials on uv unwrapping:](#for-more-tutorials-on-uv-unwrapping)
- [Export the UV map (for example, if you want to paint in other software)](#export-the-uv-map-for-example-if-you-want-to-paint-in-other-software)
- [How to bake blender materials into a UV Texture](#how-to-bake-blender-materials-into-a-uv-texture)
- [How to texture paint directly from blender](#how-to-texture-paint-directly-from-blender)



# A "How to Use" guide to adding this wireframe shader to Godot.



# Blender for Non-Artists:  Using Blender to get gamedev tasks done.

When writing techdemos such as  ["Thousand Fishies"](https://www.patreon.com/godot_csharp_tech?tag=Thousand-Fishies-Project), I needed to use blender to perform some tasks.  I spent aprox 20 hours learning Blender basics to perform these tasks.  Here is a how-to guide on using Blender for non-artists.   

## How this file is organized

While you can skip to any section of this doc you find interesting, it can also be followed as a step-by-step tutorial.  If you want to do so, get the [```Fish1.blend``` file I'm using](https://drive.google.com/drive/folders/1SvlOveJJjmhSn-FgCRyojc1T5QHjjGkF).  This is by the Patreon creator [Quaternius](https://www.patreon.com/quaternius).


## The Problems
Some problems I dealt with in "Thousand Fishies" tech demo, with the ```Fish1.blend``` file.
1. **No UV Mapping**:  ```Fish1.blend``` doesn't contain uv mapping.  This means you can not apply a texture to it (or exported ```Fish1.obj```)
2. **Multiple Draw calls per fish**: This fish is going to be rendered 10k times with an animated shader.  If I used the default  exported ```Fish1.obj```, that would require 3 render calls per fish due to it's use of 3 materials.  Using a single material + diffuse texture allows 1 draw call per fish.
3. **colors washed out on export** I didn't like the color of the model after importing into Godot.  So Masked Texture painting is needed.


# How to UV Unwrap


## First steps
- Open ```fish1.blend``` (or your own file if not following this as a tutorial)
- Click ```Default``` view on top.  
  - ![](./img/hint-default-view.png)
- In bottom split screen, change editor type to “UV Editor”  
  - ![](./img/hint-uv-editor.png)
- On “scene collection” tree (upper right) select “fish”, causing entire fish to highlight
  - ![](./img/hint-scene-tree-fish.png)
- Change from object mode to Edit mode (bottom left, or ctrl-tab)
  - ![](./img/hint-edit-mode.png)


## The wrong way, part 1:  (unwrap but not contiguous)

- In upper editor window, choose ```UV --> Unwrap```
  - Shows unwrapped in bottom but the 3 meshes overlap
- In bottom window choose ```“pack islands”```
  - Now the layout is ok, but uv's are not contiguous
  - Ctrl - Z to undo.
  - ctrl-Z to undo the uv map gen

## The wrong way, part 2:  (unwrap continuous but not split by material)
- Select model (drag mouse over it)
  - **Gotcha**:  If you Rotate the view, you'll see that the back of the model isn’t selected.  
  - Fix by choosing ```“toggle xray”```  in lower right, then selecting again.
  - ![](./img/hint-toggle-xray.png)
- Choose ```UV-->Smart Unwrap```
  - Set island margin ```0.02```
- Looks ok, but unselect, and the uv map disapears
  - Fix by choose ```“UV Sync Selection”``` in upper left of UV Edit window
  - ![](./img/hint-uv-sync-selection.png)
- Looks ok, but select the “Bottom” material in right side tree
  - Now see that the material isn’t contiguous in the UV editor.   As we are going styleized, lets put these together.
  - Ctrl - Z to undo.
  - ctrl-Z to undo the uv map gen

## The right way to unwrap (for this model at least)
- Ctrl-z to undo changes (if any, made by previous "wrong way")
- Turn on ```Toggle Xray``` and ```UV Sync Selection``` as shown in **"wrong way, part 2"** (above)
- Generate uv’s for each material seperately:
  - Click each material in right side tree
    - ![](./img/hint-material-tree-select.png)
  - Choose “select” under material menu (under tree)
    - ![](./img/hint-material-select.png)
    - Be sure that other materials are unselected by choosing ```Deselect``` on them.
  - Choose ```UV-->Unwrap```
    - **Gotcha**: If you choose “Smart UV” with “stretch to uv bounds off” off, that will work, but it will cut seams through the material.  So instead I choose a normal unwrap.  Smart-uv may be best for you, depending on your needs.  
    - ![](./img/hint-uv-unwrap.png)
  - Do same for other materials, ignore that the uv coords overlap (we will fix that in following step)

## pack uv map nicely

- ```Select All ```
  -  ![](./img/hint-select-all.png)
- Choose ```UVEdit --> UV --> Average Island Scale```
  - ![](./img/hint-uv-average-island-scale.png)
- Choose ```UVEdit --> UV --> Pack Islands```
- uvs are a bit close, so at bottom under “Pack Islands” change margin to be ```0.1```
  - this gives some extra padding when painting and protexts against texture bleed
  - ![](./img/hint-pack-islands.png)

## For more tutorials on uv unwrapping:
- https://www.educba.com/uv-unwrapping-in-blender/
- https://blender.stackexchange.com/questions/6755/how-to-properly-unwrap-my-mesh


# Export the UV map (for example, if you want to paint in other software)
- Export .obj:  ```File --> Export```
- Export uv:   ```UVEdit --> UV --> Export UV Layout```
  - Choose “All UV’s”
  - choose Y up, Z forward
    - be sure that when importing into Godot, the model faces the ```-Z``` axis!!!

# How to bake blender materials into a UV Texture
- Goto “Render” tab under the righthand scene tree
- Select the fish then click “bake”
  -  ![](./img/hint-bake.png)
- You’ll see an error at bottom, “no active image found in material” 
  - Found answer here: https://blender.stackexchange.com/a/31073
- So create a new image to bake into:  ```UV Editor --> Image --> New```.   can leave it named as “Untitled”
- Change bottom editor from ```“UV Editor”```  to ```“Shader Editor”```
  -  ![](./img/hint-shader-editor.png)
- VERY IMPORTANT when choosing what material to edit:
  - Choose the material from the right-side materials tab, under the scene tree.   NOT from the top of the Shader editor.
  -  ![](./img/hint-material-tree-select.png)
- For each material... 
  - Click ```“use nodes”```
  - ```Add --> Texture --> Image Texture```
    -  ![](./img/hint-add-texture.png)
  - Set that image texture node’s “linked texture” to the (“Untitled”) one you created in the UV Editor
    - ![](./img/hint-select-image.png)
  - Do that for all materials.   Maybe it seems like this shouldn’t do anything, but it lets the shader output to texture.
- Choose ```“UV Editor”``` for the bottom editor again.
- Choose Render Tab on right side, choose Bake, and bake.  (might have to switch the upper editor back to select the fish to do this)
- You’ll see the texture now has the baked texture.  I want a cell shaded look, so I changed the bake settings:
  - ```Bake Type:  Diffuse```
  - Unselect ```“Direct”``` and ```“Indirect”```  (only color selected)
    - ![](./img/hint-bake-options.png)


# How to texture paint directly from blender

- Be on “Texture Paint” view (top), Switch to “Edit” mode (ctrl-tab)
- Select “Bottom” material, then “select” it (make sure nothing else selected)
- Ctrl-tab to switch to “texture paint” mode
- In bottom left, select “Paint Mask”
  - This is so your paint is only on the selected verticies
    -  ![](./img/hint-paint-mask.png)
- Right click to change color you want to paint with
- paint / fill the colors you want.

I choose more saturted colors for the fish:

![](./img/20200204-blender-fish-screenshot.png)

This concludes the "Blender for non-artists" tutorial!  If it's useful, consider supporting me on [Patreon](https://www.patreon.com/godot_csharp_tech).




