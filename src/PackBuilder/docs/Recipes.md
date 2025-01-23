# Recipe Modification
tPackBuilder adds support for dynamically modifying recipes to add, remove, or change ingredients, results, or tiles for any recipe from any mod (or vanilla!) in the game.

All you need to do is add a `.recipemod.json` file to your mod's folder, add the required data, and tPackBuilder handles the rest!

## Pre-requisites

While this is not technically required, it is recommended to have a competent text editor to assist you in building your files.

The recommended editor is [Notepad++](https://notepad-plus-plus.org/), as it is lightweight, fast, and relatively simple, while still providing enough useful tools to make your syntax nice and readable.

Alternatively, the Visual Studio text editor works fine as well. However, be warned that Visual Studio may try to give you warnings about your file's formatting, even if the file is actually formatted correctly. As long as you're following the documentation specified below, you'll be fine.

If you really wanted to, you could also build all of your recipes from the default Windows Notepad application. This is not recommended, as Notepad does not contain useful features like auto-filling end brackets or quotes, but if you really prefer it, it will work fine.

***
## Building and configuring your modifications

tPackBuilder will search your mod's files for any `.recipemod.json` files. Each of these files acts as a sort of "rule", which defines which recipes you want to change, and how you want to change them.

You can add multiple `.recipemod.json` files to your mod's folder and each one will be applied individually.

***
### Walkthrough

> This section is a step-by-step guide to setting up your first recipe modification, and a breakdown of how modifications are formatted. If you want to skip straight to see what options are available, jump to the documentation at the bottom of this file.

To get started, lets create a modification that adds 5 gel as a requirement to craft the Zenith.

First, add a file to your mod's folder and rename it to `zenith.recipemod.json`. The naming for this file doesn't matter to tPackBuilder, only that it ends with `.recipemod.json` - but we're going to call it `zenith` to make it clear what this modification is for.
 
![image](https://github.com/user-attachments/assets/c45711bf-2d1f-46d8-b892-b5bee150bd8c)

Then, open up this file in your preferred text editor. This guide will be using Notepad++, as is recommended in the top section. This file should be empty by default.

Start by adding a set of curly braces to your file. All of your modification's data will be filled into the braces. This is the default structure for .json objects.

![image](https://github.com/user-attachments/assets/a3fe704c-3f52-4efc-b2f7-c74fcc24e7f9)

Now we're going to begin actually filling in our modification's data. Recipe mods are broken down into 3 parts:
- Conditions
- Changes
- Criteria

Lets start with conditions. "Conditions" are a set of... well, conditions, that must be met in order for your modification to apply to a recipe. These conditions will be tested against every recipe in the game, and any recipes that meet the specified conditions will have the changes applied to them.

Secondly, "Changes" are... you guessed it, the changes you want to apply. You can supply the modification with multiple changes, and they will be applied in order based on the order you list them in the json file.

Lastly, there's criteria. This one is the most simple. If you have supplied multiple conditions, criteria defines how many of those conditions need to be met in order to apply. Criteria can *only* be either `"All"` or `"Any"`. `"All"` means your modification will only apply if ALL of your listed conditions are met. `"Any"` means your modification will apply if even one of your listed conditions is met. Pretty simple.

You can begin setting up those 3 parts like so:

![image](https://github.com/user-attachments/assets/bceb11ef-780f-427f-8ba5-f8299ca70a60)

Lets fill in our conditions first. We want our modification to apply to every recipe that results in crafting the Zenith. So lets add a condition that specifies the recipe must result in creating the Zenith item.

![image](https://github.com/user-attachments/assets/df79c462-91a5-4890-bccc-672c4b7ff9a1)

The first thing you may notice is that we've specified the recipe must result in `"Terraria/Zenith"`. Whenever you are specifying in-game content in your json files, you always list it as `"ModName/ContentName"`. In this case, the Zenith is actually from vanilla, so our "mod name" is going to just be `Terraria`. But if we wanted to instead use, say, the Fabstaff from Calamity, we would use `"CalamityMod/Fabstaff"'`.

There are numerous available conditions you can use when creating recipe modifications. For a full list of conditions and what kind of data they need, check the bottom of this page. If you want to add multiple conditions, you can do that like so:

![image](https://github.com/user-attachments/assets/509e4e66-61e6-4886-8066-61f6e6143ded)

> Note that each supplied condition is separated by a comma and uses its own additional set of curly braces.
